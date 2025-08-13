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

## 2. Direct Notification Publishing Pattern 📢

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

## 3. Controlled MVP Pattern 🎯

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

## 4. Architectural Validation

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

## 5. Code Templates

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

## 6. Architectural Tests

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
```

---

## Conclusion

These patterns represent **mature architectural solutions** that elegantly solve complex integration challenges while maintaining Clean Architecture principles. They should be embraced as standard patterns and used consistently across all features.

The architecture demonstrates sophisticated understanding of the trade-offs involved in maintaining clean boundaries while working with Godot's runtime-managed scene system. These are not workarounds - they are the **correct solutions** for the constraints we face.

---

**Related Documents:**
- [Architecture Guide](Architecture_Guide.md) - Core principles and constraints
- [Bug Post-Mortem BPM-005](../4_Bug_PostMortems/005_Block_Placement_Display_Bug.md) - Pattern validation through debugging
- [ADR-006](../4_Architecture_Decision_Records/ADR-006_Fin_Task_Consistency.md) - Technical debt management