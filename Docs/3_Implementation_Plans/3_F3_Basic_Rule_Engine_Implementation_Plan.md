# F3: Basic Rule Engine Implementation Plan

## Executive Summary

This document provides a comprehensive implementation plan for Slice F3: Basic Rule Engine, a critical infrastructure component that enables pattern matching and rule evaluation in the BlockLife game. The rule engine implements the **approved Anchor-Based Pattern Matching System** as defined in ADR-008, providing a 150x performance improvement over traditional grid-scanning approaches while maintaining Clean Architecture principles.

**Key Deliverables:**
- Core rule engine infrastructure with anchor-based evaluation
- Pattern definition system with declarative builder API
- Integration with existing CQRS command handlers
- Spatial indexing for O(1) position lookups
- Comprehensive test suite following TDD+VSA workflow

**Timeline:** 4 weeks (20 working days)
**Risk Level:** Medium - Critical infrastructure with performance requirements
**Dependencies:** F1 Block Placement (completed), F2 Block Movement (in progress)

## 1. Architectural Overview

### 1.1 System Context

The rule engine integrates with the existing Clean Architecture layers:

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│                  (Godot Views/Presenters)               │
└─────────────────────────────────────────────────────────┘
                              ▲
                              │ Notifications
                              ▼
┌─────────────────────────────────────────────────────────┐
│                    Application Layer                     │
│   ┌──────────────┐  ┌──────────────┐  ┌─────────────┐ │
│   │   Commands   │→ │   Handlers   │→ │ Rule Engine │ │
│   └──────────────┘  └──────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────┘
                              ▲
                              │ Uses
                              ▼
┌─────────────────────────────────────────────────────────┐
│                      Domain Layer                        │
│   ┌──────────────┐  ┌──────────────┐  ┌─────────────┐ │
│   │   Patterns   │  │   Matchers   │  │   Effects   │ │
│   └──────────────┘  └──────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### 1.2 Core Components

1. **IRuleEvaluationEngine**: Main evaluation coordinator
2. **IAnchorPattern**: Pattern definition interface
3. **IPatternResolver**: Conflict resolution system
4. **PatternBuilder**: Declarative pattern creation API
5. **IGridStateSnapshot**: Efficient grid state queries
6. **ISpatialIndex**: O(1) position lookups

### 1.3 Integration Points

- **Command Handlers**: Trigger evaluation after block operations
- **IGridStateService**: Provides current grid state
- **IMediator**: Publishes pattern match notifications
- **ISimulationManager**: Processes pattern effects

## 2. Architecture Fitness Tests (Write First)

Following TDD+VSA workflow, we define architecture constraints before implementation:

### 2.1 Pattern Architecture Tests

```csharp
// tests/BlockLife.Core.Tests/Architecture/RuleEngineArchitectureTests.cs

[Fact]
public void RuleEngine_Components_Should_Be_In_Core_Project()
{
    var ruleEngineTypes = Types.InAssembly(typeof(IRuleEvaluationEngine).Assembly)
        .That().ResideInNamespace("BlockLife.Core.Rules")
        .Or().ResideInNamespace("BlockLife.Core.Features.Rules");
        
    ruleEngineTypes.Should()
        .NotReference("Godot")
        .And.NotReference("GodotSharp");
}

[Fact]
public void Patterns_Should_Be_Immutable_Records()
{
    var patternTypes = Types.InAssembly(typeof(IAnchorPattern).Assembly)
        .That().ImplementInterface(typeof(IAnchorPattern));
        
    patternTypes.Should()
        .BeRecords()
        .Or().BeInterfaces()
        .And.NotHaveMutableFields();
}

[Fact]
public void Pattern_Evaluation_Should_Return_Fin()
{
    var patternTypes = Types.InAssembly(typeof(IAnchorPattern).Assembly)
        .That().ImplementInterface(typeof(IAnchorPattern));
        
    patternTypes.Should()
        .HaveMethod("EvaluateAt")
        .WithReturnType(typeof(Fin<PatternMatch>));
}

[Fact]
public void Rule_Engine_Should_Use_Async_Patterns()
{
    var engineType = typeof(IRuleEvaluationEngine);
    
    engineType.GetMethod("EvaluateAtAnchorAsync")
        .Should().NotBeNull()
        .And.Subject.ReturnType
        .Should().Be(typeof(Task<Fin<IReadOnlyList<PatternMatch>>>));
}

[Fact]
public void Pattern_Builders_Should_Be_Fluent()
{
    var builderTypes = Types.InAssembly(typeof(PatternBuilder).Assembly)
        .That().HaveNameEndingWith("Builder")
        .And().ResideInNamespace("BlockLife.Core.Rules");
        
    builderTypes.Should()
        .HaveMethodReturningItself()
        .And.HaveMethod("Build");
}
```

### 2.2 Dependency Architecture Tests

