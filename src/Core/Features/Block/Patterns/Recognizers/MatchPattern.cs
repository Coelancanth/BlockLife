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
    /// Immutable data class representing a match pattern found in the grid.
    /// A match pattern consists of 3+ adjacent blocks of the same type.
    /// Following ADR-001: Pattern Recognition Framework.
    /// </summary>
    public sealed record MatchPattern : IPattern
    {
        /// <summary>
        /// The type of blocks that form this match pattern.
        /// All blocks in Positions must be of this type.
        /// </summary>
        public required BlockType MatchedBlockType { get; init; }

        /// <summary>
        /// Grid positions of all blocks involved in this match pattern.
        /// Must contain at least 3 positions for a valid match.
        /// </summary>
        public required Seq<Vector2Int> Positions { get; init; }

        /// <summary>
        /// The pattern type (always Match for this implementation).
        /// </summary>
        public PatternType Type => PatternType.Match;

        /// <summary>
        /// Execution priority for this pattern instance.
        /// Matches have base priority 10, but larger matches get higher priority.
        /// </summary>
        public int Priority => PatternType.Match.GetPriority() + (Positions.Count - 3);

        /// <summary>
        /// Unique identifier for this specific pattern instance.
        /// Generated from pattern type, positions, and block type.
        /// </summary>
        public string PatternId => 
            $"Match_{MatchedBlockType}_{Positions.Count}_{GetPositionHash()}";

        /// <summary>
        /// Calculates what would happen if this pattern were executed.
        /// For match patterns: removes all blocks, awards attributes based on type.
        /// </summary>
        public PatternOutcome CalculateOutcome()
        {
            // Calculate attribute rewards based on block type and match size
            var baseReward = MatchedBlockType.GetBaseValue();
            var sizeBonus = Positions.Count >= 5 ? 2.0 : 
                           Positions.Count >= 4 ? 1.5 : 1.0;
            
            var attributes = MatchedBlockType switch
            {
                BlockType.Work => Seq(new (string, int)[] {("Money", (int)(baseReward * sizeBonus))}),
                BlockType.Study => Seq(new (string, int)[] {("Knowledge", (int)(baseReward * sizeBonus))}),
                BlockType.Health => Seq(new (string, int)[] {("Health", (int)(baseReward * sizeBonus))}),
                BlockType.Relationship => Seq(new (string, int)[] {("Social", (int)(baseReward * sizeBonus))}),
                BlockType.Creativity => Seq(new (string, int)[] {("Creativity", (int)(baseReward * sizeBonus))}),
                BlockType.Fun => Seq(new (string, int)[] {("Happiness", (int)(baseReward * sizeBonus))}),
                _ => Seq<(string, int)>()
            };

            return PatternOutcome.CreateRemoval(Positions, attributes, sizeBonus);
        }

        /// <summary>
        /// Checks if this pattern conflicts with another pattern.
        /// Patterns conflict if they share any grid positions.
        /// </summary>
        public bool ConflictsWith(IPattern other) =>
            Positions.Exists(pos => other.Positions.Contains(pos));

        /// <summary>
        /// Determines if this pattern is still valid given the current grid state.
        /// Validates that all positions still contain blocks of the expected type.
        /// </summary>
        public bool IsValidFor(IGridStateService gridService) =>
            Positions.ForAll(pos => 
                gridService.GetBlockAt(pos)
                    .Map(block => block.Type == MatchedBlockType)
                    .IfNone(false));

        /// <summary>
        /// Gets a human-readable description of this pattern.
        /// </summary>
        public string GetDescription() =>
            $"Match {Positions.Count} {MatchedBlockType.GetDisplayName()} blocks at {GetPositionsString()}";

        /// <summary>
        /// Creates a match pattern from a sequence of positions and block type.
        /// Validates that all positions form a connected component.
        /// </summary>
        public static Option<MatchPattern> Create(Seq<Vector2Int> positions, BlockType blockType)
        {
            if (positions.Count < 3)
                return Option<MatchPattern>.None;

            if (!ArePositionsConnected(positions))
                return Option<MatchPattern>.None;

            return Option<MatchPattern>.Some(new MatchPattern
            {
                MatchedBlockType = blockType,
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
            var queue = new Queue<Vector2Int>();
            
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

            return visited.Count == positions.Count;
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