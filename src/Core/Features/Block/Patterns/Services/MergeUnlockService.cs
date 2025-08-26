using BlockLife.Core.Domain.Block;

namespace BlockLife.Core.Features.Block.Patterns.Services
{
    /// <summary>
    /// Simple implementation of merge unlock service.
    /// For VS_003B-1, temporarily returns true for tier 2 merges to enable testing.
    /// </summary>
    public class MergeUnlockService : IMergeUnlockService
    {
        /// <summary>
        /// Checks if merging to a specific tier is unlocked for a block type.
        /// Temporarily hardcoded to allow merge-to-tier-2 and merge-to-tier-3 for testing.
        /// </summary>
        public bool IsMergeToTierUnlocked(BlockType blockType, int targetTier)
        {
            // For VS_003B-1 testing: Allow merging to tier 2 and tier 3
            // TODO: Replace with proper unlock tracking from player progression
            return targetTier == 2 || targetTier == 3;
        }
    }
}