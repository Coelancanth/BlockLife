using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Patterns.Models;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using static LanguageExt.Prelude;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockLife.Core.Features.Block.Patterns.Recognizers
{
    /// <summary>
    /// Immutable data class representing a tier-up pattern found in the grid.
    /// A tier-up pattern consists of 3+ adjacent blocks of the same type that can be merged into a higher tier.
    /// Following ADR-001: Pattern Recognition Framework.
    /// </summary>
    public sealed record TierUpPattern : IPattern
    {
        /// <summary>
        /// The type of blocks that form this tier-up pattern.
        /// All blocks in Positions must be of this type.
        /// </summary>
        public required BlockType MergeBlockType { get; init; }

        /// <summary>
        /// The current tier of the blocks in this pattern.
        /// Tier 1 blocks merge to Tier 2, Tier 2 to Tier 3, etc.
        /// </summary>
        public required int CurrentTier { get; init; }

        /// <summary>
        /// Grid positions of all blocks involved in this tier-up pattern.
        /// Must contain at least 3 positions for a valid tier-up.
        /// </summary>
        public required Seq<Vector2Int> Positions { get; init; }

        /// <summary>
        /// The pattern type (always TierUp for this implementation).
        /// </summary>
        public PatternType Type => PatternType.TierUp;

        /// <summary>
        /// Execution priority for this pattern instance.
        /// TierUps have base priority 20 (higher than matches), but larger patterns get even higher priority.
        /// </summary>
        public int Priority => PatternType.TierUp.GetPriority() + (Positions.Count - 3);

        /// <summary>
        /// Unique identifier for this specific pattern instance.
        /// Generated from pattern type, positions, and block type.
        /// </summary>
        public string PatternId => 
            $"TierUp_{MergeBlockType}_T{CurrentTier}_{Positions.Count}_{GetPositionHash()}";

        /// <summary>
        /// Calculates what would happen if this pattern were executed.
        /// For tier-up patterns: removes 3 blocks, creates 1 higher-tier block at centroid.
        /// </summary>
        public PatternOutcome CalculateOutcome()
        {
            // Calculate the new tier (current + 1)
            var newTier = CurrentTier + 1;
            
            // Calculate value multiplier for higher tiers
            // T1: 3^0 = 1x value, T2: 3^1 = 3x value, T3: 3^2 = 9x value, etc.
            var tierMultiplier = Math.Pow(3, CurrentTier - 1);
            var baseReward = MergeBlockType.GetBaseValue();
            
            // Tier-up doesn't directly give resources, but the higher-tier block is worth more when matched later
            // We'll return a transformation outcome rather than removal
            var attributes = Seq<(string, int)>();  // No immediate rewards for tier-up
            
            // For now, return a removal outcome since we don't have transformation yet
            // VS_003B-2 will implement the actual transformation logic
            return PatternOutcome.CreateRemoval(Positions, attributes, tierMultiplier);
        }

        /// <summary>
        /// Checks if this pattern conflicts with another pattern.
        /// Patterns conflict if they share any grid positions.
        /// </summary>
        public bool ConflictsWith(IPattern other) =>
            Positions.Exists(pos => other.Positions.Contains(pos));

        /// <summary>
        /// Determines if this pattern is still valid given the current grid state.
        /// Validates that all positions still contain blocks of the expected type and tier.
        /// </summary>
        public bool IsValidFor(IGridStateService gridService) =>
            Positions.ForAll(pos => 
                gridService.GetBlockAt(pos)
                    .Map(block => block.Type == MergeBlockType)
                    // Note: We'll need to check tier once blocks have tier property (VS_003B-2)
                    .IfNone(false));

        /// <summary>
        /// Gets a human-readable description of this pattern.
        /// </summary>
        public string GetDescription() =>
            $"TierUp {Positions.Count} {MergeBlockType.GetDisplayName()} blocks (T{CurrentTier}→T{CurrentTier + 1}) at {GetPositionsString()}";

        /// <summary>
        /// Creates a tier-up pattern from a sequence of positions and block type.
        /// Validates that all positions form a connected component and meet minimum size.
        /// </summary>
        public static Option<TierUpPattern> Create(Seq<Vector2Int> positions, BlockType blockType, int currentTier = 1)
        {
            // Must have exactly 3 blocks for tier-up (not more, not less)
            // This is different from match patterns which allow 3+
            if (positions.Count != 3)
                return Option<TierUpPattern>.None;

            if (!ArePositionsConnected(positions))
                return Option<TierUpPattern>.None;

            return Option<TierUpPattern>.Some(new TierUpPattern
            {
                MergeBlockType = blockType,
                CurrentTier = currentTier,
                Positions = positions.OrderBy(p => p.X).ThenBy(p => p.Y).ToSeq()
            });
        }

        /// <summary>
        /// Validates that all positions form a connected component using flood-fill validation.
        /// Each position must have at least one orthogonally adjacent neighbor in the set.
        /// </summary>
        private static bool ArePositionsConnected(Seq<Vector2Int> positions)
        {
            if (!positions.Any())
                return false;

            if (positions.Count == 1)
                return true;

            // Use flood-fill to ensure all positions are reachable from the first position
            var positionSet = positions.ToHashSet();
            var visited = new System.Collections.Generic.HashSet<Vector2Int>();
            var queue = new System.Collections.Generic.Queue<Vector2Int>();
            
            queue.Enqueue(positions.Head);
            visited.Add(positions.Head);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                
                foreach (var adjacent in current.GetOrthogonallyAdjacentPositions())
                {
                    if (positionSet.Contains(adjacent) && !visited.Contains(adjacent))
                    {
                        visited.Add(adjacent);
                        queue.Enqueue(adjacent);
                    }
                }
            }

            return visited.Count == positions.Count();
        }

        /// <summary>
        /// Generates a hash from positions for the pattern ID.
        /// </summary>
        private string GetPositionHash() =>
            string.Join("_", Positions.Map(p => $"{p.X}x{p.Y}"));

        /// <summary>
        /// Gets a compact string representation of positions for descriptions.
        /// </summary>
        private string GetPositionsString() =>
            Positions.Count <= 4 
                ? string.Join(", ", Positions.Map(p => p.ToString()))
                : $"{Positions.Head}...{Positions.Last} ({Positions.Count} total)";
    }
}