using BlockLife.Core.Domain.Turn;
using BlockLife.Core.Features.Turn.Effects;
using BlockLife.Core.Infrastructure.Extensions;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Turn.Commands
{
    /// <summary>
    /// Handler for AdvanceTurnCommand - Implements functional CQRS pattern with Fin&lt;T&gt; monads.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public class AdvanceTurnCommandHandler : IRequestHandler<AdvanceTurnCommand, Fin<LanguageExt.Unit>>
    {
        private readonly ITurnManager _turnManager;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public AdvanceTurnCommandHandler(
            ITurnManager turnManager,
            IMediator mediator,
            ILogger logger)
        {
            _turnManager = turnManager;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Fin<LanguageExt.Unit>> Handle(AdvanceTurnCommand request, CancellationToken cancellationToken)
        {
            _logger.Debug("Processing AdvanceTurnCommand");

            // Step 1: Get current turn before advancing
            var currentTurnOption = _turnManager.GetCurrentTurn();
            if (currentTurnOption.IsNone)
            {
                var error = Error.New("TURN_NOT_INITIALIZED", "Turn system not initialized");
                _logger.Warning("Cannot advance turn: Turn system not initialized");
                return FinFail<LanguageExt.Unit>(error);
            }

            var currentTurn = currentTurnOption.Match(Some: t => t, None: () => throw new System.InvalidOperationException());

            // Step 2: Publish TurnEndNotification for current turn
            var turnEndResult = await PublishTurnEndNotification(currentTurn, cancellationToken);
            if (turnEndResult.IsFail)
            {
                var error = turnEndResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Warning("Failed to publish turn end notification: {Error}", error.Message);
                return FinFail<LanguageExt.Unit>(error);
            }

            // Step 3: Advance turn
            var advanceResult = _turnManager.AdvanceTurn();
            if (advanceResult.IsFail)
            {
                var error = advanceResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Warning("Failed to advance turn: {Error}", error.Message);
                return FinFail<LanguageExt.Unit>(error);
            }

            var newTurn = advanceResult.Match(Succ: t => t, Fail: _ => throw new System.InvalidOperationException());

            // Step 4: Publish TurnStartNotification for new turn
            var turnStartResult = await PublishTurnStartNotification(newTurn, cancellationToken);
            if (turnStartResult.IsFail)
            {
                var error = turnStartResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Warning("Failed to publish turn start notification: {Error}", error.Message);
                return FinFail<LanguageExt.Unit>(error);
            }

            _logger.Debug("Successfully advanced from turn {OldTurn} to turn {NewTurn}",
                currentTurn.Number, newTurn.Number);

            return FinSucc(LanguageExt.Unit.Default);
        }

        private async Task<Fin<LanguageExt.Unit>> PublishTurnEndNotification(
            Domain.Turn.Turn turn,
            CancellationToken cancellationToken)
        {
            var notification = TurnEndNotification.Create(turn);
            return await _mediator.Publish(notification, cancellationToken)
                .ToFin("TURN_END_NOTIFICATION_FAILED", "Failed to publish turn end notification");
        }

        private async Task<Fin<LanguageExt.Unit>> PublishTurnStartNotification(
            Domain.Turn.Turn turn,
            CancellationToken cancellationToken)
        {
            var notification = TurnStartNotification.Create(turn);
            return await _mediator.Publish(notification, cancellationToken)
                .ToFin("TURN_START_NOTIFICATION_FAILED", "Failed to publish turn start notification");
        }
    }
}