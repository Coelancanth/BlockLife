# Standard Architectural Patterns

## Overview
This document formalizes architectural patterns that have been validated through implementation and proven to be effective solutions to specific constraints in the BlockLife codebase. These patterns are **not workarounds or hacks** - they are legitimate architectural solutions.

## 1. Static Event Bridge Pattern 🌉

### Purpose
Connect MediatR notifications to presenters while respecting Clean Architecture boundaries and DI constraints.

### When to Use
- When presenters need to react to domain events
- When views need updates based on command/query results
- When bridging between infrastructure (DI-managed) and presentation (Godot-managed) layers

### Implementation Pattern

```csharp
/// <summary>
/// Standard notification bridge connecting MediatR to presenters.
/// This pattern is required because presenters cannot implement INotificationHandler
/// due to their dependency on Godot-managed views.
/// </summary>
public class MyFeatureNotificationBridge : 
    INotificationHandler<MyDomainNotification>
{
    private readonly ILogger<MyFeatureNotificationBridge> _logger;
    
    // Static event that presenters subscribe to
    public static event Func<MyDomainNotification, Task>? MyEventOccurred;
    
    public MyFeatureNotificationBridge(ILogger<MyFeatureNotificationBridge> logger)
    {
        _logger = logger;
    }
    
    public async Task Handle(MyDomainNotification notification, CancellationToken cancellationToken)
    {
        if (MyEventOccurred != null)
        {
            await MyEventOccurred(notification);
        }
        else
        {
            _logger.LogWarning("No subscribers for {NotificationType} - views will not update", 
                typeof(MyDomainNotification).Name);
        }
    }
}
```

### Presenter Subscription Pattern

```csharp
public class MyPresenter : PresenterBase<IMyView>
{
    public override void Initialize()
    {
        // Subscribe to bridge events
        MyFeatureNotificationBridge.MyEventOccurred += OnMyEventOccurred;
        
        base.Initialize();
    }
    
    public override void Dispose()
    {
        // Always unsubscribe to prevent memory leaks
        MyFeatureNotificationBridge.MyEventOccurred -= OnMyEventOccurred;
        
        base.Dispose();
    }
    
    private async Task OnMyEventOccurred(MyDomainNotification notification)
    {
        try
        {
            await View.UpdateSomething(notification.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update view for {NotificationType}", 
                typeof(MyDomainNotification).Name);
        }
    }
}
```

### Why This Pattern is Valid

1. **Architectural Constraint**: Presenters require Godot-managed View dependencies that cannot be provided by DI
2. **Clean Boundaries**: Maintains separation between infrastructure (MediatR) and presentation (Views)
3. **Explicit Design**: Architecture Guide explicitly prohibits presenters from implementing INotificationHandler
4. **Testable**: Static events can be tested by subscribing to them in tests
5. **Debuggable**: Clear event subscription/unsubscription lifecycle

## 2. Functional Error Handling Pattern 🚂
*Railway-Oriented Programming with TaskFinExtensions*

### Purpose
Provide consistent functional error handling across all async operations, eliminating imperative try-catch blocks in favor of railway-oriented programming.

### Problem Solved
Mixed imperative/functional patterns created inconsistent error handling and broke functional composition chains:

```csharp
// ❌ PROBLEMATIC: Breaking functional chain with try-catch
public async Task<Fin<Unit>> Handle(PlaceBlockCommand request, CancellationToken cancellationToken)
{
    // ... pure functional Fin<T> chain ...
    
    // Forced to break functional chain with try-catch
    try
    {
        await _simulation.ProcessQueuedEffectsAsync(); // Returns Task, not Fin<T>
        return FinSucc(Unit.Default);
    }
    catch (Exception ex)
    {
        return FinFail<Unit>(Error.New("PROCESS_EFFECTS_FAILED", ex.Message));
    }
}
```

### Implementation Pattern

**TaskFinExtensions** (`src/Core/Infrastructure/Extensions/TaskFinExtensions.cs`):

