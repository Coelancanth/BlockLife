using BlockLife.Core.Features.Block.Patterns.Executors;
using BlockLife.Core.Features.Block.Patterns.Recognizers;
using BlockLife.Core.Features.Block.Patterns.Services;

namespace BlockLife.Core.Features.Block.Patterns
{
    /// <summary>
    /// Simple resolver that decides which executor to use for a pattern.
    /// Routes MatchPatterns to MergeExecutor when merge is unlocked.
    /// </summary>
    public class PatternExecutionResolver
    {
        private readonly IMergeUnlockService _mergeUnlockService;
        private readonly MatchPatternExecutor _matchExecutor;
        private readonly MergePatternExecutor _mergeExecutor;

        public PatternExecutionResolver(
            IMergeUnlockService mergeUnlockService,
            MatchPatternExecutor matchExecutor, 
            MergePatternExecutor mergeExecutor)
        {
            _mergeUnlockService = mergeUnlockService;
            _matchExecutor = matchExecutor;
            _mergeExecutor = mergeExecutor;
        }

        /// <summary>
        /// Determines which executor should handle the given pattern.
        /// If it's a MatchPattern and merge is unlocked for the tier, uses MergeExecutor.
        /// Otherwise uses the appropriate default executor.
        /// </summary>
        public IPatternExecutor ResolveExecutor(IPattern pattern)
        {
            // Simple logic - check for both tier 2 and tier 3 merge unlocks
            // TODO: Get actual tier from blocks when ExecutionContext includes it
            if (pattern is MatchPattern matchPattern)
            {
                // Check if any merge tier is unlocked for this block type
                // For VS_003B-1: Check both tier 2 and tier 3 unlocks
                if (_mergeUnlockService.IsMergeToTierUnlocked(matchPattern.MatchedBlockType, 2) ||
                    _mergeUnlockService.IsMergeToTierUnlocked(matchPattern.MatchedBlockType, 3))
                {
                    return _mergeExecutor;
                }
            }
            return _matchExecutor;
        }
    }
}