using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SaveSyncPlugin.Core.Models;
using SaveSyncPlugin.Core.Storage;
using SaveSyncPlugin.Core.Sync;
using SaveSyncPlugin.UI.Forms;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SaveSyncPlugin.Services
{
    public class SaveSyncService
    {
        public void DownloadAll(List<EmulatorSaveConfig> emuConfigs, List<GameSaveConfig> gameConfigs,
            IProgress<string> progress = null)
        {
            foreach (var c in emuConfigs)
            {
                progress?.Report($"Downloading emulator: {c.Title ?? c.FolderName}");
                DownloadEmulator(c, progress);
            }
            foreach (var c in gameConfigs)
            {
                progress?.Report($"Downloading game: {c.Title ?? c.FolderName}");
                DownloadGame(c, progress);
            }
        }

        public void UploadAll(List<EmulatorSaveConfig> emuConfigs, List<GameSaveConfig> gameConfigs,
            IProgress<string> progress = null)
        {
            foreach (var c in emuConfigs)
            {
                progress?.Report($"Uploading emulator: {c.Title ?? c.FolderName}");
                UploadEmulator(c, progress);
            }
            foreach (var c in gameConfigs)
            {
                progress?.Report($"Uploading game: {c.Title ?? c.FolderName}");
                UploadGame(c, progress);
            }
        }

        public void SyncAll(List<EmulatorSaveConfig> emuConfigs, List<GameSaveConfig> gameConfigs,
            IProgress<string> progress = null)
        {
            foreach (var c in emuConfigs)
            {
                progress?.Report($"Syncing emulator: {c.Title ?? c.FolderName}");
                SyncEmulator(c, progress);
            }
            foreach (var c in gameConfigs)
            {
                progress?.Report($"Syncing game: {c.Title ?? c.FolderName}");
                SyncGame(c, progress);
            }
        }

        public void DownloadEmulator(EmulatorSaveConfig config, IProgress<string> progress = null)
        {
            var paths = GetEmulatorSyncPaths(config);
            if (paths == null) return;
            foreach (var p in paths)
                RobocopyCopy(p.Server, p.Local, null, progress);
        }

        public void UploadEmulator(EmulatorSaveConfig config, IProgress<string> progress = null)
        {
            var paths = GetEmulatorSyncPaths(config);
            if (paths == null) return;
            foreach (var p in paths)
                RobocopyCopy(p.Local, p.Server, null, progress);
        }

        public void SyncEmulator(EmulatorSaveConfig config, IProgress<string> progress = null)
        {
            var paths = GetEmulatorSyncPaths(config);
            if (paths == null) return;
            foreach (var p in paths)
            {
                RobocopyCopy(p.Server, p.Local, null, progress);
                RobocopyCopy(p.Local, p.Server, null, progress);
            }
        }

        public void DownloadGame(GameSaveConfig config, IProgress<string> progress = null)
        {
            var entries = GetGameSyncPaths(config);
            if (entries == null) return;
            foreach (var e in entries)
                RobocopyCopy(e.Dest, e.Source, e.FileFilter, progress);
        }

        public void UploadGame(GameSaveConfig config, IProgress<string> progress = null)
        {
            var entries = GetGameSyncPaths(config);
            if (entries == null) return;
            foreach (var e in entries)
                RobocopyCopy(e.Source, e.Dest, e.FileFilter, progress);
        }

        public void SyncGame(GameSaveConfig config, IProgress<string> progress = null)
        {
            var entries = GetGameSyncPaths(config);
            if (entries == null) return;
            foreach (var e in entries)
            {
                RobocopyCopy(e.Dest, e.Source, e.FileFilter, progress);
                RobocopyCopy(e.Source, e.Dest, e.FileFilter, progress);
            }
        }

        public void SyncByGame(IGame game, IProgress<string> progress = null)
        {
            var configs = SaveSyncStorage.LoadGameConfigs();
            var config = configs.Find(c => c.GameId == game.Id)
                ?? configs.Find(c =>
                    c.Title != null &&
                    c.Title.Equals(game.Title, StringComparison.OrdinalIgnoreCase));
            if (config != null)
            {
                progress?.Report($"Syncing game: {config.Title ?? config.FolderName}");
                SyncGame(config);
            }
        }

        public void SyncByEmulator(IEmulator emulator, IProgress<string> progress = null)
        {
            var configs = SaveSyncStorage.LoadEmulatorConfigs();
            var config = configs.Find(c => c.EmulatorId == emulator.Id);
            if (config != null)
            {
                progress?.Report($"Syncing emulator: {config.Title ?? config.FolderName}");
                SyncEmulator(config, progress);
            }
        }

        private List<(string Local, string Server)> GetEmulatorSyncPaths(EmulatorSaveConfig config)
        {
            var emulator = PluginHelper.DataManager.GetEmulatorById(config.EmulatorId);
            if (emulator?.ApplicationPath == null) return null;

            var settings = SaveSyncStorage.LoadSettings();
            var folderName = config.FolderName ?? config.Title ?? "Unknown";

            // The local base is the emulator directory when the user configured one.
            // Without it the paths start at the application folder — for a wrapped
            // emulator that is the wrapper folder, which is why paths used to escape
            // with "..\..\.." and the server side ended up outside the backup folder.
            if (PathResolver.UsesWrapper(emulator.ApplicationPath) && string.IsNullOrWhiteSpace(config.LocalBase))
            {
                SyncLog.Write(
                    $"WARN    {config.Title ?? folderName}: emulator app is a wrapper ({emulator.ApplicationPath}) " +
                    "and LocalBase is empty; relative paths start at the wrapper folder");
            }

            var launchBoxRoot = PathResolver.FindLaunchBoxRoot(AppDomain.CurrentDomain.BaseDirectory)
                                ?? AppDomain.CurrentDomain.BaseDirectory;

            var paths = new List<(string, string)>();
            foreach (var rel in config.RelativePaths)
            {
                if (string.IsNullOrWhiteSpace(rel)) continue;

                var local = PathResolver.ResolveLocal(launchBoxRoot, emulator.ApplicationPath, config.LocalBase, rel);
                var server = PathResolver.ResolveServer(settings.ServerBasePath, folderName, rel);

                if (local == null)
                {
                    SyncLog.Write($"SKIP    {folderName}: caminho local invalido para \"{rel}\"");
                    continue;
                }

                if (server == null)
                {
                    SyncLog.Write(
                        $"SKIP    {folderName}: caminho do servidor invalido ou fora de \"{settings.ServerBasePath}\" para \"{rel}\"");
                    continue;
                }

                paths.Add((local, server));
            }
            return paths;
        }

        private class SyncEntry
        {
            public string Source { get; set; }
            public string Dest { get; set; }
            public string FileFilter { get; set; }
        }

        private List<SyncEntry> GetGameSyncPaths(GameSaveConfig config)
        {
            var game = FindGame(config);
            if (game == null) return null;

            var settings = SaveSyncStorage.LoadSettings();
            var serverBase = Path.Combine(settings.ServerBasePath ?? "", "WindowsGames",
                config.FolderName ?? config.Title ?? game.Title ?? "Unknown");

            string localBase = null;
            switch (config.SaveType?.ToLower())
            {
                case "relative":
                {
                    var gameFolder = Path.GetDirectoryName(game.ApplicationPath);
                    if (gameFolder == null) return null;
                    localBase = gameFolder;
                    break;
                }
                case "appdata":
                    localBase = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;
                case "userprofile":
                    localBase = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    break;
                case "public":
                    localBase = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                    break;
                default:
                    return null;
            }

            if (localBase == null) return null;

            var results = new List<SyncEntry>();
            var paths = config.RelativePaths ?? new List<string>();

            if (paths.Count == 0)
            {
                results.Add(new SyncEntry { Source = localBase, Dest = serverBase });
            }
            else
            {
                foreach (var rel in paths)
                {
                    if (string.IsNullOrWhiteSpace(rel)) continue;

                    var fileName = Path.GetFileName(rel);
                    bool isFile = fileName.IndexOf('.') >= 0;

                    if (isFile)
                    {
                        results.Add(new SyncEntry
                        {
                            Source = localBase,
                            Dest = serverBase,
                            FileFilter = rel
                        });
                    }
                    else
                    {
                        results.Add(new SyncEntry
                        {
                            Source = Path.Combine(localBase, rel),
                            Dest = Path.Combine(serverBase, rel)
                        });
                    }
                }
            }

            return results;
        }

        private IGame FindGame(GameSaveConfig config)
        {
            if (!string.IsNullOrEmpty(config.GameId))
            {
                var game = PluginHelper.DataManager.GetGameById(config.GameId);
                if (game != null) return game;
            }

            var allGames = PluginHelper.DataManager.GetAllGames();
            return allGames?.FirstOrDefault(g =>
                g.Title != null &&
                g.Title.Equals(config.Title, StringComparison.OrdinalIgnoreCase));
        }

        internal FileConflict DetectConflict(string source, string dest, string fileFilter)
        {
            try
            {
                if (!Directory.Exists(source) || !Directory.Exists(dest))
                    return null;

                var sourceFiles = Directory.GetFiles(source, string.IsNullOrEmpty(fileFilter) ? "*.*" : fileFilter, SearchOption.AllDirectories);
                var destFiles = Directory.GetFiles(dest, string.IsNullOrEmpty(fileFilter) ? "*.*" : fileFilter, SearchOption.AllDirectories);

                foreach (var srcFile in sourceFiles)
                {
                    var relativePath = srcFile.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    var destFile = Path.Combine(dest, relativePath);

                    if (File.Exists(destFile))
                    {
                        var srcInfo = new FileInfo(srcFile);
                        var destInfo = new FileInfo(destFile);

                        if (srcInfo.LastWriteTime != destInfo.LastWriteTime)
                        {
                            return new FileConflict
                            {
                                LocalPath = srcFile,
                                ServerPath = destFile,
                                LocalModified = srcInfo.LastWriteTime,
                                ServerModified = destInfo.LastWriteTime,
                                LocalSize = srcInfo.Length,
                                ServerSize = destInfo.Length
                            };
                        }
                    }
                }

                foreach (var dstFile in destFiles)
                {
                    var relativePath = dstFile.Substring(dest.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    var srcFile = Path.Combine(source, relativePath);

                    if (!File.Exists(srcFile))
                    {
                        var dstInfo = new FileInfo(dstFile);
                        return new FileConflict
                        {
                            LocalPath = srcFile,
                            ServerPath = dstFile,
                            LocalModified = DateTime.MinValue,
                            ServerModified = dstInfo.LastWriteTime,
                            LocalSize = 0,
                            ServerSize = dstInfo.Length
                        };
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private void RobocopyCopy(string source, string dest, string fileFilter = null,
            IProgress<string> progress = null)
        {
            try
            {
                if (!Directory.Exists(source)) return;
                Directory.CreateDirectory(dest);

                var settings = SaveSyncStorage.LoadSettings();
                var conflict = DetectConflict(source, dest, fileFilter);

                if (conflict == null)
                {
                    // No conflict: normal direction (source -> dest). /XO keeps the
                    // newest file and, without DeleteExtraneous, nothing is deleted.
                    RobocopyRunner.Run(source, dest, fileFilter, settings, progress);
                    return;
                }

                // Conflict: AlwaysKeepNewer/AskOnConflict decide only the DIRECTION
                // (who wins). Whether a file may be deleted is decided solely by
                // DeleteExtraneous, inside RobocopyRunner - it never adds /PURGE here.
                bool sourceIsNewer;
                if (settings.AlwaysKeepNewer)
                {
                    sourceIsNewer = conflict.LocalIsNewer;
                }
                else if (settings.AskOnConflict)
                {
                    sourceIsNewer = ShowConflictDialog(conflict) == DialogResult.Yes;
                }
                else
                {
                    // Neither option enabled: keep the newest copy (safe default).
                    sourceIsNewer = conflict.LocalIsNewer;
                }

                SyncLog.Write(
                    $"CONFLICT {conflict.LocalPath ?? "(missing)"} | local={conflict.LocalModified:yyyy-MM-dd HH:mm:ss} " +
                    $"server={conflict.ServerModified:yyyy-MM-dd HH:mm:ss} | winner={(sourceIsNewer ? "source" : "dest")}");

                if (sourceIsNewer)
                    RobocopyRunner.Run(source, dest, fileFilter, settings, progress);
                else
                    RobocopyRunner.Run(dest, source, fileFilter, settings, progress);
            }
            catch (Exception ex)
            {
                SyncLog.Write($"FAIL    robocopy: {ex.Message}");
            }
        }

        private DialogResult ShowConflictDialog(FileConflict conflict)
        {
            DialogResult result = DialogResult.Yes;

            if (Application.MessageLoop)
            {
                using (var form = new ConflictForm(conflict))
                {
                    result = form.ShowDialog();
                    if (form.UseLocal)
                        result = DialogResult.Yes;
                    else
                        result = DialogResult.No;
                }
            }

            return result;
        }
    }
}
