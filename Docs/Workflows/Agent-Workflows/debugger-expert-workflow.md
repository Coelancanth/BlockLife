# Debugger Expert Workflow - Godot/C# Specialist

## Purpose
Define systematic procedures for the Debugger Expert agent to diagnose and resolve complex bugs in Godot/C# applications, specializing in node lifecycle issues, signal problems, thread safety, MVP pattern debugging, and Clean Architecture boundary violations in the Godot context.

---

## Core Workflow Actions

### 1. Diagnose Godot/C# Bug Systematically

**Trigger**: "Debug this issue" or "I'm stuck on this bug" or "Node not working"

**Input Required**:
- Bug symptoms/behavior
- Scene structure and node hierarchy
- When it occurs (scene load, scene change, runtime)
- Error messages from Godot console
- C# stack traces
- What's been tried already

**Steps**:

1. **Gather Godot-Specific Evidence**
   ```
   Critical questions for Godot bugs:
   - Does it happen in _Ready() or _EnterTree()?
   - Is it related to scene changes or node disposal?
   - Are signals connected/disconnected properly?
   - Is it a main thread vs async thread issue?
   - Does the node exist in the scene tree when accessed?
   - Are there GodotObject disposal/reference issues?
   ```

2. **Form Godot-Aware Hypotheses**
   ```
   Common Godot/C# issues by symptom:
   - "Object reference not set" → Node disposed or not ready
   - "Can't change state while flushing queries" → Thread safety issue
   - Signal not firing → Weak reference or disposal race
   - Intermittent failures → Scene tree timing or async issues
   - Memory growing → Static event subscriptions to Godot nodes
   - "Node not found" → Scene not loaded or wrong path
   ```

3. **Design Godot Diagnostic Tests**
   ```csharp
   // Check node lifecycle state
   GD.Print($"Node ready: {IsNodeReady()}");
   GD.Print($"In tree: {IsInsideTree()}");
   GD.Print($"Scene root: {GetTree()?.Root?.Name}");
   
   // Verify thread context
   GD.Print($"Main thread: {OS.IsMainThread()}");
   GD.Print($"Thread ID: {Thread.CurrentThread.ManagedThreadId}");
   
   // Check signal connections
   GD.Print($"Signal connections: {GetSignalConnectionList(signalName).Count}");
   
   // Add Godot-safe assertions
   if (!IsInstanceValid(node))
       GD.PrintErr("Node is disposed but still referenced!");
   ```

4. **Isolate Godot-Specific Problems**
   ```
   Godot debugging hierarchy:
   1. Does it work in a minimal scene? → Complex scene issue
   2. Does it work without C# scripts? → C# integration issue
   3. Does it work with CallDeferred? → Thread timing issue
   4. Does it work without autoloads? → Singleton lifecycle issue
   5. Does it work in editor vs export? → Build configuration issue
   ```

