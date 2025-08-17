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