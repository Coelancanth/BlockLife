using Godot;
using GdUnit4;
using System;
using System.Threading.Tasks;
using BlockLife.Godot.Scenes;
using BlockLife.Godot.Features.Block.Placement;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using static GdUnit4.Assertions;

namespace BlockLife.Tests.Integration.Features.BlockPlacement
{
    /// <summary>
    /// SIMPLE SCENE-BASED TESTS: Integration tests that work with Godot's scene system
    /// 
    /// This approach creates test scenes as child nodes of the test runner,
    /// ensuring they run in the proper Godot context with access to autoloads.
    /// 
    /// This is simpler than using ISceneRunner and works reliably.
    /// </summary>
    [TestSuite]
    public partial class SimpleSceneTest : Node
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
        /// Creates a test scene with proper structure
        /// </summary>
        private async Task<Node> CreateTestScene()
        {
            // Try to load existing test scene first
            var scenePath = "res://test/integration/test_scenes/TestGridScene.tscn";
            if (ResourceLoader.Exists(scenePath))
            {
                var packedScene = GD.Load<PackedScene>(scenePath);
                var scene = packedScene.Instantiate();
                AddChild(scene);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                return scene;
            }
            
            // Otherwise create scene programmatically
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

        /// <summary>
        /// Helper to simulate input events
        /// </summary>
        private async Task SimulateClick(Vector2 position)
        {
            var clickEvent = new InputEventMouseButton
            {
                Position = position,
                GlobalPosition = position,
                ButtonIndex = MouseButton.Left,
                Pressed = true
            };
            
            // Send input to the controller
            _gridController!.GetViewport().PushInput(clickEvent);
            
            // Also send the release event
            var releaseEvent = new InputEventMouseButton
            {
                Position = position,
                GlobalPosition = position,
                ButtonIndex = MouseButton.Left,
                Pressed = false
            };
            
            _gridController.GetViewport().PushInput(releaseEvent);
            
            // Wait for processing
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        /// <summary>
        /// SIMPLE SCENE TEST: Click places block
        /// </summary>
        [TestCase]
        public async Task SimpleClick_PlacesBlock()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Arrange
            var clickPosition = new Vector2Int(5, 5);
            var worldPosition = new Vector2(clickPosition.X * 64 + 32, clickPosition.Y * 64 + 32);
            
            var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
            var initialBlock = gridState.GetBlockAt(clickPosition);
            initialBlock.ShouldBeNone("grid should be empty initially");

            // Act
            await SimulateClick(worldPosition);
            await Task.Delay(100); // Allow async processing
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            // Assert
            var placedBlock = gridState.GetBlockAt(clickPosition);
            placedBlock.ShouldBeSome("block should be placed after click");
            
            var blockContainer = _visualController!.GetNode("BlockContainer");
            blockContainer.GetChildCount().Should().Be(1, "exactly one visual block should exist");
        }

        /// <summary>
        /// SIMPLE SCENE TEST: Multiple blocks
        /// </summary>
        [TestCase]
        public async Task MultipleClicks_PlaceMultipleBlocks()
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
                new Vector2Int(2, 2),
                new Vector2Int(3, 3)
            };

            // Act
            foreach (var pos in positions)
            {
                var worldPos = new Vector2(pos.X * 64 + 32, pos.Y * 64 + 32);
                await SimulateClick(worldPos);
                await Task.Delay(50);
            }
            
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            // Assert
            var blockContainer = _visualController!.GetNode("BlockContainer");
            AssertInt(blockContainer.GetChildCount()).IsEqual(positions.Length);
            
            var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
            foreach (var pos in positions)
            {
                var block = gridState.GetBlockAt(pos);
                AssertBool(block.IsSome).IsTrue();
            }
        }

        /// <summary>
        /// SIMPLE SCENE TEST: Duplicate prevention
        /// </summary>
        [TestCase]
        public async Task DuplicateClick_OnlyOneBlock()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Arrange
            var clickPosition = new Vector2Int(4, 4);
            var worldPosition = new Vector2(clickPosition.X * 64 + 32, clickPosition.Y * 64 + 32);

            // Act - Click same position 3 times
            for (int i = 0; i < 3; i++)
            {
                await SimulateClick(worldPosition);
                await Task.Delay(50);
            }
            
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            // Assert - Only one block
            var blockContainer = _visualController!.GetNode("BlockContainer");
            AssertInt(blockContainer.GetChildCount()).IsEqual(1);
        }

        /// <summary>
        /// SIMPLE SCENE TEST: Grid boundaries
        /// </summary>
        [TestCase]
        public async Task OutOfBounds_NoBlock()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Arrange - Position outside 10x10 grid
            var invalidPosition = new Vector2Int(100, 100);
            var worldPosition = new Vector2(invalidPosition.X * 64 + 32, invalidPosition.Y * 64 + 32);

            // Act
            await SimulateClick(worldPosition);
            await Task.Delay(100);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            // Assert - No blocks placed
            var blockContainer = _visualController!.GetNode("BlockContainer");
            AssertInt(blockContainer.GetChildCount()).IsEqual(0);
        }
    }
}