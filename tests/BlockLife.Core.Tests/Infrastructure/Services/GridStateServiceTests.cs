using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Core.Tests.Builders;
using FluentAssertions;
using System;
using Xunit;

namespace BlockLife.Core.Tests.Infrastructure.Services
{
    /// <summary>
    /// Pillar 1: Testing Authoritative Write Model (GridStateService)
    /// These tests verify the absolute correctness of the model's internal state transition logic.
    /// Architectural Validation: Rule #1 (Single Authoritative Write Model).
    /// </summary>
    public class GridStateServiceTests
    {
        [Fact]
        public void PlaceBlock_WhenPositionIsEmpty_AddsBlockAndReturnsSuccess()
        {
            // Arrange
            var gridService = new GridStateService(5, 5);
            var block = new BlockBuilder()
                .WithType(BlockType.Work)
                .WithPosition(2, 2)
                .Build();

            // Act
            var result = gridService.PlaceBlock(block);

            // Assert
            result.IsSucc.Should().BeTrue();
            gridService.GetBlockAt(new Vector2Int(2, 2)).IsSome.Should().BeTrue();
            gridService.GetBlockAt(new Vector2Int(2, 2))
                .IfSome(b => b.Id.Should().Be(block.Id));
            gridService.GetAllBlocks().Should().HaveCount(1);
        }

        [Fact]
        public void PlaceBlock_WhenPositionIsOccupied_DoesNotAddBlockAndReturnsFail()
        {
            // Arrange
            var gridService = new GridStateServiceBuilder()
                .WithBlock(BlockType.Study, 2, 2)
                .Build();
            var newBlock = new BlockBuilder()
                .WithType(BlockType.Work)
                .WithPosition(2, 2)
                .Build();

            // Act
            var result = gridService.PlaceBlock(newBlock);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("is already occupied")
            );
            gridService.GetBlockAt(new Vector2Int(2, 2))
                .IfSome(b => b.Type.Should().Be(BlockType.Study)); // Still the original block
            gridService.GetAllBlocks().Should().HaveCount(1);
        }

