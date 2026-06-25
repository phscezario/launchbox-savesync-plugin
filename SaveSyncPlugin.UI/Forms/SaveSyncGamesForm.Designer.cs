using System;
using System.Windows.Forms;
using System.Drawing;

namespace SaveSyncPlugin.UI.Forms
{
    partial class SaveSyncGamesForm
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

        private Label lblHeader;
        private Label lblPlatform;
        private Label lblSearch;
        private Label lblGames;
        private GroupBox grpDetails;
        private Label lblType;
        private Label lblFolder;
        private Label lblFolderHint;
        private Label lblPaths;
        private Button btnSave;
        private Button btnRemove;
        private Button btnSaveAll;
        private Button btnCancel;

        private TextBox txtSearch;
        private ListBox lstGames;
        private ComboBox cmbPlatform;
        private ComboBox cmbSaveType;
        private TextBox txtFolderName;
        private TextBox txtRelativePaths;
        private Label lblPathHint;
        private Label lblStatus;

        private ToolTip toolTip;

        private void InitializeComponent()
        {
            lblHeader = new Label();
            lblPlatform = new Label();
            cmbPlatform = new ComboBox();
            lblSearch = new Label();
            txtSearch = new TextBox();
            lblGames = new Label();
            lstGames = new ListBox();
            grpDetails = new GroupBox();
            lblType = new Label();
            cmbSaveType = new ComboBox();
            lblFolder = new Label();
            txtFolderName = new TextBox();
            lblFolderHint = new Label();
            lblPaths = new Label();
            txtRelativePaths = new TextBox();
            lblPathHint = new Label();
            lblStatus = new Label();
            btnSave = new Button();
            btnRemove = new Button();
            btnSaveAll = new Button();
            btnCancel = new Button();
            toolTip = new ToolTip();
            grpDetails.SuspendLayout();
            SuspendLayout();
            // 
            // lblHeader
            // 
            lblHeader.Location = new Point(12, 12);
            lblHeader.Name = "lblHeader";
            lblHeader.Size = new Size(800, 30);
            lblHeader.TabIndex = 0;
            lblHeader.Text = "Select a platform, then configure save paths for its games.";
            lblHeader.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPlatform
            // 
            lblPlatform.Location = new Point(12, 59);
            lblPlatform.Name = "lblPlatform";
            lblPlatform.Size = new Size(102, 36);
            lblPlatform.TabIndex = 1;
            lblPlatform.Text = "Platform:";
            lblPlatform.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbPlatform
            // 
            cmbPlatform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbPlatform.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPlatform.Location = new Point(120, 60);
            cmbPlatform.Name = "cmbPlatform";
            cmbPlatform.Size = new Size(529, 36);
            cmbPlatform.TabIndex = 2;
            cmbPlatform.SelectedIndexChanged += CmbPlatform_SelectedIndexChanged;
            // 
            // lblSearch
            // 
            lblSearch.Location = new Point(12, 103);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(102, 36);
            lblSearch.TabIndex = 3;
            lblSearch.Text = "Search:";
            lblSearch.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.Location = new Point(120, 104);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(529, 34);
            txtSearch.TabIndex = 4;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            // 
            // lblGames
            // 
            lblGames.Location = new Point(12, 156);
            lblGames.Name = "lblGames";
            lblGames.Size = new Size(80, 36);
            lblGames.TabIndex = 5;
            lblGames.Text = "Games:";
            lblGames.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lstGames
            // 
            lstGames.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lstGames.DisplayMember = "Title";
            lstGames.Location = new Point(12, 195);
            lstGames.Name = "lstGames";
            lstGames.Size = new Size(637, 424);
            lstGames.TabIndex = 6;
            lstGames.SelectedIndexChanged += LstGames_SelectedIndexChanged;
            // 
            // grpDetails
            // 
            grpDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpDetails.Controls.Add(lblType);
            grpDetails.Controls.Add(cmbSaveType);
            grpDetails.Controls.Add(lblFolder);
            grpDetails.Controls.Add(txtFolderName);
            grpDetails.Controls.Add(lblFolderHint);
            grpDetails.Controls.Add(lblPaths);
            grpDetails.Controls.Add(txtRelativePaths);
            grpDetails.Controls.Add(lblPathHint);
            grpDetails.Controls.Add(lblStatus);
            grpDetails.Location = new Point(676, 45);
            grpDetails.Name = "grpDetails";
            grpDetails.Size = new Size(709, 553);
            grpDetails.TabIndex = 7;
            grpDetails.TabStop = false;
            grpDetails.Text = "Save Configuration";
            // 
            // lblType
            // 
            lblType.Location = new Point(12, 44);
            lblType.Name = "lblType";
            lblType.Size = new Size(69, 36);
            lblType.TabIndex = 0;
            lblType.Text = "Type:";
            lblType.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbSaveType
            // 
            cmbSaveType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSaveType.Items.AddRange(new object[] { "relative", "appdata", "userprofile", "public" });
            cmbSaveType.Location = new Point(93, 42);
            cmbSaveType.Name = "cmbSaveType";
            cmbSaveType.Size = new Size(225, 36);
            cmbSaveType.TabIndex = 1;
            cmbSaveType.SelectedIndexChanged += CmbSaveType_SelectedIndexChanged;
            toolTip.SetToolTip(cmbSaveType, "relative = under game folder, appdata = %APPDATA%, userprofile = %USERPROFILE%, public = %PUBLIC%\\Documents");
            // 
            // lblFolder
            // 
            lblFolder.Location = new Point(12, 88);
            lblFolder.Name = "lblFolder";
            lblFolder.Size = new Size(147, 36);
            lblFolder.TabIndex = 2;
            lblFolder.Text = "Server folder:";
            lblFolder.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtFolderName
            // 
            txtFolderName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFolderName.Location = new Point(153, 90);
            txtFolderName.Name = "txtFolderName";
            txtFolderName.Size = new Size(546, 34);
            txtFolderName.TabIndex = 3;
            toolTip.SetToolTip(txtFolderName, "Subfolder name on the server under WindowsGames\\");
            // 
            // lblFolderHint
            // 
            lblFolderHint.ForeColor = Color.Gray;
            lblFolderHint.Location = new Point(157, 121);
            lblFolderHint.Name = "lblFolderHint";
            lblFolderHint.Size = new Size(300, 36);
            lblFolderHint.TabIndex = 4;
            lblFolderHint.Text = "Subfolder under server\\WindowsGames\\";
            // 
            // lblPaths
            // 
            lblPaths.Location = new Point(12, 166);
            lblPaths.Name = "lblPaths";
            lblPaths.Size = new Size(200, 36);
            lblPaths.TabIndex = 5;
            lblPaths.Text = "Relative paths (one per line):";
            lblPaths.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtRelativePaths
            // 
            txtRelativePaths.AcceptsReturn = true;
            txtRelativePaths.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtRelativePaths.Font = new Font("Consolas", 9F);
            txtRelativePaths.Location = new Point(12, 205);
            txtRelativePaths.Multiline = true;
            txtRelativePaths.Name = "txtRelativePaths";
            txtRelativePaths.ScrollBars = ScrollBars.Vertical;
            txtRelativePaths.Size = new Size(687, 290);
            txtRelativePaths.TabIndex = 6;
            toolTip.SetToolTip(txtRelativePaths, "One path per line. Files: include extension (gm.dat). Folders: just the folder name.");
            // 
            // lblPathHint
            // 
            lblPathHint.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblPathHint.ForeColor = Color.Gray;
            lblPathHint.Location = new Point(12, 500);
            lblPathHint.Name = "lblPathHint";
            lblPathHint.Size = new Size(420, 36);
            lblPathHint.TabIndex = 7;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatus.Location = new Point(12, 510);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(400, 36);
            lblStatus.TabIndex = 8;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSave.Location = new Point(12, 635);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(110, 40);
            btnSave.TabIndex = 8;
            btnSave.Text = "Save";
            btnSave.Click += BtnSave_Click;
            // 
            // btnRemove
            // 
            btnRemove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRemove.Location = new Point(150, 635);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(110, 40);
            btnRemove.TabIndex = 9;
            btnRemove.Text = "Remove";
            btnRemove.Click += BtnRemove_Click;
            // 
            // btnSaveAll
            // 
            btnSaveAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSaveAll.Location = new Point(1021, 693);
            btnSaveAll.Name = "btnSaveAll";
            btnSaveAll.Size = new Size(200, 40);
            btnSaveAll.TabIndex = 10;
            btnSaveAll.Text = "Save All && Close";
            btnSaveAll.Click += BtnSaveAll_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(1255, 693);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(120, 40);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Cancel";
            btnCancel.Click += BtnCancel_Click;
            // 
            // SaveSyncGamesForm
            // 
            AutoScaleDimensions = new SizeF(11F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1405, 761);
            Controls.Add(lblHeader);
            Controls.Add(lblPlatform);
            Controls.Add(cmbPlatform);
            Controls.Add(lblSearch);
            Controls.Add(txtSearch);
            Controls.Add(lblGames);
            Controls.Add(lstGames);
            Controls.Add(grpDetails);
            Controls.Add(btnSave);
            Controls.Add(btnRemove);
            Controls.Add(btnSaveAll);
            Controls.Add(btnCancel);
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(860, 580);
            Name = "SaveSyncGamesForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "SaveSync: Windows Game Save Configurations";
            grpDetails.ResumeLayout(false);
            grpDetails.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
