using BlockLife.Core.Domain.Turn;
using BlockLife.Core.Features.Block.Placement.Notifications;
using BlockLife.Core.Features.Turn.Commands;
using LanguageExt;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Turn.Notifications
{
    /// <summary>
    /// Handles block placement notifications by marking actions as performed in the turn system
    /// and optionally advancing turns when conditions are met.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public class TurnAdvancementHandler : INotificationHandler<BlockPlacedNotification>
    {
        private readonly ITurnManager _turnManager;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public TurnAdvancementHandler(
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
                _logger.Debug("Processing turn advancement after block placement at {Position}", notification.Position);

                // Step 1: Mark that an action has been performed this turn
                _turnManager.MarkActionPerformed();
                _logger.Debug("Marked action as performed for current turn");

                // Step 2: Check if turn advancement is available and auto-advance
                if (_turnManager.CanAdvanceTurn())
                {
                    _logger.Debug("Turn advancement is available, auto-advancing");
                    
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