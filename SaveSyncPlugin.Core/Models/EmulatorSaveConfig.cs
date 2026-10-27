using System.Collections.Generic;

namespace SaveSyncPlugin.Core.Models
{
    public class EmulatorSaveConfig
    {
        public string EmulatorId { get; set; }
        public string Title { get; set; }
        public string FolderName { get; set; }

        /// <summary>
        /// Base folder used for the LOCAL side of the sync, relative to the LaunchBox
        /// folder (or absolute). Leave empty to use the folder of the emulator's
        /// application — which is the wrapper folder when the emulator is launched
        /// through a wrapper, so <see cref="RelativePaths"/> would have to escape it.
        /// </summary>
        public string LocalBase { get; set; }

        public List<string> RelativePaths { get; set; } = new List<string>();

        public override string ToString()
        {
            return string.IsNullOrEmpty(Title) ? "(unknown)" : Title;
        }
    }
}
