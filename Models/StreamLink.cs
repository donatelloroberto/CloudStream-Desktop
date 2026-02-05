using System.Collections.Generic;

namespace CloudStream.Desktop.Models
{
    /// <summary>
    ///     Represents a stream.  Each stream is associated with a quality label
    ///     (e.g. 1080p, 720p) and a URL.  Optional HTTP headers can be set on
    ///     the request when playing the stream.  Some providers may return
    ///     multiple streams for the same quality; the player will select
    ///     whichever is appropriate.
    /// </summary>
    public class StreamLink
    {
        public string Quality { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public Dictionary<string, string>? Headers { get; set; }
    }
}