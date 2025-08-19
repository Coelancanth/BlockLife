using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Commands;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.godot_project.features.block.input.State;
using LanguageExt;
using MediatR;
using Serilog;

namespace BlockLife.godot_project.features.block.input.Handlers;

/// <summary>
/// Handles block movement completion after drag operations.
/// With drag-to-move system, this only handles placing blocks after dragging,
/// not the initial selection (which is now handled by drag).
/// </summary>
public sealed class BlockMovementHandler
{
    private readonly IMediator _mediator;
    private readonly IGridStateService _gridStateService;
    private readonly BlockSelectionManager _selectionManager;
    private readonly ILogger? _logger;
    
    public BlockMovementHandler(
        IMediator mediator, 
        IGridStateService gridStateService,
        BlockSelectionManager selectionManager,
        ILogger? logger)
    {
        _mediator = mediator;
        _gridStateService = gridStateService;
        _selectionManager = selectionManager;
        _logger = logger?.ForContext<BlockMovementHandler>();
    }
    
    /// <summary>
    /// Handles cell clicks for block movement completion.
    /// With drag-to-move, clicking on a block no longer selects it (drag does that).
    /// This only handles placing a block that's already selected via drag.
    /// </summary>
    public async Task HandleCellClick(Vector2Int position)
    {
        if (_selectionManager.HasSelection)
        {
            await HandleMoveOrDeselect(position);
        }
        else
        {
            // With drag-to-move, we no longer select blocks via click
            // Just log that a click occurred on a block (drag handles selection)
            var blockAtPosition = GetBlockAt(position);
            blockAtPosition.IfSome(blockId =>
                _logger?.Debug("Click on block {BlockId} at {Position} - use drag to move blocks", 
                    blockId, position)
            );
        }
    }
    
    private async Task HandleMoveOrDeselect(Vector2Int position)
    {
        if (_selectionManager.CanMoveSelectedTo(position))
        {
            // Move the selected block
            await _selectionManager.SelectedBlockId.Match(
                Some: async blockId =>
                {
                    _logger?.Debug("Moving block {BlockId} to position {Position}", blockId, position);
                    await MoveBlock(blockId, position);
                    _selectionManager.ClearSelection();
                },
                None: () => Task.CompletedTask
            );
        }
        else
        {
            // Clicked same position - deselect
            _logger?.Debug("Deselecting block at {Position}", position);
            _selectionManager.ClearSelection();
        }
    }
    
    // HandleSelection method removed - selection is now handled by drag-to-move
    
    private async Task MoveBlock(Guid blockId, Vector2Int toPosition)
    {
        var command = new MoveBlockCommand
        {
            BlockId = blockId,
            ToPosition = toPosition
        };
        
        _logger?.Information("Sending MoveBlockCommand for block {BlockId} to position {ToPosition}", 
            blockId, toPosition);
        
        try
        {
            var result = await _mediator.Send(command);
            
            result.Match(
                Succ: _ => _logger?.Debug("Successfully moved block {BlockId} to {ToPosition}", 
                    blockId, toPosition),
                Fail: error => _logger?.Warning("Failed to move block {BlockId}: {Error}", 
                    blockId, error.Message)
            );
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Unexpected error moving block {BlockId}", blockId);
        }
    }
    
    private Option<Guid> GetBlockAt(Vector2Int position)
    {
        return _gridStateService
            .GetBlockAt(position)
            .Map(block => block.Id);
    }
}