```csharp
[Fact]
public void RuleEngine_Should_Not_Depend_On_Concrete_Services()
{
    var engineImplementations = Types.InAssembly(typeof(RuleEvaluationEngine).Assembly)
        .That().ImplementInterface(typeof(IRuleEvaluationEngine));
        
    engineImplementations.Should()
        .OnlyDependOnInterfacesExcept(
            typeof(PatternMatch),
            typeof(Vector2Int),
            typeof(BlockType));
}

[Fact]
public void Patterns_Should_Not_Hold_State()
{
    var patternImplementations = Types.InAssembly(typeof(IAnchorPattern).Assembly)
        .That().ImplementInterface(typeof(IAnchorPattern));
        
    patternImplementations.Should()
        .NotHaveFieldsOfType<IGridStateService>()
        .And.NotHaveFieldsOfType<IMediator>();
}
```

## 3. TDD Red-Green-Refactor Implementation

### 3.1 Phase 1: Core Infrastructure (Days 1-5)

#### RED Phase - Write Failing Tests

```csharp
// tests/BlockLife.Core.Tests/Features/Rules/RuleEvaluationEngineTests.cs

public class RuleEvaluationEngineTests
{
    private readonly RuleEvaluationEngine _sut;
    private readonly Mock<IPatternResolver> _patternResolverMock;
    private readonly Mock<ILogger> _loggerMock;
    
    [Fact]
    public async Task EvaluateAtAnchor_WithNoPatterns_ReturnsEmptyList()
    {
        // Arrange
        var anchorPosition = new Vector2Int(5, 5);
        var gridState = CreateEmptyGridState();
        
        // Act
        var result = await _sut.EvaluateAtAnchorAsync(anchorPosition, gridState);
        
        // Assert
        result.IsSucc.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
    
    [Fact]
    public async Task EvaluateAtAnchor_WithMatchingPattern_ReturnsMatch()
    {
        // Arrange
        var pattern = CreateTestPattern("TEST_PATTERN", 100);
        _sut.RegisterPattern(pattern);
        
        var anchorPosition = new Vector2Int(5, 5);
        var gridState = CreateGridStateWithPattern(anchorPosition);
        
        // Act
        var result = await _sut.EvaluateAtAnchorAsync(anchorPosition, gridState);
        
        // Assert
        result.IsSucc.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].PatternId.Should().Be("TEST_PATTERN");
        result.Value[0].AnchorPosition.Should().Be(anchorPosition);
    }
    
    [Fact]
    public async Task EvaluateAtAnchor_WithConflictingPatterns_ResolvesConflicts()
    {
        // Arrange
        var pattern1 = CreateTestPattern("PATTERN_1", 100);
        var pattern2 = CreateTestPattern("PATTERN_2", 200); // Higher priority
        _sut.RegisterPattern(pattern1);
        _sut.RegisterPattern(pattern2);
        
        var anchorPosition = new Vector2Int(5, 5);
        var gridState = CreateGridStateWithBothPatterns(anchorPosition);
        
        _patternResolverMock
            .Setup(x => x.ResolveConflicts(It.IsAny<IReadOnlyList<PatternMatch>>()))
            .Returns((IReadOnlyList<PatternMatch> matches) => 
                Fin<IReadOnlyList<PatternMatch>>.Succ(
                    matches.OrderByDescending(m => m.Priority).Take(1).ToList()));
        
        // Act
        var result = await _sut.EvaluateAtAnchorAsync(anchorPosition, gridState);
        
        // Assert
        result.IsSucc.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].PatternId.Should().Be("PATTERN_2"); // Higher priority wins
    }
    
    [Fact]
    public async Task RegisterPattern_WithDuplicateId_ReplacesExisting()
    {
        // Arrange
        var pattern1 = CreateTestPattern("DUPLICATE", 100);
        var pattern2 = CreateTestPattern("DUPLICATE", 200);
        
        // Act
        _sut.RegisterPattern(pattern1);
        _sut.RegisterPattern(pattern2);
        
        // Assert
        _sut.GetRegisteredPatterns().Should().HaveCount(1);
        _sut.GetRegisteredPatterns().First().Priority.Should().Be(200);
    }
    
    [Fact]
    public async Task UnregisterPattern_RemovesPattern()
    {
        // Arrange
        var pattern = CreateTestPattern("TO_REMOVE", 100);
        _sut.RegisterPattern(pattern);
        
        // Act
        var removed = _sut.UnregisterPattern("TO_REMOVE");
        
        // Assert
        removed.Should().BeTrue();
        _sut.GetRegisteredPatterns().Should().BeEmpty();
    }
}
```

#### GREEN Phase - Implement Minimal Code

