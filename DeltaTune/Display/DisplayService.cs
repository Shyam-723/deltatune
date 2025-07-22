using System;
using DeltaTune.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace DeltaTune.Display
{
    public class DisplayService : IDisplayService
    {
        private const double SwapThrottleInterval = 2.75f;
        
        private readonly IMediaInfoProvider mediaInfoProvider;
        private readonly GraphicsDevice graphicsDevice;
        
        private SpriteBatch spriteBatch;
        private BitmapFont musicTitleFont;
        
        private MediaInfo currentMediaInfo;
        private IMusicTitleDisplay primaryDisplay;
        private IMusicTitleDisplay secondaryDisplay;
        private double swapThrottleTime;
        private bool showPlaybackStatus = false;
        private bool previouslyPaused = false;
        private bool isFirstReceivedMediaUpdate = true;
        
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
                ScaleFactor = 3
            };
            
            secondaryDisplay = new MusicTitleDisplay(musicTitleFont)            
            {
                ScaleFactor = 3
            };
        }

        public void Update(GameTime gameTime)
        {
            bool titleChanged = false, artistChanged = false, statusChanged = false;
            while (mediaInfoProvider.UpdateQueue.TryDequeue(out MediaInfo mediaInfo))
            {
                Console.WriteLine(mediaInfo);

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
            }
            
            if (titleChanged || artistChanged || statusChanged)
            {
                if (IsSwapThrottled(gameTime.TotalGameTime.TotalSeconds))
                {
                    if (primaryDisplay.State != MusicTitleDisplayState.Appearing)
                    {
                        primaryDisplay.State = MusicTitleDisplayState.Visible;
                    }
                    
                    swapThrottleTime = gameTime.TotalGameTime.TotalSeconds;
                }
                else
                {
                    SwapAndShowPrimaryDisplay();
                    swapThrottleTime = gameTime.TotalGameTime.TotalSeconds;
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
                
            primaryDisplay.State = MusicTitleDisplayState.Appearing;
                
            if (secondaryDisplay.State == MusicTitleDisplayState.Visible || secondaryDisplay.State == MusicTitleDisplayState.Appearing)
            {
                secondaryDisplay.State = MusicTitleDisplayState.Disappearing;
            }
        }

        private bool IsSwapThrottled(double currentTime)
        {
            return currentTime != 0 && swapThrottleTime + SwapThrottleInterval >= currentTime;
        }
    }
}