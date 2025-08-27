using BlockLife.Core.Domain.Player;
using BlockLife.Core.Domain.Turn;
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
    /// Handler for PurchaseMergeUnlockCommand - VS_003B-3 implementation.
    /// Validates cost, spends Money, and updates player's MaxUnlockedTier.
    /// Following established command handler patterns with Fin&lt;T&gt; monads.
    /// </summary>
    public class PurchaseMergeUnlockCommandHandler : IRequestHandler<PurchaseMergeUnlockCommand, Fin<PlayerState>>
    {
        private readonly IPlayerStateService _playerStateService;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public PurchaseMergeUnlockCommandHandler(
            IPlayerStateService playerStateService,
            IMediator mediator,
            ILogger logger)
        {
            _playerStateService = playerStateService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Fin<PlayerState>> Handle(PurchaseMergeUnlockCommand request, CancellationToken cancellationToken)
        {
            _logger.Debug("Processing PurchaseMergeUnlockCommand for tier {TargetTier}", request.TargetTier);

            // Step 1: Validate current player exists
            var currentPlayerResult = GetCurrentPlayer();
            if (currentPlayerResult.IsFail)
            {
                var error = currentPlayerResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Warning("No current player found for PurchaseMergeUnlockCommand: {Error}", error.Message);
                return FinFail<PlayerState>(error);
            }

            var currentPlayer = currentPlayerResult.Match(Succ: p => p, Fail: _ => throw new InvalidOperationException());

            // Step 2: Validate tier and costs
            var validationResult = ValidatePurchase(currentPlayer, request.TargetTier);
            if (validationResult.IsFail)
            {
                var error = validationResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Warning("Purchase validation failed: {Error}", error.Message);
                return FinFail<PlayerState>(error);
            }

            // Step 3: Process purchase (spend money and unlock tier)
            var purchaseResult = ProcessPurchase(currentPlayer, request.TargetTier);
            if (purchaseResult.IsFail)
            {
                var error = purchaseResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Warning("Failed to process purchase: {Error}", error.Message);
                return FinFail<PlayerState>(error);
            }

            var updatedPlayer = purchaseResult.Match(Succ: p => p, Fail: _ => throw new InvalidOperationException());

            _logger.Debug("Successfully purchased tier {TargetTier} unlock. Player {PlayerName} MaxUnlockedTier: {MaxUnlockedTier}",
                request.TargetTier, updatedPlayer.Name, updatedPlayer.MaxUnlockedTier);

            // DEBUG: Show money spent  
            var cost = PurchaseMergeUnlockCommand.GetCostForTier(request.TargetTier);
            
            _logger.Information("💰 PURCHASE: Money:-{Cost} (Unlocked Merge Tier {Tier})",
                cost, request.TargetTier);

            // Step 4: Publish notification for UI updates
            var notification = PlayerStateChangedNotification.Create(
                updatedPlayer,
                Map((ResourceType.Money, -PurchaseMergeUnlockCommand.GetCostForTier(request.TargetTier))),
                Map<AttributeType, int>(),
                $"Purchased Merge Tier {request.TargetTier} Unlock");

            await _mediator.Publish(notification, cancellationToken);

            return FinSucc(updatedPlayer);
        }

        private Fin<PlayerState> GetCurrentPlayer()
        {
            var playerResult = _playerStateService.GetCurrentPlayer();
            return playerResult.Match(
                Some: player => FinSucc(player),
                None: () => FinFail<PlayerState>(Error.New("NO_CURRENT_PLAYER", "No current player available for purchase"))
            );
        }

        private Fin<LanguageExt.Unit> ValidatePurchase(PlayerState player, int targetTier)
        {
            // Validate tier range
            if (targetTier < 2 || targetTier > 4)
            {
                return FinFail<LanguageExt.Unit>(Error.New("INVALID_TIER", $"Invalid tier {targetTier}. Valid tiers are 2, 3, 4."));
            }

            // Validate not already unlocked
            if (targetTier <= player.MaxUnlockedTier)
            {
                return FinFail<LanguageExt.Unit>(Error.New("ALREADY_UNLOCKED", 
                    $"Tier {targetTier} is already unlocked. Current max: T{player.MaxUnlockedTier}"));
            }

            // Validate sequential unlocking (can't skip tiers)
            if (targetTier != player.MaxUnlockedTier + 1)
            {
                return FinFail<LanguageExt.Unit>(Error.New("NON_SEQUENTIAL_UNLOCK", 
                    $"Must unlock tiers sequentially. Current: T{player.MaxUnlockedTier}, Requested: T{targetTier}"));
            }

            // Validate affordability
            var cost = PurchaseMergeUnlockCommand.GetCostForTier(targetTier);
            if (player.GetResource(ResourceType.Money) < cost)
            {
                return FinFail<LanguageExt.Unit>(Error.New("INSUFFICIENT_MONEY", 
                    $"Insufficient Money. Required: {cost}, Available: {player.GetResource(ResourceType.Money)}"));
            }

            return FinSucc(unit);
        }

        private Fin<PlayerState> ProcessPurchase(PlayerState player, int targetTier)
        {
            var cost = PurchaseMergeUnlockCommand.GetCostForTier(targetTier);
            
            // Spend money and unlock tier
            var spendResult = player.SpendResource(ResourceType.Money, cost);
            if (spendResult.IsNone)
            {
                return FinFail<PlayerState>(Error.New("SPEND_FAILED", "Failed to spend Money for unlock"));
            }

            var playerWithSpentMoney = spendResult.Match(Some: p => p, None: () => throw new InvalidOperationException());
            
            // Update MaxUnlockedTier
            var updatedPlayer = playerWithSpentMoney with { MaxUnlockedTier = targetTier };

            // Save the updated player state back to the service
            var saveResult = _playerStateService.UpdatePlayer(updatedPlayer);
            if (saveResult.IsFail)
            {
                var error = saveResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                return FinFail<PlayerState>(error);
            }

            return FinSucc(updatedPlayer);
        }
    }
}