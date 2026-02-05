using System.Windows;

namespace CloudStream.Desktop
{
    /// <summary>
    ///     Interaction logic for <see cref="App"/>.  This class provides the entry point
    ///     for the WPF application and can be used to perform global initialization
    ///     before any windows are created.
    /// </summary>
    public partial class App : Application
    {
        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize global services.  For example the database service can be
            // initialised here so that tables are created before any view models
            // attempt to access them.  This ensures the persistence layer is ready
            // when the rest of the application loads.
            Services.DatabaseService.Instance.Initialize();
        }
    }
}