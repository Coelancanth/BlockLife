# QA Engineer Workflow

## Core Principle

**Find what breaks under real conditions** - integration flows, concurrent stress, edge cases.

## Action 1: Integration Testing

### When to Use
After feature implementation complete - "Test VS_XXX integration"

### Process
1. **Read acceptance criteria** from VS item
2. **Test end-to-end flow** - UI interactions to state changes
3. **Verify all criteria met** with actual feature usage
4. **Document any gaps** found

### Integration Test Template
```csharp
[TestSuite]
public partial class FeatureIntegrationTest : Node
{
    [TestCase]
    public async Task CompleteFeatureFlow_ValidInput_MeetsAcceptance()
    {
        // Arrange: Setup test scene
        var scene = await LoadTestScene();
        
        // Act: Execute full feature flow
        await SimulateUserInteraction();
        
        // Assert: Validate acceptance criteria
        AssertFeatureWorksAsExpected();
    }
}
```

## Action 2: Stress Testing

### When to Use
After integration tests pass - "Stress test VS_XXX"

### Process
1. **Identify stress points** - concurrent operations, shared state
2. **Run 100+ concurrent operations** simultaneously
3. **Check for race conditions** and state corruption
4. **Measure performance impact**

### Stress Test Template
```csharp
[TestCase]
public async Task StressTest_100ConcurrentOps_NoRaceConditions()
{
    // Arrange
    var tasks = Enumerable.Range(0, 100)
        .Select(_ => Task.Run(() => ExecuteOperation()))
        .ToArray();
    
    // Act
    await Task.WhenAll(tasks);
    
    // Assert
    AssertNoStateCorruption();
    AssertAllOperationsSucceeded();
}
```

## Action 3: Edge Case Discovery

### When to Use
During integration testing - "Find edge cases for VS_XXX"

### Common Edge Cases to Test
- **Null/empty inputs**: What if data is missing?
- **Boundary values**: Min/max limits, negative numbers
- **Rapid interactions**: Double-clicks, spam clicking
- **Memory pressure**: Large data sets, resource exhaustion
- **Timing issues**: Slow operations, timeouts

### Edge Case Documentation
```markdown
## Edge Cases Found in [Feature]

1. **Rapid double-click causes duplicate operations**
   - Impact: State corruption
   - Reproduction: Click button rapidly 10+ times
   - Mitigation: Debounce input handling

2. **Null input crashes handler**
   - Impact: Application crash
   - Reproduction: Send null command data
   - Mitigation: Add null checks
```

## Action 4: Regression Testing

### When to Use
When bugs are found - "Create regression test for BF_XXX"

### Process
1. **Reproduce the bug** exactly as reported
2. **Create test that would have caught it**
3. **Verify test fails before fix, passes after fix**
4. **Add to permanent test suite**

### Regression Test Template
```csharp
[TestCase]
public async Task Regression_BF045_StateCorruptionUnderLoad()
{
    // This test reproduces the exact conditions that caused BF_045
    // and ensures it never happens again
    
    // Arrange: Setup conditions that triggered the bug
    // Act: Execute the problematic operation
    // Assert: Verify corruption doesn't occur
}
```

## Testing Technology

### GdUnit4 for Godot Integration
- Test actual Godot scenes and nodes
- Simulate user interactions
- Verify UI updates correctly

### Concurrent Testing Patterns
```csharp
// Barrier pattern for synchronized stress testing
var barrier = new Barrier(100);
var tasks = Enumerable.Range(0, 100)
    .Select(_ => Task.Run(async () =>
    {
        barrier.SignalAndWait(); // All start together
        await ExecuteOperation();
    }))
    .ToArray();
```

## File Organization

```
tests/BlockLife.Integration.Tests/
├── Features/
│   ├── Block/
│   │   └── MoveBlockIntegrationTests.cs
│   └── [Feature]/
├── Stress/
│   └── ConcurrencyTests.cs
└── Regression/
    └── BugFixTests.cs
```

## Success Metrics

- **Features work end-to-end** as specified
- **No race conditions** under 100+ concurrent operations
- **Edge cases documented** with clear reproduction steps
- **Performance acceptable** (operations complete within frame time)
- **Bugs prevented** by regression tests

## Quality Gates

- All acceptance criteria validated
- Stress tests pass without corruption
- Edge cases documented and tested
- Performance within acceptable limits
- No obvious issues found