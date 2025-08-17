using BlockLife.Core.Features.Block.Effects;
using BlockLife.Core.Infrastructure.Extensions;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

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
            _logger.Debug("Processing MoveBlockCommand for BlockId: {BlockId} to Position: {ToPosition}",
                request.BlockId, request.ToPosition);

            // Step 1: Get block
            var blockResult = GetBlockById(request.BlockId);
            if (blockResult.IsFail)
            {
                var error = blockResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Warning("Block not found for BlockId {BlockId}: {Error}", request.BlockId, error.Message);
                return FinFail<LanguageExt.Unit>(error);
            }

            var block = blockResult.Match(Succ: b => b, Fail: _ => throw new InvalidOperationException());
            var fromPosition = block.Position;

            // Step 2: Validate move
            var validationResult = ValidateMove(fromPosition, request.ToPosition);
            if (validationResult.IsFail)
            {
                var error = validationResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Warning("Validation failed for MoveBlockCommand: {Error}", error.Message);
                return FinFail<LanguageExt.Unit>(error);
            }

            // Step 3: Execute move
            var moveResult = ExecuteMove(request.BlockId, request.ToPosition);
            if (moveResult.IsFail)
            {
                var error = moveResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Error("Failed to move block {BlockId}: {Error}", request.BlockId, error.Message);
                return FinFail<LanguageExt.Unit>(error);
            }

            // Step 4: Publish notification (functional style)
            var publishResult = await PublishNotification(request.BlockId, fromPosition, request.ToPosition, cancellationToken);
            if (publishResult.IsFail)
            {
                var error = publishResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Warning("Failed to publish notification for BlockId {BlockId}: {Error}", request.BlockId, error.Message);
                return FinFail<LanguageExt.Unit>(error);
            }

            _logger.Debug("Successfully moved block {BlockId} from {FromPosition} to {ToPosition}",
                request.BlockId, fromPosition, request.ToPosition);

            return FinSucc(LanguageExt.Unit.Default);
        }

        private Fin<Domain.Block.Block> GetBlockById(System.Guid blockId)
        {
            var blockResult = _gridStateService.GetBlockById(blockId);
            return blockResult.Match(
                Some: block => FinSucc(block),
                None: () => FinFail<Domain.Block.Block>(Error.New("Block not found"))
            );
        }

        private Fin<LanguageExt.Unit> ExecuteMove(System.Guid blockId, Domain.Common.Vector2Int toPosition)
        {
            return _gridStateService.MoveBlock(blockId, toPosition).Map(_ => LanguageExt.Unit.Default);
        }

        private async Task<Fin<LanguageExt.Unit>> PublishNotification(
            System.Guid blockId,
            Domain.Common.Vector2Int fromPosition,
            Domain.Common.Vector2Int toPosition,
            CancellationToken cancellationToken)
        {
            var notification = BlockMovedNotification.Create(blockId, fromPosition, toPosition);
            return await _mediator.Publish(notification, cancellationToken)
                .ToFin("NOTIFICATION_PUBLISH_FAILED", "Failed to publish block moved notification");
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
