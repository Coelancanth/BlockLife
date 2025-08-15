# TD_004: Add Memory Leak Detection Test Suite

**Status**: Ready
**Priority**: P2 - Medium
**Size**: M (3 days)
**Sprint**: TBD
**Debt Type**: Testing/Performance
**Impact**: Medium

## Problem Description
No automated memory leak detection in test suite. Memory leaks from undisposed presenters, event subscriptions, and resource handles only discovered through manual profiling or production issues. Static event pattern particularly prone to leaks.

## Current State
- Location: No memory tests exist
- Issue: Memory leaks go undetected until production
- Impact on development: Performance degradation over time
- Risk if not addressed: Application crashes, poor user experience

Current gaps:
- No presenter disposal verification
- No event subscription leak detection
- No resource handle tracking
- No memory growth monitoring

## Proposed Solution

### Test Suite Structure
```
tests/Memory/
├── PresenterLifecycleTests.cs
├── EventSubscriptionTests.cs
├── ResourceLeakTests.cs
├── MemoryGrowthTests.cs
└── Fixtures/
    ├── MemorySnapshot.cs
    └── LeakDetector.cs
```

### Code Changes
1. **Presenter Lifecycle Tests**
   ```csharp
   [Fact]
   public async Task Presenter_SceneTransition_ProperlyDisposed()
   {
       // Arrange: Create presenter, take memory snapshot
       var before = GC.GetTotalMemory(true);
       var presenter = CreatePresenter();
       
       // Act: Simulate scene transition
       presenter.Dispose();
       GC.Collect();
       GC.WaitForPendingFinalizers();
       
       // Assert: Memory freed
       var after = GC.GetTotalMemory(true);
       Assert.True(after <= before + tolerance);
   }
   ```

2. **Event Subscription Tests**
   ```csharp
   [Fact]
   public void StaticEvent_DisposedSubscriber_NoLeak()
   {
       // Create weak reference to subscriber
       // Subscribe to static event
       // Dispose subscriber
       // Force GC
       // Verify weak reference is null
   }
   ```

3. **Memory Growth Tests**
   ```csharp
   [Fact]
   public async Task Operations_1000Iterations_NoMemoryGrowth()
   {
       // Track memory over 1000 operations
       // Verify memory stays within bounds
       // Detect gradual leaks
   }
   ```

### Architecture Changes
- Add memory profiling helpers
- Create weak reference tracking
- Implement memory snapshot comparison
- Add GC forcing utilities

## Benefits
- **Early Detection**: Find leaks in development
- **Performance**: Prevent degradation
- **Reliability**: Avoid production crashes
- **Automation**: No manual profiling needed

## Migration Strategy
- [ ] Can be done incrementally: Yes
- [ ] Backward compatible: Yes
- [ ] Feature flag needed: No
- Migration steps:
  1. Create memory test infrastructure
  2. Add tests for known leak patterns
  3. Integrate with CI pipeline
  4. Add memory benchmarks

## Test Requirements

### Test Coverage
1. **Lifecycle Tests** (10 tests)
   - Presenter disposal
   - Service cleanup
   - Resource release

2. **Event Tests** (8 tests)
   - Static event leaks
   - Weak event verification
   - Subscription cleanup

3. **Growth Tests** (5 tests)
   - Operation iterations
   - Scene transitions
   - Command processing

### Success Metrics
- Detect leaks > 1MB
- Run in < 2 minutes
- Zero false positives

## Acceptance Criteria
- [ ] Memory test suite created
- [ ] 20+ memory tests implemented
- [ ] Known leaks detected and fixed
- [ ] CI integration complete
- [ ] Memory benchmarks established

## Risk Assessment
- **Breaking changes**: None
- **Performance risk**: Tests may be flaky due to GC
- **Rollback plan**: Disable tests if unreliable

## Measurement
- Metric 1: Number of leaks found
- Metric 2: Memory usage reduction
- Success criteria: Find at least 2 memory leaks

## Dependencies
- Must complete after: HF_004 (weak event pattern)
- Can be done alongside: Other test improvements
- Blocks: Long-running stability

## Definition of Done
- [ ] Memory test infrastructure created
- [ ] 20+ tests implemented
- [ ] All tests passing reliably
- [ ] CI integration complete
- [ ] Memory profiling documented
- [ ] Team trained on patterns

## References
- Related issue: TEST-002 from stress test
- Memory leak issue: CRIT-003
- Event pattern: HF_004_Static_Event_Memory_Leaks.md