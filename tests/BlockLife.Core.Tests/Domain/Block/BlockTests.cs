using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using FluentAssertions;
using System;
using Xunit;

namespace BlockLife.Core.Tests.Domain.Block
{
    /// <summary>
    /// Tests for Block domain entity, covering immutability, tier system, and factory methods.
    /// Part of VS_003B-2: Merge Execution with Result Position implementation.
    /// </summary>
    public class BlockTests
    {
        [Fact]
        public void CreateNew_WithValidParameters_CreatesBlockWithDefaultTier()
        {
            // Arrange
            var blockType = BlockType.Work;
            var position = new Vector2Int(5, 3);

            // Act
            var block = BlockLife.Core.Domain.Block.Block.CreateNew(blockType, position);

            // Assert
            block.Should().NotBeNull();
            block.Type.Should().Be(blockType);
            block.Position.Should().Be(position);
            block.Tier.Should().Be(1, "new blocks should default to Tier 1");
            block.Id.Should().NotBe(Guid.Empty);
            block.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            block.LastModifiedAt.Should().Be(block.CreatedAt);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        public void CreateNew_WithSpecificTier_CreatesBlockWithCorrectTier(int tier)
        {
            // Arrange
            var blockType = BlockType.Study;
            var position = new Vector2Int(0, 0);

            // Act
            var block = BlockLife.Core.Domain.Block.Block.CreateNew(blockType, position, tier);

            // Assert
            block.Type.Should().Be(blockType);
            block.Position.Should().Be(position);
            block.Tier.Should().Be(tier);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CreateNew_WithInvalidTier_ThrowsArgumentException(int invalidTier)
        {
            // Arrange
            var blockType = BlockType.Health;
            var position = new Vector2Int(1, 1);

            // Act & Assert
            var act = () => BlockLife.Core.Domain.Block.Block.CreateNew(blockType, position, invalidTier);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*tier must be positive*", "tier should be validated");
        }

        [Fact]
        public void MoveTo_PreservesTierValue()
        {
            // Arrange
            var originalBlock = BlockLife.Core.Domain.Block.Block.CreateNew(BlockType.Creativity, new Vector2Int(2, 2), 3);
            var newPosition = new Vector2Int(4, 4);

            // Act
            var movedBlock = originalBlock.MoveTo(newPosition);

            // Assert
            movedBlock.Position.Should().Be(newPosition);
            movedBlock.Tier.Should().Be(3, "tier should be preserved when moving");
            movedBlock.Type.Should().Be(originalBlock.Type);
            movedBlock.Id.Should().Be(originalBlock.Id);
            movedBlock.LastModifiedAt.Should().BeAfter(originalBlock.LastModifiedAt);
        }

        [Fact]
        public void Touch_PreservesTierValue()
        {
            // Arrange
            var originalBlock = BlockLife.Core.Domain.Block.Block.CreateNew(BlockType.Fun, new Vector2Int(7, 8), 2);

            // Act
            var touchedBlock = originalBlock.Touch();

            // Assert
            touchedBlock.Tier.Should().Be(2, "tier should be preserved when touching");
            touchedBlock.Type.Should().Be(originalBlock.Type);
            touchedBlock.Position.Should().Be(originalBlock.Position);
            touchedBlock.Id.Should().Be(originalBlock.Id);
            touchedBlock.LastModifiedAt.Should().BeAfter(originalBlock.LastModifiedAt);
        }

        [Fact]
        public void Tier_IsImmutable()
        {
            // Arrange & Act
            var block = BlockLife.Core.Domain.Block.Block.CreateNew(BlockType.Work, new Vector2Int(0, 0), 2);

            // Assert
            // Since Block is a record, Tier should be init-only and immutable
            block.Tier.Should().Be(2);
            // No way to modify Tier after creation - enforced at compile time
        }

        [Fact]
        public void ToString_IncludesTierInformation()
        {
            // Arrange
            var block = BlockLife.Core.Domain.Block.Block.CreateNew(BlockType.Work, new Vector2Int(1, 2), 3);

            // Act
            var result = block.ToString();

            // Assert
            result.Should().Contain("Work");
            result.Should().Contain("(1, 2)");
            result.Should().Contain("T3", "tier should be included in string representation");
        }

        [Theory]
        [InlineData(BlockType.Work)]
        [InlineData(BlockType.Study)]
        [InlineData(BlockType.Health)]
        [InlineData(BlockType.Relationship)]
        [InlineData(BlockType.Creativity)]
        [InlineData(BlockType.Fun)]
        [InlineData(BlockType.CareerOpportunity)]
        [InlineData(BlockType.Partnership)]
        [InlineData(BlockType.Passion)]
        public void CreateNew_WithAllBlockTypes_CreatesTier1Successfully(BlockType blockType)
        {
            // Arrange
            var position = new Vector2Int(3, 4);

            // Act
            var block = BlockLife.Core.Domain.Block.Block.CreateNew(blockType, position);

            // Assert
            block.Type.Should().Be(blockType);
            block.Tier.Should().Be(1, "all block types should default to Tier 1");
        }

        [Fact]
        public void HighTierBlocks_ShouldHaveCorrectProperties()
        {
            // Arrange - Creating higher tier blocks for merge system
            var tier2Block = BlockLife.Core.Domain.Block.Block.CreateNew(BlockType.Work, new Vector2Int(5, 5), 2);
            var tier3Block = BlockLife.Core.Domain.Block.Block.CreateNew(BlockType.Work, new Vector2Int(6, 6), 3);

            // Assert
            tier2Block.Tier.Should().Be(2);
            tier3Block.Tier.Should().Be(3);
            
            // Both should have same type and different positions
            tier2Block.Type.Should().Be(BlockType.Work);
            tier3Block.Type.Should().Be(BlockType.Work);
            tier2Block.Position.Should().NotBe(tier3Block.Position);
        }
    }
}