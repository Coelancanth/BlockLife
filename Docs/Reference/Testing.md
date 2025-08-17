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

## LanguageExt Testing Patterns

### Core Concepts
- **Everything returns `Fin<T>`** - Success (Succ) or failure (Fail), no exceptions
- **`Option<T>` for nullable values** - Some or None, no nulls  
- **`Eff<T>` and `Aff<T>` for effects** - Must be run to get results
- **Pattern matching over if/else** - Use Match for exhaustive handling
- **Immutable collections** - `Lst<T>`, `Map<K,V>`, `Seq<T>` instead of List/Dictionary

### Testing Fin<T> Results
```csharp
// ✅ GOOD - Explicit success assertion
result.IsSucc.Should().BeTrue();
result.IfSucc(value => value.Id.Should().Be(expectedId));

// ✅ GOOD - Pattern matching for comprehensive testing
result.Match(
    Succ: value => {
        value.Should().NotBeNull();
        value.State.Should().Be(ExpectedState.Active);
    },
    Fail: error => Assert.Fail($"Expected success but got: {error.Message}")
);

// ✅ GOOD - Testing failure cases
var result = await handler.Handle(invalidCommand);
result.IsFail.Should().BeTrue();
result.IfFail(error => {
    error.Code.Should().Be("VALIDATION_ERROR");
    error.Message.Should().Contain("Invalid input");
});

// ❌ BAD - Don't use try-catch for functional code
try { 
    var result = await handler.Handle(command);
} catch { /* This won't catch Fin failures! */ }
```

### Testing Option<T> Values
```csharp
// Testing Some case
var option = repository.FindById(id);
option.IsSome.Should().BeTrue();
option.IfSome(entity => {
    entity.Name.Should().Be("Expected");
    entity.IsActive.Should().BeTrue();
});

// Testing None case  
var missing = repository.FindById(nonExistentId);
missing.IsNone.Should().BeTrue();

// Pattern matching for Option
option.Match(
    Some: value => value.Should().BeEquivalentTo(expected),
    None: () => Assert.Fail("Expected value but got None")
);
```

### Testing Effects (Eff/Aff)
```csharp
// Eff<T> - Synchronous effects
[Test]
public void EffectOperation_ValidInput_Success()
{
    var effect = service.CreateEffect(input);
    var result = effect.Run(); // Must run to get result
    
    result.Match(
        Succ: value => value.Should().Be(expected),
        Fail: error => Assert.Fail($"Effect failed: {error}")
    );
}

// Aff<T> - Asynchronous effects
[Test]
public async Task AsyncEffect_ValidInput_Success()
{
    var effect = service.CreateAsyncEffect(input);
    var result = await effect.Run(); // Async run
    
    result.IsSucc.Should().BeTrue();
    result.IfSucc(value => value.Should().NotBeNull());
}
```

### Testing Immutable Collections
```csharp
// Lst<T> - Immutable list
var items = service.GetItems();
items.Count.Should().Be(5);
items.Head.Should().Be(firstItem); // Safe head access
items.Find(x => x.Id == targetId).Should().BeSome();

// Map<K,V> - Immutable dictionary
var map = service.GetConfiguration();
map.Find("key").Should().BeSome();
map["key"].Should().Be("value"); // Throws if missing  
map.TryFind("missing").Should().BeNone(); // Safe access

// Seq<T> - Lazy sequence
var sequence = service.GetLazySequence();
sequence.Take(5).ToList().Should().HaveCount(5);
```

### Custom Assertion Helpers
Your project includes `FluentAssertionsLanguageExtExtensions.cs` with these helpers:

```csharp
// For Fin<T>:
result.ShouldBeSuccess();           // Assert success
result.ShouldBeFail();               // Assert failure  
result.ShouldBeSuccessAnd();        // Assert success and get value
result.ShouldBeFailWithCode("ERR"); // Assert specific error code

// For Option<T>:
option.ShouldBeSome();               // Assert has value
option.ShouldBeNone();               // Assert empty
option.ShouldBeSomeAnd();            // Assert and get value
option.ShouldBeSomeMatching(v => v.Id == 5); // Assert with predicate
```

### Common Pitfalls
- **Forgetting to Run effects** - `Eff` and `Aff` must be `.Run()` to execute
- **Using try-catch for Fin** - Fin failures aren't exceptions
- **Null checks on Option** - Option can't be null, check `IsSome`/`IsNone`
- **Mutating immutable collections** - Operations return new collections
- **Wrong type names** - Use `Fin<T>` not `Either<L,R>` (LanguageExt v4+ change)

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

## Logging Guidelines for Tests

### Log Level Usage

**When to use each level:**

| Level | Use For | Examples | Performance Impact |
|-------|---------|----------|-------------------|
| **Error** | Unrecoverable failures | Exception caught, critical assertion failed | None |
| **Warning** | Recoverable issues | Missing optional config, retry needed, degraded mode | None |
| **Information** | Key lifecycle events | Test suite started/completed, major phase transitions | Minimal |
| **Debug** | Diagnostic details | Test step completed, state changes, decision points | Moderate |
| **Verbose/Trace** | Everything else | Method entry/exit, variable values, loop iterations | HIGH |

### Best Practices

1. **In Test Code:**
   ```csharp
   // ✅ GOOD - Important test lifecycle
   _logger.Information("Starting stress test with {ThreadCount} threads", 100);
   
   // ❌ BAD - Too verbose for Information
   _logger.Information("Loop iteration {i}", i);  // Should be Trace
   
   // ✅ GOOD - Helpful diagnostic at Debug level
   _logger.Debug("Command {CommandId} completed in {Ms}ms", id, elapsed);
   ```

2. **In Production Code Under Test:**
   - Keep Information level SILENT unless it's a major event
   - Use Debug for anything that helps diagnose issues
   - Use Trace for ultra-verbose tracking (method entry/exit)

3. **Performance Considerations:**
   ```csharp
   // Only log expensive operations at Trace level
   if (_logger.IsEnabled(LogEventLevel.Verbose))
   {
       _logger.Verbose("Expensive serialization: {@Object}", complexObject);
   }
   ```

### Common Anti-Patterns to Avoid

❌ **Log Spam**: Every method call at Information level
❌ **Missing Context**: "Operation failed" without details
❌ **Wrong Level**: Debug info at Warning, errors at Debug
❌ **Performance Logs**: Timing every operation at Information

### Test Output Configuration

For different test scenarios:
- **CI/CD**: Information level (clean output)
- **Local Development**: Debug level (helpful diagnostics)
- **Troubleshooting**: Trace level (everything)
- **Performance Tests**: Warning level (only problems)

## Reference Implementations

- **Unit tests**: `tests/Features/Block/Move/MoveBlockCommandHandlerTests.cs`
- **Integration**: `tests/GdUnit4/Features/Block/BlockInteractionTest.cs`
- **Architecture**: `tests/Features/Architecture/ArchitectureStressTest.cs`

---
*Consolidated testing reference for BlockLife. Focus on TDD, clear patterns, and defending architecture.*