using System.Collections.Generic;
using Newtonsoft.Json;

namespace SaveSyncPlugin.Core.Models
{
    public class GameSaveConfig
    {
        public string GameId { get; set; }
        public string Title { get; set; }
        public string FolderName { get; set; }
        public string SaveType { get; set; }
        public List<string> RelativePaths { get; set; } = new List<string>();

        /// <summary>
        /// Optional base folder for the local side, relative to the folder of the
        /// game executable. Empty means "the executable's folder" (the default).
        /// Needed by portable games that keep the save one level above the exe
        /// (for example Program\OutFox.exe with Save\ at the game root).
        /// Absolute paths are refused: the install has to stay portable.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LocalBase { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Title) ? "(unknown)" : Title;
        }
    }
}
