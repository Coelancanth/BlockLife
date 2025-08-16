using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Commands;
using BlockLife.Core.Features.Block.Effects;
using BlockLife.Core.Features.Block.Rules;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Core.Tests.Builders;
using BlockLife.Core.Tests.Utils;
using FluentAssertions;
using LanguageExt;
using MediatR;
using Moq;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BlockLife.Core.Tests.Features.Block.Move
{
    /// <summary>
    /// TDD Tests for MoveBlockCommandHandler - Following Comprehensive Development Workflow
    /// These tests are written FIRST to drive the implementation (RED phase)
    /// </summary>
    public class MoveBlockCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger> _mockLogger;
        private readonly GridStateService _gridStateService;
        private readonly MoveBlockCommandHandler _handler;

        public MoveBlockCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger>();
            _gridStateService = new GridStateService(5, 5);

            // This will fail until we implement MoveBlockCommandHandler
            _handler = new MoveBlockCommandHandler(
                _gridStateService,
                _mockMediator.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_WhenMoveIsValid_ReturnsSuccessAndPublishesNotification()
        {
            // Arrange - Place a block first
            var block = new BlockBuilder()
                .WithId(TestGuids.BlockA)
                .WithType(BlockType.Work)
                .WithPosition(1, 1)
                .Build();
            _gridStateService.PlaceBlock(block);

            // This will fail until we implement MoveBlockCommand
            var command = MoveBlockCommand.Create(
                TestGuids.BlockA,
                new Vector2Int(3, 3)
            );

            BlockMovedNotification? capturedNotification = null;
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<BlockMovedNotification>(), It.IsAny<CancellationToken>()))
                .Callback<INotification, CancellationToken>((notification, _) =>
                {
                    capturedNotification = notification as BlockMovedNotification;
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert - Result
            result.IsSucc.Should().BeTrue();

            // Assert - Block moved in grid state
            var movedBlock = _gridStateService.GetBlockAt(new Vector2Int(3, 3));
            movedBlock.IsSome.Should().BeTrue();
            movedBlock.Match(Some: b => b.Id.Should().Be(TestGuids.BlockA), None: () => { });

            // Assert - Original position is empty
            var originalPosition = _gridStateService.GetBlockAt(new Vector2Int(1, 1));
            originalPosition.IsNone.Should().BeTrue();

            // Assert - Notification published
            _mockMediator.Verify(m => m.Publish(
                It.IsAny<BlockMovedNotification>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);

            capturedNotification.Should().NotBeNull();
            capturedNotification!.BlockId.Should().Be(TestGuids.BlockA);
            capturedNotification.FromPosition.Should().Be(new Vector2Int(1, 1));
            capturedNotification.ToPosition.Should().Be(new Vector2Int(3, 3));
        }

        [Fact]
        public async Task Handle_WhenBlockDoesNotExist_ReturnsFailureAndPublishesNoNotification()
        {
            // Arrange
            var command = MoveBlockCommand.Create(
                TestGuids.BlockB, // Non-existent block
                new Vector2Int(2, 2)
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert - Result
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("Block not found")
            );

            // Assert - No notification published on failure
            _mockMediator.Verify(m => m.Publish(
                It.IsAny<BlockMovedNotification>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenToPositionIsOccupied_ReturnsFailureAndPublishesNoNotification()
        {
            // Arrange - Place two blocks
            var blockToMove = new BlockBuilder()
                .WithId(TestGuids.BlockA)
                .WithType(BlockType.Work)
                .WithPosition(1, 1)
                .Build();
            _gridStateService.PlaceBlock(blockToMove);

            var blockingBlock = new BlockBuilder()
                .WithId(TestGuids.BlockB)
                .WithType(BlockType.Study)
                .WithPosition(3, 3)
                .Build();
            _gridStateService.PlaceBlock(blockingBlock);

            var command = MoveBlockCommand.Create(
                TestGuids.BlockA,
                new Vector2Int(3, 3) // Occupied position
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert - Result
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("is already occupied")
            );

            // Assert - Block stays in original position
            var originalBlock = _gridStateService.GetBlockAt(new Vector2Int(1, 1));
            originalBlock.IsSome.Should().BeTrue();
            originalBlock.Match(Some: b => b.Id.Should().Be(TestGuids.BlockA), None: () => { });

            // Assert - No notification published on failure
            _mockMediator.Verify(m => m.Publish(
                It.IsAny<BlockMovedNotification>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenToPositionIsOutOfBounds_ReturnsFailureAndPublishesNoNotification()
        {
            // Arrange - Place a block
            var block = new BlockBuilder()
                .WithId(TestGuids.BlockA)
                .WithType(BlockType.Health)
                .WithPosition(2, 2)
                .Build();
            _gridStateService.PlaceBlock(block);

            var command = MoveBlockCommand.Create(
                TestGuids.BlockA,
                new Vector2Int(10, 10) // Out of 5x5 grid bounds
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert - Result
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("is outside grid bounds")
            );

            // Assert - Block stays in original position
            var originalBlock = _gridStateService.GetBlockAt(new Vector2Int(2, 2));
            originalBlock.IsSome.Should().BeTrue();
            originalBlock.Match(Some: b => b.Id.Should().Be(TestGuids.BlockA), None: () => { });

            // Assert - No notification published on failure
            _mockMediator.Verify(m => m.Publish(
                It.IsAny<BlockMovedNotification>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenMoveToSamePosition_ReturnsFailureAndPublishesNoNotification()
        {
            // Arrange - Place a block
            var block = new BlockBuilder()
                .WithId(TestGuids.BlockA)
                .WithType(BlockType.Fun)
                .WithPosition(2, 2)
                .Build();
            _gridStateService.PlaceBlock(block);

            var command = MoveBlockCommand.Create(
                TestGuids.BlockA,
                new Vector2Int(2, 2) // Same position as current
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert - Result
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("already at the target position")
            );

            // Assert - No notification published on failure
            _mockMediator.Verify(m => m.Publish(
                It.IsAny<BlockMovedNotification>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }
    }
}
