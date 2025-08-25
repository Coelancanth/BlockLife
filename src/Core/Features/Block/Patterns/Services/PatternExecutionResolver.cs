using BlockLife.Core.Domain.Block;
using BlockLife.Core.Features.Block.Patterns.Executors;

namespace BlockLife.Core.Features.Block.Patterns.Services
{
    /// <summary>
    /// Resolves which executor to use for a pattern based on game state.
    /// Key decision: When merge is unlocked, match patterns become merge patterns.
    /// </summary>
    public class PatternExecutionResolver
    {
        private readonly IMergeUnlockService _mergeUnlockService;

        public PatternExecutionResolver(IMergeUnlockService mergeUnlockService)
        {
            _mergeUnlockService = mergeUnlockService;
        }

        /// <summary>
        /// Determines which executor should handle a pattern.
        /// For match patterns: checks if merge-to-next-tier is unlocked.
        /// </summary>
        public IPatternExecutor? ResolveExecutor(IPattern pattern)
        {
            // For now, return null to make tests compile but fail
            // We'll implement this after seeing test failures
            return null;
        }
    }
}