using SaveSyncPlugin.Core.Interfaces;
using SaveSyncPlugin.UI.Forms;

namespace SaveSyncPlugin.UI.Reporters
{
    public class ProgressReporter : IProgressReporter
    {
        private readonly ProgressForm _form;

        public ProgressReporter(ProgressForm form)
        {
            _form = form;
        }

        public void SetTitle(string title)
        {
            _form.SetStatus(title);
        }

        public void SetStatus(string message)
        {
            _form.SetStatus(message);
        }

        public void SetProgress(int value)
        {
        }

        public void SetIndeterminate(bool value)
        {
        }
    }
}
