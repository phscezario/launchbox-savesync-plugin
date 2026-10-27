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
