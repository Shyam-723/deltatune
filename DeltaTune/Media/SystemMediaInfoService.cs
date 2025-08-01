using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace DeltaTune.Media
{
    public class SystemMediaInfoService : IMediaInfoService, IDisposable
    {
        public ConcurrentQueue<MediaInfo> UpdateQueue { get; }
        
        private GlobalSystemMediaTransportControlsSessionManager currentSessionManager;
        private GlobalSystemMediaTransportControlsSession currentSession;
        private MediaInfo lastMediaInfo;
        
        public SystemMediaInfoService()
        {
            UpdateQueue = new ConcurrentQueue<MediaInfo>();
            
            Task.Run(async () =>
            {
                GlobalSystemMediaTransportControlsSessionManager sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                currentSessionManager = sessionManager;
                sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                
                await Task.Run(() => OnCurrentSessionChanged(sessionManager, null));
            }).Wait();
        }
        
        public bool IsCurrentlyStopped()
        {
            lock (currentSessionManager)
            {
                if (currentSessionManager == null || currentSession == null) return true;
            
                return PlaybackStatusHelper.FromSystemPlaybackStatus(currentSession.GetPlaybackInfo().PlaybackStatus) == PlaybackStatus.Stopped;
            }
        }
        
        private void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sessionManager, CurrentSessionChangedEventArgs args)
        {
            lock (currentSessionManager)
            {
                if (currentSession != null)
                {
                    currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                    currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                }
            
                GlobalSystemMediaTransportControlsSession session = sessionManager.GetCurrentSession();
                currentSession = session;
            
                if (session != null)
                {
                    session.MediaPropertiesChanged += OnMediaPropertiesChanged;
                    OnMediaPropertiesChanged(session, null);
                    session.PlaybackInfoChanged += OnPlaybackInfoChanged;
                    OnPlaybackInfoChanged(session, null);
                }
            }
        }
        
        private void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            try
            {
                MediaInfo? newMediaInfo = GetCurrentMediaInfo(sender).Result;
                if (newMediaInfo != null && (newMediaInfo.Value.Title != lastMediaInfo.Title || newMediaInfo.Value.Artist != lastMediaInfo.Artist))
                {
                    // Reject updates with empty title or artist
                    if (newMediaInfo.Value.Title == string.Empty || newMediaInfo.Value.Artist == string.Empty)
                    {
                        return;
                    }
                    
                    UpdateQueue.Enqueue(newMediaInfo.Value);
                    lastMediaInfo = newMediaInfo.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving media properties: {ex.Message}");
            }
        }

        private void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            try
            {
                GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo = sender.GetPlaybackInfo();
                PlaybackStatus newStatus = PlaybackStatusHelper.FromSystemPlaybackStatus(playbackInfo.PlaybackStatus);
                if (newStatus != lastMediaInfo.Status)
                {
                    MediaInfo? newMediaInfo = GetCurrentMediaInfo(sender).Result;
                    if (newMediaInfo != null)
                    {
                        lastMediaInfo = newMediaInfo.Value;
                    }
                    
                    MediaInfo update = new MediaInfo(lastMediaInfo.Title, lastMediaInfo.Artist, newStatus);

                    // Reject updates with empty title or artist
                    if (update.Title == string.Empty || update.Artist == string.Empty)
                    {
                        return;
                    }

                    UpdateQueue.Enqueue(update);
                    lastMediaInfo = update;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving playback info: {ex.Message}");
            }
        }

        private async Task<MediaInfo?> GetCurrentMediaInfo(GlobalSystemMediaTransportControlsSession session)
        {
            GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties = await session.TryGetMediaPropertiesAsync();

            string correctedArtist = mediaProperties.Artist.Trim();
            string correctedTitle = mediaProperties.Title.Trim();

            // Remove YouTube's "- Topic" suffix
            if (correctedArtist.EndsWith(" - Topic"))
            {
                correctedArtist = correctedArtist.Substring(0, correctedArtist.Length - 8);
            }

            // Remove artist prefix from the title if it exists
            if (correctedTitle.StartsWith($"{correctedArtist} - "))
            {
                correctedTitle = correctedTitle.Remove(0, $"{correctedArtist} - ".Length);
            }

            // Remove artist suffix from the title if it exists
            if (correctedTitle.EndsWith($" - {correctedArtist}"))
            {
                int startIndex = correctedTitle.LastIndexOf($" - {correctedArtist}", StringComparison.Ordinal);
                correctedTitle = correctedTitle.Remove(startIndex);
            }

            return new MediaInfo(correctedTitle, correctedArtist, lastMediaInfo.Status);
        }

        public void Dispose()
        {
            if (currentSessionManager != null)
            {
                currentSessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;
            }

            if (currentSession != null)
            {
                currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
            }
        }
    }
}