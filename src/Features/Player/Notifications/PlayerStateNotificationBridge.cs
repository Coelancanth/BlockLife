using System;
using System.Threading;
using System.Threading.Tasks;
using BlockLife.Core.Infrastructure.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlockLife.Core.Features.Player.Notifications
{
    /// <summary>
    /// CRITICAL INFRASTRUCTURE: Bridges MediatR notifications to static events for presenter consumption.
    /// Without this bridge, attribute changes won't reach the UI layer.
    /// Uses thread-safe weak event pattern to prevent memory leaks and ensure thread safety.
    /// Following the established notification bridge pattern from BlockLife architecture.
    /// </summary>
    public class PlayerStateNotificationBridge : INotificationHandler<PlayerStateChangedNotification>
    {
        private readonly ILogger<PlayerStateNotificationBridge> _logger;

        // Thread-safe weak event manager that prevents memory leaks
        private static readonly WeakEventManager<PlayerStateChangedNotification> _playerStateChangedManager =
            new("PlayerStateChanged");

        public PlayerStateNotificationBridge(ILogger<PlayerStateNotificationBridge> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles MediatR notification and bridges to weak event system.
        /// This is the critical link between CQRS commands and UI updates.
        /// </summary>
        public async Task Handle(PlayerStateChangedNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("PlayerStateNotificationBridge.Handle called for player {PlayerName}",
                notification.UpdatedPlayerState.Name);

            try
            {
                // Bridge to weak event system for presenters
                await _playerStateChangedManager.InvokeAsync(notification);

                _logger.LogDebug("PlayerStateChanged event published to {SubscriberCount} subscribers",
                    _playerStateChangedManager.GetSubscriberCount());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling player state changed notification");
            }
        }

        /// <summary>
        /// Subscribes to player state changed events using thread-safe weak references.
        /// Returns a disposable token that must be disposed to unsubscribe.
        /// </summary>
        public static IDisposable SubscribeToPlayerStateChanged(Func<PlayerStateChangedNotification, Task> handler)
        {
            return _playerStateChangedManager.Subscribe(handler);
        }

        /// <summary>
        /// Clears all event subscriptions. Used in testing to prevent memory leaks.
        /// </summary>
        public static void ClearSubscriptions()
        {
            _playerStateChangedManager.ClearSubscriptions();
        }

        /// <summary>
        /// Gets the number of subscribers to PlayerStateChanged events. Used for testing.
        /// </summary>
        public static int GetPlayerStateChangedSubscriberCount()
        {
            return _playerStateChangedManager.GetSubscriberCount();
        }
    }
}