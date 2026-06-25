using System;
using System.Windows.Forms;

namespace SaveSyncPlugin.UI.Forms
{
    partial class ProgressForm
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

        private Label lblStatus;
        private ProgressBar progressBar;

        private void InitializeComponent()
        {
            lblStatus = new Label();
            progressBar = new ProgressBar();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.Location = new System.Drawing.Point(12, 20);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(410, 36);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Processing...";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            progressBar.Location = new System.Drawing.Point(12, 72);
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Name = "progressBar";
            progressBar.Size = new System.Drawing.Size(410, 30);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 1;
            // 
            // ProgressForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(11F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(438, 117);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            Font = new System.Drawing.Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(400, 140);
            Name = "ProgressForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "SaveSync";
            ResumeLayout(false);
        }
    }
}
