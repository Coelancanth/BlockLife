using BlockLife.Core.Domain.Common;
using BlockLife.Core.Domain.Block;
using LanguageExt;
using System.Collections.Generic;

namespace BlockLife.Core.Features.Spawn.Domain
{
    /// <summary>
    /// Strategy interface for selecting spawn positions for auto-spawned blocks.
    /// Implements Strategy pattern to allow different spawn logic implementations.
    /// Pure functional interface - no side effects, deterministic given same inputs.
    /// </summary>
    public interface IAutoSpawnStrategy
    {
        /// <summary>
        /// Selects a spawn position from available empty positions.
        /// Uses functional Option pattern - None when no positions available (grid full).
        /// Strategy implementation should be pure (no random state changes, etc.).
        /// </summary>
        /// <param name="emptyPositions">Collection of available empty grid positions</param>
        /// <param name="gridSize">Size of the grid for validation</param>
        /// <returns>Option containing selected position, or None if no valid positions</returns>
        Option<Vector2Int> SelectSpawnPosition(IEnumerable<Vector2Int> emptyPositions, Vector2Int gridSize);

        /// <summary>
        /// Selects which block type should be spawned.
        /// Allows strategy to control both position and block type selection.
        /// Current turn can influence spawning logic (e.g., specific types on certain turns).
        /// </summary>
        /// <param name="currentTurn">Current game turn number</param>
        /// <returns>BlockType to spawn</returns>
        BlockType SelectBlockType(int currentTurn);

        /// <summary>
        /// Gets a human-readable name for this spawn strategy.
        /// Used for debugging and logging purposes.
        /// </summary>
        string StrategyName { get; }
    }
}