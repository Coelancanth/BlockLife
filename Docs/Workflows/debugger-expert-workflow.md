# Debugger Expert Workflow

## Purpose
Define systematic procedures for the Debugger Expert agent to diagnose and resolve complex bugs, especially those involving race conditions, state synchronization, and architectural issues.

---

## Core Workflow Actions

### 1. Diagnose Bug Systematically

**Trigger**: "Debug this issue" or "I'm stuck on this bug"

**Input Required**:
- Bug symptoms/behavior
- When it started occurring
- Reproduction steps (if known)
- Error messages or logs
- What's been tried already

**Steps**:

1. **Gather Initial Evidence**
   ```
   Questions to ask:
   - What exactly happens vs what should happen?
   - When did this last work correctly?
   - What changed between working and broken?
   - How frequently does it occur?
   - Any error messages or stack traces?
   ```

2. **Form Initial Hypotheses**
   ```
   Based on symptoms, likely causes:
   - Race condition (intermittent failures)
   - State corruption (inconsistent behavior)
   - Memory leak (degradation over time)
   - Integration issue (works in isolation)
   - Configuration problem (environment specific)
   ```

3. **Design Diagnostic Tests**
   ```csharp
   // Add strategic logging
   _logger.Information("State before: {State}", currentState);
   await Operation();
   _logger.Information("State after: {State}", newState);
   
   // Add assertions
   Debug.Assert(invariant, "Invariant violated");
   
   // Add breakpoints at key locations
   ```

4. **Isolate the Problem**
   ```
   Binary search approach:
   1. Does it happen in unit tests? → Code logic issue
   2. Does it happen in integration tests? → Integration issue
   3. Does it happen under load? → Concurrency issue
   4. Does it happen in production only? → Environment issue
   ```

5. **Identify Root Cause**
   ```
   Not just "View doesn't update"
   But "Presenter not subscribed because Initialize() not called due to early disposal"
   ```

**Output Format**:
```
🔍 Root Cause Analysis

SYMPTOMS: [What was observed]
ROOT CAUSE: [The actual problem]
EVIDENCE: [How we know this is the cause]

REPRODUCTION:
1. [Step to reproduce]
2. [Step to reproduce]

FIX APPROACH:
[Specific code changes needed]

REGRESSION TEST:
[Test that would have caught this]
```

---

### 2. Debug Notification Pipeline

**Trigger**: "View not updating" or "Events not working"

**Steps**:

1. **Trace Notification Flow**
   ```
   Command → Handler → Publish → Bridge → Presenter → View
   
   Check each step:
   □ Command handler publishes notification?
   □ Notification handler receives it?
   □ Bridge raises static event?
   □ Presenter subscribed to event?
   □ Presenter updates view?
   ```

2. **Common Failure Points**
   ```csharp
   // Check 1: Handler publishes
   public async Task<Fin<Unit>> Handle(Command cmd)
   {
       // Do work...
       await _mediator.Publish(new Notification()); // ← Often missing
   }
   
   // Check 2: Bridge connects
   public class NotificationBridge : INotificationHandler<Notification>
   {
       public static event Action<Data>? Event; // ← Must be static
       
       public Task Handle(Notification n)
       {
           Event?.Invoke(n.Data); // ← Must invoke
       }
   }
   
   // Check 3: Presenter subscribes
   public override void Initialize()
   {
       Bridge.Event += OnEvent; // ← Must subscribe
   }
   
   public override void Dispose()
   {
       Bridge.Event -= OnEvent; // ← Must unsubscribe
   }
   ```

3. **Diagnostic Commands**
   ```csharp
   // Log all notification flow
   _logger.Information("Publishing: {Type}", notification.GetType());
   _logger.Information("Subscribers: {Count}", Event?.GetInvocationList()?.Length);
   _logger.Information("Presenter state: {Initialized}", _initialized);
   ```

**Output**: Exact point where notification pipeline breaks

---

### 3. Debug Race Conditions

**Trigger**: "Intermittent failures" or "Works sometimes"

**Steps**:

1. **Reproduce Under Load**
   ```csharp
   [Test]
   public async Task StressTest_ConcurrentOperations()
   {
       var tasks = Enumerable.Range(0, 100)
           .Select(_ => Task.Run(async () => 
           {
               await PerformOperation();
           }))
           .ToArray();
           
       await Task.WhenAll(tasks);
       // Check for corruption
   }
   ```

2. **Identify Shared State**
   ```csharp
   // Look for:
   - Static variables
   - Singleton services
   - Shared collections
   - Cache instances
   
   // Add thread-safety checks:
   private readonly object _lock = new();
   lock (_lock)
   {
       // Critical section
   }
   ```

3. **Check Async Patterns**
   ```csharp
   // Common issues:
   
   // ❌ Fire and forget
   Task.Run(() => DoWork());
   
   // ❌ Blocking on async
   var result = DoWorkAsync().Result;
   
   // ❌ Missing await
   DoWorkAsync(); // No await!
   
   // ✅ Proper async
   await DoWorkAsync();
   ```

4. **Add Synchronization**
   ```csharp
   // Use thread-safe collections
   ConcurrentDictionary<K, V>
   ConcurrentQueue<T>
   
   // Use async synchronization
   SemaphoreSlim
   AsyncLock
   
   // Use immutable data
   public record State(int Value);
   ```

**Output**: Race condition identified with fix

---

### 4. Debug State Synchronization

**Trigger**: "State corruption" or "Phantom entities"

