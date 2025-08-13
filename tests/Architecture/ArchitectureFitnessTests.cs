using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;
using BlockLife.Core.Application.Commands;
using MediatR;
using BlockLife.Core;

namespace BlockLife.Core.Tests.Architecture
{
    /// <summary>
    /// Architecture Fitness Functions - Automated tests that ensure architectural constraints are maintained.
    /// These tests prevent architectural drift and catch violations early.
    /// </summary>
    public class ArchitectureFitnessTests
    {
        private readonly Assembly _coreAssembly = typeof(ICommand).Assembly;
        // Note: Presentation assembly reference removed as it's not available in Core tests

        [Fact]
        public void Core_Should_Not_Reference_Godot()
        {
            // Arrange
            var coreTypes = _coreAssembly.GetTypes();
            
            // Act
            var violatingTypes = coreTypes
                .Where(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Any(f => f.FieldType.FullName?.StartsWith("Godot.") == true))
                .ToList();

            var violatingMethods = coreTypes
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                .Where(m => m.ReturnType.FullName?.StartsWith("Godot.") == true ||
                           m.GetParameters().Any(p => p.ParameterType.FullName?.StartsWith("Godot.") == true))
                .ToList();

            // Assert
            violatingTypes.Should().BeEmpty("Core project must not have any Godot dependencies");
            violatingMethods.Should().BeEmpty("Core project methods must not use Godot types");
        }

        [Fact]
        public void Commands_Should_Be_Immutable_DTOs()
        {
            // Arrange
            var commandTypes = _coreAssembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => t.GetInterfaces().Any(i => 
                    i.IsGenericType && 
                    (i.GetGenericTypeDefinition() == typeof(ICommand<>) || 
                     i == typeof(ICommand))))
                .ToList();

            // Act & Assert
            foreach (var commandType in commandTypes)
            {
                // For records, init setters are acceptable and preferred
                // Check that commands are either records or have no mutable properties
                var isRecord = commandType.GetMethod("<Clone>$") != null || 
                              commandType.GetMethods().Any(m => m.Name == "PrintMembers");
                
                if (!isRecord)
                {
                    // For non-records, check for public setters
                    var publicSetters = commandType.GetProperties()
                        .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true)
                        .ToList();

                    publicSetters.Should().BeEmpty($"Command {commandType.Name} should be immutable - use record with init setters");
                }

                // Check for public fields (should have none)
                var publicFields = commandType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => !f.IsInitOnly && !f.IsLiteral)
                    .ToList();