```csharp
// src/Core/Rules/IRuleEvaluationEngine.cs

namespace BlockLife.Core.Rules;

public interface IRuleEvaluationEngine
{
    Task<Fin<IReadOnlyList<PatternMatch>>> EvaluateAtAnchorAsync(
        Vector2Int anchorPosition,
        IGridStateSnapshot gridState,
        CancellationToken cancellationToken = default);
        
    void RegisterPattern(IAnchorPattern pattern);
    bool UnregisterPattern(string patternId);
    IReadOnlyList<IAnchorPattern> GetRegisteredPatterns();
}

// src/Core/Rules/RuleEvaluationEngine.cs

public sealed class RuleEvaluationEngine : IRuleEvaluationEngine
{
    private readonly IPatternResolver _patternResolver;
    private readonly ILogger _logger;
    private readonly Dictionary<string, IAnchorPattern> _patterns = new();
    
    public RuleEvaluationEngine(IPatternResolver patternResolver, ILogger logger)
    {
        _patternResolver = patternResolver;
        _logger = logger.ForContext<RuleEvaluationEngine>();
    }
    
    public async Task<Fin<IReadOnlyList<PatternMatch>>> EvaluateAtAnchorAsync(
        Vector2Int anchorPosition,
        IGridStateSnapshot gridState,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var candidateMatches = new List<PatternMatch>();
            
            foreach (var pattern in _patterns.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var evaluationResult = pattern.EvaluateAt(anchorPosition, gridState);
                evaluationResult.IfSucc(match => 
                {
                    candidateMatches.Add(match);
                    _logger.Debug("Pattern {PatternId} matched at {Position}", 
                        match.PatternId, anchorPosition);
                });
            }
            
            return await Task.FromResult(_patternResolver.ResolveConflicts(candidateMatches));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to evaluate rules at anchor {Position}", anchorPosition);
            return Error.New("RULE_EVALUATION_FAILED", ex.Message);
        }
    }
    
    public void RegisterPattern(IAnchorPattern pattern)
    {
        _patterns[pattern.PatternId] = pattern;
        _logger.Information("Registered pattern {PatternId} with priority {Priority}",
            pattern.PatternId, pattern.Priority);
    }
    
    public bool UnregisterPattern(string patternId)
    {
        var removed = _patterns.Remove(patternId);
        if (removed)
        {
            _logger.Information("Unregistered pattern {PatternId}", patternId);
        }
        return removed;
    }
    
    public IReadOnlyList<IAnchorPattern> GetRegisteredPatterns() => 
        _patterns.Values.ToList().AsReadOnly();
}
```

#### REFACTOR Phase - Optimize and Clean

After tests pass, refactor for performance and clarity while keeping tests green.

### 3.2 Phase 2: Pattern Definition System (Days 6-10)

#### Property Tests for Pattern Invariants

```csharp
// tests/BlockLife.Core.Tests/Properties/PatternPropertyTests.cs

[Property]
public Property Pattern_RelativePositions_Should_Be_Unique()
{
    return Prop.ForAll(
        Gen.ListOf(Gen.Choose(-10, 10).Two().Select(t => new Vector2Int(t.Item1, t.Item2))),
        positions =>
        {
            var pattern = new TestPattern("TEST", 100, positions);
            var uniquePositions = pattern.RequiredPositions.Distinct().Count();
            return uniquePositions == pattern.RequiredPositions.Count;
        }
    );
}

[Property]
public Property Pattern_Evaluation_Should_Be_Deterministic()
{
    return Prop.ForAll(
        BlockLifeGenerators.ValidGridPosition(),
        BlockLifeGenerators.GridStateWithBlocks(),
        (anchorPos, gridState) =>
        {
            var pattern = CreateDeterministicPattern();
            var result1 = pattern.EvaluateAt(anchorPos, gridState);
            var result2 = pattern.EvaluateAt(anchorPos, gridState);
            
            return result1.IsSucc == result2.IsSucc &&
                   result1.Match(
                       Succ: m1 => result2.Match(
                           Succ: m2 => m1.Equals(m2),
                           Fail: _ => false),
                       Fail: e1 => result2.Match(
                           Succ: _ => false,
                           Fail: e2 => e1.Code == e2.Code));
        }
    );
}

[Property]
public Property Pattern_Priority_Should_Determine_Resolution_Order()
{
    return Prop.ForAll(
        Gen.Choose(1, 1000),
        Gen.Choose(1, 1000),
        (priority1, priority2) =>
        {
            var pattern1 = CreatePatternWithPriority(priority1);
            var pattern2 = CreatePatternWithPriority(priority2);
            
            var resolver = new PatternResolver();
            var matches = new[] 
            { 
                CreateMatch(pattern1), 
                CreateMatch(pattern2) 
            };
            
            var resolved = resolver.ResolveConflicts(matches);
            
            return resolved.IsSucc && 
                   resolved.Value.First().Priority == Math.Max(priority1, priority2);
        }
    );
}
```

#### Pattern Implementation Tests