**Steps**:

1. **Map State Sources**
   ```
   Find all sources of truth:
   - Database/Repository
   - In-memory cache
   - UI state
   - Service state
   
   Should be exactly ONE source
   ```

2. **Check DI Registration**
   ```csharp
   // ❌ WRONG - Dual sources
   services.AddSingleton<IGridState, GridStateService>();
   services.AddSingleton<IBlockRepository, BlockRepository>();
   
   // ✅ CORRECT - Single source
   services.AddSingleton<GridStateService>();
   services.AddSingleton<IGridState>(p => p.GetRequiredService<GridStateService>());
   services.AddSingleton<IBlockRepository>(p => p.GetRequiredService<GridStateService>());
   ```

3. **Verify State Consistency**
   ```csharp
   // Add validation
   public bool ValidateState()
   {
       var invariants = new[]
       {
           () => _blocks.Count == _blockIndex.Count,
           () => _blocks.All(b => _blockIndex.ContainsKey(b.Id)),
           () => !_blocks.GroupBy(b => b.Position).Any(g => g.Count() > 1)
       };
       
       return invariants.All(check => check());
   }
   ```

**Output**: State inconsistency source identified

---

### 5. Debug Memory Leaks

**Trigger**: "Memory growing" or "App slows over time"

**Steps**:

1. **Identify Leak Patterns**
   ```
   Common sources:
   - Event handlers not unsubscribed
   - Static collections growing
   - Disposed objects still referenced
   - Circular references
   ```

2. **Check Event Subscriptions**
   ```csharp
   // ❌ Strong reference - leaks
   public static event Action<Data> Event;
   
   // ✅ Weak reference - no leak
   public static event EventHandler<Data> Event
   {
       add { WeakEvent.Subscribe(value); }
       remove { WeakEvent.Unsubscribe(value); }
   }
   ```

3. **Check Disposal**
   ```csharp
   public override void Dispose()
   {
       // Unsubscribe all events
       SomeEvent -= Handler;
       
       // Clear collections
       _cache?.Clear();
       
       // Dispose managed resources
       _subscription?.Dispose();
       
       base.Dispose();
   }
   ```

4. **Memory Profiling**
   ```
   Tools to suggest:
   - dotMemory
   - PerfView
   - Visual Studio Diagnostic Tools
   
   Look for:
   - Growing object counts
   - Retained references
   - Large object heap
   ```

**Output**: Memory leak source and fix

---

### 6. Create Regression Test

**Trigger**: After bug is fixed

**Steps**:

1. **Capture Bug Scenario**
   ```csharp
   [Test]
   public async Task Regression_BugId_Description()
   {
       // Arrange - Setup exact conditions that caused bug
       var conditions = CreateBugConditions();
       
       // Act - Perform operation that failed
       var result = await OperationThatFailed();
       
       // Assert - Verify bug doesn't recur
       result.Should().NotHaveOldBugBehavior();
   }
   ```

2. **Test Must Fail Without Fix**
   ```
   1. Temporarily revert fix
   2. Run regression test
   3. Verify test fails
   4. Reapply fix
   5. Verify test passes
   ```

**Output**: Regression test preventing recurrence

---

## Debugging Patterns Reference

### Systematic Approach Template
```
1. REPRODUCE
   - Get minimal reproduction
   - Verify consistently reproducible
   
2. ISOLATE
   - Binary search to component
   - Remove unrelated code
   
3. DIAGNOSE
   - Form hypothesis
   - Test hypothesis
   - Gather evidence
   
4. FIX
   - Implement minimal fix
   - Verify fix works
   
5. PREVENT
   - Add regression test
   - Document lesson learned
```

### Common Fix Patterns

**Race Condition Fix**:
```csharp
// Before: Unsafe
private List<Item> _items = new();

// After: Thread-safe
private ConcurrentBag<Item> _items = new();
```

**Event Leak Fix**:
```csharp
// Before: Strong reference
public static event Action Event;

// After: Weak reference
private static readonly WeakEvent<EventArgs> _event = new();
```

**State Sync Fix**:
```csharp
// Before: Multiple sources
services.AddSingleton<IService1, ServiceImpl>();
services.AddSingleton<IService2, ServiceImpl>();

// After: Single source
services.AddSingleton<ServiceImpl>();
services.AddSingleton<IService1>(p => p.GetRequiredService<ServiceImpl>());
```

---

## Quality Checklist

Before marking diagnosis complete:
- [ ] Root cause identified (not just symptoms)?
- [ ] Reproduction steps documented?
- [ ] Fix verified to work?
- [ ] Regression test created?
- [ ] No side effects from fix?
- [ ] Lesson learned documented?

---

## Response Templates

### When bug diagnosed:
"🔍 Bug Diagnosed: [Issue Name]

ROOT CAUSE: [Specific technical cause]
EVIDENCE: [How we determined this]

REPRODUCTION:
1. [Step]
2. [Step]

FIX:
[Code changes needed]

Location: [File:Line]
Confidence: [High/Medium/Low]

Regression test will prevent recurrence."

### When more info needed:
"🔍 Need more information:

To diagnose, please provide:
1. [Specific log or trace]
2. [Reproduction step]
3. [Configuration detail]

This will help determine if it's a [hypothesis]."

### When providing fix:
"✅ Fix for [Issue]:

```csharp
// Change this (line X)
[old code]

// To this
[new code]
```

This fixes the root cause by [explanation].
Also created regression test to prevent recurrence."