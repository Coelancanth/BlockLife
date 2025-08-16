using BlockLife.Core.Application.Simulation;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Features.Block.Placement.Effects;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Core;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BlockLife.Core.Tests.StressTests;

/// <summary>
/// Simplified stress tests to validate thread-safety fixes.
/// </summary>
public class SimplifiedStressTest : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly IGridStateService _gridState;
    private readonly ISimulationManager _simulation;
    private readonly Random _random = new();

    public SimplifiedStressTest(ITestOutputHelper output)
    {
        _output = output;

        // Initialize with minimal logging configuration
        var masterSwitch = new LoggingLevelSwitch(LogEventLevel.Warning);
        var categorySwitches = new Dictionary<string, LoggingLevelSwitch>();
        var mockSink = new Mock<ILogEventSink>();
        _serviceProvider = GameStrapper.Initialize(masterSwitch, categorySwitches, mockSink.Object);

        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _gridState = _serviceProvider.GetRequiredService<IGridStateService>();
        _simulation = _serviceProvider.GetRequiredService<ISimulationManager>();
    }

    [Fact]
    public async Task ConcurrentBlockOperations_ShouldMaintainDataIntegrity()
    {
        // Arrange
        const int threadCount = 50;
        const int operationsPerThread = 10;
        var errors = new ConcurrentBag<string>();

        // Act - Simulate concurrent users
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(async () =>
        {
            for (int i = 0; i < operationsPerThread; i++)
            {
                try
                {
                    var position = new Vector2Int(_random.Next(0, 10), _random.Next(0, 10));

                    // Place and remove blocks randomly
                    var placeResult = await _mediator.Send(new PlaceBlockCommand(position, Domain.Block.BlockType.Basic));
                    if (placeResult.IsSucc)
                    {
                        await Task.Delay(_random.Next(1, 5));
                        await _mediator.Send(new RemoveBlockCommand(position));
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Thread {threadId}: {ex.Message}");
                }
            }
        }));

        await Task.WhenAll(tasks);

        // Assert - No critical errors
        _output.WriteLine($"Completed {threadCount * operationsPerThread} operations with {errors.Count} errors");

        // Check for critical errors (rollback failures, state corruption)
        var criticalErrors = errors.Where(e => e.Contains("Critical") || e.Contains("rollback")).ToList();
        Assert.Empty(criticalErrors);

        // Verify state consistency
        var allBlocks = _gridState.GetAllBlocks();
        foreach (var block in allBlocks)
        {
            // Each block should be retrievable by both ID and position
            var blockById = _gridState.GetBlockById(block.Id);
            Assert.True(blockById.IsSome);

            var blockByPos = _gridState.GetBlockAt(block.Position);
            Assert.True(blockByPos.IsSome);
        }
    }

    [Fact]
    public async Task ConcurrentQueueOperations_ShouldBeThreadSafe()
    {
        // Arrange
        const int threadCount = 100;
        const int effectsPerThread = 50;
        var errors = new ConcurrentBag<string>();

        // Act - Queue effects from multiple threads
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            for (int i = 0; i < effectsPerThread; i++)
            {
                try
                {
                    var effect = new BlockPlacedEffect(
                        Guid.NewGuid(),
                        new Vector2Int(threadId, i),
                        Domain.Block.BlockType.Basic,
                        DateTime.UtcNow
                    );

                    _simulation.QueueEffect(effect);
                }
                catch (Exception ex)
                {
                    errors.Add($"Queue error: {ex.Message}");
                }
            }
        }));

        await Task.WhenAll(tasks);

        // Process all effects
        await _simulation.ProcessQueuedEffectsAsync();

        // Assert
        Assert.Empty(errors);
        Assert.False(_simulation.HasPendingEffects);

        _output.WriteLine($"Successfully processed {threadCount * effectsPerThread} effects");
    }

    [Fact]
    public async Task RapidMoveOperations_ShouldNotCorruptState()
    {
        // Arrange - Place initial blocks in a grid
        var blocks = new List<Guid>();
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                var result = await _mediator.Send(new PlaceBlockCommand(
                    new Vector2Int(x, y),
                    Domain.Block.BlockType.Basic
                ));

                if (result.IsSucc)
                {
                    var block = _gridState.GetBlockAt(new Vector2Int(x, y));
                    if (block.IsSome)
                    {
                        blocks.Add(block.Match(b => b.Id, () => Guid.Empty));
                    }
                }
            }
        }

        var errors = new ConcurrentBag<string>();

        // Act - Concurrent moves
        var moveTasks = blocks.Select(blockId => Task.Run(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var block = _gridState.GetBlockById(blockId);
                    if (block.IsSome)
                    {
                        var currentPos = block.Match(b => b.Position, () => default);
                        var newPos = new Vector2Int(
                            _random.Next(0, 10),
                            _random.Next(0, 10)
                        );

                        _gridState.MoveBlock(currentPos, newPos);
                    }
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Critical"))
                {
                    errors.Add($"CRITICAL: {ex.Message}");
                }
                catch (Exception)
                {
                    // Non-critical errors (position occupied, etc.) are expected
                }
            }
        }));

        await Task.WhenAll(moveTasks);

        // Assert - No critical rollback failures
        Assert.Empty(errors);

        _output.WriteLine($"Completed rapid moves with no critical failures");
    }

    [Fact]
    public async Task AdjacentBlockQueries_ShouldBeEfficient()
    {
        // Arrange - Create dense grid
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                await _mediator.Send(new PlaceBlockCommand(
                    new Vector2Int(x, y),
                    Domain.Block.BlockType.Basic
                ));
            }
        }

        var totalTime = TimeSpan.Zero;
        var queryCount = 0;

        // Act - Concurrent queries
        var tasks = Enumerable.Range(0, 50).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                var position = new Vector2Int(_random.Next(0, 10), _random.Next(0, 10));

                var start = DateTime.UtcNow;
                var adjacent = _gridState.GetAdjacentBlocks(position);
                var elapsed = DateTime.UtcNow - start;

                lock (_random)
                {
                    totalTime = totalTime.Add(elapsed);
                    queryCount++;
                }
            }
        }));

        await Task.WhenAll(tasks);

        // Assert - Performance check
        var avgMs = totalTime.TotalMilliseconds / queryCount;
        _output.WriteLine($"Performed {queryCount} queries, avg: {avgMs:F3}ms");

        // With O(4) optimization, should be very fast
        Assert.True(avgMs < 1.0, $"Queries too slow: {avgMs:F3}ms");
    }

    public void Dispose()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }
}
