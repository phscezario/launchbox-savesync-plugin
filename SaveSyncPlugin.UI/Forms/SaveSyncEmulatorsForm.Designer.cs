using System;
using System.Windows.Forms;

namespace SaveSyncPlugin.UI.Forms
{
    partial class SaveSyncEmulatorsForm
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

        private ListBox lstEmulators;
        private TextBox txtFolderName;
        private TextBox txtPaths;
        private ToolTip toolTip;

        private void InitializeComponent()
        {
            lblInfo = new Label();
            lblEmulators = new Label();
            lstEmulators = new ListBox();
            lblFolderName = new Label();
            txtFolderName = new TextBox();
            lblFolderHint = new Label();
            lblPaths = new Label();
            txtPaths = new TextBox();
            lblPathHint = new Label();
            btnSave = new Button();
            btnSaveAll = new Button();
            btnCancel = new Button();
            toolTip = new ToolTip();
            SuspendLayout();
            // 
            // lblInfo
            // 
            lblInfo.Location = new System.Drawing.Point(12, 12);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new System.Drawing.Size(700, 30);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Select an emulator, then configure the server folder name and paths to sync.";
            lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblEmulators
            // 
            lblEmulators.Location = new System.Drawing.Point(12, 50);
            lblEmulators.Name = "lblEmulators";
            lblEmulators.Size = new System.Drawing.Size(240, 36);
            lblEmulators.TabIndex = 1;
            lblEmulators.Text = "Emulators:";
            lblEmulators.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lstEmulators
            // 
            lstEmulators.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lstEmulators.Location = new System.Drawing.Point(12, 84);
            lstEmulators.Name = "lstEmulators";
            lstEmulators.Size = new System.Drawing.Size(400, 564);
            lstEmulators.TabIndex = 2;
            lstEmulators.SelectedIndexChanged += LstEmulators_SelectedIndexChanged;
            // 
            // lblFolderName
            // 
            lblFolderName.Location = new System.Drawing.Point(436, 49);
            lblFolderName.Name = "lblFolderName";
            lblFolderName.Size = new System.Drawing.Size(200, 36);
            lblFolderName.TabIndex = 3;
            lblFolderName.Text = "Server folder name:";
            lblFolderName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtFolderName
            // 
            txtFolderName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFolderName.Location = new System.Drawing.Point(436, 83);
            txtFolderName.Name = "txtFolderName";
            txtFolderName.Size = new System.Drawing.Size(504, 34);
            txtFolderName.TabIndex = 4;
            toolTip.SetToolTip(txtFolderName, "Folder name on the server");
            // 
            // lblFolderHint
            // 
            lblFolderHint.ForeColor = System.Drawing.Color.Gray;
            lblFolderHint.Location = new System.Drawing.Point(436, 119);
            lblFolderHint.Name = "lblFolderHint";
            lblFolderHint.Size = new System.Drawing.Size(400, 36);
            lblFolderHint.TabIndex = 5;
            lblFolderHint.Text = "ex: \"Arcade\", \"3DS\" (subfolder under server path)";
            // 
            // lblPaths
            // 
            lblPaths.Location = new System.Drawing.Point(436, 159);
            lblPaths.Name = "lblPaths";
            lblPaths.Size = new System.Drawing.Size(400, 36);
            lblPaths.TabIndex = 6;
            lblPaths.Text = "Relative paths to sync (one per line):";
            lblPaths.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPaths
            // 
            txtPaths.AcceptsReturn = true;
            txtPaths.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtPaths.Location = new System.Drawing.Point(436, 193);
            txtPaths.Multiline = true;
            txtPaths.Name = "txtPaths";
            txtPaths.ScrollBars = ScrollBars.Vertical;
            txtPaths.Size = new System.Drawing.Size(504, 288);
            txtPaths.TabIndex = 7;
            toolTip.SetToolTip(txtPaths, "One path per line. Files: include extension. Folders: just the folder name.");
            // 
            // lblPathHint
            // 
            lblPathHint.ForeColor = System.Drawing.Color.Gray;
            lblPathHint.Location = new System.Drawing.Point(436, 484);
            lblPathHint.Name = "lblPathHint";
            lblPathHint.Size = new System.Drawing.Size(420, 36);
            lblPathHint.TabIndex = 8;
            lblPathHint.Text = "ex: nvram, sta, snap, _FinalBurnNeo\\arcadeController\\savestates";
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSave.Location = new System.Drawing.Point(436, 523);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(130, 40);
            btnSave.TabIndex = 9;
            btnSave.Text = "Save Config";
            btnSave.Click += BtnSave_Click;
            // 
            // btnSaveAll
            // 
            btnSaveAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSaveAll.Location = new System.Drawing.Point(565, 608);
            btnSaveAll.Name = "btnSaveAll";
            btnSaveAll.Size = new System.Drawing.Size(210, 40);
            btnSaveAll.TabIndex = 10;
            btnSaveAll.Text = "Save All && Close";
            btnSaveAll.Click += BtnSaveAll_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new System.Drawing.Point(820, 608);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(120, 40);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Cancel";
            btnCancel.Click += BtnCancel_Click;
            // 
            // SaveSyncEmulatorsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(11F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new System.Drawing.Size(964, 676);
            Controls.Add(lblInfo);
            Controls.Add(lblEmulators);
            Controls.Add(lstEmulators);
            Controls.Add(lblFolderName);
            Controls.Add(txtFolderName);
            Controls.Add(lblFolderHint);
            Controls.Add(lblPaths);
            Controls.Add(txtPaths);
            Controls.Add(lblPathHint);
            Controls.Add(btnSave);
            Controls.Add(btnSaveAll);
            Controls.Add(btnCancel);
            Font = new System.Drawing.Font("Segoe UI", 10F);
            MinimumSize = new System.Drawing.Size(700, 520);
            Name = "SaveSyncEmulatorsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "SaveSync: Emulator Save Configurations";
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblInfo;
        private Label lblEmulators;
        private Label lblFolderName;
        private Label lblFolderHint;
        private Label lblPaths;
        private Label lblPathHint;
        private Button btnSave;
        private Button btnSaveAll;
        private Button btnCancel;
    }
}
