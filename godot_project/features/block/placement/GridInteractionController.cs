using Godot;
using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Domain.Common;
using BlockLife.Godot.Scenes;
using LanguageExt;
using static LanguageExt.Prelude;

namespace BlockLife.Godot.Features.Block.Placement;

public partial class GridInteractionController : Control, IGridInteractionView
{
    [Export] public Vector2I GridSize { get; set; } = new(10, 10);
    [Export] public float CellSize { get; set; } = 64f;
    
    // Reactive Streams
    private readonly Subject<Vector2Int> _cellClicked = new();
    private readonly Subject<Vector2Int> _cellHovered = new();
    private readonly Subject<Vector2Int> _cellExited = new();
    
    private Option<Vector2Int> _hoveredCell = None;
    private bool _isInputEnabled = true;
    
    public override void _Ready()
    {
        // Enable mouse input
        MouseFilter = Control.MouseFilterEnum.Pass;
        
        // Set up control size based on grid
        CustomMinimumSize = new Vector2(GridSize.X * CellSize, GridSize.Y * CellSize);
        Size = CustomMinimumSize;
        
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Information("GridInteractionController ready with size {GridWidth}x{GridHeight}, cell size {CellSize}", GridSize.X, GridSize.Y, CellSize);
        
        // Force initial draw of the grid
        QueueRedraw();
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        if (!IsInputEnabled) 
        {
            // Trace: Input is disabled, ignoring event
            return;
        }
        
        switch (@event)
        {
            case InputEventMouseButton mouseButton when mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left:
                // Trace: Mouse click detected at position
                HandleMouseClick(mouseButton.Position);
                break;
                
            case InputEventMouseMotion mouseMotion:
                HandleMouseMotion(mouseMotion.Position);
                break;
        }
    }
    
    public override void _ExitTree()
    {
        _cellClicked.Dispose();
        _cellHovered.Dispose();
        _cellExited.Dispose();
    }
    
    public override void _Notification(int what)
    {
        if (what == NotificationMouseExit)
        {
            // Handle mouse leaving the control entirely
            _hoveredCell.IfSome(pos => _cellExited.OnNext(pos));
            _hoveredCell = None;
            
            // Redraw to clear hover highlight
            QueueRedraw();
        }
    }
    
    private void HandleMouseClick(Vector2 mousePosition)
    {
        var gridPosition = ScreenToGridPositionInternal(mousePosition);
        // Trace: HandleMouseClick - mousePosition and gridPosition
        
        if (IsValidGridPosition(gridPosition))
        {
            // Trace: Valid grid position, emitting cell clicked event
            var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
            logger?.Information("🎯 Grid cell clicked at ({X}, {Y})", gridPosition.X, gridPosition.Y);
            _cellClicked.OnNext(gridPosition);
        }
        else
        {
            // Trace: Invalid grid position, ignoring click
        }
    }
    
    private void HandleMouseMotion(Vector2 mousePosition)
    {
        var gridPosition = ScreenToGridPositionInternal(mousePosition);
        
        if (IsValidGridPosition(gridPosition))
        {
            // Check if we're entering a new cell
            var isNewCell = _hoveredCell.Match(
                Some: pos => pos.X != gridPosition.X || pos.Y != gridPosition.Y,
                None: () => true
            );
            
            if (isNewCell)
            {
                // Exited previous cell
                _hoveredCell.IfSome(pos => _cellExited.OnNext(pos));
                
                // Entered new cell
                _hoveredCell = Some(gridPosition);
                _cellHovered.OnNext(gridPosition);
                
                // Redraw to update hover highlight
                QueueRedraw();
            }
        }
        else
        {
            // Exited grid entirely
            _hoveredCell.IfSome(pos => _cellExited.OnNext(pos));
            _hoveredCell = None;
            
            // Redraw to clear hover highlight
            QueueRedraw();
        }
    }
    
    private bool IsValidGridPosition(Vector2Int position) =>
        position.X >= 0 && position.Y >= 0 &&
        position.X < GridSize.X && position.Y < GridSize.Y;
    
    // Interface Implementation
    public IObservable<Vector2Int> GridCellClicked => _cellClicked.AsObservable();
    public IObservable<Vector2Int> GridCellHovered => _cellHovered.AsObservable();
    public IObservable<Vector2Int> GridCellExited => _cellExited.AsObservable();
    
    public bool IsInputEnabled
    {
        get => _isInputEnabled;
        set => _isInputEnabled = value;
    }
    
    public Option<Vector2Int> HoveredCell => _hoveredCell;
    
    public Vector2Int ScreenToGridPosition(object screenPosition)
    {
        if (screenPosition is Vector2 vec2)
        {
            return new Vector2Int(
                Mathf.FloorToInt(vec2.X / CellSize),
                Mathf.FloorToInt(vec2.Y / CellSize)
            );
        }
        throw new ArgumentException("screenPosition must be a Godot Vector2");
    }
    
    public object GridToScreenPosition(Vector2Int gridPosition) =>
        new Vector2(gridPosition.X * CellSize, gridPosition.Y * CellSize);
    
    // Internal helper that uses actual types
    private Vector2Int ScreenToGridPositionInternal(Vector2 screenPosition) =>
        new(
            Mathf.FloorToInt(screenPosition.X / CellSize),
            Mathf.FloorToInt(screenPosition.Y / CellSize)
        );
    
    private Vector2 GridToScreenPositionInternal(Vector2Int gridPosition) =>
        new(gridPosition.X * CellSize, gridPosition.Y * CellSize);
    
    // Debug Drawing (optional - helps visualize grid)
    public override void _Draw()
    {
        // Draw grid lines for debugging
        var gridColor = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Lighter and more opaque
        var lineWidth = 2.0f; // Thicker lines for visibility
        
        // Vertical lines
        for (int x = 0; x <= GridSize.X; x++)
        {
            var xPos = x * CellSize;
            DrawLine(new Vector2(xPos, 0), new Vector2(xPos, GridSize.Y * CellSize), gridColor, lineWidth);
        }
        
        // Horizontal lines  
        for (int y = 0; y <= GridSize.Y; y++)
        {
            var yPos = y * CellSize;
            DrawLine(new Vector2(0, yPos), new Vector2(GridSize.X * CellSize, yPos), gridColor, lineWidth);
        }
        
        // Draw border with thicker lines
        var borderColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
        var borderWidth = 3.0f;
        var maxX = GridSize.X * CellSize;
        var maxY = GridSize.Y * CellSize;
        DrawLine(new Vector2(0, 0), new Vector2(maxX, 0), borderColor, borderWidth);
        DrawLine(new Vector2(maxX, 0), new Vector2(maxX, maxY), borderColor, borderWidth);
        DrawLine(new Vector2(maxX, maxY), new Vector2(0, maxY), borderColor, borderWidth);
        DrawLine(new Vector2(0, maxY), new Vector2(0, 0), borderColor, borderWidth);
        
        // Highlight hovered cell
        _hoveredCell.IfSome(pos =>
        {
            var rect = new Rect2(
                GridToScreenPositionInternal(pos),
                new Vector2(CellSize, CellSize)
            );
            DrawRect(rect, new Color(1, 1, 1, 0.2f)); // More visible highlight
        });
    }
    
    // Force redraw when hover changes
    private void OnHoverChanged()
    {
        QueueRedraw();
    }
}