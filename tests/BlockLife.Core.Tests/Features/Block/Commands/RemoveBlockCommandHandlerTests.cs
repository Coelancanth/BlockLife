using BlockLife.Core.Application.Simulation;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Features.Block.Placement.Effects;
using BlockLife.Core.Features.Block.Placement.Rules;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Core.Tests.Builders;
using BlockLife.Core.Tests.Utils;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LanguageExt.Prelude;
using Unit = LanguageExt.Unit;

namespace BlockLife.Core.Tests.Features.Block.Commands
{
    /// <summary>
    /// Tests for RemoveBlockCommandHandler following the new implementation structure.
    /// </summary>
    public class RemoveBlockCommandHandlerTests
    {
        private readonly Mock<IBlockExistsRule> _mockBlockExistsRule;
        private readonly Mock<IGridStateService> _mockGridState;
        private readonly Mock<ISimulationManager> _mockSimulation;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<RemoveBlockCommandHandler>> _mockLogger;
        private readonly RemoveBlockCommandHandler _handler;

        public RemoveBlockCommandHandlerTests()
        {
            _mockBlockExistsRule = new Mock<IBlockExistsRule>();
            _mockGridState = new Mock<IGridStateService>();
            _mockSimulation = new Mock<ISimulationManager>();
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<RemoveBlockCommandHandler>>();

            _handler = new RemoveBlockCommandHandler(
                _mockBlockExistsRule.Object,
                _mockGridState.Object,
                _mockSimulation.Object,
                _mockMediator.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_WhenBlockExists_ReturnsSuccessAndQueuesEffect()
        {
            // Arrange
            var position = new Vector2Int(2, 2);
            var block = new BlockBuilder()
                .WithId(TestGuids.BlockA)
                .WithType(BlockType.Work)
                .WithPosition(2, 2)
                .Build();

            var command = new RemoveBlockCommand(position);

            _mockBlockExistsRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(block));

            _mockGridState
                .Setup(x => x.RemoveBlock(position))
                .Returns(FinSucc(Unit.Default));

            _mockSimulation
                .Setup(x => x.QueueEffect(It.IsAny<BlockRemovedEffect>()))
                .Returns(FinSucc(Unit.Default));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSucc.Should().BeTrue();

            _mockBlockExistsRule.Verify(x => x.Validate(position), Times.Once);
            _mockGridState.Verify(x => x.RemoveBlock(position), Times.Once);
            _mockSimulation.Verify(x => x.QueueEffect(It.Is<BlockRemovedEffect>(e =>
                e.BlockId == TestGuids.BlockA &&
                e.Position == position &&
                e.Type == BlockType.Work
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenBlockDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var position = new Vector2Int(2, 2);
            var command = new RemoveBlockCommand(position);
            var expectedError = Error.New("NO_BLOCK_AT_POSITION", "No block exists at position");

            _mockBlockExistsRule
                .Setup(x => x.Validate(position))
                .Returns(FinFail<BlockLife.Core.Domain.Block.Block>(expectedError));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("NO_BLOCK_AT_POSITION")
            );

            _mockBlockExistsRule.Verify(x => x.Validate(position), Times.Once);
            _mockGridState.Verify(x => x.RemoveBlock(It.IsAny<Vector2Int>()), Times.Never);
            _mockSimulation.Verify(x => x.QueueEffect(It.IsAny<BlockRemovedEffect>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenGridStateFailsToRemove_ReturnsFailure()
        {
            // Arrange
            var position = new Vector2Int(2, 2);
            var block = new BlockBuilder()
                .WithId(TestGuids.BlockA)
                .WithType(BlockType.Work)
                .WithPosition(2, 2)
                .Build();

            var command = new RemoveBlockCommand(position);
            var expectedError = Error.New("REMOVAL_FAILED", "Failed to remove block");

            _mockBlockExistsRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(block));

            _mockGridState
                .Setup(x => x.RemoveBlock(position))
                .Returns(FinFail<Unit>(expectedError));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("REMOVAL_FAILED")
            );

            _mockSimulation.Verify(x => x.QueueEffect(It.IsAny<BlockRemovedEffect>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenEffectQueueingFails_ReturnsFailure()
        {
            // Arrange
            var position = new Vector2Int(2, 2);
            var block = new BlockBuilder()
                .WithId(TestGuids.BlockA)
                .WithType(BlockType.Work)
                .WithPosition(2, 2)
                .Build();

            var command = new RemoveBlockCommand(position);
            var expectedError = Error.New("QUEUE_EFFECT_FAILED", "Failed to queue effect");

            _mockBlockExistsRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(block));

            _mockGridState
                .Setup(x => x.RemoveBlock(position))
                .Returns(FinSucc(Unit.Default));

            _mockSimulation
                .Setup(x => x.QueueEffect(It.IsAny<BlockRemovedEffect>()))
                .Returns(FinFail<Unit>(expectedError));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("QUEUE_EFFECT_FAILED")
            );
        }

        [Fact]
        public async Task Handle_RemovingDifferentBlockTypes_WorksCorrectly()
        {
            // Arrange
            var testCases = new[]
            {
                (new Vector2Int(0, 0), BlockType.Basic, TestGuids.BlockA),
                (new Vector2Int(1, 1), BlockType.Work, TestGuids.BlockB),
                (new Vector2Int(2, 2), BlockType.Study, TestGuids.BlockC),
                (new Vector2Int(3, 3), BlockType.Health, TestGuids.BlockD)
            };

            _mockGridState
                .Setup(x => x.RemoveBlock(It.IsAny<Vector2Int>()))
                .Returns(FinSucc(Unit.Default));

            _mockSimulation
                .Setup(x => x.QueueEffect(It.IsAny<BlockRemovedEffect>()))
                .Returns(FinSucc(Unit.Default));

            // Act & Assert
            foreach (var (position, blockType, blockId) in testCases)
            {
                var block = new BlockBuilder()
                    .WithId(blockId)
                    .WithType(blockType)
                    .WithPosition(position.X, position.Y)
                    .Build();

                _mockBlockExistsRule
                    .Setup(x => x.Validate(position))
                    .Returns(FinSucc(block));

                var command = new RemoveBlockCommand(position);
                var result = await _handler.Handle(command, CancellationToken.None);

                result.IsSucc.Should().BeTrue();

                _mockSimulation.Verify(x => x.QueueEffect(It.Is<BlockRemovedEffect>(e =>
                    e.BlockId == blockId &&
                    e.Position == position &&
                    e.Type == blockType
                )), Times.Once);
            }
        }
    }

    /// <summary>
    /// Tests for RemoveBlockByIdCommandHandler.
    /// </summary>
    public class RemoveBlockByIdCommandHandlerTests
    {
        private readonly Mock<IBlockExistsRule> _mockBlockExistsRule;
        private readonly Mock<IGridStateService> _mockGridState;
        private readonly Mock<ISimulationManager> _mockSimulation;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<RemoveBlockByIdCommandHandler>> _mockLogger;
        private readonly RemoveBlockByIdCommandHandler _handler;

        public RemoveBlockByIdCommandHandlerTests()
        {
            _mockBlockExistsRule = new Mock<IBlockExistsRule>();
            _mockGridState = new Mock<IGridStateService>();
            _mockSimulation = new Mock<ISimulationManager>();
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<RemoveBlockByIdCommandHandler>>();

            _handler = new RemoveBlockByIdCommandHandler(
                _mockBlockExistsRule.Object,
                _mockGridState.Object,
                _mockSimulation.Object,
                _mockMediator.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_WhenBlockExists_ReturnsSuccessAndQueuesEffect()
        {
            // Arrange
            var blockId = TestGuids.BlockA;
            var block = new BlockBuilder()
                .WithId(blockId)
                .WithType(BlockType.Work)
                .WithPosition(2, 2)
                .Build();

            var command = new RemoveBlockByIdCommand(blockId);

            _mockBlockExistsRule
                .Setup(x => x.Validate(blockId))
                .Returns(FinSucc(block));

            _mockGridState
                .Setup(x => x.RemoveBlock(blockId))
                .Returns(FinSucc(Unit.Default));

            _mockSimulation
                .Setup(x => x.QueueEffect(It.IsAny<BlockRemovedEffect>()))
                .Returns(FinSucc(Unit.Default));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSucc.Should().BeTrue();

            _mockBlockExistsRule.Verify(x => x.Validate(blockId), Times.Once);
            _mockGridState.Verify(x => x.RemoveBlock(blockId), Times.Once);
            _mockSimulation.Verify(x => x.QueueEffect(It.Is<BlockRemovedEffect>(e =>
                e.BlockId == blockId &&
                e.Position == new Vector2Int(2, 2) &&
                e.Type == BlockType.Work
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenBlockDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var blockId = TestGuids.BlockA;
            var command = new RemoveBlockByIdCommand(blockId);
            var expectedError = Error.New("BLOCK_NOT_FOUND", "Block not found");

            _mockBlockExistsRule
                .Setup(x => x.Validate(blockId))
                .Returns(FinFail<BlockLife.Core.Domain.Block.Block>(expectedError));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("BLOCK_NOT_FOUND")
            );

            _mockBlockExistsRule.Verify(x => x.Validate(blockId), Times.Once);
            _mockGridState.Verify(x => x.RemoveBlock(It.IsAny<Guid>()), Times.Never);
            _mockSimulation.Verify(x => x.QueueEffect(It.IsAny<BlockRemovedEffect>()), Times.Never);
        }
    }
}
