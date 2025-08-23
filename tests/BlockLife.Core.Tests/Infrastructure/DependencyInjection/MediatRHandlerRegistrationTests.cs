using BlockLife.Core;
using BlockLife.Core.Infrastructure.Services;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace BlockLife.Core.Tests.Infrastructure.DependencyInjection;

/// <summary>
/// Regression tests for TD_068 and PM_003 - MediatR handler registration issues.
/// These tests ensure that:
/// 1. All handlers are in the correct namespace for auto-discovery
/// 2. All handler dependencies are registered in DI
/// 3. MediatR can discover and instantiate all handlers
/// 
/// Created: 2025-08-23 following namespace DI resolution failure (PM_003)
/// </summary>
public class MediatRHandlerRegistrationTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Assembly _coreAssembly;

    public MediatRHandlerRegistrationTests()
    {
        // Initialize test DI container
        var masterSwitch = new LoggingLevelSwitch(LogEventLevel.Error);
        var categorySwitches = new Dictionary<string, LoggingLevelSwitch>();
        var mockSink = new Mock<ILogEventSink>();
        _serviceProvider = GameStrapper.Initialize(masterSwitch, categorySwitches, mockSink.Object);
        _coreAssembly = typeof(GameStrapper).Assembly;
    }

    [Fact]
    public void All_RequestHandlers_Should_Be_In_Correct_Namespace()
    {
        // Arrange - Find all IRequestHandler implementations
        var handlerTypes = _coreAssembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();

        handlerTypes.Should().NotBeEmpty("Core assembly should contain request handlers");

        // Act & Assert - Verify namespace alignment
        var incorrectNamespaces = new List<string>();
        
        foreach (var handlerType in handlerTypes)
        {
            var namespaceName = handlerType.Namespace ?? string.Empty;
            
            // CRITICAL: All handlers must be in BlockLife.Core namespace hierarchy
            // This ensures MediatR's assembly scanning finds them
            if (!namespaceName.StartsWith("BlockLife.Core"))
            {
                incorrectNamespaces.Add($"{handlerType.Name} in {namespaceName}");
            }
        }

        // Provide detailed error message for debugging
        incorrectNamespaces.Should().BeEmpty(
            $"All handlers must be in BlockLife.Core.* namespace for MediatR auto-discovery. " +
            $"Found {incorrectNamespaces.Count} handlers with incorrect namespaces: " +
            $"{string.Join(", ", incorrectNamespaces)}. " +
            $"See PM_003 for details on this issue.");
    }

    [Fact]
    public void All_RequestHandlers_Should_Have_Dependencies_Registered()
    {
        // Arrange - Find all handler types
        var handlerTypes = _coreAssembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)) &&
                !t.IsAbstract &&
                t.Namespace?.StartsWith("BlockLife.Core") == true)
            .ToList();

        var missingDependencies = new List<string>();

        // Act - Check each handler's constructor dependencies
        foreach (var handlerType in handlerTypes)
        {
            var constructors = handlerType.GetConstructors();
            
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                
                foreach (var parameter in parameters)
                {
                    try
                    {
                        // Try to resolve each dependency
                        var service = _serviceProvider.GetService(parameter.ParameterType);
                        
                        // Special check for critical services that should never be null
                        if (parameter.ParameterType == typeof(IPlayerStateService) && service == null)
                        {
                            missingDependencies.Add(
                                $"{handlerType.Name} requires IPlayerStateService (not registered in GameStrapper)");
                        }
                        else if (service == null && !parameter.HasDefaultValue)
                        {
                            missingDependencies.Add(
                                $"{handlerType.Name} requires {parameter.ParameterType.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        missingDependencies.Add(
                            $"{handlerType.Name} dependency check failed: {ex.Message}");
                    }
                }
            }
        }

        // Assert with detailed diagnostics
        missingDependencies.Should().BeEmpty(
            $"All handler dependencies must be registered. Found {missingDependencies.Count} missing: " +
            $"{string.Join("; ", missingDependencies)}");
    }

    [Fact]
    public void MediatR_Should_Discover_All_Handlers_In_Core_Assembly()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        
        // Find all command/query types (requests) - exclude interfaces and abstract classes
        var requestTypes = _coreAssembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract &&
                       t.GetInterfaces().Any(i => i == typeof(IRequest) || 
                (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))))
            .ToList();

        requestTypes.Should().NotBeEmpty("Core assembly should contain requests");

        // Act & Assert - Verify each request has a discoverable handler
        var unmappedRequests = new List<string>();
        
        foreach (var requestType in requestTypes)
        {
            // Build the handler interface type
            var responseType = requestType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && 
                    i.GetGenericTypeDefinition() == typeof(IRequest<>))
                ?.GetGenericArguments()[0];

            Type handlerInterfaceType;
            if (responseType != null)
            {
                handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            }
            else
            {
                // For IRequest without response type
                handlerInterfaceType = typeof(IRequestHandler<>).MakeGenericType(requestType);
            }

            // Check if handler can be resolved
            try
            {
                var handlers = _serviceProvider.GetServices(handlerInterfaceType);
                if (!handlers.Any())
                {
                    unmappedRequests.Add($"{requestType.Name} has no registered handler");
                }
            }
            catch
            {
                unmappedRequests.Add($"{requestType.Name} handler resolution failed");
            }
        }

        unmappedRequests.Should().BeEmpty(
            $"All requests should have discoverable handlers. Found {unmappedRequests.Count} without handlers: " +
            $"{string.Join(", ", unmappedRequests)}");
    }

    [Fact]
    public void PlayerStateService_Should_Be_Registered_As_Singleton()
    {
        // Regression test for TD_068 - IPlayerStateService was not registered
        
        // Act
        var playerStateService = _serviceProvider.GetService<IPlayerStateService>();
        
        // Assert
        playerStateService.Should().NotBeNull(
            "IPlayerStateService must be registered in GameStrapper.RegisterCoreServices(). " +
            "This was missing in TD_068 causing handler instantiation failures.");
        
        // Verify it's a singleton (consistent with other state services)
        var secondInstance = _serviceProvider.GetService<IPlayerStateService>();
        playerStateService.Should().BeSameAs(secondInstance, 
            "IPlayerStateService should be registered as Singleton to match other state services");
    }

    [Fact]
    public void All_Feature_Folders_Should_Follow_Namespace_Convention()
    {
        // Proactive test to catch future namespace misalignments
        
        // Get all types from Features namespace
        var featureTypes = _coreAssembly.GetTypes()
            .Where(t => t.Namespace?.Contains("Features") == true)
            .ToList();

        var violations = new List<string>();
        
        foreach (var type in featureTypes)
        {
            var namespaceParts = type.Namespace!.Split('.');
            
            // Verify namespace structure: BlockLife.Core.Features.[Feature].[SubFeature]
            if (namespaceParts.Length < 4 || 
                namespaceParts[0] != "BlockLife" || 
                namespaceParts[1] != "Core" || 
                namespaceParts[2] != "Features")
            {
                violations.Add($"{type.Name}: {type.Namespace} (should be BlockLife.Core.Features.*)");
            }
        }

        violations.Should().BeEmpty(
            $"All feature types must follow BlockLife.Core.Features.* namespace pattern. " +
            $"Found {violations.Count} violations: {string.Join("; ", violations)}");
    }

    [Fact]
    public void Handler_Registration_Should_Not_Have_Duplicate_Implementations()
    {
        // Ensure no handler is registered multiple times (can cause resolution issues)
        
        var handlerInterfaces = _coreAssembly.GetTypes()
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            .Distinct()
            .ToList();

        foreach (var handlerInterface in handlerInterfaces)
        {
            var implementations = _serviceProvider.GetServices(handlerInterface).ToList();
            
            implementations.Count.Should().BeLessThanOrEqualTo(1,
                $"Handler interface {handlerInterface.Name} should have at most one implementation registered. " +
                $"Multiple implementations can cause ambiguous resolution.");
        }
    }

    [Theory]
    [InlineData("ApplyMatchRewardsCommand")]
    [InlineData("CreatePlayerCommand")]
    [InlineData("GetCurrentPlayerQuery")]
    public void VS003A_Phase4_Handlers_Should_Be_Registered(string requestTypeName)
    {
        // Specific regression test for VS_003A Phase 4 handlers that failed in TD_068
        
        // Arrange
        var requestType = _coreAssembly.GetTypes()
            .FirstOrDefault(t => t.Name == requestTypeName);
        
        requestType.Should().NotBeNull($"{requestTypeName} should exist in Core assembly");
        
        // Find the handler for this request
        var handlerType = _coreAssembly.GetTypes()
            .FirstOrDefault(t => t.Name == $"{requestTypeName}Handler");
        
        // Assert
        handlerType.Should().NotBeNull($"{requestTypeName}Handler should exist");
        
        handlerType!.Namespace.Should().StartWith("BlockLife.Core.Features",
            $"Handler must be in BlockLife.Core.Features namespace for MediatR discovery (was in wrong namespace in TD_068)");
        
        // Verify handler can be instantiated
        var handlerInterface = handlerType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
        
        handlerInterface.Should().NotBeNull($"{handlerType.Name} should implement IRequestHandler");
        
        var handler = _serviceProvider.GetService(handlerInterface!);
        handler.Should().NotBeNull(
            $"{handlerType.Name} should be resolvable from DI container. " +
            $"Check that all dependencies (especially IPlayerStateService) are registered.");
    }
}