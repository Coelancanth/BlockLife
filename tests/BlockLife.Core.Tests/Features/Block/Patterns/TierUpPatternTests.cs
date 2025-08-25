using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Patterns.Recognizers;
using FluentAssertions;
using LanguageExt;
using static LanguageExt.Prelude;
using Xunit;
using System.Linq;

namespace BlockLife.Core.Tests.Features.Block.Patterns
{
    /// <summary>
    /// Tests for TierUpPattern data class behavior.
    /// Validates pattern creation, validation, and outcome calculation.
    /// </summary>
    public class TierUpPatternTests
    {
        #region Pattern Creation Tests

        [Fact]
        public void Create_WithExactlyThreeBlocks_ReturnsValidPattern()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );
            var blockType = BlockType.Work;

            // Act
            var result = TierUpPattern.Create(positions, blockType);

            // Assert
            result.IsSome.Should().BeTrue("exactly 3 connected blocks should create a valid tier-up pattern");
            result.IfSome(pattern =>
            {
                pattern.MergeBlockType.Should().Be(BlockType.Work);
                pattern.CurrentTier.Should().Be(1);
                pattern.Positions.Count.Should().Be(3);
                pattern.Type.Should().Be(Core.Features.Block.Patterns.PatternType.TierUp);
            });
        }

        [Fact]
        public void Create_WithTwoBlocks_ReturnsNone()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0)
            );
            var blockType = BlockType.Study;

            // Act
            var result = TierUpPattern.Create(positions, blockType);

            // Assert
            result.IsNone.Should().BeTrue("tier-up requires exactly 3 blocks, not 2");
        }

        [Fact]
        public void Create_WithFourBlocks_ReturnsNone()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(3, 0)
            );
            var blockType = BlockType.Health;

            // Act
            var result = TierUpPattern.Create(positions, blockType);

            // Assert
            result.IsNone.Should().BeTrue("tier-up requires exactly 3 blocks, not 4");
        }

        [Fact]
        public void Create_WithDisconnectedBlocks_ReturnsNone()
        {
            // Arrange - Three blocks but not connected
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(2, 0),  // Gap between first and second
                new Vector2Int(3, 0)
            );
            var blockType = BlockType.Creativity;

            // Act
            var result = TierUpPattern.Create(positions, blockType);

            // Assert
            result.IsNone.Should().BeTrue("disconnected blocks cannot form a tier-up pattern");
        }

        [Fact]
        public void Create_WithLShapeBlocks_ReturnsValidPattern()
        {
            // Arrange - L-shape configuration
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(1, 1)
            );
            var blockType = BlockType.Fun;

            // Act
            var result = TierUpPattern.Create(positions, blockType);

            // Assert
            result.IsSome.Should().BeTrue("L-shaped connected blocks should form a valid tier-up pattern");
        }

        #endregion

        #region Pattern Properties Tests

        [Fact]
        public void PatternId_IsUniquePerPattern()
        {
            // Arrange
            var positions1 = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );
            var positions2 = Seq(
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(2, 1)
            );

            // Act
            var pattern1 = TierUpPattern.Create(positions1, BlockType.Work).IfNone(() => null!);
            var pattern2 = TierUpPattern.Create(positions2, BlockType.Work).IfNone(() => null!);

            // Assert
            pattern1.Should().NotBeNull();
            pattern2.Should().NotBeNull();
            pattern1.PatternId.Should().NotBe(pattern2.PatternId, "different positions should generate different IDs");
            pattern1.PatternId.Should().StartWith("TierUp_Work_T1_3_");
        }

        [Fact]
        public void Priority_IsHigherThanMatchPatterns()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );

            // Act
            var pattern = TierUpPattern.Create(positions, BlockType.Study).IfNone(() => null!);

            // Assert
            pattern.Should().NotBeNull();
            pattern.Priority.Should().Be(20, "TierUp base priority is 20");
            pattern.Priority.Should().BeGreaterThan(10, "TierUp priority should be higher than Match priority (10)");
        }

        [Fact]
        public void GetDescription_ProvidesReadableInfo()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );

            // Act
            var pattern = TierUpPattern.Create(positions, BlockType.Relationship, currentTier: 2).IfNone(() => null!);
            var description = pattern.GetDescription();

            // Assert
            pattern.Should().NotBeNull();
            description.Should().Contain("TierUp");
            description.Should().Contain("3");
            description.Should().Contain("Relationship");
            description.Should().Contain("T2→T3");
        }

        #endregion

        #region Pattern Outcome Tests

        [Fact]
        public void CalculateOutcome_ReturnsCorrectMultiplier()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );
            var pattern = TierUpPattern.Create(positions, BlockType.Work, currentTier: 1).IfNone(() => null!);

            // Act
            var outcome = pattern.CalculateOutcome();

            // Assert
            pattern.Should().NotBeNull();
            outcome.Should().NotBeNull();
            outcome.BonusMultiplier.Should().Be(1.0, "Tier 1 has multiplier of 3^0 = 1");
        }

        [Fact]
        public void CalculateOutcome_HigherTiers_HaveHigherMultipliers()
        {
            // Arrange
            var positions = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );
            var tier2Pattern = TierUpPattern.Create(positions, BlockType.Study, currentTier: 2).IfNone(() => null!);

            // Act
            var outcome = tier2Pattern.CalculateOutcome();

            // Assert
            tier2Pattern.Should().NotBeNull();
            outcome.BonusMultiplier.Should().Be(3.0, "Tier 2 has multiplier of 3^(2-1) = 3");
        }

        #endregion

        #region Pattern Conflict Tests

        [Fact]
        public void ConflictsWith_OverlappingPatterns_ReturnsTrue()
        {
            // Arrange
            var positions1 = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );
            var positions2 = Seq(
                new Vector2Int(1, 0),  // Overlaps with first pattern
                new Vector2Int(1, 1),
                new Vector2Int(1, 2)
            );
            var pattern1 = TierUpPattern.Create(positions1, BlockType.Work).IfNone(() => null!);
            var pattern2 = TierUpPattern.Create(positions2, BlockType.Work).IfNone(() => null!);

            // Act
            var conflicts = pattern1.ConflictsWith(pattern2);

            // Assert
            pattern1.Should().NotBeNull();
            pattern2.Should().NotBeNull();
            conflicts.Should().BeTrue("patterns sharing position (1,0) should conflict");
        }

        [Fact]
        public void ConflictsWith_NonOverlappingPatterns_ReturnsFalse()
        {
            // Arrange
            var positions1 = Seq(
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            );
            var positions2 = Seq(
                new Vector2Int(0, 2),
                new Vector2Int(1, 2),
                new Vector2Int(2, 2)
            );
            var pattern1 = TierUpPattern.Create(positions1, BlockType.Health).IfNone(() => null!);
            var pattern2 = TierUpPattern.Create(positions2, BlockType.Health).IfNone(() => null!);

            // Act
            var conflicts = pattern1.ConflictsWith(pattern2);

            // Assert
            pattern1.Should().NotBeNull();
            pattern2.Should().NotBeNull();
            conflicts.Should().BeFalse("non-overlapping patterns should not conflict");
        }

        #endregion
    }
}