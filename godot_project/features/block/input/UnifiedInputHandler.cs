using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Commands;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Infrastructure.Services;
using Godot;
using LanguageExt;
using MediatR;
using Serilog;

namespace BlockLife.godot_project.features.block.input;

/// <summary>
/// Unified handler for all block input operations.
/// Consolidates placement, movement, and inspection into a single cohesive handler.
/// </summary>
public sealed class UnifiedInputHandler : IDisposable
{
    private readonly IMediator _mediator;
    private readonly IGridStateService _gridStateService;
    private readonly InputStateManager _stateManager;
    private readonly ILogger? _logger;
    
    // Block type management
    private BlockType _currentBlockType = BlockType.Work;
    private readonly BlockType[] _availableTypes = new[]
    {
        BlockType.Work,      // Royal Blue
        BlockType.Study,     // Lime Green
        BlockType.Health,    // Tomato Red
        BlockType.Fun,       // Gold
        BlockType.Basic      // Gray
    };
    private int _currentTypeIndex = 0;
    
    public BlockType CurrentBlockType => _currentBlockType;
    
    public UnifiedInputHandler(
        IMediator mediator, 
        IGridStateService gridStateService,
        InputStateManager stateManager,
        ILogger? logger)
    {
        _mediator = mediator;
        _gridStateService = gridStateService;
        _stateManager = stateManager;
        _logger = logger?.ForContext<UnifiedInputHandler>();
    }
    
    /// <summary>
    /// Handles block placement at the current hover position.
    /// </summary>
    public async Task HandlePlaceBlock()
    {
        await _stateManager.CurrentHoverPosition.Match(
            Some: async position =>
            {
                _logger?.Debug("Placing {BlockType} block at {Position}", _currentBlockType, position);
                
                var command = new PlaceBlockCommand(position, _currentBlockType);
                var result = await _mediator.Send(command);
                
                result.Match(
                    Succ: _ => _logger?.Information("{BlockType} block placed at {Position}", 
                        _currentBlockType.GetDisplayName(), position),
                    Fail: error => _logger?.Warning("Failed to place block: {Error}", error.Message)
                );
            },
            None: () =>
            {
                _logger?.Debug("No position hovered - move cursor over grid to place block");
                return Task.CompletedTask;
            }
        );
    }
    
    /// <summary>
    /// Handles block inspection at the current hover position.
    /// </summary>
    public void HandleInspectBlock()
    {
        _stateManager.CurrentHoverPosition.Match(
            Some: position => 
            {
                InspectPosition(position);
                return LanguageExt.Unit.Default;
            },
            None: () =>
            {
                _logger?.Debug("No position hovered - move cursor over grid to inspect");
                return LanguageExt.Unit.Default;
            }
        );
    }
    
    /// <summary>
    /// Handles cell click for movement completion or deselection.
    /// With drag-to-move, this primarily handles placing dragged blocks.
    /// </summary>
    public async Task HandleCellClick(Vector2Int position)
    {
        if (_stateManager.HasSelection)
        {
            // Complete movement or deselect
            if (_stateManager.CanMoveSelectedTo(position))
            {
                await _stateManager.SelectedBlockId.Match(
                    Some: async blockId =>
                    {
                        _logger?.Debug("Moving block {BlockId} to {Position}", blockId, position);
                        
                        var command = new MoveBlockCommand
                        {
                            BlockId = blockId,
                            ToPosition = position
                        };
                        
                        var result = await _mediator.Send(command);
                        
                        result.Match(
                            Succ: _ => 
                            {
                                _logger?.Information("Moved block {BlockId} to {Position}", blockId, position);
                                _stateManager.ClearSelection();
                            },
                            Fail: error => _logger?.Warning("Failed to move block: {Error}", error.Message)
                        );
                    },
                    None: () => Task.CompletedTask
                );
            }
            else
            {
                // Clicked same position - deselect
                _logger?.Debug("Deselecting block");
                _stateManager.ClearSelection();
            }
        }
        else
        {
            // With drag-to-move, clicking without selection just logs
            var blockAtPosition = GetBlockAt(position);
            blockAtPosition.IfSome(blockId =>
                _logger?.Debug("Click on block {BlockId} at {Position} - use drag to move", blockId, position)
            );
        }
    }
    
    /// <summary>
    /// Cycles to the next available block type for placement.
    /// </summary>
    public void CycleBlockType()
    {
        _currentTypeIndex = (_currentTypeIndex + 1) % _availableTypes.Length;
        _currentBlockType = _availableTypes[_currentTypeIndex];
        _logger?.Information("Switched to {BlockType} blocks", _currentBlockType.GetDisplayName());
    }
    
    /// <summary>
    /// Updates the hover position for the handler.
    /// </summary>
    public void UpdateHoverPosition(Vector2Int position)
    {
        _stateManager.UpdateHoverPosition(position);
    }
    
    /// <summary>
    /// Clears the hover position when cursor leaves grid.
    /// </summary>
    public void ClearHoverPosition()
    {
        _stateManager.ClearHoverPosition();
    }
    
    /// <summary>
    /// Selects a block for movement operations.
    /// </summary>
    public void SelectBlock(Guid blockId, Vector2Int position)
    {
        _stateManager.SelectBlock(blockId, position);
    }
    
    /// <summary>
    /// Clears the current block selection.
    /// </summary>
    public void ClearSelection()
    {
        _stateManager.ClearSelection();
    }
    
    private void InspectPosition(Vector2Int position)
    {
        _logger?.Debug("Inspecting position {Position}", position);
        
        var blockAtPosition = _gridStateService.GetBlockAt(position);
        
        blockAtPosition.Match(
            Some: block =>
            {
                // Log detailed block information
                _logger?.Information("BLOCK INFO at {Position}:", position);
                _logger?.Information("   BlockId: {BlockId}", block.Id);
                _logger?.Information("   Type: {BlockType}", block.Type);
                _logger?.Information("   CreatedAt: {CreatedAt}", block.CreatedAt);
                
                // Also print to Godot console for visibility
                GD.Print($"=== BLOCK INSPECTION ===");
                GD.Print($"Position: ({position.X}, {position.Y})");
                GD.Print($"BlockId: {block.Id}");
                GD.Print($"Type: {block.Type}");
                GD.Print($"========================");
            },
            None: () =>
            {
                _logger?.Information("EMPTY POSITION at {Position}", position);
                GD.Print($"=== POSITION INSPECTION ===");
                GD.Print($"Position: ({position.X}, {position.Y})");
                GD.Print($"Status: Empty");
                GD.Print($"===========================");
            }
        );
    }
    
    private Option<Guid> GetBlockAt(Vector2Int position)
    {
        return _gridStateService
            .GetBlockAt(position)
            .Map(block => block.Id);
    }
    
    public void Dispose()
    {
        _stateManager?.Dispose();
    }
}