using BlockLife.Core.Domain.Block;

namespace BlockLife.Core.Features.Block.Patterns.Services
{
    /// <summary>
    /// Service for checking merge unlock status.
    /// Per Glossary: Each merge tier must be unlocked separately (merge-to-T2, merge-to-T3, etc).
    /// When merge-to-tier-(N+1) is unlocked, tier-N blocks merge instead of matching.
    /// </summary>
    public interface IMergeUnlockService
    {
        /// <summary>
        /// Checks if merging to a specific tier is unlocked for a block type.
        /// Example: IsMergeToTierUnlocked(BlockType.Work, 2) checks if Work-T1 can merge to Work-T2.
        /// </summary>
        /// <param name="blockType">The type of block to check</param>
        /// <param name="targetTier">The tier to merge TO (not from)</param>
        /// <returns>True if merge-to-tier-N is unlocked for this block type</returns>
        bool IsMergeToTierUnlocked(BlockType blockType, int targetTier);
    }
}