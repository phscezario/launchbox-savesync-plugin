using System;
using SaveSyncPlugin.Core.Models;
using Xunit;

namespace SaveSyncPlugin.Tests
{
    public class FileConflictTests
    {
        [Fact]
        public void LocalIsNewer_WhenLocalNewer_ReturnsTrue()
        {
            var conflict = new FileConflict
            {
                LocalModified = new DateTime(2025, 7, 17),
                ServerModified = new DateTime(2025, 7, 16)
            };

            Assert.True(conflict.LocalIsNewer);
        }

        [Fact]
        public void LocalIsNewer_WhenServerNewer_ReturnsFalse()
        {
            var conflict = new FileConflict
            {
                LocalModified = new DateTime(2025, 7, 16),
                ServerModified = new DateTime(2025, 7, 17)
            };

            Assert.False(conflict.LocalIsNewer);
        }

        [Fact]
        public void LocalIsNewer_WhenEqual_ReturnsFalse()
        {
            var conflict = new FileConflict
            {
                LocalModified = new DateTime(2025, 7, 17),
                ServerModified = new DateTime(2025, 7, 17)
            };

            Assert.False(conflict.LocalIsNewer);
        }

        [Fact]
        public void Properties_AreSetCorrectly()
        {
            var conflict = new FileConflict
            {
                LocalPath = @"C:\local\save.sav",
                ServerPath = @"\\server\save.sav",
                LocalModified = new DateTime(2025, 7, 17, 14, 30, 0),
                ServerModified = new DateTime(2025, 7, 16, 9, 15, 0),
                LocalSize = 1024,
                ServerSize = 512
            };

            Assert.Equal(@"C:\local\save.sav", conflict.LocalPath);
            Assert.Equal(@"\\server\save.sav", conflict.ServerPath);
            Assert.Equal(new DateTime(2025, 7, 17, 14, 30, 0), conflict.LocalModified);
            Assert.Equal(new DateTime(2025, 7, 16, 9, 15, 0), conflict.ServerModified);
            Assert.Equal(1024, conflict.LocalSize);
            Assert.Equal(512, conflict.ServerSize);
        }
    }
}
