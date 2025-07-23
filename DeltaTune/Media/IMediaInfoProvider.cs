using System.Collections.Concurrent;

namespace DeltaTune.Media
{
    public interface IMediaInfoProvider
    {
        ConcurrentQueue<MediaInfo> UpdateQueue { get; }
        bool IsCurrentlyStopped();
    }
}