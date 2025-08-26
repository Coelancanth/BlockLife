using BlockLife.Core.Domain.Common;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;

namespace BlockLife.Core.Features.Block.Patterns
{
    /// <summary>
    /// Interface for recognizing specific types of patterns in the grid.
    /// Each implementation handles one pattern type (Match, Merge, Transmute, etc.).
    /// Pattern recognizers are pure functions that find patterns without side effects.
    /// Following ADR-001: Pattern Recognition Framework.
    /// </summary>
    public interface IPatternRecognizer
    {
        /// <summary>
        /// The type of pattern this recognizer can find.
        /// Used by the pattern engine to route recognition requests.
        /// </summary>
        PatternType SupportedType { get; }

        /// <summary>
        /// Whether this recognizer is currently enabled.
        /// Can be disabled for testing or when features are locked.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Unique identifier for this recognizer implementation.
        /// Used for logging, debugging, and configuration.
        /// </summary>
        string RecognizerId { get; }

        /// <summary>
        /// Finds all patterns of this type starting from or involving the trigger position.
        /// This is the core pattern recognition algorithm for this pattern type.
        /// Must be a pure function with no side effects on the grid state.
        /// </summary>
        /// <param name="gridService">Current grid state (read-only)</param>
        /// <param name="triggerPosition">Position that triggered pattern recognition</param>
        /// <param name="context">Additional context and constraints</param>
        /// <returns>Sequence of found patterns, or empty if none found</returns>
        Fin<Seq<IPattern>> Recognize(IGridStateService gridService, Vector2Int triggerPosition, PatternContext context);

        /// <summary>
        /// Performs a quick check to see if patterns of this type could potentially exist
        /// at the given position without doing full recognition.
        /// Used for optimization - avoid expensive recognition when impossible.
        /// </summary>
        /// <param name="gridService">Current grid state</param>
        /// <param name="position">Position to check</param>
        /// <returns>True if patterns might exist, false if definitely none</returns>
        bool CanRecognizeAt(IGridStateService gridService, Vector2Int position);

        /// <summary>
        /// Gets performance statistics for this recognizer.
        /// Used for monitoring and optimization.
        /// </summary>
        /// <returns>Performance metrics as key-value pairs</returns>
        Seq<(string Metric, double Value)> GetPerformanceMetrics();

        /// <summary>
        /// Validates that this recognizer's configuration is correct.
        /// Called during system startup to ensure recognizers are properly configured.
        /// </summary>
        /// <returns>Success or failure with error details</returns>
        Fin<Unit> ValidateConfiguration();
    }

    /// <summary>
    /// Extension methods for IPatternRecognizer to provide common functionality.
    /// </summary>
    public static class PatternRecognizerExtensions
    {
        /// <summary>
        /// Safely recognizes patterns with error handling and performance timing.
        /// Wraps the core Recognize method with additional monitoring.
        /// </summary>
        public static Fin<Seq<IPattern>> SafeRecognize(this IPatternRecognizer recognizer, 
            IGridStateService gridService, Vector2Int triggerPosition, PatternContext context)
        {
            try
            {
                if (!recognizer.IsEnabled)
                    return Seq<IPattern>.Empty;

                if (!recognizer.CanRecognizeAt(gridService, triggerPosition))
                    return Seq<IPattern>.Empty;

                var startTime = System.DateTime.UtcNow;
                var result = recognizer.Recognize(gridService, triggerPosition, context);
                var elapsed = (System.DateTime.UtcNow - startTime).TotalMilliseconds;

                return result.Match<Seq<IPattern>>(
                    Succ: patterns => 
                    {
                        // Log successful recognition if patterns found
                        if (patterns.Any())
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"Recognizer {recognizer.RecognizerId} found {patterns.Count} patterns in {elapsed:F1}ms");
                        }
                        return patterns;
                    },
                    Fail: error => 
                    {
                        // Log recognition error
                        System.Diagnostics.Debug.WriteLine(
                            $"Recognizer {recognizer.RecognizerId} failed: {error}");
                        return Seq<IPattern>.Empty;
                    }
                );
            }
            catch (System.Exception ex)
            {
                // Catch any unhandled exceptions and convert to Fin
                System.Diagnostics.Debug.WriteLine(
                    $"Recognizer {recognizer.RecognizerId} threw exception: {ex.Message}");
                return Seq<IPattern>.Empty;
            }
        }

        /// <summary>
        /// Checks if the recognizer supports a specific pattern type.
        /// </summary>
        public static bool Supports(this IPatternRecognizer recognizer, PatternType patternType) =>
            recognizer.SupportedType == patternType;

        /// <summary>
        /// Gets a description of what this recognizer does.
        /// </summary>
        public static string GetDescription(this IPatternRecognizer recognizer) =>
            $"{recognizer.RecognizerId} (Type: {recognizer.SupportedType}, Enabled: {recognizer.IsEnabled})";
    }
}