```csharp
// tests/BlockLife.Core.Tests/Features/Rules/Patterns/StudyChainPatternTests.cs

public class StudyChainPatternTests
{
    [Fact]
    public void StudyChainPattern_WithThreeStudyBlocks_Matches()
    {
        // Arrange
        var pattern = new StudyChainPattern();
        var anchorPosition = new Vector2Int(5, 5);
        var gridState = new GridStateBuilder()
            .WithBlock(new Vector2Int(4, 5), BlockType.Study)
            .WithBlock(new Vector2Int(5, 5), BlockType.Study)
            .WithBlock(new Vector2Int(6, 5), BlockType.Study)
            .Build();
        
        // Act
        var result = pattern.EvaluateAt(anchorPosition, gridState);
        
        // Assert
        result.IsSucc.Should().BeTrue();
        result.Value.PatternId.Should().Be("STUDY_CHAIN_3");
        result.Value.AffectedPositions.Should().HaveCount(3);
    }
    
    [Fact]
    public void StudyChainPattern_WithMissingBlock_DoesNotMatch()
    {
        // Arrange
        var pattern = new StudyChainPattern();
        var anchorPosition = new Vector2Int(5, 5);
        var gridState = new GridStateBuilder()
            .WithBlock(new Vector2Int(4, 5), BlockType.Work) // Wrong type
            .WithBlock(new Vector2Int(5, 5), BlockType.Study)
            .WithBlock(new Vector2Int(6, 5), BlockType.Study)
            .Build();
        
        // Act
        var result = pattern.EvaluateAt(anchorPosition, gridState);
        
        // Assert
        result.IsFail.Should().BeTrue();
        result.Match(
            Succ: _ => false,
            Fail: error => error.Message.Should().Contain("requires Study"));
    }
}
```

### 3.3 Phase 3: CQRS Integration (Days 11-15)

#### Integration with Command Handlers

```csharp
// tests/BlockLife.Core.Tests/Features/Rules/Integration/RuleEngineIntegrationTests.cs

public class RuleEngineIntegrationTests
{
    [Fact]
    public async Task PlaceBlockCommand_Should_Trigger_Rule_Evaluation()
    {
        // Arrange
        var services = new ServiceCollection();
        ConfigureServices(services);
        var provider = services.BuildServiceProvider();
        
        var mediator = provider.GetRequiredService<IMediator>();
        var ruleEngine = provider.GetRequiredService<IRuleEvaluationEngine>();
        var gridStateService = provider.GetRequiredService<IGridStateService>();
        
        // Register a test pattern
        ruleEngine.RegisterPattern(new StudyChainPattern());
        
        // Setup grid with partial pattern
        await mediator.Send(new PlaceBlockCommand 
        { 
            Position = new Vector2Int(4, 5), 
            BlockType = BlockType.Study 
        });
        await mediator.Send(new PlaceBlockCommand 
        { 
            Position = new Vector2Int(6, 5), 
            BlockType = BlockType.Study 
        });
        
        // Act - Place block that completes pattern
        var result = await mediator.Send(new PlaceBlockCommand 
        { 
            Position = new Vector2Int(5, 5), 
            BlockType = BlockType.Study 
        });
        
        // Assert
        result.IsSucc.Should().BeTrue();
        
        // Verify pattern was detected (through notification)
        var notifications = provider.GetRequiredService<INotificationCollector>();
        notifications.Should().ContainSingle(n => 
            n is PatternMatchedNotification pmn && 
            pmn.PatternId == "STUDY_CHAIN_3");
    }
    
    [Fact]
    public async Task MoveBlockCommand_Should_Trigger_Rule_Evaluation_At_Both_Positions()
    {
        // Arrange
        var handler = new MoveBlockCommandHandlerWithRules(
            _gridStateService,
            _ruleEngine,
            _mediator,
            _logger);
            
        var blockId = Guid.NewGuid();
        var fromPosition = new Vector2Int(5, 5);
        var toPosition = new Vector2Int(7, 7);
        
        _gridStateService.AddBlock(blockId, fromPosition, BlockType.Study);
        
        // Act
        var result = await handler.Handle(new MoveBlockCommand
        {
            BlockId = blockId,
            ToPosition = toPosition
        }, CancellationToken.None);
        
        // Assert
        result.IsSucc.Should().BeTrue();
        
        // Verify rule evaluation was triggered at both positions
        _ruleEngine.Received(1).EvaluateAtAnchorAsync(
            fromPosition, 
            Arg.Any<IGridStateSnapshot>(), 
            Arg.Any<CancellationToken>());
        _ruleEngine.Received(1).EvaluateAtAnchorAsync(
            toPosition, 
            Arg.Any<IGridStateSnapshot>(), 
            Arg.Any<CancellationToken>());
    }
}
```

#### Enhanced Command Handler Implementation

```csharp
// src/Features/Block/Commands/PlaceBlockCommandHandler.cs (Enhanced)

public class PlaceBlockCommandHandler : IRequestHandler<PlaceBlockCommand, Fin<Unit>>
{
    private readonly IGridStateService _gridStateService;
    private readonly IRuleEvaluationEngine _ruleEngine;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    
    public async Task<Fin<Unit>> Handle(PlaceBlockCommand request, CancellationToken ct)
    {
        return await (
            from validation in ValidatePlacement(request.Position)
            from block in PlaceBlock(request.Position, request.BlockType)
            from ruleMatches in EvaluateRulesAtAnchor(request.Position, ct)
            from _ in PublishNotifications(block, ruleMatches, ct)
            select Unit.Default
        );
    }
    
    private async Task<Fin<IReadOnlyList<PatternMatch>>> EvaluateRulesAtAnchor(
        Vector2Int anchorPosition, 
        CancellationToken ct)
    {
        var gridSnapshot = _gridStateService.CreateSnapshot();
        var result = await _ruleEngine.EvaluateAtAnchorAsync(anchorPosition, gridSnapshot, ct);
        
        result.IfSucc(matches =>
        {
            if (matches.Any())
            {
                _logger.Information("Found {Count} pattern matches at {Position}: {Patterns}",
                    matches.Count, anchorPosition, 
                    string.Join(", ", matches.Select(m => m.PatternId)));
            }
        });
        
        return result;
    }
    
    private async Task<Fin<Unit>> PublishNotifications(
        Block block,
        IReadOnlyList<PatternMatch> ruleMatches,
        CancellationToken ct)
    {
        // Publish block placed notification
        await _mediator.Publish(new BlockPlacedNotification(block.Id, block.Position), ct);
        
        // Publish pattern match notifications
        foreach (var match in ruleMatches)
        {
            await _mediator.Publish(new PatternMatchedNotification(
                match.PatternId,
                match.AnchorPosition,
                match.AffectedPositions,
                match.Priority,
                DateTime.UtcNow), ct);
        }
        
        return Unit.Default;
    }
}
```

