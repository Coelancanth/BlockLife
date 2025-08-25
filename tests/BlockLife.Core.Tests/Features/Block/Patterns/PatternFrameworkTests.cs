using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Patterns;
using BlockLife.Core.Features.Block.Patterns.Models;
using FluentAssertions;
using LanguageExt;
using static LanguageExt.Prelude;
using System;
using System.Linq;
using Xunit;

namespace BlockLife.Core.Tests.Features.Block.Patterns
{
    /// <summary>
    /// Tests for the Pattern Recognition Framework interfaces and core types.
    /// Following TDD approach - these tests drive Phase 1 implementation.
    /// Tests interface compilation, enum coverage, and immutability requirements.
    /// </summary>
    public class PatternFrameworkTests
    {
        #region Interface Compilation Tests

        [Fact]
        public void IPattern_Interface_CompilesToValidContract()
        {
            // Arrange & Act - Create a mock implementation to verify interface compilation
            var mockPattern = new MockPattern();

            // Assert - Interface methods are accessible and return expected types
            mockPattern.Type.Should().Be(PatternType.Match);
            mockPattern.Positions.Should().NotBeNull();
            mockPattern.Priority.Should().BeGreaterThan(0);
            mockPattern.PatternId.Should().NotBeNullOrEmpty();
            ((IPattern)mockPattern).Size.Should().Be(mockPattern.Positions.Count);

            // Assert - Methods can be called without compilation errors
            var outcome = mockPattern.CalculateOutcome();
            outcome.Should().NotBeNull();

            var conflicts = mockPattern.ConflictsWith(mockPattern);
            conflicts.Should().BeTrue(); // Same pattern should conflict with itself

            var description = mockPattern.GetDescription();
            description.Should().NotBeNullOrEmpty();
        }

        [Fact] 
        public void IPatternRecognizer_Interface_CompilesToValidContract()
        {
            // Arrange & Act - Verify interface compilation through reflection
            var interfaceType = typeof(IPatternRecognizer);
            
            // Assert - Interface has all required properties
            interfaceType.GetProperty(nameof(IPatternRecognizer.SupportedType)).Should().NotBeNull();
            interfaceType.GetProperty(nameof(IPatternRecognizer.IsEnabled)).Should().NotBeNull();
            interfaceType.GetProperty(nameof(IPatternRecognizer.RecognizerId)).Should().NotBeNull();
            
            // Assert - Interface has all required methods
            var recognizeMethod = interfaceType.GetMethod(nameof(IPatternRecognizer.Recognize));
            recognizeMethod.Should().NotBeNull();
            recognizeMethod!.ReturnType.Should().Be(typeof(Fin<Seq<IPattern>>));
            
            var canRecognizeMethod = interfaceType.GetMethod(nameof(IPatternRecognizer.CanRecognizeAt));
            canRecognizeMethod.Should().NotBeNull();
            canRecognizeMethod!.ReturnType.Should().Be(typeof(bool));
        }

        [Fact]
        public void IPatternResolver_Interface_CompilesToValidContract()
        {
            // Arrange & Act - Verify interface compilation through reflection
            var interfaceType = typeof(IPatternResolver);
            
            // Assert - Interface has all required properties
            interfaceType.GetProperty(nameof(IPatternResolver.ResolverId)).Should().NotBeNull();
            interfaceType.GetProperty(nameof(IPatternResolver.ResolutionStrategy)).Should().NotBeNull();
            
            // Assert - Interface has all required methods  
            var resolveMethod = interfaceType.GetMethod(nameof(IPatternResolver.Resolve));
            resolveMethod.Should().NotBeNull();
            resolveMethod!.ReturnType.Should().Be(typeof(Fin<Seq<IPattern>>));
        }

        [Fact]
        public void IPatternExecutor_Interface_CompilesToValidContract()
        {
            // Arrange & Act - Verify interface compilation through reflection
            var interfaceType = typeof(IPatternExecutor);
            
            // Assert - Interface has all required properties
            interfaceType.GetProperty(nameof(IPatternExecutor.SupportedType)).Should().NotBeNull();
            interfaceType.GetProperty(nameof(IPatternExecutor.ExecutorId)).Should().NotBeNull();
            interfaceType.GetProperty(nameof(IPatternExecutor.IsEnabled)).Should().NotBeNull();
            
            // Assert - Interface has all required methods
            var executeMethod = interfaceType.GetMethod(nameof(IPatternExecutor.Execute));
            executeMethod.Should().NotBeNull();
            executeMethod!.ReturnType.Name.Should().StartWith("Task"); // Task<Fin<ExecutionResult>>
        }

