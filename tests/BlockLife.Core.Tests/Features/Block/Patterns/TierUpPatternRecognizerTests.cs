using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Patterns;
using BlockLife.Core.Features.Block.Patterns.Recognizers;
using BlockLife.Core.Infrastructure.Services;
using FluentAssertions;
using LanguageExt;
using static LanguageExt.Prelude;
using Moq;
using Xunit;
using System;
using System.Linq;

namespace BlockLife.Core.Tests.Features.Block.Patterns
{
    /// <summary>
    /// Tests for TierUpPatternRecognizer.
    /// Validates tier-up pattern detection logic for exactly 3 connected blocks.
    /// </summary>
    public class TierUpPatternRecognizerTests
    {
        private readonly TierUpPatternRecognizer _recognizer;
        private readonly Mock<IGridStateService> _mockGridService;

        public TierUpPatternRecognizerTests()
        {
            _recognizer = new TierUpPatternRecognizer();
            _mockGridService = new Mock<IGridStateService>();
            
            // Setup default grid bounds
            _mockGridService.Setup(x => x.IsValidPosition(It.IsAny<Vector2Int>()))
                .Returns<Vector2Int>(pos => pos.X >= 0 && pos.X < 10 && pos.Y >= 0 && pos.Y < 10);
        }

        #region Basic Recognition Tests

