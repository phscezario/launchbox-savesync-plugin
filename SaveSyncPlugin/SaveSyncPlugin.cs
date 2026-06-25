using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaveSyncPlugin.Core.Storage;
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
        }

        public void OnAfterGameLaunched(IGame game, IAdditionalApplication app, IEmulator emulator)
        {
        }

        public void OnGameExited()
        {
            try
            {
                var settings = SaveSyncStorage.LoadSettings();
                if (!settings.SyncOnGameClose || _lastGame == null) return;

                using (var form = new ProgressForm())
                {
                    var progress = new Progress<string>(m => form.SetStatus(m));
                    Task task;

                    if (!string.IsNullOrEmpty(_lastGame.EmulatorId))
                    {
                        var emulator = PluginHelper.DataManager.GetEmulatorById(_lastGame.EmulatorId);
                        if (emulator != null)
                            task = Task.Run(() =>
                                _syncService.SyncByEmulator(emulator, progress));
                        else return;
                    }
                    else
                    {
                        task = Task.Run(() =>
                            _syncService.SyncByGame(_lastGame, progress));
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
                System.Diagnostics.Debug.WriteLine("[SaveSync] GameExited error: " + ex);
            }
        }
    }
}