        #endregion

        #region PatternType Enum Coverage Tests

        [Fact]
        public void PatternType_AllValuesHaveValidPriorities()
        {
            // Arrange - Get all enum values
            var allPatternTypes = Enum.GetValues<PatternType>();

            // Act & Assert - Each enum value should have a valid priority
            foreach (var patternType in allPatternTypes)
            {
                var priority = patternType.GetPriority();
                priority.Should().BeGreaterThan(0, $"PatternType.{patternType} should have positive priority");
            }
        }

        [Fact]
        public void PatternType_AllValuesHaveDisplayNames()
        {
            // Arrange - Get all enum values
            var allPatternTypes = Enum.GetValues<PatternType>();

            // Act & Assert - Each enum value should have a display name
            foreach (var patternType in allPatternTypes)
            {
                var displayName = patternType.GetDisplayName();
                displayName.Should().NotBeNullOrWhiteSpace($"PatternType.{patternType} should have display name");
                displayName.Should().NotBe(patternType.ToString(), $"PatternType.{patternType} should have custom display name");
            }
        }

        [Fact]
        public void PatternType_PrioritiesAreDistinctAndOrdered()
        {
            // Arrange - Get all pattern types and their priorities
            var allPatternTypes = Enum.GetValues<PatternType>();
            var priorities = allPatternTypes.Select(pt => new { Type = pt, Priority = pt.GetPriority() }).ToList();

            // Act & Assert - Priorities should be distinct
            var distinctPriorities = priorities.Select(p => p.Priority).Distinct().ToList();
            distinctPriorities.Should().HaveCount(priorities.Count, "All pattern types should have unique priorities");

            // Assert - Priority order should match expected execution order (higher = executed first)
            PatternType.Transmute.GetPriority().Should().BeGreaterThan(PatternType.TierUp.GetPriority(), 
                "Transmute should have higher priority than TierUp");
            PatternType.TierUp.GetPriority().Should().BeGreaterThan(PatternType.Match.GetPriority(),
                "TierUp should have higher priority than Match");
        }

        [Fact]
        public void PatternType_EnabledStatesAreConsistent()
        {
            // Arrange & Act - Check enabled states
            var matchEnabled = PatternType.Match.IsEnabled();
            var tierUpEnabled = PatternType.TierUp.IsEnabled();
            var transmuteEnabled = PatternType.Transmute.IsEnabled();

            // Assert - Match and TierUp should be enabled (VS_003B-1), Transmute disabled for now
            matchEnabled.Should().BeTrue("Match patterns should be enabled in Phase 1");
            tierUpEnabled.Should().BeTrue("TierUp patterns should be enabled for VS_003B-1 implementation");
            transmuteEnabled.Should().BeFalse("Transmute patterns should be disabled until future phases");
        }

        #endregion

        #region PatternContext Immutability Tests

        [Fact]
        public void PatternContext_IsImmutable_RecordBehavior()
        {
            // Arrange - Create a pattern context
            var originalContext = PatternContext.CreateDefault(new Vector2Int(1, 2));

            // Act - Try to create modified versions using 'with' expressions
            var modifiedContext1 = originalContext with { MaxPatternsPerType = 20 };
            var modifiedContext2 = originalContext.WithMaxPatterns(15);

            // Assert - Original should be unchanged
            originalContext.MaxPatternsPerType.Should().Be(10, "Original context should be unchanged");
            originalContext.TriggerPosition.Should().Be(new Vector2Int(1, 2), "Original trigger position unchanged");

            // Assert - Modified versions should have changes
            modifiedContext1.MaxPatternsPerType.Should().Be(20, "Modified context should have new max patterns");
            modifiedContext1.TriggerPosition.Should().Be(new Vector2Int(1, 2), "Trigger position should remain same");

            modifiedContext2.MaxPatternsPerType.Should().Be(15, "Context created with factory should have correct value");
        }

