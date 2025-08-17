using BlockLife.Core.Features.Block.Drag.Services;
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
    /// Handler for StartDragCommand - Initiates a drag operation.
    /// Validates that the block exists and can be dragged.
    /// </summary>
    public class StartDragCommandHandler : IRequestHandler<StartDragCommand, Fin<LanguageExt.Unit>>
    {
        private readonly IGridStateService _gridStateService;
        private readonly IDragStateService _dragStateService;
        private readonly ILogger _logger;

        public StartDragCommandHandler(
            IGridStateService gridStateService,
            IDragStateService dragStateService,
            ILogger logger)
        {
            _gridStateService = gridStateService;
            _dragStateService = dragStateService;
            _logger = logger;
        }

        public Task<Fin<LanguageExt.Unit>> Handle(StartDragCommand request, CancellationToken cancellationToken)
        {
            _logger.Debug("Starting drag for BlockId: {BlockId} from Position: {StartPosition}",
                request.BlockId, request.StartPosition);

            // Step 1: Check if a drag is already in progress
            if (_dragStateService.IsDragging)
            {
                _logger.Warning("Cannot start drag - another drag operation is in progress");
                return Task.FromResult(FinFail<LanguageExt.Unit>(Error.New("DRAG_IN_PROGRESS", "Another drag operation is already in progress")));
            }

            // Step 2: Verify block exists
            var blockResult = _gridStateService.GetBlockById(request.BlockId);
            if (blockResult.IsNone)
            {
                _logger.Warning("Cannot start drag - block not found: {BlockId}", request.BlockId);
                return Task.FromResult(FinFail<LanguageExt.Unit>(Error.New("BLOCK_NOT_FOUND", $"Block {request.BlockId} not found")));
            }

            var block = blockResult.Match(
                Some: b => b,
                None: () => throw new System.InvalidOperationException("Block should exist")
            );

            // Step 3: Start the drag operation
            var startResult = _dragStateService.StartDrag(request.BlockId, block.Position);
            if (startResult.IsFail)
            {
                var error = startResult.Match<Error>(
                    Succ: _ => Error.New("UNKNOWN", "Unknown error"),
                    Fail: e => e
                );
                _logger.Error("Failed to start drag for BlockId {BlockId}: {Error}", 
                    request.BlockId, error.Message);
                return Task.FromResult(FinFail<LanguageExt.Unit>(error));
            }

            _logger.Debug("Successfully started drag for BlockId {BlockId}", request.BlockId);
            return Task.FromResult(FinSucc(LanguageExt.Unit.Default));
        }
    }
}