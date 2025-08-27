using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Features.Spawn.Domain;
using BlockLife.Core.Features.Spawn.Notifications;
using BlockLife.Core.Features.Turn.Effects;
using BlockLife.Core.Infrastructure.Services;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Serilog;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Tests.Features.Spawn.Notifications
{
    /// <summary>
    /// Comprehensive tests for AutoSpawnHandler application logic.
    /// Tests MediatR integration, error handling, and graceful degradation.
    /// Part of VS_007 Phase 2 - Application Layer testing.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Feature", "Spawn")]
    [Trait("Layer", "Handlers")]
    public class AutoSpawnHandlerTests
    {
        private readonly Mock<IAutoSpawnStrategy> _mockSpawnStrategy;
        private readonly Mock<IGridStateService> _mockGridStateService;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger> _mockLogger;
        private readonly AutoSpawnHandler _handler;
        private readonly Vector2Int _standardGridSize = new(5, 5);

        public AutoSpawnHandlerTests()
        {
            _mockSpawnStrategy = new Mock<IAutoSpawnStrategy>();
            _mockGridStateService = new Mock<IGridStateService>();
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger>();

            _handler = new AutoSpawnHandler(
                _mockSpawnStrategy.Object,
                _mockGridStateService.Object,
                _mockMediator.Object,
                _mockLogger.Object
            );

            // Setup default grid state service behavior
            _mockGridStateService.Setup(x => x.GetGridDimensions())
                .Returns(_standardGridSize);
        }

        [Fact]
        public void Constructor_WithNullSpawnStrategy_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AutoSpawnHandler(
                null!,
                _mockGridStateService.Object,
                _mockMediator.Object,
                _mockLogger.Object
            ));
        }

        [Fact]
        public void Constructor_WithNullGridStateService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AutoSpawnHandler(
                _mockSpawnStrategy.Object,
                null!,
                _mockMediator.Object,
                _mockLogger.Object
            ));
        }

        [Fact]
        public void Constructor_WithNullMediator_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AutoSpawnHandler(
                _mockSpawnStrategy.Object,
                _mockGridStateService.Object,
                null!,
                _mockLogger.Object
            ));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AutoSpawnHandler(
                _mockSpawnStrategy.Object,
                _mockGridStateService.Object,
                _mockMediator.Object,
                null!
            ));
        }

        [Fact]
        public async Task Handle_WithEmptyGrid_SpawnsBlockSuccessfully()
        {
            // Arrange
            var turn = BlockLife.Core.Domain.Turn.Turn.Create(5).IfFail(_ => throw new Exception());
            var notification = TurnStartNotification.Create(turn);
            var selectedPosition = new Vector2Int(2, 2);
            var selectedBlockType = BlockType.Work;

            SetupEmptyGrid(new[] { selectedPosition });
            
            _mockSpawnStrategy.Setup(x => x.SelectSpawnPosition(It.IsAny<IEnumerable<Vector2Int>>(), _standardGridSize))
                .Returns(Some(selectedPosition));
            
            _mockSpawnStrategy.Setup(x => x.SelectBlockType(5))
                .Returns(selectedBlockType);
            
            _mockSpawnStrategy.Setup(x => x.StrategyName)
                .Returns("Test Strategy");

            _mockMediator.Setup(x => x.Send(It.IsAny<PlaceBlockCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FinSucc<LanguageExt.Unit>(LanguageExt.Unit.Default));

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _mockMediator.Verify(x => x.Send(
                It.Is<PlaceBlockCommand>(cmd => 
                    cmd.Position == selectedPosition && 
                    cmd.Type == selectedBlockType),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Handle_WithFullGrid_SkipsSpawnGracefully()
        {
            // Arrange
            var turn = BlockLife.Core.Domain.Turn.Turn.Create(3).IfFail(_ => throw new Exception());
            var notification = TurnStartNotification.Create(turn);

            SetupFullGrid(); // No empty positions
            
            _mockSpawnStrategy.Setup(x => x.SelectSpawnPosition(It.IsAny<IEnumerable<Vector2Int>>(), _standardGridSize))
                .Returns(None);
            
            _mockSpawnStrategy.Setup(x => x.StrategyName)
                .Returns("Test Strategy");

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert - Should not attempt to place any blocks
            _mockMediator.Verify(x => x.Send(It.IsAny<PlaceBlockCommand>(), It.IsAny<CancellationToken>()), 
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenPlaceBlockCommandFails_LogsWarningButDoesNotThrow()
        {
            // Arrange
            var turn = BlockLife.Core.Domain.Turn.Turn.Create(1).IfFail(_ => throw new Exception());
            var notification = TurnStartNotification.Create(turn);
            var selectedPosition = new Vector2Int(1, 1);

            SetupEmptyGrid(new[] { selectedPosition });
            
            _mockSpawnStrategy.Setup(x => x.SelectSpawnPosition(It.IsAny<IEnumerable<Vector2Int>>(), _standardGridSize))
                .Returns(Some(selectedPosition));
            
            _mockSpawnStrategy.Setup(x => x.SelectBlockType(1))
                .Returns(BlockType.Health);
            
            _mockSpawnStrategy.Setup(x => x.StrategyName)
                .Returns("Test Strategy");

            var error = Error.New("Position is occupied");
            _mockMediator.Setup(x => x.Send(It.IsAny<PlaceBlockCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FinFail<LanguageExt.Unit>(error));

            // Act & Assert - Should not throw
            await _handler.Handle(notification, CancellationToken.None);

            // Verify command was attempted
            _mockMediator.Verify(x => x.Send(It.IsAny<PlaceBlockCommand>(), It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task Handle_PassesCorrectTurnNumberToSpawnStrategy()
        {
            // Arrange
            var turn = BlockLife.Core.Domain.Turn.Turn.Create(42).IfFail(_ => throw new Exception());
            var notification = TurnStartNotification.Create(turn);
            var selectedPosition = new Vector2Int(0, 0);

            SetupEmptyGrid(new[] { selectedPosition });
            
            _mockSpawnStrategy.Setup(x => x.SelectSpawnPosition(It.IsAny<IEnumerable<Vector2Int>>(), _standardGridSize))
                .Returns(Some(selectedPosition));
            
            _mockSpawnStrategy.Setup(x => x.StrategyName)
                .Returns("Test Strategy");

            _mockMediator.Setup(x => x.Send(It.IsAny<PlaceBlockCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FinSucc<LanguageExt.Unit>(LanguageExt.Unit.Default));

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert - Verify turn number 42 was passed to SelectBlockType
            _mockSpawnStrategy.Verify(x => x.SelectBlockType(42), Times.Once);
        }

        [Fact]
        public async Task Handle_PassesEmptyPositionsToSpawnStrategy()
        {
            // Arrange
            var turn = BlockLife.Core.Domain.Turn.Turn.Create(1).IfFail(_ => throw new Exception());
            var notification = TurnStartNotification.Create(turn);
            
            var emptyPositions = new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 1),
                new Vector2Int(4, 4)
            };
            
            SetupEmptyGrid(emptyPositions);
            
            _mockSpawnStrategy.Setup(x => x.SelectSpawnPosition(It.IsAny<IEnumerable<Vector2Int>>(), _standardGridSize))
                .Returns(Some(emptyPositions[0]));
            
            _mockSpawnStrategy.Setup(x => x.StrategyName)
                .Returns("Test Strategy");

            _mockMediator.Setup(x => x.Send(It.IsAny<PlaceBlockCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FinSucc<LanguageExt.Unit>(LanguageExt.Unit.Default));

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert - Verify correct empty positions were passed
            _mockSpawnStrategy.Verify(x => x.SelectSpawnPosition(
                It.Is<IEnumerable<Vector2Int>>(positions => 
                    positions.Count() == 3 &&
                    positions.Contains(new Vector2Int(0, 0)) &&
                    positions.Contains(new Vector2Int(1, 1)) &&
                    positions.Contains(new Vector2Int(4, 4))
                ),
                _standardGridSize
            ), Times.Once);
        }

        [Fact]
        public async Task Handle_WithUnexpectedException_LogsErrorButDoesNotThrow()
        {
            // Arrange
            var turn = BlockLife.Core.Domain.Turn.Turn.Create(1).IfFail(_ => throw new Exception());
            var notification = TurnStartNotification.Create(turn);

            _mockSpawnStrategy.Setup(x => x.StrategyName)
                .Returns("Test Strategy");

            _mockGridStateService.Setup(x => x.GetGridDimensions())
                .Throws(new InvalidOperationException("Grid service error"));

            // Act & Assert - Should not throw
            await _handler.Handle(notification, CancellationToken.None);

            // Verify no commands were sent due to early failure
            _mockMediator.Verify(x => x.Send(It.IsAny<PlaceBlockCommand>(), It.IsAny<CancellationToken>()), 
                Times.Never);
        }

        [Fact]
        public async Task Handle_CancellationRequested_RespectsCancellation()
        {
            // Arrange
            var turn = BlockLife.Core.Domain.Turn.Turn.Create(1).IfFail(_ => throw new Exception());
            var notification = TurnStartNotification.Create(turn);
            var selectedPosition = new Vector2Int(2, 2);

            SetupEmptyGrid(new[] { selectedPosition });
            
            _mockSpawnStrategy.Setup(x => x.SelectSpawnPosition(It.IsAny<IEnumerable<Vector2Int>>(), _standardGridSize))
                .Returns(Some(selectedPosition));
            
            _mockSpawnStrategy.Setup(x => x.SelectBlockType(1))
                .Returns(BlockType.Study);
            
            _mockSpawnStrategy.Setup(x => x.StrategyName)
                .Returns("Test Strategy");

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel(); // Cancel before calling

            _mockMediator.Setup(x => x.Send(It.IsAny<PlaceBlockCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert - Should not throw despite cancellation
            await _handler.Handle(notification, cancellationTokenSource.Token);
        }

        [Fact]
        public async Task Handle_WithDifferentGridSizes_AdaptsCorrectly()
        {
            // Arrange
            var turn = BlockLife.Core.Domain.Turn.Turn.Create(1).IfFail(_ => throw new Exception());
            var notification = TurnStartNotification.Create(turn);
            var largeGridSize = new Vector2Int(10, 10);
            var selectedPosition = new Vector2Int(5, 5);

            _mockGridStateService.Setup(x => x.GetGridDimensions())
                .Returns(largeGridSize);

            SetupGridWithSpecificPositions(largeGridSize, new[] { selectedPosition });
            
            _mockSpawnStrategy.Setup(x => x.SelectSpawnPosition(It.IsAny<IEnumerable<Vector2Int>>(), largeGridSize))
                .Returns(Some(selectedPosition));
            
            _mockSpawnStrategy.Setup(x => x.StrategyName)
                .Returns("Test Strategy");

            _mockMediator.Setup(x => x.Send(It.IsAny<PlaceBlockCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FinSucc<LanguageExt.Unit>(LanguageExt.Unit.Default));

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert - Strategy should receive the larger grid size
            _mockSpawnStrategy.Verify(x => x.SelectSpawnPosition(
                It.IsAny<IEnumerable<Vector2Int>>(), largeGridSize
            ), Times.Once);
        }

        [Fact]
        public async Task GetEmptyPositions_WithPartiallyFilledGrid_ReturnsOnlyEmptyPositions()
        {
            // Arrange
            var turn = BlockLife.Core.Domain.Turn.Turn.Create(1).IfFail(_ => throw new Exception());
            var notification = TurnStartNotification.Create(turn);
            
            // Setup grid where only (1,1) and (3,3) are empty
            _mockGridStateService.Setup(x => x.IsPositionEmpty(new Vector2Int(1, 1)))
                .Returns(true);
            _mockGridStateService.Setup(x => x.IsPositionEmpty(new Vector2Int(3, 3)))
                .Returns(true);
            
            // All other positions are occupied
            for (int x = 0; x < _standardGridSize.X; x++)
            {
                for (int y = 0; y < _standardGridSize.Y; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (pos != new Vector2Int(1, 1) && pos != new Vector2Int(3, 3))
                    {
                        _mockGridStateService.Setup(x => x.IsPositionEmpty(pos))
                            .Returns(false);
                    }
                }
            }

            _mockSpawnStrategy.Setup(x => x.SelectSpawnPosition(It.IsAny<IEnumerable<Vector2Int>>(), _standardGridSize))
                .Returns(Some(new Vector2Int(1, 1)));
            
            _mockSpawnStrategy.Setup(x => x.StrategyName)
                .Returns("Test Strategy");

            _mockMediator.Setup(x => x.Send(It.IsAny<PlaceBlockCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FinSucc<LanguageExt.Unit>(LanguageExt.Unit.Default));

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert - Only empty positions should be passed to strategy
            _mockSpawnStrategy.Verify(x => x.SelectSpawnPosition(
                It.Is<IEnumerable<Vector2Int>>(positions => 
                    positions.Count() == 2 &&
                    positions.Contains(new Vector2Int(1, 1)) &&
                    positions.Contains(new Vector2Int(3, 3))
                ),
                _standardGridSize
            ), Times.Once);
        }

        private void SetupEmptyGrid(Vector2Int[] emptyPositions)
        {
            // Setup all positions as occupied by default
            for (int x = 0; x < _standardGridSize.X; x++)
            {
                for (int y = 0; y < _standardGridSize.Y; y++)
                {
                    _mockGridStateService.Setup(s => s.IsPositionEmpty(new Vector2Int(x, y)))
                        .Returns(false);
                }
            }

            // Setup specified positions as empty
            foreach (var pos in emptyPositions)
            {
                _mockGridStateService.Setup(s => s.IsPositionEmpty(pos))
                    .Returns(true);
            }
        }

        private void SetupFullGrid()
        {
            // Setup all positions as occupied
            for (int x = 0; x < _standardGridSize.X; x++)
            {
                for (int y = 0; y < _standardGridSize.Y; y++)
                {
                    _mockGridStateService.Setup(s => s.IsPositionEmpty(new Vector2Int(x, y)))
                        .Returns(false);
                }
            }
        }

        private void SetupGridWithSpecificPositions(Vector2Int gridSize, Vector2Int[] emptyPositions)
        {
            // Setup all positions as occupied by default
            for (int x = 0; x < gridSize.X; x++)
            {
                for (int y = 0; y < gridSize.Y; y++)
                {
                    _mockGridStateService.Setup(s => s.IsPositionEmpty(new Vector2Int(x, y)))
                        .Returns(false);
                }
            }

            // Setup specified positions as empty
            foreach (var pos in emptyPositions)
            {
                _mockGridStateService.Setup(s => s.IsPositionEmpty(pos))
                    .Returns(true);
            }
        }
    }
}