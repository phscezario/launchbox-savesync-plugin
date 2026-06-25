using System;
using System.Windows.Forms;
using SaveSyncPlugin.Core.Models;
using SaveSyncPlugin.Core.Storage;

namespace SaveSyncPlugin.UI.Forms
{
    public partial class SaveSyncSettingsForm : Form
    {
        public SaveSyncSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var s = SaveSyncStorage.LoadSettings();
            txtServerPath.Text = s.ServerBasePath;
            chkSyncOnStartup.Checked = s.SyncOnStartup;
            chkSyncOnGameClose.Checked = s.SyncOnGameClose;
            nudThreads.Value = s.RobocopyThreads;
            nudRetries.Value = s.RobocopyRetries;
            nudWait.Value = s.RobocopyWaitSeconds;
        }

        private void BtnBrowseServer_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog { Description = "Select server backup path" })
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    txtServerPath.Text = dlg.SelectedPath;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var s = new SaveSyncSettings
            {
                ServerBasePath = txtServerPath.Text.Trim(),
                SyncOnStartup = chkSyncOnStartup.Checked,
                SyncOnGameClose = chkSyncOnGameClose.Checked,
                RobocopyThreads = (int)nudThreads.Value,
                RobocopyRetries = (int)nudRetries.Value,
                RobocopyWaitSeconds = (int)nudWait.Value
            };

            SaveSyncStorage.SaveSettings(s);

            MessageBox.Show("Settings saved.", "SaveSync",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }
}
