using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaveSyncPlugin.Core.Storage;
using SaveSyncPlugin.Core.Sync;
using SaveSyncPlugin.Services;
using SaveSyncPlugin.UI.Forms;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace SaveSyncPlugin
{
    public class SaveSyncMenuPlugin : ISystemEventsPlugin, IGameLaunchingPlugin
    {
        private readonly SaveSyncService _syncService = new SaveSyncService();
        private IGame _lastGame;

        public void OnEventRaised(string eventType)
        {
            if (eventType == SystemEventTypes.LaunchBoxStartupCompleted)
            {
                try
                {
                    var settings = SaveSyncStorage.LoadSettings();
                    if (settings.SyncOnStartup)
                    {
                        var emuConfigs = SaveSyncStorage.LoadEmulatorConfigs();
                        var gameConfigs = SaveSyncStorage.LoadGameConfigs();
                        if (emuConfigs.Count > 0 || gameConfigs.Count > 0)
                        {
                            var form = new ProgressForm();
                            var progress = new Progress<string>(m => form.SetStatus(m));
                            var task = Task.Run(() =>
                                _syncService.DownloadAll(emuConfigs, gameConfigs, progress));
                            task.ContinueWith(_ =>
                            {
                                try
                                {
                                    form.Invoke(new Action(() =>
                                    {
                                        form.SetStatus("Startup sync complete!");
                                        System.Threading.Thread.Sleep(500);
                                        form.Close();
                                    }));
                                }
                                catch { }
                            });
                            form.Show();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[SaveSync] Startup error: " + ex);
                }
            }
        }

        public void OnBeforeGameLaunching(IGame game, IAdditionalApplication app, IEmulator emulator)
        {
            _lastGame = game;

            // Proof that the plugin is tracking the launch, and which application
            // (the wrapper, when there is one) LaunchBox actually started.
            SyncLog.Write(
                $"LAUNCH  {game?.Title ?? "(sem titulo)"} | emulator={emulator?.Id ?? game?.EmulatorId ?? "-"} | app={emulator?.ApplicationPath ?? "-"}");
        }

        public void OnAfterGameLaunched(IGame game, IAdditionalApplication app, IEmulator emulator)
        {
        }

        public void OnGameExited()
        {
            try
            {
                var settings = SaveSyncStorage.LoadSettings();

                if (!settings.SyncOnGameClose)
                {
                    SyncLog.Write("EXIT    (sync on game close disabled)");
                    return;
                }

                if (_lastGame == null)
                {
                    SyncLog.Write("EXIT    (no game tracked)");
                    return;
                }

                var game = _lastGame;

                SyncLog.Write($"EXIT    {game.Title ?? "(sem titulo)"} | emulator={game.EmulatorId ?? "-"}");

                using (var form = new ProgressForm())
                {
                    var progress = new Progress<string>(m => form.SetStatus(m));
                    Task task;

                    var emulator = string.IsNullOrEmpty(game.EmulatorId)
                        ? null
                        : PluginHelper.DataManager.GetEmulatorById(game.EmulatorId);

                    if (emulator != null)
                    {
                        task = Task.Run(() => _syncService.SyncByEmulator(emulator, progress));
                    }
                    else
                    {
                        // No emulator resolved: fall back to the per-game config instead
                        // of silently leaving the save unsynced.
                        SyncLog.Write($"WARN    EXIT without emulator (id={game.EmulatorId ?? "-"}); trying game config");
                        task = Task.Run(() => _syncService.SyncByGame(game, progress));
                    }

                    form.Shown += async (_, _) =>
                    {
                        await task;
                        form.SetStatus("Sync complete!");
                        await Task.Delay(500);
                        form.Close();
                    };
                    form.ShowDialog();
                }

                _lastGame = null;
            }
            catch (Exception ex)
            {
                SyncLog.Write($"FAIL    OnGameExited: {ex.Message}");
            }
        }
    }
}
