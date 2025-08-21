using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.godot_project.features.block.placement;
using BlockLife.godot_project.scenes.Main;
using FluentAssertions;
using GdUnit4;
using Godot;
using Microsoft.Extensions.DependencyInjection;

namespace BlockLife.test.integration.features.block_placement
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
    public partial class BlockPlacementIntegrationTest : Node
    {
        private Node? _testScene;
        private GridInteractionController? _gridController;
        private BlockVisualizationController? _visualController;
        private IServiceProvider? _serviceProvider;
        private SceneTree? _sceneTree;

        [Before]
        public async Task Setup()
        {
            // Get the scene tree from the test node itself
            _sceneTree = GetTree();
            _sceneTree.Should().NotBeNull("scene tree must be available");

            // Get SceneRoot autoload - should be available in Godot context
            var sceneRoot = _sceneTree!.Root.GetNodeOrNull<SceneRoot>("/root/SceneRoot");

            if (sceneRoot == null)
            {
                // If SceneRoot doesn't exist, we're not in a proper Godot context
                // This test should be run from within Godot editor
                GD.PrintErr("SceneRoot not found - test must be run from Godot editor with SceneRoot autoload");
                return;
            }

            // Get service provider
            var serviceProviderField = typeof(SceneRoot).GetField("_serviceProvider",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            _serviceProvider = serviceProviderField?.GetValue(sceneRoot) as IServiceProvider;
            _serviceProvider.Should().NotBeNull("service provider must be initialized");

            // Create test scene as a child of this test node
            _testScene = await CreateTestScene();
            _testScene.Should().NotBeNull("test scene must be created");

            // Get controllers
            _gridController = _testScene!.GetNode<GridInteractionController>("GridView/GridInteractionController");
            _visualController = _testScene.GetNode<BlockVisualizationController>("GridView/BlockVisualizationController");

            _gridController.Should().NotBeNull("grid controller must exist in scene");
            _visualController.Should().NotBeNull("visualization controller must exist in scene");
        }

        /// <summary>
        /// Creates a test scene with proper structure (COPIED from SimpleSceneTest)
        /// </summary>
        private async Task<Node> CreateTestScene()
        {
            // Create scene programmatically (same as SimpleSceneTest)
            var root = new Node2D();
            root.Name = "TestRoot";
            AddChild(root);

            // Create GridView structure
            var gridView = new GridView();
            gridView.Name = "GridView";
            root.AddChild(gridView);

            var gridController = new GridInteractionController();
            gridController.Name = "GridInteractionController";
            gridView.AddChild(gridController);

            var visualController = new BlockVisualizationController();
            visualController.Name = "BlockVisualizationController";
            gridView.AddChild(visualController);

            var blockContainer = new Node2D();
            blockContainer.Name = "BlockContainer";
            visualController.AddChild(blockContainer);

            // Let everything initialize
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            return root;
        }

        [After]
        public async Task Cleanup()
        {
            // Clean up test scene
            if (_testScene != null && IsInstanceValid(_testScene))
            {
                _testScene.QueueFree();
                await ToSignal(_sceneTree!, SceneTree.SignalName.ProcessFrame);
            }
        }

        /// <summary>
        /// Helper method to get services from the cached service provider
        /// </summary>
        private T GetRequiredService<T>() where T : class
        {
            return _serviceProvider!.GetRequiredService<T>();
        }

        /// <summary>
        /// Helper method for waiting with proper frame processing
        /// </summary>
        private async Task WaitMilliseconds(int milliseconds)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await Task.Delay(milliseconds);
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
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }

            TestLogger.Log("=== Starting ClickOnGrid_PlacesBlock_CompleteFlow ===");
            GD.Print("[TEST] === Starting ClickOnGrid_PlacesBlock_CompleteFlow ===");

            // TRACE: Check initial block count
            var initialBlockContainer = _visualController!.BlockContainer;
            TestLogger.Log($"TEST TRACE: ClickOnGrid starting with {initialBlockContainer?.GetChildCount() ?? -1} blocks");
            GD.Print($"[TEST] ClickOnGrid starting with {initialBlockContainer?.GetChildCount() ?? -1} blocks");

            // Arrange
            var clickPosition = new Vector2Int(5, 5);
            var worldPosition = new Vector2(clickPosition.X * 64 + 32, clickPosition.Y * 64 + 32);

            // Get the grid state service from real SceneRoot
            var gridState = GetRequiredService<IGridStateService>();

            // Verify grid is initially empty
            var initialBlock = gridState.GetBlockAt(clickPosition);
            initialBlock.ShouldBeNone("grid should be empty initially");

            // Act - Simulate mouse click
            var clickEvent = new InputEventMouseButton
            {
                Position = worldPosition,
                ButtonIndex = MouseButton.Left,
                Pressed = true
            };

            _gridController!._GuiInput(clickEvent);

            // Wait for async operations to complete
            await WaitMilliseconds(100);

            // Assert - Verify core state updated
            var placedBlock = gridState.GetBlockAt(clickPosition);
            placedBlock.ShouldBeSome("block should be placed after click");

            // Wait for async operations to complete (notification pipeline)
            await WaitMilliseconds(500);

            // Assert - Verify visual state updated
            var blockContainer = _visualController!.BlockContainer;
            blockContainer.Should().NotBeNull("block container must exist");
            blockContainer!.GetChildCount().Should().Be(1, "exactly one visual block should be created");

            // Verify the visual block is at correct position
            var visualBlock = blockContainer.GetChild(0) as Node2D;
            visualBlock.Should().NotBeNull("visual block should exist");
            visualBlock!.Position.Should().Be(new Vector2(clickPosition.X * 64, clickPosition.Y * 64),
                "visual block should be at the correct grid position");
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
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }

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
                _gridController!._GuiInput(clickEvent);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                await Task.Delay(50);
            }

            // Assert - Should only have one block
            var blockContainer = _visualController!.BlockContainer;
            blockContainer!.GetChildCount().Should().Be(1, "only one block should exist despite multiple clicks");

            // Verify no duplicate blocks in core state
            var gridState = GetRequiredService<IGridStateService>();
            var block = gridState.GetBlockAt(clickPosition);
            block.ShouldBeSome("block should exist at clicked position");
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
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }

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

                _gridController!._GuiInput(clickEvent);
                await WaitMilliseconds(50);
            }

            // Assert - All blocks should be placed
            var blockContainer = _visualController!.BlockContainer;
            blockContainer!.GetChildCount().Should().Be(positions.Length,
                "all {0} blocks should be placed", positions.Length);

            // Verify each position has a block in core state
            var gridState = GetRequiredService<IGridStateService>();
            foreach (var pos in positions)
            {
                var block = gridState.GetBlockAt(pos);
                block.ShouldBeSome($"block should exist at position ({pos.X}, {pos.Y})");
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
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }

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
            _gridController!._GuiInput(clickEvent);
            await WaitMilliseconds(100);

            // Assert - No blocks should be placed
            var blockContainer = _visualController!.BlockContainer;
            blockContainer!.GetChildCount().Should().Be(0,
                "no blocks should be placed for invalid position");
        }

        /// <summary>
        /// PERFORMANCE TEST: Rapid block placement
        /// 
        /// Tests that the system can handle rapid clicking without issues.
        /// </summary>
        [TestCase]
        public async Task RapidClicking_AllBlocksProcessedCorrectly()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }

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

                _gridController!._GuiInput(clickEvent);
                // Minimal delay to simulate rapid clicking
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            // Wait for all processing to complete
            await WaitMilliseconds(500);

            // Assert - All blocks should be placed
            var blockContainer = _visualController!.BlockContainer;
            blockContainer!.GetChildCount().Should().Be(25,
                "all 25 blocks from rapid clicking should be placed");

            // Verify core state
            var gridState = GetRequiredService<IGridStateService>();
            foreach (var pos in positions)
            {
                var block = gridState.GetBlockAt(pos);
                block.ShouldBeSome($"block at ({pos.X}, {pos.Y}) should exist after rapid clicking");
            }
        }
    }
}