        [Fact]
        public void PatternContext_TargetPatternTypes_AreImmutable()
        {
            // Arrange - Create context with pattern types
            var context = PatternContext.CreateForTypes(new Vector2Int(0, 0), PatternType.Match, PatternType.TierUp);
            var originalTypes = context.TargetPatternTypes;

            // Act - Try to modify the sequence (this should not affect the original)
            var newContext = context.WithAdditionalTypes(PatternType.Transmute);

            // Assert - Original sequence should be unchanged
            originalTypes.Should().HaveCount(2, "Original context should have 2 types");
            originalTypes.Should().Contain(PatternType.Match);
            originalTypes.Should().Contain(PatternType.TierUp);
            originalTypes.Should().NotContain(PatternType.Transmute);

            // Assert - New context should have additional type
            newContext.TargetPatternTypes.Should().HaveCount(3, "New context should have 3 types");
            newContext.TargetPatternTypes.Should().Contain(PatternType.Transmute);
        }

        [Fact]
        public void PatternContext_FactoryMethods_CreateCorrectInstances()
        {
            // Arrange & Act - Use all factory methods
            var defaultContext = PatternContext.CreateDefault(new Vector2Int(5, 5));
            var typedContext = PatternContext.CreateForTypes(new Vector2Int(3, 3), PatternType.Match);
            var actionContext = PatternContext.CreateWithAction(new Vector2Int(1, 1), "BlockPlaced");

            // Assert - Default context
            defaultContext.TriggerPosition.Should().Be(new Vector2Int(5, 5));
            defaultContext.TargetPatternTypes.Should().BeEmpty("Default should search all enabled types");
            defaultContext.MaxPatternsPerType.Should().Be(10, "Default max patterns");
            defaultContext.TriggerAction.IsNone.Should().BeTrue("Default should have no action");

            // Assert - Typed context  
            typedContext.TriggerPosition.Should().Be(new Vector2Int(3, 3));
            typedContext.TargetPatternTypes.Should().ContainSingle().Which.Should().Be(PatternType.Match);

            // Assert - Action context
            actionContext.TriggerPosition.Should().Be(new Vector2Int(1, 1));
            actionContext.TriggerAction.IsSome.Should().BeTrue("Should have trigger action");
            actionContext.TriggerAction.Match(Some: action => action.Should().Be("BlockPlaced"), None: () => { });
        }

        [Fact]
        public void PatternContext_ToString_ProvidesUsefulOutput()
        {
            // Arrange - Create contexts with different states
            var simpleContext = PatternContext.CreateDefault(new Vector2Int(2, 3));
            var complexContext = PatternContext.CreateWithAction(new Vector2Int(4, 5), "TestAction")
                .WithAdditionalTypes(PatternType.Match, PatternType.TierUp);

            // Act & Assert - ToString should provide meaningful information
            var simpleString = simpleContext.ToString();
            simpleString.Should().Contain("(2, 3)", "Should include trigger position");
            simpleString.Should().Contain("Action: None", "Should show no action");

            var complexString = complexContext.ToString();
            complexString.Should().Contain("(4, 5)", "Should include trigger position");
            complexString.Should().Contain("Action: TestAction", "Should show action");
            complexString.Should().Contain("Types: [2]", "Should show type count");
        }

        #endregion

        #region Helper Classes for Testing

        /// <summary>
        /// Mock implementation of IPattern for interface compilation testing.
        /// Provides minimal implementation to verify interface contract.
        /// </summary>
        private class MockPattern : IPattern
        {
            public PatternType Type => PatternType.Match;
            public Seq<Vector2Int> Positions => List(new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2)).ToSeq();
            public int Priority => 10;
            public string PatternId => "mock-pattern-001";

            public PatternOutcome CalculateOutcome() => PatternOutcome.CreateRemoval(
                Positions, 
                List(("TestAttribute", 10)).ToSeq(),
                1.0);

            public bool ConflictsWith(IPattern other) => 
                Positions.Intersect(other.Positions).Any();

            public bool IsValidFor(BlockLife.Core.Infrastructure.Services.IGridStateService gridService) => true;

            public string GetDescription() => $"Mock {Type} pattern with {Positions.Count} blocks";
        }

        #endregion
    }
}