### 3.4 Phase 4: Performance Optimization (Days 16-20)

#### Performance Benchmarks

```csharp
// tests/BlockLife.Core.Tests/Performance/RuleEngineBenchmarks.cs

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class RuleEngineBenchmarks
{
    private IRuleEvaluationEngine _ruleEngine;
    private IGridStateSnapshot _smallGrid; // 20x20 with 100 blocks
    private IGridStateSnapshot _mediumGrid; // 50x50 with 500 blocks
    private IGridStateSnapshot _largeGrid; // 100x100 with 2000 blocks
    
    [GlobalSetup]
    public void Setup()
    {
        _ruleEngine = CreateRuleEngineWith20Patterns();
        _smallGrid = CreateGridState(20, 100);
        _mediumGrid = CreateGridState(50, 500);
        _largeGrid = CreateGridState(100, 2000);
    }
    
    [Benchmark]
    public async Task EvaluateSmallGrid()
    {
        var result = await _ruleEngine.EvaluateAtAnchorAsync(
            new Vector2Int(10, 10), _smallGrid);
    }
    
    [Benchmark]
    public async Task EvaluateMediumGrid()
    {
        var result = await _ruleEngine.EvaluateAtAnchorAsync(
            new Vector2Int(25, 25), _mediumGrid);
    }
    
    [Benchmark]
    public async Task EvaluateLargeGrid()
    {
        var result = await _ruleEngine.EvaluateAtAnchorAsync(
            new Vector2Int(50, 50), _largeGrid);
    }
}

// Expected Results:
// | Method             | Mean     | Error   | StdDev  | Gen0   | Allocated |
// |------------------- |---------:|--------:|--------:|-------:|----------:|
// | EvaluateSmallGrid  | 0.1 ms   | 0.01 ms | 0.01 ms | 0.5    | 2 KB      |
// | EvaluateMediumGrid | 0.3 ms   | 0.02 ms | 0.02 ms | 1.0    | 4 KB      |
// | EvaluateLargeGrid  | 0.5 ms   | 0.03 ms | 0.03 ms | 2.0    | 8 KB      |
```

#### Spatial Index Implementation

```csharp
// src/Core/Infrastructure/SpatialIndex.cs

public interface ISpatialIndex<T>
{
    void Add(Vector2Int position, T item);
    void Remove(Vector2Int position);
    Option<T> GetAt(Vector2Int position);
    IEnumerable<T> GetInRadius(Vector2Int center, int radius);
    void Clear();
}

public sealed class SpatialIndex<T> : ISpatialIndex<T>
{
    private readonly Dictionary<Vector2Int, T> _index = new();
    
    public void Add(Vector2Int position, T item)
    {
        _index[position] = item;
    }
    
    public void Remove(Vector2Int position)
    {
        _index.Remove(position);
    }
    
    public Option<T> GetAt(Vector2Int position)
    {
        return _index.TryGetValue(position, out var item) 
            ? Some(item) 
            : None;
    }
    
    public IEnumerable<T> GetInRadius(Vector2Int center, int radius)
    {
        var radiusSquared = radius * radius;
        
        return _index
            .Where(kvp => 
            {
                var dx = kvp.Key.X - center.X;
                var dy = kvp.Key.Y - center.Y;
                return (dx * dx + dy * dy) <= radiusSquared;
            })
            .Select(kvp => kvp.Value);
    }
    
    public void Clear()
    {
        _index.Clear();
    }
}
```

## 4. Pattern Library Implementation

### 4.1 Basic Pattern Set

