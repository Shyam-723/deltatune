using System;

namespace DeltaTune.Media
{
    public struct MediaInfo : IEquatable<MediaInfo>
    {
        public string Title;
        public string Artist;
        public PlaybackStatus Status;

        public MediaInfo(string title, string artist, PlaybackStatus status)
        {
            Title = title;
            Artist = artist;
            Status = status;
        }

        public bool Equals(MediaInfo other)
        {
            return Title == other.Title && Artist == other.Artist && Status == other.Status;
        }

        public override bool Equals(object obj)
        {
            return obj is MediaInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Title, Artist, (int)Status);
        }

        public override string ToString()
        {
            return $"{nameof(Title)}: {Title}, {nameof(Artist)}: {Artist}, {nameof(Status)}: {Status}";
        }
    }
}