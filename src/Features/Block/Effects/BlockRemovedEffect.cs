using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using System;

namespace BlockLife.Core.Features.Block.Effects
{
    /// <summary>
    /// Effect representing that a block has been removed from the grid.
    /// This effect is used to trigger notifications and subsequent rule evaluations.
    /// </summary>
    public sealed record BlockRemovedEffect
    {
        /// <summary>
        /// The ID of the block that was removed.
        /// </summary>
        public required Guid BlockId { get; init; }

        /// <summary>
        /// The type of block that was removed.
        /// </summary>
        public required BlockType BlockType { get; init; }

        /// <summary>
        /// The position where the block was removed from.
        /// </summary>
        public required Vector2Int Position { get; init; }

        /// <summary>
        /// Timestamp when the block was removed.
        /// </summary>
        public required DateTime RemovedAt { get; init; }

        /// <summary>
        /// Creates a BlockRemovedEffect from a Block instance.
        /// </summary>
        public static BlockRemovedEffect FromBlock(Domain.Block.Block block) => new()
        {
            BlockId = block.Id,
            BlockType = block.Type,
            Position = block.Position,
            RemovedAt = DateTime.UtcNow
        };
    }
}
