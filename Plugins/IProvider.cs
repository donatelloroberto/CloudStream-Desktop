using System.Collections.Generic;
using System.Threading.Tasks;
using CloudStream.Desktop.Models;

namespace CloudStream.Desktop.Plugins
{
    /// <summary>
    ///     Interface that all provider plugins must implement.  Providers are
    ///     responsible for returning search results, loading detailed item
    ///     information, and resolving streaming links.  Plugins are loaded
    ///     dynamically at runtime from assemblies located in the Plugins
    ///     directory.
    /// </summary>
    public interface IProvider
    {
        /// <summary>
        ///     Gets the human readable name of the provider.  This is shown to
        ///     users in the Extensions manager.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Perform a search against the provider using the supplied query.
        ///     Providers should return a list of basic <see cref="MediaItem"/>
        ///     objects without detailed metadata.  The UI will call
        ///     <see cref="LoadDetailsAsync"/> when the user selects an item.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <returns>A list of media items matching the query.</returns>
        Task<List<MediaItem>> SearchAsync(string query);

        /// <summary>
        ///     Load full details for the specified media item.  This method
        ///     should populate fields such as description, poster URL, and any
        ///     children (e.g. seasons for a show).
        /// </summary>
        /// <param name="item">The media item to load details for.</param>
        /// <returns>The updated media item.</returns>
        Task<MediaItem> LoadDetailsAsync(MediaItem item);

        /// <summary>
        ///     Get the available streams for the specified media item.  This
        ///     returns a list of streams along with their qualities and any
        ///     necessary request headers.  The player will choose the appropriate
        ///     stream.
        /// </summary>
        /// <param name="item">The media item to get streams for.</param>
        /// <returns>A list of stream links.</returns>
        Task<List<StreamLink>> GetStreamsAsync(MediaItem item);
    }
}