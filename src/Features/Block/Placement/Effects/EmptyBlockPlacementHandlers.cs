using System.Threading;
using System.Threading.Tasks;
using BlockLife.Core.Features.Block.Placement.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlockLife.Core.Features.Block.Placement.Effects;

/// <summary>
/// Empty notification handlers to satisfy MediatR's requirement for at least one handler per notification.
/// The actual view updates are handled by presenters that are created when views are instantiated.
/// </summary>
public class EmptyBlockPlacementHandlers : 
    INotificationHandler<BlockPlacedNotification>,
    INotificationHandler<BlockRemovedNotification>
{
    private readonly ILogger<EmptyBlockPlacementHandlers> _logger;

    public EmptyBlockPlacementHandlers(ILogger<EmptyBlockPlacementHandlers> logger)
    {
        _logger = logger;
    }

    public Task Handle(BlockPlacedNotification notification, CancellationToken cancellationToken)
    {
        // Log for debugging purposes
        _logger.LogTrace("BlockPlacedNotification published for block {BlockId} at {Position}", 
            notification.BlockId, notification.Position);
        
        // The actual handling is done by presenters that subscribe to these notifications
        return Task.CompletedTask;
    }

    public Task Handle(BlockRemovedNotification notification, CancellationToken cancellationToken)
    {
        // Log for debugging purposes
        _logger.LogTrace("BlockRemovedNotification published for block {BlockId}", 
            notification.BlockId);
        
        // The actual handling is done by presenters that subscribe to these notifications
        return Task.CompletedTask;
    }
}