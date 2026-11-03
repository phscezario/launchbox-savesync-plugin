using System.IO;
using SaveSyncPlugin.Core.Sync;
using Xunit;

namespace SaveSyncPlugin.Tests
{
    /// <summary>
    /// Regression tests for the path bug: the same relative path was applied to the
    /// local base (the wrapper folder) and to the server base, so "..\..\.." escaped
    /// SavesBackup and the "backup" landed in C:\Drive\Emuladores instead.
    /// </summary>
    public class PathResolverTests
    {
        private const string LaunchBoxRoot = @"D:\Jogos\LaunchBox";
        private const string ServerRoot = @"C:\Drive\Jogos\SavesBackup";
        private const string Wrapper = @"..\Apps\EmulatorExtensions\psvita\Vita3KWrapper.exe";

        // ---------------- local side ----------------

        [Fact]
        public void ResolveLocal_WithEmulatorDirectory_IgnoresWrapperFolder()
        {
            var local = PathResolver.ResolveLocal(
                LaunchBoxRoot,
                Wrapper,
                @"..\Emuladores\PSVita\portable\fs\ux0\user\00",
                "savedata");

            Assert.Equal(
                @"D:\Jogos\Emuladores\PSVita\portable\fs\ux0\user\00\savedata",
                local);
        }

        [Fact]
        public void ResolveLocal_WithoutEmulatorDirectory_UsesApplicationFolder()
        {
            var local = PathResolver.ResolveLocal(LaunchBoxRoot, Wrapper, null, "savedata");

            Assert.Equal(@"D:\Jogos\Apps\EmulatorExtensions\psvita\savedata", local);
        }

        [Fact]
        public void ResolveLocal_IsAbsoluteEvenWhenInputIsRelative()
        {
            var local = PathResolver.ResolveLocal(LaunchBoxRoot, Wrapper, null, "savedata");

            Assert.True(Path.IsPathRooted(local));
        }

        [Fact]
        public void ResolveLocal_WithoutApplicationPath_ReturnsNull()
        {
            Assert.Null(PathResolver.ResolveLocal(LaunchBoxRoot, null, null, "savedata"));
        }

        // ---------------- server side ----------------

        [Fact]
        public void ResolveServer_StaysInsideServerRoot()
        {
            var server = PathResolver.ResolveServer(ServerRoot, "PSVita", "savedata");

            Assert.Equal(@"C:\Drive\Jogos\SavesBackup\PSVita\savedata", server);
        }

        [Fact]
        public void ResolveServer_RefusesPathThatEscapesServerRoot()
        {
            // This is exactly what used to happen: the escaping path was accepted and
            // the files were written to C:\Drive\Emuladores instead of the backup.
            var server = PathResolver.ResolveServer(
                ServerRoot,
                "PSVita",
                @"..\..\..\Emuladores\PSVita\portable\fs\ux0\user\00\savedata");

            Assert.Null(server);
        }

        [Fact]
        public void ResolveServer_WithoutServerPath_ReturnsNull()
        {
            Assert.Null(PathResolver.ResolveServer(null, "PSVita", "savedata"));
            Assert.Null(PathResolver.ResolveServer("", "PSVita", "savedata"));
        }

        [Fact]
        public void ResolveServer_NestedFolder_StaysInsideServerRoot()
        {
            var server = PathResolver.ResolveServer(ServerRoot, "WindowsGames", @"Documents\Korra");

            Assert.Equal(@"C:\Drive\Jogos\SavesBackup\WindowsGames\Documents\Korra", server);
        }

        // ---------------- wrapper detection ----------------

        [Theory]
        [InlineData(@"..\Apps\EmulatorExtensions\psvita\Vita3KWrapper.exe", true)]
        [InlineData(@"..\Emuladores\PSVita\Vita3K.exe", false)]
        [InlineData(null, false)]
        public void UsesWrapper_DetectsWrapperExecutable(string applicationPath, bool expected)
        {
            Assert.Equal(expected, PathResolver.UsesWrapper(applicationPath));
        }

        // ---------------- LaunchBox root discovery ----------------

        [Fact]
        public void FindLaunchBoxRoot_FromPluginFolder_ReturnsInstallRoot()
        {
            var root = CreateTree(out var cleanup);
            try
            {
                var pluginFolder = Path.Combine(root, "Plugins", "SaveSync LaunchBox Integration");

                Assert.Equal(root, PathResolver.FindLaunchBoxRoot(pluginFolder));
            }
            finally { cleanup(); }
        }

