using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using LanguageExt;
using System;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Block.Placement;

public interface IBlockVisualizationView
{
    // Block Rendering
    Task ShowBlockAsync(Guid blockId, Vector2Int position, BlockType type);
    Task HideBlockAsync(Guid blockId);
    Task UpdateBlockPositionAsync(Guid blockId, Vector2Int newPosition);
    
    // Visual Feedback
    Task ShowPlacementPreviewAsync(Vector2Int position, BlockType type);
    Task HidePlacementPreviewAsync();
    Task ShowInvalidPlacementFeedbackAsync(Vector2Int position, string reason);
    Task ShowValidPlacementFeedbackAsync(Vector2Int position);
    
    // State Queries
    bool IsBlockVisible(Guid blockId);
    Option<Vector2Int> GetBlockVisualPosition(Guid blockId);
}