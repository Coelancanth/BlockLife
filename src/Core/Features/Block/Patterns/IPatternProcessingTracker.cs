using System.Threading;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Block.Patterns
{
    /// <summary>
    /// Tracks active pattern processing operations to ensure proper synchronization
    /// between pattern cascades and turn advancement.
    /// Prevents race conditions where auto-spawn occurs during pattern processing.
    /// </summary>
    public interface IPatternProcessingTracker
    {
        /// <summary>
        /// Increments the count of active pattern processing operations.
        /// Call this when starting to process patterns at a position.
        /// </summary>
        void BeginProcessing();

        /// <summary>
        /// Decrements the count of active pattern processing operations.
        /// Call this when pattern processing completes (including all cascades).
        /// </summary>
        void EndProcessing();

        /// <summary>
        /// Gets whether any pattern processing is currently active.
        /// </summary>
        bool IsProcessing { get; }

        /// <summary>
        /// Gets the count of active pattern processing operations.
        /// </summary>
        int ActiveProcessingCount { get; }

        /// <summary>
        /// Waits until all pattern processing operations complete.
        /// Use this before advancing turns to ensure grid stability.
        /// </summary>
        /// <param name="timeout">Maximum time to wait in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if all processing completed, false if timeout</returns>
        Task<bool> WaitForProcessingCompleteAsync(int timeout = 5000, CancellationToken cancellationToken = default);
    }
}