        [Fact]
        public void PlaceBlock_WhenPositionIsOutOfBounds_ReturnsFail()
        {
            // Arrange
            var gridService = new GridStateService(5, 5);
            var block = new BlockBuilder()
                .WithType(BlockType.Health)
                .WithPosition(10, 10) // Out of bounds
                .Build();

            // Act
            var result = gridService.PlaceBlock(block);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("is outside grid bounds")
            );
            gridService.GetAllBlocks().Should().BeEmpty();
        }

        [Fact]
        public void RemoveBlock_WhenBlockExists_RemovesAndReturnsBlock()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var gridService = new GridStateServiceBuilder()
                .WithBlock(blockId, BlockType.Creativity, new Vector2Int(3, 3))
                .Build();

            // Act
            var result = gridService.RemoveBlock(blockId);

            // Assert
            result.IsSucc.Should().BeTrue();
            gridService.GetBlockAt(new Vector2Int(3, 3)).IsNone.Should().BeTrue();
            gridService.GetAllBlocks().Should().BeEmpty();
        }

        [Fact]
        public void RemoveBlock_WhenBlockDoesNotExist_ReturnsFail()
        {
            // Arrange
            var gridService = new GridStateService(5, 5);
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = gridService.RemoveBlock(nonExistentId);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("No block found")
            );
        }

        [Fact]
        public void RemoveBlockByPosition_WhenBlockExists_RemovesSuccessfully()
        {
            // Arrange
            var position = new Vector2Int(2, 2);
            var gridService = new GridStateServiceBuilder()
                .WithBlock(BlockType.Work, 2, 2)
                .Build();

            // Act
            var result = gridService.RemoveBlock(position);

            // Assert
            result.IsSucc.Should().BeTrue();
            gridService.GetBlockAt(position).IsNone.Should().BeTrue();
            gridService.GetAllBlocks().Should().BeEmpty();
        }

        [Fact]
        public void RemoveBlockByPosition_WhenNoBlockAtPosition_ReturnsFail()
        {
            // Arrange
            var gridService = new GridStateService(5, 5);
            var position = new Vector2Int(2, 2);

            // Act
            var result = gridService.RemoveBlock(position);

            // Assert
            result.IsFail.Should().BeTrue();
            result.Match(
                Succ: _ => { },
                Fail: error => error.Message.Should().Contain("No block found")
            );
        }

        [Fact]
        public void IsPositionOccupied_WhenPositionHasBlock_ReturnsTrue()
        {
            // Arrange
            var gridService = new GridStateServiceBuilder()
                .WithBlock(BlockType.Work, 2, 2)
                .Build();

            // Act
            var result = gridService.IsPositionOccupied(new Vector2Int(2, 2));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsPositionOccupied_WhenPositionIsEmpty_ReturnsFalse()
        {
            // Arrange
            var gridService = new GridStateService(5, 5);

            // Act
            var result = gridService.IsPositionOccupied(new Vector2Int(2, 2));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void MoveBlock_WhenTargetIsEmpty_MovesBlockSuccessfully()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var gridService = new GridStateServiceBuilder()
                .WithBlock(blockId, BlockType.Fun, new Vector2Int(1, 1))
                .Build();

            // Act
            var result = gridService.MoveBlock(blockId, new Vector2Int(3, 3));

            // Assert
            result.IsSucc.Should().BeTrue();
            result.IfSucc(block =>
            {
                block.Position.Should().Be(new Vector2Int(3, 3));
                block.LastModifiedAt.Should().BeAfter(block.CreatedAt);
            });
            gridService.GetBlockAt(new Vector2Int(1, 1)).IsNone.Should().BeTrue();
            gridService.GetBlockAt(new Vector2Int(3, 3)).IsSome.Should().BeTrue();
            gridService.GetAllBlocks().Should().HaveCount(1);
        }

        [Fact]
        public void MoveBlock_WhenTargetIsOccupied_FailsAndLeavesOriginalIntact()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var gridService = new GridStateServiceBuilder()
                .WithBlock(blockId, BlockType.Work, new Vector2Int(1, 1))
                .WithBlock(BlockType.Study, new Vector2Int(2, 2))
                .Build();

            // Act
            var result = gridService.MoveBlock(blockId, new Vector2Int(2, 2));

            // Assert
            result.IsFail.Should().BeTrue();
            gridService.GetBlockAt(new Vector2Int(1, 1))
                .IfSome(b => b.Id.Should().Be(blockId)); // Block still at original position
            gridService.GetBlockAt(new Vector2Int(2, 2))
                .IfSome(b => b.Type.Should().Be(BlockType.Study)); // Target still occupied
            gridService.GetAllBlocks().Should().HaveCount(2);
        }

        [Fact]
        public void GetAdjacentBlocks_ReturnsCorrectOrthogonalBlocks()
        {
            // Arrange
            var gridService = new GridStateServiceBuilder()
                .WithBlock(BlockType.Work, 2, 2)    // Center
                .WithBlock(BlockType.Study, 2, 1)   // Below
                .WithBlock(BlockType.Health, 2, 3)  // Above
                .WithBlock(BlockType.Fun, 1, 2)     // Left
                .WithBlock(BlockType.Creativity, 3, 2) // Right
                .WithBlock(BlockType.Relationship, 3, 3) // Diagonal (should not be included)
                .Build();

            // Act
            var adjacentBlocks = gridService.GetAdjacentBlocks(new Vector2Int(2, 2));

            // Assert
            adjacentBlocks.Should().HaveCount(4);
            adjacentBlocks.Should().Contain(b => b.Type == BlockType.Study);
            adjacentBlocks.Should().Contain(b => b.Type == BlockType.Health);
            adjacentBlocks.Should().Contain(b => b.Type == BlockType.Fun);
            adjacentBlocks.Should().Contain(b => b.Type == BlockType.Creativity);
            adjacentBlocks.Should().NotContain(b => b.Type == BlockType.Relationship);
        }

        [Fact]
        public void ClearGrid_RemovesAllBlocks()
        {
            // Arrange
            var gridService = new GridStateServiceBuilder()
                .WithBlock(BlockType.Work, 1, 1)
                .WithBlock(BlockType.Study, 2, 2)
                .WithBlock(BlockType.Health, 3, 3)
                .Build();

            // Act
            gridService.ClearGrid();

            // Assert
            gridService.GetAllBlocks().Should().BeEmpty();
            gridService.GetBlockAt(new Vector2Int(1, 1)).IsNone.Should().BeTrue();
            gridService.GetBlockAt(new Vector2Int(2, 2)).IsNone.Should().BeTrue();
            gridService.GetBlockAt(new Vector2Int(3, 3)).IsNone.Should().BeTrue();
        }

        [Fact]
        public void IsValidPosition_CorrectlyValidatesBounds()
        {
            // Arrange
            var gridService = new GridStateService(5, 5);

            // Act & Assert
            gridService.IsValidPosition(new Vector2Int(0, 0)).Should().BeTrue();
            gridService.IsValidPosition(new Vector2Int(4, 4)).Should().BeTrue();
            gridService.IsValidPosition(new Vector2Int(2, 2)).Should().BeTrue();
            gridService.IsValidPosition(new Vector2Int(-1, 0)).Should().BeFalse();
            gridService.IsValidPosition(new Vector2Int(0, -1)).Should().BeFalse();
            gridService.IsValidPosition(new Vector2Int(5, 0)).Should().BeFalse();
            gridService.IsValidPosition(new Vector2Int(0, 5)).Should().BeFalse();
        }
    }
}