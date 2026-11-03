using System;
using System.IO;
using System.Reflection;

namespace SaveSyncPlugin.Core.Sync
{
    /// <summary>
    /// Single source of truth for "where is the plugin" and "where is LaunchBox".
    ///
    /// Nothing here uses <see cref="AppDomain.BaseDirectory"/> as the root of the
    /// install: since LaunchBox 14 the managed host is <c>&lt;root&gt;\Core\LaunchBox.exe</c>,
    /// so <c>BaseDirectory</c> is <c>&lt;root&gt;\Core</c> and any relative path built
    /// from it lands one level too deep (that was the v0.0.3 bug: <c>..\Emuladores</c>
    /// resolved to <c>&lt;root&gt;\Emuladores</c> instead of the real emulator folder).
    ///
    /// The plugin folder comes from the assembly location, which is the folder the
    /// DLL was actually loaded from, and the LaunchBox root is validated by looking
    /// for the files/folders a real install has (<c>Data\Settings.xml</c>,
    /// <c>Plugins</c> + <c>Core</c>, or the legacy <c>LaunchBox.exe</c> + <c>Plugins</c>).
    /// A directory called <c>Core</c> or <c>Plugins</c> is never accepted as the root.
    /// </summary>
    public static class PluginPaths
    {
        public const string PluginFolderName = "SaveSync LaunchBox Integration";
        public const string PluginsFolderName = "Plugins";
        public const string CoreFolderName = "Core";
        public const string DataFolderName = "Data";

        /// <summary>Process base directory (the managed host folder, usually <c>&lt;root&gt;\Core</c>).</summary>
        public static string BaseDirectory => Trim(AppDomain.CurrentDomain.BaseDirectory);

        /// <summary>Folder of the running SaveSyncPlugin.Core.dll (empty when loaded from memory).</summary>
        public static string AssemblyLocation
        {
            get
            {
                try
                {
                    var location = Assembly.GetExecutingAssembly().Location;
                    return string.IsNullOrWhiteSpace(location) ? null : location;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>Folder that holds settings.json / emulators.json / games.json / sync.log.</summary>
        public static string PluginFolder =>
            ResolvePluginFolder(AssemblyLocation, BaseDirectory);

        /// <summary>LaunchBox install folder (the folder that contains <c>Plugins</c> and <c>Data</c>).</summary>
        public static string LaunchBoxRoot => FindRoot(PluginFolder);

        public static string Version
        {
            get
            {
                try
                {
                    var version = typeof(PluginPaths).Assembly.GetName().Version;
                    return version == null ? "?" : version.ToString();
                }
                catch
                {
                    return "?";
                }
            }
        }

        /// <summary>
        /// Resolves the plugin folder from the assembly location first, then from the
        /// process base directory (covers <c>Assembly.Location</c> being empty).
        /// </summary>
        public static string ResolvePluginFolder(string assemblyLocation, string baseDirectory)
        {
            var fromAssembly = SafeDirectory(assemblyLocation);
            if (fromAssembly != null && Directory.Exists(fromAssembly))
                return fromAssembly;

            var baseDir = Trim(baseDirectory);
            if (!string.IsNullOrWhiteSpace(baseDir))
            {
                if (IsSameName(baseDir, PluginFolderName))
                    return baseDir;

                var candidates = new[]
                {
                    Path.Combine(baseDir, PluginsFolderName, PluginFolderName),
                    CombineParent(baseDir, 1),
                    CombineParent(baseDir, 2)
                };

                foreach (var candidate in candidates)
                {
                    if (candidate != null && Directory.Exists(candidate))
                        return candidate;
                }

                var fallback = Path.Combine(baseDir, PluginsFolderName, PluginFolderName);
                if (fromAssembly != null)
                    return fromAssembly;
                return fallback;
            }

            return fromAssembly;
        }

        /// <summary>
        /// Walks up from <paramref name="startDirectory"/> and returns the first folder
        /// that looks like a LaunchBox install. Never returns a folder called
        /// <c>Core</c> or <c>Plugins</c>.
        /// </summary>
        public static string FindRoot(string startDirectory)
        {
            var start = Trim(startDirectory);
            if (string.IsNullOrWhiteSpace(start))
                return null;

            try
            {
                var dir = new DirectoryInfo(Path.GetFullPath(start));
                string pluginsParent = null;

                while (dir != null)
                {
                    if (IsLaunchBoxRoot(dir.FullName))
                        return dir.FullName;

                    if (pluginsParent == null && IsSameName(dir.FullName, PluginsFolderName))
                        pluginsParent = dir.Parent?.FullName;

                    dir = dir.Parent;
                }

                if (pluginsParent != null)
                    return pluginsParent;

                // Nothing looked like an install: never hand back the host folders.
                var startFull = Path.GetFullPath(start);
                if (IsHostFolderName(startFull))
                {
                    var parent = Directory.GetParent(startFull);
                    return parent?.FullName ?? startFull;
                }

                return startFull;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>True when the folder really is a LaunchBox install folder.</summary>
        public static bool IsLaunchBoxRoot(string directory)
        {
            var dir = Trim(directory);
            if (string.IsNullOrWhiteSpace(dir))
                return false;

            // Core and Plugins are always children of the install, never the install.
            if (IsHostFolderName(dir))
                return false;

            try
            {
                if (File.Exists(Path.Combine(dir, DataFolderName, "Settings.xml")))
                    return true;

                var hasPlugins = Directory.Exists(Path.Combine(dir, PluginsFolderName));
                if (!hasPlugins)
                    return false;

                if (Directory.Exists(Path.Combine(dir, CoreFolderName)))
                    return true;

                // Legacy layout: LaunchBox.exe next to Plugins, no Core folder.
                return File.Exists(Path.Combine(dir, "LaunchBox.exe"));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>One-line diagnostic written at the top of every sync.log run.</summary>
        public static string Describe()
        {
            var root = LaunchBoxRoot;
            return $"ROOT    {root ?? "(nao encontrado)"} | base={BaseDirectory ?? "-"} | " +
                   $"cwd={SafeCurrentDirectory()} | asm={AssemblyLocation ?? "(sem location)"} | v{Version}";
        }

        private static string SafeCurrentDirectory()
        {
            try { return Environment.CurrentDirectory; }
            catch { return "-"; }
        }

        private static string CombineParent(string directory, int levels)
        {
            try
            {
                var info = new DirectoryInfo(directory);
                for (var i = 0; i < levels && info?.Parent != null; i++)
                    info = info.Parent;

                return info == null ? null : Path.Combine(info.FullName, PluginsFolderName, PluginFolderName);
            }
            catch
            {
                return null;
            }
        }

        private static string SafeDirectory(string path)
        {
            var value = Trim(path);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            try { return Path.GetDirectoryName(value); }
            catch { return null; }
        }

        private static bool IsHostFolderName(string directory)
        {
            return IsSameName(directory, CoreFolderName) || IsSameName(directory, PluginsFolderName);
        }

        private static bool IsSameName(string path, string name)
        {
            try
            {
                var trimmed = Trim(path);
                return string.Equals(Path.GetFileName(trimmed), name, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string Trim(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? path
                : path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
