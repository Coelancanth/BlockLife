using System.Threading.Tasks;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.godot_project.features.block.performance;
using BlockLife.godot_project.infrastructure;
using BlockLife.godot_project.scenes.Main;
using Godot;

namespace BlockLife.godot_project.features.block.placement;

public partial class GridView : Control, IBlockManagementView
{
    // Performance debugging commands
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");

            switch (keyEvent.Keycode)
            {
                case Key.F9:
                    logger?.Debug("F9 pressed - Printing performance report");
                    PerformanceProfiler.PrintReport();
                    GetViewport().SetInputAsHandled();
                    break;

                case Key.F10:
                    // Toggle animations on/off
                    if (VisualizationController != null)
                    {
                        VisualizationController.EnableAnimations = !VisualizationController.EnableAnimations;
                        logger?.Debug("F10 pressed - Animations {Status}",
                            VisualizationController.EnableAnimations ? "ENABLED" : "DISABLED");
                    }
                    GetViewport().SetInputAsHandled();
                    break;

                case Key.F11:
                    // Cycle animation speed
                    if (VisualizationController != null)
                    {
                        var speeds = new[] { 0.05f, 0.1f, 0.15f, 0.2f, 0.3f };
                        var currentIndex = System.Array.IndexOf(speeds, VisualizationController.AnimationSpeed);
                        var nextIndex = (currentIndex + 1) % speeds.Length;
                        VisualizationController.AnimationSpeed = speeds[nextIndex];
                        logger?.Debug("F11 pressed - Animation speed set to {Speed}ms",
                            VisualizationController.AnimationSpeed * 1000);
                    }
                    GetViewport().SetInputAsHandled();
                    break;
            }
        }
    }

    [Export] public BlockVisualizationController? VisualizationController { get; set; }
    [Export] public GridInteractionController? InteractionController { get; set; }

    private BlockManagementPresenter? _presenter;
    public BlockManagementPresenter? Presenter => _presenter; // Expose for BlockInputManager

    // Interface Implementation
    public IBlockVisualizationView Visualization => VisualizationController!;
    public IGridInteractionView Interaction => InteractionController!;

    public bool IsInitialized { get; private set; }
    public Vector2Int GridSize { get; private set; }

    public override void _Ready()
    {
        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("GridView _Ready called");

        // Initialize performance profiler
        if (logger != null)
        {
            PerformanceProfiler.Initialize(logger);
            logger?.Debug("Performance profiler initialized");
        }

        // Validate required components
        if (VisualizationController == null)
        {
            logger?.Error("VisualizationController is not assigned!");
        }
        if (InteractionController == null)
        {
            logger?.Error("InteractionController is not assigned!");
        }

        // Initialize presenter using the factory pattern - defer to avoid race condition
        CallDeferred(nameof(InitializePresenterWhenSceneRootReady));
    }

    /// <summary>
    /// Deferred presenter initialization to avoid race condition with SceneRoot async setup
    /// </summary>
    private void InitializePresenterWhenSceneRootReady()
    {
        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        var sceneRoot = GetNodeOrNull<SceneRoot>("/root/SceneRoot");
        
        if (sceneRoot?.PresenterFactory != null)
        {
            _presenter = sceneRoot.CreatePresenterFor<BlockManagementPresenter, IBlockManagementView>(this);
            _presenter?.Initialize();
            logger?.Debug("GridView presenter created and initialized successfully");
        }
        else
        {
            // SceneRoot might still be initializing - try again in the next frame
            if (sceneRoot != null)
            {
                logger?.Debug("SceneRoot PresenterFactory not ready yet, retrying...");
                CallDeferred(nameof(InitializePresenterWhenSceneRootReady));
            }
            else
            {
                logger?.Warning("SceneRoot not found at /root/SceneRoot. Presenter creation skipped. This is expected in test scenarios.");
            }
        }
    }

    /// <summary>
    /// Allow manual presenter injection for testing scenarios
    /// </summary>
    public void SetPresenterForTesting(BlockManagementPresenter presenter)
    {
        // Trace: SetPresenterForTesting called with presenter
        _presenter = presenter;
        if (_presenter != null)
        {
            // Trace: Calling presenter.Initialize()
            _presenter.Initialize();
            // Trace: presenter.Initialize() completed
        }
    }

    public override void _ExitTree()
    {
        // Print performance report before exiting
        PerformanceProfiler.PrintReport();
        _presenter?.Dispose();
    }

    public async Task InitializeAsync(Vector2Int gridSize)
    {
        GridSize = gridSize;

        // Configure interaction controller
        if (InteractionController != null)
        {
            InteractionController.GridSize = new Vector2I(gridSize.X, gridSize.Y);
            InteractionController.QueueRedraw();
        }

        IsInitialized = true;

        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("GridView initialized with size {GridWidth}x{GridHeight}", gridSize.X, gridSize.Y);
        await Task.CompletedTask;
    }

    public Task CleanupAsync()
    {
        IsInitialized = false;
        return Task.CompletedTask;
    }
}
