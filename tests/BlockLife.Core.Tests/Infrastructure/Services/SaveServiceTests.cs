using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Domain.Save;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Core.Infrastructure.Services.Migrations;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Tests.Infrastructure.Services
{
    public class SaveServiceTests
    {
        private readonly Mock<ILogger<SaveService>> _loggerMock;
        private readonly List<ISaveMigration> _migrations;
        private readonly SaveService _saveService;

        public SaveServiceTests()
        {
            _loggerMock = new Mock<ILogger<SaveService>>();
            _migrations = new List<ISaveMigration> { new ExampleV0ToV1Migration() };
            _saveService = new SaveService(_loggerMock.Object, _migrations);
        }

        [Fact]
        public async Task SaveAsync_WithValidData_ShouldSucceed()
        {
            // Arrange
            var saveData = CreateTestSaveData();

            // Act
            var result = await _saveService.SaveAsync(saveData, slot: 99);

            // Assert
            result.IsSucc.Should().BeTrue();
        }

        [Fact]
        public async Task SaveAsync_AlwaysSavesInCurrentVersion()
        {
            // Arrange
            var oldVersionData = CreateTestSaveData() with { Version = 0 };

            // Act
            await _saveService.SaveAsync(oldVersionData, slot: 98);
            var loaded = await _saveService.LoadAsync(slot: 98);

            // Assert
            loaded.IsSucc.Should().BeTrue();
            loaded.Match(
                Succ: data => data.Version.Should().Be(SaveData.CURRENT_VERSION),
                Fail: _ => throw new Exception("Load should have succeeded")
            );

            // Cleanup
            await _saveService.DeleteSaveAsync(slot: 98);
        }

        [Fact]
        public async Task LoadAsync_WithNonExistentSlot_ShouldReturnError()
        {
            // Act
            var result = await _saveService.LoadAsync(slot: 999);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => throw new Exception("Should have failed"),
                Fail: error => error.Message.Should().Contain("No save found")
            );
        }

        [Fact]
        public void LoadAsync_WithOldVersion_ShouldMigrate()
        {
            // Arrange
            var oldSaveData = CreateTestSaveData() with { Version = 0 };
            
            // Simulate saving old version by using export/import
            var exported = _saveService.ExportSave(oldSaveData);
            var modifiedExport = exported.Replace("\"version\": 1", "\"version\": 0");
            
            // Act
            var imported = _saveService.ImportSave(modifiedExport);

            // Assert
            imported.IsSucc.Should().BeTrue();
            imported.Match(
                Succ: data =>
                {
                    data.Version.Should().Be(SaveData.CURRENT_VERSION);
                    data.Blocks.Should().BeEquivalentTo(oldSaveData.Blocks);
                    data.Settings.Should().NotBeNull();
                    data.Metadata.Should().NotBeNull();
                },
                Fail: _ => throw new Exception("Import should have succeeded")
            );
        }

        [Fact]
        public async Task SaveExistsAsync_AfterSave_ShouldReturnTrue()
        {
            // Arrange
            var saveData = CreateTestSaveData();
            await _saveService.SaveAsync(saveData, slot: 97);

            // Act
            var exists = await _saveService.SaveExistsAsync(slot: 97);

            // Assert
            exists.Should().BeTrue();

            // Cleanup
            await _saveService.DeleteSaveAsync(slot: 97);
        }

        [Fact]
        public async Task DeleteSaveAsync_ShouldRemoveSave()
        {
            // Arrange
            var saveData = CreateTestSaveData();
            await _saveService.SaveAsync(saveData, slot: 96);

            // Act
            var deleteResult = await _saveService.DeleteSaveAsync(slot: 96);
            var exists = await _saveService.SaveExistsAsync(slot: 96);

            // Assert
            deleteResult.IsSucc.Should().BeTrue();
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task GetSaveInfoAsync_WithExistingSave_ShouldReturnInfo()
        {
            // Arrange
            var saveData = CreateTestSaveData();
            await _saveService.SaveAsync(saveData, slot: 95);

            // Act
            var info = await _saveService.GetSaveInfoAsync(slot: 95);

            // Assert
            info.IsSome.Should().BeTrue();
            info.Match(
                Some: saveInfo =>
                {
                    saveInfo.Slot.Should().Be(95);
                    saveInfo.Version.Should().Be(SaveData.CURRENT_VERSION);
                    saveInfo.CurrentTurn.Should().Be(saveData.CurrentTurn);
                    saveInfo.TotalScore.Should().Be(saveData.TotalScore);
                    saveInfo.NeedsMigration.Should().BeFalse();
                },
                None: () => throw new Exception("Should have returned info")
            );

            // Cleanup
            await _saveService.DeleteSaveAsync(slot: 95);
        }

        [Fact]
        public void ExportSave_ShouldSerializeToJson()
        {
            // Arrange
            var saveData = CreateTestSaveData();

            // Act
            var exported = _saveService.ExportSave(saveData);

            // Assert
            exported.Should().NotBeNullOrEmpty();
            exported.Should().Contain("\"version\"");
            exported.Should().Contain("\"blocks\"");
            exported.Should().Contain("\"currentTurn\"");
        }

        [Fact]
        public void ImportSave_WithValidJson_ShouldDeserialize()
        {
            // Arrange
            var original = CreateTestSaveData();
            var json = _saveService.ExportSave(original);

            // Act
            var imported = _saveService.ImportSave(json);

            // Assert
            imported.IsSucc.Should().BeTrue();
            imported.Match(
                Succ: data =>
                {
                    data.Version.Should().Be(SaveData.CURRENT_VERSION);
                    data.CurrentTurn.Should().Be(original.CurrentTurn);
                    data.TotalScore.Should().Be(original.TotalScore);
                    data.Blocks.Count.Should().Be(original.Blocks.Count);
                },
                Fail: _ => throw new Exception("Import should have succeeded")
            );
        }

        [Fact]
        public void ImportSave_WithInvalidJson_ShouldReturnError()
        {
            // Arrange
            var invalidJson = "{ this is not valid json }";

            // Act
            var result = _saveService.ImportSave(invalidJson);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => throw new Exception("Should have failed"),
                Fail: error => error.Message.Should().Contain("Failed to import")
            );
        }

        [Fact]
        public async Task LoadAsync_UpdatesLoadCount()
        {
            // Arrange
            var saveData = CreateTestSaveData();
            await _saveService.SaveAsync(saveData, slot: 94);

            // Act
            var firstLoad = await _saveService.LoadAsync(slot: 94);
            await _saveService.SaveAsync(
                firstLoad.Match(Succ: d => d, Fail: _ => saveData), 
                slot: 94);
            var secondLoad = await _saveService.LoadAsync(slot: 94);

            // Assert
            firstLoad.IsSucc.Should().BeTrue();
            secondLoad.IsSucc.Should().BeTrue();
            
            secondLoad.Match(
                Succ: data => data.Metadata.LoadCount.Should().Be(2),
                Fail: _ => throw new Exception("Load should have succeeded")
            );

            // Cleanup
            await _saveService.DeleteSaveAsync(slot: 94);
        }

        [Fact]
        public void Migration_FromV0ToV1_ShouldAddDefaults()
        {
            // Arrange
            var migration = new ExampleV0ToV1Migration();
            var oldData = new SaveData
            {
                Version = 0,
                CurrentTurn = 42,
                TotalScore = 1337,
                Blocks = new List<Block>(),
                Settings = null!, // Simulate missing settings
                Metadata = null!  // Simulate missing metadata
            };

            // Act
            var result = migration.Migrate(oldData);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: data =>
                {
                    data.Version.Should().Be(1);
                    data.CurrentTurn.Should().Be(42); // Preserved
                    data.TotalScore.Should().Be(1337); // Preserved
                    data.Settings.Should().NotBeNull();
                    data.Settings.Difficulty.Should().Be(1);
                    data.Metadata.Should().NotBeNull();
                    data.Metadata.GameVersion.Should().Be("0.9.0");
                },
                Fail: _ => throw new Exception("Migration should have succeeded")
            );
        }

        private static SaveData CreateTestSaveData()
        {
            var blocks = new List<Block>
            {
                Block.CreateNew(BlockType.Work, new Vector2Int(0, 0)),
                Block.CreateNew(BlockType.Study, new Vector2Int(1, 0)),
                Block.CreateNew(BlockType.Health, new Vector2Int(2, 0))
            };

            return new SaveData
            {
                Version = SaveData.CURRENT_VERSION,
                CurrentTurn = 10,
                TotalScore = 500,
                Blocks = blocks,
                GridDimensions = new Vector2Int(10, 10),
                Resources = new Dictionary<string, int>
                {
                    ["Money"] = 100,
                    ["SocialCapital"] = 50
                },
                Attributes = new Dictionary<string, int>
                {
                    ["Knowledge"] = 75,
                    ["Health"] = 80,
                    ["Happiness"] = 60
                },
                Settings = new GameSettings
                {
                    Difficulty = 1,
                    AutoSpawnEnabled = true,
                    SpawnRate = 1
                },
                Metadata = new SaveMetadata
                {
                    GameVersion = "1.0.0",
                    PlayTimeSeconds = 3600,
                    LoadCount = 0
                }
            };
        }
    }
}