using BlockLife.Core.Domain.Turn;
using BlockLife.Core.Features.Block.Effects;
using BlockLife.Core.Features.Turn.Commands;
using LanguageExt;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Turn.Notifications
{
    /// <summary>
    /// Handles block movement notifications by advancing turns.
    /// IMPORTANT: Only block MOVEMENT consumes turns, not block PLACEMENT.
    /// - Placing a new block: FREE (no turn consumed)
    /// - Moving an existing block: COSTS 1 TURN
    /// This creates strategic depth - initial placement is critical since fixes cost turns.
    /// </summary>
    public class TurnAdvancementAfterMoveHandler : INotificationHandler<BlockMovedNotification>
    {
        private readonly ITurnManager _turnManager;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public TurnAdvancementAfterMoveHandler(
            ITurnManager turnManager,
            IMediator mediator,
            ILogger logger)
        {
            _turnManager = turnManager;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(BlockMovedNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                var currentTurn = _turnManager.GetTurnsElapsed();
                _logger.Information("🎯 BLOCK MOVED from {FromPosition} to {ToPosition}, Turn {Turn} - advancing turn", 
                    notification.FromPosition, notification.ToPosition, currentTurn);

                // Mark that the user has performed their action for this turn
                _turnManager.MarkActionPerformed();

                // Check if turn advancement is available and auto-advance
                if (_turnManager.CanAdvanceTurn())
                {
                    _logger.Information("➡️ ADVANCING TURN after block movement");
                    
                    var command = AdvanceTurnCommand.Create();
                    var result = await _mediator.Send(command, cancellationToken);
                    
                    result.Match(
                        Succ: _ => _logger.Debug("Successfully auto-advanced turn after block movement"),
                        Fail: error => _logger.Warning("Failed to auto-advance turn: {Error}", error.Message)
                    );
                }
                else
                {
                    _logger.Debug("Turn advancement not available (conditions not met)");
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "Error processing turn advancement after block movement: {ErrorMessage}", 
                    ex.Message);
                // Don't throw - turn advancement failure shouldn't break the block movement
            }
        }
    }
}