using BlockLife.Core.Domain.Common;
using MediatR;
using System;

namespace BlockLife.Core.Features.Block.Drag.Effects
{
    /// <summary>
    /// Notification published when a drag operation is cancelled.
    /// Used to trigger UI reset and cleanup.
    /// </summary>
    public sealed record DragCancelledNotification : INotification
    {
        /// <summary>
        /// The ID of the block that was being dragged.
        /// </summary>
        public required Guid BlockId { get; init; }

        /// <summary>
        /// The original position to return the block to visually.
        /// </summary>
        public required Vector2Int OriginalPosition { get; init; }

        /// <summary>
        /// Creates a new DragCancelledNotification.
        /// </summary>
        public static DragCancelledNotification Create(Guid blockId, Vector2Int originalPosition) =>
            new()
            {
                BlockId = blockId,
                OriginalPosition = originalPosition
            };
    }
}