```csharp
public static class TaskFinExtensions
{
    // Basic conversion with automatic error handling
    public static async Task<Fin<T>> ToFin<T>(this Task<T> task)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            return FinSucc(result);
        }
        catch (Exception ex)
        {
            return FinFail<T>(Error.New("TASK_EXECUTION_FAILED", ex.Message));
        }
    }

    // With custom error code and message
    public static async Task<Fin<T>> ToFin<T>(this Task<T> task, string errorCode, string errorMessage)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            return FinSucc(result);
        }
        catch (Exception ex)
        {
            return FinFail<T>(Error.New(errorCode, $"{errorMessage}: {ex.Message}"));
        }
    }

    // For Task (no return value) → Fin<Unit>
    public static async Task<Fin<Unit>> ToFin(this Task task, string errorCode, string errorMessage)
    {
        try
        {
            await task.ConfigureAwait(false);
            return FinSucc(Unit.Default);
        }
        catch (Exception ex)
        {
            return FinFail<Unit>(Error.New(errorCode, $"{errorMessage}: {ex.Message}"));
        }
    }
}
```

**Clean Command Handler Pattern:**

```csharp
// ✅ SOLUTION: Clean railway-oriented programming
public async Task<Fin<Unit>> Handle(PlaceBlockCommand request, CancellationToken cancellationToken)
{
    return await result.Match(
        Succ: async block =>
        {
            var notification = new BlockPlacedNotification(request.Position, request.BlockType);
            
            // Clean functional composition - no try-catch needed!
            return await _mediator.Publish(notification, cancellationToken)
                .ToFin("NOTIFICATION_FAILED", "Failed to publish block placed notification")
                .Map(_ => Unit.Default);
        },
        Fail: error => Task.FromResult(FinFail<Unit>(error))
    );
}
```

### Usage Examples

```csharp
// Infrastructure service calls
var result = await _simulation.ProcessQueuedEffectsAsync()
    .ToFin("PROCESS_EFFECTS_FAILED", "Failed to process queued effects");

// MediatR publishing
var publishResult = await _mediator.Publish(notification, cancellationToken)
    .ToFin("PUBLISH_FAILED", "Failed to publish notification");

// Generic Task<T> conversion
var dataResult = await _repository.GetDataAsync()
    .ToFin("DATA_RETRIEVAL_FAILED", "Failed to retrieve data");

// Chaining functional operations
return await GetBlockData()
    .Bind(block => ValidateBlock(block))
    .Bind(validBlock => _service.ProcessBlockAsync(validBlock)
        .ToFin("PROCESSING_FAILED", "Block processing failed"));
```

### Architecture Enforcement

**Architecture Test Prevents Regression:**

```csharp
[Fact]
public void CommandHandlers_ShouldNotContain_TryCatchBlocks()
{
    // ADR-006: Fin<T> vs Task<T> Consistency - Phase 1 Implementation
    // Command handlers should use functional error handling with Fin<T> extensions
    
    var handlerTypes = GetAllCommandHandlerTypes();
    
    foreach (var handlerType in handlerTypes)
    {
        var sourceCode = GetSourceCode(handlerType);
        
        sourceCode.Should().NotContain("try {")
            .And.NotContain("catch (")
            .Because($"handler {handlerType.Name} should use functional error handling with TaskFinExtensions");
    }
}
```

### Benefits

1. **Consistent Error Handling**: All async operations follow same pattern
2. **Composable Pipelines**: Clean functional composition throughout
3. **Better Error Information**: Structured errors with codes and context  
4. **Testability**: Pure functions easier to test and reason about
5. **Maintainability**: Railway-oriented programming reduces cognitive load

### When to Use

- **Always** in command handlers for async operations
- When calling infrastructure services that return `Task<T>`
- When publishing notifications through MediatR
- For any async operation that could throw exceptions

### Why This Pattern is Valid

1. **ADR-006 Decision**: Formally adopted to solve functional/imperative inconsistency
2. **Architecture Enforcement**: Tests prevent regression to imperative patterns
3. **Battle-Tested**: All command handlers successfully converted
4. **Performance**: No overhead - exceptions still bubble up as errors
5. **Composable**: Maintains functional programming benefits

## 3. Direct Notification Publishing Pattern 📢

### Purpose
Publish domain notifications directly from command handlers without intermediate effect queuing.

### When to Use
- For immediate view updates after command execution
- When effect orchestration complexity is not needed
- When debugging requires simple, traceable notification flow

### Implementation Pattern

```csharp
public async Task<Fin<Unit>> Handle(MyCommand request, CancellationToken cancellationToken)
{
    // Business logic execution
    var result = await ExecuteBusinessLogic(request);
    
    if (result.IsSucc)
    {
        var data = result.Match(Succ: d => d, Fail: _ => throw new InvalidOperationException());
        
        // Direct notification publishing
        var notification = new MyDomainNotification(data.Id, data.Status);
        await _mediator.Publish(notification, cancellationToken);
        
        return FinSucc(Unit.Default);
    }
    else
    {
        var error = result.Match(Succ: _ => throw new InvalidOperationException(), Fail: e => e);
        return FinFail<Unit>(error);
    }
}
```

