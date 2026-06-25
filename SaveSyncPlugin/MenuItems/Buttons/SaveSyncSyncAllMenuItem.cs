using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaveSyncPlugin.Core.Storage;
using SaveSyncPlugin.MenuItems;
using SaveSyncPlugin.Services;
using SaveSyncPlugin.UI.Forms;
using Unbroken.LaunchBox.Plugins;

namespace SaveSyncPlugin.MenuItems.Buttons
{
    public class SaveSyncSyncAllMenuItem : SaveSyncMenuItem, ISystemMenuItemPlugin
    {
        public override string Caption => "SaveSync: Sync All";
        private readonly SaveSyncService _service = new SaveSyncService();

        public override void OnSelected()
        {
            var result = MessageBox.Show(
                "Sync all saves bidirectionally?\n\n" +
                "\"Sync\" keeps the newest files on both sides.\n\n" +
                "Yes = Sync all\n" +
                "No = Upload all (local to server)\n" +
                "Cancel = Download all (server to local)",
                "SaveSync",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );

            var emuConfigs = SaveSyncStorage.LoadEmulatorConfigs();
            var gameConfigs = SaveSyncStorage.LoadGameConfigs();

            Action action = null;
            string actionLabel = "";

            if (result == DialogResult.Yes)
            {
                action = () => _service.SyncAll(emuConfigs, gameConfigs, _progress);
                actionLabel = "Syncing";
            }
            else if (result == DialogResult.No)
            {
                action = () => _service.UploadAll(emuConfigs, gameConfigs, _progress);
                actionLabel = "Uploading";
            }
            else if (result == DialogResult.Cancel)
            {
                action = () => _service.DownloadAll(emuConfigs, gameConfigs, _progress);
                actionLabel = "Downloading";
            }

            if (action == null) return;

            RunWithProgress(action, actionLabel);
        }

        private IProgress<string> _progress;

        private void RunWithProgress(Action action, string actionLabel)
        {
            using (var form = new ProgressForm())
            {
                _progress = new Progress<string>(msg => form.SetStatus(msg));
                var task = Task.Run(action);
                form.Shown += async (_, _) =>
                {
                    await task;
                    form.SetStatus($"{actionLabel} complete!");
                    await Task.Delay(800);
                    form.Close();
                };
                form.ShowDialog(Form.ActiveForm);
            }
        }
    }
}
