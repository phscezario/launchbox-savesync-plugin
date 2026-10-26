using System;
using System.Windows.Forms;
using SaveSyncPlugin.Core.Models;
using SaveSyncPlugin.Core.Storage;

namespace SaveSyncPlugin.UI.Forms
{
    public partial class SaveSyncSettingsForm : Form
    {
        private bool _deleteExtraneousOnLoad;

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
            chkAlwaysKeepNewer.Checked = s.AlwaysKeepNewer;
            chkAskOnConflict.Checked = s.AskOnConflict;
            nudThreads.Value = s.RobocopyThreads;
            nudRetries.Value = s.RobocopyRetries;
            nudWait.Value = s.RobocopyWaitSeconds;

            _deleteExtraneousOnLoad = s.DeleteExtraneous;
            chkDeleteExtraneous.Checked = s.DeleteExtraneous;
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
            if (chkDeleteExtraneous.Checked && !_deleteExtraneousOnLoad)
            {
                var answer = MessageBox.Show(
                    "Mirror mode adds /PURGE to robocopy.\r\n\r\n" +
                    "A save that exists on only one side (for example, a save made while " +
                    "the server folder was offline) will be DELETED from the other side.\r\n\r\n" +
                    "Enable mirror mode?",
                    "SaveSync: dangerous option",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (answer != DialogResult.Yes)
                {
                    chkDeleteExtraneous.Checked = false;
                    return;
                }
            }

            var s = new SaveSyncSettings
            {
                ServerBasePath = txtServerPath.Text.Trim(),
                SyncOnStartup = chkSyncOnStartup.Checked,
                SyncOnGameClose = chkSyncOnGameClose.Checked,
                AlwaysKeepNewer = chkAlwaysKeepNewer.Checked,
                AskOnConflict = chkAskOnConflict.Checked,
                RobocopyThreads = (int)nudThreads.Value,
                RobocopyRetries = (int)nudRetries.Value,
                RobocopyWaitSeconds = (int)nudWait.Value,
                DeleteExtraneous = chkDeleteExtraneous.Checked
            };

            SaveSyncStorage.SaveSettings(s);

            MessageBox.Show("Settings saved.", "SaveSync",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }
}
