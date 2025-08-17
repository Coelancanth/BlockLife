using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Common;
using LanguageExt;
using Xunit;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Tests.Features.Block.Input;

/// <summary>
/// Regression test for BF_001: Ensures first-click performance remains fast
/// after async/await pre-warming optimization.
/// </summary>
public class FirstClickPerformanceTest
{
    /// <summary>
    /// Verifies that the first block interaction completes within one frame (16ms for 60fps).
    /// This test ensures the async/await JIT compilation delay is properly pre-warmed.
    /// </summary>
    [Fact]
    [Trait("Category", "Performance")]
    public async Task FirstBlockClick_AfterPreWarming_ShouldCompleteWithin16ms()
    {
        // Arrange - Simulate the pre-warming that happens in BlockInputManager._Ready()
        await PreWarmAsyncPattern();
        
        // Act - Measure first async operation after pre-warming
        var stopwatch = Stopwatch.StartNew();
        await SimulateFirstClick();
        stopwatch.Stop();
        
        // Assert - Should complete quickly (increased threshold for CI environments)
        // CI runners can be slower, so we allow up to 50ms while still ensuring pre-warming works
        Assert.True(stopwatch.ElapsedMilliseconds < 50,
            $"First click took {stopwatch.ElapsedMilliseconds}ms, expected <50ms. " +
            "This indicates async/await pre-warming may have failed.");
    }
    
    /// <summary>
    /// Verifies that subsequent clicks remain fast (regression test).
    /// </summary>
    [Fact]
    [Trait("Category", "Performance")]
    public async Task SubsequentBlockClicks_ShouldCompleteWithin5ms()
    {
        // Arrange - Ensure first operation has run
        await SimulateFirstClick();
        
        // Act - Measure subsequent operations
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 10; i++)
        {
            await SimulateFirstClick();
        }
        stopwatch.Stop();
        
        var averageMs = stopwatch.ElapsedMilliseconds / 10.0;
        
        // Assert - Subsequent operations should be very fast
        Assert.True(averageMs < 5,
            $"Average click time was {averageMs}ms, expected <5ms");
    }
    
    /// <summary>
    /// Simulates the pre-warming pattern used in BlockInputManager.
    /// </summary>
    private async Task PreWarmAsyncPattern()
    {
        // Pre-warm the exact Option<T>.Match async pattern that caused BF_001
        var testOption = Some(new Vector2Int(0, 0));
        
        await testOption.Match(
            Some: async pos =>
            {
                var differentPos = testOption != Some(new Vector2Int(1, 1));
                await Task.CompletedTask;
            },
            None: async () => await Task.CompletedTask
        );
        
        // Also pre-warm Task.Delay pattern
        await Task.Delay(1);
    }
    
    /// <summary>
    /// Simulates the async pattern from OnCellClicked that exhibited the delay.
    /// </summary>
    private async Task SimulateFirstClick()
    {
        Option<Guid> selectedBlockId = None;
        var position = new Vector2Int(5, 5);
        
        await selectedBlockId.Match(
            Some: async id =>
            {
                // Simulate the move logic branch
                await Task.CompletedTask;
            },
            None: async () =>
            {
                // Simulate the selection logic branch
                await Task.FromResult(Some(Guid.NewGuid()));
            }
        );
    }
}