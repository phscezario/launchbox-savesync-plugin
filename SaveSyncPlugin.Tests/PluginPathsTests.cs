using System;
using System.IO;
using SaveSyncPlugin.Core.Sync;
using Xunit;

namespace SaveSyncPlugin.Tests
{
    /// <summary>
    /// The v0.0.3 bug: the install root was taken from
    /// <c>AppDomain.BaseDirectory</c>, which is <c>&lt;root&gt;\Core</c> in
    /// LaunchBox 14. <c>..\Emuladores</c> then resolved to
    /// <c>&lt;root&gt;\Emuladores</c> instead of the real emulator folder.
    ///
    /// These tests pin the root discovery: the install folder is the one that
    /// contains <c>Plugins</c> (plus <c>Core</c> or a legacy <c>LaunchBox.exe</c>)
    /// or <c>Data\Settings.xml</c>, and it is never a folder called <c>Core</c> or
    /// <c>Plugins</c>.
    /// </summary>
    public class PluginPathsTests
    {
        // ---------------- root discovery ----------------

        [Fact]
        public void FindRoot_FromCoreHostDirectory_ReturnsInstallRoot()
        {
            var root = CreateTree(out var cleanup);
            try
            {
                var host = Path.Combine(root, "Core");
                Directory.CreateDirectory(host);
                File.WriteAllText(Path.Combine(host, "LaunchBox.exe"), string.Empty);

                Assert.Equal(root, PluginPaths.FindRoot(host));
            }
            finally { cleanup(); }
        }

        [Fact]
        public void FindRoot_FromPluginFolder_ReturnsInstallRoot()
        {
            var root = CreateTree(out var cleanup);
            try
            {
                var pluginFolder = Path.Combine(root, "Plugins", PluginPaths.PluginFolderName);

                Assert.Equal(root, PluginPaths.FindRoot(pluginFolder));
            }
            finally { cleanup(); }
        }

        [Fact]
        public void FindRoot_WithDataSettingsXml_ReturnsInstallRoot()
        {
            var root = CreateTree(out var cleanup, withPlugins: false);
            try
            {
                Directory.CreateDirectory(Path.Combine(root, "Data"));
                File.WriteAllText(Path.Combine(root, "Data", "Settings.xml"), "<Settings />");

                Assert.Equal(root, PluginPaths.FindRoot(Path.Combine(root, "Data")));
            }
            finally { cleanup(); }
        }

        [Fact]
        public void FindRoot_WithoutMarkers_NeverReturnsCoreFolder()
        {
            var root = CreateTree(out var cleanup, withPlugins: false);
            try
            {
                var core = Path.Combine(root, "Core");
                Directory.CreateDirectory(core);

                var found = PluginPaths.FindRoot(core);

                Assert.NotEqual(core, found);
                Assert.Equal(root, found);
            }
            finally { cleanup(); }
        }

        [Fact]
        public void IsLaunchBoxRoot_RefusesFolderNamedCore()
        {
            var root = CreateTree(out var cleanup);
            try
            {
                var core = Path.Combine(root, "Core");
                Directory.CreateDirectory(core);

                // A folder called Core that happens to contain a Plugins folder is
                // still not an install: it is the managed host folder.
                Directory.CreateDirectory(Path.Combine(core, "Plugins"));

                Assert.False(PluginPaths.IsLaunchBoxRoot(core));
                Assert.True(PluginPaths.IsLaunchBoxRoot(root));
            }
            finally { cleanup(); }
        }

        [Fact]
        public void IsLaunchBoxRoot_RefusesFolderNamedPlugins()
        {
            var root = CreateTree(out var cleanup);
            try
            {
                Assert.False(PluginPaths.IsLaunchBoxRoot(Path.Combine(root, "Plugins")));
            }
            finally { cleanup(); }
        }

        [Fact]
        public void IsLaunchBoxRoot_RefusesRandomFolder()
        {
            var root = CreateTree(out var cleanup, withPlugins: false);
            try
            {
                Assert.False(PluginPaths.IsLaunchBoxRoot(root));
            }
            finally { cleanup(); }
        }

        // ---------------- plugin folder ----------------

        [Fact]
        public void ResolvePluginFolder_PrefersAssemblyFolder()
        {
            var root = CreateTree(out var cleanup);
            try
            {
                var pluginFolder = Path.Combine(root, "Plugins", PluginPaths.PluginFolderName);
                var assembly = Path.Combine(pluginFolder, "SaveSyncPlugin.Core.dll");

                Assert.Equal(pluginFolder, PluginPaths.ResolvePluginFolder(assembly, Path.Combine(root, "Core")));
            }
            finally { cleanup(); }
        }

        [Fact]
        public void ResolvePluginFolder_FromCoreBaseDirectory_FindsPluginsFolder()
        {
            var root = CreateTree(out var cleanup);
            try
            {
                var expected = Path.Combine(root, "Plugins", PluginPaths.PluginFolderName);

                // This is the LaunchBox 14 case: no assembly location, base directory
                // is <root>\Core.
                Assert.Equal(expected, PluginPaths.ResolvePluginFolder(null, Path.Combine(root, "Core")));
            }
            finally { cleanup(); }
        }

        [Fact]
        public void ResolvePluginFolder_FromInstallBaseDirectory_FindsPluginsFolder()
        {
            var root = CreateTree(out var cleanup);
            try
            {
                var expected = Path.Combine(root, "Plugins", PluginPaths.PluginFolderName);

                Assert.Equal(expected, PluginPaths.ResolvePluginFolder(null, root));
            }
            finally { cleanup(); }
        }

        [Fact]
        public void Describe_MentionsRootAndVersion()
        {
            var text = PluginPaths.Describe();

            Assert.StartsWith("ROOT", text);
            Assert.Contains("base=", text);
            Assert.Contains("asm=", text);
        }

        // ---------------- helper ----------------

        private static string CreateTree(out Action cleanup, bool withPlugins = true)
        {
            var root = Path.Combine(Path.GetTempPath(), "savesync-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            if (withPlugins)
                Directory.CreateDirectory(Path.Combine(root, "Plugins", PluginPaths.PluginFolderName));

            cleanup = () =>
            {
                try { Directory.Delete(root, true); } catch { }
            };

            return root;
        }
    }
}
