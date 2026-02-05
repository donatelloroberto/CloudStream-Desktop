using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CloudStream.Desktop.Models;
using CloudStream.Desktop.Services;

namespace CloudStream.Desktop.Views
{
    /// <summary>
    ///     Interaction logic for <see cref="HomePage"/>.  This page displays
    ///     the user's recently watched items and bookmarks.  Selection of an
    ///     item will navigate to the player page for that item.
    /// </summary>
    public partial class HomePage : Page
    {
        private List<(MediaItem Item, long Position, long Duration)> _history = new();
        private List<MediaItem> _bookmarks = new();

        public HomePage()
        {
            InitializeComponent();
            Loaded += HomePage_Loaded;
        }

        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            // Load history and bookmarks from the database service.
            _history = new List<(MediaItem, long, long)>(DatabaseService.Instance.GetHistory());
            _bookmarks = new List<MediaItem>(DatabaseService.Instance.GetBookmarks());

            ContinueWatchingList.ItemsSource = _history;
            BookmarksList.ItemsSource = _bookmarks;
        }

        private void ContinueWatchingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContinueWatchingList.SelectedItem is (MediaItem item, long pos, long dur))
            {
                // Navigate to player page with resume position
                var player = new PlayerPage(item, pos);
                NavigationService?.Navigate(player);
            }
        }

        private void BookmarksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BookmarksList.SelectedItem is MediaItem item)
            {
                var player = new PlayerPage(item, 0);
                NavigationService?.Navigate(player);
            }
        }
    }
}