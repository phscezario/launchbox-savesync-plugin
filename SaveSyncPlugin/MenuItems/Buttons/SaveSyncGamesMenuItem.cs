using SaveSyncPlugin.Core.Storage;
using SaveSyncPlugin.MenuItems;
using SaveSyncPlugin.UI.Forms;
using Unbroken.LaunchBox.Plugins;

namespace SaveSyncPlugin.MenuItems.Buttons
{
    public class SaveSyncGamesMenuItem : SaveSyncMenuItem, ISystemMenuItemPlugin
    {
        public override string Caption => "SaveSync: Games";

        public override void OnSelected()
        {
            var configs = SaveSyncStorage.LoadGameConfigs();
            using (var form = new SaveSyncGamesForm(configs))
            {
                form.ShowDialog();
            }
        }
    }
}
