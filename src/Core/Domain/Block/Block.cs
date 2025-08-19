using BlockLife.Core.Domain.Common;
using Newtonsoft.Json;
using System;

namespace BlockLife.Core.Domain.Block
{
    /// <summary>
    /// Represents a single block in the grid with its type, position, and state.
    /// Immutable record that serves as the core domain entity for block management.
    /// </summary>
    public sealed record Block
    {
        /// <summary>
        /// Unique identifier for this block instance.
        /// </summary>
        public required Guid Id { get; init; }

        /// <summary>
        /// The type of block, determining its properties and behaviors.
        /// </summary>
        public required BlockType Type { get; init; }

        /// <summary>
        /// Current position of the block in the grid.
        /// </summary>
        public required Vector2Int Position { get; init; }

        /// <summary>
        /// Timestamp when this block was created.
        /// </summary>
        public required DateTime CreatedAt { get; init; }

        /// <summary>
        /// Timestamp when this block was last modified.
        /// </summary>
        public required DateTime LastModifiedAt { get; init; }

        /// <summary>
        /// Default constructor required for record initialization with required properties.
        /// </summary>
        public Block()
        {
        }

        /// <summary>
        /// JsonConstructor for Newtonsoft.Json deserialization on Linux.
        /// Ensures cross-platform compatibility with required properties.
        /// </summary>
        [JsonConstructor]
        public Block(Guid id, BlockType type, Vector2Int position, DateTime createdAt, DateTime lastModifiedAt)
        {
            Id = id;
            Type = type;
            Position = position;
            CreatedAt = createdAt;
            LastModifiedAt = lastModifiedAt;
        }

        /// <summary>
        /// Creates a new block at the specified position.
        /// </summary>
        public static Block CreateNew(BlockType type, Vector2Int position)
        {
            var now = DateTime.UtcNow;
            return new Block
            {
                Id = Guid.NewGuid(),
                Type = type,
                Position = position,
                CreatedAt = now,
                LastModifiedAt = now
            };
        }

        /// <summary>
        /// Creates a copy of this block with a new position.
        /// Updates the LastModifiedAt timestamp.
        /// </summary>
        public Block MoveTo(Vector2Int newPosition) => this with
        {
            Position = newPosition,
            LastModifiedAt = DateTime.UtcNow
        };

        /// <summary>
        /// Creates a copy of this block with an updated LastModifiedAt timestamp.
        /// Used when the block is affected by game events without changing position.
        /// </summary>
        public Block Touch() => this with { LastModifiedAt = DateTime.UtcNow };

        /// <summary>
        /// Calculates if this block is adjacent to another block.
        /// </summary>
        public bool IsAdjacentTo(Block other) => Position.IsAdjacentTo(other.Position);

        /// <summary>
        /// Calculates if this block is orthogonally adjacent to another block.
        /// </summary>
        public bool IsOrthogonallyAdjacentTo(Block other) => Position.IsOrthogonallyAdjacentTo(other.Position);

        /// <summary>
        /// Gets the Manhattan distance to another block.
        /// </summary>
        public int DistanceTo(Block other) => Position.ManhattanDistanceTo(other.Position);

        /// <summary>
        /// Checks if this block can be combined with another block type.
        /// </summary>
        public bool CanCombineWith(BlockType otherType) => (Type, otherType) switch
        {
            (BlockType.Work, BlockType.Study) => true,
            (BlockType.Study, BlockType.Work) => true,
            (BlockType.Relationship, BlockType.Health) => true,
            (BlockType.Health, BlockType.Relationship) => true,
            (BlockType.Creativity, BlockType.Fun) => true,
            (BlockType.Fun, BlockType.Creativity) => true,
            _ => false
        };

        /// <summary>
        /// Gets the resulting block type from combining this block with another type.
        /// Returns null if combination is not valid.
        /// </summary>
        public BlockType? GetCombinationResult(BlockType otherType) => (Type, otherType) switch
        {
            (BlockType.Work, BlockType.Study) => BlockType.CareerOpportunity,
            (BlockType.Study, BlockType.Work) => BlockType.CareerOpportunity,
            (BlockType.Relationship, BlockType.Health) => BlockType.Partnership,
            (BlockType.Health, BlockType.Relationship) => BlockType.Partnership,
            (BlockType.Creativity, BlockType.Fun) => BlockType.Passion,
            (BlockType.Fun, BlockType.Creativity) => BlockType.Passion,
            _ => null
        };

        public override string ToString() => $"{Type.GetDisplayName()} at {Position}";
    }
}
