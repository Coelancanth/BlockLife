using BlockLife.Core.Domain.Player;
using BlockLife.Core.Features.Player.Notifications;
using BlockLife.Core.Features.Player.Queries;
using BlockLife.Core.Features.Player.Views;
using BlockLife.Core.Presentation;
using LanguageExt;
using MediatR;
using Serilog;
using System;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Player.Presenters
{
    /// <summary>
    /// Presenter for the attribute display view that shows player resources and attributes.
    /// Coordinates between player state queries and view updates.
    /// Following the established MVP pattern from BlockLife architecture.
    /// </summary>
    public sealed class AttributePresenter : PresenterBase<IAttributeView>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private IDisposable? _playerStateChangedSubscription;

        public AttributePresenter(IAttributeView view, IMediator mediator, ILogger logger)
            : base(view)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the attribute presenter and loads the current player state.
        /// Sets up any notification subscriptions needed for state updates.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _logger.Information("AttributePresenter initialized, setting up player state subscriptions");

            try
            {
                // Subscribe to player state change notifications
                _playerStateChangedSubscription = PlayerStateNotificationBridge.SubscribeToPlayerStateChanged(OnPlayerStateChangedNotification);

                _logger.Information("Subscribed to player state change notifications using thread-safe weak events");

                // Load initial state
                _ = RefreshAttributeDisplayAsync();

                _logger.Information("AttributePresenter setup complete");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error setting up attribute display");
            }
        }

        /// <summary>
        /// Refreshes the attribute display with current player state.
        /// Called on initialization and whenever player state changes.
        /// </summary>
        public async Task RefreshAttributeDisplayAsync()
        {
            _logger.Debug("Refreshing attribute display");

            try
            {
                var query = GetCurrentPlayerQuery.Create();
                var result = await _mediator.Send(query);

                await result.Match(
                    Succ: async playerState =>
                    {
                        _logger.Debug("Successfully retrieved player state for display");
                        await View.UpdateAttributeDisplayAsync(playerState);
                    },
                    Fail: async error =>
                    {
                        _logger.Warning("Failed to retrieve player state: {Error}", error.Message);
                        await View.ShowErrorAsync(error.Message);
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error refreshing attribute display");
                await View.ShowErrorAsync("An unexpected error occurred while loading player data");
            }
        }

        /// <summary>
        /// Handles notification that player attributes have changed due to match rewards.
        /// Updates the display to show the new values and provides feedback about changes.
        /// </summary>
        /// <param name="resourceChanges">Resources that changed</param>
        /// <param name="attributeChanges">Attributes that changed</param>
        /// <param name="matchDescription">Description of what caused the changes</param>
        public async Task HandleAttributeChangesAsync(
            Map<ResourceType, int> resourceChanges,
            Map<AttributeType, int> attributeChanges,
            string? matchDescription = null)
        {
            try
            {
                _logger.Information("Handling attribute changes: {ResourceCount} resources, {AttributeCount} attributes",
                    resourceChanges.Count, attributeChanges.Count);

                // Show the changes with feedback
                await View.ShowAttributeChangesAsync(resourceChanges, attributeChanges, matchDescription);

                // Refresh the display with updated state
                await RefreshAttributeDisplayAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling attribute changes");
            }
        }

        /// <summary>
        /// Clears the attribute display when no player exists.
        /// </summary>
        public async Task HandleNoPlayerAsync()
        {
            try
            {
                _logger.Information("No current player - clearing attribute display");
                await View.ClearDisplayAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error clearing attribute display");
            }
        }

        /// <summary>
        /// Handles player state changed notifications from the notification bridge.
        /// Automatically updates the view when player state changes.
        /// </summary>
        private async Task OnPlayerStateChangedNotification(PlayerStateChangedNotification notification)
        {
            try
            {
                _logger.Debug("Received player state changed notification for player {PlayerName}",
                    notification.UpdatedPlayerState.Name);

                // Show feedback for the changes that triggered this notification
                await View.ShowAttributeChangesAsync(
                    notification.ResourceChanges,
                    notification.AttributeChanges,
                    notification.ChangeDescription);

                // Update the display with the new state
                await View.UpdateAttributeDisplayAsync(notification.UpdatedPlayerState);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling player state changed notification");
            }
        }

        /// <summary>
        /// Disposes the presenter and cleans up any subscriptions.
        /// </summary>
        public override void Dispose()
        {
            // Dispose weak event subscriptions to prevent memory leaks
            _playerStateChangedSubscription?.Dispose();

            _logger.Information("AttributePresenter disposed and weak event subscriptions cleaned up");
            base.Dispose();
        }
    }
}