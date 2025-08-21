using BlockLife.Core.Domain.Common;
using LanguageExt;
using System;

namespace BlockLife.Core.Features.Block.Drag.Services
{
    /// <summary>
    /// Service for tracking the state of drag operations.
    /// Ensures only one drag operation can be active at a time.
    /// </summary>
    public interface IDragStateService
    {
        /// <summary>
        /// Gets whether a drag operation is currently in progress.
        /// </summary>
        bool IsDragging { get; }

        /// <summary>
        /// Gets the ID of the block currently being dragged.
        /// Returns Guid.Empty if no drag is in progress.
        /// </summary>
        Guid DraggedBlockId { get; }

        /// <summary>
        /// Gets the original position of the block before dragging started.
        /// </summary>
        Vector2Int OriginalPosition { get; }

        /// <summary>
        /// Gets the current preview position during drag.
        /// This is where the block would drop if released now.
        /// </summary>
        Vector2Int CurrentPreviewPosition { get; }

        /// <summary>
        /// Starts a new drag operation for the specified block.
        /// </summary>
        /// <param name="blockId">The ID of the block to drag</param>
        /// <param name="originalPosition">The block's position when drag starts</param>
        /// <returns>Success if drag started, failure if another drag is in progress</returns>
        Fin<Unit> StartDrag(Guid blockId, Vector2Int originalPosition);

        /// <summary>
        /// Updates the preview position during drag.
        /// </summary>
        /// <param name="previewPosition">The new preview position</param>
        /// <returns>Success if updated, failure if no drag in progress</returns>
        Fin<Unit> UpdatePreviewPosition(Vector2Int previewPosition);

        /// <summary>
        /// Completes the current drag operation.
        /// </summary>
        /// <returns>Success if completed, failure if no drag in progress</returns>
        Fin<Unit> CompleteDrag();

        /// <summary>
        /// Cancels the current drag operation.
        /// </summary>
        /// <returns>Success if cancelled, failure if no drag in progress</returns>
        Fin<Unit> CancelDrag();

        /// <summary>
        /// Gets whether the specified position is within the drag range.
        /// Used for Phase 2 range validation.
        /// </summary>
        /// <param name="targetPosition">The position to check</param>
        /// <param name="maxRange">Maximum range in grid cells (default: 3)</param>
        /// <returns>True if within range, false otherwise</returns>
        bool IsWithinDragRange(Vector2Int targetPosition, int maxRange = 3);
    }
}
