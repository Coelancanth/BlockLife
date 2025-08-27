using BlockLife.Core.Domain.Common;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Features.Spawn.Domain;
using FluentAssertions;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BlockLife.Core.Tests.Features.Spawn.Domain
{
    /// <summary>
    /// Comprehensive tests for RandomSpawnStrategy domain logic.
    /// Tests pure functional behavior, edge cases, and randomization patterns.
    /// Part of VS_007 Phase 1 - Domain Model testing.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Feature", "Spawn")]
    [Trait("Layer", "Domain")]
    public class RandomSpawnStrategyTests
    {
        private readonly Vector2Int _standardGridSize = new(5, 5);

        [Fact]
        public void StrategyName_Always_ReturnsCorrectName()
        {
            // Arrange
            var strategy = new RandomSpawnStrategy();

            // Act
            var name = strategy.StrategyName;

            // Assert
            name.Should().Be("Random Selection");
        }

        [Theory]
        [InlineData(42)]
        [InlineData(1337)]
        [InlineData(999)]
        public void Constructor_WithSeed_IsDeterministic(int seed)
        {
            // Arrange
            var emptyPositions = new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 1),
                new Vector2Int(2, 2),
                new Vector2Int(3, 3),
                new Vector2Int(4, 4)
            };

            var strategy1 = new RandomSpawnStrategy(seed);
            var strategy2 = new RandomSpawnStrategy(seed);

            // Act
            var result1 = strategy1.SelectSpawnPosition(emptyPositions, _standardGridSize);
            var result2 = strategy2.SelectSpawnPosition(emptyPositions, _standardGridSize);

            // Assert
            result1.IsSome.Should().BeTrue();
            result2.IsSome.Should().BeTrue();
            
            result1.IfNone(() => throw new Exception()).Should().Be(result2.IfNone(() => throw new Exception()));
        }

        [Fact]
        public void SelectSpawnPosition_EmptyCollection_ReturnsNone()
        {
            // Arrange
            var strategy = new RandomSpawnStrategy();
            var emptyPositions = Array.Empty<Vector2Int>();

            // Act
            var result = strategy.SelectSpawnPosition(emptyPositions, _standardGridSize);

            // Assert
            result.IsNone.Should().BeTrue();
        }

        [Fact]
        public void SelectSpawnPosition_NullCollection_ReturnsNone()
        {
            // Arrange
            var strategy = new RandomSpawnStrategy();
            IEnumerable<Vector2Int> nullPositions = null!;

            // Act
            var result = strategy.SelectSpawnPosition(nullPositions, _standardGridSize);

            // Assert
            result.IsNone.Should().BeTrue();
        }

        [Fact]
        public void SelectSpawnPosition_SinglePosition_ReturnsThatPosition()
        {
            // Arrange
            var strategy = new RandomSpawnStrategy();
            var expectedPosition = new Vector2Int(2, 3);
            var singlePosition = new[] { expectedPosition };

            // Act
            var result = strategy.SelectSpawnPosition(singlePosition, _standardGridSize);

            // Assert
            result.IsSome.Should().BeTrue();
            result.IfNone(() => throw new Exception()).Should().Be(expectedPosition);
        }

        [Fact]
        public void SelectSpawnPosition_MultiplePositions_ReturnsOneOfThem()
        {
            // Arrange
            var strategy = new RandomSpawnStrategy();
            var validPositions = new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 1),
                new Vector2Int(2, 2),
                new Vector2Int(4, 4)
            };

            // Act
            var result = strategy.SelectSpawnPosition(validPositions, _standardGridSize);

            // Assert
            result.IsSome.Should().BeTrue();
            var selectedPosition = result.IfNone(() => throw new Exception());
            validPositions.Should().Contain(selectedPosition);
        }

        [Fact]
        public void SelectSpawnPosition_MultipleCallsSameSeed_ProducesVariedResults()
        {
            // Arrange
            var seed = 12345;
            var positions = new[]
            {
                new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2),
                new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2),
                new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2)
            };

            // Act - Make multiple selections with same seed
            var results = new List<Vector2Int>();
            for (int i = 0; i < 20; i++)
            {
                var strategy = new RandomSpawnStrategy(seed + i); // Different seed each time
                var result = strategy.SelectSpawnPosition(positions, new Vector2Int(3, 3));
                result.IsSome.Should().BeTrue();
                results.Add(result.IfNone(() => throw new Exception()));
            }

            // Assert - Should get variety, not the same position every time
            results.Distinct().Count().Should().BeGreaterThan(1, "Random selection should produce variety");
        }

        [Fact]
        public void SelectBlockType_AlwaysReturnsPrimaryTypes()
        {
            // Arrange
            var strategy = new RandomSpawnStrategy();

            // Act - Test multiple turns
            var results = new List<BlockType>();
            for (int turn = 1; turn <= 50; turn++)
            {
                var blockType = strategy.SelectBlockType(turn);
                results.Add(blockType);
            }

            // Assert - Should only return primary types, never special types
            var primaryTypes = new[]
            {
                BlockType.Work, BlockType.Study, BlockType.Health,
                BlockType.Relationship, BlockType.Creativity, BlockType.Fun
            };

            results.Should().OnlyContain(bt => primaryTypes.Contains(bt));
        }

        [Fact]
        public void SelectBlockType_NeverReturnsSpecialTypes()
        {
            // Arrange
            var strategy = new RandomSpawnStrategy();
            var specialTypes = new[] { BlockType.CareerOpportunity, BlockType.Partnership, BlockType.Passion };

            // Act - Test many selections
            var results = new List<BlockType>();
            for (int i = 0; i < 100; i++)
            {
                results.Add(strategy.SelectBlockType(i + 1));
            }

            // Assert
            results.Should().NotContain(specialTypes);
        }

        [Fact]
        public void SelectBlockType_ProducesVariety()
        {
            // Arrange
            var strategy = new RandomSpawnStrategy();

            // Act - Generate many block types
            var results = new List<BlockType>();
            for (int turn = 1; turn <= 200; turn++)
            {
                results.Add(strategy.SelectBlockType(turn));
            }

            // Assert - Should see multiple different types
            results.Distinct().Count().Should().BeGreaterThan(3, "Should produce variety of block types");
        }

        [Fact]
        public void SelectBlockType_FunBlocksAreRare()
        {
            // Arrange  
            var strategy = new RandomSpawnStrategy(12345); // Fixed seed for reproducible test
            
            // Act - Generate many selections
            var results = new List<BlockType>();
            for (int turn = 1; turn <= 1000; turn++)
            {
                results.Add(strategy.SelectBlockType(turn));
            }

            // Assert - Fun blocks should be less common (7/100 weight = ~7%)
            var funCount = results.Count(bt => bt == BlockType.Fun);
            var funPercentage = (double)funCount / results.Count * 100;
            
            funPercentage.Should().BeLessThan(15, "Fun blocks should be relatively rare");
            funPercentage.Should().BeGreaterThan(2, "But should still appear occasionally");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(999)]
        public void SelectBlockType_DifferentTurns_AllReturnValidTypes(int turnNumber)
        {
            // Arrange
            var strategy = new RandomSpawnStrategy();

            // Act
            var blockType = strategy.SelectBlockType(turnNumber);

            // Assert
            blockType.Should().BeOneOf(
                BlockType.Work, BlockType.Study, BlockType.Health,
                BlockType.Relationship, BlockType.Creativity, BlockType.Fun
            );
        }

        [Fact]
        public void SelectBlockType_SameSeed_ProducesDeterministicSequence()
        {
            // Arrange
            const int seed = 42;
            var strategy1 = new RandomSpawnStrategy(seed);
            var strategy2 = new RandomSpawnStrategy(seed);

            // Act - Generate sequence with both strategies
            var sequence1 = new List<BlockType>();
            var sequence2 = new List<BlockType>();

            for (int turn = 1; turn <= 10; turn++)
            {
                sequence1.Add(strategy1.SelectBlockType(turn));
                sequence2.Add(strategy2.SelectBlockType(turn));
            }

            // Assert - Sequences should be identical
            sequence1.Should().BeEquivalentTo(sequence2, options => options.WithStrictOrdering());
        }

        [Fact]
        public void EdgeCase_LargeGridSize_HandlesCorrectly()
        {
            // Arrange
            var strategy = new RandomSpawnStrategy();
            var largeGridSize = new Vector2Int(1000, 1000);
            var positions = new[] { new Vector2Int(500, 500) };

            // Act
            var result = strategy.SelectSpawnPosition(positions, largeGridSize);

            // Assert
            result.IsSome.Should().BeTrue();
            result.IfNone(() => throw new Exception()).Should().Be(new Vector2Int(500, 500));
        }

        [Fact]
        public void EdgeCase_NegativeTurnNumber_StillWorksCorrectly()
        {
            // Arrange
            var strategy = new RandomSpawnStrategy();

            // Act & Assert - Should not throw, should return valid type
            var blockType = strategy.SelectBlockType(-5);
            blockType.Should().BeOneOf(
                BlockType.Work, BlockType.Study, BlockType.Health,
                BlockType.Relationship, BlockType.Creativity, BlockType.Fun
            );
        }

        [Fact]
        public void WeightedSelection_Statistical_ProducesExpectedDistribution()
        {
            // Arrange - Use fixed seed for reproducible test
            var strategy = new RandomSpawnStrategy(987654);
            const int sampleSize = 10000;
            
            // Act - Generate large sample
            var results = new List<BlockType>();
            for (int i = 0; i < sampleSize; i++)
            {
                results.Add(strategy.SelectBlockType(i + 1));
            }

            // Assert - Check approximate distribution matches weights
            // Total weight: 20+18+22+18+15+7 = 100
            var workCount = results.Count(bt => bt == BlockType.Work);
            var healthCount = results.Count(bt => bt == BlockType.Health);
            var funCount = results.Count(bt => bt == BlockType.Fun);

            // Health (22%) should be more common than Work (20%)
            healthCount.Should().BeGreaterThan(workCount, "Health blocks should be more common than Work");
            
            // Work (20%) should be more common than Fun (7%)
            workCount.Should().BeGreaterThan(funCount * 2, "Work blocks should be much more common than Fun");
            
            // All types should appear at least once in large sample
            results.Should().Contain(BlockType.Work);
            results.Should().Contain(BlockType.Study);
            results.Should().Contain(BlockType.Health);
            results.Should().Contain(BlockType.Relationship);
            results.Should().Contain(BlockType.Creativity);
            results.Should().Contain(BlockType.Fun);
        }
    }
}