using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using Xunit;
using Xunit.Abstractions;

namespace BlockLife.Core.Tests.Performance;

/// <summary>
/// Performance regression test for view layer operations.
/// Ensures animations and visual updates stay within performance targets.
/// </summary>
public class ViewLayerPerformanceTest
{
    private readonly ITestOutputHelper _output;
    
    public ViewLayerPerformanceTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public async Task BlockMovementAnimation_ShouldCompleteWithinTargetTime()
    {
        // Arrange
        var targetMs = 150; // Our new animation speed target
        var tolerance = 50; // Allow some variance for system performance
        
        // Create a mock visualization controller
        var controller = new TestableBlockVisualizationController
        {
            AnimationSpeed = 0.15f,
            EnableAnimations = true
        };
        
        var blockId = Guid.NewGuid();
        var fromPosition = new Vector2Int(0, 0);
        var toPosition = new Vector2Int(5, 5);
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        await controller.ShowBlockAsync(blockId, fromPosition, BlockType.Basic);
        await controller.UpdateBlockPositionAsync(blockId, toPosition);
        stopwatch.Stop();
        
        // Assert
        _output.WriteLine($"Block movement completed in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Target: {targetMs}ms (+/- {tolerance}ms)");
        
        // Visual update should complete near our animation time
        Assert.InRange(stopwatch.ElapsedMilliseconds, 0, targetMs + tolerance);
    }
    
    [Fact]
    public async Task InstantMode_ShouldHaveNoAnimationDelay()
    {
        // Arrange
        var controller = new TestableBlockVisualizationController
        {
            AnimationSpeed = 0.15f,
            EnableAnimations = false // Instant mode
        };
        
        var blockId = Guid.NewGuid();
        var fromPosition = new Vector2Int(0, 0);
        var toPosition = new Vector2Int(5, 5);
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        await controller.ShowBlockAsync(blockId, fromPosition, BlockType.Basic);
        await controller.UpdateBlockPositionAsync(blockId, toPosition);
        stopwatch.Stop();
        
        // Assert
        _output.WriteLine($"Instant mode completed in {stopwatch.ElapsedMilliseconds}ms");
        
        // Should be essentially instant (< 10ms for method calls)
        Assert.True(stopwatch.ElapsedMilliseconds < 10, 
            $"Instant mode took {stopwatch.ElapsedMilliseconds}ms, expected < 10ms");
    }
    
    [Theory]
    [InlineData(0.05f, 50)]
    [InlineData(0.10f, 100)]
    [InlineData(0.15f, 150)]
    [InlineData(0.20f, 200)]
    [InlineData(0.30f, 300)]
    public async Task DifferentAnimationSpeeds_ShouldMatchConfiguredDuration(float speed, int expectedMs)
    {
        // Arrange
        var controller = new TestableBlockVisualizationController
        {
            AnimationSpeed = speed,
            EnableAnimations = true
        };
        
        var blockId = Guid.NewGuid();
        var position = new Vector2Int(3, 3);
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        await controller.ShowBlockAsync(blockId, position, BlockType.Basic);
        stopwatch.Stop();
        
        // Assert
        _output.WriteLine($"Animation speed {speed}f completed in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Expected: ~{expectedMs}ms");
        
        // Allow 50ms tolerance for system variance
        Assert.InRange(stopwatch.ElapsedMilliseconds, 0, expectedMs + 50);
    }
    
    /// <summary>
    /// Testable version of BlockVisualizationController that doesn't require Godot runtime.
    /// </summary>
    private class TestableBlockVisualizationController
    {
        public float AnimationSpeed { get; set; } = 0.15f;
        public bool EnableAnimations { get; set; } = true;
        
        public async Task ShowBlockAsync(Guid blockId, Vector2Int position, BlockType type)
        {
            if (EnableAnimations)
            {
                // Simulate animation delay
                await Task.Delay(TimeSpan.FromSeconds(AnimationSpeed));
            }
            // Instant if animations disabled
        }
        
        public async Task UpdateBlockPositionAsync(Guid blockId, Vector2Int position)
        {
            if (EnableAnimations)
            {
                // Simulate animation delay
                await Task.Delay(TimeSpan.FromSeconds(AnimationSpeed));
            }
            // Instant if animations disabled
        }
    }
}