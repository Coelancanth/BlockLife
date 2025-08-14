using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlockLife.Core.Infrastructure.Events;
using Xunit;
using Xunit.Abstractions;

namespace BlockLife.Core.Tests.Infrastructure.Events;

public class WeakEventManagerTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly WeakEventManager<TestEventArgs> _manager;
    private readonly List<string> _receivedEvents;

    public WeakEventManagerTests(ITestOutputHelper output)
    {
        _output = output;
        _manager = new WeakEventManager<TestEventArgs>("TestEvent");
        _receivedEvents = new List<string>();
    }

    [Fact]
    public async Task Subscribe_And_Invoke_Should_Call_Handler()
    {
        // Arrange
        var eventReceived = false;
        var subscription = _manager.Subscribe(async args =>
        {
            eventReceived = true;
            await Task.CompletedTask;
        });

        // Act
        await _manager.InvokeAsync(new TestEventArgs { Message = "Test" });

        // Assert
        Assert.True(eventReceived);
        subscription.Dispose();
    }

    [Fact]
    public async Task Multiple_Subscribers_Should_All_Receive_Event()
    {
        // Arrange
        var count = 0;
        var sub1 = _manager.Subscribe(async _ => { Interlocked.Increment(ref count); await Task.CompletedTask; });
        var sub2 = _manager.Subscribe(async _ => { Interlocked.Increment(ref count); await Task.CompletedTask; });
        var sub3 = _manager.Subscribe(async _ => { Interlocked.Increment(ref count); await Task.CompletedTask; });

        // Act
        await _manager.InvokeAsync(new TestEventArgs { Message = "Test" });

        // Assert
        Assert.Equal(3, count);

        sub1.Dispose();
        sub2.Dispose();
        sub3.Dispose();
    }

    [Fact]
    public async Task Disposed_Subscription_Should_Not_Receive_Events()
    {
        // Arrange
        var count = 0;
        var subscription = _manager.Subscribe(async _ =>
        {
            count++;
            await Task.CompletedTask;
        });

        // Act
        await _manager.InvokeAsync(new TestEventArgs { Message = "First" });
        subscription.Dispose();
        await _manager.InvokeAsync(new TestEventArgs { Message = "Second" });

        // Assert
        Assert.Equal(1, count); // Should only receive first event
    }

    [Fact]
    public async Task Weak_References_Should_Allow_Garbage_Collection()
    {
        // Arrange
        WeakReference? weakRef = null;

        // Create subscription in a separate method to ensure it goes out of scope
        void CreateTemporarySubscription()
        {
            var handler = new EventHandler();
            weakRef = new WeakReference(handler);
            _manager.Subscribe(handler.HandleEventAsync);
            // Subscription not disposed - relying on weak reference
        }

        CreateTemporarySubscription();

        // Act
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert
        Assert.False(weakRef!.IsAlive, "Handler should have been garbage collected");

        // Invoking should not throw even with dead handlers
        await _manager.InvokeAsync(new TestEventArgs { Message = "Test" });
    }

    [Fact]
    public async Task Concurrent_Subscriptions_And_Invocations_Should_Be_Thread_Safe()
    {
        // Arrange
        var errors = new List<Exception>();
        var successCount = 0;
        const int threadCount = 100;
        const int operationsPerThread = 50;

        // Act
        var tasks = Enumerable.Range(0, threadCount).Select(i => Task.Run(async () =>
        {
            try
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    if (i % 2 == 0)
                    {
                        // Subscribe and unsubscribe
                        var sub = _manager.Subscribe(async args =>
                        {
                            Interlocked.Increment(ref successCount);
                            await Task.CompletedTask;
                        });
                        await Task.Delay(1);
                        sub.Dispose();
                    }
                    else
                    {
                        // Invoke events
                        await _manager.InvokeAsync(new TestEventArgs { Message = $"Thread{i}-Event{j}" });
                    }
                }
            }
            catch (Exception ex)
            {
                lock (errors)
                {
                    errors.Add(ex);
                }
            }
        }));

        await Task.WhenAll(tasks);

        // Assert
        Assert.Empty(errors);
        _output.WriteLine($"Successfully processed {successCount} events across {threadCount} threads");
    }

    [Fact]
    public void GetSubscriberCount_Should_Return_Correct_Count()
    {
        // Arrange & Act
        Assert.Equal(0, _manager.GetSubscriberCount());

        var sub1 = _manager.Subscribe(async _ => await Task.CompletedTask);
        Assert.Equal(1, _manager.GetSubscriberCount());

        var sub2 = _manager.Subscribe(async _ => await Task.CompletedTask);
        Assert.Equal(2, _manager.GetSubscriberCount());

        sub1.Dispose();
        Assert.Equal(1, _manager.GetSubscriberCount());

        sub2.Dispose();
        Assert.Equal(0, _manager.GetSubscriberCount());
    }

    [Fact]
    public async Task Handler_Exception_Should_Not_Affect_Other_Handlers()
    {
        // Arrange
        var handler1Called = false;
        var handler3Called = false;

        var sub1 = _manager.Subscribe(async _ =>
        {
            handler1Called = true;
            await Task.CompletedTask;
        });

        var sub2 = _manager.Subscribe(async _ =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Test exception");
        });

        var sub3 = _manager.Subscribe(async _ =>
        {
            handler3Called = true;
            await Task.CompletedTask;
        });

        // Act
        await _manager.InvokeAsync(new TestEventArgs { Message = "Test" });

        // Assert
        Assert.True(handler1Called);
        Assert.True(handler3Called);

        sub1.Dispose();
        sub2.Dispose();
        sub3.Dispose();
    }

    [Fact]
    public void ClearSubscriptions_Should_Remove_All_Subscriptions()
    {
        // Arrange
        _manager.Subscribe(async _ => await Task.CompletedTask);
        _manager.Subscribe(async _ => await Task.CompletedTask);
        _manager.Subscribe(async _ => await Task.CompletedTask);

        Assert.Equal(3, _manager.GetSubscriberCount());

        // Act
        _manager.ClearSubscriptions();

        // Assert
        Assert.Equal(0, _manager.GetSubscriberCount());
    }

    public void Dispose()
    {
        _manager.ClearSubscriptions();
    }

    private class TestEventArgs
    {
        public string Message { get; set; } = "";
    }

    private class EventHandler
    {
        public async Task HandleEventAsync(TestEventArgs args)
        {
            await Task.CompletedTask;
        }
    }
}
