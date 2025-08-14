using Godot;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Commands;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Godot.Features.Block.Placement;
using BlockLife.Godot.Scenes;
using LanguageExt;
using static LanguageExt.Prelude;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BlockLife.Godot.Features.Block.Input;

/// <summary>
/// Centralized input manager for all block-related operations.
/// Handles both block placement and movement with configurable key bindings.
/// Replaces click-to-place with key-triggered placement for better control.
/// </summary>
public partial class BlockInputManager : Node
{
    // Configurable key bindings
    [Export] public Key PlaceBlockKey { get; set; } = Key.Space;
    [Export] public Key InspectBlockKey { get; set; } = Key.I;
    
    // Movement state
    private Option<Guid> _selectedBlockId = None;
    private Option<Vector2Int> _selectedBlockPosition = None;
    
    // Services
    private BlockManagementPresenter? _blockManagementPresenter;
    private IMediator? _mediator;
    private IGridStateService? _gridStateService;
    
    // Controllers
    private GridInteractionController? _interactionController;
    private BlockVisualizationController? _visualizationController;
    
    // Subscriptions
    private IDisposable? _cellClickedSubscription;
    private IDisposable? _cellHoveredSubscription;
    
    // Current hover position for placement and inspection
    private Option<Vector2Int> _currentHoverPosition = None;
    
