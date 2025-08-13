using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using MediatR;
using System;

namespace BlockLife.Core.Features.Block.Effects
{
    /// <summary>
    /// Notification that a block has been placed on the grid.
    /// This allows other systems to react to block placement events.
    /// </summary>
    public sealed record BlockPlacedNotification : INotification
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
        /// Creates a BlockPlacedNotification from a BlockPlacedEffect.
        /// </summary>
        public static BlockPlacedNotification FromEffect(BlockPlacedEffect effect) => new()
        {
            BlockId = effect.BlockId,
            BlockType = effect.BlockType,
            Position = effect.Position,
            PlacedAt = effect.PlacedAt
        };

        /// <summary>
        /// Creates a BlockPlacedNotification from a Block instance.
        /// </summary>
        public static BlockPlacedNotification FromBlock(Domain.Block.Block block) => new()
        {
            BlockId = block.Id,
            BlockType = block.Type,
            Position = block.Position,
            PlacedAt = block.CreatedAt
        };
    }
}