using BlockLife.Core.Domain.Common;
using System;

namespace BlockLife.Core.Features.Block.Drag.Views
{
    /// <summary>
    /// View interface for drag-and-drop operations.
    /// Defines the contract between the presenter and the UI for drag interactions.
    /// </summary>
    public interface IDragView
    {
        /// <summary>
        /// Event raised when user starts dragging a block.
        /// </summary>
        event Action<Guid, Vector2Int> DragStarted;

        /// <summary>
        /// Event raised when user moves the mouse/pointer during drag.
        /// </summary>
        event Action<Vector2Int> DragUpdated;

        /// <summary>
        /// Event raised when user releases the drag (drops the block).
        /// </summary>
        event Action<Guid, Vector2Int> DragCompleted;

        /// <summary>
        /// Event raised when user cancels the drag (ESC or right-click).
        /// </summary>
        event Action<Guid> DragCancelled;

        /// <summary>
        /// Shows visual feedback that a block is being dragged.
        /// </summary>
        /// <param name="blockId">The ID of the block being dragged</param>
        /// <param name="startPosition">The position where drag started</param>
        void ShowDragFeedback(Guid blockId, Vector2Int startPosition);

        /// <summary>
        /// Updates the visual preview of where the block will drop.
        /// </summary>
        /// <param name="previewPosition">The current preview position</param>
        /// <param name="isValidDrop">Whether this is a valid drop location</param>
        void UpdateDragPreview(Vector2Int previewPosition, bool isValidDrop);

        /// <summary>
        /// Hides all drag-related visual feedback.
        /// </summary>
        void HideDragFeedback();

        /// <summary>
        /// Shows range indicators during drag (Phase 2).
        /// </summary>
        /// <param name="centerPosition">The original position of the block</param>
        /// <param name="range">The maximum drag range in cells</param>
        void ShowRangeIndicators(Vector2Int centerPosition, int range);

        /// <summary>
        /// Hides range indicators.
        /// </summary>
        void HideRangeIndicators();

        /// <summary>
        /// Shows feedback for an invalid drop attempt.
        /// </summary>
        /// <param name="reason">The reason why the drop is invalid</param>
        void ShowInvalidDropFeedback(string reason);

        /// <summary>
        /// Animates the block returning to its original position on cancel.
        /// </summary>
        /// <param name="blockId">The ID of the block</param>
        /// <param name="originalPosition">The position to return to</param>
        void AnimateReturnToOriginal(Guid blockId, Vector2Int originalPosition);

        /// <summary>
        /// Animates the successful drop of a block.
        /// </summary>
        /// <param name="blockId">The ID of the block</param>
        /// <param name="fromPosition">The position dragged from</param>
        /// <param name="toPosition">The position dropped to</param>
        void AnimateSuccessfulDrop(Guid blockId, Vector2Int fromPosition, Vector2Int toPosition);
    }
}
