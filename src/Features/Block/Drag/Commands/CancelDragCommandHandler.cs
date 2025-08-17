using BlockLife.Core.Features.Block.Drag.Services;
using BlockLife.Core.Features.Block.Drag.Effects;
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
    /// Handler for CancelDragCommand - Cancels an ongoing drag operation.
    /// Returns the block to its original position visually.
    /// </summary>
    public class CancelDragCommandHandler : IRequestHandler<CancelDragCommand, Fin<LanguageExt.Unit>>
    {
        private readonly IDragStateService _dragStateService;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public CancelDragCommandHandler(
            IDragStateService dragStateService,
            IMediator mediator,
            ILogger logger)
        {
            _dragStateService = dragStateService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Fin<LanguageExt.Unit>> Handle(CancelDragCommand request, CancellationToken cancellationToken)
        {
            _logger.Debug("Cancelling drag for BlockId: {BlockId}", request.BlockId);

            // Step 1: Verify drag is in progress for this block
            if (!_dragStateService.IsDragging || _dragStateService.DraggedBlockId != request.BlockId)
            {
                _logger.Warning("Cannot cancel drag - no active drag for BlockId {BlockId}", request.BlockId);
                return FinFail<LanguageExt.Unit>(Error.New("NO_ACTIVE_DRAG", $"No active drag operation for block {request.BlockId}"));
            }

            // Step 2: Get the original position before cancelling
            var originalPosition = _dragStateService.OriginalPosition;

            // Step 3: Cancel the drag operation
            var cancelResult = _dragStateService.CancelDrag();
            if (cancelResult.IsFail)
            {
                var error = cancelResult.Match<Error>(
                    Succ: _ => Error.New("UNKNOWN", "Unknown error"),
                    Fail: e => e
                );
                _logger.Error("Failed to cancel drag state: {Error}", error.Message);
                return FinFail<LanguageExt.Unit>(error);
            }

            // Step 4: Publish drag cancelled notification
            var notification = DragCancelledNotification.Create(request.BlockId, originalPosition);
            await _mediator.Publish(notification, cancellationToken);

            _logger.Debug("Successfully cancelled drag for BlockId {BlockId}", request.BlockId);
            return FinSucc(LanguageExt.Unit.Default);
        }
    }
}