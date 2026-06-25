using System.Windows.Forms;

namespace SaveSyncPlugin.UI.Forms
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        public void SetStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => lblStatus.Text = message));
                return;
            }
            lblStatus.Text = message;
        }
    }
}
