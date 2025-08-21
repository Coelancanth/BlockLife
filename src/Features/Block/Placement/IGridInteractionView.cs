using BlockLife.Core.Domain.Common;
using LanguageExt;
using System;

namespace BlockLife.Core.Features.Block.Placement;

public interface IGridInteractionView
{
    // Input Events
    IObservable<Vector2Int> GridCellClicked { get; }
    IObservable<Vector2Int> GridCellHovered { get; }
    IObservable<Vector2Int> GridCellExited { get; }

    // Drag Events (Phase 1 Foundation)
    IObservable<Vector2Int> DragStarted { get; }
    IObservable<Vector2Int> DragMoved { get; }
    IObservable<Vector2Int> DragEnded { get; }

    // Input State
    bool IsInputEnabled { get; set; }
    Option<Vector2Int> HoveredCell { get; }

    // Coordinate Conversion (Vector2 is a placeholder - will be replaced with actual type in Godot layer)
    Vector2Int ScreenToGridPosition(object screenPosition);
    object GridToScreenPosition(Vector2Int gridPosition);
}
