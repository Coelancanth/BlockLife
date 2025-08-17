# 🔧 Technical Patterns & Deep Knowledge

## 🎯 When to Use This Document

**Use for:**
- Complex debugging and investigation
- Architecture decision-making
- Learning from historical bug patterns
- Production readiness validation
- State management and performance issues

**Don't use for:**
- Basic agent selection (→ Agent_Quick_Reference.md)
- Daily workflow steps (→ Development_Workflows.md)
- Simple template lookup (→ Agent_Quick_Reference.md)

## 🐛 Bug Patterns & Debugging

### 🚨 Common Historical Bug Patterns

#### GUID Stability Issues
**Problem**: Properties generating new values on each access
```csharp
// ❌ DANGEROUS - New GUID every time!
public Guid BlockId => RequestedId ?? Guid.NewGuid();

// ✅ SAFE - Cached stable value
private readonly Lazy<Guid> _blockId = new(() => Guid.NewGuid());
public Guid BlockId => RequestedId ?? _blockId.Value;
```
**Detection**: "Already exists" errors despite successful operations
**Prevention**: Test property stability across multiple accesses

#### DI Registration Circular Dependencies
**Problem**: Presenters depending on Views not in DI container
```csharp
// ❌ PROBLEM - Presenter as notification handler
public class MyPresenter : INotificationHandler<Event>

// ✅ SOLUTION - Separate handler + factory pattern
public class MyPresenter // No MediatR interfaces
// Create via PresenterFactory at runtime
```
**Detection**: DI validation errors at startup
**Prevention**: Presenters depend only on interfaces, use factories

#### Duplicate Notification Publishing
**Problem**: Same notification published from multiple handlers
**Detection**: Log entries showing duplicate notifications
**Root Cause**: Command handler AND simulation manager both publishing
**Fix**: Single point of notification responsibility per event

#### Missing Validation Enforcement
**Problem**: Business rules bypassed in handlers
```csharp
// ❌ DANGEROUS - Direct state change
await _gridService.RemoveBlock(id);

// ✅ SAFE - Validation first
var validation = await _validator.ValidateRemoval(id);
if (validation.IsFail) return validation.Error;
await _gridService.RemoveBlock(id);
```

### 🔍 Systematic Bug Investigation

#### Phase 1: Reproduction & Isolation
1. **Create minimal reproduction case**
2. **Binary search through recent changes**
3. **Isolate to specific component or layer**
4. **Document exact reproduction steps**

#### Phase 2: Analysis & Hypothesis
1. **Analyze symptoms**: Logs, state, timing
2. **Form hypothesis**: What could cause this behavior?
3. **Identify potential root causes**
4. **Check similar patterns in codebase**

#### Phase 3: Testing & Verification  
1. **Test hypothesis with focused experiments**
2. **Implement minimal fix targeting root cause**
3. **Create regression test to prevent recurrence**
4. **Verify fix doesn't introduce new issues**

### 🚨 Critical Investigation Patterns
- **Race conditions**: Timing-dependent failures, use stress testing
- **Memory leaks**: Resources not properly disposed, check using statements
- **State corruption**: Shared mutable state, verify thread safety
- **Performance issues**: Operations >16ms, profile and optimize
- **Type ambiguity**: Conflicts between custom/library types, use fully qualified names

## 🏗️ Architecture Patterns

### 🔥 State Management (Critical Anti-Patterns)

#### The Dual-State Disaster (NEVER DO THIS)
```csharp
// ❌ CREATES RACE CONDITIONS AND DATA CORRUPTION
services.AddSingleton<IGridStateService, GridStateService>();
services.AddSingleton<IBlockRepository, InMemoryBlockRepository>();

// Results in TWO separate data stores:
// - GridStateService: Uses ConcurrentDictionary (thread-safe)
// - InMemoryBlockRepository: Uses Dictionary (NOT thread-safe)
```

**Why This Fails:**
- **Race Conditions**: Plain Dictionary throws InvalidOperationException under concurrent access
- **State Drift**: Two stores managing same data get out of sync
- **Phantom Data**: Blocks exist in one store but not the other
- **Silent Corruption**: Data loss without error indication

#### The Correct Pattern (ALWAYS DO THIS)
```csharp
// ✅ SINGLE SOURCE OF TRUTH - Thread-safe and consistent
services.AddSingleton<GridStateService>();
services.AddSingleton<IGridStateService>(provider => 
    provider.GetRequiredService<GridStateService>());
services.AddSingleton<IBlockRepository>(provider => 
    provider.GetRequiredService<GridStateService>());
```

**Why This Works:**
- **Single Instance**: One concrete class manages all state
- **Multiple Interfaces**: Different concerns access through appropriate interfaces
- **Thread Safety**: ConcurrentDictionary handles concurrent access
- **Consistency**: Impossible for state to diverge

### 🔒 Thread Safety Patterns

#### Dual Dictionary Anti-Pattern (CRITICAL LESSON)
**Problem**: Using two dictionaries to manage the same entities creates race conditions
```csharp
// ❌ DANGEROUS - Dual state that can desynchronize
private readonly ConcurrentDictionary<Vector2Int, Block> _blocksByPosition;
private readonly ConcurrentDictionary<Guid, Block> _blocksById;

// Race condition scenario:
// Thread 1: MoveBlock(A, pos1 → pos2) 
// Thread 2: MoveBlock(B, pos3 → pos2)
// Both pass "empty" check, both try to move → data corruption
```

