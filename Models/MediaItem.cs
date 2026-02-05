using System;
using System.Collections.Generic;

namespace CloudStream.Desktop.Models
{
    /// <summary>
    ///     Represents a generic media item within the CloudStream ecosystem.  A media
    ///     item may be a movie, TV show, season, or episode.  Plugins create
    ///     instances of this class when returning search results or detailed
    ///     information.  The <see cref="ProviderId"/> and <see cref="Id"/>
    ///     uniquely identify the item within the context of its provider.
    /// </summary>
    public class MediaItem
    {
        public string ProviderId { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public MediaType Type { get; set; } = MediaType.Movie;
        public List<MediaItem> Children { get; set; } = new();
        public Dictionary<string, object?> Properties { get; set; } = new();
    }

    /// <summary>
    ///     Enum describing the high level type of the media item.  Used to distinguish
    ///     between movies, shows, seasons and episodes when rendering the UI.
    /// </summary>
    public enum MediaType
    {
        Movie,
        Show,
        Season,
        Episode
    }
}