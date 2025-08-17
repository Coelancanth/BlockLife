using BlockLife.Core.Application.Simulation;
using BlockLife.Core.Features.Block.Placement.Effects;
using BlockLife.Core.Domain.Common;
using BlockLife.Core;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BlockLife.Core.Tests.StressTests;

/// <summary>
/// Integration tests for HF_002 regression - Thread-safety in SimulationManager
/// These tests verify that SimulationManager's ConcurrentQueue implementation
/// handles concurrent operations correctly under heavy load
/// </summary>
public class SimulationManagerThreadSafetyTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly ISimulationManager _simulation;
    private readonly Random _random = new();

    public SimulationManagerThreadSafetyTests(ITestOutputHelper output)
    {
        _output = output;

        // Initialize with minimal logging to reduce noise during stress testing
        var masterSwitch = new LoggingLevelSwitch(LogEventLevel.Error);
        var categorySwitches = new Dictionary<string, LoggingLevelSwitch>();
        var mockSink = new Mock<ILogEventSink>();
        _serviceProvider = GameStrapper.Initialize(masterSwitch, categorySwitches, mockSink.Object);

        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _simulation = _serviceProvider.GetRequiredService<ISimulationManager>();
    }

    #region High-Load Thread Safety Tests

    [Fact]
    public async Task ConcurrentEffectQueuing_HighVolume_ShouldMaintainDataIntegrity()
    {
        // Arrange
        const int threadCount = 100;
        const int effectsPerThread = 100;
        const int totalExpectedEffects = threadCount * effectsPerThread;
        
        var errors = new ConcurrentBag<string>();
        var effectsQueued = new ConcurrentBag<Guid>();

        // Act - Simulate high-volume concurrent queuing
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            for (int i = 0; i < effectsPerThread; i++)
            {
                try
                {
                    var effectId = Guid.NewGuid();
                    var effect = new BlockPlacedEffect(
                        effectId,
                        new Vector2Int(threadId % 50, i % 50), // Distribute across grid
                        Domain.Block.BlockType.Basic,
                        DateTime.UtcNow
                    );

                    var result = _simulation.QueueEffect(effect);
                    if (result.IsSucc)
                    {
                        effectsQueued.Add(effectId);
                    }
                    else
                    {
                        errors.Add($"Thread {threadId}: Failed to queue effect {i}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Thread {threadId}: Exception queuing effect {i}: {ex.Message}");
                }
            }
        }));

        await Task.WhenAll(tasks);

        // Assert
        _output.WriteLine($"Queued {effectsQueued.Count} effects from {threadCount} threads");
        _output.WriteLine($"Errors: {errors.Count}");
        _output.WriteLine($"Pending effects: {_simulation.PendingEffectCount}");

        errors.Should().BeEmpty("No errors should occur during concurrent queuing");
        effectsQueued.Count.Should().Be(totalExpectedEffects, "All effects should be queued successfully");
        _simulation.PendingEffectCount.Should().Be(totalExpectedEffects, "All effects should be pending in queue");
        _simulation.HasPendingEffects.Should().BeTrue("Should have pending effects");
    }

    [Fact]
    public async Task ConcurrentQueueAndDequeue_ShouldNotLoseEffects()
    {
        // Arrange
        const int producerThreads = 20;
        const int effectsPerProducer = 50;
        const int totalExpectedEffects = producerThreads * effectsPerProducer;
        
        var publishedNotifications = new ConcurrentBag<INotification>();
        var errors = new ConcurrentBag<string>();

        // Mock mediator to capture published notifications
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((notification, _) =>
            {
                publishedNotifications.Add(notification);
            })
            .Returns(Task.CompletedTask);

        var simulationManager = new SimulationManager(mockMediator.Object, Mock.Of<Microsoft.Extensions.Logging.ILogger<SimulationManager>>());

        // Act - Concurrent producers and periodic processing
        var producerTasks = Enumerable.Range(0, producerThreads).Select(threadId => Task.Run(() =>
        {
            for (int i = 0; i < effectsPerProducer; i++)
            {
                try
                {
                    var effect = new BlockPlacedEffect(
                        Guid.NewGuid(),
                        new Vector2Int(threadId, i),
                        Domain.Block.BlockType.Basic,
                        DateTime.UtcNow
                    );

                    var result = simulationManager.QueueEffect(effect);
                    if (result.IsFail)
                    {
                        errors.Add($"Producer {threadId}: Failed to queue effect {i}");
                    }

                    // Random small delays to create realistic timing
                    if (_random.Next(0, 10) == 0)
                    {
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Producer {threadId}: Exception: {ex.Message}");
                }
            }
        }));

        // Consumer task that processes effects periodically
        var consumerTask = Task.Run(async () =>
        {
            var processedCount = 0;
            while (processedCount < totalExpectedEffects)
            {
                try
                {
                    if (simulationManager.HasPendingEffects)
                    {
                        await simulationManager.ProcessQueuedEffectsAsync();
                        processedCount = publishedNotifications.Count;
                    }
                    
                    // Small delay to allow producers to add more effects
                    await Task.Delay(5);
                }
                catch (Exception ex)
                {
                    errors.Add($"Consumer: Exception processing effects: {ex.Message}");
                    break;
                }
            }
        });

        await Task.WhenAll(producerTasks.Concat(new[] { consumerTask }));

        // Final processing to ensure all effects are handled
        await simulationManager.ProcessQueuedEffectsAsync();

        // Assert
        _output.WriteLine($"Total effects produced: {totalExpectedEffects}");
        _output.WriteLine($"Total notifications published: {publishedNotifications.Count}");
        _output.WriteLine($"Remaining pending effects: {simulationManager.PendingEffectCount}");
        _output.WriteLine($"Errors encountered: {errors.Count}");

        errors.Should().BeEmpty("No errors should occur during concurrent queue/dequeue operations");
        publishedNotifications.Count.Should().Be(totalExpectedEffects, "All effects should result in published notifications");
        simulationManager.HasPendingEffects.Should().BeFalse("All effects should be processed");
    }

    #endregion

    #region Race Condition Detection Tests

    [Fact]
    public async Task RapidSequentialQueueAndProcess_ShouldNotCorruptState()
    {
        // Arrange - This test rapidly alternates between queuing and processing
        // to try to detect race conditions in the ConcurrentQueue implementation
        const int iterations = 1000;
        var processedNotifications = new ConcurrentBag<INotification>();
        var errors = new ConcurrentBag<string>();

        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((notification, _) =>
            {
                processedNotifications.Add(notification);
            })
            .Returns(Task.CompletedTask);

        var simulationManager = new SimulationManager(mockMediator.Object, Mock.Of<Microsoft.Extensions.Logging.ILogger<SimulationManager>>());

        // Act - Rapid queue/process cycles
        var tasks = new List<Task>();

        for (int cycle = 0; cycle < 10; cycle++)
        {
            int currentCycle = cycle;
            tasks.Add(Task.Run(async () =>
            {
                for (int i = 0; i < iterations / 10; i++)
                {
                    try
                    {
                        // Queue multiple effects rapidly
                        for (int j = 0; j < 5; j++)
                        {
                            var effect = new BlockPlacedEffect(
                                Guid.NewGuid(),
                                new Vector2Int(currentCycle, i * 5 + j),
                                Domain.Block.BlockType.Basic,
                                DateTime.UtcNow
                            );
                            simulationManager.QueueEffect(effect);
                        }

                        // Process some effects
                        await simulationManager.ProcessQueuedEffectsAsync();

                        // Queue more effects
                        for (int j = 0; j < 3; j++)
                        {
                            var effect = new BlockRemovedEffect(
                                Guid.NewGuid(),
                                new Vector2Int(currentCycle, i * 3 + j),
                                Domain.Block.BlockType.Study,
                                DateTime.UtcNow
                            );
                            simulationManager.QueueEffect(effect);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Cycle {currentCycle}, Iteration {i}: {ex.Message}");
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Final processing
        await simulationManager.ProcessQueuedEffectsAsync();

        // Assert
        _output.WriteLine($"Processed {processedNotifications.Count} notifications");
        _output.WriteLine($"Remaining pending effects: {simulationManager.PendingEffectCount}");
        _output.WriteLine($"Errors: {errors.Count}");

        errors.Should().BeEmpty("No race conditions should occur during rapid queue/process cycles");
        simulationManager.HasPendingEffects.Should().BeFalse("All effects should be processed");
        processedNotifications.Count.Should().BeGreaterThan(0, "Should have processed some notifications");
    }

    #endregion

    #region Performance Under Concurrent Load

    [Fact(Skip = "Performance timing tests are unreliable in virtualized CI environments")]
    public async Task ConcurrentOperations_ShouldMaintainPerformance()
    {
        // Arrange
        const int threadCount = 50;
        const int operationsPerThread = 200;
        var stopwatch = Stopwatch.StartNew();
        var errors = new ConcurrentBag<string>();

        // Act
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            for (int i = 0; i < operationsPerThread; i++)
            {
                try
                {
                    var effect = new BlockPlacedEffect(
                        Guid.NewGuid(),
                        new Vector2Int(threadId % 20, i % 20),
                        Domain.Block.BlockType.Basic,
                        DateTime.UtcNow
                    );

                    var queueStart = Stopwatch.GetTimestamp();
                    var result = _simulation.QueueEffect(effect);
                    var queueTime = Stopwatch.GetElapsedTime(queueStart);

                    if (result.IsFail)
                    {
                        errors.Add($"Queue operation failed for thread {threadId}, operation {i}");
                    }

                    // Performance check - queuing should be very fast
                    if (queueTime.TotalMilliseconds > 10)
                    {
                        errors.Add($"Queue operation too slow: {queueTime.TotalMilliseconds:F2}ms");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Thread {threadId}: {ex.Message}");
                }
            }
        }));

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var totalOperations = threadCount * operationsPerThread;
        var avgTimePerOperation = stopwatch.Elapsed.TotalMilliseconds / totalOperations;

        _output.WriteLine($"Total operations: {totalOperations}");
        _output.WriteLine($"Total time: {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
        _output.WriteLine($"Average time per operation: {avgTimePerOperation:F4}ms");
        _output.WriteLine($"Operations per second: {totalOperations / stopwatch.Elapsed.TotalSeconds:F0}");
        _output.WriteLine($"Errors: {errors.Count}");

        errors.Should().BeEmpty("No errors should occur during performance test");
        avgTimePerOperation.Should().BeLessThan(1.0, "Average operation time should be under 1ms");
        _simulation.PendingEffectCount.Should().Be(totalOperations, "All operations should be queued");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task ConcurrentNullEffectHandling_ShouldBeRobust()
    {
        // Arrange - Test how ConcurrentQueue handles null values under concurrent access
        const int threadCount = 20;
        const int operationsPerThread = 50;
        var errors = new ConcurrentBag<string>();

        // Act
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            for (int i = 0; i < operationsPerThread; i++)
            {
                try
                {
                    // Mix of valid effects and some that will cause exceptions
                    if (i % 3 == 0)
                    {
                        // Test null handling
                        BlockPlacedEffect? nullEffect = null;
                        try
                        {
                            var result = _simulation.QueueEffect(nullEffect!);
                        }
                        catch (ArgumentNullException)
                        {
                            // Expected exception for null - this is fine
                        }
                    }
                    else
                    {
                        var effect = new BlockPlacedEffect(
                            Guid.NewGuid(),
                            new Vector2Int(threadId, i),
                            Domain.Block.BlockType.Basic,
                            DateTime.UtcNow
                        );

                        var result = _simulation.QueueEffect(effect);
                    }
                    
                    // The important thing is that it doesn't throw an exception
                    // The specific behavior (success/failure) is implementation-dependent
                }
                catch (Exception ex)
                {
                    // Some exceptions might be expected (ArgumentNullException), 
                    // but no threading-related exceptions should occur
                    if (ex.Message.Contains("thread") || ex.Message.Contains("concurrent") || 
                        ex.Message.Contains("race") || ex.Message.Contains("deadlock"))
                    {
                        errors.Add($"Thread-related error in thread {threadId}: {ex.Message}");
                    }
                }
            }
        }));

        await Task.WhenAll(tasks);

        // Assert
        _output.WriteLine($"Completed null effect handling test with {errors.Count} thread-related errors");
        
        // Only thread-related errors are failures - regular ArgumentNullExceptions are fine
        errors.Should().BeEmpty("No thread-related errors should occur during null effect handling");
    }

    #endregion

    #region Stress Test with Processing

    [Fact]
    public async Task HighVolumeQueueAndProcess_ShouldHandleAllEffects()
    {
        // Arrange
        const int totalEffects = 5000;
        var processedCount = 0;
        var errors = new ConcurrentBag<string>();

        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((notification, _) =>
            {
                Interlocked.Increment(ref processedCount);
            })
            .Returns(Task.CompletedTask);

        var simulationManager = new SimulationManager(mockMediator.Object, Mock.Of<Microsoft.Extensions.Logging.ILogger<SimulationManager>>());

        // Act - High-volume queue and process
        var queueTask = Task.Run(() =>
        {
            var tasks = Enumerable.Range(0, totalEffects).Select(i => Task.Run(() =>
            {
                try
                {
                    var effect = new BlockPlacedEffect(
                        Guid.NewGuid(),
                        new Vector2Int(i % 100, (i / 100) % 100),
                        Domain.Block.BlockType.Basic,
                        DateTime.UtcNow
                    );
                    simulationManager.QueueEffect(effect);
                }
                catch (Exception ex)
                {
                    errors.Add($"Queue error for effect {i}: {ex.Message}");
                }
            }));
            return Task.WhenAll(tasks);
        });

        var processTask = Task.Run(async () =>
        {
            while (processedCount < totalEffects)
            {
                try
                {
                    if (simulationManager.HasPendingEffects)
                    {
                        await simulationManager.ProcessQueuedEffectsAsync();
                    }
                    await Task.Delay(1); // Small delay to prevent busy waiting
                }
                catch (Exception ex)
                {
                    errors.Add($"Process error: {ex.Message}");
                }
            }
        });

        await Task.WhenAll(queueTask, processTask);

        // Final processing
        await simulationManager.ProcessQueuedEffectsAsync();

        // Assert
        _output.WriteLine($"Queued effects: {totalEffects}");
        _output.WriteLine($"Processed effects: {processedCount}");
        _output.WriteLine($"Remaining pending: {simulationManager.PendingEffectCount}");
        _output.WriteLine($"Errors: {errors.Count}");

        errors.Should().BeEmpty("No errors should occur during high-volume processing");
        processedCount.Should().Be(totalEffects, "All effects should be processed");
        simulationManager.HasPendingEffects.Should().BeFalse("No effects should remain pending");
    }

    #endregion

    public void Dispose()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }
}