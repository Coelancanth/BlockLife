using System.Threading;
using System.Threading.Tasks;
using BlockLife.Core.Features.Block.Placement.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlockLife.Core.Features.Block.Placement.Effects;

/// <summary>
/// Notification handlers that receive MediatR notifications and forward them to presenters.
/// This solves the architectural constraint where presenters cannot implement INotificationHandler directly.
/// </summary>
public class BlockPlacementNotificationBridge :
    INotificationHandler<BlockPlacedNotification>,
    INotificationHandler<BlockRemovedNotification>
{
    private readonly ILogger<BlockPlacementNotificationBridge> _logger;

    // Static event that presenters can subscribe to
    public static event Func<BlockPlacedNotification, Task>? BlockPlacedEvent;
    public static event Func<BlockRemovedNotification, Task>? BlockRemovedEvent;

    /// <summary>
    /// Clears all event subscriptions. Used in testing to prevent memory leaks.
    /// </summary>
    public static void ClearSubscriptions()
    {
        BlockPlacedEvent = null;
        BlockRemovedEvent = null;
    }

    /// <summary>
    /// Gets the number of subscribers to BlockPlacedEvent. Used for testing.
    /// </summary>
    public static int GetBlockPlacedSubscriberCount()
    {
        return BlockPlacedEvent?.GetInvocationList().Length ?? 0;
    }

    /// <summary>
    /// Manually triggers the BlockPlacedEvent for testing purposes.
    /// </summary>
    public static async Task TriggerBlockPlacedEventForTesting(BlockPlacedNotification notification)
    {
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

        if (BlockPlacedEvent != null)
        {
            await BlockPlacedEvent(notification);
            _logger.LogInformation("BlockPlacedEvent invoked successfully");
        }
        else
        {
            _logger.LogWarning("No subscribers for BlockPlaced event - no view will be updated!");
        }
    }

    public async Task Handle(BlockRemovedNotification notification, CancellationToken cancellationToken)
    {
        if (BlockRemovedEvent != null)
        {
            await BlockRemovedEvent(notification);
        }
        else
        {
            _logger.LogWarning("No subscribers for BlockRemoved event - no view will be updated!");
        }
    }
}
