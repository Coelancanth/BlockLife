using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Move;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Block.Presenters
{
    /// <summary>
    /// Interface for the grid view that handles block visualization and user interactions.
    /// This interface exposes capabilities that the presenter needs to update the visual representation.
    /// Follows the Composite View Pattern to expose different view capabilities.
    /// </summary>
    public interface IGridView
    {
        /// <summary>
        /// Gets the animation capability for this view.
        /// Provides access to block animation functionality following the Composite View Pattern.
        /// May return null if animation is not supported by this view implementation.
        /// </summary>
        IBlockAnimationView? BlockAnimator { get; }
        /// <summary>
        /// Displays a block at the specified position with the given type and ID.
        /// </summary>
        /// <param name="blockId">Unique identifier for the block</param>
        /// <param name="blockType">Type of block to display</param>
        /// <param name="position">Position where the block should be displayed</param>
        Task DisplayBlockAsync(Guid blockId, BlockType blockType, Vector2Int position);

        /// <summary>
        /// Removes the visual representation of a block from the grid.
        /// </summary>
        /// <param name="blockId">ID of the block to remove</param>
        /// <param name="position">Position where the block was located</param>
        Task RemoveBlockAsync(Guid blockId, Vector2Int position);

        /// <summary>
        /// Updates the visual representation of an existing block.
        /// </summary>
        /// <param name="blockId">ID of the block to update</param>
        /// <param name="newType">New type for the block (if changed)</param>
        /// <param name="newPosition">New position for the block (if moved)</param>
        Task UpdateBlockAsync(Guid blockId, BlockType newType, Vector2Int newPosition);

        /// <summary>
        /// Highlights a block to indicate selection or focus.
        /// </summary>
        /// <param name="blockId">ID of the block to highlight</param>
        /// <param name="highlightType">Type of highlight to apply</param>
        Task HighlightBlockAsync(Guid blockId, HighlightType highlightType);

        /// <summary>
        /// Removes highlighting from a block.
        /// </summary>
        /// <param name="blockId">ID of the block to unhighlight</param>
        Task UnhighlightBlockAsync(Guid blockId);

        /// <summary>
        /// Clears all blocks from the visual grid.
        /// </summary>
        Task ClearGridAsync();

        /// <summary>
        /// Displays feedback for invalid placement attempts.
        /// </summary>
        /// <param name="position">Position where the invalid placement was attempted</param>
        /// <param name="errorMessage">Error message to display</param>
        Task ShowPlacementErrorAsync(Vector2Int position, string errorMessage);

        /// <summary>
        /// Shows visual feedback for successful operations.
        /// </summary>
        /// <param name="position">Position where the operation occurred</param>
        /// <param name="message">Success message to display</param>
        Task ShowSuccessFeedbackAsync(Vector2Int position, string message);

        /// <summary>
        /// Displays the grid boundaries and available placement areas.
        /// </summary>
        /// <param name="gridDimensions">Dimensions of the grid</param>
        Task DisplayGridBoundariesAsync(Vector2Int gridDimensions);

        /// <summary>
        /// Updates the entire grid display with the current state.
        /// Used for initialization or full refresh scenarios.
        /// </summary>
        /// <param name="blocks">All blocks currently on the grid</param>
        Task RefreshGridAsync(IReadOnlyCollection<Domain.Block.Block> blocks);

        /// <summary>
        /// Shows preview of where a block would be placed.
        /// </summary>
        /// <param name="blockType">Type of block being previewed</param>
        /// <param name="position">Position where it would be placed</param>
        Task ShowPlacementPreviewAsync(BlockType blockType, Vector2Int position);

        /// <summary>
        /// Hides the placement preview.
        /// </summary>
        Task HidePlacementPreviewAsync();
    }

    /// <summary>
    /// Types of highlighting that can be applied to blocks.
    /// </summary>
    public enum HighlightType
    {
        /// <summary>
        /// Block is currently selected by the user.
        /// </summary>
        Selected,

        /// <summary>
        /// Block is being hovered over by the cursor.
        /// </summary>
        Hover,

        /// <summary>
        /// Block is involved in a valid pattern or combination.
        /// </summary>
        Pattern,

        /// <summary>
        /// Block is showing an error state.
        /// </summary>
        Error,

        /// <summary>
        /// Block is showing a warning state.
        /// </summary>
        Warning,

        /// <summary>
        /// Block is showing a success state.
        /// </summary>
        Success
    }
}