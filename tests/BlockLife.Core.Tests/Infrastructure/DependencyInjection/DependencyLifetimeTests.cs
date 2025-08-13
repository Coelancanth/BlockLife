using BlockLife.Core.Presentation;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Linq;
using Moq;
using Xunit;
using Serilog.Core;
using System.Collections.Generic;
using Serilog.Events;

namespace BlockLife.Core.Tests.Infrastructure.DependencyInjection;

public class DependencyLifetimeTests
{
    private readonly IServiceProvider _serviceProvider;

    public DependencyLifetimeTests()
    {
        var masterSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);
        var categorySwitches = new Dictionary<string, LoggingLevelSwitch>();
        var mockSink = new Mock<ILogEventSink>();
        _serviceProvider = GameStrapper.Initialize(masterSwitch, categorySwitches, mockSink.Object);
    }

    [Fact]
    public void Key_Singleton_Services_Should_Always_Return_Same_Instance()
    {
        // Act
        var logger1 = _serviceProvider.GetRequiredService<ILogger>();
        var logger2 = _serviceProvider.GetRequiredService<ILogger>();

        var factory1 = _serviceProvider.GetRequiredService<IPresenterFactory>();
        var factory2 = _serviceProvider.GetRequiredService<IPresenterFactory>();
        
        // Assert
        logger1.Should().BeSameAs(logger2, "singletons should be the same instance");
        factory1.Should().BeSameAs(factory2, "singletons should be the same instance");
    }

    [Fact]
    public void Presenters_Should_Always_Return_New_Instance()
    {
        // Arrange
        var presenterFactory = _serviceProvider.GetRequiredService<IPresenterFactory>();
        
        var presenterTypeToTest = typeof(GameStrapper).Assembly.GetTypes().FirstOrDefault(t =>
            !t.IsAbstract && !t.IsInterface &&
            t.BaseType is { IsGenericType: true } && 
            t.BaseType.GetGenericTypeDefinition() == typeof(PresenterBase<>)
        );

        // This test is only valid if we have at least one presenter to test.
        if (presenterTypeToTest is null)
        {
            return; // No presenters to test, so we can pass.
        }

        var viewInterfaceType = presenterTypeToTest.BaseType?.GetGenericArguments()[0];
        viewInterfaceType.Should().NotBeNull("presenter base type should have a generic argument");
        
        var createMethod = typeof(IPresenterFactory).GetMethod(nameof(IPresenterFactory.Create))?
            .MakeGenericMethod(presenterTypeToTest, viewInterfaceType!);
        createMethod.Should().NotBeNull("IPresenterFactory should have a Create method");

        // Use reflection to create an instance of Mock<T> where T is the viewInterfaceType
        var mockType = typeof(Mock<>).MakeGenericType(viewInterfaceType!);
        var mockInstance = (Mock?)Activator.CreateInstance(mockType);
        mockInstance.Should().NotBeNull("an instance of the mock view should be creatable");
        var mockView = mockInstance!.Object;

        // Act
        var presenter1 = createMethod!.Invoke(presenterFactory, new[] { mockView });
        var presenter2 = createMethod.Invoke(presenterFactory, new[] { mockView });

        // Assert
        presenter1.Should().NotBeNull();
        presenter2.Should().NotBeNull();
        presenter1.Should().NotBeSameAs(presenter2, "transient services should be different instances");
    }
}
