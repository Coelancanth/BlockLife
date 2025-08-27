using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Drag.Commands;
using BlockLife.Core.Features.Block.Drag.Services;
using BlockLife.Core.Features.Block.Move;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Core.Tests.Builders;
using FluentAssertions;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;
using System;
using System.Threading.Tasks;

namespace BlockLife.Core.Tests.Features.Block.Drag
{
    /// <summary>
    /// Tests for Phase 2: Drag Range Limits functionality
    /// </summary>
    public class DragRangeValidationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IMediator _mediator;
        private readonly IGridStateService _gridStateService;
        private readonly IDragStateService _dragStateService;
        private readonly ILogger _logger;

        public DragRangeValidationTests()
        {
            var services = new ServiceCollection();

            // Setup test logger
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            services.AddSingleton(_logger);
            services.AddLogging(builder => builder.AddSerilog(_logger));

            // Add MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(StartDragCommand).Assembly));

            // Add required services
            services.AddSingleton<GridStateService>();
            services.AddSingleton<IGridStateService>(p => p.GetRequiredService<GridStateService>());
            services.AddSingleton<IBlockRepository>(p => p.GetRequiredService<GridStateService>());
            services.AddSingleton<IDragStateService, DragStateService>();

            // Add turn manager (required by TurnAdvancementAfterMoveHandler)
            services.AddSingleton<BlockLife.Core.Domain.Turn.ITurnManager, 
                BlockLife.Core.Features.Turn.Services.TurnManager>();

            _serviceProvider = services.BuildServiceProvider();
            _mediator = _serviceProvider.GetRequiredService<IMediator>();
            _gridStateService = _serviceProvider.GetRequiredService<IGridStateService>();
            _dragStateService = _serviceProvider.GetRequiredService<IDragStateService>();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        [Fact]
        public async Task DragRangeValidation_WithinRange_ShouldAllowDrop()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var startPosition = new Vector2Int(5, 5);
            var dropPosition = new Vector2Int(7, 7); // 2 cells away, within default range of 3

            // Place block
            var block = new BlockBuilder()
                .WithId(blockId)
                .WithPosition(startPosition)
                .WithType(BlockType.Basic)
                .Build();
            _gridStateService.PlaceBlock(block);

            // Start drag
            var startResult = await _mediator.Send(StartDragCommand.Create(blockId, startPosition));
            startResult.IsSucc.Should().BeTrue();

            // Act - Complete drag within range
            var completeResult = await _mediator.Send(CompleteDragCommand.Create(blockId, dropPosition));

            // Assert
            completeResult.IsSucc.Should().BeTrue();
            var movedBlock = _gridStateService.GetBlockAt(dropPosition);
            movedBlock.IsSome.Should().BeTrue();
            movedBlock.IfSome(b => b.Id.Should().Be(blockId));
        }

        [Fact]
        public void IsWithinDragRange_ExactlyAtRangeLimit_ShouldReturnTrue()
        {
            // Arrange
            var startPosition = new Vector2Int(5, 5);
            _dragStateService.StartDrag(Guid.NewGuid(), startPosition);

            // Act - Test positions exactly 3 Manhattan distance away
            var straightUp = new Vector2Int(5, 2);      // 3 cells up (Manhattan: 3)
            var straightRight = new Vector2Int(8, 5);   // 3 cells right (Manhattan: 3)
            var straightDown = new Vector2Int(5, 8);    // 3 cells down (Manhattan: 3)
            var straightLeft = new Vector2Int(2, 5);    // 3 cells left (Manhattan: 3)
            var diagonal1 = new Vector2Int(6, 7);       // 1 right, 2 down (Manhattan: 3)
            var diagonal2 = new Vector2Int(7, 6);       // 2 right, 1 down (Manhattan: 3)

            // Assert
            _dragStateService.IsWithinDragRange(straightUp, 3).Should().BeTrue();
            _dragStateService.IsWithinDragRange(straightRight, 3).Should().BeTrue();
            _dragStateService.IsWithinDragRange(straightDown, 3).Should().BeTrue();
            _dragStateService.IsWithinDragRange(straightLeft, 3).Should().BeTrue();
            _dragStateService.IsWithinDragRange(diagonal1, 3).Should().BeTrue();
            _dragStateService.IsWithinDragRange(diagonal2, 3).Should().BeTrue();
        }

        [Fact]
        public void IsWithinDragRange_BeyondRangeLimit_ShouldReturnFalse()
        {
            // Arrange
            var startPosition = new Vector2Int(5, 5);
            _dragStateService.StartDrag(Guid.NewGuid(), startPosition);

            // Act - Test positions beyond 3 cells away
            var farPosition1 = new Vector2Int(9, 5);  // 4 cells horizontally
            var farPosition2 = new Vector2Int(5, 9);  // 4 cells vertically
            var farPosition3 = new Vector2Int(9, 9);  // 4 cells diagonally
            var farPosition4 = new Vector2Int(0, 0);  // 5 cells diagonally

            // Assert
            _dragStateService.IsWithinDragRange(farPosition1, 3).Should().BeFalse();
            _dragStateService.IsWithinDragRange(farPosition2, 3).Should().BeFalse();
            _dragStateService.IsWithinDragRange(farPosition3, 3).Should().BeFalse();
            _dragStateService.IsWithinDragRange(farPosition4, 3).Should().BeFalse();
        }

