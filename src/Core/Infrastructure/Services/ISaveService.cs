using System.Threading.Tasks;
using BlockLife.Core.Domain.Save;
using LanguageExt;
using LanguageExt.Common;

namespace BlockLife.Core.Infrastructure.Services
{
    /// <summary>
    /// Service responsible for saving and loading game state with version migration support.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// Saves the current game state to the specified slot.
        /// </summary>
        /// <param name="saveData">The data to save</param>
        /// <param name="slot">Save slot number (0-based)</param>
        /// <returns>Success or failure with error details</returns>
        Task<Fin<Unit>> SaveAsync(SaveData saveData, int slot = 0);

        /// <summary>
        /// Loads game state from the specified slot.
        /// Automatically migrates older save formats to the current version.
        /// </summary>
        /// <param name="slot">Save slot number (0-based)</param>
        /// <returns>The loaded and migrated save data, or failure if load fails</returns>
        Task<Fin<SaveData>> LoadAsync(int slot = 0);

        /// <summary>
        /// Checks if a save exists in the specified slot.
        /// </summary>
        /// <param name="slot">Save slot number (0-based)</param>
        /// <returns>True if save exists, false otherwise</returns>
        Task<bool> SaveExistsAsync(int slot = 0);

        /// <summary>
        /// Deletes the save in the specified slot.
        /// </summary>
        /// <param name="slot">Save slot number (0-based)</param>
        /// <returns>Success or failure with error details</returns>
        Task<Fin<Unit>> DeleteSaveAsync(int slot = 0);

        /// <summary>
        /// Gets metadata about a save without fully loading it.
        /// Useful for displaying save information in UI.
        /// </summary>
        /// <param name="slot">Save slot number (0-based)</param>
        /// <returns>Save metadata or None if save doesn't exist</returns>
        Task<Option<SaveInfo>> GetSaveInfoAsync(int slot = 0);

        /// <summary>
        /// Exports a save to a string (for sharing or backup).
        /// </summary>
        /// <param name="saveData">The save data to export</param>
        /// <returns>Serialized save data as string</returns>
        string ExportSave(SaveData saveData);

        /// <summary>
        /// Imports a save from a string.
        /// Validates and migrates the data if needed.
        /// </summary>
        /// <param name="saveString">The serialized save data</param>
        /// <returns>The imported and migrated save data, or failure if invalid</returns>
        Fin<SaveData> ImportSave(string saveString);
    }

    /// <summary>
    /// Lightweight information about a save file.
    /// </summary>
    public sealed record SaveInfo
    {
        /// <summary>
        /// Save slot number.
        /// </summary>
        public int Slot { get; init; }

        /// <summary>
        /// When the save was created.
        /// </summary>
        public System.DateTime CreatedAt { get; init; }

        /// <summary>
        /// When the save was last modified.
        /// </summary>
        public System.DateTime LastModifiedAt { get; init; }

        /// <summary>
        /// Current turn number.
        /// </summary>
        public int CurrentTurn { get; init; }

        /// <summary>
        /// Total score.
        /// </summary>
        public int TotalScore { get; init; }

        /// <summary>
        /// Total play time in seconds.
        /// </summary>
        public double PlayTimeSeconds { get; init; }

        /// <summary>
        /// Save format version.
        /// </summary>
        public int Version { get; init; }

        /// <summary>
        /// Whether this save needs migration to current version.
        /// </summary>
        public bool NeedsMigration => Version < SaveData.CURRENT_VERSION;
    }
}
