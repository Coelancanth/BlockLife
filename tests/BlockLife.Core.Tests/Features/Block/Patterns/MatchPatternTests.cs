using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Patterns;
using BlockLife.Core.Features.Block.Patterns.Recognizers;
using BlockLife.Core.Infrastructure.Services;
using FluentAssertions;
using LanguageExt;
using static LanguageExt.Prelude;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace BlockLife.Core.Tests.Features.Block.Patterns
{
    /// <summary>
    /// Tests for MatchPattern data class.
    /// Validates pattern creation, validation, and outcome calculation.
    /// </summary>
    public class MatchPatternTests
    {
        #region Pattern Creation Tests

        [Fact]
        public void Create_WithValidThreeBlockLine_ReturnsMatchPattern()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );
            var blockType = BlockType.Work;

            // Act
            var result = MatchPattern.Create(positions, blockType);

            // Assert
            result.IsSome.Should().BeTrue();
            result.IfSome(pattern =>
            {
                pattern.MatchedBlockType.Should().Be(BlockType.Work);
                pattern.Positions.Count.Should().Be(3);
                pattern.Type.Should().Be(PatternType.Match);
                pattern.Priority.Should().Be(10); // Base priority (10) + size bonus (0 for size 3)
            });
        }

        [Fact]
        public void Create_WithValidFourBlockLShape_ReturnsMatchPattern()
        {
            // Arrange - L-shaped pattern
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(2, 1)
            );
            var blockType = BlockType.Health;

            // Act
            var result = MatchPattern.Create(positions, blockType);

            // Assert
            result.IsSome.Should().BeTrue();
            result.IfSome(pattern =>
            {
                pattern.MatchedBlockType.Should().Be(BlockType.Health);
                pattern.Positions.Count.Should().Be(4);
                pattern.Priority.Should().Be(11); // Base priority (10) + size bonus (1 for size 4)
            });
        }

        [Fact]
        public void Create_WithTwoBlocks_ReturnsNone()
        {
            // Arrange - Too few blocks for a match
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0)
            );
            var blockType = BlockType.Study;

            // Act
            var result = MatchPattern.Create(positions, blockType);

            // Assert
            result.IsNone.Should().BeTrue();
        }

        [Fact]
        public void Create_WithDisconnectedBlocks_ReturnsNone()
        {
            // Arrange - Blocks not connected (gap between them)
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(3, 0) // Gap at position (2,0)
            );
            var blockType = BlockType.Creativity;

            // Act
            var result = MatchPattern.Create(positions, blockType);

            // Assert
            result.IsNone.Should().BeTrue();
        }

        #endregion

        #region Pattern Validation Tests

        [Fact]
        public void IsValidFor_WithCorrectBlockTypes_ReturnsTrue()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );
            var blockType = BlockType.Fun;
            var pattern = MatchPattern.Create(positions, blockType).IfNone(() => throw new InvalidOperationException());

            var gridService = new Mock<IGridStateService>();
            gridService.Setup(gs => gs.GetBlockAt(new Vector2Int(0, 0))).Returns(Some(new BlockLife.Core.Domain.Block.Block 
                { Id = Guid.NewGuid(), Type = BlockType.Fun, Position = new Vector2Int(0, 0), CreatedAt = DateTime.Now, LastModifiedAt = DateTime.Now }));
            gridService.Setup(gs => gs.GetBlockAt(new Vector2Int(1, 0))).Returns(Some(new BlockLife.Core.Domain.Block.Block 
                { Id = Guid.NewGuid(), Type = BlockType.Fun, Position = new Vector2Int(1, 0), CreatedAt = DateTime.Now, LastModifiedAt = DateTime.Now }));
            gridService.Setup(gs => gs.GetBlockAt(new Vector2Int(2, 0))).Returns(Some(new BlockLife.Core.Domain.Block.Block 
                { Id = Guid.NewGuid(), Type = BlockType.Fun, Position = new Vector2Int(2, 0), CreatedAt = DateTime.Now, LastModifiedAt = DateTime.Now }));

            // Act
            var isValid = pattern.IsValidFor(gridService.Object);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void IsValidFor_WithMismatchedBlockType_ReturnsFalse()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );
            var blockType = BlockType.Work;
            var pattern = MatchPattern.Create(positions, blockType).IfNone(() => throw new InvalidOperationException());

            var gridService = new Mock<IGridStateService>();
            gridService.Setup(gs => gs.GetBlockAt(new Vector2Int(0, 0))).Returns(Some(new BlockLife.Core.Domain.Block.Block 
                { Id = Guid.NewGuid(), Type = BlockType.Work, Position = new Vector2Int(0, 0), CreatedAt = DateTime.Now, LastModifiedAt = DateTime.Now }));
            gridService.Setup(gs => gs.GetBlockAt(new Vector2Int(1, 0))).Returns(Some(new BlockLife.Core.Domain.Block.Block 
                { Id = Guid.NewGuid(), Type = BlockType.Study, Position = new Vector2Int(1, 0), CreatedAt = DateTime.Now, LastModifiedAt = DateTime.Now })); // Wrong type
            gridService.Setup(gs => gs.GetBlockAt(new Vector2Int(2, 0))).Returns(Some(new BlockLife.Core.Domain.Block.Block 
                { Id = Guid.NewGuid(), Type = BlockType.Work, Position = new Vector2Int(2, 0), CreatedAt = DateTime.Now, LastModifiedAt = DateTime.Now }));

            // Act
            var isValid = pattern.IsValidFor(gridService.Object);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void IsValidFor_WithMissingBlock_ReturnsFalse()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );
            var blockType = BlockType.Relationship;
            var pattern = MatchPattern.Create(positions, blockType).IfNone(() => throw new InvalidOperationException());

            var gridService = new Mock<IGridStateService>();
            gridService.Setup(gs => gs.GetBlockAt(new Vector2Int(0, 0))).Returns(Some(new BlockLife.Core.Domain.Block.Block 
                { Id = Guid.NewGuid(), Type = BlockType.Relationship, Position = new Vector2Int(0, 0), CreatedAt = DateTime.Now, LastModifiedAt = DateTime.Now }));
            gridService.Setup(gs => gs.GetBlockAt(new Vector2Int(1, 0))).Returns(Option<BlockLife.Core.Domain.Block.Block>.None); // Missing block
            gridService.Setup(gs => gs.GetBlockAt(new Vector2Int(2, 0))).Returns(Some(new BlockLife.Core.Domain.Block.Block 
                { Id = Guid.NewGuid(), Type = BlockType.Relationship, Position = new Vector2Int(2, 0), CreatedAt = DateTime.Now, LastModifiedAt = DateTime.Now }));

            // Act
            var isValid = pattern.IsValidFor(gridService.Object);

            // Assert
            isValid.Should().BeFalse();
        }

        #endregion

        #region Outcome Calculation Tests

        [Fact]
        public void CalculateOutcome_WithWorkBlocks_ReturnsMoneyAttribute()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );
            var pattern = MatchPattern.Create(positions, BlockType.Work).IfNone(() => throw new InvalidOperationException());

            // Act
            var outcome = pattern.CalculateOutcome();

            // Assert
            outcome.RemovedPositions.Should().Equal(positions);
            outcome.AttributeRewards.Should().ContainSingle()
                .Which.Should().Be(("Money", 10)); // Base value for Work (10) * size bonus (1.0)
            outcome.CanTriggerChains.Should().BeTrue();
        }

        [Fact]
        public void CalculateOutcome_WithFiveBlocks_IncludesSizeBonus()
        {
            // Arrange - 5 blocks should get 2.0x bonus
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(3, 0),
                new Vector2Int(4, 0)
            );
            var pattern = MatchPattern.Create(positions, BlockType.Health).IfNone(() => throw new InvalidOperationException());

            // Act
            var outcome = pattern.CalculateOutcome();

            // Assert
            outcome.AttributeRewards.Should().ContainSingle()
                .Which.Should().Be(("Health", 30)); // Base value for Health (15) * size bonus (2.0)
            outcome.BonusMultiplier.Should().Be(2.0);
        }

        [Fact]
        public void CalculateOutcome_WithBasicBlocks_ReturnsNoAttributes()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );
            var pattern = MatchPattern.Create(positions, BlockType.Basic).IfNone(() => throw new InvalidOperationException());

            // Act
            var outcome = pattern.CalculateOutcome();

            // Assert
            outcome.AttributeRewards.Should().BeEmpty();
            outcome.ScoreReward.Should().Be(30); // 10 points per block * 3 blocks
        }

        #endregion

        #region Conflict Detection Tests

        [Fact]
        public void ConflictsWith_WithOverlappingPositions_ReturnsTrue()
        {
            // Arrange
            var positions1 = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var positions2 = Seq(new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2)); // Overlaps at (1,0)

            var pattern1 = MatchPattern.Create(positions1, BlockType.Work).IfNone(() => throw new InvalidOperationException());
            var pattern2 = MatchPattern.Create(positions2, BlockType.Study).IfNone(() => throw new InvalidOperationException());

            // Act
            var conflicts = pattern1.ConflictsWith(pattern2);

            // Assert
            conflicts.Should().BeTrue();
        }

        [Fact]
        public void ConflictsWith_WithNoOverlappingPositions_ReturnsFalse()
        {
            // Arrange
            var positions1 = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var positions2 = Seq(new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)); // No overlap

            var pattern1 = MatchPattern.Create(positions1, BlockType.Creativity).IfNone(() => throw new InvalidOperationException());
            var pattern2 = MatchPattern.Create(positions2, BlockType.Fun).IfNone(() => throw new InvalidOperationException());

            // Act
            var conflicts = pattern1.ConflictsWith(pattern2);

            // Assert
            conflicts.Should().BeFalse();
        }

        #endregion

        #region Pattern Properties Tests

        [Fact]
        public void PatternId_IsUniquePerPattern()
        {
            // Arrange
            var positions1 = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var positions2 = Seq(new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1));

            var pattern1 = MatchPattern.Create(positions1, BlockType.Work).IfNone(() => throw new InvalidOperationException());
            var pattern2 = MatchPattern.Create(positions2, BlockType.Work).IfNone(() => throw new InvalidOperationException());

            // Act & Assert
            pattern1.PatternId.Should().NotBe(pattern2.PatternId);
            pattern1.PatternId.Should().Contain("Work");
            pattern1.PatternId.Should().Contain("3"); // Size
        }

        [Fact]
        public void GetDescription_ProvidesReadablePatternInfo()
        {
            // Arrange
            var positions = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var pattern = MatchPattern.Create(positions, BlockType.Health).IfNone(() => throw new InvalidOperationException());

            // Act
            var description = pattern.GetDescription();

            // Assert
            description.Should().Contain("Match 3 Health blocks");
            description.Should().Contain("(0, 0)");
        }

        #endregion
    }
}