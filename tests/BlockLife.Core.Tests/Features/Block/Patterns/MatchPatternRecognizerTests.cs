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
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BlockLife.Core.Tests.Features.Block.Patterns
{
    /// <summary>
    /// Tests for MatchPatternRecognizer flood-fill algorithm and pattern detection.
    /// Validates horizontal, vertical, L-shaped, T-shaped, and complex patterns.
    /// Tests edge cases, boundaries, and performance characteristics.
    /// </summary>
    public class MatchPatternRecognizerTests
    {
        private readonly MatchPatternRecognizer _recognizer;

        public MatchPatternRecognizerTests()
        {
            _recognizer = new MatchPatternRecognizer();
        }

        #region Basic Recognition Tests

        [Fact]
        public void Recognize_WithHorizontalMatch_FindsPattern()
        {
            // Arrange
            var gridService = CreateGridService();
            PlaceBlocksHorizontal(gridService, BlockType.Work, new Vector2Int(1, 1), 3);

            var triggerPosition = new Vector2Int(1, 1);
            var context = PatternContext.CreateDefault(triggerPosition);

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns =>
                {
                    patterns.Should().HaveCount(1);
                    var matchPattern = patterns.Head as MatchPattern;
                    matchPattern.Should().NotBeNull();
                    matchPattern!.MatchedBlockType.Should().Be(BlockType.Work);
                    matchPattern.Positions.Should().HaveCount(3);
                    matchPattern.Positions.Should().Contain(new Vector2Int(1, 1));
                    matchPattern.Positions.Should().Contain(new Vector2Int(2, 1));
                    matchPattern.Positions.Should().Contain(new Vector2Int(3, 1));
                },
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        [Fact]
        public void Recognize_WithVerticalMatch_FindsPattern()
        {
            // Arrange
            var gridService = CreateGridService();
            PlaceBlocksVertical(gridService, BlockType.Health, new Vector2Int(2, 1), 4);

            var triggerPosition = new Vector2Int(2, 2);
            var context = PatternContext.CreateDefault(triggerPosition);

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns =>
                {
                    patterns.Should().HaveCount(1);
                    var matchPattern = patterns.Head as MatchPattern;
                    matchPattern.Should().NotBeNull();
                    matchPattern!.MatchedBlockType.Should().Be(BlockType.Health);
                    matchPattern.Positions.Should().HaveCount(4);
                },
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        [Fact]
        public void Recognize_WithLShapeMatch_FindsPattern()
        {
            // Arrange
            var gridService = CreateGridService();
            var positions = new[]
            {
                new Vector2Int(1, 1),
                new Vector2Int(2, 1),
                new Vector2Int(3, 1),
                new Vector2Int(3, 2)
            };
            PlaceBlocksAtPositions(gridService, BlockType.Study, positions);

            var triggerPosition = new Vector2Int(1, 1);
            var context = PatternContext.CreateDefault(triggerPosition);

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns =>
                {
                    patterns.Should().HaveCount(1);
                    var matchPattern = patterns.Head as MatchPattern;
                    matchPattern.Should().NotBeNull();
                    matchPattern!.MatchedBlockType.Should().Be(BlockType.Study);
                    matchPattern.Positions.Should().HaveCount(4);
                },
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        [Fact]
        public void Recognize_WithCrossShapeMatch_FindsPattern()
        {
            // Arrange - Cross/Plus shape pattern
            var gridService = CreateGridService();
            var positions = new[]
            {
                new Vector2Int(1, 2), // Top
                new Vector2Int(2, 1), // Left  
                new Vector2Int(2, 2), // Center
                new Vector2Int(2, 3), // Right
                new Vector2Int(3, 2)  // Bottom
            };
            PlaceBlocksAtPositions(gridService, BlockType.Creativity, positions);

            var triggerPosition = new Vector2Int(2, 2);
            var context = PatternContext.CreateDefault(triggerPosition);

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns =>
                {
                    patterns.Should().HaveCount(1);
                    var matchPattern = patterns.Head as MatchPattern;
                    matchPattern.Should().NotBeNull();
                    matchPattern!.MatchedBlockType.Should().Be(BlockType.Creativity);
                    matchPattern.Positions.Should().HaveCount(5);
                    // Verify it found the complete cross/plus pattern
                    matchPattern.Positions.Should().Contain(new Vector2Int(1, 2)); // Top
                    matchPattern.Positions.Should().Contain(new Vector2Int(2, 1)); // Left
                    matchPattern.Positions.Should().Contain(new Vector2Int(2, 2)); // Center
                    matchPattern.Positions.Should().Contain(new Vector2Int(2, 3)); // Right
                    matchPattern.Positions.Should().Contain(new Vector2Int(3, 2)); // Bottom
                },
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void Recognize_WithTwoBlocks_FindsNoPattern()
        {
            // Arrange
            var gridService = CreateGridService();
            PlaceBlocksHorizontal(gridService, BlockType.Fun, new Vector2Int(1, 1), 2);

            var triggerPosition = new Vector2Int(1, 1);
            var context = PatternContext.CreateDefault(triggerPosition);

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns =>
                {
                    patterns.Should().BeEmpty();
                },
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        [Fact]
        public void Recognize_WithSingleBlock_FindsNoPattern()
        {
            // Arrange
            var gridService = CreateGridService();
            PlaceBlockAt(gridService, BlockType.Basic, new Vector2Int(1, 1));

            var triggerPosition = new Vector2Int(1, 1);
            var context = PatternContext.CreateDefault(triggerPosition);

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns =>
                {
                    patterns.Should().BeEmpty();
                },
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        [Fact]
        public void Recognize_WithDisconnectedBlocks_FindsOnlyConnectedPattern()
        {
            // Arrange
            var gridService = CreateGridService();
            // Create two separate groups of 3 blocks with a gap
            PlaceBlocksHorizontal(gridService, BlockType.Work, new Vector2Int(1, 1), 3);
            PlaceBlocksHorizontal(gridService, BlockType.Work, new Vector2Int(5, 1), 3); // Gap at x=4

            var triggerPosition = new Vector2Int(1, 1);
            var context = PatternContext.CreateDefault(triggerPosition);

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns =>
                {
                    patterns.Should().HaveCount(1); // Only finds the connected group near trigger
                    var matchPattern = patterns.Head as MatchPattern;
                    matchPattern!.Positions.Should().HaveCount(3);
                    matchPattern.Positions.Should().OnlyContain(pos => pos.X <= 3); // All positions in first group
                },
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        [Fact]
        public void Recognize_WithMixedBlockTypes_FindsOnlyMatchingTypes()
        {
            // Arrange
            var gridService = CreateGridService();
            // Create mixed pattern: Work-Work-Study-Work
            PlaceBlockAt(gridService, BlockType.Work, new Vector2Int(1, 1));
            PlaceBlockAt(gridService, BlockType.Work, new Vector2Int(2, 1));
            PlaceBlockAt(gridService, BlockType.Study, new Vector2Int(3, 1));
            PlaceBlockAt(gridService, BlockType.Work, new Vector2Int(4, 1));

            var triggerPosition = new Vector2Int(1, 1);
            var context = PatternContext.CreateDefault(triggerPosition);

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns =>
                {
                    patterns.Should().BeEmpty(); // No group has 3+ connected blocks of same type
                },
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        [Fact]
        public void Recognize_AtGridBoundary_HandlesEdgeCorrectly()
        {
            // Arrange
            var gridService = CreateGridService();
            // Place pattern at grid boundary
            PlaceBlocksHorizontal(gridService, BlockType.Relationship, new Vector2Int(0, 0), 3);

            var triggerPosition = new Vector2Int(0, 0);
            var context = PatternContext.CreateDefault(triggerPosition);

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns =>
                {
                    patterns.Should().HaveCount(1);
                    var matchPattern = patterns.Head as MatchPattern;
                    matchPattern!.Positions.Should().HaveCount(3);
                },
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        #endregion

        #region Multiple Pattern Tests

        [Fact]
        public void Recognize_WithMultipleDisconnectedMatches_FindsAllPatterns()
        {
            // Arrange
            var gridService = CreateGridService();
            // Create two separate groups of different types
            PlaceBlocksHorizontal(gridService, BlockType.Work, new Vector2Int(1, 1), 3);
            PlaceBlocksVertical(gridService, BlockType.Health, new Vector2Int(1, 3), 3);

            var triggerPosition = new Vector2Int(1, 1);
            var context = PatternContext.CreateDefault(triggerPosition);

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns =>
                {
                    patterns.Should().HaveCount(2);
                    patterns.Should().Contain(p => ((MatchPattern)p).MatchedBlockType == BlockType.Work);
                    patterns.Should().Contain(p => ((MatchPattern)p).MatchedBlockType == BlockType.Health);
                },
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        [Fact]
        public void Recognize_WithMaxPatternsLimit_RespectsLimit()
        {
            // Arrange
            var gridService = CreateGridService();
            // Create multiple patterns
            PlaceBlocksHorizontal(gridService, BlockType.Work, new Vector2Int(1, 1), 3);
            PlaceBlocksVertical(gridService, BlockType.Health, new Vector2Int(1, 3), 3);
            PlaceBlocksHorizontal(gridService, BlockType.Study, new Vector2Int(5, 1), 3);

            var triggerPosition = new Vector2Int(1, 1);
            var context = PatternContext.CreateDefault(triggerPosition).WithMaxPatterns(2);

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns =>
                {
                    patterns.Should().HaveCount(2); // Limited by maxPatterns
                },
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        #endregion

        #region Configuration and Performance Tests

        [Fact]
        public void CanRecognizeAt_WithEmptyPosition_ReturnsFalse()
        {
            // Arrange
            var gridService = CreateGridService();
            var position = new Vector2Int(1, 1);

            // Act
            var canRecognize = _recognizer.CanRecognizeAt(gridService, position);

            // Assert
            canRecognize.Should().BeFalse();
        }

        [Fact]
        public void CanRecognizeAt_WithBlockButNoAdjacent_ReturnsFalse()
        {
            // Arrange
            var gridService = CreateGridService();
            PlaceBlockAt(gridService, BlockType.Work, new Vector2Int(1, 1));
            var position = new Vector2Int(1, 1);

            // Act
            var canRecognize = _recognizer.CanRecognizeAt(gridService, position);

            // Assert
            canRecognize.Should().BeFalse();
        }

        [Fact]
        public void CanRecognizeAt_WithBlockAndTwoAdjacent_ReturnsTrue()
        {
            // Arrange
            var gridService = CreateGridService();
            PlaceBlocksHorizontal(gridService, BlockType.Work, new Vector2Int(1, 1), 3);
            var position = new Vector2Int(1, 1);

            // Act
            var canRecognize = _recognizer.CanRecognizeAt(gridService, position);

            // Assert
            canRecognize.Should().BeTrue();
        }

        [Fact]
        public void ValidateConfiguration_WithValidSetup_ReturnsSuccess()
        {
            // Act
            var result = _recognizer.ValidateConfiguration();

            // Assert
            result.IsSucc.Should().BeTrue();
        }

        [Fact]
        public void GetPerformanceMetrics_AfterRecognition_ReturnsMetrics()
        {
            // Arrange
            var gridService = CreateGridService();
            PlaceBlocksHorizontal(gridService, BlockType.Work, new Vector2Int(1, 1), 3);
            var context = PatternContext.CreateDefault(new Vector2Int(1, 1));

            // Act
            _recognizer.Recognize(gridService, new Vector2Int(1, 1), context);
            var metrics = _recognizer.GetPerformanceMetrics();

            // Assert
            metrics.Should().NotBeEmpty();
            metrics.Should().Contain(m => m.Metric == "TotalRecognitions");
            metrics.Should().Contain(m => m.Metric == "TotalMatchesFound");
        }

        #endregion

        #region Context Filtering Tests

        [Fact]
        public void Recognize_WithNonMatchTargetTypes_ReturnsEmpty()
        {
            // Arrange
            var gridService = CreateGridService();
            PlaceBlocksHorizontal(gridService, BlockType.Work, new Vector2Int(1, 1), 3);

            var triggerPosition = new Vector2Int(1, 1);
            var context = PatternContext.CreateForTypes(triggerPosition, PatternType.TierUp); // Wrong type

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns => patterns.Should().BeEmpty(),
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        [Fact]
        public void Recognize_WithMatchTargetType_FindsPatterns()
        {
            // Arrange
            var gridService = CreateGridService();
            PlaceBlocksHorizontal(gridService, BlockType.Work, new Vector2Int(1, 1), 3);

            var triggerPosition = new Vector2Int(1, 1);
            var context = PatternContext.CreateForTypes(triggerPosition, PatternType.Match);

            // Act
            var result = _recognizer.Recognize(gridService, triggerPosition, context);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.Match(
                Succ: patterns => patterns.Should().HaveCount(1),
                Fail: error => throw new Exception($"Expected success but got: {error}")
            );
        }

        #endregion

        #region Helper Methods

        private static IGridStateService CreateGridService()
        {
            return new GridStateService(10, 10);
        }

        private static Mock<IGridStateService> CreateGridServiceMock()
        {
            var gridService = new Mock<IGridStateService>();
            
            // Configure grid dimensions
            gridService.Setup(gs => gs.GetGridDimensions()).Returns(new Vector2Int(10, 10));
            
            // Configure position validation - assume all positions 0-9 are valid
            gridService.Setup(gs => gs.IsValidPosition(It.IsAny<Vector2Int>()))
                .Returns<Vector2Int>(pos => pos.X >= 0 && pos.X < 10 && pos.Y >= 0 && pos.Y < 10);

            return gridService;
        }

        private static void PlaceBlockAt(IGridStateService gridService, BlockType blockType, Vector2Int position)
        {
            var block = new BlockLife.Core.Domain.Block.Block 
            { 
                Id = Guid.NewGuid(), 
                Type = blockType, 
                Position = position, 
                CreatedAt = DateTime.Now, 
                LastModifiedAt = DateTime.Now 
            };
            
            // For concrete GridStateService, use PlaceBlock method
            if (gridService is GridStateService concreteService)
            {
                concreteService.PlaceBlock(block);
            }
            else
            {
                // For mocked services, configure the mock
                var gridServiceMock = (Mock<IGridStateService>)gridService;
                gridServiceMock.Setup(gs => gs.GetBlockAt(position)).Returns(Some(block));
                gridServiceMock.Setup(gs => gs.IsPositionOccupied(position)).Returns(true);
                gridServiceMock.Setup(gs => gs.IsPositionEmpty(position)).Returns(false);
            }
        }

        private static void PlaceBlocksHorizontal(IGridStateService gridService, BlockType blockType, Vector2Int startPosition, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var position = new Vector2Int(startPosition.X + i, startPosition.Y);
                PlaceBlockAt(gridService, blockType, position);
            }
        }

        private static void PlaceBlocksVertical(IGridStateService gridService, BlockType blockType, Vector2Int startPosition, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var position = new Vector2Int(startPosition.X, startPosition.Y + i);
                PlaceBlockAt(gridService, blockType, position);
            }
        }

        private static void PlaceBlocksAtPositions(IGridStateService gridService, BlockType blockType, Vector2Int[] positions)
        {
            foreach (var position in positions)
            {
                PlaceBlockAt(gridService, blockType, position);
            }
        }

        #endregion
    }
}