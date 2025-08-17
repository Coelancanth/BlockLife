using BlockLife.Core.Features.Block.Commands;
using BlockLife.Core.Features.Block.Drag.Services;
using BlockLife.Core.Features.Block.Drag.Effects;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Block.Drag.Commands
{
    /// <summary>
    /// Handler for CompleteDragCommand - Completes a drag operation by moving the block.
    /// Validates the drop position and executes the move if valid.
    /// </summary>
    public class CompleteDragCommandHandler : IRequestHandler<CompleteDragCommand, Fin<LanguageExt.Unit>>
    {
        private readonly IGridStateService _gridStateService;
        private readonly IDragStateService _dragStateService;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public CompleteDragCommandHandler(
            IGridStateService gridStateService,
            IDragStateService dragStateService,
            IMediator mediator,
            ILogger logger)
        {
            _gridStateService = gridStateService;
            _dragStateService = dragStateService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Fin<LanguageExt.Unit>> Handle(CompleteDragCommand request, CancellationToken cancellationToken)
        {
            _logger.Debug("Completing drag for BlockId: {BlockId} to Position: {DropPosition}",
                request.BlockId, request.DropPosition);

            // Step 1: Verify drag is in progress for this block
            if (!_dragStateService.IsDragging || _dragStateService.DraggedBlockId != request.BlockId)
            {
                _logger.Warning("Cannot complete drag - no active drag for BlockId {BlockId}", request.BlockId);
                return FinFail<LanguageExt.Unit>(Error.New("NO_ACTIVE_DRAG", $"No active drag operation for block {request.BlockId}"));
            }

            // Step 2: Get the original position from drag state
            var originalPosition = _dragStateService.OriginalPosition;

            // Step 3: Check if dropping at same position (no move needed)
            if (request.DropPosition.Equals(originalPosition))
            {
                _logger.Debug("Dropping block at same position - completing drag without move");
                var samePositionResult = _dragStateService.CompleteDrag();
                if (samePositionResult.IsFail)
                {
                    var error = samePositionResult.Match<Error>(
                        Succ: _ => Error.New("UNKNOWN", "Unknown error"),
                        Fail: e => e
                    );
                    _logger.Error("Failed to complete drag state: {Error}", error.Message);
                    return FinFail<LanguageExt.Unit>(error);
                }
                
                // No notification needed since block didn't move
                _logger.Debug("Successfully completed drag without movement for BlockId {BlockId}", request.BlockId);
                return FinSucc(LanguageExt.Unit.Default);
            }

            // Step 4: Validate the drop position
            var validationResult = ValidateDrop(request.DropPosition, originalPosition);
            if (validationResult.IsFail)
            {
                var error = validationResult.Match<Error>(
                    Succ: _ => Error.New("UNKNOWN", "Unknown error"),
                    Fail: e => e
                );
                _logger.Warning("Drop validation failed: {Error}", error.Message);
                
                // Clean up drag state even on failed drop
                _dragStateService.CancelDrag();
                return FinFail<LanguageExt.Unit>(error);
            }

            // Step 5: Execute the move using existing MoveBlockCommand
            var moveCommand = MoveBlockCommand.Create(request.BlockId, request.DropPosition);
            var moveResult = await _mediator.Send(moveCommand, cancellationToken);
            
            if (moveResult.IsFail)
            {
                var error = moveResult.Match<Error>(
                    Succ: _ => Error.New("UNKNOWN", "Unknown error"),
                    Fail: e => e
                );
                _logger.Error("Failed to move block during drop: {Error}", error.Message);
                _dragStateService.CancelDrag();
                return FinFail<LanguageExt.Unit>(error);
            }

            // Step 6: Complete the drag operation
            var completeResult = _dragStateService.CompleteDrag();
            if (completeResult.IsFail)
            {
                var error = completeResult.Match<Error>(
                    Succ: _ => Error.New("UNKNOWN", "Unknown error"),
                    Fail: e => e
                );
                _logger.Error("Failed to complete drag state: {Error}", error.Message);
                return FinFail<LanguageExt.Unit>(error);
            }

            // Step 7: Publish drag completed notification
            var notification = DragCompletedNotification.Create(request.BlockId, originalPosition, request.DropPosition);
            await _mediator.Publish(notification, cancellationToken);

            _logger.Debug("Successfully completed drag for BlockId {BlockId} from {FromPosition} to {ToPosition}",
                request.BlockId, originalPosition, request.DropPosition);

            return FinSucc(LanguageExt.Unit.Default);
        }

        private Fin<LanguageExt.Unit> ValidateDrop(Domain.Common.Vector2Int dropPosition, Domain.Common.Vector2Int originalPosition)
        {

            // Check if target position is valid
            if (!_gridStateService.IsValidPosition(dropPosition))
            {
                return FinFail<LanguageExt.Unit>(Error.New("INVALID_POSITION", $"Position {dropPosition} is outside grid bounds"));
            }

            // Check if target position is empty
            if (!_gridStateService.IsPositionEmpty(dropPosition))
            {
                return FinFail<LanguageExt.Unit>(Error.New("POSITION_OCCUPIED", $"Position {dropPosition} is already occupied"));
            }

            return FinSucc(LanguageExt.Unit.Default);
        }
    }
}