using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Tests.Builders;
using BlockLife.Core.Features.Block.Drag.Commands;
using BlockLife.Core.Features.Block.Drag.Services;
using BlockLife.Core.Infrastructure.Services;
using FluentAssertions;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BlockLife.Core.Tests.Features.Block.Drag
{
    /// <summary>
    /// Tests for drag-and-drop commands and handlers.
    /// Validates the complete drag lifecycle from start to completion/cancellation.
    /// </summary>
    public class DragCommandTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IMediator _mediator;
        private readonly IGridStateService _gridStateService;
        private readonly IDragStateService _dragStateService;
        private readonly ILogger _logger;

        public DragCommandTests()
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

            // Add drag services
            services.AddSingleton<IDragStateService, DragStateService>();
            services.AddTransient<StartDragCommandHandler>();
            services.AddTransient<CompleteDragCommandHandler>();
            services.AddTransient<CancelDragCommandHandler>();

            // Add move command handler (needed by CompleteDragCommandHandler)
            services.AddTransient<BlockLife.Core.Features.Block.Commands.MoveBlockCommandHandler>();

            // Add turn manager (required by TurnAdvancementAfterMoveHandler)
            services.AddSingleton<BlockLife.Core.Domain.Turn.ITurnManager, 
                BlockLife.Core.Features.Turn.Services.TurnManager>();

            // Add simulation manager (required by some handlers)
            services.AddSingleton<BlockLife.Core.Application.Simulation.ISimulationManager,
                BlockLife.Core.Application.Simulation.SimulationManager>();

            // Add validation rules
            services.AddTransient<BlockLife.Core.Features.Block.Placement.Rules.IPositionIsValidRule,
                BlockLife.Core.Features.Block.Placement.Rules.PositionIsValidRule>();
            services.AddTransient<BlockLife.Core.Features.Block.Placement.Rules.IPositionIsEmptyRule,
                BlockLife.Core.Features.Block.Placement.Rules.PositionIsEmptyRule>();
            services.AddTransient<BlockLife.Core.Features.Block.Placement.Rules.IBlockExistsRule,
                BlockLife.Core.Features.Block.Placement.Rules.BlockExistsRule>();

            _serviceProvider = services.BuildServiceProvider();
            _mediator = _serviceProvider.GetRequiredService<IMediator>();
            _gridStateService = _serviceProvider.GetRequiredService<IGridStateService>();
            _dragStateService = _serviceProvider.GetRequiredService<IDragStateService>();
        }

        [Fact]
        public async Task StartDrag_WithValidBlock_ShouldSucceed()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var position = new Vector2Int(5, 5);
            var block = new BlockBuilder()
                .WithId(blockId)
                .WithPosition(position)
                .WithType(BlockType.Basic)
                .Build();
            _gridStateService.PlaceBlock(block);

            // Act
            var command = StartDragCommand.Create(blockId, position);
            var result = await _mediator.Send(command);

            // Assert
            result.IsSucc.Should().BeTrue();
            _dragStateService.IsDragging.Should().BeTrue();
            _dragStateService.DraggedBlockId.Should().Be(blockId);
            _dragStateService.OriginalPosition.Should().Be(position);
        }

        [Fact]
        public async Task StartDrag_WithNonExistentBlock_ShouldFail()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var position = new Vector2Int(5, 5);

            // Act
            var command = StartDragCommand.Create(blockId, position);
            var result = await _mediator.Send(command);

            // Assert
            result.IsFail.Should().BeTrue();
            _dragStateService.IsDragging.Should().BeFalse();
        }

        [Fact]
        public async Task StartDrag_WhenAlreadyDragging_ShouldFail()
        {
            // Arrange
            var block1Id = Guid.NewGuid();
            var block2Id = Guid.NewGuid();
            var position1 = new Vector2Int(5, 5);
            var position2 = new Vector2Int(7, 7);

            var block1 = new BlockBuilder()
                .WithId(block1Id)
                .WithPosition(position1)
                .WithType(BlockType.Work)
                .Build();
            var block2 = new BlockBuilder()
                .WithId(block2Id)
                .WithPosition(position2)
                .WithType(BlockType.Study)
                .Build();
            _gridStateService.PlaceBlock(block1);
            _gridStateService.PlaceBlock(block2);

            // Start first drag
            var command1 = StartDragCommand.Create(block1Id, position1);
            await _mediator.Send(command1);

            // Act - Try to start second drag
            var command2 = StartDragCommand.Create(block2Id, position2);
            var result = await _mediator.Send(command2);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => "",
                Fail: error => error.ToString()
            ).Should().Be("DRAG_IN_PROGRESS");
            _dragStateService.DraggedBlockId.Should().Be(block1Id); // First drag still active
        }

        [Fact]
        public async Task CompleteDrag_ToValidPosition_ShouldMoveBlock()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var startPosition = new Vector2Int(5, 5);
            var dropPosition = new Vector2Int(7, 7);

            var block = new BlockBuilder()
                .WithId(blockId)
                .WithPosition(startPosition)
                .WithType(BlockType.Basic)
                .Build();
            _gridStateService.PlaceBlock(block);

            // Start drag
            var startCommand = StartDragCommand.Create(blockId, startPosition);
            await _mediator.Send(startCommand);

            // Act - Complete drag
            var completeCommand = CompleteDragCommand.Create(blockId, dropPosition);
            var result = await _mediator.Send(completeCommand);

            // Assert
            result.IsSucc.Should().BeTrue();
            _dragStateService.IsDragging.Should().BeFalse();

            // Block should be at new position
            var movedBlock = _gridStateService.GetBlockAt(dropPosition);
            movedBlock.IsSome.Should().BeTrue();
            movedBlock.Match(
                Some: b => b.Id,
                None: () => Guid.Empty
            ).Should().Be(blockId);

            // Old position should be empty
            var oldPositionBlock = _gridStateService.GetBlockAt(startPosition);
            oldPositionBlock.IsNone.Should().BeTrue();
        }

        [Fact]
        public async Task CompleteDrag_ToOccupiedPosition_WithinRange_ShouldSwapBlocks()
        {
            // Arrange
            var block1Id = Guid.NewGuid();
            var block2Id = Guid.NewGuid();
            var position1 = new Vector2Int(5, 5);
            var position2 = new Vector2Int(6, 6);  // Within range 3 (distance = 2)

            var block1 = new BlockBuilder()
                .WithId(block1Id)
                .WithPosition(position1)
                .WithType(BlockType.Work)
                .Build();
            var block2 = new BlockBuilder()
                .WithId(block2Id)
                .WithPosition(position2)
                .WithType(BlockType.Study)
                .Build();
            _gridStateService.PlaceBlock(block1);
            _gridStateService.PlaceBlock(block2);

            // Start drag
            var startCommand = StartDragCommand.Create(block1Id, position1);
            await _mediator.Send(startCommand);

            // Act - Try to drop on occupied position (should swap)
            var completeCommand = CompleteDragCommand.Create(block1Id, position2);
            var result = await _mediator.Send(completeCommand);

            // Assert
            result.IsSucc.Should().BeTrue("blocks within range should swap");
            _dragStateService.IsDragging.Should().BeFalse(); // Drag should be complete

            // Block1 should be at position2
            var block1Check = _gridStateService.GetBlockAt(position2);
            block1Check.IsSome.Should().BeTrue();
            block1Check.Match(
                Some: b => b.Id,
                None: () => Guid.Empty
            ).Should().Be(block1Id);

            // Block2 should be at position1
            var block2Check = _gridStateService.GetBlockAt(position1);
            block2Check.IsSome.Should().BeTrue();
            block2Check.Match(
                Some: b => b.Id,
                None: () => Guid.Empty
            ).Should().Be(block2Id);
        }

        [Fact]
        public async Task CompleteDrag_ToOccupiedPosition_OutOfRange_ShouldFail()
        {
            // Arrange
            var block1Id = Guid.NewGuid();
            var block2Id = Guid.NewGuid();
            var position1 = new Vector2Int(5, 5);
            var position2 = new Vector2Int(9, 9);  // Out of range (distance = 8)

            var block1 = new BlockBuilder()
                .WithId(block1Id)
                .WithPosition(position1)
                .WithType(BlockType.Work)
                .Build();
            var block2 = new BlockBuilder()
                .WithId(block2Id)
                .WithPosition(position2)
                .WithType(BlockType.Study)
                .Build();
            _gridStateService.PlaceBlock(block1);
            _gridStateService.PlaceBlock(block2);

            // Start drag
            var startCommand = StartDragCommand.Create(block1Id, position1);
            await _mediator.Send(startCommand);

            // Act - Try to drop on occupied position out of range
            var completeCommand = CompleteDragCommand.Create(block1Id, position2);
            var result = await _mediator.Send(completeCommand);

            // Assert
            result.IsFail.Should().BeTrue("target block cannot reach original position");
            _dragStateService.IsDragging.Should().BeFalse(); // Drag should be cancelled

            // Both blocks should still be at original positions
            var block1Check = _gridStateService.GetBlockAt(position1);
            block1Check.IsSome.Should().BeTrue();
            block1Check.Match(
                Some: b => b.Id,
                None: () => Guid.Empty
            ).Should().Be(block1Id);

            var block2Check = _gridStateService.GetBlockAt(position2);
            block2Check.IsSome.Should().BeTrue();
            block2Check.Match(
                Some: b => b.Id,
                None: () => Guid.Empty
            ).Should().Be(block2Id);
        }

        [Fact]
        public async Task CompleteDrag_SwapAtMaxRange_ShouldSucceed()
        {
            // Arrange
            var block1Id = Guid.NewGuid();
            var block2Id = Guid.NewGuid();
            var position1 = new Vector2Int(5, 5);
            var position2 = new Vector2Int(5, 8);  // Exactly at range 3 (distance = 3)

            var block1 = new BlockBuilder()
                .WithId(block1Id)
                .WithPosition(position1)
                .WithType(BlockType.Work)
                .Build();
            var block2 = new BlockBuilder()
                .WithId(block2Id)
                .WithPosition(position2)
                .WithType(BlockType.Study)
                .Build();
            _gridStateService.PlaceBlock(block1);
            _gridStateService.PlaceBlock(block2);

            // Start drag
            var startCommand = StartDragCommand.Create(block1Id, position1);
            await _mediator.Send(startCommand);

            // Act - Swap at exact max range
            var completeCommand = CompleteDragCommand.Create(block1Id, position2);
            var result = await _mediator.Send(completeCommand);

            // Assert
            result.IsSucc.Should().BeTrue("blocks at exact max range should swap");

            // Verify swap occurred
            var block1Check = _gridStateService.GetBlockAt(position2);
            block1Check.IsSome.Should().BeTrue();
            block1Check.Match(
                Some: b => b.Id,
                None: () => Guid.Empty
            ).Should().Be(block1Id);

            var block2Check = _gridStateService.GetBlockAt(position1);
            block2Check.IsSome.Should().BeTrue();
            block2Check.Match(
                Some: b => b.Id,
                None: () => Guid.Empty
            ).Should().Be(block2Id);
        }

        [Fact]
        public async Task CompleteDrag_ToSamePosition_ShouldSucceedWithoutMove()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var position = new Vector2Int(5, 5);

            var block = new BlockBuilder()
                .WithId(blockId)
                .WithPosition(position)
                .WithType(BlockType.Basic)
                .Build();
            _gridStateService.PlaceBlock(block);

            // Start drag
            var startCommand = StartDragCommand.Create(blockId, position);
            await _mediator.Send(startCommand);

            // Act - Drop at same position
            var completeCommand = CompleteDragCommand.Create(blockId, position);
            var result = await _mediator.Send(completeCommand);

            // Assert
            result.IsSucc.Should().BeTrue();
            _dragStateService.IsDragging.Should().BeFalse();

            // Block should still be at same position
            var blockCheck = _gridStateService.GetBlockAt(position);
            blockCheck.IsSome.Should().BeTrue();
            blockCheck.Match(
                Some: b => b.Id,
                None: () => Guid.Empty
            ).Should().Be(blockId);
        }

        [Fact]
        public async Task CancelDrag_WithActiveDrag_ShouldResetState()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var position = new Vector2Int(5, 5);

            var block = new BlockBuilder()
                .WithId(blockId)
                .WithPosition(position)
                .WithType(BlockType.Basic)
                .Build();
            _gridStateService.PlaceBlock(block);

            // Start drag
            var startCommand = StartDragCommand.Create(blockId, position);
            await _mediator.Send(startCommand);

            // Update preview position
            _dragStateService.UpdatePreviewPosition(new Vector2Int(7, 7));

            // Act - Cancel drag
            var cancelCommand = CancelDragCommand.Create(blockId);
            var result = await _mediator.Send(cancelCommand);

            // Assert
            result.IsSucc.Should().BeTrue();
            _dragStateService.IsDragging.Should().BeFalse();
            _dragStateService.DraggedBlockId.Should().Be(Guid.Empty);

            // Block should still be at original position
            var blockCheck = _gridStateService.GetBlockAt(position);
            blockCheck.IsSome.Should().BeTrue();
            blockCheck.Match(
                Some: b => b.Id,
                None: () => Guid.Empty
            ).Should().Be(blockId);
        }

        [Fact]
        public async Task CancelDrag_WithNoDrag_ShouldFail()
        {
            // Arrange
            var blockId = Guid.NewGuid();

            // Act - Cancel without active drag
            var cancelCommand = CancelDragCommand.Create(blockId);
            var result = await _mediator.Send(cancelCommand);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => "",
                Fail: error => error.ToString()
            ).Should().Be("NO_ACTIVE_DRAG");
        }

        [Fact]
        public void DragRange_Validation_ShouldCalculateCorrectly()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var originalPosition = new Vector2Int(5, 5);
            var block = new BlockBuilder()
                .WithId(blockId)
                .WithPosition(originalPosition)
                .WithType(BlockType.Basic)
                .Build();
            _gridStateService.PlaceBlock(block);

            _dragStateService.StartDrag(blockId, originalPosition);

            // Act & Assert - Test various positions
            // Within range (Manhattan distance <= 3)
            _dragStateService.IsWithinDragRange(new Vector2Int(5, 8), 3).Should().BeTrue();  // Distance = 3
            _dragStateService.IsWithinDragRange(new Vector2Int(7, 6), 3).Should().BeTrue();  // Distance = 3
            _dragStateService.IsWithinDragRange(new Vector2Int(6, 6), 3).Should().BeTrue();  // Distance = 2

            // Outside range (Manhattan distance > 3)
            _dragStateService.IsWithinDragRange(new Vector2Int(5, 9), 3).Should().BeFalse(); // Distance = 4
            _dragStateService.IsWithinDragRange(new Vector2Int(9, 5), 3).Should().BeFalse(); // Distance = 4
            _dragStateService.IsWithinDragRange(new Vector2Int(8, 8), 3).Should().BeFalse(); // Distance = 6
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}
