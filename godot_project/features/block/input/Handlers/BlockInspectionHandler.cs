using BlockLife.Core.Domain.Common;
using BlockLife.Core.Infrastructure.Services;
using Godot;
using LanguageExt;
using Serilog;

namespace BlockLife.godot_project.features.block.input.Handlers;

/// <summary>
/// Handles block inspection input operations.
/// Provides detailed information about blocks at the cursor position.
/// </summary>
public sealed class BlockInspectionHandler
{
    private readonly IGridStateService _gridStateService;
    private readonly ILogger? _logger;

    public BlockInspectionHandler(IGridStateService gridStateService, ILogger? logger)
    {
        _gridStateService = gridStateService;
        _logger = logger?.ForContext<BlockInspectionHandler>();
    }

    /// <summary>
    /// Handles the inspect block key press.
    /// Logs detailed information about the block at the hover position.
    /// </summary>
    public void HandleInspectKey(Option<Vector2Int> hoverPosition)
    {
        hoverPosition.Match(
            Some: position =>
            {
                InspectPosition(position);
                return Unit.Default;
            },
            None: () =>
            {
                _logger?.Debug("No position hovered - move cursor over grid to inspect");
                return Unit.Default;
            }
        );
    }

    private void InspectPosition(Vector2Int position)
    {
        _logger?.Debug("Inspecting position {Position}", position);

        var blockAtPosition = _gridStateService.GetBlockAt(position);

        blockAtPosition.Match(
            Some: block =>
            {
                LogBlockInfo(block, position);
                PrintToConsole(block, position);
            },
            None: () =>
            {
                LogEmptyPosition(position);
                PrintEmptyToConsole(position);
            }
        );
    }

    private void LogBlockInfo(Core.Domain.Block.Block block, Vector2Int position)
    {
        _logger?.Information("BLOCK INFO at {Position}:", position);
        _logger?.Information("   BlockId: {BlockId}", block.Id);
        _logger?.Information("   Type: {BlockType}", block.Type);
        _logger?.Information("   Position: {Position}", block.Position);
        _logger?.Information("   CreatedAt: {CreatedAt}", block.CreatedAt);
        _logger?.Information("   LastModifiedAt: {LastModifiedAt}", block.LastModifiedAt);
    }

    private void PrintToConsole(Core.Domain.Block.Block block, Vector2Int position)
    {
        GD.Print($"=== BLOCK INSPECTION ===");
        GD.Print($"Position: ({position.X}, {position.Y})");
        GD.Print($"BlockId: {block.Id}");
        GD.Print($"Type: {block.Type}");
        GD.Print($"CreatedAt: {block.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        GD.Print($"LastModifiedAt: {block.LastModifiedAt:yyyy-MM-dd HH:mm:ss}");
        GD.Print($"========================");
    }

    private void LogEmptyPosition(Vector2Int position)
    {
        _logger?.Information("EMPTY POSITION at {Position}:", position);
        _logger?.Information("   No block present");
        _logger?.Information("   Is Valid Position: {IsValid}", _gridStateService.IsValidPosition(position));
    }

    private void PrintEmptyToConsole(Vector2Int position)
    {
        GD.Print($"=== POSITION INSPECTION ===");
        GD.Print($"Position: ({position.X}, {position.Y})");
        GD.Print($"Status: Empty");
        GD.Print($"Valid: {_gridStateService.IsValidPosition(position)}");
        GD.Print($"===========================");
    }
}
