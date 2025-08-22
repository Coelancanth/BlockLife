using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using FsCheck;
using FsCheck.Fluent;
using System;
using System.Linq;

namespace BlockLife.Core.Tests.Properties
{
    /// <summary>
    /// Custom generators for BlockLife domain types to support property-based testing.
    /// These generators create valid and invalid instances for testing invariants.
    /// Migrated to FsCheck 3.x API
    /// </summary>
    public static class BlockLifeGenerators
    {
        /// <summary>
        /// Generates valid grid positions within specified bounds.
        /// </summary>
        public static Gen<Vector2Int> ValidPosition(int width, int height)
        {
            return
                from x in Gen.Choose(0, width - 1)
                from y in Gen.Choose(0, height - 1)
                select new Vector2Int(x, y);
        }

        /// <summary>
        /// Generates positions that are guaranteed to be outside grid bounds.
        /// </summary>
        public static Gen<Vector2Int> InvalidPosition(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Grid dimensions must be positive");

            var invalidX = Gen.OneOf(
                Gen.Choose(-1000, -1),
                Gen.Choose(width, width + 1000)
            );

            var invalidY = Gen.OneOf(
                Gen.Choose(-1000, -1),
                Gen.Choose(height, height + 1000)
            );

            return Gen.OneOf(
                    // Invalid X, any Y
                    from x in invalidX
                    from y in Gen.Choose(-1000, 1000)
                    select new Vector2Int(x, y),

                    // Valid X, invalid Y (only if width > 0)
                    from x in Gen.Choose(0, Math.Max(0, width - 1))
                    from y in invalidY
                    select new Vector2Int(x, y),

                    // Invalid X and invalid Y
                    from x in invalidX
                    from y in invalidY
                    select new Vector2Int(x, y)
                );
        }

        /// <summary>
        /// Generates primary block types that can be directly placed.
        /// </summary>
        public static Gen<BlockType> PrimaryBlockType()
        {
            var primaryTypes = new[]
            {
                BlockType.Work,
                BlockType.Study,
                BlockType.Relationship,
                BlockType.Health,
                BlockType.Creativity,
                BlockType.Fun
            };

            return Gen.Elements(primaryTypes);
        }

        /// <summary>
        /// Generates special block types that require combinations.
        /// </summary>
        public static Gen<BlockType> SpecialBlockType()
        {
            var specialTypes = new[]
            {
                BlockType.CareerOpportunity,
                BlockType.Partnership,
                BlockType.Passion
            };

            return Gen.Elements(specialTypes);
        }

        /// <summary>
        /// Generates any valid block type.
        /// </summary>
        public static Gen<BlockType> AnyBlockType()
        {
            return Gen.Elements(Enum.GetValues<BlockType>());
        }

        /// <summary>
        /// Generates a valid block with specified constraints.
        /// </summary>
        public static Gen<Block> ValidBlock(int gridWidth = 10, int gridHeight = 10)
        {
            return
                from position in ValidPosition(gridWidth, gridHeight)
                from blockType in PrimaryBlockType()
                select Block.CreateNew(blockType, position);
        }

        /// <summary>
        /// Generates a sequence of non-overlapping positions within grid bounds.
        /// </summary>
        public static Gen<Vector2Int[]> NonOverlappingPositions(int gridWidth, int gridHeight, int maxCount = 10)
        {
            return
                Gen.Sized(size =>
                {
                    var count = Math.Min(size, Math.Min(maxCount, gridWidth * gridHeight));
                    var allPositions = AllValidPositions(gridWidth, gridHeight);
                    return Gen.Shuffle(allPositions)
                          .Select(positions => positions.Take(count).ToArray());
                });
        }

        /// <summary>
        /// Generates pairs of adjacent positions.
        /// </summary>
        public static Gen<(Vector2Int, Vector2Int)> AdjacentPositions(int gridWidth = 10, int gridHeight = 10)
        {
            return
                from pos in ValidPosition(gridWidth, gridHeight)
                from direction in Gen.Elements(new[] { (0, 1), (0, -1), (1, 0), (-1, 0) })
                let adjacent = new Vector2Int(pos.X + direction.Item1, pos.Y + direction.Item2)
                where adjacent.X >= 0 && adjacent.X < gridWidth &&
                      adjacent.Y >= 0 && adjacent.Y < gridHeight
                select (pos, adjacent);
        }

        /// <summary>
        /// Generates positive grid dimensions.
        /// </summary>
        public static Gen<(int width, int height)> GridDimensions()
        {
            return
                from width in Gen.Choose(1, 20)
                from height in Gen.Choose(1, 20)
                select (width, height);
        }

        /// <summary>
        /// Generates a sequence of valid command operations for testing.
        /// </summary>
        public static Gen<GridOperation[]> GridOperationSequence(int gridWidth, int gridHeight, int maxOperations = 20)
        {
            var placeOperation = from pos in ValidPosition(gridWidth, gridHeight)
                                 from blockType in PrimaryBlockType()
                                 select (GridOperation)new GridOperation.Place(pos, blockType);

            var removeOperation = from pos in ValidPosition(gridWidth, gridHeight)
                                  select (GridOperation)new GridOperation.Remove(pos);

            return Gen.Sized(size =>
            {
                var operationCount = Math.Min(size, maxOperations);
                return Gen.ArrayOf(Gen.OneOf(placeOperation, removeOperation)).Resize(operationCount)
                          ;
            });
        }

        /// <summary>
        /// Helper to generate all valid positions in a grid.
        /// </summary>
        private static Vector2Int[] AllValidPositions(int width, int height)
        {
            return (from x in Enumerable.Range(0, width)
                    from y in Enumerable.Range(0, height)
                    select new Vector2Int(x, y)).ToArray();
        }
    }

    /// <summary>
    /// Represents operations that can be performed on a grid for property testing.
    /// </summary>
    public abstract record GridOperation
    {
        public sealed record Place(Vector2Int Position, BlockType BlockType) : GridOperation;
        public sealed record Remove(Vector2Int Position) : GridOperation;
    }
}