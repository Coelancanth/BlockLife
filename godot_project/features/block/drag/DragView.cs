using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Drag.Views;
using BlockLife.Core.Features.Block.Drag.Presenters;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Godot.Features.Block.Placement;
using BlockLife.Godot.Scenes;
using BlockLife.Godot.Infrastructure;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BlockLife.Godot.Features.Block.Drag;

/// <summary>
/// Godot implementation of IDragView for drag-and-drop operations.
/// Handles visual feedback and input events for dragging blocks.
/// </summary>
public partial class DragView : Control, IDragView
{
    // Events from IDragView interface
    public event Action<Guid, Vector2Int>? DragStarted;
    public event Action<Vector2Int>? DragUpdated;
    public event Action<Guid, Vector2Int>? DragCompleted;
    public event Action<Guid>? DragCancelled;
    
    // Visual feedback nodes
    private Control? _ghostBlock;
    private Control? _previewIndicator;
    private Control? _rangeIndicator;
    private Label? _invalidDropLabel;
    
    // References to other components
    private GridInteractionController? _interactionController;
    private BlockVisualizationController? _visualizationController;
    private IGridStateService? _gridStateService;
    private DragPresenter? _presenter;
    private Serilog.ILogger? _logger;
    
    // Drag state
    private Option<Guid> _currentDraggedBlockId = Option<Guid>.None;
    private Vector2Int _dragStartPosition;
    private bool _isDragging = false;
    
    // Subscriptions
    private IDisposable? _dragStartSubscription;
    private IDisposable? _dragMoveSubscription;
    private IDisposable? _dragEndSubscription;
    
    // Visual settings
    [Export] public float CellSize { get; set; } = 64f;
    [Export] public Color ValidDropColor { get; set; } = new Color(0, 1, 0, 0.5f);
    [Export] public Color InvalidDropColor { get; set; } = new Color(1, 0, 0, 0.5f);
    [Export] public Color GhostBlockColor { get; set; } = new Color(1, 1, 1, 0.5f);
    [Export] public Color RangeIndicatorColor { get; set; } = new Color(0, 0, 1, 0.3f);
    
    public override void _Ready()
    {
        _logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext<DragView>();
        _logger?.Debug("DragView _Ready called");
        
        InitializeServices();
        SetupVisualElements();
        SubscribeToEvents();
        InitializePresenter();
        
        _logger?.Debug("DragView initialized successfully");
    }
    
    private void InitializeServices()
    {
        var sceneRoot = GetNodeOrNull<SceneRoot>("/root/SceneRoot");
        if (sceneRoot?.ServiceProvider == null)
        {
            _logger?.Error("ServiceProvider not available");
            return;
        }
        
        _gridStateService = sceneRoot.ServiceProvider.GetService<IGridStateService>();
        if (_gridStateService == null)
        {
            _logger?.Error("GridStateService not available");
        }
        
        // Find interaction controller from parent GridView
        var gridView = GetParent();
        if (gridView != null)
        {
            _interactionController = gridView.GetNodeOrNull<GridInteractionController>("InteractionController");
            _visualizationController = gridView.GetNodeOrNull<BlockVisualizationController>("VisualizationController");
            
            if (_interactionController == null)
                _logger?.Warning("GridInteractionController not found");
            if (_visualizationController == null)
                _logger?.Warning("BlockVisualizationController not found");
        }
    }
    
    private void SetupVisualElements()
    {
        // Create ghost block for dragging feedback
        _ghostBlock = new Control();
        _ghostBlock.Name = "GhostBlock";
        _ghostBlock.SetSize(new Vector2(CellSize, CellSize));
        _ghostBlock.MouseFilter = Control.MouseFilterEnum.Ignore;
        _ghostBlock.Visible = false;
        AddChild(_ghostBlock);
        
        // Create preview indicator for drop location
        _previewIndicator = new Control();
        _previewIndicator.Name = "PreviewIndicator";
        _previewIndicator.SetSize(new Vector2(CellSize, CellSize));
        _previewIndicator.MouseFilter = Control.MouseFilterEnum.Ignore;
        _previewIndicator.Visible = false;
        AddChild(_previewIndicator);
        
        // Create range indicator (for Phase 2)
        _rangeIndicator = new Control();
        _rangeIndicator.Name = "RangeIndicator";
        _rangeIndicator.MouseFilter = Control.MouseFilterEnum.Ignore;
        _rangeIndicator.Visible = false;
        AddChild(_rangeIndicator);
        
        // Create invalid drop feedback label
        _invalidDropLabel = new Label();
        _invalidDropLabel.Name = "InvalidDropLabel";
        _invalidDropLabel.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
        _invalidDropLabel.Visible = false;
        AddChild(_invalidDropLabel);
    }
    
