using BlockLife.Core.Domain.Player;
using BlockLife.Core.Features.Player.Notifications;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Player.Commands
{
    /// <summary>
    /// Handler for ApplyMatchRewardsCommand - Implements functional CQRS pattern with Fin&lt;T&gt; monads.
    /// Core command for VS_003A match-3 system integration with player state.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public class ApplyMatchRewardsCommandHandler : IRequestHandler<ApplyMatchRewardsCommand, Fin<PlayerState>>
    {
        private readonly IPlayerStateService _playerStateService;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public ApplyMatchRewardsCommandHandler(
            IPlayerStateService playerStateService,
            IMediator mediator,
            ILogger logger)
        {
            _playerStateService = playerStateService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Fin<PlayerState>> Handle(ApplyMatchRewardsCommand request, CancellationToken cancellationToken)
        {
            _logger.Debug("Processing ApplyMatchRewardsCommand with {ResourceCount} resources and {AttributeCount} attributes. Description: {Description}",
                request.ResourceChanges.Count, request.AttributeChanges.Count, request.MatchDescription ?? "None");

            // Step 1: Validate current player exists
            var currentPlayerResult = GetCurrentPlayer();
            if (currentPlayerResult.IsFail)
            {
                var error = currentPlayerResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Warning("No current player found for ApplyMatchRewardsCommand: {Error}", error.Message);
                return FinFail<PlayerState>(error);
            }

            var currentPlayer = currentPlayerResult.Match(Succ: p => p, Fail: _ => throw new InvalidOperationException());

            // Step 2: Apply rewards atomically
            var rewardResult = ApplyRewards(request.ResourceChanges, request.AttributeChanges);
            if (rewardResult.IsFail)
            {
                var error = rewardResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Warning("Failed to apply rewards: {Error}", error.Message);
                return FinFail<PlayerState>(error);
            }

            var updatedPlayer = rewardResult.Match(Succ: p => p, Fail: _ => throw new InvalidOperationException());

            _logger.Debug("Successfully applied match rewards. Player {PlayerName} total score: {TotalScore} (was {PreviousScore})",
                updatedPlayer.Name, updatedPlayer.GetTotalScore(), currentPlayer.GetTotalScore());

            // Step 3: Publish notification for UI updates
            var notification = PlayerStateChangedNotification.Create(
                updatedPlayer,
                request.ResourceChanges,
                request.AttributeChanges,
                request.MatchDescription);

            await _mediator.Publish(notification, cancellationToken);

            // Return the updated player state (the ICommand<PlayerState> pattern)
            return FinSucc(updatedPlayer);
        }

        private Fin<PlayerState> GetCurrentPlayer()
        {
            var playerResult = _playerStateService.GetCurrentPlayer();
            return playerResult.Match(
                Some: player => FinSucc(player),
                None: () => FinFail<PlayerState>(Error.New("NO_CURRENT_PLAYER", "No current player available for reward application"))
            );
        }

        private Fin<PlayerState> ApplyRewards(
            Map<ResourceType, int> resourceChanges,
            Map<AttributeType, int> attributeChanges)
        {
            return _playerStateService.ApplyRewards(resourceChanges, attributeChanges);
        }
    }
}