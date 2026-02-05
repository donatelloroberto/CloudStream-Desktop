using System.Windows;
using System.Windows.Controls;
using CloudStream.Desktop.Services;

namespace CloudStream.Desktop.Views
{
    /// <summary>
    ///     Interaction logic for <see cref="SettingsPage"/>.  This page exposes
    ///     basic application settings such as theme selection and allows the
    ///     user to clear their watch history.
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            DatabaseService.Instance.ClearHistory();
            MessageBox.Show("History cleared.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}