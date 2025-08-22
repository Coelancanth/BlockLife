using FluentAssertions;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace BlockLife.Core.Tests.Infrastructure.Architecture;

/// <summary>
/// Architecture fitness tests for namespace conventions.
/// These tests ensure code follows namespace patterns required for:
/// - MediatR auto-discovery
/// - Clean Architecture boundaries  
/// - DI container resolution
/// 
/// Created: 2025-08-23 following PM_003 namespace resolution failure
/// These tests run WITHOUT needing DI container setup (fast feedback)
/// </summary>
public class NamespaceConventionTests
{
    private readonly ITestOutputHelper _output;
    private readonly Assembly _coreAssembly;

    public NamespaceConventionTests(ITestOutputHelper output)
    {
        _output = output;
        _coreAssembly = typeof(BlockLife.Core.GameStrapper).Assembly;
    }

    [Fact]
    public void All_Types_In_Core_Assembly_Should_Have_Correct_Root_Namespace()
    {
        // Arrange
        var allTypes = _coreAssembly.GetTypes()
            .Where(t => !t.IsNested) // Skip nested types
            .ToList();

        var incorrectTypes = new List<string>();

        // Act
        foreach (var type in allTypes)
        {
            var ns = type.Namespace ?? string.Empty;
            
            // All types in Core assembly MUST start with BlockLife.Core
            if (!ns.StartsWith("BlockLife.Core") && !string.IsNullOrEmpty(ns))
            {
                incorrectTypes.Add($"{type.Name} has namespace '{ns}'");
                _output.WriteLine($"VIOLATION: {type.Name} in {ns}");
            }
        }

        // Assert
        incorrectTypes.Should().BeEmpty(
            $"All types in Core assembly must have namespace starting with 'BlockLife.Core'. " +
            $"Found {incorrectTypes.Count} violations. This breaks MediatR assembly scanning! " +
            $"See PM_003 for details. Violations: {string.Join("; ", incorrectTypes.Take(5))}");
    }

