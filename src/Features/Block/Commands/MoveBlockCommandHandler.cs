using BlockLife.Core.Features.Block.Effects;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Block.Commands
{
    /// <summary>
    /// Handler for MoveBlockCommand - Implements functional CQRS pattern with Fin&lt;T&gt; monads.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public class MoveBlockCommandHandler : IRequestHandler<MoveBlockCommand, Fin<LanguageExt.Unit>>
    {
        private readonly IGridStateService _gridStateService;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public MoveBlockCommandHandler(
            IGridStateService gridStateService,
            IMediator mediator,
            ILogger logger)
        {
            _gridStateService = gridStateService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Fin<LanguageExt.Unit>> Handle(MoveBlockCommand request, CancellationToken cancellationToken)
        {
            _logger.Information("Processing MoveBlockCommand for BlockId: {BlockId} to Position: {ToPosition}", 
                request.BlockId, request.ToPosition);

            try
            {
                // Get the block to move
                var blockResult = _gridStateService.GetBlockById(request.BlockId);
                if (blockResult.IsNone)
                {
                    _logger.Warning("Block not found: {BlockId}", request.BlockId);
                    return Fin<LanguageExt.Unit>.Fail(Error.New("Block not found"));
                }

                var block = blockResult.Match(Some: b => b, None: () => throw new InvalidOperationException());
                var fromPosition = block.Position;

                // Validate the move
                var validationResult = ValidateMove(fromPosition, request.ToPosition);
                if (validationResult.IsFail)
                {
                    var error = validationResult.Match<Error?>(Succ: _ => null, Fail: e => e);
                    _logger.Warning("Validation failed for MoveBlockCommand: {Error}", error?.Message ?? "Unknown validation error");
                    return validationResult;
                }

                // Execute the move
                var moveResult = _gridStateService.MoveBlock(request.BlockId, request.ToPosition);
                if (moveResult.IsFail)
                {
                    var error = moveResult.Match<Error?>(Succ: _ => null, Fail: e => e);
                    _logger.Error("Failed to move block {BlockId}: {Error}", request.BlockId, error?.Message ?? "Unknown move error");
                    return Fin<LanguageExt.Unit>.Fail(error ?? Error.New("Move operation failed"));
                }

                // Publish notification on success
                var notification = BlockMovedNotification.Create(
                    request.BlockId,
                    fromPosition,
                    request.ToPosition
                );

                await _mediator.Publish(notification, cancellationToken);
                
                _logger.Information("Successfully moved block {BlockId} from {FromPosition} to {ToPosition}", 
                    request.BlockId, fromPosition, request.ToPosition);

                return Fin<LanguageExt.Unit>.Succ(LanguageExt.Unit.Default);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error during MoveBlockCommand handling");
                return Fin<LanguageExt.Unit>.Fail(Error.New($"Unexpected error: {ex.Message}"));
            }
        }

        private Fin<LanguageExt.Unit> ValidateMove(Domain.Common.Vector2Int fromPosition, Domain.Common.Vector2Int toPosition)
        {
            // Check if trying to move to same position
            if (fromPosition.Equals(toPosition))
            {
                return Error.New("Block is already at the target position");
            }

            // Check if target position is valid
            if (!_gridStateService.IsValidPosition(toPosition))
            {
                return Error.New($"Position {toPosition} is outside grid bounds");
            }

            // Check if target position is empty
            if (!_gridStateService.IsPositionEmpty(toPosition))
            {
                return Error.New($"Position {toPosition} is already occupied");
            }

            return LanguageExt.Unit.Default;
        }

    }
}