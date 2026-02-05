using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CloudStream.Desktop.Models;
using CloudStream.Desktop.Plugins;
using CloudStream.Desktop.Services;

namespace CloudStream.Desktop.Views
{
    /// <summary>
    ///     Interaction logic for <see cref="SearchPage"/>.  This page allows the
    ///     user to search across all loaded providers and displays the
    ///     aggregated results.  Selecting a result navigates directly to the
    ///     player page for that media item.
    /// </summary>
    public partial class SearchPage : Page
    {
        private readonly List<MediaItem> _results = new();

        public SearchPage()
        {
            InitializeComponent();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = QueryBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(query))
                return;

            _results.Clear();
            ResultsList.ItemsSource = null;
            var providers = PluginLoader.Instance.Providers;
            foreach (var provider in providers)
            {
                try
                {
                    var list = await provider.SearchAsync(query);
                    if (list != null)
                    {
                        // annotate provider id so PlayerPage can find provider later
                        foreach (var item in list)
                        {
                            // Use provider.Name as ProviderId if not already set
                            if (string.IsNullOrEmpty(item.ProviderId))
                                item.ProviderId = provider.Name;
                            _results.Add(item);
                        }
                    }
                }
                catch
                {
                    // ignore provider failure
                }
            }

            ResultsList.ItemsSource = _results;
        }

        private async void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultsList.SelectedItem is MediaItem item)
            {
                var provider = PluginLoader.Instance.Providers.FirstOrDefault(p => p.Name == item.ProviderId);
                if (provider != null)
                {
                    try
                    {
                        // Load details to populate description and children (not used yet)
                        await provider.LoadDetailsAsync(item);
                    }
                    catch
                    {
                        // ignore errors
                    }
                }
                // Navigate to player page with no resume
                NavigationService?.Navigate(new PlayerPage(item, 0));
            }
        }
    }
}