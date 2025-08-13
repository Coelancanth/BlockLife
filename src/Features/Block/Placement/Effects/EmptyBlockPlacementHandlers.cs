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

    public BlockPlacementNotificationBridge(ILogger<BlockPlacementNotificationBridge> logger)
    {
        _logger = logger;
    }

    public async Task Handle(BlockPlacedNotification notification, CancellationToken cancellationToken)
    {
        if (BlockPlacedEvent != null)
        {
            await BlockPlacedEvent(notification);
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