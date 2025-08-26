using LanguageExt;

namespace BlockLife.Core.Features.Block.Patterns
{
    /// <summary>
    /// Interface for resolving conflicts between multiple detected patterns.
    /// When multiple patterns compete for the same grid positions, the resolver
    /// determines which patterns should be executed and in what order.
    /// Following ADR-001: Pattern Recognition Framework.
    /// </summary>
    public interface IPatternResolver
    {
        /// <summary>
        /// Unique identifier for this resolver implementation.
        /// Used for logging, debugging, and configuration.
        /// </summary>
        string ResolverId { get; }

        /// <summary>
        /// The strategy this resolver uses to handle conflicts.
        /// Examples: "Priority", "HighestValue", "Random", "UserChoice"
        /// </summary>
        string ResolutionStrategy { get; }

        /// <summary>
        /// Resolves conflicts between multiple detected patterns and determines
        /// which patterns should be executed and in what order.
        /// This is the core conflict resolution algorithm.
        /// </summary>
        /// <param name="detectedPatterns">All patterns found by recognizers</param>
        /// <returns>Ordered sequence of non-conflicting patterns to execute</returns>
        Fin<Seq<IPattern>> Resolve(Seq<IPattern> detectedPatterns);

        /// <summary>
        /// Identifies groups of patterns that conflict with each other.
        /// Patterns conflict if they share any grid positions.
        /// Returns groups where each group contains conflicting patterns.
        /// </summary>
        /// <param name="patterns">Patterns to analyze for conflicts</param>
        /// <returns>Groups of conflicting patterns</returns>
        Seq<Seq<IPattern>> IdentifyConflictGroups(Seq<IPattern> patterns);

        /// <summary>
        /// From a group of conflicting patterns, selects the best one to execute.
        /// Uses the resolver's strategy to make the decision.
        /// </summary>
        /// <param name="conflictingPatterns">Patterns that compete for same positions</param>
        /// <returns>The pattern that should be executed, or failure if none valid</returns>
        Fin<IPattern> SelectBestPattern(Seq<IPattern> conflictingPatterns);

        /// <summary>
        /// Validates the resolver's configuration and strategy.
        /// Called during system startup to ensure resolver is properly configured.
        /// </summary>
        /// <returns>Success or failure with error details</returns>
        Fin<Unit> ValidateConfiguration();
    }

    /// <summary>
    /// Extension methods for IPatternResolver to provide common functionality.
    /// </summary>
    public static class PatternResolverExtensions
    {
        /// <summary>
        /// Safely resolves patterns with error handling and logging.
        /// Wraps the core Resolve method with additional monitoring.
        /// </summary>
        public static Fin<Seq<IPattern>> SafeResolve(this IPatternResolver resolver, Seq<IPattern> detectedPatterns)
        {
            try
            {
                if (!detectedPatterns.Any())
                    return Seq<IPattern>.Empty;

                var startTime = System.DateTime.UtcNow;
                var result = resolver.Resolve(detectedPatterns);
                var elapsed = (System.DateTime.UtcNow - startTime).TotalMilliseconds;

                return result.Match<Fin<Seq<IPattern>>>(
                    Succ: resolvedPatterns =>
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Resolver {resolver.ResolverId} processed {detectedPatterns.Count} patterns → {resolvedPatterns.Count} resolved in {elapsed:F1}ms");
                        return resolvedPatterns;
                    },
                    Fail: error =>
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Resolver {resolver.ResolverId} failed: {error}");
                        return LanguageExt.Common.Error.New($"Pattern resolution failed: {error}");
                    }
                );
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Resolver {resolver.ResolverId} threw exception: {ex.Message}");
                return LanguageExt.Common.Error.New($"Pattern resolver exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if any patterns in the sequence conflict with each other.
        /// </summary>
        public static bool HasConflicts(this IPatternResolver resolver, Seq<IPattern> patterns)
        {
            var conflictGroups = resolver.IdentifyConflictGroups(patterns);
            return conflictGroups.Any(group => group.Count > 1);
        }

        /// <summary>
        /// Counts the total number of conflicting pattern relationships.
        /// </summary>
        public static int CountConflicts(this IPatternResolver resolver, Seq<IPattern> patterns)
        {
            var conflictGroups = resolver.IdentifyConflictGroups(patterns);
            return conflictGroups.Sum(group => group.Count > 1 ? group.Count : 0);
        }

        /// <summary>
        /// Gets a description of the resolver's strategy and current state.
        /// </summary>
        public static string GetDescription(this IPatternResolver resolver) =>
            $"{resolver.ResolverId} (Strategy: {resolver.ResolutionStrategy})";
    }

    /// <summary>
    /// Common resolution strategies that can be referenced by implementations.
    /// These are standard approaches to pattern conflict resolution.
    /// </summary>
    public static class ResolutionStrategies
    {
        /// <summary>
        /// Select patterns with highest priority value first.
        /// Most common strategy - Merge beats Match, etc.
        /// </summary>
        public const string Priority = "Priority";

        /// <summary>
        /// Select patterns that provide the highest total value/score.
        /// Good for maximizing player rewards.
        /// </summary>
        public const string HighestValue = "HighestValue";

        /// <summary>
        /// Select patterns with the most blocks involved.
        /// Encourages large, complex patterns over small ones.
        /// </summary>
        public const string LargestFirst = "LargestFirst";

        /// <summary>
        /// Select patterns randomly when multiple options exist.
        /// Adds unpredictability to the game.
        /// </summary>
        public const string Random = "Random";

        /// <summary>
        /// Present options to player and wait for choice.
        /// Most interactive but can slow down gameplay.
        /// </summary>
        public const string UserChoice = "UserChoice";

        /// <summary>
        /// Execute all non-conflicting patterns simultaneously.
        /// Maximizes actions per turn but can be overwhelming.
        /// </summary>
        public const string ExecuteAll = "ExecuteAll";
    }
}