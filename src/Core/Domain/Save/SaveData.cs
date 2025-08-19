using System;
using System.Collections.Generic;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;

namespace BlockLife.Core.Domain.Save
{
    /// <summary>
    /// Represents the saved game state with versioning support for migrations.
    /// Immutable record that contains all persistent game data.
    /// </summary>
    public sealed record SaveData
    {
        /// <summary>
        /// Current version of the save format. Increment when breaking changes occur.
        /// </summary>
        public const int CURRENT_VERSION = 1;

        /// <summary>
        /// Version of this save file. Used to determine if migration is needed.
        /// </summary>
        public int Version { get; init; } = CURRENT_VERSION;

        /// <summary>
        /// Unique identifier for this save file.
        /// </summary>
        public Guid SaveId { get; init; } = Guid.NewGuid();

        /// <summary>
        /// When this save was created.
        /// </summary>
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// When this save was last modified.
        /// </summary>
        public DateTime LastModifiedAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// All blocks currently in the game grid.
        /// </summary>
        public IReadOnlyList<Block.Block> Blocks { get; init; } = Array.Empty<Block.Block>();

        /// <summary>
        /// Grid dimensions at time of save.
        /// </summary>
        public Vector2Int GridDimensions { get; init; } = new Vector2Int(10, 10);

        /// <summary>
        /// Player's current resources (Money, SocialCapital, etc).
        /// </summary>
        public IReadOnlyDictionary<string, int> Resources { get; init; } = new Dictionary<string, int>();

        /// <summary>
        /// Player's current attributes (Knowledge, Health, Happiness, etc).
        /// </summary>
        public IReadOnlyDictionary<string, int> Attributes { get; init; } = new Dictionary<string, int>();

        /// <summary>
        /// Current turn number in the game.
        /// </summary>
        public int CurrentTurn { get; init; } = 0;

        /// <summary>
        /// Total score accumulated.
        /// </summary>
        public int TotalScore { get; init; } = 0;

        /// <summary>
        /// Game settings that affect gameplay.
        /// </summary>
        public GameSettings Settings { get; init; } = new();

        /// <summary>
        /// Metadata for debugging and analytics.
        /// </summary>
        public SaveMetadata Metadata { get; init; } = new();
    }

    /// <summary>
    /// Game settings that persist with the save.
    /// </summary>
    public sealed record GameSettings
    {
        /// <summary>
        /// Difficulty level (0=Easy, 1=Normal, 2=Hard).
        /// </summary>
        public int Difficulty { get; init; } = 1;

        /// <summary>
        /// Whether auto-spawn is enabled.
        /// </summary>
        public bool AutoSpawnEnabled { get; init; } = true;

        /// <summary>
        /// Blocks spawned per turn.
        /// </summary>
        public int SpawnRate { get; init; } = 1;

        /// <summary>
        /// Unlocked features/abilities.
        /// </summary>
        public IReadOnlySet<string> UnlockedFeatures { get; init; } = new HashSet<string>();
    }

    /// <summary>
    /// Metadata about the save file for debugging and analytics.
    /// </summary>
    public sealed record SaveMetadata
    {
        /// <summary>
        /// Game version that created this save.
        /// </summary>
        public string GameVersion { get; init; } = "1.0.0";

        /// <summary>
        /// Platform the save was created on.
        /// </summary>
        public string Platform { get; init; } = Environment.OSVersion.Platform.ToString();

        /// <summary>
        /// Total play time in seconds.
        /// </summary>
        public double PlayTimeSeconds { get; init; } = 0;

        /// <summary>
        /// Number of times this save has been loaded.
        /// </summary>
        public int LoadCount { get; init; } = 0;

        /// <summary>
        /// Custom properties for future extensibility.
        /// </summary>
        public IReadOnlyDictionary<string, string> CustomProperties { get; init; } = new Dictionary<string, string>();
    }
}