using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SaveSyncPlugin.Core.Models;
using SaveSyncPlugin.Core.Sync;

namespace SaveSyncPlugin.Core.Storage
{
    public static class SaveSyncStorage
    {
        /// <summary>
        /// Folder that holds settings.json / emulators.json / games.json. Comes from
        /// <see cref="PluginPaths.PluginFolder"/> (assembly location), never from
        /// <c>AppDomain.BaseDirectory</c>: the LaunchBox 14 host lives in
        /// <c>&lt;root&gt;\Core</c>, so BaseDirectory would point at
        /// <c>&lt;root&gt;\Core\Plugins\...</c>.
        /// </summary>
        public static string PluginFolder => PluginPaths.PluginFolder;

        private static string SettingsPath => Path.Combine(PluginFolder, "settings.json");
        private static string EmulatorsPath => Path.Combine(PluginFolder, "emulators.json");
        private static string GamesPath => Path.Combine(PluginFolder, "games.json");

        public static string DataFolder
        {
            get
            {
                var dir = PluginFolder;
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static SaveSyncSettings LoadSettings()
        {
            if (!File.Exists(SettingsPath))
                return new SaveSyncSettings();

            try
            {
                return JsonConvert.DeserializeObject<SaveSyncSettings>(
                    File.ReadAllText(SettingsPath)
                ) ?? new SaveSyncSettings();
            }
            catch
            {
                return new SaveSyncSettings();
            }
        }

        public static void SaveSettings(SaveSyncSettings settings)
        {
            Directory.CreateDirectory(PluginFolder);
            File.WriteAllText(SettingsPath,
                JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public static List<EmulatorSaveConfig> LoadEmulatorConfigs()
        {
            if (!File.Exists(EmulatorsPath))
                return new List<EmulatorSaveConfig>();

            try
            {
                return JsonConvert.DeserializeObject<List<EmulatorSaveConfig>>(
                    File.ReadAllText(EmulatorsPath)
                ) ?? new List<EmulatorSaveConfig>();
            }
            catch
            {
                return new List<EmulatorSaveConfig>();
            }
        }

        public static void SaveEmulatorConfigs(List<EmulatorSaveConfig> configs)
        {
            // Safety net: an absolute path in LocalBase breaks the install on any
            // other machine, so it is refused before it reaches the JSON file.
            var error = ValidateEmulatorConfigs(configs);
            if (error != null)
                throw new ArgumentException(error, nameof(configs));

            Directory.CreateDirectory(PluginFolder);
            File.WriteAllText(EmulatorsPath,
                JsonConvert.SerializeObject(configs, Formatting.Indented));
        }

        public static List<GameSaveConfig> LoadGameConfigs()
        {
            if (!File.Exists(GamesPath))
                return new List<GameSaveConfig>();

            try
            {
                return JsonConvert.DeserializeObject<List<GameSaveConfig>>(
                    File.ReadAllText(GamesPath)
                ) ?? new List<GameSaveConfig>();
            }
            catch
            {
                return new List<GameSaveConfig>();
            }
        }

        public static void SaveGameConfigs(List<GameSaveConfig> configs)
        {
            Directory.CreateDirectory(PluginFolder);
            File.WriteAllText(GamesPath,
                JsonConvert.SerializeObject(configs, Formatting.Indented));
        }

        public static List<GameSaveConfig> LoadGameDefaults()
        {
            var path = Path.Combine(PluginFolder, "default_games.json");
            if (!File.Exists(path)) return new List<GameSaveConfig>();
            try
            {
                return JsonConvert.DeserializeObject<List<GameSaveConfig>>(
                    File.ReadAllText(path)
                ) ?? new List<GameSaveConfig>();
            }
            catch
            {
                return new List<GameSaveConfig>();
            }
        }

        public static List<EmulatorSaveConfig> LoadEmulatorDefaults()
        {
            var path = Path.Combine(PluginFolder, "default_emulators.json");
            if (!File.Exists(path)) return new List<EmulatorSaveConfig>();
            try
            {
                return JsonConvert.DeserializeObject<List<EmulatorSaveConfig>>(
                    File.ReadAllText(path)
                ) ?? new List<EmulatorSaveConfig>();
            }
            catch
            {
                return new List<EmulatorSaveConfig>();
            }
        }

        /// <summary>
        /// Returns an error message when <paramref name="localBase"/> is not a usable
        /// relative path, or <c>null</c> when it is fine (empty is fine: it means
        /// "use the emulator folder").
        /// </summary>
        public static string ValidateLocalBase(string localBase)
        {
            if (string.IsNullOrWhiteSpace(localBase))
                return null;

            if (PathResolver.IsAbsolute(localBase))
            {
                return "Use a relative path (for example ..\\Emuladores\\PSVita\\...). " +
                       "Absolute paths are not allowed: they break the install on another machine.";
            }

            return null;
        }

        /// <summary>Same rule for the paths below the local base.</summary>
        public static string ValidateRelativePaths(IEnumerable<string> relativePaths)
        {
            if (relativePaths == null)
                return null;

            foreach (var path in relativePaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                if (PathResolver.IsAbsolute(path))
                {
                    return $"\"{path}\" is absolute. Relative paths only " +
                           "(they are applied to the local base and to the server folder).";
                }
            }

            return null;
        }

        /// <summary>Validates every emulator config, returning the first problem found.</summary>
        public static string ValidateEmulatorConfigs(IEnumerable<EmulatorSaveConfig> configs)
        {
            if (configs == null)
                return null;

            foreach (var config in configs)
            {
                if (config == null)
                    continue;

                var localBaseError = ValidateLocalBase(config.LocalBase);
                if (localBaseError != null)
                    return $"{config.Title ?? config.FolderName ?? "(emulator)"}: {localBaseError}";

                var pathsError = ValidateRelativePaths(config.RelativePaths);
                if (pathsError != null)
                    return $"{config.Title ?? config.FolderName ?? "(emulator)"}: {pathsError}";
            }

            return null;
        }
    }
}
