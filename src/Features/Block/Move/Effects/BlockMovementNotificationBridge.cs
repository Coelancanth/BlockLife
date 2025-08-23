using System;
using System.Threading;
using System.Threading.Tasks;
using BlockLife.Core.Features.Block.Effects;
using BlockLife.Core.Infrastructure.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlockLife.Core.Features.Block.Move.Effects;

/// <summary>
/// CRITICAL INFRASTRUCTURE: Bridges MediatR notifications to static events for presenter consumption.
/// Without this bridge, the Move Block feature is 0% functional - backend changes won't reach UI.
/// Uses thread-safe weak event pattern to prevent memory leaks and ensure thread safety.
/// </summary>
public class BlockMovementNotificationBridge : INotificationHandler<BlockMovedNotification>
{
    private readonly ILogger<BlockMovementNotificationBridge> _logger;

    // Thread-safe weak event manager that prevents memory leaks
    private static readonly WeakEventManager<BlockMovedNotification> _blockMovedManager =
        new("BlockMoved");

    // Legacy static event for backward compatibility (deprecated)
    [Obsolete("Use SubscribeToBlockMoved instead for thread-safe weak event subscription")]
    public static event Func<BlockMovedNotification, Task>? BlockMovedEvent;

    /// <summary>
    /// Subscribes to block moved events using thread-safe weak references.
    /// Returns a disposable token that must be disposed to unsubscribe.
    /// </summary>
    public static IDisposable SubscribeToBlockMoved(Func<BlockMovedNotification, Task> handler)
    {
        return _blockMovedManager.Subscribe(handler);
    }

    /// <summary>
    /// Clears all event subscriptions. Used in testing to prevent memory leaks.
    /// </summary>
    public static void ClearSubscriptions()
    {
        _blockMovedManager.ClearSubscriptions();
        BlockMovedEvent = null;
    }

    /// <summary>
    /// Gets the number of subscribers to BlockMoved events. Used for testing.
    /// </summary>
    public static int GetBlockMovedSubscriberCount()
    {
        return _blockMovedManager.GetSubscriberCount() +
               (BlockMovedEvent?.GetInvocationList().Length ?? 0);
    }

    public BlockMovementNotificationBridge(ILogger<BlockMovementNotificationBridge> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the BlockMovedNotification from the command handler and bridges it to the static event.
    /// This method is called by MediatR's notification pipeline after a successful block move.
    /// </summary>
    public async Task Handle(BlockMovedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("BlockMovementNotificationBridge.Handle called for BlockMoved from {FromPosition} to {ToPosition}",
            notification.FromPosition, notification.ToPosition);

        // Invoke weak event manager (thread-safe)
        await _blockMovedManager.InvokeAsync(notification);

        // Invoke legacy static event for backward compatibility
        if (BlockMovedEvent != null)
        {
            await BlockMovedEvent(notification);
            _logger.LogDebug("Legacy BlockMovedEvent invoked");
        }

        if (_blockMovedManager.GetSubscriberCount() == 0 && BlockMovedEvent == null)
        {
            _logger.LogWarning("No subscribers for BlockMoved event - no view will be updated!");
        }
    }
}
