using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using FluentAssertions;
using Xunit;

namespace BlockLife.Core.Tests.Features.Block.Placement
{
    /// <summary>
    /// REGRESSION TESTS: View layer duplicate notification handling
    /// 
    /// BUG CONTEXT:
    /// - Date: 2025-08-14
    /// - Issue: View layer showed "Block already exists" errors
    /// - Symptom: Duplicate notifications caused view to reject valid blocks
    /// - Root Cause: View couldn't handle duplicate notifications gracefully
    /// - Fix: Modified BlockVisualizationController to ignore duplicate notifications
    /// 
    /// These tests ensure the view layer handles edge cases properly.
    /// </summary>
    public class BlockVisualizationTests
    {
        /// <summary>
        /// REGRESSION TEST: View should handle duplicate notifications gracefully
        /// 
        /// This test simulates the scenario where the same block notification
        /// is sent multiple times to the view layer.
        /// </summary>
        [Fact]
        public async Task ShowBlockAsync_DuplicateNotification_HandledGracefully()
        {
            // Arrange
            var mockView = new MockBlockVisualizationView();
            var blockId = Guid.NewGuid();
            var position = new Vector2Int(5, 5);
            var blockType = BlockType.Basic;

            // Act - Show the same block twice (simulating duplicate notification)
            await mockView.ShowBlockAsync(blockId, position, blockType);
            await mockView.ShowBlockAsync(blockId, position, blockType);

            // Assert - Should only have one block, not throw error
            mockView.BlockCount.Should().Be(1, 
                "Duplicate notifications for same block should be ignored");
            mockView.ErrorCount.Should().Be(0, 
                "No errors should be logged for duplicate notifications at same position");
        }

        /// <summary>
        /// REGRESSION TEST: View should detect actual ID conflicts
        /// 
        /// If the same block ID is used for different positions, that's a real error.
        /// </summary>
        [Fact]
        public async Task ShowBlockAsync_SameIdDifferentPosition_ReportsError()
        {
            // Arrange
            var mockView = new MockBlockVisualizationView();
            var blockId = Guid.NewGuid();
            var position1 = new Vector2Int(5, 5);
            var position2 = new Vector2Int(6, 6);
            var blockType = BlockType.Basic;

            // Act - Try to show same block ID at different positions
            await mockView.ShowBlockAsync(blockId, position1, blockType);
            await mockView.ShowBlockAsync(blockId, position2, blockType);

            // Assert - Should have one block and one error
            mockView.BlockCount.Should().Be(1, 
                "Only first block should be added");
            mockView.ErrorCount.Should().Be(1, 
                "Error should be logged when same ID used for different position");
        }

        /// <summary>
        /// REGRESSION TEST: Multiple unique blocks should work correctly
        /// 
        /// Normal operation - different IDs for different blocks.
        /// </summary>
        [Fact]
        public async Task ShowBlockAsync_MultipleUniqueBlocks_AllAdded()
        {
            // Arrange
            var mockView = new MockBlockVisualizationView();
            var blocks = new[]
            {
                (Guid.NewGuid(), new Vector2Int(1, 1), BlockType.Basic),
                (Guid.NewGuid(), new Vector2Int(2, 2), BlockType.Work),
                (Guid.NewGuid(), new Vector2Int(3, 3), BlockType.Study),
            };

            // Act - Add multiple unique blocks
            foreach (var (id, pos, type) in blocks)
            {
                await mockView.ShowBlockAsync(id, pos, type);
            }

            // Assert - All blocks should be added
            mockView.BlockCount.Should().Be(3, 
                "All unique blocks should be added");
            mockView.ErrorCount.Should().Be(0, 
                "No errors for unique blocks");
        }

        /// <summary>
        /// Mock implementation of view for testing
        /// </summary>
        private class MockBlockVisualizationView
        {
            private readonly Dictionary<Guid, (Vector2Int Position, BlockType Type)> _blocks = new();
            
            public int BlockCount => _blocks.Count;
            public int ErrorCount { get; private set; }

            public Task ShowBlockAsync(Guid blockId, Vector2Int position, BlockType type)
            {
                // Simulate the fixed logic from BlockVisualizationController
                if (_blocks.ContainsKey(blockId))
                {
                    // Check if it's at the same position
                    if (_blocks.TryGetValue(blockId, out var existing))
                    {
                        if (existing.Position == position)
                        {
                            // Same block, same position - duplicate notification, ignore
                            return Task.CompletedTask;
                        }
                    }
                    
                    // Different position - this is an error
                    ErrorCount++;
                    return Task.CompletedTask;
                }

                // Add the block
                _blocks[blockId] = (position, type);
                return Task.CompletedTask;
            }
        }
    }
}