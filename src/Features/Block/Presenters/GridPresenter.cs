using BlockLife.Core.Domain.Block; // For BlockTypeExtensions
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Core.Presentation;
using MediatR;
using Serilog;
using System;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Block.Presenters
{
    /// <summary>
    /// Presenter for the grid view that handles block placement and management interactions.
    /// Coordinates between user input, business logic, and view updates.
    /// </summary>
    public sealed class GridPresenter : PresenterBase<IGridView>
    {
        private readonly IMediator _mediator;
        private readonly IGridStateService _gridStateService;
        private readonly ILogger _logger;

        public GridPresenter(IGridView view, IMediator mediator, IGridStateService gridStateService, ILogger logger)
            : base(view)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _gridStateService = gridStateService ?? throw new ArgumentNullException(nameof(gridStateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the grid presenter and refreshes the view with current state.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _logger.Information("GridPresenter initialized, setting up grid display");

            try
            {
                // Display grid boundaries
                var gridDimensions = _gridStateService.GetGridDimensions();
                _ = View.DisplayGridBoundariesAsync(gridDimensions);

                // Refresh with current state
                var allBlocks = _gridStateService.GetAllBlocks();
                _ = View.RefreshGridAsync(allBlocks);

                _logger.Information("Grid display setup initiated with {BlockCount} blocks", allBlocks.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error setting up grid display");
            }
        }

        /// <summary>
        /// Handles user request to place a block at a specific position.
        /// </summary>
        /// <param name="blockType">Type of block to place</param>
        /// <param name="position">Position where the block should be placed</param>
        public async Task HandlePlaceBlockAsync(Domain.Block.BlockType blockType, Domain.Common.Vector2Int position)
        {
            _logger.Information("User requested to place {BlockType} at {Position}", blockType, position);

            try
            {
                var command = new PlaceBlockCommand(position, blockType);
                var result = await _mediator.Send(command);

                await result.Match(
                    Succ: async _ =>
                    {
                        _logger.Information("Successfully placed block at {Position}", position);
                        await View.ShowSuccessFeedbackAsync(position, $"Placed {blockType.GetDisplayName()}");
                    },
                    Fail: async error =>
                    {
                        _logger.Warning("Failed to place block: {Error}", error.Message);
                        await View.ShowPlacementErrorAsync(position, error.Message);
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error handling place block request");
                await View.ShowPlacementErrorAsync(position, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Handles user request to remove a block from a specific position.
        /// </summary>
        /// <param name="position">Position of the block to remove</param>
        public async Task HandleRemoveBlockAsync(Domain.Common.Vector2Int position)
        {
            _logger.Information("User requested to remove block at {Position}", position);

            try
            {
                var command = new RemoveBlockCommand(position);
                var result = await _mediator.Send(command);

                await result.Match(
                    Succ: async _ =>
                    {
                        _logger.Information("Successfully removed block from {Position}", position);
                        await View.ShowSuccessFeedbackAsync(position, "Block removed");
                    },
                    Fail: async error =>
                    {
                        _logger.Warning("Failed to remove block: {Error}", error.Message);
                        await View.ShowPlacementErrorAsync(position, error.Message);
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error handling remove block request");
                await View.ShowPlacementErrorAsync(position, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Handles mouse hover over a grid position to show placement preview.
        /// </summary>
        /// <param name="blockType">Type of block being considered for placement</param>
        /// <param name="position">Position being hovered over</param>
        public async Task HandlePositionHoverAsync(Domain.Block.BlockType blockType, Domain.Common.Vector2Int position)
        {
            try
            {
                if (_gridStateService.IsValidPosition(position) && _gridStateService.IsPositionEmpty(position))
                {
                    await View.ShowPlacementPreviewAsync(blockType, position);
                }
                else
                {
                    await View.HidePlacementPreviewAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error handling position hover");
            }
        }

        /// <summary>
        /// Handles mouse leaving a grid position to hide placement preview.
        /// </summary>
        public async Task HandlePositionUnhoverAsync()
        {
            try
            {
                await View.HidePlacementPreviewAsync();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error handling position unhover");
            }
        }

        /// <summary>
        /// Handles block selection for highlighting.
        /// </summary>
        /// <param name="blockId">ID of the block to select</param>
        public async Task HandleBlockSelectionAsync(Guid blockId)
        {
            try
            {
                await View.HighlightBlockAsync(blockId, HighlightType.Selected);
                _logger.Debug("Selected block {BlockId}", blockId);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error handling block selection");
            }
        }

        /// <summary>
        /// Handles clearing block selection.
        /// </summary>
        /// <param name="blockId">ID of the block to deselect</param>
        public async Task HandleBlockDeselectionAsync(Guid blockId)
        {
            try
            {
                await View.UnhighlightBlockAsync(blockId);
                _logger.Debug("Deselected block {BlockId}", blockId);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error handling block deselection");
            }
        }

        /// <summary>
        /// Handles notification that a block was placed.
        /// Updates the view to reflect the new block.
        /// </summary>
        public async Task HandleBlockPlacedAsync(Guid blockId, BlockType blockType, Domain.Common.Vector2Int position)
        {
            try
            {
                _logger.Debug("Handling block placed for block {BlockId}", blockId);
                await View.DisplayBlockAsync(blockId, blockType, position);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling block placed");
            }
        }

        /// <summary>
        /// Handles notification that a block was removed.
        /// Updates the view to reflect the removal.
        /// </summary>
        public async Task HandleBlockRemovedAsync(Guid blockId, Domain.Common.Vector2Int position)
        {
            try
            {
                _logger.Debug("Handling block removed for block {BlockId}", blockId);
                await View.RemoveBlockAsync(blockId, position);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling block removed");
            }
        }

        /// <summary>
        /// Handles notification that a block was moved.
        /// Updates the view to animate or instantly update the block position.
        /// </summary>
        /// <param name="blockId">The unique identifier of the moved block</param>
        /// <param name="fromPosition">The position the block was moved from</param>
        /// <param name="toPosition">The position the block was moved to</param>
        public async Task HandleBlockMovedAsync(Guid blockId, Domain.Common.Vector2Int fromPosition, Domain.Common.Vector2Int toPosition)
        {
            try
            {
                _logger.Information("Handling block moved for block {BlockId} from {FromPosition} to {ToPosition}",
                    blockId, fromPosition, toPosition);

                // Check if the view has animation capability
                if (View.BlockAnimator != null)
                {
                    // Animate the block movement
                    await View.BlockAnimator.AnimateMoveAsync(
                        blockId,
                        fromPosition,
                        toPosition);

                    _logger.Debug("Block move animation initiated for block {BlockId}", blockId);
                }
                else
                {
                    // Fallback to instant update if animation is not available
                    _logger.Warning("BlockAnimator not available, updating block position instantly");
                    var block = _gridStateService.GetBlockById(blockId);
                    if (block.IsSome)
                    {
                        var blockData = block.Match(Some: b => b, None: () => throw new InvalidOperationException());
                        await View.UpdateBlockAsync(blockId, blockData.Type, toPosition);
                    }
                }

                // Show success feedback at the new position
                await View.ShowSuccessFeedbackAsync(toPosition, "Block moved successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling block moved for block {BlockId}", blockId);
            }
        }
    }
}