using System;
using System.IO;
using Newtonsoft.Json;
using SaveSyncPlugin.Core.Models;
using Xunit;

namespace SaveSyncPlugin.Tests
{
    public class SettingsBehaviorTests
    {
        [Fact]
        public void AlwaysKeepNewer_DefaultTrue()
        {
            var settings = new SaveSyncSettings();

            Assert.True(settings.AlwaysKeepNewer);
        }

        [Fact]
        public void AskOnConflict_DefaultTrue()
        {
            var settings = new SaveSyncSettings();

            Assert.True(settings.AskOnConflict);
        }

        [Fact]
        public void SyncOnStartup_DefaultTrue()
        {
            var settings = new SaveSyncSettings();

            Assert.True(settings.SyncOnStartup);
        }

        [Fact]
        public void SyncOnGameClose_DefaultTrue()
        {
            var settings = new SaveSyncSettings();

            Assert.True(settings.SyncOnGameClose);
        }

        [Fact]
        public void RobocopyThreads_Default8()
        {
            var settings = new SaveSyncSettings();

            Assert.Equal(8, settings.RobocopyThreads);
        }

        [Fact]
        public void RobocopyRetries_Default3()
        {
            var settings = new SaveSyncSettings();

            Assert.Equal(3, settings.RobocopyRetries);
        }

        [Fact]
        public void RobocopyWaitSeconds_Default10()
        {
            var settings = new SaveSyncSettings();

            Assert.Equal(10, settings.RobocopyWaitSeconds);
        }

        [Fact]
        public void Serialization_Roundtrip_PreservesValues()
        {
            var settings = new SaveSyncSettings
            {
                ServerBasePath = @"\\server\saves",
                SyncOnStartup = false,
                SyncOnGameClose = false,
                AlwaysKeepNewer = false,
                AskOnConflict = false,
                RobocopyThreads = 16,
                RobocopyRetries = 5,
                RobocopyWaitSeconds = 20
            };

            var json = JsonConvert.SerializeObject(settings);
            var deserialized = JsonConvert.DeserializeObject<SaveSyncSettings>(json);

            Assert.Equal(@"\\server\saves", deserialized.ServerBasePath);
            Assert.False(deserialized.SyncOnStartup);
            Assert.False(deserialized.SyncOnGameClose);
            Assert.False(deserialized.AlwaysKeepNewer);
            Assert.False(deserialized.AskOnConflict);
            Assert.Equal(16, deserialized.RobocopyThreads);
            Assert.Equal(5, deserialized.RobocopyRetries);
            Assert.Equal(20, deserialized.RobocopyWaitSeconds);
        }

        [Fact]
        public void Serialization_DeserializesFromJson()
        {
            var json = @"{
                ""ServerBasePath"": ""C:\\saves"",
                ""SyncOnStartup"": true,
                ""SyncOnGameClose"": true,
                ""AlwaysKeepNewer"": true,
                ""AskOnConflict"": true,
                ""RobocopyThreads"": 8,
                ""RobocopyRetries"": 3,
                ""RobocopyWaitSeconds"": 10
            }";

            var settings = JsonConvert.DeserializeObject<SaveSyncSettings>(json);

            Assert.Equal(@"C:\saves", settings.ServerBasePath);
            Assert.True(settings.AlwaysKeepNewer);
            Assert.True(settings.AskOnConflict);
        }

        [Fact]
        public void Serialization_NullJson_ReturnsNull()
        {
            var settings = JsonConvert.DeserializeObject<SaveSyncSettings>("null");

            Assert.Null(settings);
        }
    }
}
