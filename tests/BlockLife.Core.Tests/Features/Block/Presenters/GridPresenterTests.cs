using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Move;
using BlockLife.Core.Features.Block.Presenters;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Core.Tests.Builders;
using FluentAssertions;
using LanguageExt;
using MediatR;
using Moq;
using Serilog;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BlockLife.Core.Tests.Features.Block.Presenters
{
    /// <summary>
    /// Unit tests for GridPresenter, focusing on block moved handling.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public class GridPresenterTests
    {
        private readonly Mock<IGridView> _mockView;
        private readonly Mock<IBlockAnimationView> _mockAnimator;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IGridStateService> _mockGridStateService;
        private readonly Mock<ILogger> _mockLogger;
        private readonly GridPresenter _presenter;

        public GridPresenterTests()
        {
            _mockView = new Mock<IGridView>();
            _mockAnimator = new Mock<IBlockAnimationView>();
            _mockMediator = new Mock<IMediator>();
            _mockGridStateService = new Mock<IGridStateService>();
            _mockLogger = new Mock<ILogger>();

            // Setup the view to return the animator
            _mockView.Setup(v => v.BlockAnimator).Returns(_mockAnimator.Object);

            _presenter = new GridPresenter(
                _mockView.Object,
                _mockMediator.Object,
                _mockGridStateService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task HandleBlockMovedAsync_WithAnimator_ShouldAnimateMove()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var fromPosition = new Vector2Int(0, 0);
            var toPosition = new Vector2Int(2, 3);

            _mockAnimator
                .Setup(a => a.AnimateMoveAsync(blockId, fromPosition, toPosition))
                .Returns(Task.CompletedTask);

            _mockView
                .Setup(v => v.ShowSuccessFeedbackAsync(toPosition, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _presenter.HandleBlockMovedAsync(blockId, fromPosition, toPosition);

            // Assert
            _mockAnimator.Verify(
                a => a.AnimateMoveAsync(blockId, fromPosition, toPosition),
                Times.Once,
                "Should animate the block movement");

            _mockView.Verify(
                v => v.ShowSuccessFeedbackAsync(toPosition, It.IsAny<string>()),
                Times.Once,
                "Should show success feedback at the new position");

            _mockLogger.Verify(
                l => l.Information(
                    It.IsAny<string>(),
                    blockId,
                    fromPosition,
                    toPosition),
                Times.Once,
                "Should log the notification handling");
        }

        [Fact]
        public async Task HandleBlockMovedAsync_WithoutAnimator_ShouldUpdateInstantly()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var fromPosition = new Vector2Int(0, 0);
            var toPosition = new Vector2Int(2, 3);
            var blockType = BlockType.Work;

            var block = new BlockBuilder()
                .WithId(blockId)
                .WithPosition(toPosition)
                .WithType(blockType)
                .Build();

            // Setup view without animator
            _mockView.Setup(v => v.BlockAnimator).Returns((IBlockAnimationView)null!);

            _mockGridStateService
                .Setup(g => g.GetBlockById(blockId))
                .Returns(Option<Core.Domain.Block.Block>.Some(block));

            _mockView
                .Setup(v => v.UpdateBlockAsync(blockId, blockType, toPosition))
                .Returns(Task.CompletedTask);

            _mockView
                .Setup(v => v.ShowSuccessFeedbackAsync(toPosition, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _presenter.HandleBlockMovedAsync(blockId, fromPosition, toPosition);

            // Assert
            _mockView.Verify(
                v => v.UpdateBlockAsync(blockId, blockType, toPosition),
                Times.Once,
                "Should update block position instantly when animator is not available");

            _mockView.Verify(
                v => v.ShowSuccessFeedbackAsync(toPosition, It.IsAny<string>()),
                Times.Once,
                "Should show success feedback at the new position");

            _mockLogger.Verify(
                l => l.Warning(It.IsAny<string>()),
                Times.Once,
                "Should log warning about missing animator");
        }

        [Fact]
        public async Task HandleBlockMovedAsync_WithException_ShouldLogError()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var fromPosition = new Vector2Int(0, 0);
            var toPosition = new Vector2Int(2, 3);
            var exception = new InvalidOperationException("Animation failed");

            _mockAnimator
                .Setup(a => a.AnimateMoveAsync(blockId, fromPosition, toPosition))
                .ThrowsAsync(exception);

            // Act
            await _presenter.HandleBlockMovedAsync(blockId, fromPosition, toPosition);

            // Assert
            _mockLogger.Verify(
                l => l.Error(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    blockId),
                Times.Once,
                "Should log error when exception occurs");
        }

        [Fact]
        public async Task HandleBlockMovedAsync_BlockNotFound_ShouldNotUpdateView()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var fromPosition = new Vector2Int(0, 0);
            var toPosition = new Vector2Int(2, 3);

            // Setup view without animator and block not found
            _mockView.Setup(v => v.BlockAnimator).Returns((IBlockAnimationView)null!);

            _mockGridStateService
                .Setup(g => g.GetBlockById(blockId))
                .Returns(Option<Core.Domain.Block.Block>.None);

            // Act
            await _presenter.HandleBlockMovedAsync(blockId, fromPosition, toPosition);

            // Assert
            _mockView.Verify(
                v => v.UpdateBlockAsync(It.IsAny<Guid>(), It.IsAny<BlockType>(), It.IsAny<Vector2Int>()),
                Times.Never,
                "Should not update view when block is not found");

            _mockView.Verify(
                v => v.ShowSuccessFeedbackAsync(toPosition, It.IsAny<string>()),
                Times.Once,
                "Should still show success feedback");
        }

        [Fact]
        public async Task HandleBlockMovedAsync_MultipleBlocks_ShouldHandleIndependently()
        {
            // Arrange
            var blockId1 = Guid.NewGuid();
            var blockId2 = Guid.NewGuid();
            var fromPosition1 = new Vector2Int(0, 0);
            var toPosition1 = new Vector2Int(2, 3);
            var fromPosition2 = new Vector2Int(1, 1);
            var toPosition2 = new Vector2Int(3, 4);

            _mockAnimator
                .Setup(a => a.AnimateMoveAsync(It.IsAny<Guid>(), It.IsAny<Vector2Int>(), It.IsAny<Vector2Int>()))
                .Returns(Task.CompletedTask);

            _mockView
                .Setup(v => v.ShowSuccessFeedbackAsync(It.IsAny<Vector2Int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await Task.WhenAll(
                _presenter.HandleBlockMovedAsync(blockId1, fromPosition1, toPosition1),
                _presenter.HandleBlockMovedAsync(blockId2, fromPosition2, toPosition2)
            );

            // Assert
            _mockAnimator.Verify(
                a => a.AnimateMoveAsync(blockId1, fromPosition1, toPosition1),
                Times.Once,
                "Should animate first block movement");

            _mockAnimator.Verify(
                a => a.AnimateMoveAsync(blockId2, fromPosition2, toPosition2),
                Times.Once,
                "Should animate second block movement");

            _mockView.Verify(
                v => v.ShowSuccessFeedbackAsync(It.IsAny<Vector2Int>(), It.IsAny<string>()),
                Times.Exactly(2),
                "Should show success feedback for both blocks");
        }
    }
}