        [Fact]
        public void FindLaunchBoxRoot_WithoutExecutable_FallsBackToPluginsParent()
        {
            var root = CreateTree(out var cleanup, createExecutable: false);
            try
            {
                var pluginFolder = Path.Combine(root, "Plugins", "SaveSync LaunchBox Integration");

                Assert.Equal(root, PathResolver.FindLaunchBoxRoot(pluginFolder));
            }
            finally { cleanup(); }
        }

        [Fact]
        public void FindLaunchBoxRoot_FromInstallRoot_ReturnsItself()
        {
            var root = CreateTree(out var cleanup);
            try
            {
                Assert.Equal(root, PathResolver.FindLaunchBoxRoot(root));
            }
            finally { cleanup(); }
        }

        // ---------------- portable paths only ----------------

        [Theory]
        [InlineData(@"C:\Emuladores\PSVita")]
        [InlineData(@"\\servidor\jogos\PSVita")]
        [InlineData(@"\Emuladores\PSVita")]
        public void ResolveLocalBase_RefusesAbsoluteLocalBase(string localBase)
        {
            Assert.Null(PathResolver.ResolveLocalBase(LaunchBoxRoot, Wrapper, localBase));
        }

        [Fact]
        public void ResolveLocal_RefusesAbsoluteRelativePath()
        {
            Assert.Null(PathResolver.ResolveLocal(
                LaunchBoxRoot, Wrapper, @"..\Emuladores\PSVita", @"C:\Saves\savedata"));
        }

        [Fact]
        public void ResolveLocalBase_DefaultMode_ResolvesEmulatorFolderFromRelativeApplicationPath()
        {
            var baseDir = PathResolver.ResolveLocalBase(
                LaunchBoxRoot, @"..\Emuladores\Switch\yuzu.exe", null);

            Assert.Equal(@"D:\Jogos\Emuladores\Switch", baseDir);
        }

        [Fact]
        public void ResolveLocalBase_DefaultMode_NormalizesAbsoluteApplicationPath()
        {
            // LaunchBox may store an absolute ApplicationPath; it must be turned into
            // a relative folder, never passed through as an absolute path.
            var baseDir = PathResolver.ResolveLocalBase(
                LaunchBoxRoot, @"D:\Jogos\Emuladores\Switch\yuzu.exe", null);

            Assert.Equal(@"D:\Jogos\Emuladores\Switch", baseDir);
        }

        [Fact]
        public void ResolveLocalBase_DefaultMode_RefusesApplicationPathOutsideTheTreeOnAnotherDrive()
        {
            Assert.Null(PathResolver.ResolveLocalBase(
                LaunchBoxRoot, @"Z:\Emuladores\Switch\yuzu.exe", null));
        }

        [Fact]
        public void ResolveLocalBase_RefusesHostFolders()
        {
            // The plugin folder is not an emulator folder; syncing it would copy the
            // plugin (and its logs) instead of saves.
            Assert.Null(PathResolver.ResolveLocalBase(
                LaunchBoxRoot, Wrapper, @"Plugins\SaveSync LaunchBox Integration"));
            Assert.Null(PathResolver.ResolveLocalBase(
                LaunchBoxRoot, Wrapper, @"Core"));
        }

        [Fact]
        public void ResolveLocal_RefusesPathThatEscapesIntoHostFolders()
        {
            Assert.Null(PathResolver.ResolveLocal(
                LaunchBoxRoot,
                @"..\Emuladores\Switch\yuzu.exe",
                @"..\Emuladores\Switch",
                @"..\..\LaunchBox\Core\saves"));
        }

        [Fact]
        public void ShouldSkipWrapperSync_OnlyWhenWrapperWithoutLocalBase()
        {
            Assert.True(PathResolver.ShouldSkipWrapperSync(Wrapper, null, 1));
            Assert.True(PathResolver.ShouldSkipWrapperSync(Wrapper, "  ", 3));
            Assert.False(PathResolver.ShouldSkipWrapperSync(Wrapper, @"..\Emuladores\PSVita", 1));
            Assert.False(PathResolver.ShouldSkipWrapperSync(@"..\Emuladores\Switch\yuzu.exe", null, 1));
            Assert.False(PathResolver.ShouldSkipWrapperSync(Wrapper, null, 0));
        }

