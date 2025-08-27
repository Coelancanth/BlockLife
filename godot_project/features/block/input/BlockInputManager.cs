using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Common;
using BlockLife.godot_project.features.block.input.Infrastructure;
using BlockLife.godot_project.features.block.placement;
using BlockLife.godot_project.scenes.Main;
using Godot;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BlockLife.godot_project.features.block.input;

/// <summary>
/// Centralized input router for block operations.
/// Now uses unified handler and extracted input mappings for cleaner architecture.
/// </summary>
public partial class BlockInputManager : Node
{
    // Input configuration via resource (can be shared/saved)
    [Export] private InputMappings? _inputMappings;

    private UnifiedInputHandler? _unifiedHandler;
    private InputStateManager? _stateManager;
    private ILogger? _logger;

    private IDisposable? _cellClickedSubscription;
    private IDisposable? _cellHoveredSubscription;

    public override void _Ready()
    {
        // Use default mappings if not configured in editor
        _inputMappings ??= InputMappings.CreateDefault();

        // Defer initialization to avoid race condition with SceneRoot async setup
        CallDeferred(nameof(InitializeWhenSceneRootReady));
    }

    /// <summary>
    /// Deferred initialization to avoid race condition with SceneRoot async setup
    /// </summary>
    private async void InitializeWhenSceneRootReady()
    {
        var sceneRoot = GetNode<SceneRoot>("/root/SceneRoot");
        
        if (sceneRoot?.ServiceProvider != null)
        {
            // Pre-warm input system components to prevent first-operation delays
            await InputSystemInitializer.Initialize(this, sceneRoot.Logger);

            InitializeServices();
            SubscribeToEvents();

            _logger?.Debug("BlockInputManager initialized with mappings: {Mappings}",
                _inputMappings?.GetMappingDescription());
        }
        else
        {
            // SceneRoot might still be initializing - try again in the next frame
            if (sceneRoot != null)
            {
                _logger?.Debug("SceneRoot ServiceProvider not ready yet, retrying...");
                CallDeferred(nameof(InitializeWhenSceneRootReady));
            }
            else
            {
                _logger?.Warning("SceneRoot not found. Input system initialization skipped.");
            }
        }
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

        // Initialize state manager and unified handler
        _stateManager = new InputStateManager(_logger);
        _unifiedHandler = new UnifiedInputHandler(mediator, gridStateService, _stateManager, _logger);
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

        if (_inputMappings == null || _unifiedHandler == null)
            return;

        if (keyEvent.Keycode == _inputMappings.PlaceBlockKey)
        {
            _ = _unifiedHandler.HandlePlaceBlock();
            GetViewport().SetInputAsHandled();
        }
        else if (keyEvent.Keycode == _inputMappings.InspectBlockKey)
        {
            _unifiedHandler.HandleInspectBlock();
            GetViewport().SetInputAsHandled();
        }
        else if (keyEvent.Keycode == _inputMappings.CycleBlockTypeKey)
        {
            _unifiedHandler.CycleBlockType();
            GetViewport().SetInputAsHandled();
        }
        else if (keyEvent.Keycode == _inputMappings.UnlockMergeKey)
        {
            _ = _unifiedHandler.HandleUnlockMerge();
            GetViewport().SetInputAsHandled();
        }
    }

    private async Task HandleCellClick(Vector2Int position)
    {
        // With drag-to-move system, clicks no longer select blocks
        // They only complete movement for already-dragged blocks
        if (_unifiedHandler != null)
            await _unifiedHandler.HandleCellClick(position);
    }

    private void HandleCellHover(Vector2Int position)
    {
        _unifiedHandler?.UpdateHoverPosition(position);
    }

    public override void _ExitTree()
    {
        _cellClickedSubscription?.Dispose();
        _cellHoveredSubscription?.Dispose();
        _unifiedHandler?.Dispose();
        _stateManager?.Dispose();
        base._ExitTree();
    }
}
