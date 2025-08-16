using BlockLife.Core.Domain.Common;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Block.Placement;

public interface IBlockManagementView
{
    // Capability Sub-Views
    IBlockVisualizationView Visualization { get; }
    IGridInteractionView Interaction { get; }

    // View Lifecycle
    Task InitializeAsync(Vector2Int gridSize);
    Task CleanupAsync();

    // Global State
    bool IsInitialized { get; }
    Vector2Int GridSize { get; }
}
