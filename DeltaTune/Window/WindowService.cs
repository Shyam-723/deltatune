using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DeltaTune.Settings;
using Microsoft.Xna.Framework;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace DeltaTune.Window
{
    public class WindowService : IWindowService
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
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        
        private readonly Game game;
        private readonly GameWindow window;
        private readonly GraphicsDeviceManager graphicsDeviceManager;
        private readonly ISettingsMenu settingsMenu;

        public WindowService(Game game, GraphicsDeviceManager graphicsDeviceManager, ISettingsMenu settingsMenu)
        {
            this.game = game;
            this.window = game.Window;
            this.graphicsDeviceManager = graphicsDeviceManager;
            this.settingsMenu = settingsMenu;
        }

        public void InitializeWindow()
        {
            IntPtr windowHandle = window.Handle;
            
            game.IsMouseVisible = true;
            game.IsFixedTimeStep = true;
            game.TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / ScreenRefreshRateProvider.GetScreenRefreshRate(windowHandle, 60));
            game.InactiveSleepTime = TimeSpan.Zero;
            graphicsDeviceManager.SynchronizeWithVerticalRetrace = true;

            window.IsBorderless = true;
            window.AllowUserResizing = false;
            
            SetWindowLong(windowHandle, GWL_EXSTYLE, WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOPMOST | WS_EX_TOOLWINDOW);
            SetLayeredWindowAttributes(windowHandle, 0, 255, LWA_ALPHA);
            
            int[] margins = { -1 };
            DwmExtendFrameIntoClientArea(window.Handle, ref margins);

            SetWindowPos(window.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            
            Rectangle currentScreenBounds = GetCurrentScreenBounds();
            SetWindowSize(new Point(currentScreenBounds.Width, 17 * 3));
            SetFractionalWindowPosition(new Vector2(0, 0));
            
            CreateTrayIcon(windowHandle);
        }

        public void SetFractionalWindowPosition(Vector2 fractionalPosition)
        {
            Rectangle currentScreenBounds = GetCurrentScreenBounds();
            window.Position = new Point(
                (int)(currentScreenBounds.X * fractionalPosition.X) * Math.Sign(fractionalPosition.X - 0.5f) * -1,
                (int)(currentScreenBounds.Y * fractionalPosition.Y) * Math.Sign(fractionalPosition.Y - 0.5f) * -1
            );
        }

        private Rectangle GetCurrentScreenBounds()
        {
            System.Drawing.Rectangle boundsSystemType = Screen.FromHandle(window.Handle).Bounds;
            return new Rectangle(boundsSystemType.X, boundsSystemType.Y, boundsSystemType.Width, boundsSystemType.Height);
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
            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Icon = icon;
            notifyIcon.Text = "Right-click to open the settings menu";
            notifyIcon.Visible = true;

            notifyIcon.BalloonTipTitle = "DeltaTune is now running!";
            notifyIcon.BalloonTipText = "Play some music to get started or click the DeltaTune icon in your system tray for customization options.";
            notifyIcon.ShowBalloonTip(1000);

            notifyIcon.MouseClick += (sender, args) =>
            {
                if (args.Button == MouseButtons.Left || args.Button == MouseButtons.Right)
                {
                    settingsMenu.Show();
                }
            };
        }
    }
}