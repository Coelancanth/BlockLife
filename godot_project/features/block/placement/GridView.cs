using Godot;
using System.Threading.Tasks;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Presentation;
using BlockLife.Godot.Infrastructure;
using BlockLife.Godot.Scenes;

namespace BlockLife.Godot.Features.Block.Placement;

public partial class GridView : Control, IBlockManagementView
{
    [Export] public BlockVisualizationController? VisualizationController { get; set; }
    [Export] public GridInteractionController? InteractionController { get; set; }
    
    private BlockManagementPresenter? _presenter;
    
    // Interface Implementation
    public IBlockVisualizationView Visualization => VisualizationController!;
    public IGridInteractionView Interaction => InteractionController!;
    
    public bool IsInitialized { get; private set; }
    public Vector2Int GridSize { get; private set; }
    
    public override void _Ready()
    {
        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Information("GridView _Ready called");
        
        // Validate required components
        if (VisualizationController == null)
        {
            logger?.Error("VisualizationController is not assigned!");
        }
        if (InteractionController == null)
        {
            logger?.Error("InteractionController is not assigned!");
        }
        
        // Initialize presenter using the factory pattern
        var sceneRoot = GetNodeOrNull<SceneRoot>("/root/SceneRoot");
        if (sceneRoot != null)
        {
            _presenter = sceneRoot.CreatePresenterFor<BlockManagementPresenter, IBlockManagementView>(this);
            _presenter?.Initialize();
            logger?.Information("GridView presenter created and initialized successfully");
        }
        else
        {
            logger?.Warning("SceneRoot not found at /root/SceneRoot. Presenter creation skipped. This is expected in test scenarios.");
        }
    }
    
    /// <summary>
    /// Allow manual presenter injection for testing scenarios
    /// </summary>
    public void SetPresenterForTesting(BlockManagementPresenter presenter)
    {
        GD.Print($"[TRACE] SetPresenterForTesting called with presenter: {presenter != null}");
        _presenter = presenter;
        if (_presenter != null)
        {
            GD.Print("[TRACE] Calling presenter.Initialize()");
            _presenter.Initialize();
            GD.Print("[TRACE] presenter.Initialize() completed");
        }
    }
    
    public override void _ExitTree()
    {
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
        logger?.Information("GridView initialized with size {GridWidth}x{GridHeight}", gridSize.X, gridSize.Y);
        await Task.CompletedTask;
    }
    
    public Task CleanupAsync()
    {
        IsInitialized = false;
        return Task.CompletedTask;
    }
}