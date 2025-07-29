using Microsoft.Xna.Framework;
using R3;

namespace DeltaTune.Settings
{
    public interface ISettingsService
    {
        bool IsFactorySettings { get; set; }
        ReactiveProperty<int> ScaleFactor { get; }
        ReactiveProperty<Vector2> Position { get; }
        ReactiveProperty<bool> ShowArtistName { get; }
        ReactiveProperty<bool> ShowPlaybackStatus { get; }
        ReactiveProperty<bool> HideAutomatically { get; }
    }
}