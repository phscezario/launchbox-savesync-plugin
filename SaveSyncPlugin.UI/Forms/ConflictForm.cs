using System;
using System.Windows.Forms;
using SaveSyncPlugin.Core.Models;

namespace SaveSyncPlugin.UI.Forms
{
    public partial class ConflictForm : Form
    {
        public bool UseLocal { get; private set; }

        public ConflictForm(FileConflict conflict)
        {
            InitializeComponent();
            LoadConflict(conflict);
        }

        private void LoadConflict(FileConflict conflict)
        {
            var fileName = System.IO.Path.GetFileName(conflict.LocalPath);
            lblFileName.Text = fileName;

            lblLocalInfo.Text = $"Local: {conflict.LocalModified:dd/MM/yyyy HH:mm} ({FormatSize(conflict.LocalSize)})";
            lblServerInfo.Text = $"Servidor: {conflict.ServerModified:dd/MM/yyyy HH:mm} ({FormatSize(conflict.ServerSize)})";

            if (conflict.LocalIsNewer)
            {
                lblMessage.Text = "O save local é mais novo.";
                btnUseLocal.Text = "Sim, usar local";
                btnUseServer.Text = "Não, usar servidor";
            }
            else
            {
                lblMessage.Text = "O save do servidor é mais novo.";
                btnUseLocal.Text = "Sim, usar servidor";
                btnUseServer.Text = "Não, usar local";
            }
        }

        private string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }

        private void BtnUseLocal_Click(object sender, EventArgs e)
        {
            UseLocal = true;
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void BtnUseServer_Click(object sender, EventArgs e)
        {
            UseLocal = false;
            DialogResult = DialogResult.No;
            Close();
        }
    }
}
