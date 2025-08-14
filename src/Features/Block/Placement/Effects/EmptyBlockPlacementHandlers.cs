using System;
using System.Threading;
using System.Threading.Tasks;
using BlockLife.Core.Features.Block.Placement.Notifications;
using BlockLife.Core.Infrastructure.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlockLife.Core.Features.Block.Placement.Effects;

/// <summary>
/// Notification handlers that receive MediatR notifications and forward them to presenters.
/// This solves the architectural constraint where presenters cannot implement INotificationHandler directly.
/// Uses thread-safe weak event pattern to prevent memory leaks and ensure thread safety.
/// </summary>
public class BlockPlacementNotificationBridge :
    INotificationHandler<BlockPlacedNotification>,
    INotificationHandler<BlockRemovedNotification>
{
    private readonly ILogger<BlockPlacementNotificationBridge> _logger;

    // Thread-safe weak event managers that prevent memory leaks
    private static readonly WeakEventManager<BlockPlacedNotification> _blockPlacedManager = 
        new("BlockPlaced");
    private static readonly WeakEventManager<BlockRemovedNotification> _blockRemovedManager = 
        new("BlockRemoved");

    // Legacy static events for backward compatibility (deprecated)
    [Obsolete("Use SubscribeToBlockPlaced instead for thread-safe weak event subscription")]
    public static event Func<BlockPlacedNotification, Task>? BlockPlacedEvent;
    
    [Obsolete("Use SubscribeToBlockRemoved instead for thread-safe weak event subscription")]
    public static event Func<BlockRemovedNotification, Task>? BlockRemovedEvent;

    /// <summary>
    /// Subscribes to block placed events using thread-safe weak references.
    /// Returns a disposable token that must be disposed to unsubscribe.
    /// </summary>
    public static IDisposable SubscribeToBlockPlaced(Func<BlockPlacedNotification, Task> handler)
    {
        return _blockPlacedManager.Subscribe(handler);
    }

    /// <summary>
    /// Subscribes to block removed events using thread-safe weak references.
    /// Returns a disposable token that must be disposed to unsubscribe.
    /// </summary>
    public static IDisposable SubscribeToBlockRemoved(Func<BlockRemovedNotification, Task> handler)
    {
        return _blockRemovedManager.Subscribe(handler);
    }

    /// <summary>
    /// Clears all event subscriptions. Used in testing to prevent memory leaks.
    /// </summary>
    public static void ClearSubscriptions()
    {
        _blockPlacedManager.ClearSubscriptions();
        _blockRemovedManager.ClearSubscriptions();
        BlockPlacedEvent = null;
        BlockRemovedEvent = null;
    }

    /// <summary>
    /// Gets the number of subscribers to BlockPlaced events. Used for testing.
    /// </summary>
    public static int GetBlockPlacedSubscriberCount()
    {
        return _blockPlacedManager.GetSubscriberCount() + 
               (BlockPlacedEvent?.GetInvocationList().Length ?? 0);
    }

    /// <summary>
    /// Manually triggers the BlockPlaced events for testing purposes.
    /// </summary>
    public static async Task TriggerBlockPlacedEventForTesting(BlockPlacedNotification notification)
    {
        await _blockPlacedManager.InvokeAsync(notification);
        if (BlockPlacedEvent != null)
        {
            await BlockPlacedEvent(notification);
        }
    }

    public BlockPlacementNotificationBridge(ILogger<BlockPlacementNotificationBridge> logger)
    {
        _logger = logger;
    }

    public async Task Handle(BlockPlacedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("BlockPlacementNotificationBridge.Handle called for BlockPlaced at {Position}", notification.Position);

        // Invoke weak event manager (thread-safe)
        await _blockPlacedManager.InvokeAsync(notification);
        
        // Invoke legacy static event for backward compatibility
        if (BlockPlacedEvent != null)
        {
            await BlockPlacedEvent(notification);
            _logger.LogDebug("Legacy BlockPlacedEvent invoked");
        }

        if (_blockPlacedManager.GetSubscriberCount() == 0 && BlockPlacedEvent == null)
        {
            _logger.LogWarning("No subscribers for BlockPlaced event - no view will be updated!");
        }
    }

    public async Task Handle(BlockRemovedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("BlockPlacementNotificationBridge.Handle called for BlockRemoved at {Position}", notification.Position);

        // Invoke weak event manager (thread-safe)
        await _blockRemovedManager.InvokeAsync(notification);
        
        // Invoke legacy static event for backward compatibility
        if (BlockRemovedEvent != null)
        {
            await BlockRemovedEvent(notification);
            _logger.LogDebug("Legacy BlockRemovedEvent invoked");
        }

        if (_blockRemovedManager.GetSubscriberCount() == 0 && BlockRemovedEvent == null)
        {
            _logger.LogWarning("No subscribers for BlockRemoved event - no view will be updated!");
        }
    }
}
