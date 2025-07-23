using DeltaTune.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace DeltaTune.Display
{
    public class DisplayService : IDisplayService
    {
        private const double SwapThrottleInterval = 2;
        private const double LongSwapThrottleInterval = 2.5;
        private const double LastMediaInfoUpdateStopCheckDelay = 1;
        
        private readonly IMediaInfoProvider mediaInfoProvider;
        private readonly GraphicsDevice graphicsDevice;
        
        private SpriteBatch spriteBatch;
        private BitmapFont musicTitleFont;
        
        private MediaInfo currentMediaInfo;
        private IMusicTitleDisplay primaryDisplay;
        private IMusicTitleDisplay secondaryDisplay;
        private double lastSwapTime;
        private bool disappearAutomatically = true;
        private bool showPlaybackStatus = false;
        private bool previouslyPaused = false;
        private bool isFirstReceivedMediaUpdate = true;
        private double lastMediaInfoUpdateTime;
        private double currentSwapThrottleInterval = SwapThrottleInterval;
        
        public DisplayService(IMediaInfoProvider mediaInfoProvider, GraphicsDevice graphicsDevice)
        {
            this.mediaInfoProvider = mediaInfoProvider;
            this.graphicsDevice = graphicsDevice;
        }

        public void LoadContent()
        {
            musicTitleFont = BitmapFont.FromFile(graphicsDevice, "Content/Fonts/MusicTitleFont.fnt");
            musicTitleFont.FallbackCharacter = '▯';
            
            spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public void BeginRun()
        {
            primaryDisplay = new MusicTitleDisplay(musicTitleFont)
            {
                ScaleFactor = 3,
                DisappearAutomatically = disappearAutomatically
            };
            
            secondaryDisplay = new MusicTitleDisplay(musicTitleFont)            
            {
                ScaleFactor = 3,
                DisappearAutomatically = disappearAutomatically
            };
        }

        public void Update(GameTime gameTime)
        {
            bool titleChanged = false, artistChanged = false, statusChanged = false;
            while (mediaInfoProvider.UpdateQueue.TryDequeue(out MediaInfo mediaInfo))
            {
                if (isFirstReceivedMediaUpdate)
                {
                    if (!showPlaybackStatus && mediaInfo.Status != PlaybackStatus.Playing)
                    {
                        continue;
                    }
                    
                    isFirstReceivedMediaUpdate = false;
                }
                
                if (!showPlaybackStatus && mediaInfo.Status == PlaybackStatus.Paused)
                {
                    mediaInfo.Status = PlaybackStatus.Playing;
                    previouslyPaused = true;
                } 
                else if (!showPlaybackStatus && mediaInfo.Status == PlaybackStatus.Playing)
                {
                    if (currentMediaInfo.Artist == mediaInfo.Artist &&
                        currentMediaInfo.Title == mediaInfo.Title &&
                        previouslyPaused)
                    {
                        statusChanged = true;
                    }
                    
                    mediaInfo.Status = PlaybackStatus.Playing;
                    previouslyPaused = false;
                }
                
                titleChanged |= mediaInfo.Title != currentMediaInfo.Title;
                artistChanged |= mediaInfo.Artist != currentMediaInfo.Artist;
                statusChanged |= mediaInfo.Status != currentMediaInfo.Status;
                
                currentMediaInfo = mediaInfo;
                lastMediaInfoUpdateTime = gameTime.TotalGameTime.TotalSeconds;
            }

            if (lastMediaInfoUpdateTime + LastMediaInfoUpdateStopCheckDelay < gameTime.TotalGameTime.TotalSeconds)
            {
                if(mediaInfoProvider.IsCurrentlyStopped())
                {
                    currentMediaInfo.Status = PlaybackStatus.Stopped;
                    if (primaryDisplay.State != MusicTitleDisplayState.Hidden &&
                        primaryDisplay.State != MusicTitleDisplayState.Disappearing)
                    {
                        primaryDisplay.State = MusicTitleDisplayState.Disappearing;
                        lastSwapTime = gameTime.TotalGameTime.TotalSeconds;
                    }
                }
                
                lastMediaInfoUpdateTime = gameTime.TotalGameTime.TotalSeconds;
            }
            
            if (titleChanged || artistChanged || statusChanged)
            {
                if (IsSwapThrottled(gameTime.TotalGameTime.TotalSeconds) || (!disappearAutomatically && !(titleChanged || artistChanged)))
                {
                    if (primaryDisplay.State == MusicTitleDisplayState.Visible || primaryDisplay.State == MusicTitleDisplayState.Disappearing)
                    {
                        primaryDisplay.State = MusicTitleDisplayState.Visible;
                    } 
                    else if (primaryDisplay.State == MusicTitleDisplayState.AppearingDelay || primaryDisplay.State == MusicTitleDisplayState.Hidden)
                    {
                        primaryDisplay.State = MusicTitleDisplayState.Appearing;
                    }
                    
                    if (!(!disappearAutomatically && !(titleChanged || artistChanged)))
                    {
                        currentSwapThrottleInterval = SwapThrottleInterval;
                        lastSwapTime = gameTime.TotalGameTime.TotalSeconds;
                    }
                }
                else
                {
                    SwapAndShowPrimaryDisplay();
                    lastSwapTime = gameTime.TotalGameTime.TotalSeconds;
                }
            }
            
            primaryDisplay.Content = currentMediaInfo;
            
            secondaryDisplay.Update(gameTime);
            primaryDisplay.Update(gameTime);
        }
        
        public void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            primaryDisplay.Draw(spriteBatch, gameTime);
            secondaryDisplay.Draw(spriteBatch, gameTime);
            spriteBatch.End();
        }
        
        private void SwapAndShowPrimaryDisplay()
        {
            (primaryDisplay, secondaryDisplay) = (secondaryDisplay, primaryDisplay);

            if (secondaryDisplay.State == MusicTitleDisplayState.Hidden)
            {
                primaryDisplay.State = MusicTitleDisplayState.Appearing;
                currentSwapThrottleInterval = SwapThrottleInterval;
            }
            else
            {
                primaryDisplay.State = MusicTitleDisplayState.AppearingDelay;
                currentSwapThrottleInterval = LongSwapThrottleInterval;
            }
                
            if (secondaryDisplay.State == MusicTitleDisplayState.Visible || secondaryDisplay.State == MusicTitleDisplayState.Appearing)
            {
                secondaryDisplay.State = MusicTitleDisplayState.Disappearing;
            }
            else if (secondaryDisplay.State == MusicTitleDisplayState.AppearingDelay)
            {
                secondaryDisplay.State = MusicTitleDisplayState.Hidden;
            }
        }

        private bool IsSwapThrottled(double currentTime)
        {
            return currentTime != 0 && lastSwapTime + currentSwapThrottleInterval >= currentTime;
        }
    }
}