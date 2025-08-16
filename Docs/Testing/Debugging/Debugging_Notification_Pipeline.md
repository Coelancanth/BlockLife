# Debugging Notification Pipeline Issues

## Overview
This guide provides systematic approaches to debugging notification pipeline issues, based on lessons learned from the Block Placement Display Bug (BPM-005).

## Common Symptoms
- ✅ Commands execute successfully (no errors logged)
- ✅ Business logic completes (data is stored/updated)  
- ❌ View doesn't update to reflect changes
- ❌ Users don't see expected visual feedback

## Debugging Methodology

### Phase 1: Command Handler Verification

**Check that notifications are being published:**

```csharp
// Add temporary tracing in command handler
_logger.LogInformation("📢 Publishing {NotificationType}", nameof(MyNotification));
await _mediator.Publish(notification);
_logger.LogInformation("✅ Notification published successfully");
```

**Look for these patterns:**
- ✅ **Good**: `await _mediator.Publish(notification)`
- ❌ **Bad**: Effect queued but never processed
- ❌ **Bad**: Notification created but not published

### Phase 2: Handler Registration Verification

**Check MediatR registration in GameStrapper:**

```bash
# Search for handler registration
dotnet build 2>&1 | grep "INotificationHandler"
```

**Verify handler is discoverable:**
- Handler must be in Core assembly
- Handler must implement `INotificationHandler<TNotification>`
- Handler must be registered in DI (automatic via MediatR assembly scanning)

### Phase 3: Handler Execution Verification

**Add tracing to notification handlers:**

```csharp
public async Task Handle(MyNotification notification, CancellationToken cancellationToken)
{
    _logger.LogWarning("🔔 Handler received notification: {NotificationData}", notification);
    
    if (MyStaticEvent != null)
    {
        _logger.LogInformation("📞 Calling {Count} subscribers", 
            MyStaticEvent.GetInvocationList().Length);
        await MyStaticEvent(notification);
    }
    else
    {
        _logger.LogWarning("⚠️ No subscribers - view won't update!");
    }
}
```

### Phase 4: Presenter Subscription Verification

**Check presenter initialization:**

```csharp
public override void Initialize()
{
    // Verify subscription happens
    MyNotificationHandler.MyStaticEvent += OnMyNotification;
    _logger.LogInformation("📞 Subscribed to notification events");
    
    base.Initialize();
}

private async Task OnMyNotification(MyNotification notification)
{
    _logger.LogInformation("🎯 Presenter received notification");
    
    try
    {
        // Your view update logic
        await UpdateView(notification);
        _logger.LogInformation("✅ View updated successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ Failed to update view");
    }
}
```

### Phase 5: View Update Verification

**Trace view interface calls:**

```csharp
private async Task UpdateView(MyNotification notification)
{
    _logger.LogInformation("🎨 Calling View.UpdateSomething()");
    await View.UpdateSomething(notification.Data);
    _logger.LogInformation("🎨 View.UpdateSomething() completed");
}
```

## Diagnostic Commands

### Quick Pipeline Test
```csharp
// Add to any method for testing
await _mediator.Publish(new TestNotification("Debug test"));
```

### Handler Discovery Check
```bash
# Find all notification handlers
grep -r "INotificationHandler" src/ --include="*.cs"
```

### Event Subscription Audit
```csharp
// Check static event subscribers
var delegates = MyNotificationHandler.MyStaticEvent?.GetInvocationList();
_logger.LogInformation("Event has {Count} subscribers: {Subscribers}", 
    delegates?.Length ?? 0, 
    delegates?.Select(d => d.Method.DeclaringType?.Name));
```

## Common Issues & Solutions

### Issue: "Empty Handler" Pattern
```csharp
// ❌ BAD: Handler does nothing
public Task Handle(MyNotification notification, CancellationToken cancellationToken)
{
    _logger.LogTrace("Notification received"); // Just logs, no action
    return Task.CompletedTask;
}

// ✅ GOOD: Handler bridges to presenter
public async Task Handle(MyNotification notification, CancellationToken cancellationToken)
{
    if (MyStaticEvent != null)
        await MyStaticEvent(notification);
}
```

### Issue: Inconsistent Notification Patterns
```csharp
// ❌ BAD: Some handlers use effects, others direct publish
// Handler A:
_simulation.QueueEffect(new MyEffect()); // Never processed!

// Handler B: 
await _mediator.Publish(new MyNotification()); // Works

// ✅ GOOD: All handlers use consistent pattern
await _mediator.Publish(new MyNotification());
```

### Issue: Missing Presenter Subscription
```csharp
// ❌ BAD: Presenter has handler method but never subscribes
public class MyPresenter : PresenterBase<IMyView>
{
    public async Task HandleMyNotification(MyNotification notification) { } // Never called!
}

// ✅ GOOD: Presenter subscribes to events
public override void Initialize()
{
    MyNotificationHandler.MyStaticEvent += HandleMyNotification;
}
```

## Prevention Strategies

### 1. Architectural Tests
Add tests to verify notification patterns:

```csharp
[Fact]
public void AllCommandHandlers_ShouldPublishNotifications_WhenSuccessful()
{
    var handlerTypes = GetAllCommandHandlerTypes();
    
    foreach (var handlerType in handlerTypes)
    {
        // Verify handler publishes notifications via _mediator.Publish()
        // Not via effect queues without processing
    }
}
```

### 2. Code Review Checklist
- [ ] Command handler publishes notification via `_mediator.Publish()`
- [ ] Notification handler bridges to presenter via static events
- [ ] Presenter subscribes to events in `Initialize()`
- [ ] Presenter unsubscribes in `Dispose()`

### 3. Integration Test Pattern
```csharp
[Fact]
public async Task PlaceBlockCommand_ShouldUpdateView_WhenSuccessful()
{
    // Arrange: Command and expected view change
    var command = new PlaceBlockCommand(position, blockType);
    var viewUpdate = false;
    
    // Subscribe to the end of pipeline
    MyNotificationHandler.MyStaticEvent += _ => { viewUpdate = true; return Task.CompletedTask; };
    
    // Act: Execute command
    await _mediator.Send(command);
    
    // Assert: View should be updated
    viewUpdate.Should().BeTrue("because the full pipeline should execute");
}
```

## Reference Implementation

See `BlockPlacementNotificationBridge` and `BlockManagementPresenter` for the reference implementation of the static event bridge pattern between MediatR notifications and presenter methods.

---

**Related Documents:**
- [Bug Post-Mortem BPM-005: Block Placement Display Bug](../4_Post_Mortems/Block_Placement_Display_Bug_Report.md)
- [Architecture FAQ: Notification Pipeline](../1_Architecture/Architecture_FAQ.md#notification-pipeline--event-handling)
- [Comprehensive Development Workflow](Essential_Development_Workflow.md)