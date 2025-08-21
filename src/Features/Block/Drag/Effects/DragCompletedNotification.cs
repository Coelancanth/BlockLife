using BlockLife.Core.Domain.Common;
using MediatR;
using System;

namespace BlockLife.Core.Features.Block.Drag.Effects
{
    /// <summary>
    /// Notification published when a drag operation is successfully completed.
    /// Used to trigger UI updates and animations.
    /// </summary>
    public sealed record DragCompletedNotification : INotification
    {
        /// <summary>
        /// The ID of the block that was dragged.
        /// </summary>
        public required Guid BlockId { get; init; }

        /// <summary>
        /// The original position before the drag started.
        /// </summary>
        public required Vector2Int FromPosition { get; init; }

        /// <summary>
        /// The final position where the block was dropped.
        /// </summary>
        public required Vector2Int ToPosition { get; init; }

        /// <summary>
        /// Creates a new DragCompletedNotification.
        /// </summary>
        public static DragCompletedNotification Create(Guid blockId, Vector2Int fromPosition, Vector2Int toPosition) =>
            new()
            {
                BlockId = blockId,
                FromPosition = fromPosition,
                ToPosition = toPosition
            };
    }
}