    [Fact]
    public void Handler_Types_Should_Match_Feature_Folder_Structure()
    {
        // This ensures namespace matches folder structure for handlers
        
        var handlerTypes = _coreAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Handler") && 
                       t.GetInterfaces().Any(i => i.IsGenericType && 
                           (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                            i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))))
            .ToList();

        var misalignments = new List<string>();

        foreach (var handler in handlerTypes)
        {
            var ns = handler.Namespace ?? string.Empty;
            // Allow both new pattern (with Commands/Queries folders) and legacy pattern (direct in feature folder)
            var newPattern = @"^BlockLife\.Core\.Features\.\w+\.(?:Commands?|Queries|Notifications?)$";
            var legacyPattern = @"^BlockLife\.Core\.Features\.\w+\.(?:Placement|Drag\.\w+|Move\.\w+)$";
            
            if (!Regex.IsMatch(ns, newPattern) && !Regex.IsMatch(ns, legacyPattern))
            {
                misalignments.Add($"{handler.Name}: '{ns}' doesn't match pattern");
                _output.WriteLine($"Pattern mismatch: {handler.Name} in {ns}");
            }
        }

        misalignments.Should().BeEmpty(
            $"Handler namespaces should follow: BlockLife.Core.Features.[Feature].[Commands|Queries|Notifications]. " +
            $"Found {misalignments.Count} misalignments: {string.Join("; ", misalignments.Take(3))}");
    }

    [Fact]
    public void Command_And_Query_Types_Should_Be_In_Correct_Namespace()
    {
        // Commands should be in .Commands namespace, Queries in .Queries namespace
        
        var commandTypes = _coreAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Command") && !t.Name.EndsWith("CommandHandler"))
            .ToList();
            
        var queryTypes = _coreAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Query") && !t.Name.EndsWith("QueryHandler"))
            .ToList();

        var violations = new List<string>();

        foreach (var cmd in commandTypes)
        {
            // Allow Commands folder or legacy patterns (Placement, Drag, etc.)
            var isInCommandsFolder = cmd.Namespace?.EndsWith(".Commands") == true || 
                                    cmd.Namespace?.EndsWith(".Command") == true;
            var isLegacyPattern = cmd.Namespace?.Contains(".Placement") == true ||
                                 cmd.Namespace?.Contains(".Drag") == true;
            
            if (!isInCommandsFolder && !isLegacyPattern)
            {
                violations.Add($"Command '{cmd.Name}' in wrong namespace: {cmd.Namespace}");
            }
        }

        foreach (var query in queryTypes)
        {
            // Allow Queries folder or legacy patterns  
            var isInQueriesFolder = query.Namespace?.EndsWith(".Queries") == true || 
                                   query.Namespace?.EndsWith(".Query") == true;
            var isLegacyPattern = query.Namespace?.Contains(".Placement") == true ||
                                 query.Namespace?.Contains(".Drag") == true;
            
            if (!isInQueriesFolder && !isLegacyPattern)
            {
                violations.Add($"Query '{query.Name}' in wrong namespace: {query.Namespace}");
            }
        }

        violations.Should().BeEmpty(
            $"Commands must be in .Commands namespace, Queries in .Queries namespace. " +
            $"Violations: {string.Join("; ", violations)}");
    }

    [Fact]
    public void No_Types_Should_Use_Root_BlockLife_Namespace()
    {
        // Prevent accidental use of BlockLife instead of BlockLife.Core
        
        var rootNamespaceTypes = _coreAssembly.GetTypes()
            .Where(t => t.Namespace == "BlockLife" || 
                       (t.Namespace?.StartsWith("BlockLife.") == true && 
                        !t.Namespace.StartsWith("BlockLife.Core")))
            .Select(t => $"{t.Name} in {t.Namespace}")
            .ToList();

        rootNamespaceTypes.Should().BeEmpty(
            $"No types should use 'BlockLife' root namespace in Core assembly. " +
            $"All must use 'BlockLife.Core.*'. This caused TD_068! " +
            $"Found violations: {string.Join(", ", rootNamespaceTypes)}");
    }

    [Fact]
    public void Infrastructure_Types_Should_Be_In_Infrastructure_Namespace()
    {
        // Clean Architecture: Infrastructure types in Infrastructure namespace
        
        var infrastructureTypes = new[] 
        { 
            "Service", "Repository", "Manager", "Factory", "Provider", "Configuration"
        };

        var violations = new List<string>();

        foreach (var type in _coreAssembly.GetTypes())
        {
            var typeName = type.Name;
            var ns = type.Namespace ?? string.Empty;
            
            if (infrastructureTypes.Any(suffix => typeName.EndsWith(suffix)))
            {
                // Should be in Infrastructure namespace (with some exceptions)
                if (!ns.Contains("Infrastructure") && 
                    !ns.Contains("Services") && 
                    !ns.Contains("Application") &&
                    !ns.Contains("Presentation") &&
                    !typeName.Contains("Test"))
                {
                    violations.Add($"{typeName} should be in Infrastructure namespace, found in: {ns}");
                }
            }
        }

        // This is more of a guideline than strict rule
        _output.WriteLine($"Found {violations.Count} potential infrastructure namespace violations");
        violations.ForEach(v => _output.WriteLine($"  - {v}"));
    }

    [Theory]
    [InlineData("BlockLife.Features.Player.Commands", "BlockLife.Core.Features.Player.Commands")]
    [InlineData("BlockLife.Features.Player.Queries", "BlockLife.Core.Features.Player.Queries")]
    [InlineData("BlockLife.Application", "BlockLife.Core.Application")]
    [InlineData("BlockLife.Domain", "BlockLife.Core.Domain")]
    public void Common_Namespace_Mistakes_Should_Not_Exist(string wrongNamespace, string correctNamespace)
    {
        // Test for specific namespace mistakes that have occurred
        
        var typesWithWrongNamespace = _coreAssembly.GetTypes()
            .Where(t => t.Namespace == wrongNamespace)
            .Select(t => t.Name)
            .ToList();

        typesWithWrongNamespace.Should().BeEmpty(
            $"Found types in '{wrongNamespace}' but they should be in '{correctNamespace}'. " +
            $"Types: {string.Join(", ", typesWithWrongNamespace)}. " +
            $"This exact issue caused TD_068!");
    }

    [Fact]
    public void All_Domain_Types_Should_Be_In_Domain_Namespace()
    {
        // Clean Architecture: Domain types should be in Domain namespace
        
        var domainTypeNames = new[] 
        { 
            "Entity", "ValueObject", "Aggregate", "DomainEvent", "Specification"
        };

        var domainInterfaces = new[]
        {
            "IRepository", "ISpecification", "IAggregateRoot", "IEntity"
        };

        var violations = new List<string>();

        foreach (var type in _coreAssembly.GetTypes())
        {
            var ns = type.Namespace ?? string.Empty;
            
            // Check if it's likely a domain type
            bool isDomainType = domainTypeNames.Any(n => type.Name.Contains(n)) ||
                               domainInterfaces.Any(i => type.GetInterfaces().Any(impl => impl.Name == i));

            if (isDomainType && !ns.Contains("Domain") && !type.Name.Contains("Test"))
            {
                violations.Add($"{type.Name} appears to be domain type but is in: {ns}");
            }
        }

        // This is informational - log but don't fail
        if (violations.Any())
        {
            _output.WriteLine($"Potential domain namespace violations ({violations.Count}):");
            violations.Take(5).ToList().ForEach(v => _output.WriteLine($"  - {v}"));
        }
    }

    [Fact]
    public void Namespace_Depth_Should_Be_Reasonable()
    {
        // Prevent overly deep namespace hierarchies
        const int maxDepth = 7;
        
        var deepNamespaces = _coreAssembly.GetTypes()
            .Where(t => t.Namespace?.Split('.').Length > maxDepth)
            .Select(t => $"{t.Name}: {t.Namespace} (depth: {t.Namespace?.Split('.').Length})")
            .ToList();

        deepNamespaces.Should().BeEmpty(
            $"Namespaces shouldn't be deeper than {maxDepth} levels. " +
            $"Found: {string.Join("; ", deepNamespaces.Take(3))}");
    }
}