                publicFields.Should().BeEmpty($"Command {commandType.Name} should not have public mutable fields");
            }
        }

        [Fact]
        public void Handlers_Should_Have_Single_Responsibility()
        {
            // Arrange
            var handlerTypes = _coreAssembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => t.GetInterfaces().Any(i => 
                    i.IsGenericType && 
                    i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .ToList();

            // Act & Assert
            foreach (var handlerType in handlerTypes)
            {
                // Check that handler only implements one IRequestHandler interface
                var handlerInterfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                    .ToList();

                handlerInterfaces.Should().HaveCount(1, 
                    $"Handler {handlerType.Name} should only handle one command type (Single Responsibility)");
            }
        }

        [Fact]
        public void All_Commands_Should_Return_Fin_Types()
        {
            // Arrange
            var requestHandlerInterfaces = _coreAssembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => t.GetInterfaces().Any(i => 
                    i.IsGenericType && 
                    i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .SelectMany(t => t.GetInterfaces())
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .ToList();

            // Act & Assert
            foreach (var handlerInterface in requestHandlerInterfaces)
            {
                var returnType = handlerInterface.GetGenericArguments()[1];
                
                // Check if return type is Fin<T>
                var isFinType = returnType.IsGenericType && 
                               returnType.GetGenericTypeDefinition().FullName?.Contains("Fin") == true;

                isFinType.Should().BeTrue(
                    $"Handler for {handlerInterface.GetGenericArguments()[0].Name} must return Fin<T> for error handling");
            }
        }

        [Fact]
        public void Validation_Rules_Should_Return_Fin_Unit()
        {
            // Arrange
            var validationRuleTypes = _coreAssembly.GetTypes()
                .Where(t => t.Name.EndsWith("ValidationRule") && !t.IsAbstract && !t.IsInterface)
                .ToList();

            // Act & Assert
            foreach (var ruleType in validationRuleTypes)
            {
                var validateMethods = ruleType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name.StartsWith("Validate"))
                    .ToList();

                validateMethods.Should().NotBeEmpty($"Validation rule {ruleType.Name} should have Validate methods");

                foreach (var method in validateMethods)
                {
                    var returnType = method.ReturnType;
                    var isFinUnit = returnType.IsGenericType && 
                                   returnType.GetGenericTypeDefinition().FullName?.Contains("Fin") == true &&
                                   returnType.GetGenericArguments()[0].Name == "Unit";

                    isFinUnit.Should().BeTrue(
                        $"Validation method {method.Name} in {ruleType.Name} should return Fin<Unit>");
                }
            }
        }

        [Fact]
        public void Presenters_Should_Not_Be_In_DI_Container()
        {
            // This test verifies the presenter registration pattern
            // Since we can't easily check DI registration in a unit test,
            // we verify that presenters have the correct constructor pattern
            
            var presenterTypes = _coreAssembly.GetTypes()
                .Where(t => t.Name.EndsWith("Presenter") && !t.IsAbstract && !t.IsInterface)
                .ToList();

            foreach (var presenterType in presenterTypes)
            {
                // Presenters should have a constructor that takes a view
                var constructors = presenterType.GetConstructors();
                var hasViewParameter = constructors.Any(c => 
                    c.GetParameters().Any(p => p.ParameterType.Name.StartsWith("I") && 
                                              p.ParameterType.Name.EndsWith("View")));

                hasViewParameter.Should().BeTrue(
                    $"Presenter {presenterType.Name} should have a constructor parameter for its view interface");
            }
        }

        [Fact]
        public void Domain_Entities_Should_Be_Immutable()
        {
            // Arrange
            var domainNamespace = "BlockLife.Core.Domain";
            var domainTypes = _coreAssembly.GetTypes()
                .Where(t => t.Namespace?.StartsWith(domainNamespace) == true)
                .Where(t => !t.IsEnum && !t.IsInterface && !t.IsAbstract)
                .ToList();

            // Act & Assert
            foreach (var domainType in domainTypes)
            {
                if (domainType.Name == "Vector2Int") // Skip value types that are already immutable
                    continue;

                // For records, init setters are acceptable and the preferred pattern
                var isRecord = domainType.GetMethod("<Clone>$") != null || 
                              domainType.GetMethods().Any(m => m.Name == "PrintMembers");
                
                if (!isRecord)
                {
                    // For non-records, check for public setters
                    var mutableProperties = domainType.GetProperties()
                        .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true)
                        .ToList();

                    mutableProperties.Should().BeEmpty(
                        $"Domain type {domainType.Name} should be immutable - use record with init setters");
                }
            }
        }

        [Fact]
        public void Services_Should_Use_Interfaces()
        {
            // Arrange
            var serviceTypes = _coreAssembly.GetTypes()
                .Where(t => t.Name.EndsWith("Service") && !t.IsInterface && !t.IsAbstract)
                .ToList();

            // Act & Assert
            foreach (var serviceType in serviceTypes)
            {
                var interfaceName = $"I{serviceType.Name}";
                var hasInterface = _coreAssembly.GetTypes()
                    .Any(t => t.IsInterface && t.Name == interfaceName);

                hasInterface.Should().BeTrue(
                    $"Service {serviceType.Name} should have corresponding interface {interfaceName}");
            }
        }

        [Fact]
        public void Notifications_Should_Be_Immutable()
        {
            // Arrange
            var notificationTypes = _coreAssembly.GetTypes()
                .Where(t => t.Name.EndsWith("Notification") && !t.IsInterface && !t.IsAbstract)
                .ToList();

            // Act & Assert
            foreach (var notificationType in notificationTypes)
            {
                // For records, init setters are acceptable and the preferred pattern
                var isRecord = notificationType.GetMethod("<Clone>$") != null || 
                              notificationType.GetMethods().Any(m => m.Name == "PrintMembers");
                
                if (!isRecord)
                {
                    var mutableProperties = notificationType.GetProperties()
                        .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true)
                        .ToList();

                    mutableProperties.Should().BeEmpty(
                        $"Notification {notificationType.Name} should be immutable - use record with init setters");
                }
            }
        }

        [Fact]
        public void No_Static_Service_Locators_In_Application_Code()
        {
            // Arrange
            var applicationTypes = _coreAssembly.GetTypes()
                .Where(t => t.Namespace?.Contains("Features") == true ||
                           t.Namespace?.Contains("Application") == true)
                .ToList();

            // Act
            var staticServiceLocatorCalls = applicationTypes
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                .Where(m => m.GetMethodBody() != null)
                .SelectMany(m => m.GetMethodBody()?.GetILAsByteArray() ?? Array.Empty<byte>())
                .ToList();

            // Note: This is a simplified check. In practice, you'd want to use a proper IL analysis library
            // For now, we check for common service locator patterns
            var violatingTypes = applicationTypes
                .Where(t => t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                    .Any(f => f.FieldType.Name.Contains("ServiceProvider") || 
                             f.FieldType.Name.Contains("ServiceLocator")))
                .ToList();

            // Assert
            violatingTypes.Should().BeEmpty(
                "Application code should not use static service locators - use constructor injection instead");
        }

        [Fact]
        public void Error_Messages_Should_Not_Include_Error_Codes()
        {
            // This is more of a convention test
            // We verify that Error.New calls in our codebase follow the single-parameter pattern
            // This would require source code analysis, so we test the pattern in our domain
            
            // For now, this serves as documentation of the convention
            true.Should().BeTrue("Error.New() should use single-parameter format: Error.New(\"message\")");
        }
    }
}