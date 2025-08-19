using BlockLife.Core.Application.Simulation;
using BlockLife.Core.Features.Block.Placement.Effects;
using BlockLife.Core.Domain.Common;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace BlockLife.Core.Tests.Application.Simulation;

/// <summary>
/// Regression tests for HF_002 - Thread-safety in SimulationManager
/// These tests prevent regression to non-thread-safe collections
/// </summary>
public class SimulationManagerRegressionTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<SimulationManager>> _mockLogger;
    private readonly SimulationManager _simulationManager;

    public SimulationManagerRegressionTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<SimulationManager>>();
        _simulationManager = new SimulationManager(_mockMediator.Object, _mockLogger.Object);
    }

    #region Regression Tests - Prevent Return to Non-Thread-Safe Collections

    [Fact]
    public void SimulationManager_EffectQueue_ShouldUseConcurrentQueue()
    {
        // Arrange - Get the private field through reflection
        var effectQueueField = typeof(SimulationManager)
            .GetField("_effectQueue", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var effectQueueValue = effectQueueField?.GetValue(_simulationManager);

        // Assert - Must be ConcurrentQueue, not regular Queue
        effectQueueValue.Should().NotBeNull("_effectQueue field must exist");
        effectQueueValue.Should().BeOfType<ConcurrentQueue<object>>(
            "SimulationManager must use ConcurrentQueue<object> for thread safety. " +
            "If this fails, someone changed it back to Queue<T> - this breaks thread safety!");
    }

    [Fact]
    public void SimulationManager_EffectQueue_ShouldNotBeRegularQueue()
    {
        // Arrange - Get the private field through reflection
        var effectQueueField = typeof(SimulationManager)
            .GetField("_effectQueue", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var effectQueueValue = effectQueueField?.GetValue(_simulationManager);

        // Assert - Explicitly ensure it's NOT System.Collections.Generic.Queue
        effectQueueValue.Should().NotBeNull();
        effectQueueValue.GetType().Name.Should().NotBe("Queue`1", 
            "SimulationManager must NOT use System.Collections.Generic.Queue - this is not thread-safe!");
        
        // Additional check - ensure it's in the correct namespace
        effectQueueValue.GetType().Namespace.Should().Be("System.Collections.Concurrent",
            "Queue implementation must be from System.Collections.Concurrent namespace for thread safety");
    }

    [Fact]
    public void SimulationManager_EffectQueue_ShouldSupportThreadSafeOperations()
    {
        // Arrange
        var effectQueueField = typeof(SimulationManager)
            .GetField("_effectQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        var effectQueue = effectQueueField?.GetValue(_simulationManager);

        // Act & Assert - Verify it has thread-safe methods
        var queueType = effectQueue?.GetType();
        queueType.Should().NotBeNull();

        // ConcurrentQueue has TryDequeue, regular Queue has Dequeue
        var tryDequeueMethod = queueType.GetMethod("TryDequeue");
        tryDequeueMethod.Should().NotBeNull(
            "Queue must have TryDequeue method (ConcurrentQueue) not Dequeue method (regular Queue)");

        // Regular Queue has Count property that's not thread-safe, ConcurrentQueue has thread-safe Count
        var countProperty = queueType.GetProperty("Count");
        countProperty.Should().NotBeNull("Queue must have Count property");
    }

    #endregion

    #region Functional Regression Tests

    [Fact]
    public void QueueEffect_AfterMultipleEffects_ShouldMaintainCorrectCount()
    {
        // Arrange
        var effect1 = new BlockPlacedEffect(Guid.NewGuid(), new Vector2Int(0, 0), BlockLife.Core.Domain.Block.BlockType.Basic, DateTime.UtcNow);
        var effect2 = new BlockPlacedEffect(Guid.NewGuid(), new Vector2Int(1, 1), BlockLife.Core.Domain.Block.BlockType.Work, DateTime.UtcNow);
        var effect3 = new BlockRemovedEffect(Guid.NewGuid(), new Vector2Int(2, 2), BlockLife.Core.Domain.Block.BlockType.Health, DateTime.UtcNow);

        // Act
        var result1 = _simulationManager.QueueEffect(effect1);
        var result2 = _simulationManager.QueueEffect(effect2);
        var result3 = _simulationManager.QueueEffect(effect3);

        // Assert
        result1.IsSucc.Should().BeTrue("First effect should queue successfully");
        result2.IsSucc.Should().BeTrue("Second effect should queue successfully");
        result3.IsSucc.Should().BeTrue("Third effect should queue successfully");
        
        _simulationManager.PendingEffectCount.Should().Be(3, "All three effects should be queued");
        _simulationManager.HasPendingEffects.Should().BeTrue("Should have pending effects");
    }

    [Fact]
    public async Task ProcessQueuedEffectsAsync_ShouldProcessAllEffectsUsingTryDequeue()
    {
        // Arrange
        var effect1 = new BlockPlacedEffect(Guid.NewGuid(), new Vector2Int(0, 0), BlockLife.Core.Domain.Block.BlockType.Basic, DateTime.UtcNow);
        var effect2 = new BlockRemovedEffect(Guid.NewGuid(), new Vector2Int(1, 1), BlockLife.Core.Domain.Block.BlockType.Work, DateTime.UtcNow);
        
        _simulationManager.QueueEffect(effect1);
        _simulationManager.QueueEffect(effect2);

        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _simulationManager.ProcessQueuedEffectsAsync();

        // Assert
        _simulationManager.HasPendingEffects.Should().BeFalse("All effects should be processed");
        _simulationManager.PendingEffectCount.Should().Be(0, "Queue should be empty after processing");
        
        // Verify both effects were published
        _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), default), Times.Exactly(2));
    }

    [Fact]
    public void QueueEffect_WithNullEffect_ShouldHandleGracefully()
    {
        // Arrange
        BlockPlacedEffect? nullEffect = null;

        // Act & Assert
        // This test verifies the queue can handle null values without throwing
        // The actual behavior (success/failure) depends on implementation
        // The key is that it doesn't crash with threading issues
        var action = () => _simulationManager.QueueEffect(nullEffect!);
        action.Should().NotThrow("QueueEffect should handle null gracefully without throwing threading exceptions");
    }

    #endregion

    #region Thread-Safety Verification Tests

    [Fact]
    public async Task ConcurrentQueueOperations_ShouldNotCorruptState()
    {
        // Arrange
        const int threadCount = 10;
        const int effectsPerThread = 50;
        var tasks = new Task[threadCount];

        // Act - Multiple threads queuing effects simultaneously
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i; // Capture for closure
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < effectsPerThread; j++)
                {
                    var effect = new BlockPlacedEffect(
                        Guid.NewGuid(),
                        new Vector2Int(threadId, j),
                        BlockLife.Core.Domain.Block.BlockType.Basic,
                        DateTime.UtcNow
                    );
                    
                    var result = _simulationManager.QueueEffect(effect);
                    result.IsSucc.Should().BeTrue($"Effect queuing should succeed for thread {threadId}, effect {j}");
                }
            });
        }

        await Task.WhenAll(tasks);

        // Assert
        _simulationManager.PendingEffectCount.Should().Be(threadCount * effectsPerThread,
            "All effects from all threads should be queued");
        _simulationManager.HasPendingEffects.Should().BeTrue("Should have pending effects");
    }

    [Fact]
    public async Task ConcurrentQueueAndProcess_ShouldMaintainDataIntegrity()
    {
        // Arrange
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), default))
            .Returns(Task.CompletedTask);

        const int producerThreads = 5;
        const int effectsPerProducer = 20;
        var allTasks = new Task[producerThreads + 1]; // +1 for consumer

        // Act - Concurrent producers and consumer
        for (int i = 0; i < producerThreads; i++)
        {
            int threadId = i;
            allTasks[i] = Task.Run(async () =>
            {
                for (int j = 0; j < effectsPerProducer; j++)
                {
                    var effect = new BlockPlacedEffect(
                        Guid.NewGuid(),
                        new Vector2Int(threadId, j),
                        BlockLife.Core.Domain.Block.BlockType.Basic,
                        DateTime.UtcNow
                    );
                    
                    _simulationManager.QueueEffect(effect);
                    
                    // Small delay to create realistic interleaving
                    await Task.Delay(1);
                }
            });
        }

        // Consumer task that processes effects while they're being added
        allTasks[producerThreads] = Task.Run(async () =>
        {
            int processedCount = 0;
            while (processedCount < producerThreads * effectsPerProducer)
            {
                if (_simulationManager.HasPendingEffects)
                {
                    await _simulationManager.ProcessQueuedEffectsAsync();
                    processedCount = (producerThreads * effectsPerProducer) - _simulationManager.PendingEffectCount;
                }
                await Task.Delay(10); // Allow producers to add more effects
            }
        });

        await Task.WhenAll(allTasks);

        // Final cleanup
        await _simulationManager.ProcessQueuedEffectsAsync();

        // Assert
        _simulationManager.HasPendingEffects.Should().BeFalse("All effects should be processed");
        
        // Verify all effects were published (some might be processed multiple times, but at least once each)
        _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), default), 
            Times.AtLeast(producerThreads * effectsPerProducer));
    }

    #endregion
}