using System;
using System.IO;

namespace SaveSyncPlugin.Core.Sync
{
    /// <summary>
    /// Resolves the local and server folders used by an emulator sync.
    ///
    /// Everything is relative on purpose: the whole tree (LaunchBox + emulators +
    /// server folder) can live under any path or drive letter, as long as the
    /// relative layout is preserved.
    /// </summary>
    public static class PathResolver
    {
        /// <summary>
        /// Local side. <paramref name="localBase"/> is relative to
        /// <paramref name="launchBoxRoot"/>; when it is empty the folder of the
        /// emulator application is used (the wrapper folder, for wrapped emulators).
        /// </summary>
        public static string ResolveLocal(
            string launchBoxRoot,
            string applicationPath,
            string localBase,
            string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

            var baseDir = string.IsNullOrWhiteSpace(localBase)
                ? SafeDirectoryName(applicationPath)
                : localBase;

            if (string.IsNullOrWhiteSpace(baseDir))
                return null;

            return SafeFullPath(launchBoxRoot, baseDir, relativePath);
        }

        /// <summary>
        /// Server side: always under <paramref name="serverBasePath"/>. A path that
        /// escapes it (for example the old <c>..\..\..\Emuladores\...</c> entry) is
        /// refused instead of silently writing outside the backup folder.
        /// </summary>
        public static string ResolveServer(string serverBasePath, string folderName, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(serverBasePath) || string.IsNullOrWhiteSpace(relativePath))
                return null;

            var root = SafeFullPath(serverBasePath);
            if (root == null)
                return null;

            var full = SafeFullPath(serverBasePath, folderName, relativePath);
            if (full == null)
                return null;

            return IsUnder(full, root) ? full : null;
        }

        /// <summary>
        /// True when the emulator is launched through a wrapper executable. Used to
        /// warn when <see cref="Models.EmulatorSaveConfig.LocalBase"/> is empty: the
        /// relative paths then start at the wrapper folder, not at the emulator one.
        /// </summary>
        public static bool UsesWrapper(string applicationPath)
        {
            if (string.IsNullOrWhiteSpace(applicationPath))
                return false;

            return Path.GetFileName(applicationPath).IndexOf("wrapper", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Finds the LaunchBox folder from any directory inside the install (the
        /// plugin folder, when the CLI runs as a separate process). Relative paths in
        /// <see cref="Models.EmulatorSaveConfig.LocalBase"/> are resolved against it,
        /// never against the process working directory.
        /// </summary>
        public static string FindLaunchBoxRoot(string startDirectory)
        {
            if (string.IsNullOrWhiteSpace(startDirectory))
                return null;

            try
            {
                var dir = new DirectoryInfo(Path.GetFullPath(startDirectory));
                string pluginsParent = null;

                while (dir != null)
                {
                    if (File.Exists(Path.Combine(dir.FullName, "LaunchBox.exe")))
                        return dir.FullName;

                    if (pluginsParent == null &&
                        string.Equals(dir.Name, "Plugins", StringComparison.OrdinalIgnoreCase))
                    {
                        pluginsParent = dir.Parent?.FullName;
                    }

                    dir = dir.Parent;
                }

                return pluginsParent ?? Path.GetFullPath(startDirectory);
            }
            catch
            {
                return null;
            }
        }

        public static bool IsUnder(string path, string root)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(root))
                return false;

            var prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                         + Path.DirectorySeparatorChar;

            return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        private static string SafeDirectoryName(string applicationPath)
        {
            if (string.IsNullOrWhiteSpace(applicationPath))
                return null;

            try { return Path.GetDirectoryName(applicationPath); }
            catch { return null; }
        }

        private static string SafeFullPath(params string[] parts)
        {
            try
            {
                var combined = parts[0] ?? string.Empty;
                for (var i = 1; i < parts.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(parts[i]))
                        continue;

                    combined = Path.Combine(combined, parts[i]);
                }

                if (string.IsNullOrWhiteSpace(combined))
                    return null;

                return Path.GetFullPath(combined);
            }
            catch
            {
                // Invalid characters, missing drive, etc.: the caller skips this path.
                return null;
            }
        }
    }
}
