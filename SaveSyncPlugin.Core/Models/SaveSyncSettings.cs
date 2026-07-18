namespace SaveSyncPlugin.Core.Models
{
    public class SaveSyncSettings
    {
        public string ServerBasePath { get; set; }
        public bool SyncOnStartup { get; set; } = true;
        public bool SyncOnGameClose { get; set; } = true;
        public int RobocopyThreads { get; set; } = 8;
        public int RobocopyRetries { get; set; } = 3;
        public int RobocopyWaitSeconds { get; set; } = 10;
        public bool AlwaysKeepNewer { get; set; } = true;
        public bool AskOnConflict { get; set; } = true;
    }
}
