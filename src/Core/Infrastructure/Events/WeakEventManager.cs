using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BlockLife.Core.Infrastructure.Events;

/// <summary>
/// Thread-safe weak event manager that prevents memory leaks from static events.
/// Uses weak references to allow subscribers to be garbage collected.
/// </summary>
public class WeakEventManager<TEventArgs> where TEventArgs : class
{
    private readonly ConcurrentDictionary<int, WeakReference> _handlers = new();
    private readonly object _lock = new();
    private readonly ILogger? _logger;
    private readonly string _eventName;
    private int _nextHandlerId = 0;

    public WeakEventManager(string eventName, ILogger? logger = null)
    {
        _eventName = eventName;
        _logger = logger;
    }

    /// <summary>
    /// Subscribes a handler to the event. Returns a token that must be disposed to unsubscribe.
    /// </summary>
    public IDisposable Subscribe(Func<TEventArgs, Task> handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        lock (_lock)
        {
            var handlerId = _nextHandlerId++;
            _handlers.TryAdd(handlerId, new WeakReference(handler));
            
            _logger?.LogDebug("Subscribed handler {HandlerId} to event {EventName}", handlerId, _eventName);
            
            return new SubscriptionToken(this, handlerId);
        }
    }

    /// <summary>
    /// Invokes all alive handlers with the given event args.
    /// </summary>
    public async Task InvokeAsync(TEventArgs args)
    {
        var aliveHandlers = new List<Func<TEventArgs, Task>>();
        var deadHandlerIds = new List<int>();

        // Collect alive handlers and identify dead ones
        foreach (var kvp in _handlers)
        {
            if (kvp.Value.Target is Func<TEventArgs, Task> handler)
            {
                aliveHandlers.Add(handler);
            }
            else
            {
                deadHandlerIds.Add(kvp.Key);
            }
        }

        // Clean up dead handlers
        foreach (var id in deadHandlerIds)
        {
            _handlers.TryRemove(id, out _);
            _logger?.LogDebug("Removed dead handler {HandlerId} from event {EventName}", id, _eventName);
        }

        // Invoke all alive handlers
        if (aliveHandlers.Count > 0)
        {
            _logger?.LogDebug("Invoking {Count} handlers for event {EventName}", aliveHandlers.Count, _eventName);
            
            // Execute all handlers concurrently for better performance
            var tasks = aliveHandlers.Select(handler => InvokeHandlerSafely(handler, args));
            await Task.WhenAll(tasks);
        }
        else
        {
            _logger?.LogDebug("No handlers to invoke for event {EventName}", _eventName);
        }
    }

    private async Task InvokeHandlerSafely(Func<TEventArgs, Task> handler, TEventArgs args)
    {
        try
        {
            await handler(args);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error invoking handler for event {EventName}", _eventName);
        }
    }

    /// <summary>
    /// Gets the number of alive subscribers.
    /// </summary>
    public int GetSubscriberCount()
    {
        return _handlers.Count(kvp => kvp.Value.Target != null);
    }

    /// <summary>
    /// Clears all subscriptions. Used primarily for testing.
    /// </summary>
    public void ClearSubscriptions()
    {
        _handlers.Clear();
        _logger?.LogDebug("Cleared all subscriptions for event {EventName}", _eventName);
    }

    private void Unsubscribe(int handlerId)
    {
        if (_handlers.TryRemove(handlerId, out _))
        {
            _logger?.LogDebug("Unsubscribed handler {HandlerId} from event {EventName}", handlerId, _eventName);
        }
    }

    /// <summary>
    /// Subscription token that automatically unsubscribes when disposed.
    /// </summary>
    private class SubscriptionToken : IDisposable
    {
        private readonly WeakEventManager<TEventArgs> _manager;
        private readonly int _handlerId;
        private bool _disposed;

        public SubscriptionToken(WeakEventManager<TEventArgs> manager, int handlerId)
        {
            _manager = manager;
            _handlerId = handlerId;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _manager.Unsubscribe(_handlerId);
                _disposed = true;
            }
        }
    }
}