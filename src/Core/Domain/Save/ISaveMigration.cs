using LanguageExt;
using LanguageExt.Common;

namespace BlockLife.Core.Domain.Save
{
    /// <summary>
    /// Interface for save data migrations.
    /// Each migration handles upgrading from one version to the next.
    /// </summary>
    public interface ISaveMigration
    {
        /// <summary>
        /// The version this migration upgrades FROM.
        /// </summary>
        int FromVersion { get; }

        /// <summary>
        /// The version this migration upgrades TO.
        /// </summary>
        int ToVersion { get; }

        /// <summary>
        /// Description of what this migration does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Performs the migration on the save data.
        /// Returns a new SaveData instance with the migrated data.
        /// </summary>
        /// <param name="data">The save data to migrate</param>
        /// <returns>Success with migrated data or failure with error details</returns>
        Fin<SaveData> Migrate(SaveData data);

        /// <summary>
        /// Validates that the migration can be applied to this data.
        /// </summary>
        /// <param name="data">The save data to validate</param>
        /// <returns>True if migration can be applied, false otherwise</returns>
        bool CanMigrate(SaveData data);
    }

    /// <summary>
    /// Base class for save migrations with common functionality.
    /// </summary>
    public abstract class SaveMigrationBase : ISaveMigration
    {
        public abstract int FromVersion { get; }
        public abstract int ToVersion { get; }
        public abstract string Description { get; }

        public virtual bool CanMigrate(SaveData data)
        {
            return data.Version == FromVersion;
        }

        public abstract Fin<SaveData> Migrate(SaveData data);

        /// <summary>
        /// Helper method to create a migration error.
        /// </summary>
        protected static Error MigrationError(string message)
        {
            return Error.New($"Migration failed: {message}");
        }
    }
}