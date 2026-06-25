using SaveSyncPlugin.Core.Storage;
using SaveSyncPlugin.MenuItems;
using SaveSyncPlugin.UI.Forms;
using Unbroken.LaunchBox.Plugins;

namespace SaveSyncPlugin.MenuItems.Buttons
{
    public class SaveSyncEmulatorsMenuItem : SaveSyncMenuItem, ISystemMenuItemPlugin
    {
        public override string Caption => "SaveSync: Emulators";

        public override void OnSelected()
        {
            var configs = SaveSyncStorage.LoadEmulatorConfigs();
            using (var form = new SaveSyncEmulatorsForm(configs))
            {
                form.ShowDialog();
            }
        }
    }
}
