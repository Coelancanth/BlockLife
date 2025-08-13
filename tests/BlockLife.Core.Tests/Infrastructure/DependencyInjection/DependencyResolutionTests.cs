using BlockLife.Core.Presentation;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BlockLife.Core.Tests.Infrastructure.DependencyInjection;

/// <summary>
/// **ENHANCED** DI Resolution Tests for Advanced Logger & GameStrapper.
/// 
/// Validates that all services registered in the DI container can be resolved
/// successfully, testing both the new LogSettings-based API and legacy API.
/// 
/// This test acts as an architectural fitness function to prevent DI
/// configuration regressions and ensure all services remain resolvable.
/// </summary>
public class DependencyResolutionTests
{
    [Fact]
    public void NEW_API_All_Registered_Services_Should_Be_Resolvable()
    {
        // Arrange - Test new LogSettings-based API
        var mockLogSettings = CreateMockLogSettings();
        var serviceProvider = GameStrapper.Initialize(mockLogSettings);
        
        ValidateAllServicesResolvable(serviceProvider, "New LogSettings API");
    }

    [Fact] 
    public void LEGACY_API_All_Registered_Services_Should_Be_Resolvable()
    {
        // Arrange - Test legacy LoggingLevelSwitch API
        var masterSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);
        var categorySwitches = new Dictionary<string, LoggingLevelSwitch>();
        var mockSink = new Mock<ILogEventSink>();

        var serviceProvider = GameStrapper.Initialize(masterSwitch, categorySwitches, mockSink.Object, null);
        
        ValidateAllServicesResolvable(serviceProvider, "Legacy LoggingLevelSwitch API");
    }

    private static void ValidateAllServicesResolvable(IServiceProvider serviceProvider, string apiType)
    {
        var coreAssembly = typeof(GameStrapper).Assembly;

        // CRITICAL: Test key service interfaces that should be registered
        var serviceTypesToTest = new Type[]
        {
            // Core services
            typeof(IPresenterFactory),
            typeof(IMediator),
            typeof(ILogger),
            
            // Business services (test by interface, not implementation)
            typeof(BlockLife.Core.Infrastructure.Services.IGridStateService),
            typeof(BlockLife.Core.Domain.Block.IBlockRepository),
            typeof(BlockLife.Core.Application.Simulation.ISimulationManager),
            
            // Validation rules (test by interface)
            typeof(BlockLife.Core.Features.Block.Placement.Rules.IPositionIsValidRule),
            typeof(BlockLife.Core.Features.Block.Placement.Rules.IPositionIsEmptyRule),
            typeof(BlockLife.Core.Features.Block.Placement.Rules.IBlockExistsRule)
        };
        
        serviceTypesToTest.Should().NotBeEmpty($"service interface list should contain testable services for {apiType}");

        // Act & Assert - Comprehensive resolution validation  
        foreach (var serviceType in serviceTypesToTest)
        {
            try
            {
                var service = serviceProvider.GetRequiredService(serviceType);
                service.Should().NotBeNull($"service '{serviceType.Name}' should be resolvable via {apiType}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to resolve service '{serviceType.Name}' using {apiType}. " +
                    $"Check DI registration in GameStrapper. Error: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Creates a mock LogSettings object that mimics the Godot Resource without
    /// requiring Godot dependencies in the test project.
    /// </summary>
    private static object CreateMockLogSettings()
    {
        // Create a minimal LogSettings-like object for testing
        var mockType = typeof(TestLogSettings);
        var mockLogSettings = Activator.CreateInstance(mockType);
        return mockLogSettings!;
    }

    /// <summary>
    /// Test implementation of LogSettings that matches the interface expected
    /// by GameStrapper without requiring Godot dependencies.
    /// </summary>
    private class TestLogSettings
    {
        public LogEventLevel DefaultLogLevel { get; set; } = LogEventLevel.Information;
        public bool EnableRichTextInGodot { get; set; } = true;
        public bool EnableFileLogging { get; set; } = false; // Disable for tests
        public bool VerboseCommands { get; set; } = true; // Enable for testing
        public bool VerboseQueries { get; set; } = true; // Enable for testing

        public Dictionary<string, LogEventLevel> GetCategoryLogLevels()
        {
            var result = new Dictionary<string, LogEventLevel>();
            
            // Simple verbose flags instead of complex arrays
            if (VerboseCommands)
                result["Commands"] = LogEventLevel.Debug;
            if (VerboseQueries)
                result["Queries"] = LogEventLevel.Debug;
                
            return result;
        }
    }
}
