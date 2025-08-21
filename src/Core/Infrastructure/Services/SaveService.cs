using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Save;
using BlockLife.Core.Infrastructure.Logging;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Infrastructure.Services
{
    /// <summary>
    /// Implementation of save/load functionality with automatic version migration.
    /// </summary>
    public sealed class SaveService : ISaveService
    {
        private readonly ILogger<SaveService> _logger;
        private readonly IEnumerable<ISaveMigration> _migrations;
        private readonly JsonSerializerSettings _jsonSettings;
        private readonly string _saveDirectory;

        public SaveService(
            ILogger<SaveService> logger,
            IEnumerable<ISaveMigration> migrations)
        {
            _logger = logger;
            _migrations = migrations.OrderBy(m => m.FromVersion).ToList();

            // Configure JSON serialization with Newtonsoft.Json
            // Handles 'required' properties and domain models better than System.Text.Json
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter { NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy() }
                }
            };

            // Setup save directory
            _saveDirectory = GetSaveDirectory();
            EnsureSaveDirectoryExists();
        }

        public async Task<Fin<Unit>> SaveAsync(SaveData saveData, int slot = 0)
        {
            try
            {
                var filePath = GetSaveFilePath(slot);

                // Update timestamps
                var dataToSave = saveData with
                {
                    LastModifiedAt = DateTime.UtcNow,
                    Version = SaveData.CURRENT_VERSION // Always save in current format
                };

                // Serialize to JSON using Newtonsoft.Json
                var json = JsonConvert.SerializeObject(dataToSave, _jsonSettings);

                // Write to file
                await File.WriteAllTextAsync(filePath, json);

                _logger.LogInformation("Game saved to slot {Slot}", slot);
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save game to slot {Slot}", slot);
                return FinFail<Unit>(Error.New($"Failed to save game: {ex.Message}"));
            }
        }

        public async Task<Fin<SaveData>> LoadAsync(int slot = 0)
        {
            try
            {
                var filePath = GetSaveFilePath(slot);

                if (!File.Exists(filePath))
                {
                    return FinFail<SaveData>(Error.New($"No save found in slot {slot}"));
                }

                // Read and deserialize using Newtonsoft.Json
                var json = await File.ReadAllTextAsync(filePath);
                var saveData = JsonConvert.DeserializeObject<SaveData>(json, _jsonSettings);

                if (saveData == null)
                {
                    return FinFail<SaveData>(Error.New("Failed to deserialize save data"));
                }

                // Migrate if needed
                var migrated = await MigrateToLatestAsync(saveData);

                return migrated.Match(
                    Succ: data =>
                    {
                        // Update metadata
                        var loaded = data with
                        {
                            Metadata = data.Metadata with
                            {
                                LoadCount = data.Metadata.LoadCount + 1
                            }
                        };

                        _logger.LogInformation(
                            "Game loaded from slot {Slot} (version {Version})",
                            slot, saveData.Version);

                        return FinSucc(loaded);
                    },
                    Fail: error =>
                    {
                        _logger.LogError(
                            "Failed to migrate save from version {Version}: {Error}",
                            saveData.Version, error.Message);
                        return FinFail<SaveData>(error);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load game from slot {Slot}", slot);
                return FinFail<SaveData>(Error.New($"Failed to load game: {ex.Message}"));
            }
        }

        public async Task<bool> SaveExistsAsync(int slot = 0)
        {
            var filePath = GetSaveFilePath(slot);
            return await Task.FromResult(File.Exists(filePath));
        }

        public async Task<Fin<Unit>> DeleteSaveAsync(int slot = 0)
        {
            try
            {
                var filePath = GetSaveFilePath(slot);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Save deleted from slot {Slot}", slot);
                }

                return await Task.FromResult(FinSucc(unit));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete save from slot {Slot}", slot);
                return FinFail<Unit>(Error.New($"Failed to delete save: {ex.Message}"));
            }
        }

        public async Task<Option<SaveInfo>> GetSaveInfoAsync(int slot = 0)
        {
            try
            {
                var filePath = GetSaveFilePath(slot);

                if (!File.Exists(filePath))
                {
                    return None;
                }

                // Read just enough to get metadata using Newtonsoft.Json
                var json = await File.ReadAllTextAsync(filePath);
                var jObject = JObject.Parse(json);

                var info = new SaveInfo
                {
                    Slot = slot,
                    CreatedAt = jObject["createdAt"]?.ToObject<DateTime>() ?? DateTime.MinValue,
                    LastModifiedAt = jObject["lastModifiedAt"]?.ToObject<DateTime>() ?? DateTime.MinValue,
                    CurrentTurn = jObject["currentTurn"]?.ToObject<int>() ?? 0,
                    TotalScore = jObject["totalScore"]?.ToObject<int>() ?? 0,
                    PlayTimeSeconds = jObject["metadata"]?["playTimeSeconds"]?.ToObject<double>() ?? 0,
                    Version = jObject["version"]?.ToObject<int>() ?? 0
                };

                return Some(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get save info for slot {Slot}", slot);
                return None;
            }
        }

        public string ExportSave(SaveData saveData)
        {
            // Ensure we're exporting in current version format
            var dataToExport = saveData with { Version = SaveData.CURRENT_VERSION };
            return JsonConvert.SerializeObject(dataToExport, _jsonSettings);
        }

        public Fin<SaveData> ImportSave(string saveString)
        {
            try
            {
                var saveData = JsonConvert.DeserializeObject<SaveData>(saveString, _jsonSettings);

                if (saveData == null)
                {
                    return FinFail<SaveData>(Error.New("Invalid save data format"));
                }

                // Migrate synchronously
                return MigrateToLatest(saveData);
            }
            catch (Exception ex)
            {
                return FinFail<SaveData>(Error.New($"Failed to import save: {ex.Message}"));
            }
        }

        /// <summary>
        /// Migrates save data to the latest version by chaining migrations.
        /// </summary>
        private async Task<Fin<SaveData>> MigrateToLatestAsync(SaveData data)
        {
            return await Task.FromResult(MigrateToLatest(data));
        }

        /// <summary>
        /// Synchronous migration logic that chains migrations together.
        /// </summary>
        private Fin<SaveData> MigrateToLatest(SaveData data)
        {
            var currentData = data;

            // Already at current version?
            if (currentData.Version >= SaveData.CURRENT_VERSION)
            {
                return FinSucc(currentData);
            }

            _logger.LogInformation(
                "Migrating save from version {FromVersion} to {ToVersion}",
                currentData.Version, SaveData.CURRENT_VERSION);

            // Apply migrations in sequence
            while (currentData.Version < SaveData.CURRENT_VERSION)
            {
                var migration = _migrations.FirstOrDefault(m => m.CanMigrate(currentData));

                if (migration == null)
                {
                    return FinFail<SaveData>(Error.New(
                        $"No migration path from version {currentData.Version} to {SaveData.CURRENT_VERSION}"));
                }

                _logger.LogDebug(
                    "Applying migration: {Description} (v{From} -> v{To})",
                    migration.Description, migration.FromVersion, migration.ToVersion);

                var result = migration.Migrate(currentData);

                if (result.IsFail)
                {
                    return result;
                }

                currentData = result.Match(
                    Succ: data => data,
                    Fail: _ => currentData // Won't reach here due to IsFail check
                );
            }

            _logger.LogInformation("Migration completed successfully");
            return FinSucc(currentData);
        }

        /// <summary>
        /// Gets the platform-specific save directory.
        /// </summary>
        private string GetSaveDirectory()
        {
            // Use platform-specific user data directory
            // This will be in AppData/Local on Windows, ~/.local/share on Linux, etc.
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "BlockLife", "saves");
        }

        /// <summary>
        /// Ensures the save directory exists.
        /// </summary>
        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
                _logger.LogDebug("Created save directory: {Directory}", _saveDirectory);
            }
        }

        /// <summary>
        /// Gets the file path for a specific save slot.
        /// </summary>
        private string GetSaveFilePath(int slot)
        {
            return Path.Combine(_saveDirectory, $"save_{slot}.json");
        }
    }
}