        // ---------------- per-game local base ----------------

        [Fact]
        public void ResolveGameLocalBase_Empty_UsesExecutableFolder()
        {
            var baseDir = PathResolver.ResolveGameLocalBase(
                LaunchBoxRoot, @"..\Roms\romm\roms\WindowsGames\JUMP FORCE\JUMP_FORCE.exe", null);

            Assert.Equal(@"D:\Jogos\Roms\romm\roms\WindowsGames\JUMP FORCE", baseDir);
        }

        [Fact]
        public void ResolveGameLocalBase_ParentOfExecutableFolder_IsAllowed()
        {
            // StepMania - OutFox: the exe lives in Program\, the save in ..\Save.
            var baseDir = PathResolver.ResolveGameLocalBase(
                LaunchBoxRoot,
                @"..\Roms\romm\roms\WindowsGameEngine\StepMania - OutFox\Program\OutFox.exe",
                "..");

            Assert.Equal(@"D:\Jogos\Roms\romm\roms\WindowsGameEngine\StepMania - OutFox", baseDir);
        }

        [Theory]
        [InlineData(@"C:\Saves")]
        [InlineData(@"\\servidor\saves")]
        [InlineData(@"\Saves")]
        public void ResolveGameLocalBase_RefusesAbsoluteLocalBase(string localBase)
        {
            Assert.Null(PathResolver.ResolveGameLocalBase(
                LaunchBoxRoot, @"..\Roms\romm\roms\WindowsGames\JUMP FORCE\JUMP_FORCE.exe", localBase));
        }

        [Fact]
        public void ResolveGameLocalBase_RefusesHostFolders()
        {
            Assert.Null(PathResolver.ResolveGameLocalBase(
                LaunchBoxRoot,
                @"..\Roms\romm\roms\WindowsGames\JUMP FORCE\JUMP_FORCE.exe",
                @"..\..\..\..\..\LaunchBox\Plugins"));
        }

        [Fact]
        public void ResolveGameLocalBase_WithoutApplicationPath_ReturnsNull()
        {
            Assert.Null(PathResolver.ResolveGameLocalBase(LaunchBoxRoot, null, null));
        }

        // ---------------- relativize ----------------

        [Fact]
        public void Relativize_ReturnsPathRelativeToLaunchBoxRoot()
        {
            Assert.Equal(@"..\Emuladores\Switch",
                PathResolver.Relativize(LaunchBoxRoot, @"D:\Jogos\Emuladores\Switch"));
        }

        [Fact]
        public void Relativize_ReturnsNullForTheRootItself()
        {
            Assert.Null(PathResolver.Relativize(LaunchBoxRoot, LaunchBoxRoot));
        }

        [Fact]
        public void Relativize_ReturnsNullForAnotherDrive()
        {
            Assert.Null(PathResolver.Relativize(LaunchBoxRoot, @"Z:\Emuladores\Switch"));
        }

        // ---------------- server base ----------------

        [Fact]
        public void ResolveServer_RefusesAbsoluteRelativePath()
        {
            Assert.Null(PathResolver.ResolveServer(ServerRoot, "PSVita", @"C:\Saves\savedata"));
        }

        [Fact]
        public void ResolveServer_RelativeServerBase_IsRootedAndKeepsTheFolderLayout()
        {
            // A relative ServerBasePath is resolved against the LaunchBox folder, never
            // against the process working directory.
            var server = PathResolver.ResolveServer(@"..\SavesBackup", "PSVita", "savedata");

            Assert.NotNull(server);
            Assert.True(Path.IsPathRooted(server));
            Assert.EndsWith(
                Path.Combine("SavesBackup", "PSVita", "savedata"),
                server);
        }

        private static string CreateTree(out Action cleanup, bool createExecutable = true)
        {
            var root = Path.Combine(Path.GetTempPath(), "savesync-tests", Guid.NewGuid().ToString("N"));
            var pluginFolder = Path.Combine(root, "Plugins", "SaveSync LaunchBox Integration");
            Directory.CreateDirectory(pluginFolder);

            if (createExecutable)
                File.WriteAllText(Path.Combine(root, "LaunchBox.exe"), string.Empty);

            cleanup = () =>
            {
                try { Directory.Delete(root, true); } catch { }
            };

            return root;
        }
    }
}
