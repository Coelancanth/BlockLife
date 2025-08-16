using BlockLife.Core.Domain.Common;
using MediatR;
using System;

namespace BlockLife.Core.Features.Block.Effects
{
    /// <summary>
    /// Notification published when a block has been successfully moved.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public sealed record BlockMovedNotification : INotification
    {
        /// <summary>
        /// The unique identifier of the block that was moved.
        /// </summary>
        public required Guid BlockId { get; init; }

        /// <summary>
        /// The position the block was moved from.
        /// </summary>
        public required Vector2Int FromPosition { get; init; }

        /// <summary>
        /// The position the block was moved to.
        /// </summary>
        public required Vector2Int ToPosition { get; init; }

        /// <summary>
        /// Creates a new BlockMovedNotification with the specified parameters.
        /// </summary>
        public static BlockMovedNotification Create(Guid blockId, Vector2Int fromPosition, Vector2Int toPosition) =>
            new()
            {
                BlockId = blockId,
                FromPosition = fromPosition,
                ToPosition = toPosition
            };
    }
}
