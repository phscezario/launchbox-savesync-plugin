using System;
using System.IO;

namespace SaveSyncPlugin.Core.Sync
{
    /// <summary>
    /// Minimal append-only log for sync operations, written next to the plugin
    /// DLLs (<c>sync.log</c>). Robocopy output is otherwise invisible: the plugin
    /// only used <c>Debug.WriteLine</c>, which nobody sees in a release build.
    ///
    /// The first line of every process is the <c>ROOT</c> line from
    /// <see cref="PluginPaths.Describe"/>, so a wrong install root can be spotted
    /// without guessing.
    /// </summary>
    public static class SyncLog
    {
        private const long MaxBytes = 2 * 1024 * 1024;

        private static readonly object Gate = new object();
        private static bool _headerWritten;

        public static string PluginDirectory => PluginPaths.PluginFolder;

        public static string FilePath
        {
            get
            {
                var dir = PluginDirectory;
                return string.IsNullOrWhiteSpace(dir) ? null : Path.Combine(dir, "sync.log");
            }
        }

        public static void Write(string message)
        {
            try
            {
                lock (Gate)
                {
                    var directory = PluginDirectory;
                    if (string.IsNullOrWhiteSpace(directory))
                        return;

                    Directory.CreateDirectory(directory);

                    if (!_headerWritten)
                    {
                        // Set before writing so a failure here cannot loop forever.
                        _headerWritten = true;
                        Append(directory, PluginPaths.Describe());
                    }

                    Append(directory, message);
                }
            }
            catch
            {
                // Logging must never break a sync.
            }
        }

        private static void Append(string directory, string message)
        {
            var file = Path.Combine(directory, "sync.log");
            var info = new FileInfo(file);
            if (info.Exists && info.Length > MaxBytes)
            {
                var previous = Path.Combine(directory, "sync.prev.log");
                try { File.Copy(file, previous, true); } catch { }
                try { File.WriteAllText(file, string.Empty); } catch { }
            }

            File.AppendAllText(file, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
        }
    }
}
