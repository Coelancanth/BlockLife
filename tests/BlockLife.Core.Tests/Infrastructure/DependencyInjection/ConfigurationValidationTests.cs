using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace BlockLife.Core.Tests.Infrastructure.DependencyInjection;

public class DependentScopedService { }
public class DependentSingletonService
{
    // This singleton incorrectly depends on a scoped service.
    public DependentSingletonService(DependentScopedService scopedService) { }
}

public class ConfigurationValidationTests
{
    [Fact]
    public void BuildServiceProvider_Should_Throw_On_Invalid_Scope_When_Validation_Enabled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<DependentScopedService>();
        services.AddSingleton<DependentSingletonService>();

        var options = new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true };

        // Act
        // With ValidateOnBuild = true, the exception is thrown when the provider is built.
        Action act = () => services.BuildServiceProvider(options);

        // Assert
        act.Should().Throw<AggregateException>() // It's wrapped in an AggregateException
            .WithInnerExceptionExactly<InvalidOperationException>()
            .WithMessage("*Cannot consume scoped service*", "a singleton cannot depend on a scoped service when ValidateOnBuild is true");
    }

    [Fact]
    public void BuildServiceProvider_Should_Throw_On_Unresolvable_Service_When_Validation_Enabled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<DependentSingletonService>(); // Missing DependentTransientService

        var options = new ServiceProviderOptions { ValidateOnBuild = true };

        // Act
        Action act = () => services.BuildServiceProvider(options);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unable to resolve service for type*", "the container should throw if a dependency is not registered when ValidateOnBuild is true");
    }
}
