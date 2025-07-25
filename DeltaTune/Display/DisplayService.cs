using System;
using DeltaTune.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace DeltaTune.Display
{
    public class DisplayService : IDisplayService
    {
        private const double LastMediaInfoUpdateStopCheckDelay = 1;
        
        private readonly IMediaInfoProvider mediaInfoProvider;
        private readonly GraphicsDevice graphicsDevice;
        
        private SpriteBatch spriteBatch;
        private BitmapFont musicTitleFont;
        
        private MediaInfo currentMediaInfo;
        private IMusicTitleDisplay primaryDisplay;
        private IMusicTitleDisplay secondaryDisplay;
        private bool disappearAutomatically = true;
        private bool showPlaybackStatus = false;

        private double lastMediaInfoUpdateTime;
        
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
                ScaleFactor = 4,
                DisappearAutomatically = disappearAutomatically,
                ShowPlaybackStatus = showPlaybackStatus
            };
            
            secondaryDisplay = new MusicTitleDisplay(musicTitleFont)            
            {
                ScaleFactor = 4,
                DisappearAutomatically = disappearAutomatically,
                ShowPlaybackStatus = showPlaybackStatus
            };
        }

        public void Update(GameTime gameTime)
        {
            bool titleChanged = false, artistChanged = false, statusChanged = false;
            while (mediaInfoProvider.UpdateQueue.TryDequeue(out MediaInfo mediaInfo))
            {
                titleChanged |= mediaInfo.Title != currentMediaInfo.Title;
                artistChanged |= mediaInfo.Artist != currentMediaInfo.Artist;
                statusChanged |= mediaInfo.Status != currentMediaInfo.Status;
                
                currentMediaInfo = mediaInfo;
                lastMediaInfoUpdateTime = gameTime.TotalGameTime.TotalSeconds;
            }

            bool shouldUpdateDisplayState = showPlaybackStatus ? titleChanged || artistChanged || statusChanged : titleChanged || artistChanged;

            // Even if playback status shouldn't be shown, show the song title again when resuming playback
            if (!shouldUpdateDisplayState && !showPlaybackStatus && statusChanged &&
                currentMediaInfo.Status == PlaybackStatus.Playing)
            {
                shouldUpdateDisplayState = true;
            }
            
            // If it's been too long since the last media info update, check if anything is still playing
            // If not, make the display disappear
            if (lastMediaInfoUpdateTime + LastMediaInfoUpdateStopCheckDelay < gameTime.TotalGameTime.TotalSeconds)
            {
                if(mediaInfoProvider.IsCurrentlyStopped())
                {
                    currentMediaInfo.Status = PlaybackStatus.Stopped;
                    
                    if (primaryDisplay.State != MusicTitleDisplayState.Disappearing &&
                        primaryDisplay.State != MusicTitleDisplayState.Hidden)
                    {
                        primaryDisplay.State = MusicTitleDisplayState.Disappearing;
                    }
                }
                
                lastMediaInfoUpdateTime = gameTime.TotalGameTime.TotalSeconds;
            }
            
            // If the display is hidden and doesn't show playback status,
            // only start showing it once the media state changes to playing
            if (!showPlaybackStatus && 
                shouldUpdateDisplayState && 
                currentMediaInfo.Status != PlaybackStatus.Playing && 
                primaryDisplay.State == MusicTitleDisplayState.Hidden)
            {
                shouldUpdateDisplayState = false;
            }
            
            if (shouldUpdateDisplayState)
            {
                switch (primaryDisplay.State)
                {
                    case MusicTitleDisplayState.Hidden:
                        SwapAndShowPrimaryDisplay();
                        break;
                    case MusicTitleDisplayState.AppearingDelay:
                        break;
                    case MusicTitleDisplayState.Appearing:
                        break;
                    case MusicTitleDisplayState.Visible:
                        // Make the slide animation appear even if the display doesn't disappear automatically
                        if (!disappearAutomatically && (titleChanged || artistChanged))
                        {
                            SwapAndShowPrimaryDisplay();
                        }
                        else
                        {
                            primaryDisplay.State = MusicTitleDisplayState.Visible;
                        }

                        break;
                    case MusicTitleDisplayState.Disappearing:
                        SwapAndShowPrimaryDisplay();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            primaryDisplay.Update(gameTime);
            secondaryDisplay.Update(gameTime);
            
            primaryDisplay.Content = currentMediaInfo;
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
            }
            else
            {
                if (secondaryDisplay.State != MusicTitleDisplayState.Disappearing)
                {
                    secondaryDisplay.State = MusicTitleDisplayState.Disappearing;
                }
                
                primaryDisplay.State = MusicTitleDisplayState.AppearingDelay;
            }
        }
    }
}