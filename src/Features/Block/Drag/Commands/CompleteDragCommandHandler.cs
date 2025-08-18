using BlockLife.Core.Features.Block.Commands;
using BlockLife.Core.Features.Block.Drag.Services;
using BlockLife.Core.Features.Block.Drag.Effects;
using BlockLife.Core.Features.Block.Effects;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Serilog;
using System;
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

            // Step 5: Check if target position is occupied (swap scenario)
            var targetBlockOption = _gridStateService.GetBlockAt(request.DropPosition);
            var isSwap = targetBlockOption.IsSome;
            
            if (isSwap)
            {
                // Step 5a: Handle swap operation
                var targetBlock = targetBlockOption.Match(
                    Some: b => b,
                    None: () => throw new InvalidOperationException("Target block should exist")
                );
                
                _logger.Debug("Performing swap between BlockId {DraggedBlock} and BlockId {TargetBlock}",
                    request.BlockId, targetBlock.Id);
                
                // Validate that target block can reach the original position (swap is valid)
                // We need to check if the target block at DropPosition can move to originalPosition
                var targetDistance = Math.Abs(originalPosition.X - request.DropPosition.X) + 
                                   Math.Abs(originalPosition.Y - request.DropPosition.Y);
                
                if (targetDistance > 3) // Using same max range as drag
                {
                    _logger.Warning("Target block cannot reach original position - swap invalid. Distance: {Distance}", targetDistance);
                    _dragStateService.CancelDrag();
                    return FinFail<LanguageExt.Unit>(Error.New("INVALID_SWAP", 
                        $"Target block at {request.DropPosition} cannot reach position {originalPosition} (distance: {targetDistance})"));
                }
                
                // Execute swap using three steps to avoid collision
                // Step 1: Get the dragged block data before removing it
                var draggedBlockOption = _gridStateService.GetBlockById(request.BlockId);
                if (draggedBlockOption.IsNone)
                {
                    _logger.Error("Could not find dragged block with ID {BlockId}", request.BlockId);
                    _dragStateService.CancelDrag();
                    return FinFail<LanguageExt.Unit>(Error.New("BLOCK_NOT_FOUND", 
                        $"Could not find dragged block with ID {request.BlockId}"));
                }
                
                var draggedBlock = draggedBlockOption.Match(
                    Some: b => b,
                    None: () => throw new InvalidOperationException("Dragged block should exist")
                );
                
                // Step 2: Remove dragged block from its original position
                var removeResult = _gridStateService.RemoveBlock(request.BlockId);
                if (removeResult.IsFail)
                {
                    var error = removeResult.Match<Error>(
                        Succ: _ => Error.New("UNKNOWN", "Unknown error"),
                        Fail: e => e
                    );
                    _logger.Error("Failed to remove dragged block during swap: {Error}", error.Message);
                    _dragStateService.CancelDrag();
                    return FinFail<LanguageExt.Unit>(error);
                }
                
                // Step 3: Move target block to original position (now empty)
                // We use GridStateService directly instead of MoveBlockCommand to avoid double validation
                var moveTargetResult = _gridStateService.MoveBlock(targetBlock.Id, originalPosition);
                
                if (moveTargetResult.IsFail)
                {
                    var error = moveTargetResult.Match<Error>(
                        Succ: _ => Error.New("UNKNOWN", "Unknown error"),
                        Fail: e => e
                    );
                    _logger.Error("Failed to move target block during swap: {Error}", error.Message);
                    
                    // Restore dragged block to original position
                    _gridStateService.PlaceBlock(draggedBlock);
                    
                    _dragStateService.CancelDrag();
                    return FinFail<LanguageExt.Unit>(error);
                }
                
                // Step 4: Place dragged block at target position (now empty)
                var draggedBlockWithNewPosition = draggedBlock with { Position = request.DropPosition };
                var placeResult = _gridStateService.PlaceBlock(draggedBlockWithNewPosition);
                
                if (placeResult.IsFail)
                {
                    var error = placeResult.Match<Error>(
                        Succ: _ => Error.New("UNKNOWN", "Unknown error"),
                        Fail: e => e
                    );
                    _logger.Error("Failed to place dragged block during swap: {Error}", error.Message);
                    
                    // Attempt to rollback target block move
                    _gridStateService.MoveBlock(targetBlock.Id, request.DropPosition);
                    
                    // Restore dragged block to original position
                    _gridStateService.PlaceBlock(draggedBlock);
                    
                    _dragStateService.CancelDrag();
                    return FinFail<LanguageExt.Unit>(error);
                }
                
                // Step 5: Publish notifications for both block movements
                // This ensures the view updates correctly for both blocks in the swap
                var targetMovedNotification = BlockMovedNotification.Create(targetBlock.Id, request.DropPosition, originalPosition);
                await _mediator.Publish(targetMovedNotification, cancellationToken);
                
                var draggedMovedNotification = BlockMovedNotification.Create(request.BlockId, originalPosition, request.DropPosition);
                await _mediator.Publish(draggedMovedNotification, cancellationToken);
                
                _logger.Information("Successfully swapped blocks: {DraggedBlock} to {TargetPos}, {TargetBlock} to {OriginalPos}",
                    request.BlockId, request.DropPosition, targetBlock.Id, originalPosition);
            }
            else
            {
                // Step 5b: Normal move (no swap)
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

            // Note: We no longer fail if position is occupied - this enables swapping
            // The swap logic is handled in the main Handle method

            return FinSucc(LanguageExt.Unit.Default);
        }
    }
}