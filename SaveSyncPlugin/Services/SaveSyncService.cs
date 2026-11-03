using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using SaveSyncPlugin.Core.Models;
using SaveSyncPlugin.Core.Storage;
using SaveSyncPlugin.Core.Sync;
using SaveSyncPlugin.UI.Forms;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SaveSyncPlugin.Services
{
    /// <summary>
    /// Which way a copy is allowed to write. A conflict used to be resolved by
    /// "newest wins" no matter who asked for the copy, so a startup download could
    /// silently upload a newer local file. Download/upload now only ever write in
    /// their own direction; <see cref="Both"/> is the manual/bidirectional sync and
    /// keeps the newest copy on either side.
    /// </summary>
    internal enum SyncDirection
    {
        Download,
        Upload,
        Both
    }

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
                RobocopyCopy(p.Server, p.Local, null, progress, SyncDirection.Download);
        }

        public void UploadEmulator(EmulatorSaveConfig config, IProgress<string> progress = null)
        {
            var paths = GetEmulatorSyncPaths(config);
            if (paths == null) return;
            foreach (var p in paths)
                RobocopyCopy(p.Local, p.Server, null, progress, SyncDirection.Upload);
        }

        public void SyncEmulator(EmulatorSaveConfig config, IProgress<string> progress = null)
        {
            var paths = GetEmulatorSyncPaths(config);
            if (paths == null) return;
            foreach (var p in paths)
            {
                RobocopyCopy(p.Server, p.Local, null, progress, SyncDirection.Both);
                RobocopyCopy(p.Local, p.Server, null, progress, SyncDirection.Both);
            }
        }

        public void DownloadGame(GameSaveConfig config, IProgress<string> progress = null)
        {
            var entries = GetGameSyncPaths(config);
            if (entries == null) return;
            foreach (var e in entries)
                RobocopyCopy(e.Dest, e.Source, e.FileFilter, progress, SyncDirection.Download);
        }

        public void UploadGame(GameSaveConfig config, IProgress<string> progress = null)
        {
            var entries = GetGameSyncPaths(config);
            if (entries == null) return;
            foreach (var e in entries)
                RobocopyCopy(e.Source, e.Dest, e.FileFilter, progress, SyncDirection.Upload);
        }

        public void SyncGame(GameSaveConfig config, IProgress<string> progress = null)
        {
            var entries = GetGameSyncPaths(config);
            if (entries == null) return;
            foreach (var e in entries)
            {
                RobocopyCopy(e.Dest, e.Source, e.FileFilter, progress, SyncDirection.Both);
                RobocopyCopy(e.Source, e.Dest, e.FileFilter, progress, SyncDirection.Both);
            }
        }

        public void SyncByGame(IGame game, IProgress<string> progress = null)
        {
            if (game == null) return;

            var configs = SaveSyncStorage.LoadGameConfigs();
            var config = configs.Find(c => !string.IsNullOrEmpty(c.GameId) && c.GameId == game.Id);

            if (config == null)
            {
                // Same tolerant lookup as FindGame: the config may use an older title
                // than the library ("TMNT - ..." vs "Teenage Mutant Ninja Turtles: ...").
                var titles = new List<string>(configs.Count);
                foreach (var c in configs)
                    titles.Add(c.Title ?? string.Empty);

                var index = TitleMatcher.FindUniqueMatch(game.Title, titles, out var exact);
                if (index >= 0)
                {
                    config = configs[index];
                    if (!exact)
                        SyncLog.Write($"MATCH   {game.Title} -> {config.Title} (aproximado)");
                }
            }

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
            var title = config.Title ?? folderName;
            var relativePaths = config.RelativePaths ?? new List<string>();

            // The root comes from the plugin assembly location (PluginPaths), never
            // from AppDomain.BaseDirectory: in LaunchBox 14 that is <root>\Core, and
            // "..\Emuladores" then resolved to <root>\Emuladores (the v0.0.3 bug).
            var launchBoxRoot = PluginPaths.LaunchBoxRoot;
            if (string.IsNullOrWhiteSpace(launchBoxRoot))
            {
                SyncLog.Write($"SKIP    {title}: raiz do LaunchBox nao encontrada (base={PluginPaths.BaseDirectory})");
                return new List<(string, string)>();
            }

            // A wrapper executable lives in Apps\EmulatorExtensions\..., not in the
            // emulator folder. Syncing the wrapper folder would copy the wrong tree,
            // so the emulator directory has to be configured explicitly.
            if (PathResolver.ShouldSkipWrapperSync(
                    emulator.ApplicationPath, config.LocalBase, relativePaths.Count))
            {
                SyncLog.Write(
                    $"SKIP    {title}: emulator app is a wrapper ({emulator.ApplicationPath}) " +
                    "and LocalBase is empty; set the emulator directory (relative) in the plugin settings");
                return new List<(string, string)>();
            }

            var localBaseError = SaveSyncStorage.ValidateLocalBase(config.LocalBase);
            if (localBaseError != null)
            {
                SyncLog.Write($"SKIP    {title}: {localBaseError}");
                return new List<(string, string)>();
            }

            var mode = string.IsNullOrWhiteSpace(config.LocalBase) ? "padrao" : "relativo";

            var paths = new List<(string, string)>();
            foreach (var rel in relativePaths)
            {
                if (string.IsNullOrWhiteSpace(rel)) continue;

                var local = PathResolver.ResolveLocal(launchBoxRoot, emulator.ApplicationPath, config.LocalBase, rel);
                var server = PathResolver.ResolveServer(settings.ServerBasePath, folderName, rel);

                if (local == null)
                {
                    SyncLog.Write(
                        $"SKIP    {title}: caminho local invalido para \"{rel}\" " +
                        $"(raiz={launchBoxRoot}, base={config.LocalBase ?? "(pasta do emulador)"})");
                    continue;
                }

                if (server == null)
                {
                    SyncLog.Write(
                        $"SKIP    {title}: caminho do servidor invalido ou fora de \"{settings.ServerBasePath}\" para \"{rel}\"");
                    continue;
                }

                SyncLog.Write(mode == "relativo"
                    ? $"PATH    {title} | modo=relativo | base={config.LocalBase} | local={local} | server={server}"
                    : $"PATH    {title} | modo=padrao | local={local} | server={server}");

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
            if (game == null)
            {
                // The config points at a title that is not in the library: the sync
                // used to disappear without a trace, which looked like a silent bug.
                SyncLog.Write(
                    $"SKIP    {config.Title ?? config.FolderName}: jogo nao encontrado na biblioteca");
                return null;
            }

            var settings = SaveSyncStorage.LoadSettings();
            var folderName = config.FolderName ?? config.Title ?? game.Title ?? "Unknown";
            var title = config.Title ?? folderName;

            // Used to be Path.Combine(ServerBasePath ?? "", ...): with an unset server
            // path that produced a RELATIVE destination and robocopy wrote it under
            // whatever the process working directory happened to be.
            var serverBase = PathResolver.ResolveServer(settings.ServerBasePath, "WindowsGames", folderName);
            if (serverBase == null)
            {
                SyncLog.Write(
                    $"SKIP    {title}: ServerBasePath nao configurado ou caminho invalido \"{settings.ServerBasePath}\"");
                return new List<SyncEntry>();
            }

            string localBase;
            switch (config.SaveType?.ToLower())
            {
                case "relative":
                {
                    // ApplicationPath may be relative ("..\Roms\...") and used to be
                    // passed to robocopy as-is: it only worked because the working
                    // directory happened to be the LaunchBox folder. An optional
                    // LocalBase moves the start one level away from the exe folder
                    // (portable games whose save lives at the game root).
                    localBase = PathResolver.ResolveGameLocalBase(
                        PluginPaths.LaunchBoxRoot, game.ApplicationPath, config.LocalBase);
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

            if (string.IsNullOrWhiteSpace(localBase))
            {
                SyncLog.Write($"SKIP    {title}: pasta local do jogo nao resolvida (app={game.ApplicationPath})");
                return new List<SyncEntry>();
            }

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

                    if (PathResolver.IsAbsolute(rel))
                    {
                        SyncLog.Write($"SKIP    {title}: \"{rel}\" e um caminho absoluto; use caminho relativo");
                        continue;
                    }

                    var fileName = Path.GetFileName(rel);
                    var dirPart = Path.GetDirectoryName(rel) ?? string.Empty;

                    // File or folder? The old rule ("has a dot in the name") failed
                    // for saves without an extension (gamestate, nglist, profile) and
                    // for folders that contain a dot. Existence wins; the dot rule is
                    // only the fallback for a path that does not exist yet.
                    var localFull = Path.Combine(localBase, rel);
                    var serverFull = PathResolver.ResolveServer(serverBase, null, rel);

                    bool isFile;
                    if (Directory.Exists(localFull) ||
                        (serverFull != null && Directory.Exists(serverFull)))
                        isFile = false;
                    else if (File.Exists(localFull) ||
                             (serverFull != null && File.Exists(serverFull)))
                        isFile = true;
                    else
                        isFile = fileName.IndexOf('.') >= 0 ||
                                 fileName.IndexOf('*') >= 0 ||
                                 fileName.IndexOf('?') >= 0;

                    if (isFile)
                    {
                        // Split the entry so robocopy gets a folder plus a file name:
                        // "data\gamestate" becomes source <local>\data + filter "gamestate".
                        var sourceDir = string.IsNullOrEmpty(dirPart)
                            ? localBase
                            : Path.Combine(localBase, dirPart);
                        var destDir = string.IsNullOrEmpty(dirPart)
                            ? serverBase
                            : PathResolver.ResolveServer(serverBase, null, dirPart);

                        if (destDir == null)
                        {
                            SyncLog.Write($"SKIP    {title}: caminho do servidor invalido para \"{rel}\"");
                            continue;
                        }

                        results.Add(new SyncEntry
                        {
                            Source = sourceDir,
                            Dest = destDir,
                            FileFilter = fileName
                        });
                    }
                    else
                    {
                        if (serverFull == null)
                        {
                            SyncLog.Write($"SKIP    {title}: caminho do servidor invalido para \"{rel}\"");
                            continue;
                        }

                        results.Add(new SyncEntry
                        {
                            Source = localFull,
                            Dest = serverFull
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
            if (allGames == null) return null;

            var list = new List<IGame>(allGames);
            var titles = new List<string>(list.Count);
            foreach (var g in list)
                titles.Add(g.Title ?? string.Empty);

            // Exact match first (same as before). When that fails, TitleMatcher
            // accepts one unambiguous "close enough" title: LaunchBox/RomM rename
            // games ("TMNT - ..." became "Teenage Mutant Ninja Turtles: ...") and the
            // old exact-only lookup silently stopped syncing those saves.
            var index = TitleMatcher.FindUniqueMatch(config.Title, titles, out var exact);
            if (index < 0) return null;

            if (!exact)
                SyncLog.Write($"MATCH   {config.Title} -> {list[index].Title} (aproximado)");

            return list[index];
        }

        internal FileConflict DetectConflict(string source, string dest, string fileFilter)
        {
            try
            {
                if (!Directory.Exists(source) || !Directory.Exists(dest))
                    return null;

                var sourceFiles = Directory.GetFiles(source, string.IsNullOrEmpty(fileFilter) ? "*.*" : fileFilter, SearchOption.AllDirectories);

                foreach (var srcFile in sourceFiles)
                {
                    var relativePath = srcFile.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    var destFile = Path.Combine(dest, relativePath);

                    // A file that exists on only one side is not a conflict: robocopy
                    // copies it in whatever direction was requested. Reporting it as a
                    // conflict is what let a startup download push local-only files up.
                    if (!File.Exists(destFile))
                        continue;

                    var srcInfo = new FileInfo(srcFile);
                    var destInfo = new FileInfo(destFile);

                    // Size is checked first (cheap). Equal sizes still need a hash:
                    // "same size and timestamp, different bytes" is the case robocopy
                    // /XO silently ignores, and treating it as identical would leave
                    // the two sides diverged with no log line at all.
                    if (SameContent(srcInfo, destInfo))
                        continue;

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
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// True when both files hold the same bytes. Size is checked first, so a
        /// different length never pays for a hash. Unreadable files return false and
        /// fall through to robocopy instead of hiding the problem.
        /// </summary>
        private static bool SameContent(FileInfo a, FileInfo b)
        {
            if (a.Length != b.Length)
                return false;

            try
            {
                using (var sha = SHA256.Create())
                using (var streamA = a.OpenRead())
                using (var streamB = b.OpenRead())
                {
                    var hashA = sha.ComputeHash(streamA);
                    var hashB = sha.ComputeHash(streamB);
                    return hashA.SequenceEqual(hashB);
                }
            }
            catch
            {
                return false;
            }
        }

        private void RobocopyCopy(string source, string dest, string fileFilter = null,
            IProgress<string> progress = null, SyncDirection direction = SyncDirection.Both)
        {
            try
            {
                // A sync entry may point at a single file (Xemu's xbox_hdd.qcow2 is
                // configured as "files\xbox_hdd.qcow2"). Robocopy always works with a
                // folder plus a filter, so the file is split into its parent folder
                // and its name. Without this the entry was silently ignored, because
                // Directory.Exists() is false for a file.
                var sourceDir = source;
                var destDir = dest;
                var filter = fileFilter;

                if (File.Exists(source))
                {
                    filter = Path.GetFileName(source);
                    sourceDir = Path.GetDirectoryName(source);
                    destDir = Path.GetDirectoryName(dest);
                }

                if (string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir))
                {
                    // Used to return silently, which hid every misconfigured path.
                    SyncLog.Write($"SKIP    origem nao existe: {source}");
                    return;
                }

                if (string.IsNullOrEmpty(destDir))
                {
                    SyncLog.Write($"SKIP    destino invalido: {dest}");
                    return;
                }

                Directory.CreateDirectory(destDir);

                var settings = SaveSyncStorage.LoadSettings();
                var conflict = DetectConflict(sourceDir, destDir, filter);

                if (conflict == null)
                {
                    // No conflict: normal direction (source -> dest). /XO keeps the
                    // newest file and, without DeleteExtraneous, nothing is deleted.
                    RobocopyRunner.Run(sourceDir, destDir, filter, settings, progress);
                    return;
                }

                // Conflict: AlwaysKeepNewer/AskOnConflict decide only the DIRECTION
                // (who wins). Whether a file may be deleted is decided solely by
                // DeleteExtraneous, inside RobocopyRunner - it never adds /PURGE here.
                //
                // AlwaysKeepNewer wins over AskOnConflict: the startup sync runs in a
                // background task, and a dialog there would block before the user is
                // even at the machine.
                bool sourceIsNewer;
                if (settings.AlwaysKeepNewer)
                {
                    sourceIsNewer = conflict.LocalIsNewer;
                }
                else if (settings.AskOnConflict && direction == SyncDirection.Both)
                {
                    sourceIsNewer = ShowConflictDialog(conflict) == DialogResult.Yes;
                }
                else
                {
                    // Neither option enabled: keep the newest copy (safe default).
                    sourceIsNewer = conflict.LocalIsNewer;
                }

                // The newest copy always wins. What the direction changes is whether
                // the other side may be written at all: a download never pushes a
                // local file up, an upload never pulls a server file down. Only the
                // manual/both sync may write in the opposite direction.
                var action = sourceIsNewer
                    ? "copiar origem -> destino"
                    : direction == SyncDirection.Both
                        ? "copiar destino -> origem (mais novo)"
                        : "manter destino, que e mais novo";

                SyncLog.Write(
                    $"CONFLICT {conflict.LocalPath ?? "(missing)"} | origem={conflict.LocalModified:yyyy-MM-dd HH:mm:ss} " +
                    $"destino={conflict.ServerModified:yyyy-MM-dd HH:mm:ss} | {action}");

                if (sourceIsNewer)
                    RobocopyRunner.Run(sourceDir, destDir, filter, settings, progress);
                else if (direction == SyncDirection.Both)
                    RobocopyRunner.Run(destDir, sourceDir, filter, settings, progress);
                else
                    SyncLog.Write(
                        $"SKIP    {direction}: destino mais novo, nao copia no sentido inverso | {destDir}");
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
