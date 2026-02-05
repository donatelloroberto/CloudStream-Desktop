using System.Windows.Controls;
using CloudStream.Desktop.Plugins;

namespace CloudStream.Desktop.Views
{
    /// <summary>
    ///     Interaction logic for <see cref="ExtensionsPage"/>.  This page
    ///     displays the list of loaded providers.  In a full implementation
    ///     this page would also allow enabling/disabling and managing external
    ///     extension repositories.
    /// </summary>
    public partial class ExtensionsPage : Page
    {
        public ExtensionsPage()
        {
            InitializeComponent();
            Loaded += ExtensionsPage_Loaded;
        }

        private void ExtensionsPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ProvidersList.ItemsSource = PluginLoader.Instance.Providers;
        }
    }
}