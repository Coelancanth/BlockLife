using System;
using BlockLife.Core.Domain.Common;
using LanguageExt;
using static LanguageExt.Prelude;
using Serilog;

namespace BlockLife.Godot.Features.Block.Input.State;

/// <summary>
/// Manages block selection and hover state for input operations.
/// Single source of truth for what block is selected and where the cursor is.
/// </summary>
public sealed class BlockSelectionManager : IDisposable
{
    private readonly ILogger? _logger;
    
    // Selection state
    private Option<Guid> _selectedBlockId = None;
    private Option<Vector2Int> _selectedBlockPosition = None;
    
    // Hover state
    private Option<Vector2Int> _currentHoverPosition = None;
    
    public Option<Guid> SelectedBlockId => _selectedBlockId;
    public Option<Vector2Int> SelectedBlockPosition => _selectedBlockPosition;
    public Option<Vector2Int> CurrentHoverPosition => _currentHoverPosition;
    
    public bool HasSelection => _selectedBlockId.IsSome;
    
    public BlockSelectionManager(ILogger? logger)
    {
        _logger = logger?.ForContext<BlockSelectionManager>();
    }
    
    /// <summary>
    /// Selects a block at the given position.
    /// </summary>
    public void SelectBlock(Guid blockId, Vector2Int position)
    {
        _selectedBlockId = Some(blockId);
        _selectedBlockPosition = Some(position);
        _logger?.Debug("Block {BlockId} selected at position {Position}", blockId, position);
    }
    
    /// <summary>
    /// Clears the current selection.
    /// </summary>
    public void ClearSelection()
    {
        var hadSelection = _selectedBlockId.IsSome;
        _selectedBlockId = None;
        _selectedBlockPosition = None;
        
        if (hadSelection)
            _logger?.Debug("Block selection cleared");
    }
    
    /// <summary>
    /// Updates the current hover position.
    /// </summary>
    public void UpdateHoverPosition(Vector2Int position)
    {
        _currentHoverPosition = Some(position);
    }
    
    /// <summary>
    /// Clears the hover position when cursor leaves the grid.
    /// </summary>
    public void ClearHoverPosition()
    {
        _currentHoverPosition = None;
    }
    
    /// <summary>
    /// Checks if the selected block can move to the target position.
    /// </summary>
    public bool CanMoveSelectedTo(Vector2Int targetPosition)
    {
        return _selectedBlockPosition.Match(
            Some: pos => pos != targetPosition,
            None: () => false
        );
    }
    
    public void Dispose()
    {
        ClearSelection();
        ClearHoverPosition();
    }
}