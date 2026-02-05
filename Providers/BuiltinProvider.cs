using System.Collections.Generic;
using System.Threading.Tasks;
using CloudStream.Desktop.Models;
using CloudStream.Desktop.Plugins;

namespace CloudStream.Desktop.Providers
{
    /// <summary>
    ///     A simple built‑in provider that returns a few hardcoded media items
    ///     for demonstration purposes.  This allows the application to function
    ///     without any external plugins being present.
    /// </summary>
    public class BuiltinProvider : IProvider
    {
        public string Name => "Builtin";

        public Task<List<MediaItem>> SearchAsync(string query)
        {
            // Return fake search results containing the query string.
            var list = new List<MediaItem>
            {
                new MediaItem
                {
                    ProviderId = Name,
                    Id = "demo1",
                    Title = $"Sample movie for '{query}'",
                    Description = "This is a demo movie returned by the built‑in provider.",
                    PosterUrl = null,
                    Type = MediaType.Movie
                },
                new MediaItem
                {
                    ProviderId = Name,
                    Id = "demo2",
                    Title = $"Another sample for '{query}'",
                    Description = "This is another demo item.",
                    PosterUrl = null,
                    Type = MediaType.Movie
                }
            };
            return Task.FromResult(list);
        }

        public Task<MediaItem> LoadDetailsAsync(MediaItem item)
        {
            // No extra details to load for demo items.
            return Task.FromResult(item);
        }

        public Task<List<StreamLink>> GetStreamsAsync(MediaItem item)
        {
            // Return a dummy stream pointing to a remote sample video.  Users can
            // replace this with actual streaming URLs when writing real providers.
            var streams = new List<StreamLink>
            {
                new StreamLink
                {
                    Quality = "360p",
                    Url = "https://sample-videos.com/video123/mp4/480/asdasdas.mp4",
                    Headers = null
                }
            };
            return Task.FromResult(streams);
        }
    }
}