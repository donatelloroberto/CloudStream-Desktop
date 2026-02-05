using System.Windows;
using System.Windows.Controls;

namespace CloudStream.Desktop
{
    /// <summary>
    ///     The main window of the application.  It hosts a navigation bar and
    ///     provides a frame for displaying the various pages that make up the
    ///     application.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Navigate to the home page on startup.
            NavigateToHome();
        }

        private void NavigateToHome()
        {
            MainFrame.Navigate(new Views.HomePage());
        }

        private void NavigateToSearch()
        {
            MainFrame.Navigate(new Views.SearchPage());
        }

        private void NavigateToExtensions()
        {
            MainFrame.Navigate(new Views.ExtensionsPage());
        }

        private void NavigateToSettings()
        {
            MainFrame.Navigate(new Views.SettingsPage());
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToHome();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSearch();
        }

        private void ExtensionsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToExtensions();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSettings();
        }
    }
}