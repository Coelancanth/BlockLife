using System.Reflection;
using FluentAssertions;
using Xunit;
using BlockLife.Core;
using System.Collections.Concurrent;

namespace BlockLife.Core.Tests.Architecture;

public class ArchitectureTests
{
    private readonly Assembly _coreAssembly = typeof(GameStrapper).Assembly;

    [Fact]
    public void Core_Should_Not_Reference_Godot()
    {
        // Ensures Core layer remains pure C#
        var godotReferences = _coreAssembly.GetReferencedAssemblies()
            .Where(a => a.Name?.Contains("Godot", StringComparison.OrdinalIgnoreCase) ?? false);

        godotReferences.Should().BeEmpty("Core layer must not depend on Godot");
    }

    [Fact]
    public void Commands_Should_Not_Contain_Logic()
    {
        // Commands should be pure DTOs
        var commandTypes = _coreAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Command"));

        foreach (var commandType in commandTypes)
        {
            var methods = commandType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName &&
                           !m.Name.Equals("ToString") &&
                           !m.Name.Equals("GetHashCode") &&
                           !m.Name.Equals("Equals") &&
                           !m.Name.Contains("Clone") &&
                           !m.Name.Equals("PrintMembers") &&
                           !m.Name.Equals("Deconstruct")); // Exclude property getters/setters and record methods

            methods.Should().BeEmpty($"{commandType.Name} should not have business logic methods (pure DTO)");
        }
    }

    [Fact]
    public void Handlers_Should_Return_Fin_Types()
    {
        // All handlers should use Fin<T> for error handling
        var handlerTypes = _coreAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Handler"));

        foreach (var handlerType in handlerTypes)
        {
            var handleMethod = handlerType.GetMethod("Handle");
            if (handleMethod != null)
            {
                var returnType = handleMethod.ReturnType;
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var innerType = returnType.GetGenericArguments()[0];
                    innerType.Name.Should().StartWith("Fin",
                        $"{handlerType.Name}.Handle should return Task<Fin<T>> for proper error handling");
                }
            }
        }
    }

    [Fact]
    public void Presenters_Should_Not_Have_State_Fields()
    {
        // Presenters should only have injected dependencies  
        var presenterTypes = _coreAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Presenter"));

        foreach (var presenterType in presenterTypes)
        {
            var fields = presenterType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => !f.IsInitOnly && !f.Name.StartsWith("_") && !f.Name.Contains("BackingField")); // Exclude readonly/injected deps and compiler-generated

            fields.Should().BeEmpty($"{presenterType.Name} should not have mutable state fields");
        }
    }

    [Fact]
    public void ViewInterfaces_Should_Not_Expose_Godot_Types()
    {
        // IView interfaces should use primitive/DTO types only
        var viewInterfaces = _coreAssembly.GetTypes()
            .Where(t => t.IsInterface && t.Name.EndsWith("View"));

        foreach (var viewInterface in viewInterfaces)
        {
            var methods = viewInterface.GetMethods();
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                foreach (var param in parameters)
                {
                    param.ParameterType.Assembly.FullName.Should().NotContain("Godot",
                        $"{viewInterface.Name}.{method.Name} exposes Godot type {param.ParameterType.Name}");
                }
            }
        }
    }

    [Fact]
    public void Core_Assembly_Should_Use_Thread_Safe_Collections()
    {
        // Regression test for HF_002 - Prevent use of non-thread-safe collections
        // This enforces that all Queue usage in Core assembly uses ConcurrentQueue

        var violations = new List<string>();
        var allTypes = _coreAssembly.GetTypes();

        foreach (var type in allTypes)
        {
            // Check private fields for Queue usage
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var field in fields)
            {
                CheckFieldForThreadSafetyViolations(field, type, violations);
            }

            // Check public fields (less common but possible)
            var publicFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (var field in publicFields)
            {
                CheckFieldForThreadSafetyViolations(field, type, violations);
            }

            // Check properties for Queue usage
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var property in properties)
            {
                CheckPropertyForThreadSafetyViolations(property, type, violations);
            }
        }

        violations.Should().BeEmpty($"Found thread-safety violations in Core assembly:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    private static void CheckFieldForThreadSafetyViolations(FieldInfo field, Type declaringType, List<string> violations)
    {
        var fieldType = field.FieldType;

        // Check for System.Collections.Generic.Queue<T>
        if (IsNonThreadSafeQueue(fieldType))
        {
            violations.Add($"{declaringType.FullName}.{field.Name} uses non-thread-safe Queue<T>. Use ConcurrentQueue<T> instead for thread safety.");
        }

        // Check for other non-thread-safe collections that might be problematic
        if (IsNonThreadSafeCollection(fieldType))
        {
            violations.Add($"{declaringType.FullName}.{field.Name} uses potentially non-thread-safe collection {fieldType.Name}. Consider thread-safe alternatives from System.Collections.Concurrent.");
        }
    }

    private static void CheckPropertyForThreadSafetyViolations(PropertyInfo property, Type declaringType, List<string> violations)
    {
        var propertyType = property.PropertyType;

        // Check for System.Collections.Generic.Queue<T>
        if (IsNonThreadSafeQueue(propertyType))
        {
            violations.Add($"{declaringType.FullName}.{property.Name} property uses non-thread-safe Queue<T>. Use ConcurrentQueue<T> instead for thread safety.");
        }
    }

    private static bool IsNonThreadSafeQueue(Type type)
    {
        // Check if it's exactly System.Collections.Generic.Queue<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition().FullName == "System.Collections.Generic.Queue`1")
        {
            return true;
        }

        // Check if it's the non-generic Queue (less common but possible)
        if (type.FullName == "System.Collections.Queue")
        {
            return true;
        }

        return false;
    }

    private static bool IsNonThreadSafeCollection(Type type)
    {
        // List of potentially problematic non-thread-safe collection types
        var nonThreadSafeTypes = new[]
        {
            "System.Collections.Generic.Stack`1",
            "System.Collections.Stack",
            "System.Collections.ArrayList",
            "System.Collections.Hashtable"
        };

        if (type.IsGenericType)
        {
            var genericTypeName = type.GetGenericTypeDefinition().FullName;
            return nonThreadSafeTypes.Contains(genericTypeName);
        }

        return nonThreadSafeTypes.Contains(type.FullName);
    }
}
