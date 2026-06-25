using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SaveSyncPlugin.Core.Models;

namespace SaveSyncPlugin.Core.Storage
{
    public static class SaveSyncStorage
    {
        private static string PluginFolder =>
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Plugins",
                "SaveSync LaunchBox Integration"
            );

        private static string SettingsPath => Path.Combine(PluginFolder, "settings.json");
        private static string EmulatorsPath => Path.Combine(PluginFolder, "emulators.json");
        private static string GamesPath => Path.Combine(PluginFolder, "games.json");

        public static string DataFolder
        {
            get
            {
                var dir = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Plugins",
                    "SaveSync LaunchBox Integration"
                );
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
    }
}
