using System;
using System.Windows.Forms;
using SaveSyncPlugin.Core.Storage;
using SaveSyncPlugin.Services;
using SaveSyncPlugin.UI.Forms;

namespace SaveSyncPlugin.CLI
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                MessageBox.Show(
                    "Usage:\n" +
                    "  SaveSyncPlugin.CLI.exe <upload|download|sync> <gameId|--all>\n" +
                    "  SaveSyncPlugin.CLI.exe <upload|download|sync> --emulator <emulatorId>\n" +
                    "  SaveSyncPlugin.CLI.exe settings",
                    "SaveSync CLI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var action = args[0].ToLower();

            if (action == "settings")
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                using (var form = new SaveSyncSettingsForm())
                    form.ShowDialog();
                return;
            }

            if (args.Length < 2)
            {
                MessageBox.Show("Missing argument.", "SaveSync CLI",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var service = new SaveSyncService();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var form = new ProgressForm())
            {
                form.Show();
                form.SetStatus($"SaveSync: {action}ing...");
                Application.DoEvents();

                try
                {
                    bool isEmulator = args[1].ToLower() == "--emulator" && args.Length >= 3;
                    string targetId = isEmulator ? args[2] : args[1];

                    var processor = new SaveSyncProcessor();
                    processor.Process(action, targetId, isEmulator);

                    form.SetStatus("Done!");
                    System.Threading.Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "SaveSync CLI",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