```csharp
// src/Features/Rules/Patterns/BasicPatterns.cs

public sealed record StudyChainPattern : IAnchorPattern
{
    public string PatternId => "STUDY_CHAIN_3";
    public int Priority => 100;
    
    public IReadOnlyList<Vector2Int> RequiredPositions { get; } = new[]
    {
        new Vector2Int(-1, 0), // Left
        new Vector2Int(0, 0),  // Anchor
        new Vector2Int(1, 0)   // Right
    };
    
    public IReadOnlyDictionary<Vector2Int, BlockType> RequiredBlockTypes { get; } = 
        new Dictionary<Vector2Int, BlockType>
        {
            [new Vector2Int(-1, 0)] = BlockType.Study,
            [new Vector2Int(0, 0)] = BlockType.Study,
            [new Vector2Int(1, 0)] = BlockType.Study
        };
    
    public Fin<PatternMatch> EvaluateAt(Vector2Int anchorPosition, IGridStateSnapshot gridState)
    {
        foreach (var (relativePos, requiredType) in RequiredBlockTypes)
        {
            var absolutePos = anchorPosition + relativePos;
            var block = gridState.GetBlockAt(absolutePos);
            
            if (block.IsNone || block.Map(b => b.Type) != Some(requiredType))
            {
                return Error.New("PATTERN_MISMATCH", 
                    $"Study chain requires {requiredType} at {absolutePos}");
            }
        }
        
        var affectedPositions = RequiredPositions
            .Select(p => anchorPosition + p)
            .ToList();
            
        return new PatternMatch(PatternId, anchorPosition, affectedPositions, Priority);
    }
}

public sealed record WorkStudyAdjacencyPattern : IAnchorPattern
{
    public string PatternId => "WORK_STUDY_ADJACENCY";
    public int Priority => 80;
    
    public IReadOnlyList<Vector2Int> RequiredPositions { get; } = new[]
    {
        new Vector2Int(0, 0),  // Anchor (Work)
        new Vector2Int(1, 0)   // Adjacent (Study)
    };
    
    public IReadOnlyDictionary<Vector2Int, BlockType> RequiredBlockTypes { get; } = 
        new Dictionary<Vector2Int, BlockType>
        {
            [new Vector2Int(0, 0)] = BlockType.Work,
            [new Vector2Int(1, 0)] = BlockType.Study
        };
    
    // Implementation similar to above
}
```

### 4.2 Pattern Builder API

```csharp
// src/Core/Rules/PatternBuilder.cs

public sealed class PatternBuilder
{
    private string _patternId = "";
    private int _priority = 100;
    private readonly List<(Vector2Int position, BlockType type)> _requirements = new();
    
    public static PatternBuilder Create(string patternId)
    {
        return new PatternBuilder { _patternId = patternId };
    }
    
    public PatternBuilder WithPriority(int priority)
    {
        _priority = priority;
        return this;
    }
    
    public PatternBuilder RequireBlock(int x, int y, BlockType type)
    {
        _requirements.Add((new Vector2Int(x, y), type));
        return this;
    }
    
    public PatternBuilder RequireHorizontalLine(BlockType type, int length, int startX = 0)
    {
        for (int i = 0; i < length; i++)
        {
            RequireBlock(startX + i, 0, type);
        }
        return this;
    }
    
    public PatternBuilder RequireVerticalLine(BlockType type, int length, int startY = 0)
    {
        for (int i = 0; i < length; i++)
        {
            RequireBlock(0, startY + i, type);
        }
        return this;
    }
    
    public PatternBuilder RequireTShape(BlockType center, BlockType arms)
    {
        RequireBlock(0, 0, center);    // Center
        RequireBlock(-1, 0, arms);     // Left
        RequireBlock(1, 0, arms);      // Right
        RequireBlock(0, -1, arms);     // Top
        RequireBlock(0, 1, arms);      // Bottom
        return this;
    }
    
    public IAnchorPattern Build()
    {
        if (string.IsNullOrEmpty(_patternId))
            throw new InvalidOperationException("Pattern ID is required");
            
        if (!_requirements.Any())
            throw new InvalidOperationException("Pattern must have at least one requirement");
            
        return new DeclarativePattern(_patternId, _priority, _requirements);
    }
}
```

## 5. Integration Tests

### 5.1 End-to-End Pattern Detection

```csharp
// tests/BlockLife.Core.Tests/Features/Rules/Integration/PatternDetectionE2ETests.cs

public class PatternDetectionE2ETests : IClassFixture<GameTestFixture>
{
    [Fact]
    public async Task CompleteGameplayScenario_DetectsMultiplePatterns()
    {
        // Arrange
        var game = _fixture.CreateGame();
        await game.InitializeAsync();
        
        // Register patterns
        game.RuleEngine.RegisterPattern(new StudyChainPattern());
        game.RuleEngine.RegisterPattern(new WorkStudyAdjacencyPattern());
        
        // Act - Build a complex grid configuration
        await game.PlaceBlock(new Vector2Int(5, 5), BlockType.Study);
        await game.PlaceBlock(new Vector2Int(6, 5), BlockType.Study);
        await game.PlaceBlock(new Vector2Int(7, 5), BlockType.Study); // Completes chain
        
        await game.PlaceBlock(new Vector2Int(5, 6), BlockType.Work);
        await game.PlaceBlock(new Vector2Int(6, 6), BlockType.Study); // Creates adjacency
        
        // Assert
        var patterns = game.GetDetectedPatterns();
        patterns.Should().Contain(p => p.PatternId == "STUDY_CHAIN_3");
        patterns.Should().Contain(p => p.PatternId == "WORK_STUDY_ADJACENCY");
    }
    
    [Fact]
    public async Task MoveOperation_UpdatesPatternsCorrectly()
    {
        // Arrange
        var game = _fixture.CreateGame();
        await game.InitializeAsync();
        
        game.RuleEngine.RegisterPattern(new StudyChainPattern());
        
        // Setup initial pattern
        var block1 = await game.PlaceBlock(new Vector2Int(5, 5), BlockType.Study);
        var block2 = await game.PlaceBlock(new Vector2Int(6, 5), BlockType.Study);
        var block3 = await game.PlaceBlock(new Vector2Int(7, 5), BlockType.Study);
        
        // Act - Move middle block, breaking the pattern
        await game.MoveBlock(block2.Id, new Vector2Int(6, 6));
        
        // Assert
        var patterns = game.GetDetectedPatterns();
        patterns.Should().NotContain(p => p.PatternId == "STUDY_CHAIN_3");
    }
}
```

