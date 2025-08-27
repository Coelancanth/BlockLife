using BlockLife.Core.Domain.Common;
using BlockLife.Core.Domain.Block;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Spawn.Domain
{
    /// <summary>
    /// Random spawn strategy that selects positions and block types randomly.
    /// Uses System.Random for selection - not cryptographically secure, but sufficient for game logic.
    /// Thread-safe implementation using local Random instances.
    /// </summary>
    public sealed class RandomSpawnStrategy : IAutoSpawnStrategy
    {
        private readonly Random _random;

        public string StrategyName => "Random Selection";

        /// <summary>
        /// Creates a new RandomSpawnStrategy with system-generated seed.
        /// Each instance gets its own Random to avoid thread safety issues.
        /// </summary>
        public RandomSpawnStrategy()
        {
            _random = new Random();
        }

        /// <summary>
        /// Creates a new RandomSpawnStrategy with specified seed for reproducible testing.
        /// Useful for deterministic unit tests and debugging scenarios.
        /// </summary>
        /// <param name="seed">Seed value for random number generation</param>
        public RandomSpawnStrategy(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Selects a random spawn position from available empty positions.
        /// Returns None if no positions available (graceful handling of full grid).
        /// Uses functional approach with LINQ and Option pattern.
        /// </summary>
        /// <param name="emptyPositions">Collection of available empty positions</param>
        /// <param name="gridSize">Grid size for validation (currently unused but part of interface)</param>
        /// <returns>Option containing randomly selected position, or None if collection empty</returns>
        public Option<Vector2Int> SelectSpawnPosition(IEnumerable<Vector2Int> emptyPositions, Vector2Int gridSize)
        {
            if (emptyPositions == null)
                return None;

            var positions = emptyPositions.ToArray();
            
            if (positions.Length == 0)
                return None;

            var selectedIndex = _random.Next(positions.Length);
            return Some(positions[selectedIndex]);
        }

        /// <summary>
        /// Selects a random primary block type for spawning.
        /// Excludes special combination types (CareerOpportunity, Partnership, Passion).
        /// Uses weighted selection - Fun blocks are less common to maintain game balance.
        /// Current turn parameter allows for future turn-based spawning logic.
        /// </summary>
        /// <param name="currentTurn">Current game turn number</param>
        /// <returns>Randomly selected primary BlockType</returns>
        public BlockType SelectBlockType(int currentTurn)
        {
            // Define spawn weights for game balance
            // Fun blocks are rarer to encourage strategic thinking
            var weightedTypes = new[]
            {
                (BlockType.Work, 20),
                (BlockType.Study, 18), 
                (BlockType.Health, 22),
                (BlockType.Relationship, 18),
                (BlockType.Creativity, 15),
                (BlockType.Fun, 7)  // Rarer - high value but situational
            };

            var totalWeight = weightedTypes.Sum(x => x.Item2);
            var randomValue = _random.Next(totalWeight);

            var cumulativeWeight = 0;
            foreach (var (blockType, weight) in weightedTypes)
            {
                cumulativeWeight += weight;
                if (randomValue < cumulativeWeight)
                    return blockType;
            }

            // Fallback (should never reach here with proper weights)
            return BlockType.Basic;
        }
    }
}