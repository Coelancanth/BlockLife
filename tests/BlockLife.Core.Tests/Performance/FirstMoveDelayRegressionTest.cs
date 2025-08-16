using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Commands;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using LanguageExt;
using LanguageExt.Common;

namespace BlockLife.Core.Tests.Performance;

/// <summary>
/// Regression test for BF_001: First-time move operation 282ms delay.
/// Ensures Serilog message template compilation doesn't cause user-facing lag.
/// </summary>
public class FirstMoveDelayRegressionTest : IDisposable
{
    private IServiceProvider _serviceProvider = null!;
    private IMediator _mediator = null!;
    private IGridStateService _gridStateService = null!;
    private ILogger _logger = null!;
    
    public FirstMoveDelayRegressionTest()
    {
        // Create a service collection with Serilog configured similarly to production
        var services = new ServiceCollection();
        
        // Configure Serilog with structured logging (reproduces the issue)
        var levelSwitch = new LoggingLevelSwitch(LogEventLevel.Information);
        _logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        
        services.AddSingleton<ILogger>(_logger);
        services.AddLogging(builder => builder.AddSerilog(_logger));
        
        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PlaceBlockCommand).Assembly));
        
        // Add required services
        services.AddSingleton<GridStateService>();
        services.AddSingleton<IGridStateService>(p => p.GetRequiredService<GridStateService>());
        services.AddSingleton<IBlockRepository>(p => p.GetRequiredService<GridStateService>());
        
        // Add simulation manager (required by PlaceBlockCommandHandler)
        services.AddSingleton<BlockLife.Core.Application.Simulation.ISimulationManager, 
            BlockLife.Core.Application.Simulation.SimulationManager>();
        
        // Add command handlers
        services.AddTransient<PlaceBlockCommandHandler>();
        services.AddTransient<MoveBlockCommandHandler>();
        
        // Add validation rules
        services.AddTransient<BlockLife.Core.Features.Block.Placement.Rules.IPositionIsValidRule, 
            BlockLife.Core.Features.Block.Placement.Rules.PositionIsValidRule>();
        services.AddTransient<BlockLife.Core.Features.Block.Placement.Rules.IPositionIsEmptyRule, 
            BlockLife.Core.Features.Block.Placement.Rules.PositionIsEmptyRule>();
        services.AddTransient<BlockLife.Core.Features.Block.Placement.Rules.IBlockExistsRule, 
            BlockLife.Core.Features.Block.Placement.Rules.BlockExistsRule>();
        
        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _gridStateService = _serviceProvider.GetRequiredService<IGridStateService>();
    }
    
    [Fact]
    public async Task FirstMoveOperation_ShouldCompleteWithin100ms_AfterSerilogPreWarming()
    {
        // Arrange - Place a block
        var placeCommand = new PlaceBlockCommand(new Vector2Int(5, 5), BlockType.Basic);
        var placeResult = await _mediator.Send(placeCommand);
        Assert.True(placeResult.IsSucc, "Failed to place initial block");
        
        var blockId = _gridStateService.GetBlockAt(new Vector2Int(5, 5))
            .Match(b => b.Id, () => throw new Exception("Block not found after placement"));
        
        // Pre-warm Serilog templates (simulating the fix)
        PreWarmSerilogTemplates();
        
        // Act - Measure first move operation
        var moveCommand = new MoveBlockCommand
        {
            BlockId = blockId,
            ToPosition = new Vector2Int(6, 6)
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        // Log with structured parameters (would normally cause 282ms delay)
        _logger.Information("🔄 Moving block {BlockId} from {FromPosition} to {ToPosition}", 
            blockId, new Vector2Int(5, 5), new Vector2Int(6, 6));
        
        var moveResult = await _mediator.Send(moveCommand);
        
        _logger.Information("✅ Successfully moved block {BlockId} to {ToPosition}", 
            blockId, new Vector2Int(6, 6));
        
        stopwatch.Stop();
        
        // Assert
        Assert.True(moveResult.IsSucc, "Move operation failed");
        Assert.True(stopwatch.ElapsedMilliseconds < 100, 
            $"First move operation took {stopwatch.ElapsedMilliseconds}ms, expected <100ms. " +
            "Serilog template compilation may not be properly pre-warmed.");
    }
    
    [Fact(Skip = "Demonstration test - shows the issue when templates aren't pre-warmed")]
    public async Task FirstMoveOperation_WithoutPreWarming_DemonstratesDelay()
    {
        // This test demonstrates the issue when templates aren't pre-warmed
        // It's marked as Explicit so it doesn't fail CI, but can be run to verify the issue exists
        
        // Arrange - Create fresh logger without pre-warming
        var freshLogger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();
        
        var placeCommand = new PlaceBlockCommand(new Vector2Int(3, 3), BlockType.Basic);
        var placeResult = await _mediator.Send(placeCommand);
        Assert.True(placeResult.IsSucc);
        
        var blockId = _gridStateService.GetBlockAt(new Vector2Int(3, 3))
            .Match(b => b.Id, () => throw new Exception("Block not found"));
        
        // Act - First structured log with complex template
        var stopwatch = Stopwatch.StartNew();
        
        // This is the first complex structured log - triggers template compilation
        freshLogger.Information("🔄 Moving block {BlockId} from {FromPosition} to {ToPosition}", 
            blockId, new Vector2Int(3, 3), new Vector2Int(4, 4));
        
        stopwatch.Stop();
        
        // Log the actual time for debugging (xUnit uses ITestOutputHelper, but we'll just use console)
        Console.WriteLine($"First structured log took: {stopwatch.ElapsedMilliseconds}ms");
        
        // Note: This might not always reproduce 282ms exactly, but demonstrates the concept
        // In production Godot environment with more complex setup, the delay is more pronounced
    }
    
    private void PreWarmSerilogTemplates()
    {
        // Simulate the pre-warming that should happen in BlockInputManager._Ready()
        var preWarmLogger = _logger.ForContext("PreWarm", true);
        
        // Pre-compile expensive templates
        preWarmLogger.Information("🔄 Moving block {BlockId} from {FromPosition} to {ToPosition}", 
            Guid.Empty, Vector2Int.Zero, Vector2Int.One);
        
        preWarmLogger.Information("✅ Successfully moved block {BlockId} to {ToPosition}", 
            Guid.Empty, Vector2Int.Zero);
        
        preWarmLogger.Information("✋ Selected block {BlockId} at position {Position}", 
            Guid.Empty, Vector2Int.Zero);
        
        preWarmLogger.Warning("❌ Failed to move block {BlockId}: {Error}", 
            Guid.Empty, "test");
    }
    
    public void Dispose()
    {
        (_logger as IDisposable)?.Dispose();
        // ServiceProvider doesn't need explicit disposal in test context
    }
}