using BlockLife.Core.Application.Simulation;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Features.Block.Placement.Effects;
using BlockLife.Core.Features.Block.Placement.Rules;
using BlockLife.Core.Infrastructure.Services;
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
    /// Tests for PlaceBlockCommandHandler following the new implementation structure.
    /// </summary>
    public class PlaceBlockCommandHandlerTests
    {
        private readonly Mock<IPositionIsValidRule> _mockPositionValidRule;
        private readonly Mock<IPositionIsEmptyRule> _mockPositionEmptyRule;
        private readonly Mock<IGridStateService> _mockGridState;
        private readonly Mock<ISimulationManager> _mockSimulation;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<PlaceBlockCommandHandler>> _mockLogger;
        private readonly PlaceBlockCommandHandler _handler;

        public PlaceBlockCommandHandlerTests()
        {
            _mockPositionValidRule = new Mock<IPositionIsValidRule>();
            _mockPositionEmptyRule = new Mock<IPositionIsEmptyRule>();
            _mockGridState = new Mock<IGridStateService>();
            _mockSimulation = new Mock<ISimulationManager>();
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<PlaceBlockCommandHandler>>();

            _handler = new PlaceBlockCommandHandler(
                _mockPositionValidRule.Object,
                _mockPositionEmptyRule.Object,
                _mockGridState.Object,
                _mockSimulation.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_WhenPlacementIsValid_ReturnsSuccessAndQueuesEffect()
        {
            // Arrange
            var position = new Vector2Int(2, 2);
            var command = new PlaceBlockCommand(position, BlockType.Work, TestGuids.BlockA);

            _mockPositionValidRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(Unit.Default));

            _mockPositionEmptyRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(Unit.Default));

            _mockGridState
                .Setup(x => x.PlaceBlock(It.IsAny<Domain.Block.Block>()))
                .Returns(FinSucc(Unit.Default));

            _mockSimulation
                .Setup(x => x.QueueEffect(It.IsAny<BlockPlacedEffect>()))
                .Returns(FinSucc(Unit.Default));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSucc.Should().BeTrue();

            _mockPositionValidRule.Verify(x => x.Validate(position), Times.Once);
            _mockPositionEmptyRule.Verify(x => x.Validate(position), Times.Once);
            _mockGridState.Verify(x => x.PlaceBlock(It.Is<Domain.Block.Block>(b =>
                b.Id == TestGuids.BlockA &&
                b.Position == position &&
                b.Type == BlockType.Work
            )), Times.Once);
            _mockSimulation.Verify(x => x.QueueEffect(It.Is<BlockPlacedEffect>(e =>
                e.BlockId == TestGuids.BlockA &&
                e.Position == position &&
                e.Type == BlockType.Work
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenPositionIsInvalid_ReturnsFailure()
        {
            // Arrange
            var position = new Vector2Int(10, 10);
            var command = new PlaceBlockCommand(position, BlockType.Work);
            var expectedError = Error.New("INVALID_POSITION", "Position is outside grid bounds");

            _mockPositionValidRule
                .Setup(x => x.Validate(position))
                .Returns(FinFail<Unit>(expectedError));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("INVALID_POSITION")
            );

            _mockPositionValidRule.Verify(x => x.Validate(position), Times.Once);
            _mockPositionEmptyRule.Verify(x => x.Validate(It.IsAny<Vector2Int>()), Times.Never);
            _mockGridState.Verify(x => x.PlaceBlock(It.IsAny<Domain.Block.Block>()), Times.Never);
            _mockSimulation.Verify(x => x.QueueEffect(It.IsAny<BlockPlacedEffect>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenPositionIsOccupied_ReturnsFailure()
        {
            // Arrange
            var position = new Vector2Int(2, 2);
            var command = new PlaceBlockCommand(position, BlockType.Work);
            var expectedError = Error.New("POSITION_OCCUPIED", "Position is already occupied");

            _mockPositionValidRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(Unit.Default));

            _mockPositionEmptyRule
                .Setup(x => x.Validate(position))
                .Returns(FinFail<Unit>(expectedError));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("POSITION_OCCUPIED")
            );

            _mockPositionValidRule.Verify(x => x.Validate(position), Times.Once);
            _mockPositionEmptyRule.Verify(x => x.Validate(position), Times.Once);
            _mockGridState.Verify(x => x.PlaceBlock(It.IsAny<Domain.Block.Block>()), Times.Never);
            _mockSimulation.Verify(x => x.QueueEffect(It.IsAny<BlockPlacedEffect>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenGridStateFailsToPlace_ReturnsFailure()
        {
            // Arrange
            var position = new Vector2Int(2, 2);
            var command = new PlaceBlockCommand(position, BlockType.Work);
            var expectedError = Error.New("PLACEMENT_FAILED", "Failed to place block");

            _mockPositionValidRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(Unit.Default));

            _mockPositionEmptyRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(Unit.Default));

            _mockGridState
                .Setup(x => x.PlaceBlock(It.IsAny<Domain.Block.Block>()))
                .Returns(FinFail<Unit>(expectedError));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("PLACEMENT_FAILED")
            );

            _mockSimulation.Verify(x => x.QueueEffect(It.IsAny<BlockPlacedEffect>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenEffectQueueingFails_ReturnsFailure()
        {
            // Arrange
            var position = new Vector2Int(2, 2);
            var command = new PlaceBlockCommand(position, BlockType.Work);
            var expectedError = Error.New("QUEUE_EFFECT_FAILED", "Failed to queue effect");

            _mockPositionValidRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(Unit.Default));

            _mockPositionEmptyRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(Unit.Default));

            _mockGridState
                .Setup(x => x.PlaceBlock(It.IsAny<Domain.Block.Block>()))
                .Returns(FinSucc(Unit.Default));

            _mockSimulation
                .Setup(x => x.QueueEffect(It.IsAny<BlockPlacedEffect>()))
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
        public async Task Handle_WithoutProvidedId_GeneratesNewGuid()
        {
            // Arrange
            var position = new Vector2Int(2, 2);
            var command = new PlaceBlockCommand(position, BlockType.Work);

            _mockPositionValidRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(Unit.Default));

            _mockPositionEmptyRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(Unit.Default));

            _mockGridState
                .Setup(x => x.PlaceBlock(It.IsAny<Domain.Block.Block>()))
                .Returns(FinSucc(Unit.Default));

            _mockSimulation
                .Setup(x => x.QueueEffect(It.IsAny<BlockPlacedEffect>()))
                .Returns(FinSucc(Unit.Default));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSucc.Should().BeTrue();

            _mockGridState.Verify(x => x.PlaceBlock(It.Is<Domain.Block.Block>(b =>
                b.Id != Guid.Empty &&
                b.Position == position &&
                b.Type == BlockType.Work
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_WithDifferentBlockTypes_PlacesCorrectly()
        {
            // Arrange
            var testCases = new[]
            {
                (new Vector2Int(0, 0), BlockType.Basic),
                (new Vector2Int(1, 1), BlockType.Work),
                (new Vector2Int(2, 2), BlockType.Study),
                (new Vector2Int(3, 3), BlockType.Health)
            };

            _mockPositionValidRule
                .Setup(x => x.Validate(It.IsAny<Vector2Int>()))
                .Returns(FinSucc(Unit.Default));

            _mockPositionEmptyRule
                .Setup(x => x.Validate(It.IsAny<Vector2Int>()))
                .Returns(FinSucc(Unit.Default));

            _mockGridState
                .Setup(x => x.PlaceBlock(It.IsAny<Domain.Block.Block>()))
                .Returns(FinSucc(Unit.Default));

            _mockSimulation
                .Setup(x => x.QueueEffect(It.IsAny<BlockPlacedEffect>()))
                .Returns(FinSucc(Unit.Default));

            // Act & Assert
            foreach (var (position, blockType) in testCases)
            {
                var command = new PlaceBlockCommand(position, blockType);
                var result = await _handler.Handle(command, CancellationToken.None);

                result.IsSucc.Should().BeTrue();

                _mockGridState.Verify(x => x.PlaceBlock(It.Is<Domain.Block.Block>(b =>
                    b.Position == position &&
                    b.Type == blockType
                )), Times.Once);
            }
        }

        /// <summary>
        /// REGRESSION TEST: Ensures BlockId remains stable across multiple property accesses.
        /// 
        /// BUG CONTEXT:
        /// - Date: 2025-08-14
        /// - Issue: PlaceBlockCommand.BlockId was generating new GUID on every access
        /// - Symptom: "Block already exists" error despite successful placement
        /// - Root Cause: Different GUIDs used for block creation vs effect queueing
        /// - Fix: Use Lazy<Guid> to generate stable ID once and cache it
        /// 
        /// This test prevents regression by verifying that:
        /// 1. BlockId property returns the same value on multiple accesses
        /// 2. The ID is consistent throughout the command lifecycle
        /// 3. All operations (placement, effects, notifications) use the same ID
        /// </summary>
        [Fact]
        public void PlaceBlockCommand_BlockId_RemainsStableAcrossMultipleAccesses()
        {
            // Arrange
            var position = new Vector2Int(5, 5);
            var command = new PlaceBlockCommand(position, BlockType.Basic);

            // Act - Access BlockId multiple times (simulating handler usage)
            var firstAccess = command.BlockId;
            var secondAccess = command.BlockId;
            var thirdAccess = command.BlockId;

            // Assert - All accesses return the same GUID
            firstAccess.Should().Be(secondAccess,
                "BlockId should return the same value on multiple accesses");
            secondAccess.Should().Be(thirdAccess,
                "BlockId should remain stable throughout command lifecycle");

            firstAccess.Should().NotBe(Guid.Empty,
                "BlockId should be a valid non-empty GUID");
        }

        /// <summary>
        /// REGRESSION TEST: Ensures BlockId consistency in realistic handler scenario.
        /// 
        /// This test simulates the exact scenario that caused the original bug:
        /// - Handler accesses BlockId for block creation
        /// - Handler accesses BlockId again for effect queueing
        /// - Both operations must use the same ID to prevent "already exists" error
        /// </summary>
        [Fact]
        public async Task Handle_BlockIdConsistency_UsesStableIdThroughoutOperation()
        {
            // Arrange
            var position = new Vector2Int(4, 3);
            var command = new PlaceBlockCommand(position, BlockType.Basic);

            // Capture the expected BlockId that should be used throughout
            var expectedBlockId = command.BlockId;

            _mockPositionValidRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(Unit.Default));

            _mockPositionEmptyRule
                .Setup(x => x.Validate(position))
                .Returns(FinSucc(Unit.Default));

            _mockGridState
                .Setup(x => x.PlaceBlock(It.IsAny<Domain.Block.Block>()))
                .Returns(FinSucc(Unit.Default));

            _mockSimulation
                .Setup(x => x.QueueEffect(It.IsAny<BlockPlacedEffect>()))
                .Returns(FinSucc(Unit.Default));

            _mockSimulation
                .Setup(x => x.ProcessQueuedEffectsAsync())
                .Returns(Task.FromResult(FinSucc(Unit.Default)));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert - Result should be successful
            result.IsSucc.Should().BeTrue();

            // CRITICAL: Verify that the SAME BlockId was used for both operations
            _mockGridState.Verify(x => x.PlaceBlock(It.Is<Domain.Block.Block>(b =>
                b.Id == expectedBlockId && // Same ID used for block creation
                b.Position == position &&
                b.Type == BlockType.Basic
            )), Times.Once, "Block creation should use the stable BlockId");

            _mockSimulation.Verify(x => x.QueueEffect(It.Is<BlockPlacedEffect>(e =>
                e.BlockId == expectedBlockId && // Same ID used for effect
                e.Position == position &&
                e.Type == BlockType.Basic
            )), Times.Once, "Effect queueing should use the same stable BlockId");
        }

        /// <summary>
        /// REGRESSION TEST: Ensures explicit RequestedId takes precedence and remains stable.
        /// 
        /// When a specific ID is provided, it should be used consistently instead of generating a new one.
        /// </summary>
        [Fact]
        public void PlaceBlockCommand_WithRequestedId_UsesProvidedIdStably()
        {
            // Arrange
            var expectedId = Guid.NewGuid();
            var position = new Vector2Int(7, 8);
            var command = new PlaceBlockCommand(position, BlockType.Work, expectedId);

            // Act - Access BlockId multiple times
            var firstAccess = command.BlockId;
            var secondAccess = command.BlockId;

            // Assert - Should use the provided ID consistently
            firstAccess.Should().Be(expectedId,
                "BlockId should use the explicitly provided RequestedId");
            secondAccess.Should().Be(expectedId,
                "BlockId should remain stable even with explicit RequestedId");
            firstAccess.Should().Be(secondAccess,
                "Multiple accesses should return the same RequestedId");
        }
    }
}
