using BlockLife.Core.Domain.Save;
using LanguageExt;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Infrastructure.Services.Migrations
{
    /// <summary>
    /// Example migration from version 0 (pre-versioning) to version 1.
    /// This handles saves created before the versioning system was implemented.
    /// </summary>
    public sealed class ExampleV0ToV1Migration : SaveMigrationBase
    {
        public override int FromVersion => 0;
        public override int ToVersion => 1;
        public override string Description => "Add versioning support to legacy saves";

        public override Fin<SaveData> Migrate(SaveData data)
        {
            // For saves without version (version 0), we just need to set the version
            // All other data remains compatible
            
            var migrated = data with
            {
                Version = ToVersion,
                
                // Ensure metadata exists with defaults if missing
                Metadata = data.Metadata ?? new SaveMetadata
                {
                    GameVersion = "0.9.0", // Pre-release version
                    Platform = System.Environment.OSVersion.Platform.ToString(),
                    PlayTimeSeconds = 0,
                    LoadCount = 1
                },
                
                // Ensure settings exist with defaults if missing
                Settings = data.Settings ?? new GameSettings
                {
                    Difficulty = 1,
                    AutoSpawnEnabled = false, // Disabled by default in old saves
                    SpawnRate = 1
                }
            };

            return FinSucc(migrated);
        }

        public override bool CanMigrate(SaveData data)
        {
            // This migration handles version 0 (unversioned) saves
            return data.Version == 0 || data.Version == FromVersion;
        }
    }
}