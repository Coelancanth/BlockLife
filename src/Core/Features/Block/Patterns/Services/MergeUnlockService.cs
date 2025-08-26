using BlockLife.Core.Domain.Block;
using BlockLife.Core.Infrastructure.Services;

namespace BlockLife.Core.Features.Block.Patterns.Services
{
    /// <summary>
    /// Implementation of merge unlock service using actual player progression data.
    /// Part of VS_003B-3: Uses PlayerState.MaxUnlockedTier to determine unlocks.
    /// </summary>
    public class MergeUnlockService : IMergeUnlockService
    {
        private readonly IPlayerStateService _playerStateService;

        public MergeUnlockService(IPlayerStateService playerStateService)
        {
            _playerStateService = playerStateService;
        }

        /// <summary>
        /// Checks if merging to a specific tier is unlocked for a block type.
        /// Uses actual player progression data from PlayerState.MaxUnlockedTier.
        /// </summary>
        public bool IsMergeToTierUnlocked(BlockType blockType, int targetTier)
        {
            // Get current player state
            var currentPlayer = _playerStateService.GetCurrentPlayer();
            if (currentPlayer.IsNone)
            {
                // No player available - default to match-only mode (no merges)
                return false;
            }

            var player = currentPlayer.Match(Some: p => p, None: () => throw new InvalidOperationException());
            
            // Player can merge TO target tier if they have unlocked it
            return targetTier <= player.MaxUnlockedTier;
        }
    }
}