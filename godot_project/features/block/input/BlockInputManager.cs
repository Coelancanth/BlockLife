using Godot;
using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Common;
using BlockLife.Godot.Features.Block.Input.Handlers;
using BlockLife.Godot.Features.Block.Input.Infrastructure;
using BlockLife.Godot.Features.Block.Input.State;
using BlockLife.Godot.Features.Block.Placement;
using BlockLife.Godot.Scenes;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BlockLife.Godot.Features.Block.Input;

/// <summary>
/// Centralized input router for block operations.
/// Delegates to specialized handlers for each input type.
/// </summary>
public partial class BlockInputManager : Node
{
    [Export] public Key PlaceBlockKey { get; set; } = Key.Space;
    [Export] public Key InspectBlockKey { get; set; } = Key.I;
    
    private BlockSelectionManager? _selectionManager;
    private BlockPlacementHandler? _placementHandler;
    private BlockInspectionHandler? _inspectionHandler;
    private BlockMovementHandler? _movementHandler;
    private ILogger? _logger;
    
    private IDisposable? _cellClickedSubscription;
    private IDisposable? _cellHoveredSubscription;

    public override async void _Ready()
    {
        // Pre-warm input system components to prevent first-operation delays
        var sceneRoot = GetNode<SceneRoot>("/root/SceneRoot");
        await InputSystemInitializer.Initialize(this, sceneRoot?.Logger);
        
        InitializeServices();
        SubscribeToEvents();
        
        _logger?.Debug("BlockInputManager initialized - Place: {PlaceKey}, Inspect: {InspectKey}", 
            PlaceBlockKey, InspectBlockKey);
    }
    
    private void InitializeServices()
    {
        var sceneRoot = GetNode<SceneRoot>("/root/SceneRoot");
        _logger = sceneRoot?.Logger?.ForContext<BlockInputManager>();
        
        if (sceneRoot?.ServiceProvider == null)
        {
            _logger?.Error("ServiceProvider not available");
            return;
        }
        
        var mediator = sceneRoot.ServiceProvider.GetService<IMediator>();
        var gridStateService = sceneRoot.ServiceProvider.GetService<Core.Infrastructure.Services.IGridStateService>();
        
        if (mediator == null || gridStateService == null)
        {
            _logger?.Error("Required services not available");
            return;
        }
        
        // Initialize managers and handlers
        _selectionManager = new BlockSelectionManager(_logger);
        _placementHandler = new BlockPlacementHandler(mediator, _logger);
        _inspectionHandler = new BlockInspectionHandler(gridStateService, _logger);
        _movementHandler = new BlockMovementHandler(mediator, gridStateService, _selectionManager, _logger);
    }
    
    private void SubscribeToEvents()
    {
        var gridView = GetParent();
        if (gridView == null) return;
        
        var interactionController = gridView.GetNodeOrNull<GridInteractionController>("InteractionController");
        if (interactionController == null)
        {
            _logger?.Warning("GridInteractionController not found");
            return;
        }
        
        _cellClickedSubscription = interactionController.GridCellClicked
            .Subscribe(async pos => await HandleCellClick(pos));
            
        _cellHoveredSubscription = interactionController.GridCellHovered
            .Subscribe(pos => HandleCellHover(pos));
    }
    
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true } keyEvent) 
            return;
            
        if (keyEvent.Keycode == PlaceBlockKey)
        {
            _placementHandler?.HandlePlaceKey(_selectionManager?.CurrentHoverPosition ?? Option<Vector2Int>.None);
            GetViewport().SetInputAsHandled();
        }
        else if (keyEvent.Keycode == InspectBlockKey)
        {
            _inspectionHandler?.HandleInspectKey(_selectionManager?.CurrentHoverPosition ?? Option<Vector2Int>.None);
            GetViewport().SetInputAsHandled();
        }
    }
    
    private async Task HandleCellClick(Vector2Int position)
    {
        // With drag-to-move system, clicks no longer select blocks
        // They only complete movement for already-dragged blocks
        if (_movementHandler != null)
            await _movementHandler.HandleCellClick(position);
    }
    
    private void HandleCellHover(Vector2Int position)
    {
        _selectionManager?.UpdateHoverPosition(position);
    }
    
    public override void _ExitTree()
    {
        _cellClickedSubscription?.Dispose();
        _cellHoveredSubscription?.Dispose();
        _selectionManager?.Dispose();
        base._ExitTree();
    }
}