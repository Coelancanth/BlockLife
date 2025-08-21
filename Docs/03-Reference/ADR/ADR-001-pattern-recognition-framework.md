# ADR-001: Pattern Recognition Framework for Game Mechanics

## Status
Accepted (2025-08-19)

## Context
BlockLife requires multiple pattern-based game mechanics:
- **VS_003A**: Match-3 mechanics for clearing blocks and earning resources
- **VS_003B**: Tier system with visual indicators and bonuses
- **VS_003C**: Tier-up mechanics for combining blocks into higher tiers
- **VS_003D**: Cross-type transmutation for creating special blocks
- **VS_005**: Chain reactions with cascading matches

Initially, we considered implementing simple flood-fill match detection. However, analyzing the roadmap revealed that we need to recognize various board patterns with different rules, priorities, and outcomes. A simple match detector would require significant refactoring for each new mechanic.

### Forces at Play
- Need to support multiple pattern types without refactoring
- Patterns may conflict (same blocks, different interpretations)
- Player unlocks determine which patterns are active
- Performance matters (pattern detection runs after every action)
- Testing complexity increases with each new pattern type
- Future patterns are unknown but likely

## Decision
We will implement an **extensible Pattern Recognition Framework** that separates pattern detection from pattern execution.

### Core Architecture

```csharp
// 1. PATTERN RECOGNITION LAYER - Pure, Immutable, Testable
namespace BlockLife.Core.Features.Block.Patterns.Core
{
    // Patterns are first-class domain concepts
    public interface IPattern
    {
        PatternType Type { get; }
        Seq<Vector2Int> Positions { get; }
        int Priority { get; }  // Higher priority patterns override lower
        IPatternOutcome CalculateOutcome();
    }
    
    // Recognizers are pure functions: Grid State → Patterns
    public interface IPatternRecognizer
    {
        PatternType SupportedType { get; }
        bool IsEnabled { get; }  // Can be toggled by unlocks
        Seq<IPattern> Recognize(IGridStateService grid, Vector2Int trigger, PatternContext ctx);
    }
}

// 2. PATTERN RESOLUTION LAYER - Decides which patterns to execute
namespace BlockLife.Core.Features.Block.Patterns.Resolution
{
    public interface IPatternResolver
    {
        Seq<IPattern> Resolve(Seq<IPattern> candidates);
    }
}

// 3. PATTERN EXECUTION LAYER - Transforms patterns into game state changes
namespace BlockLife.Core.Features.Block.Patterns.Execution
{
    public interface IPatternExecutor
    {
        Task<Fin<ExecutionResult>> Execute(IPattern pattern, ExecutionContext context);
    }
}
```

### Key Design Principles
1. **Separation of Concerns**: Recognition, resolution, and execution are independent
2. **Open/Closed Principle**: Add new patterns without modifying existing code
3. **Pure Functions**: Recognizers have no side effects, making them testable
4. **Immutable Patterns**: Thread-safe and cacheable
5. **Priority System**: Handles conflicts between overlapping patterns

## Consequences

### Positive
- **Extensible**: New pattern types are isolated additions (new recognizer + executor)
- **Testable**: Each recognizer can be tested in complete isolation
- **Maintainable**: Changes to one pattern don't affect others
- **Composable**: Pattern engine orchestrates multiple recognizers
- **Future-Proof**: Unknown future patterns can be added without refactoring
- **Chain Support**: Recursive pattern detection enables chains automatically
- **Preview Mode**: Separation allows showing potential patterns without executing
- **AI-Friendly**: Pattern data structure enables AI analysis of board state

### Negative
- **Initial Complexity**: More upfront design than simple flood-fill
- **More Files**: Each pattern type needs recognizer + executor classes
- **Learning Curve**: Developers must understand the framework
- **Indirection**: Pattern flow is less obvious than direct execution

