using BlockLife.Core.Domain.Turn;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Effects;
using BlockLife.Core.Features.Turn.Commands;
using BlockLife.Core.Features.Turn.Notifications;
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

namespace BlockLife.Core.Tests.Features.Turn.Notifications
{
    /// <summary>
    /// TDD Tests for TurnAdvancementAfterMoveHandler - Following Comprehensive Development Workflow
    /// Tests integration between block movement and turn system.
    /// Only user moves should advance turns, not cascades or patterns.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Feature", "Turn")]
    [Trait("Layer", "Handlers")]
    public class TurnAdvancementAfterMoveHandlerTests
    {
        private readonly Mock<ITurnManager> _mockTurnManager;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger> _mockLogger;
        private readonly TurnAdvancementAfterMoveHandler _handler;

        public TurnAdvancementAfterMoveHandlerTests()
        {
            _mockTurnManager = new Mock<ITurnManager>();
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger>();

            _handler = new TurnAdvancementAfterMoveHandler(
                _mockTurnManager.Object,
                _mockMediator.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_WhenBlockMovedAndCanAdvanceTurn_MarksActionAndAdvancesTurn()
        {
            // Arrange
            var blockMovedNotification = BlockMovedNotification.Create(
                TestGuids.BlockA,
                new Vector2Int(2, 3),
                new Vector2Int(4, 5)
            );

            _mockTurnManager
                .Setup(m => m.CanAdvanceTurn())
                .Returns(true);
            _mockMediator
                .Setup(m => m.Send(It.IsAny<AdvanceTurnCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FinSucc(LanguageExt.Unit.Default));

            AdvanceTurnCommand? capturedCommand = null;
            _mockMediator
                .Setup(m => m.Send(It.IsAny<AdvanceTurnCommand>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<Fin<LanguageExt.Unit>>, CancellationToken>((command, _) =>
                {
                    capturedCommand = command as AdvanceTurnCommand;
                })
                .ReturnsAsync(FinSucc(LanguageExt.Unit.Default));

            // Act
            await _handler.Handle(blockMovedNotification, CancellationToken.None);

            // Assert - Action marked
            _mockTurnManager.Verify(m => m.MarkActionPerformed(), Times.Once);

            // Assert - Turn advancement checked and executed
            _mockTurnManager.Verify(m => m.CanAdvanceTurn(), Times.Once);
            _mockMediator.Verify(m => m.Send(
                It.IsAny<AdvanceTurnCommand>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);

            capturedCommand.Should().NotBeNull();
            capturedCommand.Should().BeOfType<AdvanceTurnCommand>();
        }

        [Fact]
        public async Task Handle_WhenBlockMovedButCannotAdvanceTurn_MarksActionButDoesNotAdvance()
        {
            // Arrange
            var blockMovedNotification = BlockMovedNotification.Create(
                TestGuids.BlockB,
                new Vector2Int(1, 1),
                new Vector2Int(2, 2)
            );

            _mockTurnManager
                .Setup(m => m.CanAdvanceTurn())
                .Returns(false);

            // Act
            await _handler.Handle(blockMovedNotification, CancellationToken.None);

            // Assert - Action marked
            _mockTurnManager.Verify(m => m.MarkActionPerformed(), Times.Once);

            // Assert - Turn advancement checked but not executed
            _mockTurnManager.Verify(m => m.CanAdvanceTurn(), Times.Once);
            _mockMediator.Verify(m => m.Send(
                It.IsAny<AdvanceTurnCommand>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenAdvanceTurnCommandFails_DoesNotThrowException()
        {
            // Arrange
            var blockMovedNotification = BlockMovedNotification.Create(
                TestGuids.BlockC,
                new Vector2Int(4, 4),
                new Vector2Int(5, 5)
            );

            _mockTurnManager
                .Setup(m => m.CanAdvanceTurn())
                .Returns(true);
            _mockMediator
                .Setup(m => m.Send(It.IsAny<AdvanceTurnCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FinFail<LanguageExt.Unit>(Error.New("ADVANCE_FAILED", "Turn advancement failed")));

            // Act - Should not throw
            Func<Task> act = async () => await _handler.Handle(blockMovedNotification, CancellationToken.None);

            // Assert - No exception thrown
            await act.Should().NotThrowAsync();

            // Assert - All expected interactions occurred
            _mockTurnManager.Verify(m => m.MarkActionPerformed(), Times.Once);
            _mockTurnManager.Verify(m => m.CanAdvanceTurn(), Times.Once);
            _mockMediator.Verify(m => m.Send(
                It.IsAny<AdvanceTurnCommand>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Handle_MultipleBlockPlacements_MarksActionEachTime()
        {
            // Arrange
            var notification1 = BlockMovedNotification.Create(
                TestGuids.BlockA,
                new Vector2Int(1, 1),
                new Vector2Int(3, 3)
            );
            var notification2 = BlockMovedNotification.Create(
                TestGuids.BlockB,
                new Vector2Int(2, 2),
                new Vector2Int(4, 4)
            );

            _mockTurnManager
                .Setup(m => m.CanAdvanceTurn())
                .Returns(false); // Prevent turn advancement for clarity

            // Act
            await _handler.Handle(notification1, CancellationToken.None);
            await _handler.Handle(notification2, CancellationToken.None);

            // Assert - Action marked for each placement
            _mockTurnManager.Verify(m => m.MarkActionPerformed(), Times.Exactly(2));
            _mockTurnManager.Verify(m => m.CanAdvanceTurn(), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_WhenTurnManagerThrows_DoesNotPropagatException()
        {
            // Arrange
            var blockMovedNotification = BlockMovedNotification.Create(
                TestGuids.BlockD,
                new Vector2Int(0, 0),
                new Vector2Int(1, 1)
            );

            _mockTurnManager
                .Setup(m => m.MarkActionPerformed())
                .Throws(new InvalidOperationException("Turn manager error"));

            // Act - Should not throw
            Func<Task> act = async () => await _handler.Handle(blockMovedNotification, CancellationToken.None);

            // Assert - No exception propagated (logged instead)
            await act.Should().NotThrowAsync();

            // Assert - MarkActionPerformed was attempted
            _mockTurnManager.Verify(m => m.MarkActionPerformed(), Times.Once);
            // CanAdvanceTurn should not be called if MarkActionPerformed throws
            _mockTurnManager.Verify(m => m.CanAdvanceTurn(), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenCanAdvanceTurnThrows_ContinuesGracefully()
        {
            // Arrange
            var blockMovedNotification = BlockMovedNotification.Create(
                TestGuids.BlockE,
                new Vector2Int(3, 3),
                new Vector2Int(6, 6)
            );

            _mockTurnManager
                .Setup(m => m.CanAdvanceTurn())
                .Throws(new InvalidOperationException("Cannot check turn advancement"));

            // Act - Should not throw
            Func<Task> act = async () => await _handler.Handle(blockMovedNotification, CancellationToken.None);

            // Assert - No exception propagated
            await act.Should().NotThrowAsync();

            // Assert - Action was marked before the exception
            _mockTurnManager.Verify(m => m.MarkActionPerformed(), Times.Once);
            _mockTurnManager.Verify(m => m.CanAdvanceTurn(), Times.Once);
            _mockMediator.Verify(m => m.Send(
                It.IsAny<AdvanceTurnCommand>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }
    }
}