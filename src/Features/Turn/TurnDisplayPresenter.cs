using BlockLife.Core.Features.Turn.Effects;
using BlockLife.Core.Presentation;
using MediatR;
using Serilog;
using System;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Turn
{
    /// <summary>
    /// Presenter for the turn display view that handles turn state updates.
    /// Coordinates between turn system notifications and view updates.
    /// </summary>
    public sealed class TurnDisplayPresenter : PresenterBase<ITurnDisplayView>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private IDisposable? _turnStartSubscription;
        private IDisposable? _turnEndSubscription;

        public TurnDisplayPresenter(ITurnDisplayView view, IMediator mediator, ILogger logger)
            : base(view)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the turn display presenter and subscribes to turn notifications.
        /// Subscribes to domain notifications to keep the view in sync with turn changes.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _logger.Information("TurnDisplayPresenter initialized, setting up turn display and notification subscriptions");

            try
            {
                // Subscribe to domain notification events from the notification bridge using weak events
                _turnStartSubscription = TurnNotificationBridge.SubscribeToTurnStart(OnTurnStartNotification);
                _turnEndSubscription = TurnNotificationBridge.SubscribeToTurnEnd(OnTurnEndNotification);

                _logger.Information("Subscribed to turn notification events using thread-safe weak events");

                // Initialize view with turn 1 (default starting state)
                var initialTurn = BlockLife.Core.Domain.Turn.Turn.CreateInitial();
                View.UpdateCurrentTurn(initialTurn);

                _logger.Information("Turn display initialized with initial turn");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error setting up turn display");
            }
        }

        /// <summary>
        /// Handles turn start notifications from the domain notification bridge.
        /// Automatically updates the view when turns start.
        /// </summary>
        private Task OnTurnStartNotification(TurnStartNotification notification)
        {
            try
            {
                _logger.Debug("Received turn start notification for Turn {TurnNumber}",
                    notification.Turn.Number);

                View.DisplayTurnStart(notification.Turn);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling turn start notification for Turn {TurnNumber}", 
                    notification.Turn.Number);
                
                // Show error to user if view update fails
                View.ShowTurnError($"Failed to update turn display: {ex.Message}");
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles turn end notifications from the domain notification bridge.
        /// Automatically updates the view when turns end.
        /// </summary>
        private Task OnTurnEndNotification(TurnEndNotification notification)
        {
            try
            {
                _logger.Debug("Received turn end notification for Turn {TurnNumber}",
                    notification.Turn.Number);

                View.DisplayTurnEnd(notification.Turn);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling turn end notification for Turn {TurnNumber}", 
                    notification.Turn.Number);
                
                // Show error to user if view update fails
                View.ShowTurnError($"Failed to update turn display: {ex.Message}");
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disposes the presenter and unsubscribes from turn notifications to prevent memory leaks.
        /// </summary>
        public override void Dispose()
        {
            // Dispose weak event subscriptions to prevent memory leaks
            _turnStartSubscription?.Dispose();
            _turnEndSubscription?.Dispose();

            _logger.Information("TurnDisplayPresenter disposed and weak event subscriptions cleaned up");

            base.Dispose();
        }
    }
}