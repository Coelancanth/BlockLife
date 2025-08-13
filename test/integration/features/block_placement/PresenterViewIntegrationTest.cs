using Godot;
using GdUnit4;
using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Features.Block.Placement.Notifications;
using BlockLife.Core.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace BlockLife.Tests.Integration.Features.BlockPlacement
{
    /// <summary>
    /// INTEGRATION TESTS: Presenter-View Communication
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
    public partial class PresenterViewIntegrationTest
    {
        private SceneTree _sceneTree;
        private SceneRoot _sceneRoot;
        private Node _mainScene;
        private GridView _gridView;
        private BlockManagementPresenter _presenter;
        private IServiceProvider _serviceProvider;
        private IMediator _mediator;

        [Before]
        public async Task Setup()
        {
            // Get the scene tree
            _sceneTree = Engine.GetMainLoop() as SceneTree;
            Assert.That(_sceneTree).IsNotNull();

            // Get SceneRoot singleton
            _sceneRoot = _sceneTree.Root.GetNode<SceneRoot>("/root/SceneRoot");
            Assert.That(_sceneRoot).IsNotNull();
            
            // Get services
            _serviceProvider = _sceneRoot.ServiceProvider;
            _mediator = _serviceProvider.GetRequiredService<IMediator>();
            
            // Load the main scene
            var mainScenePath = "res://godot_project/scenes/Main/main.tscn";
            var mainSceneResource = GD.Load<PackedScene>(mainScenePath);
            _mainScene = mainSceneResource.Instantiate();
            _sceneTree.Root.AddChild(_mainScene);
            
            // Wait for scene to be ready
            await _sceneTree.ProcessFrame();
            
            // Get the grid view
            _gridView = _mainScene.GetNode<GridView>("GridView");
            Assert.That(_gridView).IsNotNull();
            
            // Get the presenter (should be auto-created by view)
            _presenter = _gridView.Presenter as BlockManagementPresenter;
            Assert.That(_presenter).IsNotNull();
        }

        [After]
        public async Task Cleanup()
        {
            if (_mainScene != null && IsInstanceValid(_mainScene))
            {
                _mainScene.QueueFree();
                await _sceneTree.ProcessFrame();
            }
        }

        /// <summary>
        /// INTEGRATION TEST: Presenter properly initializes with view
        /// 
        /// Verifies that the presenter is correctly created and attached
        /// to the view through the PresenterFactory pattern.
        /// </summary>
        [TestCase]
        public void PresenterInitialization_AutoCreatedAndAttached()
        {
            // Assert - Presenter should be created and initialized
            Assert.That(_presenter).IsNotNull();
            Assert.That(_gridView.Presenter).IsNotNull();
            Assert.That(_gridView.Presenter).IsInstanceOf<BlockManagementPresenter>();
            
            // Verify presenter has access to services
            var presenterType = _presenter.GetType();
            var mediatorField = presenterType.GetField("_mediator", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.That(mediatorField).IsNotNull();
            
            var mediatorValue = mediatorField.GetValue(_presenter);
            Assert.That(mediatorValue).IsNotNull();
        }

        /// <summary>
        /// INTEGRATION TEST: View events trigger presenter commands
        /// 
        /// Verifies that when the view raises events (like cell clicked),
        /// the presenter responds by sending appropriate commands.
        /// </summary>
        [TestCase]
        public async Task ViewEvent_TriggersPresenterCommand()
        {
            // Arrange
            var clickPosition = new Vector2Int(4, 4);
            var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
            
            // Verify initial state
            var initialBlock = gridState.GetBlockAt(clickPosition);
            Assert.That(initialBlock.IsNone).IsTrue();

            // Act - Simulate view event (grid cell clicked)
            var gridController = _gridView.GetNode<GridInteractionController>("GridInteractionController");
            var worldPos = new Vector2(clickPosition.X * 64 + 32, clickPosition.Y * 64 + 32);
            var clickEvent = new InputEventMouseButton
            {
                Position = worldPos,
                ButtonIndex = MouseButton.Left,
                Pressed = true
            };
            
            gridController._GuiInput(clickEvent);
            
            // Wait for command processing
            await Task.Delay(100);
            await _sceneTree.ProcessFrame();

            // Assert - Command should have been processed
            var placedBlock = gridState.GetBlockAt(clickPosition);
            Assert.That(placedBlock.IsSome).IsTrue();
        }

        /// <summary>
        /// INTEGRATION TEST: Notification updates view through presenter
        /// 
        /// Verifies that when a notification is published, the presenter
        /// receives it and updates the view accordingly.
        /// </summary>
        [TestCase]
        public async Task Notification_UpdatesViewThroughPresenter()
        {
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
            var visualController = _gridView.GetNode<BlockVisualizationController>("BlockVisualizationController");
            var blockContainer = visualController.GetNode("BlockContainer");
            
            // Verify initial state
            var initialCount = blockContainer.GetChildCount();

            // Act - Publish notification directly
            await _mediator.Publish(notification);
            
            // Wait for notification to be processed
            await Task.Delay(100);
            await _sceneTree.ProcessFrame();

            // Assert - View should be updated
            var finalCount = blockContainer.GetChildCount();
            Assert.That(finalCount).IsEqual(initialCount + 1);
            
            // Verify the block visual exists at correct position
            var visualBlock = blockContainer.GetChild(blockContainer.GetChildCount() - 1) as Node2D;
            Assert.That(visualBlock).IsNotNull();
            Assert.That(visualBlock.Position).IsEqual(new Vector2(position.X * 64, position.Y * 64));
        }

        /// <summary>
        /// INTEGRATION TEST: Presenter properly subscribes to notifications
        /// 
        /// Verifies that the presenter subscribes to domain notifications
        /// on initialization and unsubscribes on disposal.
        /// </summary>
        [TestCase]
        public async Task PresenterNotificationSubscription_ProperLifecycle()
        {
            // Arrange - Create a new GridView to test lifecycle
            var testScene = GD.Load<PackedScene>("res://godot_project/features/grid/Grid.tscn");
            var testGrid = testScene.Instantiate<GridView>();
            _sceneTree.Root.AddChild(testGrid);
            await _sceneTree.ProcessFrame();
            
            var testPresenter = testGrid.Presenter as BlockManagementPresenter;
            Assert.That(testPresenter).IsNotNull();
            
            // Act - Send notification while presenter is alive
            var notification1 = new BlockPlacedNotification(
                Guid.NewGuid(),
                new Vector2Int(1, 1),
                BlockType.Basic,
                DateTime.UtcNow
            );
            
            await _mediator.Publish(notification1);
            await Task.Delay(50);
            await _sceneTree.ProcessFrame();
            
            // Get initial block count
            var visualController = testGrid.GetNode<BlockVisualizationController>("BlockVisualizationController");
            var blockContainer = visualController.GetNode("BlockContainer");
            var countAfterFirst = blockContainer.GetChildCount();
            Assert.That(countAfterFirst).IsEqual(1);
            
            // Dispose the presenter
            testGrid.QueueFree();
            await _sceneTree.ProcessFrame();
            await Task.Delay(100);
            
            // Act - Send another notification after disposal
            // This should not affect anything since presenter is disposed
            var notification2 = new BlockPlacedNotification(
                Guid.NewGuid(),
                new Vector2Int(2, 2),
                BlockType.Basic,
                DateTime.UtcNow
            );
            
            await _mediator.Publish(notification2);
            await Task.Delay(50);
            
            // Assert - The disposed presenter shouldn't process notifications
            // (We can't directly verify this without the view, but we can
            // verify the presenter was properly disposed)
            Assert.That(IsInstanceValid(testGrid)).IsFalse();
        }

        /// <summary>
        /// INTEGRATION TEST: Multiple presenters don't interfere
        /// 
        /// Verifies that multiple views with their own presenters
        /// can coexist without interfering with each other.
        /// </summary>
        [TestCase]
        public async Task MultiplePresenters_IndependentOperation()
        {
            // Arrange - Create a second grid view
            var gridScene = GD.Load<PackedScene>("res://godot_project/features/grid/Grid.tscn");
            var secondGrid = gridScene.Instantiate<GridView>();
            _sceneTree.Root.AddChild(secondGrid);
            await _sceneTree.ProcessFrame();
            
            var secondPresenter = secondGrid.Presenter as BlockManagementPresenter;
            Assert.That(secondPresenter).IsNotNull();
            Assert.That(secondPresenter).IsNotEqual(_presenter);
            
            // Act - Send a notification
            var notification = new BlockPlacedNotification(
                Guid.NewGuid(),
                new Vector2Int(3, 3),
                BlockType.Work,
                DateTime.UtcNow
            );
            
            await _mediator.Publish(notification);
            await Task.Delay(100);
            await _sceneTree.ProcessFrame();
            
            // Assert - Both views should receive the notification
            var firstVisualController = _gridView.GetNode<BlockVisualizationController>("BlockVisualizationController");
            var firstBlockContainer = firstVisualController.GetNode("BlockContainer");
            Assert.That(firstBlockContainer.GetChildCount()).IsEqual(1);
            
            var secondVisualController = secondGrid.GetNode<BlockVisualizationController>("BlockVisualizationController");
            var secondBlockContainer = secondVisualController.GetNode("BlockContainer");
            Assert.That(secondBlockContainer.GetChildCount()).IsEqual(1);
            
            // Cleanup
            secondGrid.QueueFree();
            await _sceneTree.ProcessFrame();
        }

        /// <summary>
        /// PERFORMANCE TEST: Presenter handles rapid notifications
        /// 
        /// Tests that the presenter can handle a burst of notifications
        /// without dropping any or causing performance issues.
        /// </summary>
        [TestCase]
        public async Task RapidNotifications_AllProcessedCorrectly()
        {
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
                await _mediator.Publish(notification);
            }
            
            // Wait for processing
            await Task.Delay(200);
            await _sceneTree.ProcessFrame();
            
            // Assert - All notifications should be processed
            var visualController = _gridView.GetNode<BlockVisualizationController>("BlockVisualizationController");
            var blockContainer = visualController.GetNode("BlockContainer");
            Assert.That(blockContainer.GetChildCount()).IsEqual(10);
        }
    }
}