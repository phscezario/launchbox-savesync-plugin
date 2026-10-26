using System;
using System.IO;

namespace SaveSyncPlugin.Core.Sync
{
    /// <summary>
    /// Minimal append-only log for sync operations, written next to the plugin
    /// DLLs (<c>sync.log</c>). Robocopy output is otherwise invisible: the plugin
    /// only used <c>Debug.WriteLine</c>, which nobody sees in a release build.
    /// </summary>
    public static class SyncLog
    {
        private const string PluginFolderName = "SaveSync LaunchBox Integration";
        private const long MaxBytes = 2 * 1024 * 1024;

        private static readonly object Gate = new object();

        public static string FilePath => Path.Combine(PluginDirectory, "sync.log");

        private static string PluginDirectory
        {
            get
            {
                // Inside LaunchBox, BaseDirectory is the LaunchBox folder.
                // Running the CLI, BaseDirectory is already the plugin folder.
                var baseDir = AppDomain.CurrentDomain.BaseDirectory
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (string.Equals(Path.GetFileName(baseDir), PluginFolderName, StringComparison.OrdinalIgnoreCase))
                    return baseDir;

                return Path.Combine(baseDir, "Plugins", PluginFolderName);
            }
        }

        public static void Write(string message)
        {
            try
            {
                lock (Gate)
                {
                    Directory.CreateDirectory(PluginDirectory);

                    var file = FilePath;
                    var info = new FileInfo(file);
                    if (info.Exists && info.Length > MaxBytes)
                    {
                        var previous = Path.Combine(PluginDirectory, "sync.prev.log");
                        try { File.Copy(file, previous, true); } catch { }
                        try { File.WriteAllText(file, string.Empty); } catch { }
                    }

                    File.AppendAllText(file, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
                }
            }
            catch
            {
                // Logging must never break a sync.
            }
        }
    }
}
