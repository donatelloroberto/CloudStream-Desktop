using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Mpv.Wpf;
using Mpv.NET.Player;
using CloudStream.Desktop.Models;
using CloudStream.Desktop.Plugins;
using CloudStream.Desktop.Services;

namespace CloudStream.Desktop.Views
{
    /// <summary>
    ///     Interaction logic for <see cref="PlayerPage"/>.  This page embeds a
    ///     mpv-based video player using the Mpv.NET library and manages
    ///     playback controls, stream selection and resume functionality.
    /// </summary>
    public partial class PlayerPage : Page
    {
        private readonly MediaItem _item;
        private readonly long _resumePosition;
        private readonly MpvPlayer _player;
        private List<StreamLink> _streams = new();
        private bool _paused;

        public PlayerPage(MediaItem item, long resumePosition)
        {
            _item = item;
            _resumePosition = resumePosition;
            InitializeComponent();

            _player = new MpvPlayer();
            VideoView.SetMpvPlayer(_player);

            Loaded += PlayerPage_Loaded;
            Unloaded += PlayerPage_Unloaded;
        }

        private async void PlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Load available streams from the provider
            var provider = PluginLoader.Instance.Providers.FirstOrDefault(p => p.Name == _item.ProviderId || _item.ProviderId == p.Name);
            if (provider != null)
            {
                try
                {
                    _streams = await provider.GetStreamsAsync(_item);
                }
                catch
                {
                    _streams = new List<StreamLink>();
                }
            }

            StreamSelector.ItemsSource = _streams;
            StreamSelector.DisplayMemberPath = nameof(StreamLink.Quality);
            if (_streams.Count > 0)
            {
                StreamSelector.SelectedIndex = 0;
            }
        }

        private void StreamSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StreamSelector.SelectedItem is StreamLink link)
            {
                PlayStream(link);
            }
        }

        private void PlayStream(StreamLink link)
        {
            // Apply any required headers via mpv options
            var options = new List<string>();
            if (link.Headers != null)
            {
                foreach (var kvp in link.Headers)
                {
                    options.Add($"--http-header-fields={kvp.Key}:{kvp.Value}");
                }
            }
            // The Load method accepts either just the URL or an array of options.
            _player.Load(link.Url, options.ToArray());
            if (_resumePosition > 0)
            {
                // mpv expects seconds for seeking
                double sec = _resumePosition / 1000.0;
                _player.Seek(sec);
            }
            _player.Resume();
            _paused = false;
            PlayPauseButton.Content = "Pause";
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_paused)
            {
                _player.Resume();
                PlayPauseButton.Content = "Pause";
                _paused = false;
            }
            else
            {
                _player.Pause();
                PlayPauseButton.Content = "Play";
                _paused = true;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _player.Stop();
        }

        private void PlayerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Save resume position
            if (_item != null)
            {
                try
                {
                    // Position property returns seconds
                    double position = _player.Position ?? 0;
                    long ms = (long)(position * 1000);
                    DatabaseService.Instance.SaveResumePosition(_item.ProviderId, _item.Id, ms);
                }
                catch
                {
                    // ignore errors while saving resume
                }
            }
            _player.Dispose();
        }
    }
}