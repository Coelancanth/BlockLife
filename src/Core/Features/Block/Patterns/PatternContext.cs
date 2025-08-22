using BlockLife.Core.Domain.Common;
using LanguageExt;
using static LanguageExt.Prelude;
using System;

namespace BlockLife.Core.Features.Block.Patterns
{
    /// <summary>
    /// Immutable context record containing information needed for pattern recognition.
    /// Provides the environmental state at the time of pattern detection.
    /// Following functional programming principles with immutable data.
    /// </summary>
    public sealed record PatternContext
    {
        /// <summary>
        /// The grid position that triggered pattern recognition.
        /// This is typically where a block was just placed or moved.
        /// </summary>
        public required Vector2Int TriggerPosition { get; init; }

        /// <summary>
        /// The specific pattern types to search for at this time.
        /// Empty sequence means search for all enabled pattern types.
        /// </summary>
        public required Seq<PatternType> TargetPatternTypes { get; init; }

        /// <summary>
        /// Maximum number of patterns to find per recognizer.
        /// Prevents performance issues with complex grid states.
        /// </summary>
        public required int MaxPatternsPerType { get; init; }

        /// <summary>
        /// Timestamp when pattern recognition was initiated.
        /// Used for debugging and performance analysis.
        /// </summary>
        public required DateTime RecognitionStartedAt { get; init; }

        /// <summary>
        /// Optional context about the action that triggered pattern recognition.
        /// Could be "BlockPlaced", "BlockMoved", "ChainReaction", etc.
        /// </summary>
        public Option<string> TriggerAction { get; init; } = Option<string>.None;

        /// <summary>
        /// Creates a default pattern context for basic pattern recognition.
        /// </summary>
        public static PatternContext CreateDefault(Vector2Int triggerPosition) => new()
        {
            TriggerPosition = triggerPosition,
            TargetPatternTypes = Seq<PatternType>(), // Search all enabled types
            MaxPatternsPerType = 10,
            RecognitionStartedAt = DateTime.UtcNow,
            TriggerAction = Option<string>.None
        };

        /// <summary>
        /// Creates a pattern context for specific pattern types only.
        /// </summary>
        public static PatternContext CreateForTypes(Vector2Int triggerPosition, params PatternType[] targetTypes) => new()
        {
            TriggerPosition = triggerPosition,
            TargetPatternTypes = targetTypes.ToSeq(),
            MaxPatternsPerType = 10,
            RecognitionStartedAt = DateTime.UtcNow,
            TriggerAction = Option<string>.None
        };

        /// <summary>
        /// Creates a pattern context with additional trigger action information.
        /// </summary>
        public static PatternContext CreateWithAction(Vector2Int triggerPosition, string triggerAction) => new()
        {
            TriggerPosition = triggerPosition,
            TargetPatternTypes = Seq<PatternType>(),
            MaxPatternsPerType = 10,
            RecognitionStartedAt = DateTime.UtcNow,
            TriggerAction = Option<string>.Some(triggerAction)
        };

        /// <summary>
        /// Creates a copy of this context with a different maximum patterns limit.
        /// </summary>
        public PatternContext WithMaxPatterns(int maxPatterns) => this with
        {
            MaxPatternsPerType = maxPatterns
        };

        /// <summary>
        /// Creates a copy of this context with additional target pattern types.
        /// </summary>
        public PatternContext WithAdditionalTypes(params PatternType[] additionalTypes) => this with
        {
            TargetPatternTypes = TargetPatternTypes.Concat(additionalTypes)
        };

        public override string ToString() => 
            $"PatternContext(Trigger: {TriggerPosition}, Types: [{TargetPatternTypes.Count}], Action: {TriggerAction.Match(Some: a => a, None: () => "None")})";
    }
}