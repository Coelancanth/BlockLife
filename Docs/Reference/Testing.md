# Testing Reference

## Core Testing Philosophy

**Goal**: Tests defend architecture and verify business logic  
**Strategy**: TDD with FluentAssertions  
**Integration**: GdUnit4 for Godot components  

## Test Types & When to Use

### 1. Unit Tests (Most Common)
- **What**: Pure business logic, commands, handlers
- **Where**: `tests/BlockLife.Core.Tests/`
- **Speed**: Fast (< 10ms)
- **Pattern**: TDD Red-Green-Refactor

### 2. Integration Tests (Feature Validation)
- **What**: End-to-end feature flows, UI to state changes
- **Where**: `tests/GdUnit4/Features/`
- **Speed**: Medium (< 100ms)
- **Pattern**: Complete vertical slice validation

### 3. Architecture Tests (Always Required)
- **What**: Ensure patterns are followed
- **When**: Every feature implementation
- **Purpose**: Defend architectural decisions

### 4. Stress Tests (Complex Features)
- **What**: Concurrent operations, race conditions
- **When**: Features with shared state
- **Goal**: Find threading issues early

## Standard Test Patterns

### Unit Test Pattern
```csharp
[Test]
public async Task Method_Scenario_ExpectedOutcome()
{
    // Arrange
    var command = new Command(...);
    
    // Act  
    var result = await _handler.Handle(command);
    
    // Assert
    result.IsSucc.Should().BeTrue();
}
```

### Integration Test Pattern (GdUnit4)
```csharp
[TestSuite]
public partial class FeatureIntegrationTest : Node
{
    [TestCase]
    public async Task CompleteFlow_ValidInput_MeetsAcceptance()
    {
        // Setup test scene
        var scene = await LoadTestScene();
        
        // Execute user workflow
        await SimulateUserActions();
        
        // Validate complete result
        AssertAcceptanceCriteriaMet();
    }
}
```

## Edge Cases Checklist

Always test:
- **Null/empty inputs** - defensive programming
- **Boundary values** - min/max/zero
- **Invalid states** - operations on disposed objects
- **Concurrent access** - race conditions
- **Resource exhaustion** - memory/handle limits
- **Timing issues** - rapid operations, delays

## Test Organization

```
tests/
├── BlockLife.Core.Tests/        # Unit tests
│   └── Features/
│       └── [Feature]/
│           ├── CommandHandlerTests.cs
│           └── ServiceTests.cs
│
└── GdUnit4/                      # Integration tests
    └── Features/
        └── [Feature]/
            ├── IntegrationTest.cs
            └── StressTest.cs
```

## Quality Criteria

- **Fast execution** - unit tests <100ms
- **Clear failure messages** - know what broke
- **Isolated** - tests don't affect each other
- **Deterministic** - same result every run
- **Meaningful** - tests actual behavior, not implementation

## Reference Implementations

- **Unit tests**: `tests/Features/Block/Move/MoveBlockCommandHandlerTests.cs`
- **Integration**: `tests/GdUnit4/Features/Block/BlockInteractionTest.cs`
- **Architecture**: `tests/Features/Architecture/ArchitectureStressTest.cs`

---
*Consolidated testing reference for BlockLife. Focus on TDD, clear patterns, and defending architecture.*