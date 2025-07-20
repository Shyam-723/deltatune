namespace DeltaTune.Media
{
    public interface IMediaInfoProvider
    {
        string Title { get; }
        string Artist { get; }
        PlaybackStatus Status { get; }
        bool Dirty { get; set; }
    }
}