### 5.2 Performance Integration Tests

```csharp
[Fact]
public async Task RuleEngine_WithComplexGrid_MeetsPerformanceTargets()
{
    // Arrange
    var game = _fixture.CreateGame();
    await game.InitializeAsync();
    
    // Register 20 different patterns
    for (int i = 0; i < 20; i++)
    {
        game.RuleEngine.RegisterPattern(CreateTestPattern($"PATTERN_{i}", i * 10));
    }
    
    // Create a 50x50 grid with 500 blocks
    await CreateComplexGrid(game, 50, 500);
    
    // Act & Assert
    var stopwatch = Stopwatch.StartNew();
    
    for (int i = 0; i < 100; i++)
    {
        var position = new Vector2Int(Random.Next(50), Random.Next(50));
        await game.EvaluateRulesAt(position);
    }
    
    stopwatch.Stop();
    
    // Should complete 100 evaluations in under 50ms total
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(50);
    
    // Average should be under 0.5ms per evaluation
    var averageMs = stopwatch.ElapsedMilliseconds / 100.0;
    averageMs.Should().BeLessThan(0.5);
}
```

## 6. Quality Gates and Acceptance Criteria

### 6.1 Technical Quality Gates

- [ ] **Architecture Tests**: All 16 architecture fitness tests pass
- [ ] **Unit Test Coverage**: >95% coverage for rule engine components
- [ ] **Property Tests**: All 9 property tests pass with 100 random cases each
- [ ] **Integration Tests**: All CQRS integration scenarios pass
- [ ] **Performance Tests**: Meet or exceed performance targets
  - [ ] Small grid (20x20): <0.1ms evaluation time
  - [ ] Medium grid (50x50): <0.3ms evaluation time
  - [ ] Large grid (100x100): <0.5ms evaluation time
- [ ] **Memory Usage**: <10KB allocation per evaluation
- [ ] **No Godot Dependencies**: Core rule engine has zero Godot references

### 6.2 Functional Acceptance Criteria

- [ ] **Pattern Detection**: Simple 2-in-a-row patterns detected correctly
- [ ] **Conflict Resolution**: Overlapping patterns resolved by priority
- [ ] **CQRS Integration**: Rules triggered after block placement/movement
- [ ] **Performance**: Evaluation completes within 1ms for typical scenarios
- [ ] **Extensibility**: New patterns can be added without modifying engine
- [ ] **Debugging**: Comprehensive logging for pattern evaluation
- [ ] **Error Handling**: All operations return Fin<T> with meaningful errors

### 6.3 Documentation Requirements

- [ ] **API Documentation**: XML comments on all public interfaces
- [ ] **Pattern Catalog**: Documentation of all implemented patterns
- [ ] **Integration Guide**: How to integrate with command handlers
- [ ] **Performance Guide**: Optimization strategies and benchmarks
- [ ] **Testing Guide**: How to test custom patterns

## 7. Risk Analysis and Mitigation

### 7.1 Technical Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Performance degradation with many patterns | Medium | High | Implement pattern indexing and early termination |
| Complex pattern definitions | Low | Medium | Provide declarative builder API and visual tools |
| Integration conflicts with existing handlers | Low | Low | Use composition pattern, minimal changes to handlers |
| Memory leaks in pattern evaluation | Low | High | Use immutable patterns, proper disposal |
| Circular dependencies in patterns | Medium | Medium | Implement cycle detection in resolver |

### 7.2 Mitigation Strategies

1. **Performance Monitoring**: Add telemetry to track evaluation times
2. **Pattern Validation**: Compile-time and runtime validation of patterns
3. **Incremental Rollout**: Start with basic patterns, add complexity gradually
4. **Feature Flags**: Allow disabling rule engine if issues arise
5. **Comprehensive Testing**: Property-based tests for edge cases

## 8. Development Timeline

### Week 1 (Days 1-5): Core Infrastructure
- Day 1: Architecture tests and interfaces
- Day 2: RuleEvaluationEngine implementation
- Day 3: PatternResolver and conflict resolution
- Day 4: Unit tests and property tests
- Day 5: Refactoring and optimization