### Neutral
- **Performance**: Similar to direct approach (pattern detection is fast)
- **Memory**: Patterns are short-lived objects, garbage collected quickly
- **Dependencies**: All recognizers injected, but DI container handles this

## Alternatives Considered

### Alternative 1: Simple Flood-Fill Match Detection
Direct implementation of flood-fill algorithm in match command handler.
- **Pros**: Simple, direct, easy to understand initially
- **Cons**: Requires refactoring for tier-ups, transmutations; violates Open/Closed
- **Reason not chosen**: Would require significant refactoring for VS_003C/D

### Alternative 2: Visitor Pattern
Implement visitor pattern for different board analysis operations.
- **Pros**: Well-known pattern, good for tree traversal
- **Cons**: Awkward for grid structures, requires modifying visited classes
- **Reason not chosen**: Not natural fit for grid-based pattern detection

### Alternative 3: Rule Engine
Full rule engine with DSL for pattern definitions.
- **Pros**: Highly flexible, could define patterns in config files
- **Cons**: Over-engineered for our needs, performance overhead, complexity
- **Reason not chosen**: YAGNI - we don't need runtime pattern definition

### Alternative 4: Strategy Pattern Only
Simple strategy pattern for different match algorithms.
- **Pros**: Simpler than full framework
- **Cons**: Doesn't handle resolution, priorities, or execution separation
- **Reason not chosen**: Insufficient for handling pattern conflicts and chains

## Implementation Notes

### File Structure
```
src/Core/Features/Block/Patterns/
├── Core/
│   ├── IPattern.cs
│   ├── IPatternRecognizer.cs
│   ├── IPatternOutcome.cs
│   └── PatternContext.cs
├── Resolution/
│   ├── IPatternResolver.cs
│   └── PriorityResolver.cs
├── Execution/
│   ├── IPatternExecutor.cs
│   └── ExecutionResult.cs
├── Engine/
│   └── PatternEngine.cs
├── Recognizers/
│   ├── MatchPatternRecognizer.cs    // VS_003A
│   ├── TierUpRecognizer.cs          // VS_003C (future)
│   └── TransmuteRecognizer.cs       // VS_003D (future)
└── Executors/
    ├── MatchPatternExecutor.cs      // VS_003A
    ├── TierUpExecutor.cs            // VS_003C (future)
    └── TransmuteExecutor.cs         // VS_003D (future)
```

### Integration Flow
1. Player action completes → `BlockMovedNotification`
2. `ActionCompletedNotificationHandler` receives notification
3. Handler sends `ProcessPatternsCommand(triggerPosition)`
4. `PatternEngine` calls all registered recognizers
5. `PatternResolver` picks highest priority pattern
6. `PatternExecutor` applies changes to game state
7. Check for new patterns (chains) → recursive if found
8. Publish `PatternProcessedNotification` for UI updates

### Priority Guidelines
- Match patterns: Priority 10
- Tier-up patterns: Priority 20
- Transmute patterns: Priority 30
- Special/power-up patterns: Priority 40+

### Testing Strategy
```csharp
// Test recognizers in isolation
[Fact]
public void Recognize_ThreeHorizontal_ReturnsMatchPattern()
{
    var grid = TestGrid.WithBlocks(...);
    var patterns = recognizer.Recognize(grid, position, context);
    patterns.Should().ContainSingle().Which.Type.Should().Be(PatternType.Match);
}

// Test pattern resolution
[Fact]
public void Resolve_ConflictingPatterns_ChoosesHighestPriority()
{
    var patterns = Seq(matchPattern, tierUpPattern);
    var resolved = resolver.Resolve(patterns);
    resolved.Should().ContainSingle().Which.Should().Be(tierUpPattern);
}
```

## References
- [VS_003A Backlog Item](../../../01-Active/Backlog.md#vs_003a-match-3-with-attributes-phase-1)
- [HANDBOOK.md](../HANDBOOK.md)
- [Clean Architecture Principles](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Open/Closed Principle](https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle)