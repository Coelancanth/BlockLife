using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Patterns;
using BlockLife.Core.Features.Block.Patterns.Executors;
using BlockLife.Core.Features.Block.Patterns.Recognizers;
using BlockLife.Core.Infrastructure.Services;
using FluentAssertions;
using LanguageExt;
using static LanguageExt.Prelude;
using MediatR;
using Moq;
using Xunit;
using ExecutionContext = BlockLife.Core.Features.Block.Patterns.ExecutionContext;
using DomainBlock = BlockLife.Core.Domain.Block.Block;

namespace BlockLife.Core.Tests.Features.Block.Patterns
{
    /// <summary>
    /// Basic functionality tests for MergePatternExecutor.
    /// Simplified test suite to verify core merge behavior without complex setup.
    /// </summary>
    public class MergePatternExecutorBasicTests
    {
        [Fact]
        public void MergePatternExecutor_HasCorrectSupportedType()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var executor = new MergePatternExecutor(mediator.Object);

            // Act & Assert
            executor.SupportedType.Should().Be(PatternType.Match);
            executor.IsEnabled.Should().BeTrue();
            executor.ExecutorId.Should().Be("MergePatternExecutor_v1.0");
        }

        [Fact]
        public void MergePatternExecutor_ValidateConfiguration_WithValidMediator_ReturnsSuccess()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var executor = new MergePatternExecutor(mediator.Object);

            // Act
            var result = executor.ValidateConfiguration();

            // Assert
            result.Match(
                Succ: unit => true.Should().BeTrue(), // Success case
                Fail: error => true.Should().BeFalse($"Validation should succeed, but got error: {error.Message}")
            );
        }

        [Fact]
        public void MergePatternExecutor_EstimateExecutionTime_ScalesWithBlockCount()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var executor = new MergePatternExecutor(mediator.Object);

            var threeBlockPositions = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var fiveBlockPositions = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), 
                                        new Vector2Int(0, 1), new Vector2Int(0, 2));
            
            var threeBlockPattern = MatchPattern.Create(threeBlockPositions, BlockType.Work)
                .IfNone(() => throw new InvalidOperationException("Failed to create three block pattern"));
            var fiveBlockPattern = MatchPattern.Create(fiveBlockPositions, BlockType.Work)
                .IfNone(() => throw new InvalidOperationException("Failed to create five block pattern"));

            // Act
            var threeBlockTime = executor.EstimateExecutionTime(threeBlockPattern);
            var fiveBlockTime = executor.EstimateExecutionTime(fiveBlockPattern);

            // Assert - Should scale at 10ms per block
            threeBlockTime.Should().Be(30.0);
            fiveBlockTime.Should().Be(50.0);
            fiveBlockTime.Should().BeGreaterThan(threeBlockTime);
        }

        [Fact]
        public async Task MergePatternExecutor_Execute_WithNonMatchPattern_ReturnsError()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var gridService = new Mock<IGridStateService>();
            var executor = new MergePatternExecutor(mediator.Object);

            var mockPattern = new Mock<IPattern>();
            mockPattern.Setup(p => p.Type).Returns(PatternType.Transmute);

            var context = new ExecutionContext
            {
                GridService = gridService.Object,
                TriggerPosition = new Vector2Int(0, 0)
            };

            // Act
            var result = await executor.Execute(mockPattern.Object, context);

            // Assert - Should fail with pattern type error
            result.Match(
                Succ: executionResult => true.Should().BeFalse("Should not succeed with non-MatchPattern"),
                Fail: error => error.Message.Should().Contain("Pattern is not a MatchPattern")
            );
        }

        [Fact]
        public async Task MergePatternExecutor_Execute_WithNoTriggerPosition_ReturnsError()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var gridService = new Mock<IGridStateService>();
            var executor = new MergePatternExecutor(mediator.Object);

            var positions = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var pattern = MatchPattern.Create(positions, BlockType.Work)
                .IfNone(() => throw new InvalidOperationException("Failed to create pattern"));

            var context = new ExecutionContext
            {
                GridService = gridService.Object,
                TriggerPosition = null // No trigger position
            };

            // Act
            var result = await executor.Execute(pattern, context);

            // Assert - Should fail with trigger position error
            result.Match(
                Succ: executionResult => true.Should().BeFalse("Should not succeed without trigger position"),
                Fail: error => error.Message.Should().Contain("Merge patterns require a trigger position")
            );
        }

        [Fact]
        public async Task MergePatternExecutor_CanExecute_WithValidMatchPattern_ReturnsTrue()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var gridService = new Mock<IGridStateService>();
            var executor = new MergePatternExecutor(mediator.Object);

            var positions = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var pattern = MatchPattern.Create(positions, BlockType.Work)
                .IfNone(() => throw new InvalidOperationException("Failed to create pattern"));

            var context = new ExecutionContext
            {
                GridService = gridService.Object,
                TriggerPosition = new Vector2Int(1, 0)
            };

            // Mock pattern validation
            var mockBlock = DomainBlock.CreateNew(BlockType.Work, new Vector2Int(0, 0));
            gridService.Setup(g => g.GetBlockAt(It.IsAny<Vector2Int>()))
                      .Returns(Option<DomainBlock>.Some(mockBlock));

            // Act
            var result = await executor.CanExecute(pattern, context);

            // Assert
            result.Match(
                Succ: canExecute => canExecute.Should().BeTrue(),
                Fail: error => true.Should().BeFalse($"CanExecute should succeed, but got error: {error.Message}")
            );
        }

        [Fact]
        public async Task MergePatternExecutor_CanExecute_WithNonMatchPattern_ReturnsFalse()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var gridService = new Mock<IGridStateService>();
            var executor = new MergePatternExecutor(mediator.Object);

            var mockPattern = new Mock<IPattern>();
            mockPattern.Setup(p => p.Type).Returns(PatternType.Transmute);

            var context = new ExecutionContext
            {
                GridService = gridService.Object,
                TriggerPosition = new Vector2Int(0, 0)
            };

            // Act
            var result = await executor.CanExecute(mockPattern.Object, context);

            // Assert
            result.Match(
                Succ: canExecute => canExecute.Should().BeFalse(),
                Fail: error => true.Should().BeFalse($"CanExecute should succeed but return false, got error: {error.Message}")
            );
        }

        // BR_016 Defensive Programming Tests

        [Fact]
        public async Task MergePatternExecutor_Execute_WithNullContext_ReturnsError()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var executor = new MergePatternExecutor(mediator.Object);

            var positions = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var pattern = MatchPattern.Create(positions, BlockType.Work)
                .IfNone(() => throw new InvalidOperationException("Failed to create pattern"));

            // Act
            var result = await executor.Execute(pattern, null!);

            // Assert
            result.Match(
                Succ: _ => true.Should().BeFalse("Should not succeed with null context"),
                Fail: error => error.Message.Should().Contain("ExecutionContext cannot be null")
            );
        }

        [Fact]
        public async Task MergePatternExecutor_Execute_WithNullGridService_ReturnsError()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var executor = new MergePatternExecutor(mediator.Object);

            var positions = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var pattern = MatchPattern.Create(positions, BlockType.Work)
                .IfNone(() => throw new InvalidOperationException("Failed to create pattern"));

            var context = new ExecutionContext
            {
                GridService = null!,
                TriggerPosition = new Vector2Int(1, 0)
            };

            // Act
            var result = await executor.Execute(pattern, context);

            // Assert
            result.Match(
                Succ: _ => true.Should().BeFalse("Should not succeed with null GridService"),
                Fail: error => error.Message.Should().Contain("GridService is required")
            );
        }

        [Fact]
        public async Task MergePatternExecutor_Execute_WithEmptyPatternPositions_ReturnsError()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var gridService = new Mock<IGridStateService>();
            var executor = new MergePatternExecutor(mediator.Object);

            // Create pattern with empty positions
            var emptyPositions = Seq<Vector2Int>();
            var patternOption = MatchPattern.Create(emptyPositions, BlockType.Work);

            // MatchPattern.Create should fail with empty positions, but if it doesn't:
            if (patternOption.IsSome)
            {
                var context = new ExecutionContext
                {
                    GridService = gridService.Object,
                    TriggerPosition = new Vector2Int(1, 0)
                };

                // Act
                var pattern = patternOption.IfNone(() => throw new InvalidOperationException("Expected pattern"));
                var result = await executor.Execute(pattern, context);

                // Assert
                result.Match(
                    Succ: _ => true.Should().BeFalse("Should not succeed with empty pattern positions"),
                    Fail: error => error.Message.Should().Contain("Pattern has no positions")
                );
            }
        }

        [Fact]
        public async Task MergePatternExecutor_Execute_WithMaxTierBlocks_ReturnsError()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var gridService = new Mock<IGridStateService>();
            var executor = new MergePatternExecutor(mediator.Object);

            var positions = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var pattern = MatchPattern.Create(positions, BlockType.Work)
                .IfNone(() => throw new InvalidOperationException("Failed to create pattern"));

            var context = new ExecutionContext
            {
                GridService = gridService.Object,
                TriggerPosition = new Vector2Int(1, 0)
            };

            // Mock blocks at MAX_TIER (T4)
            var maxTierBlock = DomainBlock.CreateNew(BlockType.Work, new Vector2Int(0, 0), 4);
            gridService.Setup(g => g.GetBlockAt(It.IsAny<Vector2Int>()))
                      .Returns(Option<DomainBlock>.Some(maxTierBlock));

            // Act
            var result = await executor.Execute(pattern, context);

            // Assert
            result.Match(
                Succ: _ => true.Should().BeFalse("Should not succeed when merging T4 blocks"),
                Fail: error => error.Message.Should().Contain("Cannot merge blocks beyond T4")
            );
        }

        [Fact]
        public async Task MergePatternExecutor_CanExecute_WithNullPattern_ReturnsError()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var gridService = new Mock<IGridStateService>();
            var executor = new MergePatternExecutor(mediator.Object);

            var context = new ExecutionContext
            {
                GridService = gridService.Object,
                TriggerPosition = new Vector2Int(0, 0)
            };

            // Act
            var result = await executor.CanExecute(null!, context);

            // Assert
            result.Match(
                Succ: _ => true.Should().BeFalse("Should not succeed with null pattern"),
                Fail: error => error.Message.Should().Contain("Pattern cannot be null")
            );
        }

        [Fact]
        public async Task MergePatternExecutor_CanExecute_WithNullContext_ReturnsError()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var executor = new MergePatternExecutor(mediator.Object);

            var positions = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var pattern = MatchPattern.Create(positions, BlockType.Work)
                .IfNone(() => throw new InvalidOperationException("Failed to create pattern"));

            // Act
            var result = await executor.CanExecute(pattern, null!);

            // Assert
            result.Match(
                Succ: _ => true.Should().BeFalse("Should not succeed with null context"),
                Fail: error => error.Message.Should().Contain("Valid context with GridService is required")
            );
        }

        [Fact]
        public void MergePatternExecutor_EstimateExecutionTime_WithNullPattern_ReturnsZero()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var executor = new MergePatternExecutor(mediator.Object);

            // Act
            var time = executor.EstimateExecutionTime(null!);

            // Assert
            time.Should().Be(0.0);
        }
    }
}