### Week 2 (Days 6-10): Pattern System
- Day 6: IAnchorPattern and base patterns
- Day 7: PatternBuilder API
- Day 8: StudyChainPattern implementation
- Day 9: Pattern tests and validation
- Day 10: Documentation and examples

### Week 3 (Days 11-15): Integration
- Day 11: PlaceBlockCommand integration
- Day 12: MoveBlockCommand integration
- Day 13: Notification pipeline
- Day 14: Integration tests
- Day 15: End-to-end testing

### Week 4 (Days 16-20): Optimization
- Day 16: Performance benchmarks
- Day 17: Spatial indexing
- Day 18: Memory optimization
- Day 19: Final testing and validation
- Day 20: Documentation and handoff

## 9. Dependencies and Prerequisites

### 9.1 Required Components (Must be complete)
- IGridStateService with snapshot capability
- Block placement infrastructure (F1)
- Basic notification pipeline

### 9.2 Nice-to-Have Components
- Block movement system (F2) - for move-triggered patterns
- Animation system - for pattern visualization

### 9.3 External Dependencies
- LanguageExt.Core (already in project)
- MediatR (already in project)
- Serilog (already in project)

## 10. Success Metrics

### 10.1 Performance Metrics
- Evaluation time: <1ms for 95th percentile
- Memory allocation: <10KB per evaluation
- Pattern registration: <1ms
- Conflict resolution: <0.1ms

### 10.2 Quality Metrics
- Test coverage: >95%
- Cyclomatic complexity: <10 for all methods
- Code duplication: <5%
- Documentation coverage: 100% for public APIs

### 10.3 Business Metrics
- Pattern variety: Support for 10+ different pattern types
- Extensibility: New pattern added in <1 hour
- Reliability: <0.1% failure rate in production
- Developer satisfaction: Intuitive API with clear examples

## Appendix A: File Structure

```
src/
├── Core/
│   ├── Rules/
│   │   ├── IRuleEvaluationEngine.cs
│   │   ├── RuleEvaluationEngine.cs
│   │   ├── IAnchorPattern.cs
│   │   ├── IPatternResolver.cs
│   │   ├── PatternResolver.cs
│   │   ├── PatternBuilder.cs
│   │   ├── PatternMatch.cs
│   │   └── DeclarativePattern.cs
│   └── Infrastructure/
│       ├── ISpatialIndex.cs
│       └── SpatialIndex.cs
├── Features/
│   ├── Rules/
│   │   ├── Patterns/
│   │   │   ├── StudyChainPattern.cs
│   │   │   ├── WorkStudyAdjacencyPattern.cs
│   │   │   └── BasicPatternSet.cs
│   │   ├── Effects/
│   │   │   ├── PatternMatchedEffect.cs
│   │   │   └── PatternMatchedNotification.cs
│   │   └── Handlers/
│   │       └── PatternMatchedNotificationHandler.cs
│   └── Block/
│       └── Commands/ (Enhanced)
│           ├── PlaceBlockCommandHandler.cs
│           └── MoveBlockCommandHandler.cs

tests/BlockLife.Core.Tests/
├── Architecture/
│   └── RuleEngineArchitectureTests.cs
├── Features/
│   └── Rules/
│       ├── RuleEvaluationEngineTests.cs
│       ├── PatternResolverTests.cs
│       ├── PatternBuilderTests.cs
│       ├── Patterns/
│       │   ├── StudyChainPatternTests.cs
│       │   └── WorkStudyAdjacencyPatternTests.cs
│       └── Integration/
│           ├── RuleEngineIntegrationTests.cs
│           └── PatternDetectionE2ETests.cs
├── Properties/
│   └── PatternPropertyTests.cs
└── Performance/
    └── RuleEngineBenchmarks.cs
```

## Appendix B: Pattern Catalog

### Basic Patterns (Priority 50-100)
1. **STUDY_CHAIN_3**: Three Study blocks in a row
2. **WORK_STUDY_ADJACENCY**: Work block adjacent to Study block
3. **BLOCK_PAIR**: Any two identical blocks adjacent

### Advanced Patterns (Priority 100-200)
1. **T_SHAPE_INNOVATION**: T-shaped pattern with Study center
2. **L_SHAPE_CAREER**: L-shaped pattern for career progression
3. **CROSS_PATTERN**: Plus sign pattern

### Complex Patterns (Priority 200+)
1. **ENTREPRENEURSHIP_COMBO**: Complex multi-block combination
2. **WEALTH_ACCUMULATION**: Resource generation pattern
3. **LIFE_ACHIEVEMENT**: End-game pattern

## Conclusion

This implementation plan provides a comprehensive roadmap for implementing the F3: Basic Rule Engine slice. By following the TDD+VSA workflow and adhering to Clean Architecture principles, we ensure a robust, performant, and maintainable rule engine that will serve as the foundation for complex gameplay mechanics in BlockLife.

The anchor-based approach provides the required performance characteristics while maintaining simplicity and extensibility. The declarative pattern builder API ensures that game designers can easily create and test new patterns without deep technical knowledge.

With proper execution of this plan, the rule engine will be delivered on time, within performance targets, and with comprehensive test coverage, setting the stage for the more complex gameplay features in future slices.