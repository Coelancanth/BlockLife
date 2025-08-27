using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Features.Spawn.Domain;
using BlockLife.Core.Features.Turn.Effects;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using MediatR;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Spawn.Notifications
{
    /// <summary>
    /// Handles turn start notifications by attempting to spawn a new block.
    /// Uses the Strategy pattern for pluggable spawn logic and gracefully handles full grids.
    /// Part of VS_007 Phase 2 - Application Layer integration.
    /// </summary>
    public sealed class AutoSpawnHandler : INotificationHandler<TurnStartNotification>
    {
        private readonly IAutoSpawnStrategy _spawnStrategy;
        private readonly IGridStateService _gridStateService;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public AutoSpawnHandler(
            IAutoSpawnStrategy spawnStrategy,
            IGridStateService gridStateService,
            IMediator mediator,
            ILogger logger)
        {
            _spawnStrategy = spawnStrategy ?? throw new ArgumentNullException(nameof(spawnStrategy));
            _gridStateService = gridStateService ?? throw new ArgumentNullException(nameof(gridStateService));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles turn start by attempting to auto-spawn a block.
        /// Gracefully handles full grid scenarios by skipping spawn without error.
        /// Uses functional approach with Option and Fin types.
        /// </summary>
        /// <param name="notification">Turn start notification containing the new turn</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        public async Task Handle(TurnStartNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                var turnNumber = notification.Turn.Number;
                _logger.Debug("Processing auto-spawn for Turn {TurnNumber} using {StrategyName}", 
                    turnNumber, _spawnStrategy.StrategyName);

                // Get grid dimensions and generate empty positions
                var gridSize = _gridStateService.GetGridDimensions();
                var emptyPositions = GetEmptyPositions(gridSize);

                // Use spawn strategy to select position
                var maybePosition = _spawnStrategy.SelectSpawnPosition(emptyPositions, gridSize);
                
                await maybePosition.Match(
                    Some: async selectedPosition =>
                    {
                        // Select block type for this turn
                        var blockType = _spawnStrategy.SelectBlockType(turnNumber);
                        
                        _logger.Debug("Auto-spawning {BlockType} block at {Position} for Turn {TurnNumber}", 
                            blockType, selectedPosition, turnNumber);

                        // Create and send place block command
                        var placeCommand = new PlaceBlockCommand(selectedPosition, blockType);
                        var result = await _mediator.Send(placeCommand, cancellationToken);

                        result.Match(
                            Succ: _ => _logger.Information("Auto-spawned {BlockType} block at {Position} for Turn {TurnNumber}", 
                                blockType, selectedPosition, turnNumber),
                            Fail: error => _logger.Warning("Failed to auto-spawn block for Turn {TurnNumber}: {Error}", 
                                turnNumber, error.Message)
                        );
                    },
                    None: () =>
                    {
                        _logger.Information("Skipping auto-spawn for Turn {TurnNumber} - grid is full", turnNumber);
                        return Task.CompletedTask;
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error during auto-spawn for Turn {TurnNumber}: {ErrorMessage}", 
                    notification.Turn.Number, ex.Message);
                
                // Don't rethrow - auto-spawn failure shouldn't break turn advancement
                // Game should continue even if spawn fails
            }
        }

        /// <summary>
        /// Generates all empty positions on the grid by checking each position.
        /// Uses lazy evaluation for efficiency with large grids.
        /// </summary>
        /// <param name="gridSize">Dimensions of the grid</param>
        /// <returns>Sequence of empty grid positions</returns>
        private IEnumerable<Vector2Int> GetEmptyPositions(Vector2Int gridSize)
        {
            for (int x = 0; x < gridSize.X; x++)
            {
                for (int y = 0; y < gridSize.Y; y++)
                {
                    var position = new Vector2Int(x, y);
                    if (_gridStateService.IsPositionEmpty(position))
                    {
                        yield return position;
                    }
                }
            }
        }
    }
}