5. **Identify Godot Root Cause**
   ```
   Not just "Node is null"
   But "Presenter disposed during scene change before view unsubscribed from bridge event"
   
   Not just "Signal not received"
   But "Signal emitted before node entered tree, lost due to weak reference"
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

### 2. Debug Godot Node Lifecycle Issues

**Trigger**: "Node is null" or "Object disposed" or "Scene change breaks things"

**Steps**:

1. **Map Node Lifecycle Flow**
   ```
   Scene Load → _EnterTree() → _Ready() → Active
                     ↓              ↓         ↓
              Can access      Can setup   Full function
               parent          children    available
   
   Scene Exit → _ExitTree() → Dispose() → Freed
                    ↓            ↓          ↓
               Unsubscribe   Clean up   No access
   ```

2. **Common Godot Lifecycle Bugs**
   ```csharp
   // BUG: Accessing child in _EnterTree
   public override void _EnterTree()
   {
       var child = GetNode<Node>("Child"); // ❌ Child not ready!
   }
   
   // FIX: Use _Ready for child access
   public override void _Ready()
   {
       var child = GetNode<Node>("Child"); // ✅ Child exists
   }
   
   // BUG: Not checking node validity
   private Node? _cached;
   private void UseNode()
   {
       _cached.Position = Vector2.Zero; // ❌ May be disposed!
   }
   
   // FIX: Always validate Godot objects
   private void UseNode()
   {
       if (IsInstanceValid(_cached))
           _cached.Position = Vector2.Zero; // ✅ Safe
   }
   ```

3. **Scene Change Debugging**
   ```csharp
   // Check disposal order during scene change
   public override void _ExitTree()
   {
       GD.Print($"[{Name}] Exiting tree");
       // Unsubscribe from ALL events here
       SomeBridge.Event -= OnEvent;
       base._ExitTree();
   }
   
   protected override void Dispose(bool disposing)
   {
       GD.Print($"[{Name}] Disposing: {disposing}");
       if (disposing)
       {
           // Clean up C# resources
           _subscription?.Dispose();
       }
       base.Dispose(disposing);
   }
   ```

4. **Autoload/Singleton Issues**
   ```csharp
   // BUG: Autoload holds reference to freed nodes
   public partial class GameManager : Node
   {
       private List<Node> _nodes = new(); // ❌ Keeps freed nodes!
   }
   
   // FIX: Use weak references or clean up
   public partial class GameManager : Node
   {
       private List<WeakReference> _nodes = new();
       
       private void CleanupFreedNodes()
       {
           _nodes.RemoveAll(wr => !IsInstanceValid(wr.Target as Node));
       }
   }
   ```

**Output**: Lifecycle violation identified with proper fix

---

### 3. Debug Godot Signal Issues

**Trigger**: "Signal not working" or "Events not firing in Godot"

**Steps**:

1. **Verify Signal Flow**
   ```gdscript
   # Check signal is declared
   [Signal]
   public delegate void MySignalEventHandler(int value);
   
   # Check signal is emitted
   EmitSignal(SignalName.MySignal, 42);
   
   # Check connection exists
   node.Connect(SignalName.MySignal, Callable.From<int>(OnSignal));
   ```

2. **Common Signal Problems**
   ```csharp
   // BUG: Connecting to disposed node
   public override void _Ready()
   {
       var temp = new Node();
       temp.Connect("signal", Callable.From(OnSignal));
       temp.QueueFree(); // ❌ Connection lost!
   }
   
   // BUG: Not disconnecting on disposal
   public override void _ExitTree()
   {
       // ❌ Forgot to disconnect!
   }
   
   // FIX: Proper signal lifecycle
   private Node? _signalSource;
   
   public override void _Ready()
   {
       _signalSource = GetNode<Node>("Source");
       _signalSource.Connect("signal", Callable.From(OnSignal));
   }
   
   public override void _ExitTree()
   {
       if (IsInstanceValid(_signalSource))
           _signalSource.Disconnect("signal", Callable.From(OnSignal));
   }
   ```

3. **Bridge Pattern Signal Issues**
   ```csharp
   // BUG: Static event with Godot nodes
   public static event Action<Node>? NodeEvent; // ❌ Keeps nodes alive!
   
   // FIX: Use data instead of nodes
   public static event Action<NodeData>? DataEvent; // ✅ No node refs
   
   // Or use weak events
   private static readonly WeakEvent<NodeEventArgs> _nodeEvent = new();
   ```

**Output**: Signal connection issue resolved

---

### 4. Debug Notification Pipeline

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

### 5. Debug Godot Thread Safety Issues

**Trigger**: "Can't change state while flushing" or "Random crashes" or "Works in editor, not in build"

**Steps**:

1. **Understand Godot Threading Model**
   ```
   Main Thread (Godot):
   - All node operations
   - Scene tree modifications
   - Physics processing
   - Rendering
   
   Worker Threads (C#):
   - Async/await continuations
   - Task.Run operations
   - Background services
   
   RULE: Never touch Godot objects from worker threads!
   ```

2. **Identify Thread Violations**
   ```csharp
   // BUG: Modifying node from async continuation
   public async Task LoadDataAsync()
   {
       var data = await FetchDataAsync();
       _label.Text = data; // ❌ Not on main thread!
   }
   
   // FIX: Use CallDeferred for thread safety
   public async Task LoadDataAsync()
   {
       var data = await FetchDataAsync();
       CallDeferred(nameof(UpdateLabel), data); // ✅ Runs on main thread
   }
   
   private void UpdateLabel(string data)
   {
       _label.Text = data;
   }
   ```

3. **Common Godot Thread Bugs**
   ```csharp
   // BUG: Accessing scene tree from Task
   Task.Run(() =>
   {
       var node = GetNode<Node>("Path"); // ❌ Crash!
   });
   
   // BUG: Signal from wrong thread
   Task.Run(() =>
   {
       EmitSignal("SomeSignal"); // ❌ Thread violation!
   });
   
   // FIX: Proper async pattern with Godot
   public async Task ProcessAsync()
   {
       // Do async work
       var result = await ComputeAsync();
       
       // Return to main thread for Godot operations
       await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
       
       // Now safe to touch nodes
       UpdateNodeWithResult(result); // ✅ Main thread
   }
   ```

4. **MVP Thread Safety Pattern**
   ```csharp
   // Presenter handles threading boundary
   public class BlockPresenter : BasePresenter
   {
       public async Task HandleCommandAsync()
       {
           // Domain work on any thread
           var result = await _mediator.Send(command);
           
           // Marshal to main thread for view update
           if (!OS.IsMainThread())
           {
               CallDeferred(nameof(UpdateView), result);
           }
           else
           {
               UpdateView(result);
           }
       }
       
       private void UpdateView(Result result)
       {
           // Safe to update Godot nodes here
           _view.UpdateDisplay(result);
       }
   }
   ```

5. **Debug Thread Issues**
   ```csharp
   // Add thread diagnostics
   GD.Print($"Thread: {Thread.CurrentThread.ManagedThreadId}");
   GD.Print($"Main: {OS.IsMainThread()}");
   
   // Use thread-safe assertions
   Debug.Assert(OS.IsMainThread(), "Must be on main thread!");
   
   // Add defensive checks
   if (!OS.IsMainThread())
   {
       GD.PrintErr("Thread violation detected!");
       CallDeferred(nameof(SafeMethod));
       return;
   }
   ```

**Output**: Thread safety violation fixed with proper marshalling

---

### 6. Debug Clean Architecture Violations in Godot

**Trigger**: "Godot dependency in domain" or "Architecture test failing" or "Boundary violation"

**Steps**:

1. **Identify Layer Violations**
   ```
   Domain Layer (Pure C#):
   ❌ NEVER: using Godot;
   ❌ NEVER: Node, Vector2, Transform2D
   ✅ ONLY: System types, LanguageExt, domain types
   
   Application Layer (Commands/Queries):
   ❌ NEVER: Godot types in commands/queries
   ✅ BRIDGE: Notification handlers can reference bridges
   
   Presentation Layer (Godot-aware):
   ✅ CAN: Use all Godot types
   ✅ MUST: Convert between domain ↔ Godot types
   ```

2. **Common Architecture Bugs**
   ```csharp
   // BUG: Godot type in domain
   public record Block(Vector2 Position); // ❌ Godot in domain!
   
   // FIX: Use domain type
   public record Block(Position Position); // ✅ Domain type
   public record Position(float X, float Y); // ✅ Pure C#
   
   // BUG: Domain service using Godot
   public class BlockService
   {
       public void Move(Node node) // ❌ Node in domain!
       {
           node.Position = Vector2.Zero;
       }
   }
   
   // FIX: Domain service uses domain types
   public class BlockService
   {
       public Block Move(Block block, Position newPos) // ✅ Pure domain
       {
           return block with { Position = newPos };
       }
   }
   ```

3. **Debug Presenter Boundary**
   ```csharp
   // Presenter is the boundary guardian
   public class BlockPresenter : BasePresenter
   {
       // ✅ CORRECT: Convert at boundary
       private void OnBlockMoved(BlockMovedData data)
       {
           // Domain → Godot conversion
           var godotPos = new Vector2(data.Position.X, data.Position.Y);
           _view.MoveBlock(godotPos);
       }
       
       // ❌ WRONG: Leaking Godot to domain
       private async Task MoveBlock(Vector2 position)
       {
           await _mediator.Send(new MoveCommand(position)); // ❌ Godot in command!
       }
       
       // ✅ CORRECT: Convert before sending
       private async Task MoveBlock(Vector2 position)
       {
           var domainPos = new Position(position.X, position.Y);
           await _mediator.Send(new MoveCommand(domainPos)); // ✅ Domain type
       }
   }
   ```

4. **Verify with Architecture Tests**
   ```csharp
   // Run architecture fitness tests
   dotnet test --filter "FullyQualifiedName~ArchitectureFitnessTests"
   
   // Common failures and fixes:
   "Domain should not reference Godot"
   → Remove all using Godot from domain
   
   "Commands should not contain Godot types"
   → Convert to domain types before command
   
   "Handlers should not reference presentation"
   → Use notification + bridge pattern
   ```

**Output**: Architecture violation fixed with proper boundaries

---

### 7. Debug State Synchronization

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

## Godot Debugging Patterns Reference

### Godot-Specific Debugging Approach
```
1. CHECK LIFECYCLE
   - Is node in tree?
   - Is node ready?
   - Is object valid?
   
2. CHECK THREADING
   - Main thread for Godot ops?
   - CallDeferred for cross-thread?
   - Async marshalling correct?
   
3. CHECK SIGNALS
   - Signal connected?
   - Node still valid?
   - Proper disconnection?
   
4. CHECK ARCHITECTURE
   - Domain pure C#?
   - Conversions at boundary?
   - No Godot leakage?
   
5. PREVENT
   - Add Godot-aware test
   - Document pattern
```

### Common Godot Fix Patterns

**Node Lifecycle Fix**:
```csharp
// Before: Unsafe access
var child = GetNode("Child");

// After: Validated access
if (HasNode("Child") && IsInstanceValid(GetNode("Child")))
    var child = GetNode("Child");
```

**Thread Safety Fix**:
```csharp
// Before: Direct modification
_label.Text = asyncResult;

// After: Thread-safe
CallDeferred(nameof(UpdateLabel), asyncResult);
```

**Signal Memory Fix**:
```csharp
// Before: Strong reference to nodes
public static event Action<Node> NodeEvent;

// After: Data only
public static event Action<NodeData> DataEvent;
```

**Architecture Boundary Fix**:
```csharp
// Before: Godot in domain
public record Block(Vector2 Position);

// After: Pure domain
public record Block(Position Position);
public record Position(float X, float Y);
```

### Godot Debugging Tools

**Remote Debugger**:
- Enable in Project Settings
- Connect to running game
- Inspect live node tree
- Monitor performance

**Scene Dock Inspector**:
- View node properties
- Check signal connections
- Verify node paths
- Monitor lifecycle

**Godot Profiler**:
- Frame time analysis
- Script performance
- Memory usage
- Thread activity

**C# Debugging in Godot**:
```csharp
// Use GD.Print for Godot console
GD.Print($"Debug: {value}");
GD.PrintErr($"Error: {error}");

// Check thread context
if (!OS.IsMainThread())
    GD.PrintErr("Not on main thread!");

// Validate objects
if (!IsInstanceValid(node))
    GD.PrintErr("Invalid node reference!");
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

### When Godot bug diagnosed:
"🔍 Bug Diagnosed: [Issue Name]

ROOT CAUSE: [e.g., "Presenter accessing node after _ExitTree()"]
GODOT CONTEXT: [e.g., "Happens during scene transition"]
THREAD: [Main/Worker thread issue if relevant]

REPRODUCTION:
1. [e.g., "Load scene with presenter"]
2. [e.g., "Trigger scene change while async operation pending"]
3. [e.g., "Observe null reference when callback fires"]

FIX:
```csharp
// Add IsInstanceValid check before node access
if (IsInstanceValid(_view))
    _view.UpdateDisplay(data);
```

Location: [File:Line]
Lifecycle Stage: [_Ready/_EnterTree/_ExitTree/Runtime]
Confidence: [High/Medium/Low]

Will add test to verify proper disposal handling."

### When needing Godot-specific info:
"🔍 Need Godot-specific information:

To diagnose this Godot issue, please provide:
1. Full Godot console output (errors and warnings)
2. Scene structure (which nodes, their types, hierarchy)
3. When it occurs (editor, runtime, build, scene change?)
4. Thread ID from error (if async related)

Also helpful:
- Does it work with CallDeferred?
- Does it work in _Ready vs _EnterTree?
- Any autoloads involved?

This will help determine if it's a [lifecycle/thread/signal] issue."

### When providing Godot fix:
"✅ Fix for Godot/C# Issue:

```csharp
// PROBLEM: Accessing Godot node from async continuation
public async Task LoadAsync()
{
    var data = await FetchDataAsync();
    _label.Text = data; // ❌ Not on main thread!
}

// SOLUTION: Marshal to main thread
public async Task LoadAsync()
{
    var data = await FetchDataAsync();
    CallDeferred(nameof(UpdateLabel), data); // ✅ Thread-safe
}

private void UpdateLabel(string data)
{
    if (IsInstanceValid(_label))
        _label.Text = data;
}
```

This ensures Godot operations happen on the main thread.
Added IsInstanceValid check for disposal race conditions."