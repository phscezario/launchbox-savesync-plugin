using System;
using System.IO;
using SaveSyncPlugin.Core.Models;
using SaveSyncPlugin.Core.Sync;
using Xunit;

namespace SaveSyncPlugin.Tests
{
    /// <summary>
    /// Regression tests for the data-loss bug: the sync used to pass /PURGE on every
    /// robocopy call, so a save that existed on only one side was deleted by the other
    /// side. These tests pin the non-destructive default and the mirror-mode guards.
    /// </summary>
    public class RobocopyRunnerTests : IDisposable
    {
        private readonly string _root;

        public RobocopyRunnerTests()
        {
            _root = Path.Combine(Path.GetTempPath(), "savesync-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        public void Dispose()
        {
            try { Directory.Delete(_root, true); } catch { }
        }

        // ---------------- command line ----------------

        [Fact]
        public void BuildArguments_DefaultIsNonDestructive()
        {
            var args = RobocopyRunner.BuildArguments(@"C:\a", @"C:\b", null, new SaveSyncSettings());

            Assert.Contains("/E", args);
            Assert.Contains("/XO", args);
            Assert.DoesNotContain("/PURGE", args);
        }

        [Fact]
        public void BuildArguments_MirrorModeAddsPurge()
        {
            var settings = new SaveSyncSettings { DeleteExtraneous = true };

            var args = RobocopyRunner.BuildArguments(@"C:\a", @"C:\b", null, settings);

            Assert.Contains("/PURGE", args);
        }

        [Fact]
        public void BuildArguments_IncludesFileFilter()
        {
            var args = RobocopyRunner.BuildArguments(@"C:\a", @"C:\b", @"files\xbox_hdd.qcow2", new SaveSyncSettings());

            Assert.Contains("\"files\\xbox_hdd.qcow2\"", args);
        }

        // ---------------- real robocopy runs ----------------

        [Fact]
        public void Run_DoesNotDeleteFileThatExistsOnlyLocally()
        {
            // The original bug: startup "download" purged every local file that the
            // server did not have yet (offline play, power loss, new save slot).
            var server = Dir("server");
            var local = Dir("local");
            WriteFile(server, "from-server.sav", "server", DateTime.UtcNow.AddHours(-1));
            WriteFile(local, "only-local.sav", "local", DateTime.UtcNow.AddHours(-1));

            var result = RobocopyRunner.Run(server, local, null, new SaveSyncSettings());

            Assert.Equal(RobocopyOutcome.Success, result.Outcome);
            Assert.True(File.Exists(Path.Combine(local, "only-local.sav")));
            Assert.True(File.Exists(Path.Combine(local, "from-server.sav")));
        }

        [Fact]
        public void Run_KeepsNewerDestinationFile()
        {
            var server = Dir("server");
            var local = Dir("local");
            WriteFile(server, "slot.sav", "old", DateTime.UtcNow.AddHours(-2));
            WriteFile(local, "slot.sav", "new", DateTime.UtcNow.AddHours(-1));

            RobocopyRunner.Run(server, local, null, new SaveSyncSettings());

            Assert.Equal("new", File.ReadAllText(Path.Combine(local, "slot.sav")));
            Assert.Equal("old", File.ReadAllText(Path.Combine(server, "slot.sav")));
        }

        [Fact]
        public void Run_UploadKeepsNewerServerFile()
        {
            var server = Dir("server");
            var local = Dir("local");
            WriteFile(server, "slot.sav", "server-new", DateTime.UtcNow.AddHours(-1));
            WriteFile(local, "slot.sav", "local-old", DateTime.UtcNow.AddHours(-2));

            RobocopyRunner.Run(local, server, null, new SaveSyncSettings());

            Assert.Equal("server-new", File.ReadAllText(Path.Combine(server, "slot.sav")));
        }

        [Fact]
        public void Run_MirrorModeWithEmptySourceIsRefused()
        {
            // /PURGE with an empty source (server not synced / drive offline) would
            // erase every local save. The guard must skip the copy instead.
            var server = Dir("server");
            var local = Dir("local");
            WriteFile(local, "precious.sav", "data", DateTime.UtcNow);

            var result = RobocopyRunner.Run(server, local, null, new SaveSyncSettings { DeleteExtraneous = true });

            Assert.Equal(RobocopyOutcome.Skipped, result.Outcome);
            Assert.True(File.Exists(Path.Combine(local, "precious.sav")));
        }

        [Fact]
        public void Run_MirrorModeWithContentDeletesExtraneous()
        {
            // Opt-in behaviour: mirror mode is allowed when the source really has files.
            var server = Dir("server");
            var local = Dir("local");
            WriteFile(server, "kept.sav", "server", DateTime.UtcNow.AddHours(-1));
            WriteFile(local, "kept.sav", "server", DateTime.UtcNow.AddHours(-1));
            WriteFile(local, "extra.sav", "extra", DateTime.UtcNow.AddHours(-1));

            var result = RobocopyRunner.Run(server, local, null, new SaveSyncSettings { DeleteExtraneous = true });

            Assert.Equal(RobocopyOutcome.Success, result.Outcome);
            Assert.True(File.Exists(Path.Combine(local, "kept.sav")));
            Assert.False(File.Exists(Path.Combine(local, "extra.sav")));
        }

        [Fact]
        public void Run_MissingSourceIsSkipped()
        {
            var local = Dir("local");
            WriteFile(local, "save.sav", "data", DateTime.UtcNow);

            var result = RobocopyRunner.Run(Path.Combine(_root, "does-not-exist"), local, null, new SaveSyncSettings());

            Assert.Equal(RobocopyOutcome.Skipped, result.Outcome);
            Assert.True(File.Exists(Path.Combine(local, "save.sav")));
        }

        // ---------------- helpers ----------------

        private string Dir(string name)
        {
            var path = Path.Combine(_root, name);
            Directory.CreateDirectory(path);
            return path;
        }

        private static void WriteFile(string dir, string name, string content, DateTime lastWriteUtc)
        {
            var path = Path.Combine(dir, name);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
            File.SetLastWriteTimeUtc(path, lastWriteUtc);
        }
    }
}
