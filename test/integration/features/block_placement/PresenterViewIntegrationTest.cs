using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Features.Block.Placement.Notifications;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.godot_project.features.block.placement;
using BlockLife.godot_project.scenes.Main;
using FluentAssertions;
using GdUnit4;
using Godot;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using static GdUnit4.Assertions;

namespace BlockLife.test.integration.features.block_placement
{
    /// <summary>
    /// INTEGRATION TESTS: Presenter-View Communication
    /// Following the SimpleSceneTest pattern for consistency
    /// 
    /// These tests verify that the MVP pattern is correctly implemented
    /// with proper communication between presenters and views through
    /// the notification pipeline.
    /// 
    /// Focus Areas:
    /// - Presenter initialization and disposal
    /// - Event subscription and unsubscription
    /// - Notification handling
    /// - View updates from presenter commands
    /// </summary>
    [TestSuite]
    public partial class PresenterViewIntegrationTest : Node
    {
        private SceneTree? _sceneTree;
        private IServiceProvider? _serviceProvider;
        private IMediator? _mediator;
        private IGridStateService? _gridState;
        private Node? _testScene;
        private GridView? _gridView;
        private BlockManagementPresenter? _presenter;

        [Before]
        public async Task Setup()
        {
            // Get the scene tree from the test node itself (SimpleSceneTest pattern)
            _sceneTree = GetTree();
            _sceneTree.Should().NotBeNull("scene tree must be available");
            
            // Get SceneRoot autoload - should be available in Godot context
            var sceneRoot = _sceneTree!.Root.GetNodeOrNull<SceneRoot>("/root/SceneRoot");
            
            if (sceneRoot == null)
            {
                GD.PrintErr("SceneRoot not found - test must be run from Godot editor with SceneRoot autoload");
                return;
            }
            
            // Get service provider using reflection (SimpleSceneTest pattern)
            var serviceProviderField = typeof(SceneRoot).GetField("_serviceProvider",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            _serviceProvider = serviceProviderField?.GetValue(sceneRoot) as IServiceProvider;
            _serviceProvider.Should().NotBeNull("service provider must be initialized");
            
            // Get required services
            if (_serviceProvider != null)
            {
                _mediator = _serviceProvider.GetRequiredService<IMediator>();
                _gridState = _serviceProvider.GetRequiredService<IGridStateService>();
            }
            
            // Clear grid before each test
            _gridState?.ClearGrid();
            
            // Create test scene as child of this test node
            _testScene = await CreateTestScene();
            
            // Get the grid view from test scene
            _gridView = _testScene!.GetNode<GridView>("GridView");
            _gridView.Should().NotBeNull("grid view must exist in test scene");
            
            // Get presenter via reflection
            var presenterProperty = typeof(GridView).GetProperty("Presenter", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public);
            
            if (presenterProperty != null)
            {
                _presenter = presenterProperty.GetValue(_gridView) as BlockManagementPresenter;
            }
            else
            {
                // Try field if property doesn't exist
                var presenterField = typeof(GridView).GetField("_presenter", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                _presenter = presenterField?.GetValue(_gridView) as BlockManagementPresenter;
            }
            
            // Wait for initialization
            await ToSignal(_sceneTree, SceneTree.SignalName.ProcessFrame);
        }

        [After]
        public async Task Cleanup()
        {
            // Clear grid state
            if (_gridState != null)
            {
                _gridState.ClearGrid();
            }
            
            // Clean up test scene
            if (_testScene != null && IsInstanceValid(_testScene))
            {
                _testScene.QueueFree();
                await ToSignal(_sceneTree!, SceneTree.SignalName.ProcessFrame);
            }
        }

        /// <summary>
        /// Creates a test scene with proper MVP structure
        /// </summary>
        private async Task<Node> CreateTestScene()
        {
            // Create root node
            var root = new Node2D();
            root.Name = "TestRoot";
            AddChild(root);
            
            // Create GridView with full structure
            var gridView = new GridView();
            gridView.Name = "GridView";
            root.AddChild(gridView);
            
            // Create GridInteractionController
            var gridController = new GridInteractionController();
            gridController.Name = "GridInteractionController";
            gridView.AddChild(gridController);
            
            // Create BlockVisualizationController
            var visualController = new BlockVisualizationController();
            visualController.Name = "BlockVisualizationController";
            gridView.AddChild(visualController);
            
            // Create BlockContainer
            var blockContainer = new Node2D();
            blockContainer.Name = "BlockContainer";
            visualController.AddChild(blockContainer);
            
            // Set export properties
            gridView.InteractionController = gridController;
            gridView.VisualizationController = visualController;
            
            // Wait for nodes to be in tree
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            
            // Initialize grid view
            await gridView.InitializeAsync(new Vector2Int(10, 10));
            
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
            
            var gridController = _gridView!.GetNode<GridInteractionController>("GridInteractionController");
            gridController.GetViewport().PushInput(clickEvent);
            
            // Also send release event
            var releaseEvent = new InputEventMouseButton
            {
                Position = position,
                GlobalPosition = position,
                ButtonIndex = MouseButton.Left,
                Pressed = false
            };
            
            gridController.GetViewport().PushInput(releaseEvent);
            
            // Wait for processing
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        /// <summary>
        /// TEST: Presenter properly initializes with view
        /// </summary>
        [TestCase]
        public async Task PresenterInitialization_AutoCreatedAndAttached()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Assert - Presenter should be created and initialized
            _presenter.Should().NotBeNull("presenter should be automatically created and attached");
            
            if (_presenter != null)
            {
                // Verify presenter has access to services via reflection
                var presenterType = _presenter.GetType();
                var mediatorField = presenterType.GetField("_mediator", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (mediatorField != null)
                {
                    var mediatorValue = mediatorField.GetValue(_presenter);
                    mediatorValue.Should().NotBeNull("presenter should have mediator injected");
                }
            }
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// TEST: View events trigger presenter commands
        /// </summary>
        [TestCase]
        public async Task ViewEvent_TriggersPresenterCommand()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Arrange
            var clickPosition = new Vector2Int(4, 4);
            var worldPosition = new Vector2(clickPosition.X * 64 + 32, clickPosition.Y * 64 + 32);
            
            // Verify initial state
            var initialBlock = _gridState!.GetBlockAt(clickPosition);
            initialBlock.IsSome.Should().BeFalse("grid should be empty initially");

            // Act - Simulate click
            await SimulateClick(worldPosition);
            await Task.Delay(100); // Allow async processing
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            // Assert - Command should have been processed
            var placedBlock = _gridState.GetBlockAt(clickPosition);
            placedBlock.IsSome.Should().BeTrue("command should process and place block");
        }

        /// <summary>
        /// TEST: Notification updates view through presenter
        /// </summary>
        [TestCase]
        public async Task Notification_UpdatesViewThroughPresenter()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Arrange
            var blockId = Guid.NewGuid();
            var position = new Vector2Int(2, 2);
            var blockType = BlockType.Basic;
            var placedAt = DateTime.UtcNow;
            
            var notification = new BlockPlacedNotification(
                blockId,
                position,
                blockType,
                placedAt
            );
            
            // Get the visualization controller
            var visualController = _gridView!.GetNode<BlockVisualizationController>("BlockVisualizationController");
            var blockContainer = visualController.GetNode("BlockContainer");
            
            // Verify initial state
            var initialCount = blockContainer.GetChildCount();

            // Act - Publish notification directly
            await _mediator!.Publish(notification);
            
            // Wait for notification to be processed
            await Task.Delay(100);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            // Assert - View should be updated
            var finalCount = blockContainer.GetChildCount();
            finalCount.Should().Be(initialCount + 1, "notification should trigger view update");
            
            // Verify the block visual exists at correct position
            if (finalCount > 0)
            {
                var visualBlock = blockContainer.GetChild(blockContainer.GetChildCount() - 1) as Node2D;
                visualBlock.Should().NotBeNull("visual block should be created");
                visualBlock!.Position.Should().Be(new Vector2(position.X * 64, position.Y * 64),
                    "visual block should be at notification position");
            }
        }

        /// <summary>
        /// TEST: Multiple presenters don't interfere
        /// </summary>
        [TestCase]
        public async Task MultiplePresenters_IndependentOperation()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Arrange - Create a second grid view
            var secondGrid = new GridView();
            secondGrid.Name = "SecondGrid";
            AddChild(secondGrid);
            
            // Add required child nodes
            var visualController = new BlockVisualizationController();
            visualController.Name = "BlockVisualizationController";
            secondGrid.AddChild(visualController);
            
            var blockContainer = new Node2D();
            blockContainer.Name = "BlockContainer";
            visualController.AddChild(blockContainer);
            
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            
            // Act - Send a notification
            var notification = new BlockPlacedNotification(
                Guid.NewGuid(),
                new Vector2Int(3, 3),
                BlockType.Work,
                DateTime.UtcNow
            );
            
            await _mediator!.Publish(notification);
            await Task.Delay(100);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            
            // Assert - Both views should receive the notification
            var firstVisualController = _gridView!.GetNode<BlockVisualizationController>("BlockVisualizationController");
            var firstBlockContainer = firstVisualController.GetNode("BlockContainer");
            AssertInt(firstBlockContainer.GetChildCount()).IsEqual(1);
            
            var secondBlockContainer = visualController.GetNode("BlockContainer");
            AssertInt(secondBlockContainer.GetChildCount()).IsEqual(1);
            
            // Cleanup
            secondGrid.QueueFree();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        /// <summary>
        /// TEST: Presenter handles rapid notifications
        /// </summary>
        [TestCase]
        public async Task RapidNotifications_AllProcessedCorrectly()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Arrange
            var notifications = new BlockPlacedNotification[10];
            for (int i = 0; i < 10; i++)
            {
                notifications[i] = new BlockPlacedNotification(
                    Guid.NewGuid(),
                    new Vector2Int(i % 10, i / 10),
                    BlockType.Basic,
                    DateTime.UtcNow
                );
            }
            
            // Act - Send all notifications rapidly
            foreach (var notification in notifications)
            {
                await _mediator!.Publish(notification);
            }
            
            // Wait for processing
            await Task.Delay(200);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            
            // Assert - All notifications should be processed
            var visualController = _gridView!.GetNode<BlockVisualizationController>("BlockVisualizationController");
            var blockContainer = visualController.GetNode("BlockContainer");
            AssertInt(blockContainer.GetChildCount()).IsEqual(10);
        }
    }
}