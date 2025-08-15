# QA Engineer Workflow

## Purpose
Define exact procedures for the QA/Test Engineer agent to create integration tests, stress tests, and validate system robustness.

---

## Core Workflow Actions

### 1. Create Integration Tests

**Trigger**: "Create integration tests for VS_XXX" or after implementation complete

**Input Required**:
- VS item path with implementation details
- Code location (e.g., `src/Features/Block/Move/`)
- Acceptance criteria to validate

**Steps**:
1. **Analyze Feature Flow**
   - Read VS item acceptance criteria
   - Identify user workflows to test
   - Map component interactions

2. **Design Test Scenarios**
   ```
   Happy Path Tests:
   - Complete feature flow works
   - All acceptance criteria met
   - State updates correctly
   
   Failure Mode Tests:
   - Invalid inputs handled
   - Error states recoverable
   - Rollback works correctly
   ```

3. **Create Integration Test Suite**
   - Location: `tests/BlockLife.Integration.Tests/Features/[Domain]/`
   - Use GdUnit4 for Godot integration
   - Test full stack from UI to state

4. **Validate Acceptance Criteria**
   - Each criterion gets specific test
   - Document coverage mapping
   - Report any gaps found

**Output Format**:
```csharp
[TestSuite]
public partial class [Feature]IntegrationTest : Node
{
    [TestCase]
    public async Task CompleteFeatureFlow_ValidInputs_MeetsAcceptanceCriteria()
    {
        // Arrange: Setup test scene
        // Act: Execute feature flow
        // Assert: Validate all criteria met
    }
    
    [TestCase]
    public async Task FeatureUnderLoad_ConcurrentUsers_RemainsStable()
    {
        // Stress test with parallel operations
    }
}
```

---

### 2. Create Stress Tests

**Trigger**: "Stress test VS_XXX" or critical feature complete

**Input Required**:
- Feature to stress test
- Expected load parameters
- Performance requirements

**Steps**:
1. **Identify Stress Points**
   - Concurrent operation risks
   - State mutation bottlenecks
   - Resource intensive operations

2. **Design Stress Scenarios**
   ```
   Concurrency Tests:
   - 100+ simultaneous operations
   - Rapid repeated actions
   - Parallel state modifications
   
   Load Tests:
   - Maximum entity counts
   - Memory pressure scenarios
   - Network saturation (if applicable)
   ```

3. **Implement Stress Suite**
   ```csharp
   [TestCase]
   public async Task StressTest_100ConcurrentOperations_NoRaceConditions()
   {
       var tasks = new Task[100];
       for (int i = 0; i < 100; i++)
       {
           tasks[i] = Task.Run(() => ExecuteOperation());
       }
       await Task.WhenAll(tasks);
       
       // Assert no corruption, all operations succeeded
   }
   ```

4. **Measure and Report**
   - Operation throughput
   - Memory usage patterns
   - Response time degradation
   - Failure thresholds

**Output**: Stress test report with metrics and recommendations

---

### 3. Find Edge Cases

**Trigger**: "Find edge cases for VS_XXX"

**Steps**:
1. **Boundary Analysis**
   - Minimum/maximum values
   - Empty/null/undefined states
   - Resource limits

2. **Timing Issues**
   - Rapid clicks/inputs
   - Slow operations
   - Timeout scenarios
   - Order dependencies

3. **State Transitions**
   - Invalid state combinations
   - Interrupted operations
   - Partial completions
   - Rollback scenarios

4. **Document Edge Cases**
   ```markdown
   ## Edge Cases Found
   
   1. Rapid double-click causes duplicate commands
      - Impact: State corruption
      - Mitigation: Debounce input
   
   2. Memory pressure causes allocation failures
      - Impact: Crash at 1000+ entities
      - Mitigation: Implement pooling
   ```

**Output**: Edge case documentation with test coverage

---

### 4. Create Regression Tests

**Trigger**: Bug found or "Create regression test for BF_XXX"

**Input Required**:
- Bug report with reproduction steps
- Root cause analysis
- Fix implementation

**Steps**:
1. **Reproduce Bug**
   - Follow exact reproduction steps
   - Verify bug exists
   - Document preconditions

