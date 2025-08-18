using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using LanguageExt;
using MediatR;
using Serilog;

namespace BlockLife.Godot.Features.Block.Input.Handlers;

/// <summary>
/// Handles block placement input operations.
/// Responsible for placing blocks at the cursor position when the place key is pressed.
/// </summary>
public sealed class BlockPlacementHandler
{
    private readonly IMediator _mediator;
    private readonly ILogger? _logger;
    
    // Current block type for placement - can be cycled through
    private BlockType _currentBlockType = BlockType.Work;
    
    // Array of available block types for easy testing
    private readonly BlockType[] _availableTypes = new[]
    {
        BlockType.Work,      // Royal Blue
        BlockType.Study,     // Lime Green
        BlockType.Health,    // Tomato Red
        BlockType.Fun,       // Gold
        BlockType.Basic      // Gray
    };
    private int _currentTypeIndex = 0;
    
    public BlockPlacementHandler(IMediator mediator, ILogger? logger)
    {
        _mediator = mediator;
        _logger = logger?.ForContext<BlockPlacementHandler>();
    }
    
    /// <summary>
    /// Handles the place block key press.
    /// Places a block at the current hover position if valid.
    /// </summary>
    public async void HandlePlaceKey(Option<Vector2Int> hoverPosition)
    {
        await hoverPosition.Match(
            Some: async position =>
            {
                _logger?.Debug("Placing {BlockType} block at {Position}", _currentBlockType, position);
                await PlaceBlockAt(position);
            },
            None: () =>
            {
                _logger?.Debug("No position hovered - move cursor over grid to place block");
                return Task.CompletedTask;
            }
        );
    }
    
    /// <summary>
    /// Cycles to the next available block type for placement.
    /// Useful for testing different block types and swap mechanic.
    /// </summary>
    public void CycleBlockType()
    {
        _currentTypeIndex = (_currentTypeIndex + 1) % _availableTypes.Length;
        _currentBlockType = _availableTypes[_currentTypeIndex];
        _logger?.Information("Switched to placing {BlockType} blocks (Color: {Color})", 
            _currentBlockType.GetDisplayName(), 
            _currentBlockType.GetColorHex());
    }
    
    /// <summary>
    /// Gets the current block type that will be placed.
    /// </summary>
    public BlockType CurrentBlockType => _currentBlockType;
    
    private async Task PlaceBlockAt(Vector2Int position)
    {
        var command = new PlaceBlockCommand(position, _currentBlockType);
        var result = await _mediator.Send(command);
        
        result.Match(
            Succ: _ => _logger?.Information("{BlockType} block placed successfully at {Position}", 
                _currentBlockType.GetDisplayName(), position),
            Fail: error => _logger?.Warning("Failed to place {BlockType} block at {Position}: {Error}", 
                _currentBlockType.GetDisplayName(), position, error.Message)
        );
    }
}