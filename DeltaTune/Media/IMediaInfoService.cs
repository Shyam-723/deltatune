using System.Collections.Concurrent;

namespace DeltaTune.Media
{
    public interface IMediaInfoService
    {
        ConcurrentQueue<MediaInfo> UpdateQueue { get; }
        bool IsCurrentlyStopped();
    }
}