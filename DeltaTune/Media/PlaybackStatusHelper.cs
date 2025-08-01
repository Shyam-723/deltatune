using System;
using Windows.Media.Control;

namespace DeltaTune.Media
{
    public static class PlaybackStatusHelper
    {
        public static PlaybackStatus FromSystemPlaybackStatus(GlobalSystemMediaTransportControlsSessionPlaybackStatus systemPlaybackStatus)
        {
            switch (systemPlaybackStatus)
            {
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed:
                    return PlaybackStatus.Stopped;
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Opened:
                    return PlaybackStatus.Paused;
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Changing:
                    return PlaybackStatus.Paused;
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped:
                    return PlaybackStatus.Stopped;
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing:
                    return PlaybackStatus.Playing;
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused:
                    return PlaybackStatus.Paused;
            }
            
            throw new ArgumentOutOfRangeException(nameof(systemPlaybackStatus), systemPlaybackStatus, null);
        }
    }
}