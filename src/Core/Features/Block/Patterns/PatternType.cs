namespace BlockLife.Core.Features.Block.Patterns
{
    /// <summary>
    /// Defines the different types of patterns that can be recognized and executed in the game.
    /// Each pattern type represents a different kind of interaction between blocks.
    /// Following ADR-001: Pattern Recognition Framework.
    /// </summary>
    public enum PatternType
    {
        /// <summary>
        /// Match pattern: 3+ adjacent blocks of the same type that can be cleared.
        /// Priority: 10 (lowest priority - executed after other patterns).
        /// </summary>
        Match = 1,

        /// <summary>
        /// Merge pattern: Combines 3+ adjacent blocks to create higher-tier blocks.
        /// Priority: 20 (medium priority - executed before matches).
        /// </summary>
        Merge = 2,

        /// <summary>
        /// Transmute pattern: Transforms blocks based on special conditions.
        /// Priority: 30 (highest priority - executed before other patterns).
        /// </summary>
        Transmute = 3
    }

    /// <summary>
    /// Extension methods for PatternType to provide additional functionality.
    /// </summary>
    public static class PatternTypeExtensions
    {
        /// <summary>
        /// Gets the execution priority for this pattern type.
        /// Higher values are executed first to prevent conflicts.
        /// </summary>
        public static int GetPriority(this PatternType patternType) => patternType switch
        {
            PatternType.Match => 10,
            PatternType.Merge => 20,
            PatternType.Transmute => 30,
            _ => 0
        };

        /// <summary>
        /// Gets the human-readable name for this pattern type.
        /// </summary>
        public static string GetDisplayName(this PatternType patternType) => patternType switch
        {
            PatternType.Match => "Match Blocks",
            PatternType.Merge => "Merge Blocks", 
            PatternType.Transmute => "Transmute Blocks",
            _ => patternType.ToString()
        };

        /// <summary>
        /// Checks if this pattern type is currently enabled.
        /// Used for progressive unlock system.
        /// </summary>
        public static bool IsEnabled(this PatternType patternType) => patternType switch
        {
            PatternType.Match => true,      // Always available
            PatternType.Merge => true,     // Enabled for VS_003B-1 implementation
            PatternType.Transmute => false, // Future implementation
            _ => false
        };
    }
}