2. **Create Failing Test**
   ```csharp
   [TestCase]
   public async Task Regression_BF045_StateCorruptionUnderLoad()
   {
       // This test would have caught BF_045
       // Reproduces exact conditions that caused bug
       
       // Setup conditions that triggered bug
       // Execute problematic operation
       // Assert corruption doesn't occur
   }
   ```

3. **Verify Fix**
   - Test now passes with fix
   - No side effects introduced
   - Performance acceptable

4. **Add to Test Suite**
   - Permanent regression test
   - Runs in CI/CD pipeline
   - Prevents recurrence

**Output**: Regression test that prevents bug from returning

---

### 5. Validate Performance

**Trigger**: "Benchmark VS_XXX" or performance concerns

**Steps**:
1. **Establish Baselines**
   - Current performance metrics
   - Expected targets
   - Critical operations

2. **Create Benchmark Tests**
   ```csharp
   [Benchmark]
   public async Task MoveBlockOperation_Performance()
   {
       var stopwatch = Stopwatch.StartNew();
       
       await ExecuteMoveBlock();
       
       Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(16);
       // Must complete in one frame (60fps)
   }
   ```

3. **Profile Under Load**
   - Memory allocations
   - CPU usage
   - Frame rate impact
   - Resource leaks

4. **Report Findings**
   ```
   Performance Report:
   - Operation: Move Block
   - Average time: 8ms
   - 95th percentile: 12ms
   - Under load (100 ops): 15ms
   - Verdict: PASS (under 16ms target)
   ```

---

## Test Organization

### File Structure
```
tests/
├── BlockLife.Integration.Tests/
│   ├── Features/
│   │   ├── Block/
│   │   │   └── MoveBlockIntegrationTests.cs
│   │   └── Inventory/
│   └── Stress/
│       └── ConcurrencyTests.cs
├── BlockLife.Performance.Tests/
│   └── Benchmarks/
└── BlockLife.Regression.Tests/
    └── BugFixes/
```

### Test Categories
- `[Category("Integration")]` - Full stack tests
- `[Category("Stress")]` - Load and concurrency
- `[Category("Performance")]` - Benchmarks
- `[Category("Regression")]` - Bug prevention
- `[Category("EdgeCase")]` - Boundary conditions

---

## Quality Checklist

Before completing any test action:
- [ ] Tests cover acceptance criteria?
- [ ] Concurrent scenarios tested?
- [ ] Edge cases documented?
- [ ] Performance benchmarked?
- [ ] Regression tests for bugs?
- [ ] Test documentation clear?
- [ ] Can reproduce failures?

---

## Integration Points

### With Tech Lead
- Receive implementation details
- Report technical issues found
- Suggest architecture improvements

### With Product Owner
- Validate acceptance criteria
- Report features not meeting requirements
- Create BF items for bugs found

### With Backlog Maintainer
- Update test completion progress
- Mark items ready for deployment
- Track test coverage metrics

---

## Common Test Patterns

### Concurrent Operation Testing
```csharp
// Pattern for testing race conditions
var barrier = new Barrier(participantCount);
var tasks = Enumerable.Range(0, participantCount)
    .Select(_ => Task.Run(async () =>
    {
        barrier.SignalAndWait(); // Synchronize start
        await ExecuteOperation();
    }))
    .ToArray();

await Task.WhenAll(tasks);
// Assert no corruption
```

### State Verification Pattern
```csharp
// Snapshot -> Operation -> Verify
var stateBefore = CaptureState();
await ExecuteOperation();
var stateAfter = CaptureState();

Assert.That(stateAfter).IsConsistentWith(stateBefore);
```

---

## Response Templates

### When tests complete:
"✅ Integration tests created for VS_XXX
- Test location: tests/BlockLife.Integration.Tests/
- Scenarios covered: 8 integration, 3 stress
- Edge cases found: 2 (documented)
- All acceptance criteria validated
- Performance: Within targets"

### When issues found:
"⚠️ QA Issues Found in VS_XXX:
1. Race condition under load (>50 concurrent)
2. Memory leak after 1000 operations
3. Edge case: Null reference on empty state

Recommended actions:
- Create HF for race condition (CRITICAL)
- Add defensive null checks
- Implement resource pooling"

---

## Feedback Integration

Track and report:
- Test coverage gaps
- Patterns of failures
- Performance trends
- Common edge cases

Use feedback commands:
- "QA feedback: Missing stress tests"
- "QA feedback: Edge case not covered"
- "QA feedback: Performance degradation"