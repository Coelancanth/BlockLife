using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using MediatR;
using System;

namespace BlockLife.Core.Features.Block.Effects
{
    /// <summary>
    /// Notification that a block has been removed from the grid.
    /// This allows other systems to react to block removal events.
    /// </summary>
    public sealed record BlockRemovedNotification : INotification
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
        /// Creates a BlockRemovedNotification from a BlockRemovedEffect.
        /// </summary>
        public static BlockRemovedNotification FromEffect(BlockRemovedEffect effect) => new()
        {
            BlockId = effect.BlockId,
            BlockType = effect.BlockType,
            Position = effect.Position,
            RemovedAt = effect.RemovedAt
        };

        /// <summary>
        /// Creates a BlockRemovedNotification from a Block instance.
        /// </summary>
        public static BlockRemovedNotification FromBlock(Domain.Block.Block block) => new()
        {
            BlockId = block.Id,
            BlockType = block.Type,
            Position = block.Position,
            RemovedAt = DateTime.UtcNow
        };
    }
}