    private void SubscribeToEvents()
    {
        if (_interactionController == null) return;
        
        // Subscribe to drag events from GridInteractionController
        _dragStartSubscription = _interactionController.DragStarted
            .Subscribe(OnDragStart);
            
        _dragMoveSubscription = _interactionController.DragMoved
            .Subscribe(OnDragMove);
            
        _dragEndSubscription = _interactionController.DragEnded
            .Subscribe(OnDragEnd);
    }
    
    private void InitializePresenter()
    {
        var sceneRoot = GetNodeOrNull<SceneRoot>("/root/SceneRoot");
        if (sceneRoot != null)
        {
            _presenter = sceneRoot.CreatePresenterFor<DragPresenter, IDragView>(this);
            _presenter?.Initialize();
            _logger?.Debug("DragPresenter created and initialized");
        }
        else
        {
            _logger?.Warning("SceneRoot not found. Presenter creation skipped.");
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        // Handle ESC or right-click for cancel
        if (_isDragging)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                CancelCurrentDrag();
                GetViewport().SetInputAsHandled();
            }
            else if (@event is InputEventMouseButton mouseButton && 
                     mouseButton.Pressed && 
                     mouseButton.ButtonIndex == MouseButton.Right)
            {
                CancelCurrentDrag();
                GetViewport().SetInputAsHandled();
            }
        }
    }
    
    private void OnDragStart(Vector2Int position)
    {
        // Check if there's a block at this position
        if (_gridStateService == null) return;
        
        var blockAtPosition = _gridStateService.GetBlockAt(position);
        blockAtPosition.IfSome(block =>
        {
            _logger?.Debug("Starting drag for block {BlockId} at position {Position}", block.Id, position);
            
            _currentDraggedBlockId = Option<Guid>.Some(block.Id);
            _dragStartPosition = position;
            _isDragging = true;
            
            // Raise event for presenter
            DragStarted?.Invoke(block.Id, position);
        });
    }
    
    private void OnDragMove(Vector2Int position)
    {
        if (!_isDragging) return;
        
        _logger?.Verbose("Drag moved to position {Position}", position);
        
        // Raise event for presenter
        DragUpdated?.Invoke(position);
    }
    
    private void OnDragEnd(Vector2Int position)
    {
        if (!_isDragging) return;
        
        _currentDraggedBlockId.IfSome(blockId =>
        {
            _logger?.Debug("Ending drag for block {BlockId} at position {Position}", blockId, position);
            
            // Raise event for presenter
            DragCompleted?.Invoke(blockId, position);
            
            // Reset drag state
            _isDragging = false;
            _currentDraggedBlockId = Option<Guid>.None;
        });
    }
    
    private void CancelCurrentDrag()
    {
        if (!_isDragging) return;
        
        _currentDraggedBlockId.IfSome(blockId =>
        {
            _logger?.Debug("Cancelling drag for block {BlockId}", blockId);
            
            // Raise event for presenter
            DragCancelled?.Invoke(blockId);
            
            // Reset drag state
            _isDragging = false;
            _currentDraggedBlockId = Option<Guid>.None;
        });
    }
    
    // IDragView implementation
    public void ShowDragFeedback(Guid blockId, Vector2Int startPosition)
    {
        _logger?.Debug("Showing drag feedback for block {BlockId}", blockId);
        
        if (_ghostBlock != null)
        {
            _ghostBlock.Position = GridToScreenPosition(startPosition);
            _ghostBlock.Modulate = GhostBlockColor;
            _ghostBlock.Visible = true;
        }
        
        // Note: Block opacity modification would go here once supported by BlockVisualizationController
        // For now, the ghost block provides visual feedback
    }
    
    public void UpdateDragPreview(Vector2Int previewPosition, bool isValidDrop)
    {
        if (_previewIndicator != null)
        {
            _previewIndicator.Position = GridToScreenPosition(previewPosition);
            _previewIndicator.Modulate = isValidDrop ? ValidDropColor : InvalidDropColor;
            _previewIndicator.Visible = true;
        }
        
        // Update ghost block position to follow cursor
        if (_ghostBlock != null)
        {
            _ghostBlock.Position = GridToScreenPosition(previewPosition);
        }
        
        // Force redraw to show the preview
        QueueRedraw();
    }
    
    public void HideDragFeedback()
    {
        _logger?.Debug("Hiding drag feedback");
        
        if (_ghostBlock != null)
            _ghostBlock.Visible = false;
            
        if (_previewIndicator != null)
            _previewIndicator.Visible = false;
            
        if (_invalidDropLabel != null)
            _invalidDropLabel.Visible = false;
        
        // Note: Block opacity restoration would go here once supported
        // The visual feedback is cleared by hiding the ghost block
        
        QueueRedraw();
    }
    
    public void ShowRangeIndicators(Vector2Int centerPosition, int range)
    {
        _logger?.Debug("Showing range indicators at {Position} with range {Range}", centerPosition, range);
        
        if (_rangeIndicator != null)
        {
            var rangeSize = (range * 2 + 1) * CellSize;
            _rangeIndicator.Position = GridToScreenPosition(
                new Vector2Int(centerPosition.X - range, centerPosition.Y - range));
            _rangeIndicator.SetSize(new Vector2(rangeSize, rangeSize));
            _rangeIndicator.Modulate = RangeIndicatorColor;
            _rangeIndicator.Visible = true;
        }
        
        QueueRedraw();
    }
    
    public void HideRangeIndicators()
    {
        if (_rangeIndicator != null)
            _rangeIndicator.Visible = false;
            
        QueueRedraw();
    }
    
    public void ShowInvalidDropFeedback(string reason)
    {
        _logger?.Debug("Showing invalid drop feedback: {Reason}", reason);
        
        if (_invalidDropLabel != null)
        {
            _invalidDropLabel.Text = reason;
            _invalidDropLabel.Position = GetGlobalMousePosition() + new Vector2(10, -30);
            _invalidDropLabel.Visible = true;
            
            // Hide after a short delay
            GetTree().CreateTimer(2.0).Timeout += () =>
            {
                if (_invalidDropLabel != null)
                    _invalidDropLabel.Visible = false;
            };
        }
    }
    
    public void AnimateReturnToOriginal(Guid blockId, Vector2Int originalPosition)
    {
        _logger?.Debug("Animating block {BlockId} return to original position {Position}", 
            blockId, originalPosition);
        
        // Update block position visually
        _visualizationController?.UpdateBlockPositionAsync(blockId, originalPosition);
        
        HideDragFeedback();
    }
    
    public void AnimateSuccessfulDrop(Guid blockId, Vector2Int fromPosition, Vector2Int toPosition)
    {
        _logger?.Debug("Animating successful drop for block {BlockId} from {From} to {To}", 
            blockId, fromPosition, toPosition);
        
        // Update block position visually
        _visualizationController?.UpdateBlockPositionAsync(blockId, toPosition);
        
        HideDragFeedback();
    }
    
    // Helper methods
    private Vector2 GridToScreenPosition(Vector2Int gridPosition)
    {
        return new Vector2(gridPosition.X * CellSize, gridPosition.Y * CellSize);
    }
    
    public override void _Draw()
    {
        // Draw ghost block
        if (_ghostBlock != null && _ghostBlock.Visible)
        {
            var rect = new Rect2(_ghostBlock.Position, _ghostBlock.Size);
            DrawRect(rect, _ghostBlock.Modulate);
        }
        
        // Draw preview indicator
        if (_previewIndicator != null && _previewIndicator.Visible)
        {
            var rect = new Rect2(_previewIndicator.Position, _previewIndicator.Size);
            DrawRect(rect, _previewIndicator.Modulate, false, 3.0f);
        }
        
        // Draw range indicator
        if (_rangeIndicator != null && _rangeIndicator.Visible)
        {
            var rect = new Rect2(_rangeIndicator.Position, _rangeIndicator.Size);
            DrawRect(rect, _rangeIndicator.Modulate);
            DrawRect(rect, RangeIndicatorColor * 2, false, 2.0f); // Border
        }
    }
    
    public override void _ExitTree()
    {
        _dragStartSubscription?.Dispose();
        _dragMoveSubscription?.Dispose();
        _dragEndSubscription?.Dispose();
        _presenter?.Dispose();
        
        base._ExitTree();
    }
}