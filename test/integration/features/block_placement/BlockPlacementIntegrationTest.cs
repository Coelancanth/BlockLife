using Godot;
using GdUnit4;
using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlockLife.Tests.Integration.Features.BlockPlacement
{
    /// <summary>
    /// INTEGRATION TESTS: End-to-end block placement flow
    /// 
    /// These tests verify the complete flow from UI interaction through
    /// command handling, notification pipeline, and visual feedback.
    /// 
    /// Test Strategy:
    /// - Load actual scenes
    /// - Simulate user interactions
    /// - Verify both core state and visual state
    /// - Test notification pipeline integrity
    /// </summary>
    [TestSuite]
    public partial class BlockPlacementIntegrationTest
    {
        private SceneTree _sceneTree;
        private SceneRoot _sceneRoot;
        private Node _mainScene;
        private GridInteractionController _gridController;
        private BlockVisualizationController _visualController;
        private IServiceProvider _serviceProvider;

        [Before]
        public async Task Setup()
        {
            // Get the scene tree
            _sceneTree = Engine.GetMainLoop() as SceneTree;
            Assert.That(_sceneTree).IsNotNull();

            // Get SceneRoot singleton
            _sceneRoot = _sceneTree.Root.GetNode<SceneRoot>("/root/SceneRoot");
            Assert.That(_sceneRoot).IsNotNull();
            
            // Get the service provider
            _serviceProvider = _sceneRoot.ServiceProvider;
            Assert.That(_serviceProvider).IsNotNull();

            // Load the main scene
            var mainScenePath = "res://godot_project/scenes/Main/main.tscn";
            var mainSceneResource = GD.Load<PackedScene>(mainScenePath);
            Assert.That(mainSceneResource).IsNotNull();
            
            _mainScene = mainSceneResource.Instantiate();
            _sceneTree.Root.AddChild(_mainScene);
            
            // Wait for scene to be ready
            await _sceneTree.ProcessFrame();
            
            // Get the controllers
            _gridController = _mainScene.GetNode<GridInteractionController>("GridView/GridInteractionController");
            Assert.That(_gridController).IsNotNull();
            
            _visualController = _mainScene.GetNode<BlockVisualizationController>("GridView/BlockVisualizationController");
            Assert.That(_visualController).IsNotNull();
        }

        [After]
        public async Task Cleanup()
        {
            // Clean up the scene
            if (_mainScene != null && IsInstanceValid(_mainScene))
            {
                _mainScene.QueueFree();
                await _sceneTree.ProcessFrame();
            }
        }

        /// <summary>
        /// INTEGRATION TEST: Click on grid places block
        /// 
        /// This test verifies the complete flow:
        /// 1. User clicks on grid
        /// 2. GridInteractionController detects click
        /// 3. Presenter sends PlaceBlockCommand
        /// 4. Command updates core state
        /// 5. Notification is published
        /// 6. View receives notification
        /// 7. Visual block appears
        /// </summary>
        [TestCase]
        public async Task ClickOnGrid_PlacesBlock_CompleteFlow()
        {
            // Arrange
            var clickPosition = new Vector2Int(5, 5);
            var worldPosition = new Vector2(clickPosition.X * 64 + 32, clickPosition.Y * 64 + 32);
            
            // Get the grid state service
            var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
            
            // Verify grid is initially empty
            var initialBlock = gridState.GetBlockAt(clickPosition);
            Assert.That(initialBlock.IsNone).IsTrue();

            // Act - Simulate mouse click
            var clickEvent = new InputEventMouseButton
            {
                Position = worldPosition,
                ButtonIndex = MouseButton.Left,
                Pressed = true
            };
            
            _gridController._GuiInput(clickEvent);
            
            // Wait for async operations to complete
            await _sceneTree.ProcessFrame();
            await Task.Delay(100); // Allow time for async command processing
            await _sceneTree.ProcessFrame();

            // Assert - Verify core state updated
            var placedBlock = gridState.GetBlockAt(clickPosition);
            Assert.That(placedBlock.IsSome).IsTrue();
            
            // Assert - Verify visual state updated
            var blockContainer = _visualController.GetNode("BlockContainer");
            Assert.That(blockContainer).IsNotNull();
            Assert.That(blockContainer.GetChildCount()).IsEqual(1);
            
            // Verify the visual block is at correct position
            var visualBlock = blockContainer.GetChild(0) as Node2D;
            Assert.That(visualBlock).IsNotNull();
            Assert.That(visualBlock.Position).IsEqual(new Vector2(clickPosition.X * 64, clickPosition.Y * 64));
        }

        /// <summary>
        /// REGRESSION TEST: Multiple clicks on same position don't cause errors
        /// 
        /// This test ensures the fix for duplicate notification handling works
        /// in the full integration context.
        /// </summary>
        [TestCase]
        public async Task MultipleClicksSamePosition_NoErrors()
        {
            // Arrange
            var clickPosition = new Vector2Int(3, 3);
            var worldPosition = new Vector2(clickPosition.X * 64 + 32, clickPosition.Y * 64 + 32);
            
            var clickEvent = new InputEventMouseButton
            {
                Position = worldPosition,
                ButtonIndex = MouseButton.Left,
                Pressed = true
            };

            // Act - Click the same position multiple times
            for (int i = 0; i < 3; i++)
            {
                _gridController._GuiInput(clickEvent);
                await _sceneTree.ProcessFrame();
                await Task.Delay(50);
            }

            // Assert - Should only have one block
            var blockContainer = _visualController.GetNode("BlockContainer");
            Assert.That(blockContainer.GetChildCount()).IsEqual(1);
            
            // Verify no duplicate blocks in core state
            var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
            var block = gridState.GetBlockAt(clickPosition);
            Assert.That(block.IsSome).IsTrue();
        }

        /// <summary>
        /// INTEGRATION TEST: Multiple unique blocks placement
        /// 
        /// Tests that multiple blocks can be placed at different positions
        /// and all appear correctly.
        /// </summary>
        [TestCase]
        public async Task MultipleDifferentPositions_AllBlocksPlaced()
        {
            // Arrange
            var positions = new[]
            {
                new Vector2Int(1, 1),
                new Vector2Int(3, 3),
                new Vector2Int(5, 5),
                new Vector2Int(7, 7)
            };

            // Act - Click multiple positions
            foreach (var pos in positions)
            {
                var worldPos = new Vector2(pos.X * 64 + 32, pos.Y * 64 + 32);
                var clickEvent = new InputEventMouseButton
                {
                    Position = worldPos,
                    ButtonIndex = MouseButton.Left,
                    Pressed = true
                };
                
                _gridController._GuiInput(clickEvent);
                await _sceneTree.ProcessFrame();
                await Task.Delay(50);
            }

            // Assert - All blocks should be placed
            var blockContainer = _visualController.GetNode("BlockContainer");
            Assert.That(blockContainer.GetChildCount()).IsEqual(positions.Length);
            
            // Verify each position has a block in core state
            var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
            foreach (var pos in positions)
            {
                var block = gridState.GetBlockAt(pos);
                Assert.That(block.IsSome).IsTrue();
            }
        }

        /// <summary>
        /// INTEGRATION TEST: Invalid position handling
        /// 
        /// Tests that clicking outside the grid bounds doesn't place a block.
        /// </summary>
        [TestCase]
        public async Task ClickOutsideGrid_NoBlockPlaced()
        {
            // Arrange
            var invalidPosition = new Vector2Int(100, 100); // Way outside 10x10 grid
            var worldPosition = new Vector2(invalidPosition.X * 64 + 32, invalidPosition.Y * 64 + 32);
            
            var clickEvent = new InputEventMouseButton
            {
                Position = worldPosition,
                ButtonIndex = MouseButton.Left,
                Pressed = true
            };

            // Act
            _gridController._GuiInput(clickEvent);
            await _sceneTree.ProcessFrame();
            await Task.Delay(100);

            // Assert - No blocks should be placed
            var blockContainer = _visualController.GetNode("BlockContainer");
            Assert.That(blockContainer.GetChildCount()).IsEqual(0);
        }

        /// <summary>
        /// PERFORMANCE TEST: Rapid block placement
        /// 
        /// Tests that the system can handle rapid clicking without issues.
        /// </summary>
        [TestCase]
        public async Task RapidClicking_AllBlocksProcessedCorrectly()
        {
            // Arrange
            var positions = new Vector2Int[25];
            int index = 0;
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    positions[index++] = new Vector2Int(x, y);
                }
            }

            // Act - Rapid fire clicks
            foreach (var pos in positions)
            {
                var worldPos = new Vector2(pos.X * 64 + 32, pos.Y * 64 + 32);
                var clickEvent = new InputEventMouseButton
                {
                    Position = worldPos,
                    ButtonIndex = MouseButton.Left,
                    Pressed = true
                };
                
                _gridController._GuiInput(clickEvent);
                // Minimal delay to simulate rapid clicking
                await _sceneTree.ProcessFrame();
            }
            
            // Wait for all processing to complete
            await Task.Delay(500);
            await _sceneTree.ProcessFrame();

            // Assert - All blocks should be placed
            var blockContainer = _visualController.GetNode("BlockContainer");
            Assert.That(blockContainer.GetChildCount()).IsEqual(25);
            
            // Verify core state
            var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
            foreach (var pos in positions)
            {
                var block = gridState.GetBlockAt(pos);
                Assert.That(block.IsSome).IsTrue();
            }
        }
    }
}