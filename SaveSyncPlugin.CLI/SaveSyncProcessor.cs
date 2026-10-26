using System;
using System.IO;
using System.Linq;
using SaveSyncPlugin.Core.Models;
using SaveSyncPlugin.Core.Storage;
using SaveSyncPlugin.Core.Sync;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SaveSyncPlugin.CLI
{
    public class SaveSyncProcessor
    {
        public void Process(string action, string id, bool isEmulator = false)
        {
            var settings = SaveSyncStorage.LoadSettings();
            if (string.IsNullOrWhiteSpace(settings.ServerBasePath))
            {
                Console.Error.WriteLine("ServerBasePath not configured.");
                return;
            }

            if (isEmulator)
            {
                ProcessEmulator(action, id);
                return;
            }

            if (id == "--all" || id == "*")
            {
                ProcessAll(action);
                return;
            }

            var game = PluginHelper.DataManager.GetGameById(id)
                ?? PluginHelper.DataManager.GetAllGames()?.FirstOrDefault(g =>
                    g.Title != null && g.Title.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (game == null)
            {
                Console.Error.WriteLine($"Game not found: {id}");
                return;
            }

            var configs = SaveSyncStorage.LoadGameConfigs();
            var config = configs.Find(c => c.GameId == game.Id)
                ?? configs.Find(c => c.Title != null &&
                    c.Title.Equals(game.Title, StringComparison.OrdinalIgnoreCase));

            if (config == null)
            {
                Console.Error.WriteLine($"No save config for: {game.Title}");
                return;
            }

            ProcessGamePaths(action, game, config, settings);
        }

        private void ProcessEmulator(string action, string emulatorId)
        {
            var emulator = PluginHelper.DataManager.GetEmulatorById(emulatorId);
            if (emulator == null)
            {
                Console.Error.WriteLine($"Emulator not found: {emulatorId}");
                return;
            }

            var configs = SaveSyncStorage.LoadEmulatorConfigs();
            var config = configs.Find(c => c.EmulatorId == emulator.Id);
            if (config == null)
            {
                Console.Error.WriteLine($"No save config for emulator: {emulatorId}");
                return;
            }

            var settings = SaveSyncStorage.LoadSettings();
            var emuDir = Path.GetDirectoryName(emulator.ApplicationPath);
            if (emuDir == null) return;

            var localBase = emuDir;

            var serverBase = Path.Combine(settings.ServerBasePath, config.FolderName ?? config.Title ?? "Unknown");

            foreach (var rel in config.RelativePaths)
            {
                if (string.IsNullOrWhiteSpace(rel)) continue;
                var local = Path.Combine(localBase, rel);
                var server = Path.Combine(serverBase, rel);

                switch (action)
                {
                    case "upload": RobocopyCopy(local, server); break;
                    case "download": RobocopyCopy(server, local); break;
                    case "sync":
                        RobocopyCopy(server, local);
                        RobocopyCopy(local, server);
                        break;
                }
            }
        }

        private void ProcessAll(string action)
        {
            var service = new Services.SaveSyncService();
            var emuConfigs = SaveSyncStorage.LoadEmulatorConfigs();
            var gameConfigs = SaveSyncStorage.LoadGameConfigs();

            switch (action)
            {
                case "upload": service.UploadAll(emuConfigs, gameConfigs); break;
                case "download": service.DownloadAll(emuConfigs, gameConfigs); break;
                case "sync": service.SyncAll(emuConfigs, gameConfigs); break;
            }
        }

        private void ProcessGamePaths(string action, IGame game, GameSaveConfig config, SaveSyncSettings settings)
        {
            var serverBase = Path.Combine(settings.ServerBasePath, "WindowsGames",
                config.FolderName ?? config.Title ?? game.Title ?? "Unknown");

            string localBase = null;
            switch (config.SaveType?.ToLower())
            {
                case "relative":
                {
                    var gameFolder = Path.GetDirectoryName(game.ApplicationPath);
                    if (gameFolder == null) return;
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
                    return;
            }

            if (localBase == null) return;

            var paths = config.RelativePaths ?? new System.Collections.Generic.List<string>();
            if (paths.Count == 0)
            {
                RunAction(action, localBase, serverBase, null);
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
                        RunAction(action, localBase, serverBase, rel);
                    }
                    else
                    {
                        RunAction(action, Path.Combine(localBase, rel), Path.Combine(serverBase, rel), null);
                    }
                }
            }
        }

        private static void RunAction(string action, string source, string dest, string fileFilter)
        {
            switch (action)
            {
                case "upload": RobocopyCopy(source, dest, fileFilter); break;
                case "download": RobocopyCopy(dest, source, fileFilter); break;
                case "sync":
                    RobocopyCopy(dest, source, fileFilter);
                    RobocopyCopy(source, dest, fileFilter);
                    break;
            }
        }

        private static void RobocopyCopy(string source, string dest, string fileFilter = null)
        {
            // Same non-destructive runner used by the plugin: no /PURGE unless the
            // user enabled DeleteExtraneous in settings.json.
            RobocopyRunner.Run(source, dest, fileFilter, SaveSyncStorage.LoadSettings());
        }
    }
}
