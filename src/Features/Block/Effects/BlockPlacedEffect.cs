using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using System;

namespace BlockLife.Core.Features.Block.Effects
{
    /// <summary>
    /// Effect representing that a block has been placed on the grid.
    /// This effect is used to trigger notifications and subsequent rule evaluations.
    /// </summary>
    public sealed record BlockPlacedEffect
    {
        /// <summary>
        /// The ID of the block that was placed.
        /// </summary>
        public required Guid BlockId { get; init; }

        /// <summary>
        /// The type of block that was placed.
        /// </summary>
        public required BlockType BlockType { get; init; }

        /// <summary>
        /// The position where the block was placed.
        /// </summary>
        public required Vector2Int Position { get; init; }

        /// <summary>
        /// Timestamp when the block was placed.
        /// </summary>
        public required DateTime PlacedAt { get; init; }

        /// <summary>
        /// Creates a BlockPlacedEffect from a Block instance.
        /// </summary>
        public static BlockPlacedEffect FromBlock(Domain.Block.Block block) => new()
        {
            BlockId = block.Id,
            BlockType = block.Type,
            Position = block.Position,
            PlacedAt = block.CreatedAt
        };
    }
}
