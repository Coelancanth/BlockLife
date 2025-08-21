using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Drag.Commands;
using BlockLife.Core.Features.Block.Drag.Effects;
using BlockLife.Core.Features.Block.Drag.Services;
using BlockLife.Core.Features.Block.Drag.Views;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Core.Presentation;
using MediatR;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Block.Drag.Presenters
{
    /// <summary>
    /// Presenter for drag-and-drop operations.
    /// Coordinates between the view, commands, and drag state service.
    /// </summary>
    public class DragPresenter : PresenterBase<IDragView>
    {
        private readonly IMediator _mediator;
        private readonly IDragStateService _dragStateService;
        private readonly IGridStateService _gridStateService;
        private readonly ILogger _logger;

        public DragPresenter(
            IDragView view,
            IMediator mediator,
            IDragStateService dragStateService,
            IGridStateService gridStateService,
            ILogger logger) : base(view)
        {
            _mediator = mediator;
            _dragStateService = dragStateService;
            _gridStateService = gridStateService;
            _logger = logger;
        }

        public override void Initialize()
        {
            // Subscribe to view events
            View.DragStarted += OnDragStarted;
            View.DragUpdated += OnDragUpdated;
            View.DragCompleted += OnDragCompleted;
            View.DragCancelled += OnDragCancelled;

            _logger.Debug("DragPresenter initialized and events subscribed");
        }

        public override void Dispose()
        {
            // Unsubscribe from view events
            View.DragStarted -= OnDragStarted;
            View.DragUpdated -= OnDragUpdated;
            View.DragCompleted -= OnDragCompleted;
            View.DragCancelled -= OnDragCancelled;

            _logger.Debug("DragPresenter disposed and events unsubscribed");
        }

        private async void OnDragStarted(Guid blockId, Vector2Int startPosition)
        {
            _logger.Debug("Drag started for block {BlockId} at position {Position}", blockId, startPosition);

            // Send start drag command
            var command = StartDragCommand.Create(blockId, startPosition);
            var result = await _mediator.Send(command);

            result.Match(
                Succ: _ =>
                {
                    // Show visual feedback
                    View?.ShowDragFeedback(blockId, startPosition);
                    View?.UpdateDragPreview(startPosition, true);

                    // Show range indicators for Phase 2
                    if (ShouldShowRangeIndicators())
                    {
                        View?.ShowRangeIndicators(startPosition, GetDragRange());
                    }
                },
                Fail: error =>
                {
                    _logger.Warning("Failed to start drag: {Error}", error.Message);
                    View?.ShowInvalidDropFeedback(error.Message);
                }
            );
        }

        private void OnDragUpdated(Vector2Int currentPosition)
        {
            if (!_dragStateService.IsDragging)
            {
                return;
            }

            // Update preview position in service
            var updateResult = _dragStateService.UpdatePreviewPosition(currentPosition);

            updateResult.Match(
                Succ: _ =>
                {
                    // Check if this is a valid drop location
                    bool isValidDrop = IsValidDropPosition(currentPosition);

                    // Update visual preview
                    View?.UpdateDragPreview(currentPosition, isValidDrop);

                    // Check range for Phase 2
                    if (ShouldEnforceRangeLimits() && !_dragStateService.IsWithinDragRange(currentPosition))
                    {
                        View?.UpdateDragPreview(currentPosition, false);
                        View?.ShowInvalidDropFeedback("Position is outside drag range");
                    }
                },
                Fail: error =>
                {
                    _logger.Warning("Failed to update drag preview: {Error}", error.Message);
                }
            );
        }

        private async void OnDragCompleted(Guid blockId, Vector2Int dropPosition)
        {
            _logger.Debug("Drag completed for block {BlockId} at position {Position}", blockId, dropPosition);

            // Check range limits for Phase 2
            if (ShouldEnforceRangeLimits() && !_dragStateService.IsWithinDragRange(dropPosition))
            {
                View?.ShowInvalidDropFeedback("Cannot drop outside range limit");
                await CancelDrag(blockId);
                return;
            }

            // Send complete drag command
            var command = CompleteDragCommand.Create(blockId, dropPosition);
            var result = await _mediator.Send(command);

            result.Match(
                Succ: _ =>
                {
                    _logger.Debug("Drag completed successfully");
                    View?.HideDragFeedback();
                    View?.HideRangeIndicators();
                },
                Fail: error =>
                {
                    _logger.Warning("Failed to complete drag: {Error}", error.Message);
                    View?.ShowInvalidDropFeedback(error.Message);
                    // Return block to original position on failed drop
                    View?.AnimateReturnToOriginal(blockId, _dragStateService.OriginalPosition);
                    View?.HideDragFeedback();
                    View?.HideRangeIndicators();
                }
            );
        }

        private async void OnDragCancelled(Guid blockId)
        {
            _logger.Debug("Drag cancelled for block {BlockId}", blockId);
            await CancelDrag(blockId);
        }

        private async Task CancelDrag(Guid blockId)
        {
            var command = CancelDragCommand.Create(blockId);
            var result = await _mediator.Send(command);

            result.Match(
                Succ: _ =>
                {
                    _logger.Debug("Drag cancelled successfully");
                    View?.HideDragFeedback();
                    View?.HideRangeIndicators();
                },
                Fail: error =>
                {
                    _logger.Warning("Failed to cancel drag: {Error}", error.Message);
                }
            );
        }

        private bool IsValidDropPosition(Vector2Int position)
        {
            // Check if position is within grid bounds
            if (!_gridStateService.IsValidPosition(position))
            {
                return false;
            }

            // Check if position is empty (for Phase 1)
            // In Phase 3, we'll allow dropping on occupied cells for swapping
            if (!_gridStateService.IsPositionEmpty(position))
            {
                // For now, disallow dropping on occupied cells
                // Phase 3 will change this to trigger swap
                return false;
            }

            return true;
        }

        private bool ShouldShowRangeIndicators()
        {
            // Phase 2: Range indicators are now enabled
            return true;
        }

        private bool ShouldEnforceRangeLimits()
        {
            // Phase 2: Range limits are now enforced
            return true;
        }

        private int GetDragRange()
        {
            // Default range for Phase 2
            return 3;
        }

        // Note: Animation feedback is handled directly through view events,
        // not through MediatR notifications, to keep the presenter testable
        // without requiring view mocks in unit tests.
    }
}
