using BlockLife.Core.Domain.Turn;
using BlockLife.Core.Features.Block.Patterns;
using BlockLife.Core.Features.Turn.Commands;
using BlockLife.Core.Features.Turn.Effects;
using BlockLife.Core.Tests.Utils;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Moq;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Tests.Features.Turn.Commands
{
    /// <summary>
    /// TDD Tests for AdvanceTurnCommandHandler - Following Comprehensive Development Workflow
    /// These tests are written FIRST to drive the implementation (RED phase)
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Feature", "Turn")]
    [Trait("Layer", "Commands")]
    public class AdvanceTurnCommandHandlerTests
    {
        private readonly Mock<ITurnManager> _mockTurnManager;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IPatternProcessingTracker> _mockPatternTracker;
        private readonly AdvanceTurnCommandHandler _handler;

        public AdvanceTurnCommandHandlerTests()
        {
            _mockTurnManager = new Mock<ITurnManager>();
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger>();
            _mockPatternTracker = new Mock<IPatternProcessingTracker>();
            
            // Setup pattern tracker to always return complete immediately
            _mockPatternTracker
                .Setup(t => t.WaitForProcessingCompleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockPatternTracker
                .Setup(t => t.IsProcessing)
                .Returns(false);

            _handler = new AdvanceTurnCommandHandler(
                _mockTurnManager.Object,
                _mockMediator.Object,
                _mockLogger.Object,
                _mockPatternTracker.Object
            );
        }

        [Fact]
        public async Task Handle_WhenTurnAdvancementSucceeds_ReturnsSuccessAndPublishesBothNotifications()
        {
            // Arrange
            var currentTurn = new BlockLife.Core.Domain.Turn.Turn { Number = 1, CreatedAt = DateTime.UtcNow };
            var newTurn = new BlockLife.Core.Domain.Turn.Turn { Number = 2, CreatedAt = DateTime.UtcNow };
            var command = AdvanceTurnCommand.Create();

            _mockTurnManager
                .Setup(m => m.GetCurrentTurn())
                .Returns(Some(currentTurn));
            _mockTurnManager
                .Setup(m => m.AdvanceTurn())
                .Returns(FinSucc(newTurn));
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            TurnEndNotification? capturedEndNotification = null;
            TurnStartNotification? capturedStartNotification = null;
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<TurnEndNotification>(), It.IsAny<CancellationToken>()))
                .Callback<INotification, CancellationToken>((notification, _) =>
                {
                    capturedEndNotification = notification as TurnEndNotification;
                })
                .Returns(Task.CompletedTask);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<TurnStartNotification>(), It.IsAny<CancellationToken>()))
                .Callback<INotification, CancellationToken>((notification, _) =>
                {
                    capturedStartNotification = notification as TurnStartNotification;
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert - Result
            result.IsSucc.Should().BeTrue();

            // Assert - TurnManager interactions
            _mockTurnManager.Verify(m => m.GetCurrentTurn(), Times.Once);
            _mockTurnManager.Verify(m => m.AdvanceTurn(), Times.Once);

            // Assert - TurnEndNotification published
            _mockMediator.Verify(m => m.Publish(
                It.IsAny<TurnEndNotification>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
            capturedEndNotification.Should().NotBeNull();
            capturedEndNotification!.Turn.Number.Should().Be(1);

            // Assert - TurnStartNotification published
            _mockMediator.Verify(m => m.Publish(
                It.IsAny<TurnStartNotification>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
            capturedStartNotification.Should().NotBeNull();
            capturedStartNotification!.Turn.Number.Should().Be(2);
        }

        [Fact]
        public async Task Handle_WhenTurnNotInitialized_ReturnsFailureWithError()
        {
            // Arrange
            var command = AdvanceTurnCommand.Create();

            _mockTurnManager
                .Setup(m => m.GetCurrentTurn())
                .Returns(None);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert - Result
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => throw new Exception("Should not succeed"),
                Fail: error => error.Message.Should().Be("TURN_NOT_INITIALIZED")
            );

            // Assert - TurnManager interactions
            _mockTurnManager.Verify(m => m.GetCurrentTurn(), Times.Once);
            _mockTurnManager.Verify(m => m.AdvanceTurn(), Times.Never);

            // Assert - No notifications published
            _mockMediator.Verify(m => m.Publish(
                It.IsAny<INotification>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenAdvanceTurnFails_ReturnsFailureWithError()
        {
            // Arrange
            var currentTurn = new BlockLife.Core.Domain.Turn.Turn { Number = 5, CreatedAt = DateTime.UtcNow };
            var command = AdvanceTurnCommand.Create();
            var expectedError = Error.New("CANNOT_ADVANCE", "No action performed this turn");

            _mockTurnManager
                .Setup(m => m.GetCurrentTurn())
                .Returns(Some(currentTurn));
            _mockTurnManager
                .Setup(m => m.AdvanceTurn())
                .Returns(FinFail<BlockLife.Core.Domain.Turn.Turn>(expectedError));
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<TurnEndNotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert - Result
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => throw new Exception("Should not succeed"),
                Fail: error => error.Message.Should().Be("CANNOT_ADVANCE")
            );

            // Assert - TurnManager interactions
            _mockTurnManager.Verify(m => m.GetCurrentTurn(), Times.Once);
            _mockTurnManager.Verify(m => m.AdvanceTurn(), Times.Once);

            // Assert - TurnEndNotification published but not TurnStartNotification
            _mockMediator.Verify(m => m.Publish(
                It.IsAny<TurnEndNotification>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
            _mockMediator.Verify(m => m.Publish(
                It.IsAny<TurnStartNotification>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenTurnEndNotificationFails_ReturnsFailureWithError()
        {
            // Arrange
            var currentTurn = new BlockLife.Core.Domain.Turn.Turn { Number = 3, CreatedAt = DateTime.UtcNow };
            var command = AdvanceTurnCommand.Create();

            _mockTurnManager
                .Setup(m => m.GetCurrentTurn())
                .Returns(Some(currentTurn));
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<TurnEndNotification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Network error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert - Result
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => throw new Exception("Should not succeed"),
                Fail: error => error.Message.Should().Be("TURN_END_NOTIFICATION_FAILED")
            );

            // Assert - TurnManager interactions (advance should not be called if end notification fails)
            _mockTurnManager.Verify(m => m.GetCurrentTurn(), Times.Once);
            _mockTurnManager.Verify(m => m.AdvanceTurn(), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenTurnStartNotificationFails_ReturnsFailureWithError()
        {
            // Arrange
            var currentTurn = new BlockLife.Core.Domain.Turn.Turn { Number = 7, CreatedAt = DateTime.UtcNow };
            var newTurn = new BlockLife.Core.Domain.Turn.Turn { Number = 8, CreatedAt = DateTime.UtcNow };
            var command = AdvanceTurnCommand.Create();

            _mockTurnManager
                .Setup(m => m.GetCurrentTurn())
                .Returns(Some(currentTurn));
            _mockTurnManager
                .Setup(m => m.AdvanceTurn())
                .Returns(FinSucc(newTurn));
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<TurnEndNotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<TurnStartNotification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Service unavailable"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert - Result
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => throw new Exception("Should not succeed"),
                Fail: error => error.Message.Should().Be("TURN_START_NOTIFICATION_FAILED")
            );

            // Assert - All operations up to the failure should have been called
            _mockTurnManager.Verify(m => m.GetCurrentTurn(), Times.Once);
            _mockTurnManager.Verify(m => m.AdvanceTurn(), Times.Once);
            _mockMediator.Verify(m => m.Publish(
                It.IsAny<TurnEndNotification>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
            _mockMediator.Verify(m => m.Publish(
                It.IsAny<TurnStartNotification>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public void AdvanceTurnCommand_Create_ProducesValidCommand()
        {
            // Act
            var command = AdvanceTurnCommand.Create();

            // Assert
            command.Should().NotBeNull();
            command.Should().BeOfType<AdvanceTurnCommand>();
        }
    }
}