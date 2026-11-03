using System;
using System.IO;

namespace SaveSyncPlugin.Core.Sync
{
    /// <summary>
    /// Resolves the local and server folders used by an emulator sync.
    ///
    /// Everything on the emulator side is relative on purpose: the whole tree
    /// (LaunchBox + emulators) can live under any path or drive letter, as long as
    /// the relative layout is preserved. Absolute paths are refused, because they
    /// break the install as soon as it is copied to another machine.
    ///
    /// Relative paths are always resolved against the LaunchBox root, never against
    /// the process working directory.
    /// </summary>
    public static class PathResolver
    {
        /// <summary>
        /// Local side. <paramref name="localBase"/> is relative to
        /// <paramref name="launchBoxRoot"/>; when it is empty the folder of the
        /// emulator application is used (the wrapper folder, for wrapped emulators).
        /// Returns <c>null</c> when the input is absolute, escapes the install or
        /// points inside the host folders (<c>Core</c> / <c>Plugins</c>).
        /// </summary>
        public static string ResolveLocal(
            string launchBoxRoot,
            string applicationPath,
            string localBase,
            string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath) || IsAbsolute(relativePath))
                return null;

            var baseDir = ResolveLocalBase(launchBoxRoot, applicationPath, localBase);
            if (baseDir == null)
                return null;

            var full = SafeFullPath(baseDir, relativePath);
            if (full == null)
                return null;

            var root = SafeFullPath(launchBoxRoot);
            return IsInsideHostFolders(full, root) ? null : full;
        }

        /// <summary>
        /// Absolute base folder of the local side, without the relative path applied.
        /// Empty <paramref name="localBase"/> means "the folder of the emulator
        /// application" (the default). Only relative input is accepted.
        /// </summary>
        public static string ResolveLocalBase(string launchBoxRoot, string applicationPath, string localBase)
        {
            var root = SafeFullPath(launchBoxRoot);
            if (root == null)
                return null;

            string baseDir;
            if (!string.IsNullOrWhiteSpace(localBase))
            {
                if (IsAbsolute(localBase))
                    return null;

                baseDir = localBase;
            }
            else
            {
                var appFolder = SafeDirectoryName(applicationPath);
                if (appFolder == null)
                    return null;

                // LaunchBox may store an absolute ApplicationPath. Normalise it to a
                // relative one when the folder lives under the LaunchBox tree, so no
                // absolute path ever reaches robocopy.
                if (IsAbsolute(appFolder))
                {
                    appFolder = Relativize(root, appFolder);
                    if (appFolder == null)
                        return null;
                }

                baseDir = appFolder;
            }

            var full = SafeFullPath(root, baseDir);
            if (full == null)
                return null;

            return IsInsideHostFolders(full, root) ? null : full;
        }

        /// <summary>
        /// Local base folder for a per-game sync. Unlike
        /// <see cref="ResolveLocalBase"/>, the optional <paramref name="localBase"/>
        /// is relative to the folder of the game executable, not to the LaunchBox
        /// root: portable games often keep the save next to the exe, and some keep
        /// it one level above it (Program\OutFox.exe with Save\ at the game root).
        /// Absolute input is refused, and so is anything inside the host folders.
        /// </summary>
        public static string ResolveGameLocalBase(string launchBoxRoot, string applicationPath, string localBase)
        {
            var exeFolder = ResolveLocalBase(launchBoxRoot, applicationPath, null);
            if (exeFolder == null)
                return null;

            if (string.IsNullOrWhiteSpace(localBase))
                return exeFolder;

            if (IsAbsolute(localBase))
                return null;

            var full = SafeFullPath(exeFolder, localBase);
            if (full == null)
                return null;

            var root = SafeFullPath(launchBoxRoot);
            return root != null && IsInsideHostFolders(full, root) ? null : full;
        }

        /// <summary>
        /// Server side: always under <paramref name="serverBasePath"/>. A path that
        /// escapes it (for example the old <c>..\..\..\Emuladores\...</c> entry) is
        /// refused instead of silently writing outside the backup folder.
        ///
        /// The server base itself may be absolute (typical: another drive) or
        /// relative. A relative one is resolved against the LaunchBox folder, so it
        /// never depends on the process working directory.
        /// </summary>
        public static string ResolveServer(string serverBasePath, string folderName, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(serverBasePath) || string.IsNullOrWhiteSpace(relativePath))
                return null;

            if (IsAbsolute(relativePath))
                return null;

            var basePath = serverBasePath;
            if (!IsAbsolute(basePath))
            {
                var launchBoxRoot = PluginPaths.LaunchBoxRoot;
                if (string.IsNullOrWhiteSpace(launchBoxRoot))
                    return null;

                basePath = Path.Combine(launchBoxRoot, basePath);
            }

            var root = SafeFullPath(basePath);
            if (root == null)
                return null;

            var full = SafeFullPath(basePath, folderName, relativePath);
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
        /// True when the emulator is wrapped and no local base was configured: the
        /// relative paths would then start at the wrapper folder, so the sync has to
        /// be skipped instead of copying the wrong tree.
        /// </summary>
        public static bool ShouldSkipWrapperSync(string applicationPath, string localBase, int relativePathCount)
        {
            return relativePathCount > 0
                   && UsesWrapper(applicationPath)
                   && string.IsNullOrWhiteSpace(localBase);
        }

        /// <summary>
        /// Finds the LaunchBox folder from any directory inside the install. Kept for
        /// compatibility; the real logic lives in <see cref="PluginPaths.FindRoot"/>.
        /// </summary>
        public static string FindLaunchBoxRoot(string startDirectory)
        {
            return PluginPaths.FindRoot(startDirectory);
        }

        /// <summary>
        /// Converts an absolute folder into a path relative to
        /// <paramref name="launchBoxRoot"/>. Returns <c>null</c> when that is not
        /// possible (different drive) so callers can refuse instead of storing an
        /// absolute path.
        /// </summary>
        public static string Relativize(string launchBoxRoot, string absolutePath)
        {
            var root = SafeFullPath(launchBoxRoot);
            var full = SafeFullPath(absolutePath);
            if (root == null || full == null)
                return null;

            try
            {
                var relative = Path.GetRelativePath(root, full);
                if (string.IsNullOrWhiteSpace(relative) || IsAbsolute(relative))
                    return null;

                return relative == "." ? null : relative;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>True for drive-qualified (<c>C:\x</c>), UNC (<c>\\srv\x</c>) or root-relative (<c>\x</c>) paths.</summary>
        public static bool IsAbsolute(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try { return Path.IsPathRooted(path); }
            catch { return true; }
        }

        public static bool IsUnder(string path, string root)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(root))
                return false;

            var prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                         + Path.DirectorySeparatorChar;

            return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>True when the path is <paramref name="root"/> itself or inside it.</summary>
        public static bool IsSameOrUnder(string path, string root)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(root))
                return false;

            var trimmedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var trimmedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return string.Equals(trimmedPath, trimmedRoot, StringComparison.OrdinalIgnoreCase)
                   || IsUnder(trimmedPath, trimmedRoot);
        }

        /// <summary>True when the path would land inside the LaunchBox host folders.</summary>
        public static bool IsInsideHostFolders(string path, string launchBoxRoot)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(launchBoxRoot))
                return false;

            return IsSameOrUnder(path, Path.Combine(launchBoxRoot, PluginPaths.CoreFolderName))
                   || IsSameOrUnder(path, Path.Combine(launchBoxRoot, PluginPaths.PluginsFolderName));
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
