using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using CloudStream.Desktop.Plugins;

namespace CloudStream.Desktop.Services
{
    /// <summary>
    ///     Responsible for loading provider plugins from the Plugins directory.
    ///     Each plugin assembly is loaded into its own <see cref="AssemblyLoadContext"/>
    ///     to provide isolation and allow unloading in the future.  After loading,
    ///     the provider instances are exposed via <see cref="Providers"/>.
    /// </summary>
    public class PluginLoader
    {
        private readonly List<IProvider> _providers = new();

        private static readonly Lazy<PluginLoader> _instance = new(() => new PluginLoader());

        /// <summary>
        ///     Gets the singleton instance of the plugin loader.
        /// </summary>
        public static PluginLoader Instance => _instance.Value;

        /// <summary>
        ///     Gets the list of loaded providers.
        /// </summary>
        public IReadOnlyList<IProvider> Providers => _providers;

        private PluginLoader()
        {
            // private constructor ensures singleton usage.  Loading occurs
            // automatically upon construction.
            LoadPlugins();
        }

        /// <summary>
        ///     Scans the Plugins directory for assemblies and attempts to load
        ///     provider implementations from each.  Any exceptions thrown
        ///     during loading are caught and ignored to ensure a faulty plugin
        ///     does not prevent the application from starting.
        /// </summary>
        private void LoadPlugins()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string pluginsDir = Path.Combine(baseDir, "Plugins");
            if (!Directory.Exists(pluginsDir))
                Directory.CreateDirectory(pluginsDir);

            foreach (var file in Directory.EnumerateFiles(pluginsDir, "*.dll", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var context = new PluginLoadContext(file);
                    var assembly = context.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(file)));
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!typeof(IProvider).IsAssignableFrom(type) || type.IsAbstract)
                            continue;

                        if (Activator.CreateInstance(type) is IProvider provider)
                        {
                            _providers.Add(provider);
                        }
                    }
                }
                catch
                {
                    // swallow any errors during loading to isolate plugins
                }
            }

            // Always register the built‑in provider so that the application
            // functions without external extensions.  External providers can
            // override this by returning their own results.
            try
            {
                var builtin = new CloudStream.Desktop.Providers.BuiltinProvider();
                _providers.Add(builtin);
            }
            catch
            {
                // ignore failure to instantiate built‑in provider (should not happen)
            }
        }

        /// <summary>
        ///     Assembly load context providing isolation for plugin assemblies.  By
        ///     specifying the default context as parent for resource resolving we
        ///     ensure referenced framework assemblies are resolved correctly.
        /// </summary>
        private class PluginLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver _resolver;

            public PluginLoadContext(string pluginPath) : base(isCollectible: true)
            {
                _resolver = new AssemblyDependencyResolver(pluginPath);
            }

            protected override Assembly? Load(AssemblyName assemblyName)
            {
                string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
                if (assemblyPath != null)
                {
                    return LoadFromAssemblyPath(assemblyPath);
                }
                return null;
            }
        }
    }
}