    public override void _Ready()
    {
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Information("BlockInputManager ready - Place: {PlaceKey}, Inspect: {InspectKey}", 
            PlaceBlockKey, InspectBlockKey);
        
        // Get references to controllers
        var gridView = GetParent();
        if (gridView != null)
        {
            _interactionController = gridView.GetNodeOrNull<GridInteractionController>("InteractionController");
            _visualizationController = gridView.GetNodeOrNull<BlockVisualizationController>("VisualizationController");
            
            if (_interactionController != null)
            {
                // Subscribe to cell events for movement and hover tracking
                _cellClickedSubscription = _interactionController.GridCellClicked.Subscribe(OnCellClicked);
                _cellHoveredSubscription = _interactionController.GridCellHovered.Subscribe(OnCellHovered);
                logger?.Information("BlockInputManager subscribed to grid interaction events");
            }
            else
            {
                logger?.Warning("GridInteractionController not found - input will not work");
            }
            
            // Get the presenter from the parent GridView
            if (gridView is GridView gv)
            {
                var presenterField = typeof(GridView).GetField("_presenter", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                _blockManagementPresenter = presenterField?.GetValue(gv) as BlockManagementPresenter;
                
                if (_blockManagementPresenter != null)
                {
                    logger?.Information("Successfully obtained BlockManagementPresenter from GridView");
                }
            }
        }
        
        // Get services via reflection from SceneRoot
        var sceneRoot = GetNode<SceneRoot>("/root/SceneRoot");
        if (sceneRoot != null)
        {
            var serviceProviderField = typeof(SceneRoot).GetField("_serviceProvider",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var serviceProvider = serviceProviderField?.GetValue(sceneRoot) as IServiceProvider;
            
            if (serviceProvider != null)
            {
                _mediator = serviceProvider.GetService<IMediator>();
                _gridStateService = serviceProvider.GetService<IGridStateService>();
                
                logger?.Information("BlockInputManager services initialized successfully");
            }
        }
    }
    
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == PlaceBlockKey)
            {
                HandlePlaceBlockKey();
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == InspectBlockKey)
            {
                HandleInspectBlockKey();
                GetViewport().SetInputAsHandled();
            }
        }
    }
    
    /// <summary>
    /// Handles the place block key press - places a block at the current hover position.
    /// </summary>
    private async void HandlePlaceBlockKey()
    {
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        
        if (_currentHoverPosition.IsSome)
        {
            var position = _currentHoverPosition.Match(Some: p => p, None: () => new Vector2Int(0, 0));
            logger?.Information("🔨 Placing block at {Position} via {Key} key", position, PlaceBlockKey);
            
            if (_mediator != null)
            {
                // Place a basic block at the hovered position
                var command = new PlaceBlockCommand(position, BlockLife.Core.Domain.Block.BlockType.Basic);
                var result = await _mediator.Send(command);
                
                result.Match(
                    Succ: _ =>
                    {
                        logger?.Information("✅ Block placed successfully at {Position}", position);
                    },
                    Fail: error =>
                    {
                        logger?.Warning("❌ Failed to place block at {Position}: {Error}", position, error.Message);
                    }
                );
            }
            else
            {
                logger?.Warning("Cannot place block - IMediator not available");
            }
        }
        else
        {
            logger?.Information("No position hovered - move cursor over grid to place block");
        }
    }
    
    /// <summary>
    /// Handles the inspect block key press - prints detailed information about the block under cursor.
    /// </summary>
    private void HandleInspectBlockKey()
    {
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        
        if (_currentHoverPosition.IsSome)
        {
            var position = _currentHoverPosition.Match(Some: p => p, None: () => new Vector2Int(0, 0));
            logger?.Information("🔍 Inspecting position {Position}", position);
            
            if (_gridStateService != null)
            {
                var blockAtPosition = _gridStateService.GetBlockAt(position);
                blockAtPosition.Match(
                    Some: block =>
                    {
                        // Get view information if available
                        var hasView = false;
                        var viewPosition = new Vector2(0, 0);
                        var isVisible = false;
                        
                        if (_visualizationController != null)
                        {
                            // Use reflection to access the private _blockNodes dictionary
                            var blockNodesField = typeof(BlockVisualizationController).GetField("_blockNodes",
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (blockNodesField?.GetValue(_visualizationController) is System.Collections.Generic.Dictionary<Guid, Node2D> blockNodes)
                            {
                                if (blockNodes.TryGetValue(block.Id, out var node))
                                {
                                    hasView = true;
                                    viewPosition = node.Position;
                                    isVisible = node.Visible;
                                }
                            }
                        }
                        
                        // Print comprehensive block information
                        logger?.Information("📋 BLOCK INFO at {Position}:", position);
                        logger?.Information("   BlockId: {BlockId}", block.Id);
                        logger?.Information("   Type: {BlockType}", block.Type);
                        logger?.Information("   Position: {Position}", block.Position);
                        logger?.Information("   CreatedAt: {CreatedAt}", block.CreatedAt);
                        logger?.Information("   LastModifiedAt: {LastModifiedAt}", block.LastModifiedAt);
                        logger?.Information("   Has View: {HasView}", hasView);
                        if (hasView)
                        {
                            logger?.Information("   View Position: ({X}, {Y})", viewPosition.X, viewPosition.Y);
                            logger?.Information("   View Visible: {IsVisible}", isVisible);
                        }
                        logger?.Information("   Is Occupied: True");
                        
                        // Also print to Godot console for easy copying
                        GD.Print($"=== BLOCK INSPECTION ===");
                        GD.Print($"Position: ({position.X}, {position.Y})");
                        GD.Print($"BlockId: {block.Id}");
                        GD.Print($"Type: {block.Type}");
                        GD.Print($"CreatedAt: {block.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                        GD.Print($"LastModifiedAt: {block.LastModifiedAt:yyyy-MM-dd HH:mm:ss}");
                        GD.Print($"HasView: {hasView}");
                        if (hasView)
                        {
                            GD.Print($"ViewPosition: ({viewPosition.X:F1}, {viewPosition.Y:F1})");
                            GD.Print($"ViewVisible: {isVisible}");
                        }
                        GD.Print($"========================");
                    },
                    None: () =>
                    {
                        logger?.Information("📋 EMPTY POSITION at {Position}:", position);
                        logger?.Information("   No block present");
                        logger?.Information("   Is Valid Position: {IsValid}", _gridStateService.IsValidPosition(position));
                        
                        GD.Print($"=== POSITION INSPECTION ===");
                        GD.Print($"Position: ({position.X}, {position.Y})");
                        GD.Print($"Status: Empty");
                        GD.Print($"Valid: {_gridStateService.IsValidPosition(position)}");
                        GD.Print($"===========================");
                    }
                );
            }
            else
            {
                logger?.Warning("Cannot inspect - GridStateService not available");
            }
        }
        else
        {
            logger?.Information("No position hovered - move cursor over grid to inspect");
        }
    }
    
    /// <summary>
    /// Tracks mouse hover for placement and inspection targeting.
    /// </summary>
    private void OnCellHovered(Vector2Int position)
    {
        _currentHoverPosition = Some(position);
        
        // Optional: Show placement preview when hovering if no block is selected
        if (_selectedBlockId.IsNone && _blockManagementPresenter != null)
        {
            // Could add preview logic here later
        }
    }
    
    /// <summary>
    /// Handles cell clicks for block movement only (placement disabled).
    /// </summary>
    private async void OnCellClicked(Vector2Int position)
    {
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        
        // Only handle movement logic - no placement via clicks
        await _selectedBlockId.Match(
            Some: async selectedId =>
            {
                // We have a selected block - try to move it
                if (_selectedBlockPosition.IsSome && _selectedBlockPosition != Some(position))
                {
                    var fromPos = _selectedBlockPosition.Match(Some: p => p, None: () => position);
                    logger?.Information("🔄 Moving block {BlockId} from {FromPosition} to {ToPosition}", 
                        selectedId, fromPos, position);
                    
                    await MoveBlockAsync(selectedId, position);
                    await ClearSelectionAsync();
                }
                else
                {
                    // Clicked same position - deselect
                    logger?.Debug("Deselecting block at {Position}", position);
                    await ClearSelectionAsync();
                }
            },
            None: async () =>
            {
                // No block selected - try to select block at clicked position
                var blockAtPosition = await GetBlockAtPositionAsync(position);
                await blockAtPosition.Match(
                    Some: async blockId =>
                    {
                        logger?.Information("✋ Selected block {BlockId} at position {Position}", blockId, position);
                        await SelectBlockAsync(blockId, position);
                    },
                    None: () =>
                    {
                        logger?.Debug("No block at position {Position} to select", position);
                        return Task.CompletedTask;
                    }
                );
            }
        );
    }
    
    private async Task SelectBlockAsync(Guid blockId, Vector2Int position)
    {
        _selectedBlockId = Some(blockId);
        _selectedBlockPosition = Some(position);
        
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Information("Block {BlockId} selected for movement at position {Position}", blockId, position);
        
        await Task.CompletedTask;
    }
    
    private async Task ClearSelectionAsync()
    {
        _selectedBlockId = None;
        _selectedBlockPosition = None;
        
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("Block selection cleared");
        
        await Task.CompletedTask;
    }
    
    private async Task MoveBlockAsync(Guid blockId, Vector2Int toPosition)
    {
        if (_mediator == null)
        {
            var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
            logger?.Error("Cannot send MoveBlockCommand - IMediator not available");
            return;
        }
        
        var command = new MoveBlockCommand
        {
            BlockId = blockId,
            ToPosition = toPosition
        };
        
        var logger2 = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger2?.Information("Sending MoveBlockCommand for block {BlockId} to position {ToPosition}", 
            blockId, toPosition);
        
        try
        {
            var result = await _mediator.Send(command);
            
            await result.Match(
                Succ: _ =>
                {
                    logger2?.Information("✅ Successfully moved block {BlockId} to {ToPosition}", blockId, toPosition);
                    return Task.CompletedTask;
                },
                Fail: error =>
                {
                    logger2?.Warning("❌ Failed to move block {BlockId}: {Error}", blockId, error.Message);
                    return Task.CompletedTask;
                }
            );
        }
        catch (Exception ex)
        {
            logger2?.Error(ex, "💥 Unexpected error moving block {BlockId}", blockId);
        }
    }
    
    private Task<Option<Guid>> GetBlockAtPositionAsync(Vector2Int position)
    {
        if (_gridStateService != null)
        {
            var blockAtPosition = _gridStateService.GetBlockAt(position);
            return Task.FromResult(blockAtPosition.Map(b => b.Id));
        }
        
        return Task.FromResult(Option<Guid>.None);
    }
    
    public override void _ExitTree()
    {
        _cellClickedSubscription?.Dispose();
        _cellHoveredSubscription?.Dispose();
        ClearSelectionAsync().Wait(100);
        base._ExitTree();
    }
}