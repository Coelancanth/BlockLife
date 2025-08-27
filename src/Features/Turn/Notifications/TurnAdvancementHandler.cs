using BlockLife.Core.Domain.Turn;
using BlockLife.Core.Features.Block.Placement.Notifications;
using BlockLife.Core.Features.Turn.Commands;
using LanguageExt;
using MediatR;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Turn.Notifications
{
    /// <summary>
    /// DEPRECATED: This handler incorrectly advances turns on block placement.
    /// Block placement (including cascades) should NOT consume turns.
    /// Only explicit user moves should advance turns.
    /// See TurnAdvancementAfterMoveHandler for correct implementation.
    /// </summary>
    [Obsolete("Use TurnAdvancementAfterMoveHandler instead. Block placement should not advance turns.")]
    public class TurnAdvancementHandler_DISABLED : INotificationHandler<BlockPlacedNotification>
    {
        private readonly ITurnManager _turnManager;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public TurnAdvancementHandler_DISABLED(
            ITurnManager turnManager,
            IMediator mediator,
            ILogger logger)
        {
            _turnManager = turnManager;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(BlockPlacedNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                var currentTurn = _turnManager.GetTurnsElapsed();
                _logger.Information("📍 BLOCK PLACED at {Position}, Turn {Turn} - checking turn advancement", 
                    notification.Position, currentTurn);

                // Step 1: Mark that an action has been performed this turn
                _turnManager.MarkActionPerformed();

                // Step 2: Check if turn advancement is available and auto-advance
                if (_turnManager.CanAdvanceTurn())
                {
                    _logger.Information("➡️ ADVANCING TURN after block placement");
                    
                    var command = AdvanceTurnCommand.Create();
                    var result = await _mediator.Send(command, cancellationToken);
                    
                    result.Match(
                        Succ: _ => _logger.Debug("Successfully auto-advanced turn after block placement"),
                        Fail: error => _logger.Warning("Failed to auto-advance turn: {Error}", error.Message)
                    );
                }
                else
                {
                    _logger.Debug("Turn advancement not available (conditions not met)");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing turn advancement after block placement at {Position}: {ErrorMessage}", 
                    notification.Position, ex.Message);
                // Don't throw - turn advancement failure shouldn't break the block placement
            }
        }
    }
}