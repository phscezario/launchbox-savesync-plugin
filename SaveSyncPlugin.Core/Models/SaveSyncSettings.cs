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

        /// <summary>
        /// Mirror mode: adds <c>/PURGE</c> to robocopy, so a file that exists on only
        /// one side is DELETED from the other side.
        ///
        /// Default is <c>false</c> (non-destructive): files are copied both ways and
        /// the newest version wins, but nothing is ever deleted. Keep it off unless you
        /// really want the server folder to be an exact mirror.
        /// </summary>
        public bool DeleteExtraneous { get; set; } = false;
    }
}
