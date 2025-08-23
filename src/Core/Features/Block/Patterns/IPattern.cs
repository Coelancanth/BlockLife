using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Patterns.Models;
using LanguageExt;

namespace BlockLife.Core.Features.Block.Patterns
{
    /// <summary>
    /// Core abstraction representing a pattern that has been recognized in the grid.
    /// A pattern is an immutable description of blocks that could be acted upon together.
    /// This interface separates pattern detection from pattern execution (Single Responsibility).
    /// Following ADR-001: Pattern Recognition Framework.
    /// </summary>
    public interface IPattern
    {
        /// <summary>
        /// The type of this pattern (Match, TierUp, Transmute, etc.).
        /// Determines which executor will handle this pattern.
        /// </summary>
        PatternType Type { get; }

        /// <summary>
        /// Grid positions of all blocks involved in this pattern.
        /// These are the blocks that would be affected if the pattern is executed.
        /// Must be immutable using LanguageExt.Seq for thread safety.
        /// </summary>
        Seq<Vector2Int> Positions { get; }

        /// <summary>
        /// Execution priority for this pattern instance.
        /// Higher values execute first. Used for conflict resolution.
        /// Typically matches PatternType.GetPriority() but can be adjusted per instance.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Unique identifier for this specific pattern instance.
        /// Used for tracking, logging, and avoiding duplicate processing.
        /// </summary>
        string PatternId { get; }

        /// <summary>
        /// Size metric for this pattern (number of blocks involved).
        /// Used for bonus calculations and performance analysis.
        /// </summary>
        int Size => Positions.Count;

        /// <summary>
        /// Calculates what would happen if this pattern were executed.
        /// This is a pure function that should not modify any game state.
        /// Used for UI previews, conflict resolution, and player feedback.
        /// </summary>
        /// <returns>Immutable description of the pattern's effects</returns>
        PatternOutcome CalculateOutcome();

        /// <summary>
        /// Checks if this pattern conflicts with another pattern.
        /// Two patterns conflict if they share any grid positions.
        /// Used by the pattern resolver to handle competing patterns.
        /// </summary>
        /// <param name="other">The other pattern to check against</param>
        /// <returns>True if patterns share any positions</returns>
        bool ConflictsWith(IPattern other);

        /// <summary>
        /// Determines if this pattern is still valid given the current grid state.
        /// Used to verify patterns haven't been invalidated by concurrent changes.
        /// </summary>
        /// <param name="gridService">Current grid state</param>
        /// <returns>True if pattern is still executable</returns>
        bool IsValidFor(BlockLife.Core.Infrastructure.Services.IGridStateService gridService);

        /// <summary>
        /// Gets a human-readable description of this pattern for debugging and logging.
        /// Should include pattern type, size, and key positions.
        /// </summary>
        /// <returns>Descriptive string representation</returns>
        string GetDescription();
    }

    /// <summary>
    /// Extension methods for IPattern to provide common functionality.
    /// </summary>
    public static class PatternExtensions
    {
        /// <summary>
        /// Checks if a pattern involves a specific grid position.
        /// </summary>
        public static bool InvolvePosition(this IPattern pattern, Vector2Int position) =>
            pattern.Positions.Contains(position);

        /// <summary>
        /// Gets the bounding box of all positions in this pattern.
        /// Returns (min, max) coordinates encompassing all pattern positions.
        /// </summary>
        public static (Vector2Int Min, Vector2Int Max) GetBounds(this IPattern pattern)
        {
            if (!pattern.Positions.Any())
                return (Vector2Int.Zero, Vector2Int.Zero);

            var positions = pattern.Positions;
            var minX = positions.Min(p => p.X);
            var maxX = positions.Max(p => p.X);
            var minY = positions.Min(p => p.Y);
            var maxY = positions.Max(p => p.Y);

            return (new Vector2Int(minX, minY), new Vector2Int(maxX, maxY));
        }

        /// <summary>
        /// Calculates the center position of this pattern.
        /// Uses average of all positions, useful for visual effects.
        /// </summary>
        public static Vector2Int GetCenter(this IPattern pattern)
        {
            if (!pattern.Positions.Any())
                return Vector2Int.Zero;

            var positions = pattern.Positions;
            var avgX = positions.Sum(p => p.X) / positions.Count;
            var avgY = positions.Sum(p => p.Y) / positions.Count;

            return new Vector2Int(avgX, avgY);
        }
    }
}