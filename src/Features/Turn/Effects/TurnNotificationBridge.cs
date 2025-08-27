using System;
using System.Threading;
using System.Threading.Tasks;
using BlockLife.Core.Infrastructure.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlockLife.Core.Features.Turn.Effects;

/// <summary>
/// CRITICAL INFRASTRUCTURE: Bridges MediatR notifications to static events for presenter consumption.
/// Without this bridge, the Turn Display feature is 0% functional - backend changes won't reach UI.
/// Uses thread-safe weak event pattern to prevent memory leaks and ensure thread safety.
/// </summary>
public class TurnNotificationBridge : INotificationHandler<TurnStartNotification>, INotificationHandler<TurnEndNotification>
{
    private readonly ILogger<TurnNotificationBridge> _logger;

    // Thread-safe weak event managers that prevent memory leaks
    private static readonly WeakEventManager<TurnStartNotification> _turnStartManager =
        new("TurnStart");

    private static readonly WeakEventManager<TurnEndNotification> _turnEndManager =
        new("TurnEnd");

    // Legacy static events for backward compatibility (deprecated)
    [Obsolete("Use SubscribeToTurnStart instead for thread-safe weak event subscription")]
    public static event Func<TurnStartNotification, Task>? TurnStartEvent;

    [Obsolete("Use SubscribeToTurnEnd instead for thread-safe weak event subscription")]
    public static event Func<TurnEndNotification, Task>? TurnEndEvent;

    /// <summary>
    /// Subscribes to turn start events using thread-safe weak references.
    /// Returns a disposable token that must be disposed to unsubscribe.
    /// </summary>
    public static IDisposable SubscribeToTurnStart(Func<TurnStartNotification, Task> handler)
    {
        return _turnStartManager.Subscribe(handler);
    }

    /// <summary>
    /// Subscribes to turn end events using thread-safe weak references.
    /// Returns a disposable token that must be disposed to unsubscribe.
    /// </summary>
    public static IDisposable SubscribeToTurnEnd(Func<TurnEndNotification, Task> handler)
    {
        return _turnEndManager.Subscribe(handler);
    }

    /// <summary>
    /// Clears all event subscriptions. Used in testing to prevent memory leaks.
    /// </summary>
    public static void ClearSubscriptions()
    {
        _turnStartManager.ClearSubscriptions();
        _turnEndManager.ClearSubscriptions();
        TurnStartEvent = null;
        TurnEndEvent = null;
    }

    /// <summary>
    /// Gets the number of subscribers to TurnStart events. Used for testing.
    /// </summary>
    public static int GetTurnStartSubscriberCount()
    {
        return _turnStartManager.GetSubscriberCount() +
               (TurnStartEvent?.GetInvocationList().Length ?? 0);
    }

    /// <summary>
    /// Gets the number of subscribers to TurnEnd events. Used for testing.
    /// </summary>
    public static int GetTurnEndSubscriberCount()
    {
        return _turnEndManager.GetSubscriberCount() +
               (TurnEndEvent?.GetInvocationList().Length ?? 0);
    }

    public TurnNotificationBridge(ILogger<TurnNotificationBridge> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the TurnStartNotification from the command handler and bridges it to the static event.
    /// This method is called by MediatR's notification pipeline after a successful turn start.
    /// </summary>
    public async Task Handle(TurnStartNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("TurnNotificationBridge.Handle called for TurnStart - Turn {TurnNumber}",
            notification.Turn.Number);

        // Invoke weak event manager (thread-safe)
        await _turnStartManager.InvokeAsync(notification);

        // Invoke legacy static event for backward compatibility
        if (TurnStartEvent != null)
        {
            await TurnStartEvent(notification);
            _logger.LogDebug("Legacy TurnStartEvent invoked");
        }

        if (_turnStartManager.GetSubscriberCount() == 0 && TurnStartEvent == null)
        {
            _logger.LogWarning("No subscribers for TurnStart event - no view will be updated!");
        }
    }

    /// <summary>
    /// Handles the TurnEndNotification from the command handler and bridges it to the static event.
    /// This method is called by MediatR's notification pipeline after a successful turn end.
    /// </summary>
    public async Task Handle(TurnEndNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("TurnNotificationBridge.Handle called for TurnEnd - Turn {TurnNumber}",
            notification.Turn.Number);

        // Invoke weak event manager (thread-safe)
        await _turnEndManager.InvokeAsync(notification);

        // Invoke legacy static event for backward compatibility
        if (TurnEndEvent != null)
        {
            await TurnEndEvent(notification);
            _logger.LogDebug("Legacy TurnEndEvent invoked");
        }

        if (_turnEndManager.GetSubscriberCount() == 0 && TurnEndEvent == null)
        {
            _logger.LogWarning("No subscribers for TurnEnd event - no view will be updated!");
        }
    }
}