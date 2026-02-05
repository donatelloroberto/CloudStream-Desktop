namespace CloudStream.Desktop.Models
{
    /// <summary>
    ///     Represents a subtitle file associated with a media item.  Subtitles can
    ///     either be local files on disk or remote resources accessible via a
    ///     URL.  The track language is stored as a BCP-47 language tag (e.g.
    ///     "en-US", "ar", etc.).
    /// </summary>
    public class Subtitle
    {
        public string Language { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsExternal { get; set; } = true;
    }
}