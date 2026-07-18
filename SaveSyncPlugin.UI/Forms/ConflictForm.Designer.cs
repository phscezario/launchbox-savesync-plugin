using System;
using System.Windows.Forms;

namespace SaveSyncPlugin.UI.Forms
{
    partial class ConflictForm
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

        private Label lblTitle;
        private Label lblFileName;
        private Label lblMessage;
        private Label lblLocalInfo;
        private Label lblServerInfo;
        private Button btnUseLocal;
        private Button btnUseServer;

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblFileName = new Label();
            lblMessage = new Label();
            lblLocalInfo = new Label();
            lblServerInfo = new Label();
            btnUseLocal = new Button();
            btnUseServer = new Button();
            SuspendLayout();

            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblTitle.Location = new System.Drawing.Point(15, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(200, 25);
            lblTitle.Text = "Conflito de Save Detectado";

            lblFileName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblFileName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblFileName.Location = new System.Drawing.Point(15, 50);
            lblFileName.Name = "lblFileName";
            lblFileName.Size = new System.Drawing.Size(405, 25);
            lblFileName.Text = "save.sav";

            lblMessage.AutoSize = true;
            lblMessage.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblMessage.Location = new System.Drawing.Point(15, 85);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new System.Drawing.Size(300, 20);
            lblMessage.Text = "O save local é mais novo.";

            lblLocalInfo.AutoSize = true;
            lblLocalInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblLocalInfo.Location = new System.Drawing.Point(15, 115);
            lblLocalInfo.Name = "lblLocalInfo";
            lblLocalInfo.Size = new System.Drawing.Size(300, 15);
            lblLocalInfo.Text = "Local: 17/07/2026 14:30 (1.2 MB)";

            lblServerInfo.AutoSize = true;
            lblServerInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblServerInfo.Location = new System.Drawing.Point(15, 138);
            lblServerInfo.Name = "lblServerInfo";
            lblServerInfo.Size = new System.Drawing.Size(300, 15);
            lblServerInfo.Text = "Servidor: 16/07/2026 09:15 (1.1 MB)";

            btnUseLocal.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnUseLocal.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            btnUseLocal.Location = new System.Drawing.Point(200, 175);
            btnUseLocal.Name = "btnUseLocal";
            btnUseLocal.Size = new System.Drawing.Size(110, 35);
            btnUseLocal.Text = "Sim, usar local";
            btnUseLocal.UseVisualStyleBackColor = true;
            btnUseLocal.Click += BtnUseLocal_Click;

            btnUseServer.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnUseServer.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            btnUseServer.Location = new System.Drawing.Point(320, 175);
            btnUseServer.Name = "btnUseServer";
            btnUseServer.Size = new System.Drawing.Size(110, 35);
            btnUseServer.Text = "Não, usar servidor";
            btnUseServer.UseVisualStyleBackColor = true;
            btnUseServer.Click += BtnUseServer_Click;

            AutoScaleDimensions = new System.Drawing.SizeF(11F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(445, 225);
            Controls.Add(lblTitle);
            Controls.Add(lblFileName);
            Controls.Add(lblMessage);
            Controls.Add(lblLocalInfo);
            Controls.Add(lblServerInfo);
            Controls.Add(btnUseLocal);
            Controls.Add(btnUseServer);
            Font = new System.Drawing.Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConflictForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "SaveSync - Conflito";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
