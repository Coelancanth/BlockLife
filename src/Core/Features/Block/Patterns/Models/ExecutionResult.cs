using LanguageExt;
using static LanguageExt.Prelude;
using System;

namespace BlockLife.Core.Features.Block.Patterns.Models
{
    /// <summary>
    /// Immutable record containing the results of actually executing a pattern.
    /// Captures what DID happen, including any errors or partial successes.
    /// Used for logging, debugging, and chain reaction detection.
    /// </summary>
    public sealed record ExecutionResult
    {
        /// <summary>
        /// Whether the pattern execution completed successfully.
        /// </summary>
        public required bool IsSuccess { get; init; }

        /// <summary>
        /// The outcome that was predicted before execution.
        /// </summary>
        public required PatternOutcome PredictedOutcome { get; init; }

        /// <summary>
        /// The actual outcome that occurred during execution.
        /// May differ from predicted due to concurrent modifications or errors.
        /// </summary>
        public required PatternOutcome ActualOutcome { get; init; }

        /// <summary>
        /// Error message if execution failed or encountered issues.
        /// </summary>
        public Option<string> ErrorMessage { get; init; } = Option<string>.None;

        /// <summary>
        /// Time taken to execute the pattern in milliseconds.
        /// Used for performance monitoring and optimization.
        /// </summary>
        public double ExecutionTimeMs { get; init; } = 0.0;

        /// <summary>
        /// Timestamp when execution completed.
        /// </summary>
        public DateTime CompletedAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Additional positions that should trigger pattern recognition for chain reactions.
        /// These are positions where new patterns might have emerged after this execution.
        /// </summary>
        public Seq<BlockLife.Core.Domain.Common.Vector2Int> ChainTriggerPositions { get; init; } = Seq<BlockLife.Core.Domain.Common.Vector2Int>();

        /// <summary>
        /// Creates a successful execution result.
        /// </summary>
        public static ExecutionResult Success(PatternOutcome predicted, PatternOutcome actual, double executionTimeMs = 0.0) => new()
        {
            IsSuccess = true,
            PredictedOutcome = predicted,
            ActualOutcome = actual,
            ErrorMessage = Option<string>.None,
            ExecutionTimeMs = executionTimeMs,
            CompletedAt = DateTime.UtcNow,
            ChainTriggerPositions = Seq<BlockLife.Core.Domain.Common.Vector2Int>()
        };

        /// <summary>
        /// Creates a successful execution result with chain trigger positions.
        /// </summary>
        public static ExecutionResult SuccessWithChains(PatternOutcome predicted, PatternOutcome actual, 
            Seq<BlockLife.Core.Domain.Common.Vector2Int> chainTriggers, double executionTimeMs = 0.0) => new()
        {
            IsSuccess = true,
            PredictedOutcome = predicted,
            ActualOutcome = actual,
            ErrorMessage = Option<string>.None,
            ExecutionTimeMs = executionTimeMs,
            CompletedAt = DateTime.UtcNow,
            ChainTriggerPositions = chainTriggers
        };

        /// <summary>
        /// Creates a failed execution result.
        /// </summary>
        public static ExecutionResult Failure(PatternOutcome predicted, string errorMessage, double executionTimeMs = 0.0) => new()
        {
            IsSuccess = false,
            PredictedOutcome = predicted,
            ActualOutcome = PatternOutcome.Empty,
            ErrorMessage = Option<string>.Some(errorMessage),
            ExecutionTimeMs = executionTimeMs,
            CompletedAt = DateTime.UtcNow,
            ChainTriggerPositions = Seq<BlockLife.Core.Domain.Common.Vector2Int>()
        };

        /// <summary>
        /// Checks if the actual outcome matched the predicted outcome.
        /// </summary>
        public bool OutcomeMatchesPrediction => IsSuccess && 
            PredictedOutcome.RemovedPositions.SequenceEqual(ActualOutcome.RemovedPositions) &&
            PredictedOutcome.CreatedBlocks.SequenceEqual(ActualOutcome.CreatedBlocks) &&
            PredictedOutcome.ModifiedBlocks.SequenceEqual(ActualOutcome.ModifiedBlocks);

        /// <summary>
        /// Checks if chain reactions should be triggered based on this execution.
        /// </summary>
        public bool ShouldTriggerChains => IsSuccess && 
            ActualOutcome.CanTriggerChains && 
            ChainTriggerPositions.Any();

        public override string ToString() => IsSuccess switch
        {
            true => $"ExecutionResult(Success: {ActualOutcome.TotalBlocksAffected} blocks affected, {ExecutionTimeMs:F1}ms)",
            false => $"ExecutionResult(Failed: {ErrorMessage.Match(Some: e => e, None: () => "Unknown error")})"
        };
    }
}