using System;
using System.Windows.Forms;
using System.Drawing;

namespace SaveSyncPlugin.UI.Forms
{
    partial class SaveSyncSettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private GroupBox grpPaths;
        private GroupBox grpSync;
        private GroupBox grpRobocopy;
        private Label lblServer;
        private Button btnBrowseServer;
        private Button btnSave;
        private Button btnCancel;
        private Label lblThreads;
        private Label lblRetries;
        private Label lblWait;
        private ToolTip toolTip;

        private TextBox txtServerPath;
        private CheckBox chkSyncOnStartup;
        private CheckBox chkSyncOnGameClose;
        private NumericUpDown nudThreads;
        private NumericUpDown nudRetries;
        private NumericUpDown nudWait;

        private void InitializeComponent()
        {
            grpPaths = new GroupBox();
            lblServer = new Label();
            txtServerPath = new TextBox();
            btnBrowseServer = new Button();
            grpSync = new GroupBox();
            chkSyncOnStartup = new CheckBox();
            chkSyncOnGameClose = new CheckBox();
            grpRobocopy = new GroupBox();
            lblThreads = new Label();
            nudThreads = new NumericUpDown();
            lblRetries = new Label();
            nudRetries = new NumericUpDown();
            lblWait = new Label();
            nudWait = new NumericUpDown();
            btnSave = new Button();
            btnCancel = new Button();
            toolTip = new ToolTip();
            grpPaths.SuspendLayout();
            grpSync.SuspendLayout();
            grpRobocopy.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudThreads).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRetries).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudWait).BeginInit();
            SuspendLayout();
            // 
            // grpPaths
            // 
            grpPaths.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpPaths.Controls.Add(lblServer);
            grpPaths.Controls.Add(txtServerPath);
            grpPaths.Controls.Add(btnBrowseServer);
            grpPaths.Location = new Point(12, 12);
            grpPaths.Name = "grpPaths";
            grpPaths.Size = new Size(660, 100);
            grpPaths.TabIndex = 0;
            grpPaths.TabStop = false;
            grpPaths.Text = "Directories";
            // 
            // lblServer
            // 
            lblServer.Location = new Point(6, 35);
            lblServer.Name = "lblServer";
            lblServer.Size = new Size(140, 36);
            lblServer.TabIndex = 0;
            lblServer.Text = "Server Backup Path:";
            lblServer.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtServerPath
            // 
            txtServerPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtServerPath.Location = new Point(152, 33);
            txtServerPath.Name = "txtServerPath";
            txtServerPath.Size = new Size(400, 34);
            txtServerPath.TabIndex = 1;
            toolTip.SetToolTip(txtServerPath, "Full path to the server backup directory");
            // 
            // btnBrowseServer
            // 
            btnBrowseServer.Location = new Point(559, 33);
            btnBrowseServer.Name = "btnBrowseServer";
            btnBrowseServer.Size = new Size(40, 40);
            btnBrowseServer.TabIndex = 2;
            btnBrowseServer.Text = "...";
            btnBrowseServer.Click += BtnBrowseServer_Click;
            // 
            // grpSync
            // 
            grpSync.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSync.Controls.Add(chkSyncOnStartup);
            grpSync.Controls.Add(chkSyncOnGameClose);
            grpSync.Location = new Point(12, 136);
            grpSync.Name = "grpSync";
            grpSync.Size = new Size(660, 124);
            grpSync.TabIndex = 1;
            grpSync.TabStop = false;
            grpSync.Text = "Sync Options";
            // 
            // chkSyncOnStartup
            // 
            chkSyncOnStartup.Location = new Point(11, 33);
            chkSyncOnStartup.Name = "chkSyncOnStartup";
            chkSyncOnStartup.Size = new Size(500, 36);
            chkSyncOnStartup.TabIndex = 0;
            chkSyncOnStartup.Text = "Download all saves on LaunchBox startup (server \u2192 local)";
            // 
            // chkSyncOnGameClose
            // 
            chkSyncOnGameClose.Location = new Point(11, 67);
            chkSyncOnGameClose.Name = "chkSyncOnGameClose";
            chkSyncOnGameClose.Size = new Size(500, 36);
            chkSyncOnGameClose.TabIndex = 1;
            chkSyncOnGameClose.Text = "Upload saves when a game closes (local \u2192 server)";
            // 
            // grpRobocopy
            // 
            grpRobocopy.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpRobocopy.Controls.Add(lblThreads);
            grpRobocopy.Controls.Add(nudThreads);
            grpRobocopy.Controls.Add(lblRetries);
            grpRobocopy.Controls.Add(nudRetries);
            grpRobocopy.Controls.Add(lblWait);
            grpRobocopy.Controls.Add(nudWait);
            grpRobocopy.Location = new Point(12, 295);
            grpRobocopy.Name = "grpRobocopy";
            grpRobocopy.Size = new Size(660, 111);
            grpRobocopy.TabIndex = 2;
            grpRobocopy.TabStop = false;
            grpRobocopy.Text = "Robocopy";
            // 
            // lblThreads
            // 
            lblThreads.Location = new Point(11, 46);
            lblThreads.Name = "lblThreads";
            lblThreads.Size = new Size(85, 36);
            lblThreads.TabIndex = 0;
            lblThreads.Text = "Threads:";
            lblThreads.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // nudThreads
            // 
            nudThreads.Location = new Point(102, 44);
            nudThreads.Maximum = new decimal(new int[] { 128, 0, 0, 0 });
            nudThreads.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudThreads.Name = "nudThreads";
            nudThreads.Size = new Size(70, 34);
            nudThreads.TabIndex = 1;
            nudThreads.Value = new decimal(new int[] { 8, 0, 0, 0 });
            toolTip.SetToolTip(nudThreads, "Number of robocopy threads (default: 8)");
            // 
            // lblRetries
            // 
            lblRetries.Location = new Point(199, 46);
            lblRetries.Name = "lblRetries";
            lblRetries.Size = new Size(65, 36);
            lblRetries.TabIndex = 2;
            lblRetries.Text = "Retries:";
            lblRetries.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // nudRetries
            // 
            nudRetries.Location = new Point(269, 44);
            nudRetries.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            nudRetries.Name = "nudRetries";
            nudRetries.Size = new Size(70, 34);
            nudRetries.TabIndex = 3;
            nudRetries.Value = new decimal(new int[] { 3, 0, 0, 0 });
            toolTip.SetToolTip(nudRetries, "Number of retries on failure (default: 3)");
            // 
            // lblWait
            // 
            lblWait.Location = new Point(379, 46);
            lblWait.Name = "lblWait";
            lblWait.Size = new Size(65, 36);
            lblWait.TabIndex = 4;
            lblWait.Text = "Wait (s):";
            lblWait.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // nudWait
            // 
            nudWait.Location = new Point(449, 44);
            nudWait.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            nudWait.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudWait.Name = "nudWait";
            nudWait.Size = new Size(70, 34);
            nudWait.TabIndex = 5;
            nudWait.Value = new decimal(new int[] { 10, 0, 0, 0 });
            toolTip.SetToolTip(nudWait, "Wait time in seconds between retries (default: 10)");
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(403, 454);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(120, 40);
            btnSave.TabIndex = 3;
            btnSave.Text = "Save";
            btnSave.Click += BtnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(552, 454);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(120, 40);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.Click += BtnCancel_Click;
            // 
            // SaveSyncSettingsForm
            // 
            AutoScaleDimensions = new SizeF(11F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(720, 580);
            Controls.Add(grpPaths);
            Controls.Add(grpSync);
            Controls.Add(grpRobocopy);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(720, 580);
            Name = "SaveSyncSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "SaveSync: Settings";
            grpPaths.ResumeLayout(false);
            grpPaths.PerformLayout();
            grpSync.ResumeLayout(false);
            grpRobocopy.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudThreads).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRetries).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudWait).EndInit();
            ResumeLayout(false);
        }
    }
}
