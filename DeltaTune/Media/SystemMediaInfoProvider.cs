using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace DeltaTune.Media
{
    public class SystemMediaInfoProvider : IMediaInfoProvider, IDisposable
    {
        public string Title
        {
            get { lock (titleLock) return title; }
            private set { lock (titleLock) title = value; }
        }

        public string Artist
        {
            get { lock (artistLock) return artist; }
            private set { lock (artistLock) artist = value; }
        }

        public PlaybackStatus Status
        {
            get { lock (statusLock) return status; }
            private set { lock (statusLock) status = value; }
        }

        public bool Dirty
        {
            get { lock (dirtyLock) return dirty; }
            set { lock (dirtyLock) dirty = value; }
        }

        private GlobalSystemMediaTransportControlsSessionManager currentSessionManager;
        private GlobalSystemMediaTransportControlsSession currentSession;
        
        private string title = string.Empty;
        private string artist = string.Empty;
        private PlaybackStatus status = PlaybackStatus.Stopped;
        private bool dirty = false;

        private readonly object titleLock = new object();
        private readonly object artistLock = new object();
        private readonly object statusLock = new object();
        private readonly object dirtyLock = new object();
        
        public SystemMediaInfoProvider()
        {
            Task.Run(async () =>
            {
                GlobalSystemMediaTransportControlsSessionManager sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                currentSessionManager = sessionManager;
                sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                
                OnCurrentSessionChanged(sessionManager, null);
            }).Wait();
        }
        
        private void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sessionManager, CurrentSessionChangedEventArgs args)
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
        
        private async void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties = await sender.TryGetMediaPropertiesAsync();
            if (mediaProperties != null)
            {
                if (mediaProperties.Artist != Artist || mediaProperties.Title != Title)
                {
                    Artist = mediaProperties.Artist;
                    Title = mediaProperties.Title;
                    Dirty = true;
                }
            }
        }

        private void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo = sender.GetPlaybackInfo();
            PlaybackStatus newStatus = PlaybackStatusHelper.FromSystemPlaybackStatus(playbackInfo.PlaybackStatus);
            if (newStatus != Status)
            {
                Status = newStatus;
                
                if (Status == PlaybackStatus.Stopped)
                {
                    Artist = string.Empty;
                    Title = string.Empty;
                }
                
                Dirty = true;
            }
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