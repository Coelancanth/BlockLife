using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement.Effects;
using BlockLife.Core.Features.Block.Placement.Notifications;
using BlockLife.Core.Presentation;
using BlockLife.Features.Block.Move.Effects;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.Logging;
// Explicit using for BlockMovedNotification from the correct namespace
using BlockMovedNotification = BlockLife.Core.Features.Block.Effects.BlockMovedNotification;

namespace BlockLife.Core.Features.Block.Placement;

public class BlockManagementPresenter : PresenterBase<IBlockManagementView>
{
    private readonly IMediator _mediator;
    private readonly ILogger<BlockManagementPresenter> _logger;
    private readonly CompositeDisposable _subscriptions = new();
    private IDisposable? _blockPlacedSubscription;
    private IDisposable? _blockRemovedSubscription;
    private IDisposable? _blockMovedSubscription;

    // Track current placement mode (for future enhancement)
    private BlockType _currentBlockType = BlockType.Basic;

    // Control automatic placement behavior - can be disabled for manual placement control
    public bool AutoPlaceOnClick { get; set; } = false; // Changed default to false

    public BlockManagementPresenter(
        IBlockManagementView view,
        IMediator mediator,
        ILogger<BlockManagementPresenter> logger) : base(view)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Initialize()
    {
        // Trace: Initializing BlockManagementPresenter (controlled by UI log level)

        // Subscribe to input events
        _subscriptions.Add(View.Interaction.GridCellClicked
            .Subscribe(OnGridCellClicked));

        _subscriptions.Add(View.Interaction.GridCellHovered
            .Subscribe(OnGridCellHovered));

        _subscriptions.Add(View.Interaction.GridCellExited
            .Subscribe(OnGridCellExited));

        // Subscribe to MediatR notifications via the bridge using weak events
        _blockPlacedSubscription = BlockPlacementNotificationBridge.SubscribeToBlockPlaced(OnBlockPlacedNotification);
        _blockRemovedSubscription = BlockPlacementNotificationBridge.SubscribeToBlockRemoved(OnBlockRemovedNotification);
        _blockMovedSubscription = BlockMovementNotificationBridge.SubscribeToBlockMoved(OnBlockMovedNotification);
        // Trace: Subscribed to BlockPlacementNotificationBridge and BlockMovementNotificationBridge events using thread-safe weak events

        // Initialize the view with default grid size
        _ = View.InitializeAsync(new Vector2Int(10, 10)); // TODO: Get from config

        // Trace: BlockManagementPresenter initialized successfully
    }

    public override void Dispose()
    {
        _logger.LogDebug("Disposing BlockManagementPresenter");

        // Dispose weak event subscriptions
        _blockPlacedSubscription?.Dispose();
        _blockRemovedSubscription?.Dispose();
        _blockMovedSubscription?.Dispose();

        _subscriptions.Dispose();
        _ = View.CleanupAsync();

        base.Dispose();
    }

    // Input Handlers
    private async void OnGridCellClicked(Vector2Int position)
    {
        // Only handle automatic placement if enabled
        if (!AutoPlaceOnClick)
        {
            // Automatic placement disabled - clicks are handled by other input managers
            return;
        }

        try
        {
            // Legacy behavior: left click places a block
            var command = new PlaceBlockCommand(position, _currentBlockType);
            var result = await _mediator.Send(command);

            result.Match(
                Succ: _ =>
                {
                    _logger.LogDebug("Block placed successfully at {Position}", position);
                },
                Fail: error =>
                {
                    HandlePlacementError(position, error);
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in OnGridCellClicked at {Position}", position);
        }
    }

    private async void OnGridCellHovered(Vector2Int position)
    {
        try
        {
            // Show placement preview
            await View.Visualization.ShowPlacementPreviewAsync(position, _currentBlockType);

            // TODO: Check if position is valid and show appropriate feedback
            // For now, always show as valid
            await View.Visualization.ShowValidPlacementFeedbackAsync(position);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling grid cell hover at {Position}", position);
        }
    }

    private async void OnGridCellExited(Vector2Int position)
    {
        try
        {
            await View.Visualization.HidePlacementPreviewAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling grid cell exit at {Position}", position);
        }
    }

    // Event Handlers (called via notification bridge)
    private async Task OnBlockPlacedNotification(BlockPlacedNotification notification)
    {
        // Trace: OnBlockPlacedNotification called for block {BlockId} at {Position}
        try
        {
            await View.Visualization.ShowBlockAsync(
                notification.BlockId,
                notification.Position,
                notification.Type
            );
            // Trace: ShowBlockAsync completed for block {BlockId}
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error displaying block {BlockId} at {Position}", notification.BlockId, notification.Position);
        }
    }

    private async Task OnBlockRemovedNotification(BlockRemovedNotification notification)
    {
        try
        {
            await View.Visualization.HideBlockAsync(notification.BlockId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing block {BlockId}", notification.BlockId);
        }
    }

    private async Task OnBlockMovedNotification(BlockMovedNotification notification)
    {
        try
        {
            _logger.LogDebug("Updating view for moved block {BlockId} from {FromPosition} to {ToPosition}",
                notification.BlockId, notification.FromPosition, notification.ToPosition);

            await View.Visualization.UpdateBlockPositionAsync(
                notification.BlockId,
                notification.ToPosition
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating position for moved block {BlockId}", notification.BlockId);
        }
    }

    // Notification Handlers (kept for backward compatibility - now called via events)
    public async Task HandleBlockPlaced(BlockPlacedNotification notification)
    {
        await OnBlockPlacedNotification(notification);
    }

    public async Task HandleBlockRemoved(BlockRemovedNotification notification)
    {
        await OnBlockRemovedNotification(notification);
    }

    // Error Handling
    private async void HandlePlacementError(Vector2Int position, Error error)
    {
        _logger.LogWarning("Block placement failed at {Position}: {Error}", position, error);

        try
        {
            await View.Visualization.ShowInvalidPlacementFeedbackAsync(position, error.Message);

            // Auto-hide error feedback after delay
            _ = Task.Delay(2000).ContinueWith(async _ =>
            {
                await View.Visualization.HidePlacementPreviewAsync();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling placement error feedback");
        }
    }

    // Public methods for changing placement mode (future enhancement)
    public void SetBlockType(BlockType type)
    {
        _currentBlockType = type;
        _logger.LogDebug("Block type changed to {BlockType}", type);
    }
}
