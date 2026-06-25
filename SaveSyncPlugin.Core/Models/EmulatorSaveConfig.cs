using System.Collections.Generic;

namespace SaveSyncPlugin.Core.Models
{
    public class EmulatorSaveConfig
    {
        public string EmulatorId { get; set; }
        public string Title { get; set; }
        public string FolderName { get; set; }
        public List<string> RelativePaths { get; set; } = new List<string>();

        public override string ToString()
        {
            return string.IsNullOrEmpty(Title) ? "(unknown)" : Title;
        }
    }
}
