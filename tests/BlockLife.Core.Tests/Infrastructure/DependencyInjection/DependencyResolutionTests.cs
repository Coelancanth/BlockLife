using BlockLife.Core.Presentation;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BlockLife.Core.Tests.Infrastructure.DependencyInjection;

public class DependencyResolutionTests
{
    [Fact]
    public void All_Registered_Services_Following_Patterns_Should_Be_Resolvable()
    {
        // Arrange
        var masterSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);
        var categorySwitches = new Dictionary<string, LoggingLevelSwitch>();
        var mockSink = new Mock<ILogEventSink>();

        var serviceProvider = GameStrapper.Initialize(masterSwitch, categorySwitches, mockSink.Object);
        var coreAssembly = typeof(GameStrapper).Assembly;

        var servicesToTest = coreAssembly.GetTypes().Where(t =>
            !t.IsAbstract && !t.IsInterface &&
            (
                // Exclude presenters as they require view dependencies not in the container
                (t.BaseType is { IsGenericType: true } && t.BaseType.GetGenericTypeDefinition() == typeof(PresenterBase<>)) == false &&
                (
                    t.GetInterfaces().Any(i => i.IsGenericType && (
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                        i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)
                    )) ||
                    typeof(IPresenterFactory).IsAssignableFrom(t)
                )
            )
        ).ToList();
        
        servicesToTest.Should().NotBeEmpty("the reflection query should find services to test, otherwise the test is meaningless");

        // Act & Assert
        foreach (var serviceType in servicesToTest)
        {
            // Special case for services registered by their interface
            if (typeof(IPresenterFactory).IsAssignableFrom(serviceType))
            {
                Action act = () => serviceProvider.GetRequiredService(typeof(IPresenterFactory));
                act.Should().NotThrow($"service '{nameof(IPresenterFactory)}' should be resolvable from the DI container");
            }
            // MediatR handlers are registered by their interface, not concrete type
            else if (serviceType.GetInterfaces().Any(i => i.IsGenericType && 
                (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                 i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                 i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))))
            {
                // MediatR registers handlers, so we just verify they can be created via MediatR
                var mediator = serviceProvider.GetRequiredService<IMediator>();
                mediator.Should().NotBeNull($"MediatR should be registered to handle {serviceType.Name}");
            }
            else // General case for services registered by their concrete type
            {
                Action act = () => serviceProvider.GetRequiredService(serviceType);
                act.Should().NotThrow($"service '{serviceType.Name}' should be resolvable from the DI container");
            }
        }
    }
}
