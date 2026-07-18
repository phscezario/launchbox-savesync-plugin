using System;
using System.IO;
using SaveSyncPlugin.Core.Models;
using SaveSyncPlugin.Services;
using Xunit;

namespace SaveSyncPlugin.Tests
{
    public class DetectConflictTests : IDisposable
    {
        private readonly string _testDir;
        private readonly SaveSyncService _service;

        public DetectConflictTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "SaveSyncTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
            _service = new SaveSyncService();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        [Fact]
        public void NoConflict_WhenBothDirsEmpty()
        {
            var source = Path.Combine(_testDir, "source");
            var dest = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(source);
            Directory.CreateDirectory(dest);

            var result = _service.DetectConflict(source, dest, null);

            Assert.Null(result);
        }

        [Fact]
        public void NoConflict_WhenSameTimestamps()
        {
            var source = Path.Combine(_testDir, "source");
            var dest = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(source);
            Directory.CreateDirectory(dest);

            var file = "save.sav";
            File.WriteAllText(Path.Combine(source, file), "content");
            File.Copy(Path.Combine(source, file), Path.Combine(dest, file));

            var result = _service.DetectConflict(source, dest, null);

            Assert.Null(result);
        }

        [Fact]
        public void Conflict_WhenSourceIsNewer()
        {
            var source = Path.Combine(_testDir, "source");
            var dest = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(source);
            Directory.CreateDirectory(dest);

            var file = "save.sav";
            File.WriteAllText(Path.Combine(dest, file), "old content");
            File.SetLastWriteTime(Path.Combine(dest, file), new DateTime(2020, 1, 1));

            File.WriteAllText(Path.Combine(source, file), "new content");
            File.SetLastWriteTime(Path.Combine(source, file), new DateTime(2025, 1, 1));

            var result = _service.DetectConflict(source, dest, null);

            Assert.NotNull(result);
            Assert.True(result.LocalIsNewer);
            Assert.Equal(Path.Combine(source, file), result.LocalPath);
            Assert.Equal(Path.Combine(dest, file), result.ServerPath);
        }

        [Fact]
        public void Conflict_WhenDestIsNewer()
        {
            var source = Path.Combine(_testDir, "source");
            var dest = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(source);
            Directory.CreateDirectory(dest);

            var file = "save.sav";
            File.WriteAllText(Path.Combine(source, file), "old content");
            File.SetLastWriteTime(Path.Combine(source, file), new DateTime(2020, 1, 1));

            File.WriteAllText(Path.Combine(dest, file), "new content");
            File.SetLastWriteTime(Path.Combine(dest, file), new DateTime(2025, 1, 1));

            var result = _service.DetectConflict(source, dest, null);

            Assert.NotNull(result);
            Assert.False(result.LocalIsNewer);
        }

        [Fact]
        public void NoConflict_WhenFileOnlyInSource()
        {
            var source = Path.Combine(_testDir, "source");
            var dest = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(source);
            Directory.CreateDirectory(dest);

            File.WriteAllText(Path.Combine(source, "save.sav"), "content");

            var result = _service.DetectConflict(source, dest, null);

            Assert.Null(result);
        }

        [Fact]
        public void Conflict_WhenFileOnlyInDest()
        {
            var source = Path.Combine(_testDir, "source");
            var dest = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(source);
            Directory.CreateDirectory(dest);

            File.WriteAllText(Path.Combine(dest, "save.sav"), "content");

            var result = _service.DetectConflict(source, dest, null);

            Assert.NotNull(result);
            Assert.False(result.LocalIsNewer);
            Assert.Equal(0, result.LocalSize);
        }

        [Fact]
        public void Conflict_WithFileFilter()
        {
            var source = Path.Combine(_testDir, "source");
            var dest = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(source);
            Directory.CreateDirectory(dest);

            File.WriteAllText(Path.Combine(source, "save.sav"), "new");
            File.SetLastWriteTime(Path.Combine(source, "save.sav"), new DateTime(2025, 1, 1));

            File.WriteAllText(Path.Combine(dest, "save.sav"), "old");
            File.SetLastWriteTime(Path.Combine(dest, "save.sav"), new DateTime(2020, 1, 1));

            File.WriteAllText(Path.Combine(source, "config.ini"), "new");
            File.SetLastWriteTime(Path.Combine(source, "config.ini"), new DateTime(2025, 1, 1));

            File.WriteAllText(Path.Combine(dest, "config.ini"), "old");
            File.SetLastWriteTime(Path.Combine(dest, "config.ini"), new DateTime(2020, 1, 1));

            var result = _service.DetectConflict(source, dest, "*.sav");

            Assert.NotNull(result);
            Assert.Contains("save.sav", result.LocalPath);
        }

        [Fact]
        public void NoConflict_WhenSourceDirNotExists()
        {
            var source = Path.Combine(_testDir, "nonexistent");
            var dest = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(dest);

            var result = _service.DetectConflict(source, dest, null);

            Assert.Null(result);
        }

        [Fact]
        public void NoConflict_WhenDestDirNotExists()
        {
            var source = Path.Combine(_testDir, "source");
            var dest = Path.Combine(_testDir, "nonexistent");
            Directory.CreateDirectory(source);

            var result = _service.DetectConflict(source, dest, null);

            Assert.Null(result);
        }

        [Fact]
        public void Conflict_DetectsCorrectTimestamps()
        {
            var source = Path.Combine(_testDir, "source");
            var dest = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(source);
            Directory.CreateDirectory(dest);

            var file = "save.sav";
            var sourceTime = new DateTime(2025, 7, 17, 14, 30, 0);
            var destTime = new DateTime(2025, 7, 16, 9, 15, 0);

            File.WriteAllText(Path.Combine(source, file), "content");
            File.SetLastWriteTime(Path.Combine(source, file), sourceTime);

            File.WriteAllText(Path.Combine(dest, file), "content");
            File.SetLastWriteTime(Path.Combine(dest, file), destTime);

            var result = _service.DetectConflict(source, dest, null);

            Assert.NotNull(result);
            Assert.Equal(sourceTime, result.LocalModified);
            Assert.Equal(destTime, result.ServerModified);
        }

        [Fact]
        public void Conflict_DetectsCorrectSizes()
        {
            var source = Path.Combine(_testDir, "source");
            var dest = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(source);
            Directory.CreateDirectory(dest);

            var file = "save.sav";
            File.WriteAllText(Path.Combine(source, file), "new content with more data");
            File.SetLastWriteTime(Path.Combine(source, file), new DateTime(2025, 1, 1));

            File.WriteAllText(Path.Combine(dest, file), "old");
            File.SetLastWriteTime(Path.Combine(dest, file), new DateTime(2020, 1, 1));

            var result = _service.DetectConflict(source, dest, null);

            Assert.NotNull(result);
            Assert.True(result.LocalSize > result.ServerSize);
        }
    }
}