        [Fact]
        public void IsWithinDragRange_CustomRange_ShouldRespectParameter()
        {
            // Arrange
            var startPosition = new Vector2Int(10, 10);
            _dragStateService.StartDrag(Guid.NewGuid(), startPosition);

            // Act & Assert - Test with custom range of 5
            var position4Away = new Vector2Int(14, 10); // 4 cells away
            var position5Away = new Vector2Int(15, 10); // 5 cells away
            var position6Away = new Vector2Int(16, 10); // 6 cells away

            _dragStateService.IsWithinDragRange(position4Away, 5).Should().BeTrue();
            _dragStateService.IsWithinDragRange(position5Away, 5).Should().BeTrue();
            _dragStateService.IsWithinDragRange(position6Away, 5).Should().BeFalse();

            // Test with custom range of 1 (adjacent cells only)
            var adjacent = new Vector2Int(11, 10);      // 1 cell away
            var twoAway = new Vector2Int(12, 10);       // 2 cells away

            _dragStateService.IsWithinDragRange(adjacent, 1).Should().BeTrue();
            _dragStateService.IsWithinDragRange(twoAway, 1).Should().BeFalse();
        }

        [Fact]
        public void IsWithinDragRange_SamePosition_ShouldReturnTrue()
        {
            // Arrange
            var startPosition = new Vector2Int(5, 5);
            _dragStateService.StartDrag(Guid.NewGuid(), startPosition);

            // Act & Assert - Dropping at the same position is always valid
            _dragStateService.IsWithinDragRange(startPosition, 3).Should().BeTrue();
            _dragStateService.IsWithinDragRange(startPosition, 1).Should().BeTrue();
            _dragStateService.IsWithinDragRange(startPosition, 0).Should().BeTrue();
        }

        [Fact]
        public void IsWithinDragRange_WithoutActiveDrag_ShouldReturnFalse()
        {
            // Arrange - No drag started

            // Act & Assert
            var anyPosition = new Vector2Int(5, 5);
            _dragStateService.IsWithinDragRange(anyPosition, 3).Should().BeFalse();
        }

        [Theory]
        [InlineData(5, 5, 8, 5, 3, true)]   // 3 cells horizontally - Manhattan: 3 - within range
        [InlineData(5, 5, 9, 5, 3, false)]  // 4 cells horizontally - Manhattan: 4 - outside range
        [InlineData(5, 5, 5, 8, 3, true)]   // 3 cells vertically - Manhattan: 3 - within range
        [InlineData(5, 5, 5, 9, 3, false)]  // 4 cells vertically - Manhattan: 4 - outside range
        [InlineData(5, 5, 6, 6, 3, true)]   // 1 cell diagonally - Manhattan: 2 - within range
        [InlineData(5, 5, 7, 6, 3, true)]   // 2 right, 1 up - Manhattan: 3 - within range
        [InlineData(5, 5, 7, 7, 3, false)]  // 2 cells diagonally - Manhattan: 4 - outside range
        [InlineData(5, 5, 8, 8, 3, false)]  // 3 cells diagonally - Manhattan: 6 - outside range
        public void IsWithinDragRange_VariousPositions_ShouldValidateCorrectly(
            int startX, int startY,
            int targetX, int targetY,
            int maxRange, bool expectedResult)
        {
            // Arrange
            var startPosition = new Vector2Int(startX, startY);
            var targetPosition = new Vector2Int(targetX, targetY);
            _dragStateService.StartDrag(Guid.NewGuid(), startPosition);

            // Act
            var result = _dragStateService.IsWithinDragRange(targetPosition, maxRange);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public void VisualAndLogicConsistency_UseManhattanDistance_NotChebyshev()
        {
            // This test documents that we use Manhattan distance (diamond shape)
            // not Chebyshev distance (square shape) for drag range validation.
            // This prevents regression of the visual/logic mismatch bug (TD_013).

            // Arrange
            var center = new Vector2Int(5, 5);
            var range = 3;
            _dragStateService.StartDrag(Guid.NewGuid(), center);

            // Act & Assert - Corner positions at max range
            // These positions are at Chebyshev distance 3 (would be valid for square)
            // but Manhattan distance 6 (invalid for diamond with range 3)

            // Top-right corner: (8, 2) -> Manhattan = |8-5| + |2-5| = 3 + 3 = 6
            _dragStateService.IsWithinDragRange(new Vector2Int(8, 2), range)
                .Should().BeFalse("corner at max range should be outside Manhattan distance");

            // Bottom-left corner: (2, 8) -> Manhattan = |2-5| + |8-5| = 3 + 3 = 6  
            _dragStateService.IsWithinDragRange(new Vector2Int(2, 8), range)
                .Should().BeFalse("corner at max range should be outside Manhattan distance");

            // Edge positions at max range should be valid
            // Right edge: (8, 5) -> Manhattan = |8-5| + |5-5| = 3 + 0 = 3
            _dragStateService.IsWithinDragRange(new Vector2Int(8, 5), range)
                .Should().BeTrue("edge position should be within Manhattan distance");

            // Top edge: (5, 2) -> Manhattan = |5-5| + |2-5| = 0 + 3 = 3
            _dragStateService.IsWithinDragRange(new Vector2Int(5, 2), range)
                .Should().BeTrue("edge position should be within Manhattan distance");
        }
    }
}