**Solution**: Single source with efficient access patterns
```csharp
// ✅ SAFER - Single source with lookup optimization
private readonly ConcurrentDictionary<Guid, Block> _blocks;
private readonly ConcurrentDictionary<Vector2Int, Guid> _positionIndex;

// Atomic operations that maintain consistency
public bool TryMoveBlock(Guid blockId, Vector2Int newPosition)
{
    if (!_blocks.TryGetValue(blockId, out var block)) return false;
    
    // Atomic position update
    _positionIndex.TryRemove(block.Position, out _);
    _positionIndex.TryAdd(newPosition, blockId);
    _blocks.TryUpdate(blockId, block with { Position = newPosition }, block);
    
    return true;
}
```

#### Thread-Safe Collection Choices
```csharp
// ✅ Thread-safe collections for shared state
private readonly ConcurrentDictionary<TKey, TValue> _data = new();
private readonly ConcurrentQueue<TItem> _queue = new();
private readonly ConcurrentBag<TItem> _bag = new();

// ❌ NOT thread-safe - will throw exceptions under load
private readonly Dictionary<TKey, TValue> _unsafeData = new();
private readonly List<TItem> _unsafeList = new();
```

#### Stress Testing for Concurrency
```csharp
[TestCase]
public async Task StressTest_ConcurrentMoves_NoStateCorruption()
{
    // Barrier ensures all operations start simultaneously
    var barrier = new Barrier(100);
    
    var tasks = Enumerable.Range(0, 100)
        .Select(i => Task.Run(async () =>
        {
            barrier.SignalAndWait();
            await ExecuteMoveOperation(i);
        }))
        .ToArray();
    
    await Task.WhenAll(tasks);
    
    // Critical: Verify no blocks lost or duplicated
    AssertAllBlocksAccountedFor();
    AssertNoPositionConflicts();
}

#### Stress Testing for Thread Safety
```csharp
[TestCase]
public async Task StressTest_ConcurrentOperations_NoCorruption()
{
    // Create barrier for simultaneous start
    var barrier = new Barrier(100);
    
    // 100 parallel operations
    var tasks = Enumerable.Range(0, 100)
        .Select(i => Task.Run(async () =>
        {
            barrier.SignalAndWait(); // All tasks start together
            await ExecuteOperation(i);
        }))
        .ToArray();
    
    await Task.WhenAll(tasks);
    
    // Verify system integrity
    AssertNoStateCorruption();
    AssertExpectedFinalState();
}
```

### 📡 Notification Pipeline Debugging

#### Common Symptoms
- ✅ Commands execute successfully (no errors logged)
- ✅ Business logic completes (data is stored/updated)  
- ❌ View doesn't update to reflect changes
- ❌ Users don't see expected visual feedback

#### Systematic Debugging Approach

**Phase 1: Command Handler Verification**
```csharp
// Add temporary tracing in command handler
_logger.LogInformation("📢 Publishing {NotificationType}", nameof(MyNotification));
await _mediator.Publish(new MyNotification(data));
_logger.LogInformation("✅ Published {NotificationType}", nameof(MyNotification));
```

**Phase 2: Notification Handler Verification** 
```csharp
// Add tracing in notification handlers
_logger.LogInformation("🔔 Received {NotificationType}", nameof(MyNotification));
// Handler logic here
_logger.LogInformation("✅ Processed {NotificationType}", nameof(MyNotification));
```

**Phase 3: View Update Verification**
```csharp
// Add tracing in view updates
_logger.LogInformation("🎨 Updating view with {Data}", data);
// View update logic here
_logger.LogInformation("✅ View updated");
```

**Phase 4: Event System Check**
- Verify event subscriptions are active
- Check for unsubscribed handlers
- Validate presenter lifecycle management

### 🎯 Production Readiness Validation

#### Critical Validation Checklist
- [ ] **Thread Safety**: All shared state uses concurrent collections
- [ ] **Memory Management**: All disposable resources properly disposed
- [ ] **Error Handling**: All operations use Fin<T> pattern consistently
- [ ] **Validation**: All business rules enforced before state changes
- [ ] **Notification Integrity**: Each event published exactly once
- [ ] **Performance**: All operations complete within frame time (16ms)

#### Stress Testing Procedures
1. **Concurrent Operations**: 100+ simultaneous operations
2. **Memory Pressure**: Large datasets and resource exhaustion
3. **Edge Case Bombardment**: Invalid inputs and boundary conditions
4. **Performance Under Load**: Response times under stress
5. **State Consistency**: Verify no corruption after stress

#### Performance Validation
```csharp
[TestCase]
public async Task Performance_OperationCompletes_WithinFrameTime()
{
    var stopwatch = Stopwatch.StartNew();
    
    await ExecuteOperation();
    
    stopwatch.Stop();
    
    // Must complete within 60fps frame time
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(16), 
        "Operation must complete within frame time for smooth 60fps");
}
```

## 🔗 Cross-References

### Related Essential Three Documents
- **[Agent_Quick_Reference.md](Agent_Quick_Reference.md)** - Daily agent orchestration and basic templates
- **[Development_Workflows.md](Development_Workflows.md)** - Process checklists and workflow steps
- **[Architecture_Guide.md](Shared/Core/Architecture/Architecture_Guide.md)** - Core architectural principles

### Specialized Documentation  
- **Move Block Reference**: `src/Features/Block/Move/` - GOLD STANDARD implementation
- **Living Wisdom**: `Docs/Living-Wisdom/` - Advanced operational knowledge (when needed)
- **Bug Templates**: `Docs/Shared/Templates/Bug-Report-Templates/` - Incident reporting

---

*This technical reference provides deep knowledge for complex problem-solving while keeping daily workflow documents focused and lightweight.*