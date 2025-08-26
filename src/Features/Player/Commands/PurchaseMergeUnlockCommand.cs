using BlockLife.Core.Application.Commands;
using BlockLife.Core.Domain.Player;
using System;

namespace BlockLife.Core.Features.Player.Commands
{
    /// <summary>
    /// Command to purchase merge-to-tier unlock for the player.
    /// Part of VS_003B-3: Unlock Purchase System.
    /// Allows players to spend Money to unlock higher merge tiers.
    /// </summary>
    public sealed record PurchaseMergeUnlockCommand : ICommand<PlayerState>
    {
        /// <summary>
        /// The tier to unlock (2, 3, or 4).
        /// Must be the next sequential tier after player's current MaxUnlockedTier.
        /// </summary>
        public required int TargetTier { get; init; }

        /// <summary>
        /// Creates a new PurchaseMergeUnlockCommand for the specified tier.
        /// </summary>
        public static PurchaseMergeUnlockCommand Create(int targetTier) =>
            new()
            {
                TargetTier = targetTier
            };

        /// <summary>
        /// Gets the Money cost to unlock a specific tier.
        /// Cost formula: T2=100, T3=500, T4=2500
        /// </summary>
        public static int GetCostForTier(int tier) => tier switch
        {
            2 => 100,
            3 => 500,
            4 => 2500,
            _ => throw new ArgumentException($"Invalid tier {tier}. Valid tiers are 2, 3, 4.", nameof(tier))
        };
    }
}