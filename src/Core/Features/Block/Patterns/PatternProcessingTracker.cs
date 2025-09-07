using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Block.Patterns
{
    /// <summary>
    /// Thread-safe implementation of pattern processing tracker.
    /// Uses interlocked operations to track concurrent pattern processing.
    /// </summary>
    public class PatternProcessingTracker : IPatternProcessingTracker
    {
        private readonly ILogger<PatternProcessingTracker>? _logger;
        private int _activeProcessingCount = 0;
        private TaskCompletionSource<bool>? _processingComplete;
        private readonly object _lock = new();

        public PatternProcessingTracker(ILogger<PatternProcessingTracker>? logger = null)
        {
            _logger = logger;
        }

        public void BeginProcessing()
        {
            var count = Interlocked.Increment(ref _activeProcessingCount);
            _logger?.LogDebug("Pattern processing started. Active count: {Count}", count);
            
            // Reset the completion source when processing begins
            lock (_lock)
            {
                if (_processingComplete?.Task.IsCompleted == true)
                {
                    _processingComplete = null;
                }
            }
        }

        public void EndProcessing()
        {
            var count = Interlocked.Decrement(ref _activeProcessingCount);
            _logger?.LogDebug("Pattern processing ended. Active count: {Count}", count);
            
            // Signal completion if this was the last active processing
            if (count == 0)
            {
                lock (_lock)
                {
                    _processingComplete?.TrySetResult(true);
                }
                _logger?.LogDebug("All pattern processing complete");
            }
            else if (count < 0)
            {
                // This shouldn't happen - log error and reset
                _logger?.LogError("Pattern processing count went negative: {Count}. Resetting to 0.", count);
                Interlocked.Exchange(ref _activeProcessingCount, 0);
            }
        }

        public bool IsProcessing => _activeProcessingCount > 0;

        public int ActiveProcessingCount => _activeProcessingCount;

        public async Task<bool> WaitForProcessingCompleteAsync(int timeout = 5000, CancellationToken cancellationToken = default)
        {
            // Fast path: already complete
            if (_activeProcessingCount == 0)
            {
                _logger?.LogDebug("No active pattern processing, returning immediately");
                return true;
            }

            // Create or get the completion source
            TaskCompletionSource<bool> tcs;
            lock (_lock)
            {
                if (_processingComplete == null || _processingComplete.Task.IsCompleted)
                {
                    _processingComplete = new TaskCompletionSource<bool>();
                }
                tcs = _processingComplete;
            }

            _logger?.LogDebug("Waiting for {Count} pattern processing operations to complete (timeout: {Timeout}ms)", 
                _activeProcessingCount, timeout);

            // Wait with timeout and cancellation
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            try
            {
                // Check again in case processing completed while we were setting up
                if (_activeProcessingCount == 0)
                {
                    _logger?.LogDebug("Pattern processing completed during setup");
                    return true;
                }

                await tcs.Task.WaitAsync(cts.Token);
                _logger?.LogDebug("Pattern processing complete");
                return true;
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger?.LogDebug("Wait for pattern processing cancelled by user");
                    throw;
                }
                
                _logger?.LogWarning("Timeout waiting for pattern processing to complete. Active count: {Count}", 
                    _activeProcessingCount);
                return false;
            }
        }
    }
}