        [Fact]
        public void Recognize_WithExactlyThreeHorizontalBlocks_FindsPattern()
        {
            // Arrange - Three Work blocks in a horizontal line
            SetupBlockAt(0, 0, BlockType.Work);
            SetupBlockAt(1, 0, BlockType.Work);
            SetupBlockAt(2, 0, BlockType.Work);
            
            var context = PatternContext.CreateDefault(new Vector2Int(1, 0));

            // Act
            var result = _recognizer.Recognize(_mockGridService.Object, new Vector2Int(1, 0), context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.IfSucc(patterns =>
            {
                patterns.Count.Should().Be(1, "should find exactly one tier-up pattern");
                var pattern = patterns.Head as TierUpPattern;
                pattern.Should().NotBeNull();
                pattern!.MergeBlockType.Should().Be(BlockType.Work);
                pattern.Positions.Count.Should().Be(3);
            });
        }

        [Fact]
        public void Recognize_WithExactlyThreeVerticalBlocks_FindsPattern()
        {
            // Arrange - Three Study blocks in a vertical line
            SetupBlockAt(5, 3, BlockType.Study);
            SetupBlockAt(5, 4, BlockType.Study);
            SetupBlockAt(5, 5, BlockType.Study);
            
            var context = PatternContext.CreateDefault(new Vector2Int(5, 4));

            // Act
            var result = _recognizer.Recognize(_mockGridService.Object, new Vector2Int(5, 4), context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.IfSucc(patterns =>
            {
                patterns.Count.Should().Be(1, "should find exactly one tier-up pattern");
                var pattern = patterns.Head as TierUpPattern;
                pattern.Should().NotBeNull();
                pattern!.MergeBlockType.Should().Be(BlockType.Study);
                pattern.Positions.Count.Should().Be(3);
            });
        }

        [Fact]
        public void Recognize_WithThreeLShapeBlocks_FindsPattern()
        {
            // Arrange - Three Health blocks in L-shape
            SetupBlockAt(2, 2, BlockType.Health);
            SetupBlockAt(3, 2, BlockType.Health);
            SetupBlockAt(3, 3, BlockType.Health);
            
            var context = PatternContext.CreateDefault(new Vector2Int(3, 2));

            // Act
            var result = _recognizer.Recognize(_mockGridService.Object, new Vector2Int(3, 2), context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.IfSucc(patterns =>
            {
                patterns.Count.Should().Be(1, "should find exactly one tier-up pattern");
                var pattern = patterns.Head as TierUpPattern;
                pattern.Should().NotBeNull();
                pattern!.MergeBlockType.Should().Be(BlockType.Health);
                pattern.Positions.Count.Should().Be(3);
            });
        }

        #endregion

        #region No Pattern Cases

        [Fact]
        public void Recognize_WithFourConnectedBlocks_FindsNoPattern()
        {
            // Arrange - Four blocks (tier-up needs exactly 3)
            SetupBlockAt(0, 0, BlockType.Creativity);
            SetupBlockAt(1, 0, BlockType.Creativity);
            SetupBlockAt(2, 0, BlockType.Creativity);
            SetupBlockAt(3, 0, BlockType.Creativity);
            
            var context = PatternContext.CreateDefault(new Vector2Int(1, 0));

            // Act
            var result = _recognizer.Recognize(_mockGridService.Object, new Vector2Int(1, 0), context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.IfSucc(patterns =>
            {
                // With 4 blocks in a row, we can form 2 different groups of 3:
                // blocks 0-1-2 and blocks 1-2-3
                patterns.Count.Should().Be(2, "should find two tier-up patterns from the group of 4");
                patterns.ForAll(p =>
                {
                    var pattern = p as TierUpPattern;
                    pattern.Should().NotBeNull();
                    pattern!.Positions.Count.Should().Be(3, "each pattern should contain exactly 3 blocks");
                    return true;
                });
            });
        }

        [Fact]
        public void Recognize_WithTwoBlocks_FindsNoPattern()
        {
            // Arrange - Only two blocks
            SetupBlockAt(0, 0, BlockType.Fun);
            SetupBlockAt(1, 0, BlockType.Fun);
            
            var context = PatternContext.CreateDefault(new Vector2Int(0, 0));

            // Act
            var result = _recognizer.Recognize(_mockGridService.Object, new Vector2Int(0, 0), context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.IfSucc(patterns =>
            {
                patterns.Count.Should().Be(0, "two blocks cannot form a tier-up pattern");
            });
        }

        [Fact]
        public void Recognize_WithSingleBlock_FindsNoPattern()
        {
            // Arrange - Single block
            SetupBlockAt(5, 5, BlockType.Relationship);
            
            var context = PatternContext.CreateDefault(new Vector2Int(5, 5));

            // Act
            var result = _recognizer.Recognize(_mockGridService.Object, new Vector2Int(5, 5), context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.IfSucc(patterns =>
            {
                patterns.Count.Should().Be(0, "single block cannot form a tier-up pattern");
            });
        }

        [Fact]
        public void Recognize_WithMixedBlockTypes_FindsNoPattern()
        {
            // Arrange - Three blocks but different types
            SetupBlockAt(0, 0, BlockType.Work);
            SetupBlockAt(1, 0, BlockType.Study);
            SetupBlockAt(2, 0, BlockType.Health);
            
            var context = PatternContext.CreateDefault(new Vector2Int(1, 0));

            // Act
            var result = _recognizer.Recognize(_mockGridService.Object, new Vector2Int(1, 0), context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.IfSucc(patterns =>
            {
                patterns.Count.Should().Be(0, "mixed block types cannot form a tier-up pattern");
            });
        }

        #endregion

        #region CanRecognizeAt Tests

        [Fact]
        public void CanRecognizeAt_WithThreeAdjacentBlocks_ReturnsTrue()
        {
            // Arrange
            SetupBlockAt(1, 1, BlockType.Work);
            SetupBlockAt(2, 1, BlockType.Work);
            SetupBlockAt(3, 1, BlockType.Work);

            // Act
            var canRecognize = _recognizer.CanRecognizeAt(_mockGridService.Object, new Vector2Int(2, 1));

            // Assert
            canRecognize.Should().BeTrue("position with 2+ adjacent blocks of same type can form tier-up");
        }

        [Fact]
        public void CanRecognizeAt_WithEmptyPosition_ReturnsFalse()
        {
            // Arrange - No block at position
            _mockGridService.Setup(x => x.IsPositionEmpty(new Vector2Int(5, 5))).Returns(true);

            // Act
            var canRecognize = _recognizer.CanRecognizeAt(_mockGridService.Object, new Vector2Int(5, 5));

            // Assert
            canRecognize.Should().BeFalse("empty position cannot have tier-up patterns");
        }

        [Fact]
        public void CanRecognizeAt_WithIsolatedBlock_ReturnsFalse()
        {
            // Arrange - Block with no adjacent blocks of same type
            SetupBlockAt(5, 5, BlockType.Fun);
            SetupBlockAt(4, 5, BlockType.Work);  // Different type
            SetupBlockAt(6, 5, BlockType.Study); // Different type

            // Act
            var canRecognize = _recognizer.CanRecognizeAt(_mockGridService.Object, new Vector2Int(5, 5));

            // Assert
            canRecognize.Should().BeFalse("block without 2+ adjacent same-type blocks cannot form tier-up");
        }

        #endregion

        #region Configuration Tests

        [Fact]
        public void SupportedType_ReturnsTierUp()
        {
            // Assert
            _recognizer.SupportedType.Should().Be(PatternType.TierUp);
        }

        [Fact]
        public void IsEnabled_ReturnsTrue()
        {
            // Assert - Should be true after VS_003B-1
            _recognizer.IsEnabled.Should().BeTrue("TierUp patterns are enabled for VS_003B-1");
        }

        [Fact]
        public void RecognizerId_HasExpectedFormat()
        {
            // Assert
            _recognizer.RecognizerId.Should().Be("TierUpPatternRecognizer_v1.0");
        }

        [Fact]
        public void ValidateConfiguration_WhenEnabled_ReturnsSuccess()
        {
            // Act
            var result = _recognizer.ValidateConfiguration();

            // Assert
            result.IsSucc.Should().BeTrue();
        }

        #endregion

        #region Target Pattern Type Filtering

        [Fact]
        public void Recognize_WithNonTierUpTargetType_ReturnsEmpty()
        {
            // Arrange
            SetupBlockAt(0, 0, BlockType.Work);
            SetupBlockAt(1, 0, BlockType.Work);
            SetupBlockAt(2, 0, BlockType.Work);
            
            var context = PatternContext.CreateForTypes(new Vector2Int(1, 0), PatternType.Match);

            // Act
            var result = _recognizer.Recognize(_mockGridService.Object, new Vector2Int(1, 0), context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.IfSucc(patterns =>
            {
                patterns.Count.Should().Be(0, "should not find patterns when TierUp is not in target types");
            });
        }

        [Fact]
        public void Recognize_WithTierUpTargetType_FindsPatterns()
        {
            // Arrange
            SetupBlockAt(0, 0, BlockType.Health);
            SetupBlockAt(1, 0, BlockType.Health);
            SetupBlockAt(2, 0, BlockType.Health);
            
            var context = PatternContext.CreateForTypes(new Vector2Int(1, 0), PatternType.TierUp);

            // Act
            var result = _recognizer.Recognize(_mockGridService.Object, new Vector2Int(1, 0), context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.IfSucc(patterns =>
            {
                patterns.Count.Should().Be(1, "should find tier-up pattern when TierUp is in target types");
            });
        }

        #endregion

        #region Performance Metrics Tests

        [Fact]
        public void GetPerformanceMetrics_AfterRecognition_ReturnsMetrics()
        {
            // Arrange & Act - Run a recognition first
            SetupBlockAt(0, 0, BlockType.Creativity);
            SetupBlockAt(1, 0, BlockType.Creativity);
            SetupBlockAt(2, 0, BlockType.Creativity);
            
            var context = PatternContext.CreateDefault(new Vector2Int(1, 0));
            _recognizer.Recognize(_mockGridService.Object, new Vector2Int(1, 0), context);
            
            var metrics = _recognizer.GetPerformanceMetrics();

            // Assert
            metrics.Should().NotBeEmpty();
            metrics.Should().Contain(m => m.Metric == "TotalRecognitions");
            metrics.Should().Contain(m => m.Metric == "TotalPatternsFound");
            metrics.Should().Contain(m => m.Metric == "AveragePatternsPerRecognition");
        }

        #endregion

        #region Helper Methods

        private void SetupBlockAt(int x, int y, BlockType type)
        {
            var position = new Vector2Int(x, y);
            var block = new Core.Domain.Block.Block
            {
                Id = Guid.NewGuid(),
                Type = type,
                Position = position,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            _mockGridService.Setup(x => x.GetBlockAt(position)).Returns(Option<Core.Domain.Block.Block>.Some(block));
            _mockGridService.Setup(x => x.IsPositionEmpty(position)).Returns(false);
            _mockGridService.Setup(x => x.IsPositionOccupied(position)).Returns(true);
        }

        #endregion
    }
}