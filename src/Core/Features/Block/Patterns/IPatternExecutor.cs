using BlockLife.Core.Features.Block.Patterns.Models;
using LanguageExt;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Block.Patterns
{
    /// <summary>
    /// Interface for executing patterns against the game state.
    /// This is where patterns actually modify the grid, award points, and trigger effects.
    /// Pattern executors are responsible for the side effects of pattern processing.
    /// Following ADR-001: Pattern Recognition Framework.
    /// </summary>
    public interface IPatternExecutor
    {
        /// <summary>
        /// The type of pattern this executor can handle.
        /// Used by the pattern engine to route execution requests.
        /// </summary>
        PatternType SupportedType { get; }

        /// <summary>
        /// Unique identifier for this executor implementation.
        /// Used for logging, debugging, and configuration.
        /// </summary>
        string ExecutorId { get; }

        /// <summary>
        /// Whether this executor is currently enabled.
        /// Can be disabled for testing or when features are locked.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Executes a pattern against the current game state.
        /// This is the core pattern execution algorithm that applies all the pattern's effects.
        /// Must be safe for concurrent execution and handle partial failures gracefully.
        /// </summary>
        /// <param name="pattern">The pattern to execute</param>
        /// <param name="context">Execution context containing services and state</param>
        /// <returns>Result describing what happened during execution</returns>
        Task<Fin<ExecutionResult>> Execute(IPattern pattern, ExecutionContext context);

        /// <summary>
        /// Validates that a pattern can be executed in the current game state.
        /// Should verify all preconditions before attempting execution.
        /// Used to avoid partial executions that leave the game in invalid states.
        /// </summary>
        /// <param name="pattern">The pattern to validate</param>
        /// <param name="context">Current execution context</param>
        /// <returns>True if pattern can be safely executed</returns>
        Task<Fin<bool>> CanExecute(IPattern pattern, ExecutionContext context);

        /// <summary>
        /// Estimates the time this executor would need to process the given pattern.
        /// Used for performance planning and user feedback.
        /// </summary>
        /// <param name="pattern">Pattern to estimate execution time for</param>
        /// <returns>Estimated execution time in milliseconds</returns>
        double EstimateExecutionTime(IPattern pattern);

        /// <summary>
        /// Gets performance statistics for this executor.
        /// Used for monitoring and optimization.
        /// </summary>
        /// <returns>Performance metrics as key-value pairs</returns>
        Seq<(string Metric, double Value)> GetPerformanceMetrics();

        /// <summary>
        /// Validates that this executor's configuration is correct.
        /// Called during system startup to ensure executors are properly configured.
        /// </summary>
        /// <returns>Success or failure with error details</returns>
        Fin<Unit> ValidateConfiguration();
    }

    /// <summary>
    /// Execution context containing all the services and state needed for pattern execution.
    /// Passed to executors to provide access to game services without tight coupling.
    /// </summary>
    public sealed record ExecutionContext
    {
        /// <summary>
        /// Service for modifying the grid state.
        /// </summary>
        public required BlockLife.Core.Infrastructure.Services.IGridStateService GridService { get; init; }

        /// <summary>
        /// The position where the pattern was triggered (e.g., where the player acted).
        /// Used for merge patterns to determine where the merged block should appear.
        /// Per Glossary: "Result Position" - the last-acted position.
        /// </summary>
        public BlockLife.Core.Domain.Common.Vector2Int? TriggerPosition { get; init; }

        /// <summary>
        /// Timestamp when execution began.
        /// Used for performance monitoring and logging.
        /// </summary>
        public System.DateTime ExecutionStartedAt { get; init; } = System.DateTime.UtcNow;

        /// <summary>
        /// Maximum time allowed for this execution in milliseconds.
        /// Executors should abort if they exceed this timeout.
        /// </summary>
        public double TimeoutMs { get; init; } = 5000.0; // 5 second default timeout

        /// <summary>
        /// Additional context data that can be passed to executors.
        /// Used for executor-specific configuration or state.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> AdditionalData { get; init; } = new();

        /// <summary>
        /// Creates a default execution context with grid service.
        /// </summary>
        public static ExecutionContext Create(
            BlockLife.Core.Infrastructure.Services.IGridStateService gridService,
            BlockLife.Core.Domain.Common.Vector2Int? triggerPosition = null) => new()
        {
            GridService = gridService,
            TriggerPosition = triggerPosition,
            ExecutionStartedAt = System.DateTime.UtcNow,
            TimeoutMs = 5000.0,
            AdditionalData = new System.Collections.Generic.Dictionary<string, object>()
        };

        /// <summary>
        /// Creates an execution context with custom timeout.
        /// </summary>
        public static ExecutionContext CreateWithTimeout(
            BlockLife.Core.Infrastructure.Services.IGridStateService gridService, 
            double timeoutMs,
            BlockLife.Core.Domain.Common.Vector2Int? triggerPosition = null) => new()
        {
            GridService = gridService,
            TriggerPosition = triggerPosition,
            ExecutionStartedAt = System.DateTime.UtcNow,
            TimeoutMs = timeoutMs,
            AdditionalData = new System.Collections.Generic.Dictionary<string, object>()
        };

        /// <summary>
        /// Checks if the execution has exceeded its timeout.
        /// </summary>
        public bool IsTimedOut => (System.DateTime.UtcNow - ExecutionStartedAt).TotalMilliseconds > TimeoutMs;

        /// <summary>
        /// Gets the elapsed execution time in milliseconds.
        /// </summary>
        public double ElapsedMs => (System.DateTime.UtcNow - ExecutionStartedAt).TotalMilliseconds;
    }

    /// <summary>
    /// Extension methods for IPatternExecutor to provide common functionality.
    /// </summary>
    public static class PatternExecutorExtensions
    {
        /// <summary>
        /// Safely executes a pattern with comprehensive error handling and logging.
        /// Wraps the core Execute method with additional safety measures.
        /// </summary>
        public static async Task<Fin<ExecutionResult>> SafeExecute(this IPatternExecutor executor, 
            IPattern pattern, ExecutionContext context)
        {
            try
            {
                if (!executor.IsEnabled)
                    return LanguageExt.Common.Error.New($"Executor {executor.ExecutorId} is disabled");

                if (executor.SupportedType != pattern.Type)
                    return LanguageExt.Common.Error.New($"Executor {executor.ExecutorId} cannot handle pattern type {pattern.Type}");

                // Validate pattern can be executed
                var canExecuteResult = await executor.CanExecute(pattern, context);
                if (canExecuteResult.IsFail)
                    return canExecuteResult.Match<Fin<ExecutionResult>>(
                        Succ: _ => LanguageExt.Common.Error.New("Unexpected success in failed validation"),
                        Fail: error => error);

                var canExecute = canExecuteResult.Match(Succ: result => result, Fail: _ => false);
                if (!canExecute)
                    return LanguageExt.Common.Error.New("Pattern cannot be executed in current state");

                // Execute the pattern
                var startTime = System.DateTime.UtcNow;
                var result = await executor.Execute(pattern, context);
                var elapsed = (System.DateTime.UtcNow - startTime).TotalMilliseconds;

                return result.Match<Fin<ExecutionResult>>(
                    Succ: executionResult =>
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Executor {executor.ExecutorId} completed pattern {pattern.PatternId} in {elapsed:F1}ms");
                        return executionResult;
                    },
                    Fail: error =>
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Executor {executor.ExecutorId} failed on pattern {pattern.PatternId}: {error}");
                        return error;
                    }
                );
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Executor {executor.ExecutorId} threw exception on pattern {pattern.PatternId}: {ex.Message}");
                return LanguageExt.Common.Error.New($"Executor exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if the executor supports a specific pattern type.
        /// </summary>
        public static bool Supports(this IPatternExecutor executor, PatternType patternType) =>
            executor.SupportedType == patternType;

        /// <summary>
        /// Gets a description of what this executor does.
        /// </summary>
        public static string GetDescription(this IPatternExecutor executor) =>
            $"{executor.ExecutorId} (Type: {executor.SupportedType}, Enabled: {executor.IsEnabled})";
    }
}