### Versus Effect Queue Pattern

**Direct Publishing (Current Choice):**
- ✅ Simple and direct
- ✅ Immediate notifications  
- ✅ Easy debugging
- ✅ No frame delays

**Effect Queue (Available Alternative):**
- ✅ Deferred processing
- ✅ Batch operations
- ✅ Complex orchestration
- ❌ Additional complexity for simple cases

### Why This Pattern is Valid

1. **YAGNI Principle**: Complex effect orchestration not needed for current requirements
2. **Architectural Choice**: Both patterns are available; team chose simpler solution
3. **Clear Trade-off**: Immediate feedback prioritized over deferred processing
4. **Infrastructure Available**: Effect queue exists for future use if needed

## 4. Controlled MVP Pattern 🎯

### Purpose
Allow presenters to be Godot-aware while maintaining business logic purity.

### Key Principle
**Presenters can reference Godot types; Models cannot.**

### Implementation Pattern

```csharp
// ✅ VALID: Presenter coordinates between pure Model and Godot View
public class GridPresenter : PresenterBase<IGridView>
{
    private readonly IGridStateService _gridState; // Pure C# service
    private readonly IMediator _mediator;          // Pure C# infrastructure
    
    public async Task OnGridCellClicked(Vector2Int position)
    {
        // Pure business logic through CQRS
        var command = new PlaceBlockCommand { Position = position };
        var result = await _mediator.Send(command);
        
        // View updates through interface (testable)
        await result.Match(
            Succ: _ => View.ShowSuccessFeedback(position),
            Fail: error => View.ShowErrorMessage(error.Message)
        );
    }
}

// ❌ INVALID: Model depends on Godot
public class BlockDomainService
{
    public Fin<Unit> ProcessBlock(Godot.Vector2 position) // ❌ Godot dependency in Model
    {
        // Business logic
    }
}

// ✅ VALID: Model is pure C#
public class BlockDomainService  
{
    public Fin<Unit> ProcessBlock(Domain.Common.Vector2Int position) // ✅ Pure domain type
    {
        // Business logic
    }
}
```

### Why This Pattern is Valid

1. **Pragmatic MVP**: Avoids excessive mapping overhead between domain and view types
2. **Clear Boundary**: Only Models are prohibited from Godot dependencies
3. **Testable**: View interactions happen through interfaces
4. **Maintainable**: Business logic remains pure and portable

## 5. Architectural Validation

### These Patterns Are NOT Hacks Because:

1. **Explicitly Designed**: Architecture constraints led to these specific solutions
2. **Clean Architecture Compliant**: They maintain proper layer boundaries
3. **Well-Documented**: Clear rationale and implementation guidance
4. **Battle-Tested**: Proven through implementation and debugging
5. **Alternative-Aware**: Team chose these over more complex alternatives

### Anti-Patterns to Avoid

1. **Presenters implementing INotificationHandler**: Violates DI constraints
2. **Models depending on Godot types**: Breaks Clean Architecture
3. **Direct View updates from command handlers**: Bypasses MVP pattern
4. **Mixed notification patterns**: Inconsistent event publishing approaches
5. **Try-catch blocks in command handlers**: Breaks functional composition (use TaskFinExtensions)

## 6. Code Templates

### New Feature Notification Bridge

```csharp
public class [Feature]NotificationBridge : 
    INotificationHandler<[Feature]Notification>
{
    private readonly ILogger<[Feature]NotificationBridge> _logger;
    
    public static event Func<[Feature]Notification, Task>? [Feature]EventOccurred;
    
    public [Feature]NotificationBridge(ILogger<[Feature]NotificationBridge> logger)
    {
        _logger = logger;
    }
    
    public async Task Handle([Feature]Notification notification, CancellationToken cancellationToken)
    {
        if ([Feature]EventOccurred != null)
        {
            await [Feature]EventOccurred(notification);
        }
        else
        {
            _logger.LogWarning("No subscribers for [Feature] notifications");
        }
    }
}
```

### Command Handler Template

