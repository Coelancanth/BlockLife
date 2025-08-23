using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using LanguageExt;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Block.Patterns.Models
{
    /// <summary>
    /// Immutable record describing what WOULD happen if a pattern were executed.
    /// Represents the predicted effects without actually applying them to the game state.
    /// Used for UI previews, conflict resolution, and decision making.
    /// </summary>
    public sealed record PatternOutcome
    {
        /// <summary>
        /// Blocks that would be removed from the grid.
        /// </summary>
        public required Seq<Vector2Int> RemovedPositions { get; init; }

        /// <summary>
        /// Blocks that would be created and their positions.
        /// Typically used for tier-up patterns that create new blocks.
        /// </summary>
        public required Seq<(Vector2Int Position, BlockType Type)> CreatedBlocks { get; init; }

        /// <summary>
        /// Positions where existing blocks would be modified.
        /// Used for transmutation patterns that change block types.
        /// </summary>
        public required Seq<(Vector2Int Position, BlockType NewType)> ModifiedBlocks { get; init; }

        /// <summary>
        /// Resources that would be awarded to the player.
        /// Maps resource types to amounts gained.
        /// </summary>
        public required Seq<(string ResourceType, int Amount)> ResourceRewards { get; init; }

        /// <summary>
        /// Player attributes that would be increased.
        /// Maps attribute names to amounts gained.
        /// </summary>
        public required Seq<(string AttributeType, int Amount)> AttributeRewards { get; init; }

        /// <summary>
        /// Score or points that would be awarded.
        /// Used for achievement systems and leaderboards.
        /// </summary>
        public int ScoreReward { get; init; } = 0;

        /// <summary>
        /// Bonus multiplier applied to rewards based on pattern size or complexity.
        /// 1.0 = no bonus, 1.5 = 50% bonus, etc.
        /// </summary>
        public double BonusMultiplier { get; init; } = 1.0;

        /// <summary>
        /// Whether executing this pattern could trigger additional pattern recognition.
        /// Used to detect potential chain reactions.
        /// </summary>
        public bool CanTriggerChains { get; init; } = true;

        /// <summary>
        /// Creates an empty outcome (no effects).
        /// </summary>
        public static PatternOutcome Empty => new()
        {
            RemovedPositions = Seq<Vector2Int>(),
            CreatedBlocks = Seq<(Vector2Int, BlockType)>(),
            ModifiedBlocks = Seq<(Vector2Int, BlockType)>(),
            ResourceRewards = Seq<(string, int)>(),
            AttributeRewards = Seq<(string, int)>(),
            ScoreReward = 0,
            BonusMultiplier = 1.0,
            CanTriggerChains = false
        };

        /// <summary>
        /// Creates a simple removal outcome for match patterns.
        /// </summary>
        public static PatternOutcome CreateRemoval(Seq<Vector2Int> positions, 
            Seq<(string AttributeType, int Amount)> attributes = default, 
            double bonusMultiplier = 1.0) => new()
        {
            RemovedPositions = positions,
            CreatedBlocks = Seq<(Vector2Int, BlockType)>(),
            ModifiedBlocks = Seq<(Vector2Int, BlockType)>(),
            ResourceRewards = Seq<(string, int)>(),
            AttributeRewards = attributes.IsEmpty ? Seq<(string, int)>() : attributes,
            ScoreReward = positions.Count * 10, // Base 10 points per block
            BonusMultiplier = bonusMultiplier,
            CanTriggerChains = true
        };

        /// <summary>
        /// Calculates the total number of blocks affected by this outcome.
        /// </summary>
        public int TotalBlocksAffected => RemovedPositions.Count + CreatedBlocks.Count + ModifiedBlocks.Count;

        /// <summary>
        /// Calculates the final score reward including bonus multiplier.
        /// </summary>
        public int FinalScore => (int)(ScoreReward * BonusMultiplier);

        public override string ToString() => 
            $"PatternOutcome(Removed: {RemovedPositions.Count}, Created: {CreatedBlocks.Count}, Score: {FinalScore})";
    }
}