using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Commands;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Godot.Features.Block.Input.State;
using LanguageExt;
using static LanguageExt.Prelude;
using MediatR;
using Serilog;

namespace BlockLife.Godot.Features.Block.Input.Handlers;

/// <summary>
/// Handles block movement via click-to-select and click-to-move pattern.
/// First click selects a block, second click moves it to the target position.
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
    /// Handles cell clicks for block selection and movement.
    /// </summary>
    public async Task HandleCellClick(Vector2Int position)
    {
        if (_selectionManager.HasSelection)
        {
            await HandleMoveOrDeselect(position);
        }
        else
        {
            await HandleSelection(position);
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
    
    private async Task HandleSelection(Vector2Int position)
    {
        var blockAtPosition = GetBlockAt(position);
        
        await blockAtPosition.Match(
            Some: blockId =>
            {
                _logger?.Debug("Selected block {BlockId} at position {Position}", blockId, position);
                _selectionManager.SelectBlock(blockId, position);
                return Task.CompletedTask;
            },
            None: () =>
            {
                _logger?.Debug("No block at position {Position} to select", position);
                return Task.CompletedTask;
            }
        );
    }
    
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