```csharp
public async Task<Fin<Unit>> Handle([Feature]Command request, CancellationToken cancellationToken)
{
    _logger.LogDebug("Handling [Feature]Command");
    
    var result = await ExecuteBusinessLogic(request);
    
    if (result.IsSucc)
    {
        var data = result.Match(Succ: d => d, Fail: _ => throw new InvalidOperationException());
        
        var notification = new [Feature]Notification(data);
        await _mediator.Publish(notification, cancellationToken);
        
        _logger.LogDebug("[Feature]Command completed successfully");
        return FinSucc(Unit.Default);
    }
    else
    {
        var error = result.Match(Succ: _ => throw new InvalidOperationException(), Fail: e => e);
        _logger.LogWarning("[Feature]Command failed: {Error}", error);
        return FinFail<Unit>(error);
    }
}
```

## 7. Architectural Tests

### Validate Pattern Usage

```csharp
[Fact]
public void Presenters_ShouldNotImplement_INotificationHandler()
{
    var presenterTypes = Assembly.GetAssembly(typeof(PresenterBase<>))
        .GetTypes()
        .Where(t => t.BaseType?.IsGenericType == true && 
                   t.BaseType.GetGenericTypeDefinition() == typeof(PresenterBase<>));
    
    foreach (var presenterType in presenterTypes)
    {
        presenterType.GetInterfaces()
            .Should().NotContain(i => i.IsGenericType && 
                                 i.GetGenericTypeDefinition() == typeof(INotificationHandler<>),
                "Presenters must not implement INotificationHandler due to DI constraints");
    }
}

[Fact]  
public void NotificationBridges_ShouldFollow_StandardPattern()
{
    var bridgeTypes = Assembly.GetAssembly(typeof(INotificationHandler<>))
        .GetTypes()
        .Where(t => t.Name.EndsWith("NotificationBridge"));
    
    foreach (var bridgeType in bridgeTypes)
    {
        // Verify static event exists
        var staticEvents = bridgeType.GetEvents(BindingFlags.Static | BindingFlags.Public);
        staticEvents.Should().NotBeEmpty("Notification bridges must expose static events for presenter subscription");
        
        // Verify implements INotificationHandler
        bridgeType.GetInterfaces()
            .Should().Contain(i => i.IsGenericType && 
                             i.GetGenericTypeDefinition() == typeof(INotificationHandler<>),
                "Notification bridges must implement INotificationHandler");
    }
}

[Fact]
public void CommandHandlers_ShouldUse_FunctionalErrorHandling()
{
    var handlerTypes = GetAllCommandHandlerTypes();
    
    foreach (var handlerType in handlerTypes)
    {
        var sourceCode = GetSourceCode(handlerType);
        
        // Should not contain try-catch blocks
        sourceCode.Should().NotContain("try {")
            .And.NotContain("catch (")
            .Because($"handler {handlerType.Name} should use functional error handling with TaskFinExtensions");
        
        // Should use TaskFinExtensions when needed
        if (sourceCode.Contains("await") && sourceCode.Contains("Task"))
        {
            // If handler has async operations, should likely use .ToFin() extensions
            var hasToFinUsage = sourceCode.Contains(".ToFin(");
            var hasTryCatch = sourceCode.Contains("try") || sourceCode.Contains("catch");
            
            if (hasTryCatch)
            {
                Assert.Fail($"Handler {handlerType.Name} uses try-catch instead of functional error handling");
            }
        }
    }
}
```

---

## Conclusion

These patterns represent **mature architectural solutions** that elegantly solve complex integration challenges while maintaining Clean Architecture principles. They should be embraced as standard patterns and used consistently across all features.

The architecture demonstrates sophisticated understanding of the trade-offs involved in maintaining clean boundaries while working with Godot's runtime-managed scene system. These are not workarounds - they are the **correct solutions** for the constraints we face.

---

**Related Documents:**
- [Architecture Guide](Architecture_Guide.md) - Core principles and constraints
- [Bug Post-Mortem BPM-005](../4_Bug_PostMortems/005_Block_Placement_Display_Bug.md) - Pattern validation through debugging
- [ADR-006](../5_Architecture_Decision_Records/ADR-006_Fin_Task_Consistency.md) - Functional error handling decision
- [TaskFinExtensions](../../src/Core/Infrastructure/Extensions/TaskFinExtensions.cs) - Implementation reference
- [Move Block Feature](../../src/Features/Block/Move/) - Gold standard implementation using all patterns