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
                _logger?.Debug("Placing block at {Position}", position);
                await PlaceBlockAt(position);
            },
            None: () =>
            {
                _logger?.Debug("No position hovered - move cursor over grid to place block");
                return Task.CompletedTask;
            }
        );
    }
    
    private async Task PlaceBlockAt(Vector2Int position)
    {
        var command = new PlaceBlockCommand(position, BlockType.Basic);
        var result = await _mediator.Send(command);
        
        result.Match(
            Succ: _ => _logger?.Information("Block placed successfully at {Position}", position),
            Fail: error => _logger?.Warning("Failed to place block at {Position}: {Error}", 
                position, error.Message)
        );
    }
}