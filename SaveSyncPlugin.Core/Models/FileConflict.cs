using System;

namespace SaveSyncPlugin.Core.Models
{
    public class FileConflict
    {
        public string LocalPath { get; set; }
        public string ServerPath { get; set; }
        public DateTime LocalModified { get; set; }
        public DateTime ServerModified { get; set; }
        public long LocalSize { get; set; }
        public long ServerSize { get; set; }
        public bool LocalIsNewer => LocalModified > ServerModified;
    }
}
