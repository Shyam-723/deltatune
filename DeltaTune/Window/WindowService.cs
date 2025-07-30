using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DeltaTune.Settings;
using Microsoft.Xna.Framework;
using R3;
using SharpDX;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace DeltaTune.Window
{
    public class WindowService : IWindowService, IDisposable
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        private static extern int SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref int[] pMarInset);
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const int GWL_EXSTYLE = -20;
        private const int LWA_ALPHA = 0x00000002;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;
        private static readonly IntPtr HWND_TOPMOST = (IntPtr)(-1);
        private static readonly IntPtr HWND_NOTOPMOST = (IntPtr)(-2);
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        
        private readonly Game game;
        private readonly GameWindow window;
        private readonly GraphicsDeviceManager graphicsDeviceManager;
        private readonly ISettingsMenu settingsMenu;
        private readonly ISettingsService settingsService;
        private readonly float lineHeight;

        private Screen currentScreen;
        private NotifyIcon trayIcon;
        private IDisposable subscriptions;

        public WindowService(Game game, GraphicsDeviceManager graphicsDeviceManager, ISettingsMenu settingsMenu, ISettingsService settingsService, float lineHeight)
        {
            this.game = game;
            this.window = game.Window;
            this.graphicsDeviceManager = graphicsDeviceManager;
            this.settingsMenu = settingsMenu;
            this.settingsService = settingsService;
            this.lineHeight = lineHeight;
        }

        public void InitializeWindow()
        {
            IntPtr windowHandle = window.Handle;
            
            game.IsMouseVisible = true;
            game.IsFixedTimeStep = true;
            game.InactiveSleepTime = TimeSpan.Zero;
            graphicsDeviceManager.SynchronizeWithVerticalRetrace = true;

            window.IsBorderless = true;
            window.AllowUserResizing = false;
            
            SetWindowLong(windowHandle, GWL_EXSTYLE, WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOPMOST | WS_EX_TOOLWINDOW);
            SetLayeredWindowAttributes(windowHandle, 0, 255, LWA_ALPHA);
            
            int[] margins = { -1 };
            DwmExtendFrameIntoClientArea(window.Handle, ref margins);

            currentScreen = GetScreenByName(settingsService.ScreenName.Value);
            if (currentScreen == null)
            {
                currentScreen = Screen.PrimaryScreen;
                settingsService.ScreenName.Value = currentScreen.DeviceName;
            }
            
            MakeTopmostWindow();
            UpdateWindowPosition();
            UpdateWindowScale(settingsService.ScaleFactor.Value);
            UpdateGameRefreshRate();
            
            CreateTrayIcon(windowHandle);
            
            var disposableBuilder = Disposable.CreateBuilder();
            
            settingsService.ScaleFactor.Subscribe(scale =>
            {
                UpdateWindowScale(scale);
                UpdateWindowPosition();
            }).AddTo(ref disposableBuilder);
            
            settingsService.Position.Subscribe(_ => UpdateWindowPosition()).AddTo(ref disposableBuilder);
            
            settingsService.ScreenName.Subscribe(screenName =>
            {
                var screen = GetScreenByName(screenName);
                if (screen != null)
                {
                    currentScreen = screen;
                    
                    UpdateWindowPosition();
                    UpdateWindowScale(settingsService.ScaleFactor.Value);
                    UpdateGameRefreshRate();
                }
            }).AddTo(ref disposableBuilder);
            
            settingsService.ScreenCaptureCompatibilityMode.Subscribe(SetScreenCaptureCompatibilityMode).AddTo(ref disposableBuilder);

            Observable.Interval(TimeSpan.FromSeconds(0.05)).Subscribe(_ =>
            {
                if (!trayIcon.ContextMenuStrip.Visible && !settingsService.ScreenCaptureCompatibilityMode.Value)
                {
                    MakeTopmostWindow();
                }
            }).AddTo(ref disposableBuilder);
            
            subscriptions = disposableBuilder.Build();

            if (settingsService.IsFactorySettings)
            {
                settingsService.ScaleFactor.Value = GetRecommendedScale();
            }
        }

        private Screen GetScreenByName(string deviceName)
        {
            Screen[] screens = Screen.AllScreens;
            Screen match = null;

            foreach (Screen screen in screens)
            {
                if(screen.DeviceName == deviceName) match = screen;
            }
            
            return match;
        }

        private void MakeTopmostWindow()
        {
            SetWindowPos(window.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        private void UpdateWindowPosition()
        {
            Vector2 fractionalPosition = settingsService.Position.Value;
            Rectangle currentScreenBounds = GetCurrentScreenBounds();
            Point windowPosition = new Point(
                currentScreenBounds.X,
                currentScreenBounds.Y + (int)(currentScreenBounds.Height * fractionalPosition.Y)
            );

            if (fractionalPosition.Y > 0.5f)
            {
                windowPosition.Y -= (int)lineHeight * settingsService.ScaleFactor.Value;
            }
            
            window.Position = windowPosition;
        }

        private void UpdateGameRefreshRate()
        {
            game.TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / ScreenRefreshRateProvider.GetScreenRefreshRate(window.Handle, 60));
        }

        private void UpdateWindowScale(int scaleFactor)
        {
            Rectangle currentScreenBounds = GetCurrentScreenBounds();
            SetWindowSize(new Point(currentScreenBounds.Width, (int)lineHeight * scaleFactor));
        }
        
        private Rectangle GetCurrentScreenBounds()
        {
            System.Drawing.Rectangle boundsSystemType = currentScreen.WorkingArea;
            return new Rectangle(boundsSystemType.X, boundsSystemType.Y, boundsSystemType.Width, boundsSystemType.Height);
        }
        
        private int GetRecommendedScale()
        {
            System.Drawing.Rectangle bounds = currentScreen.WorkingArea;
            return MathUtil.Clamp((int)Math.Ceiling(bounds.Width / 960f) + 1, 1, 8);
        }

        private void SetWindowSize(Point size)
        {
            graphicsDeviceManager.PreferredBackBufferWidth = size.X;
            graphicsDeviceManager.PreferredBackBufferHeight = size.Y;
            graphicsDeviceManager.ApplyChanges();
        }

        private void CreateTrayIcon(IntPtr windowHandle)
        {
            Form form = (Form)Control.FromHandle(windowHandle);
            Icon icon = form.Icon;
            trayIcon = new NotifyIcon();
            trayIcon.Icon = icon;
            trayIcon.Text = "DeltaTune";
            trayIcon.Visible = true;
            trayIcon.ContextMenuStrip = settingsMenu.GetSettingsMenu();

            trayIcon.BalloonTipTitle = "DeltaTune is now running!";
            trayIcon.BalloonTipText = "Play some music to get started or right-click the DeltaTune icon in your system tray for customization options.";
            trayIcon.ShowBalloonTip(1000);
        }

        private void SetScreenCaptureCompatibilityMode(bool enabled)
        {
            if (enabled)
            {
                SetWindowLong(window.Handle, GWL_EXSTYLE, WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_APPWINDOW);
                SetWindowPos(window.Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }
            else
            {
                SetWindowLong(window.Handle, GWL_EXSTYLE, WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOPMOST | WS_EX_TOOLWINDOW);
                SetWindowPos(window.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                UpdateWindowPosition();
            }
        }

        public void Dispose()
        {
            subscriptions?.Dispose();
        }
    }
}