using SaveSyncPlugin.Core.Storage;
using SaveSyncPlugin.MenuItems;
using SaveSyncPlugin.UI.Forms;
using Unbroken.LaunchBox.Plugins;

namespace SaveSyncPlugin.MenuItems.Buttons
{
    public class SaveSyncSettingsMenuItem : SaveSyncMenuItem, ISystemMenuItemPlugin
    {
        public override string Caption => "SaveSync: Settings";

        public override void OnSelected()
        {
            using (var form = new SaveSyncSettingsForm())
            {
                form.ShowDialog();
            }
        }
    }
}
