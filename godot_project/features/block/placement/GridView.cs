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
        GD.Print("GridView _Ready called");
        
        // Validate required components
        if (VisualizationController == null)
        {
            GD.PrintErr("VisualizationController is not assigned!");
        }
        if (InteractionController == null)
        {
            GD.PrintErr("InteractionController is not assigned!");
        }
        
        // Initialize presenter using the factory pattern
        var sceneRoot = GetNode<SceneRoot>("/root/SceneRoot");
        if (sceneRoot != null)
        {
            _presenter = sceneRoot.CreatePresenterFor<BlockManagementPresenter, IBlockManagementView>(this);
            _presenter?.Initialize();
            GD.Print("GridView presenter created and initialized successfully");
        }
        else
        {
            GD.PrintErr("SceneRoot not found! Make sure SceneRoot is set as an autoload singleton.");
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
        
        GD.Print($"GridView initialized with size {gridSize.X}x{gridSize.Y}");
        await Task.CompletedTask;
    }
    
    public Task CleanupAsync()
    {
        IsInitialized = false;
        return Task.CompletedTask;
    }
}