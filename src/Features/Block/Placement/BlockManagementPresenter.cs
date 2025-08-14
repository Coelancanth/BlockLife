using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement.Effects;
using BlockLife.Core.Features.Block.Placement.Notifications;
using BlockLife.Core.Presentation;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlockLife.Core.Features.Block.Placement;

public class BlockManagementPresenter : PresenterBase<IBlockManagementView>
{
    private readonly IMediator _mediator;
    private readonly ILogger<BlockManagementPresenter> _logger;
    private readonly CompositeDisposable _subscriptions = new();
    
    // Track current placement mode (for future enhancement)
    private BlockType _currentBlockType = BlockType.Basic;
    
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
        _logger.LogInformation("[TRACE] Initializing BlockManagementPresenter");
        
        // Subscribe to input events
        _subscriptions.Add(View.Interaction.GridCellClicked
            .Subscribe(OnGridCellClicked));
            
        _subscriptions.Add(View.Interaction.GridCellHovered
            .Subscribe(OnGridCellHovered));
            
        _subscriptions.Add(View.Interaction.GridCellExited
            .Subscribe(OnGridCellExited));
        
        // Subscribe to MediatR notifications via the bridge
        BlockPlacementNotificationBridge.BlockPlacedEvent += OnBlockPlacedNotification;
        BlockPlacementNotificationBridge.BlockRemovedEvent += OnBlockRemovedNotification;
        _logger.LogInformation("[TRACE] Subscribed to BlockPlacementNotificationBridge events");
        
        // Initialize the view with default grid size
        _ = View.InitializeAsync(new Vector2Int(10, 10)); // TODO: Get from config
        
        _logger.LogInformation("[TRACE] BlockManagementPresenter initialized successfully");
    }
    
    public override void Dispose()
    {
        _logger.LogDebug("Disposing BlockManagementPresenter");
        
        // Unsubscribe from notification events
        BlockPlacementNotificationBridge.BlockPlacedEvent -= OnBlockPlacedNotification;
        BlockPlacementNotificationBridge.BlockRemovedEvent -= OnBlockRemovedNotification;
        
        _subscriptions.Dispose();
        _ = View.CleanupAsync();
        
        base.Dispose();
    }
    
    // Input Handlers
    private async void OnGridCellClicked(Vector2Int position)
    {
        _logger.LogInformation("[TRACE] Grid cell clicked at {Position}", position);
        
        try
        {
            // For now, left click places a block
            // In the future, we could check for modifier keys for different actions
            var command = new PlaceBlockCommand(position, _currentBlockType);
            _logger.LogInformation("[TRACE] Sending PlaceBlockCommand for {Position}", position);
            var result = await _mediator.Send(command);
            
            result.Match(
                Succ: _ => 
                {
                    _logger.LogInformation("[TRACE] Block placed successfully at {Position}", position);
                },
                Fail: error => 
                {
                    _logger.LogWarning("[TRACE] Block placement failed at {Position}: {Error}", position, error);
                    HandlePlacementError(position, error);
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TRACE] Exception in OnGridCellClicked at {Position}", position);
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
        _logger.LogInformation("[TRACE] OnBlockPlacedNotification called for block {BlockId} at {Position}", notification.BlockId, notification.Position);
        try
        {
            await View.Visualization.ShowBlockAsync(
                notification.BlockId,
                notification.Position,
                notification.Type
            );
            _logger.LogInformation("[TRACE] ShowBlockAsync completed for block {BlockId}", notification.BlockId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TRACE] Error displaying block {BlockId} at {Position}", notification.BlockId, notification.Position);
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