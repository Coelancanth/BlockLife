# Architecture FAQ

## Common Issues and Solutions

This document addresses frequently encountered architectural questions and issues in the BlockLife project.

---

## Dependency Injection & Service Registration

### Q: Why can't I resolve my Presenter from the DI container?
**A:** Presenters should NOT be registered in the DI container directly. They require view dependencies that only exist at runtime. Use the `PresenterFactory` to create presenters with their view dependencies.

**Correct approach:**
```csharp
// DON'T do this:
services.AddTransient<GridPresenter>();

// DO this instead:
var presenter = presenterFactory.CreatePresenter<GridPresenter>(view);
```

### Q: Why is MediatR not finding my handler?
**A:** MediatR automatically registers handlers that implement `IRequestHandler<,>` or `INotificationHandler<>`. Ensure:
1. Your handler is in an assembly scanned by MediatR
2. Your handler has a public parameterless constructor or all dependencies are registered
3. You're not accidentally implementing these interfaces on types that shouldn't be handlers (like Presenters)

### Q: Should I register validation rules in the DI container?
**A:** Yes, validation rules should be registered as Transient services since they may have dependencies on other services like `IGridStateService`.

---

## Error Handling

### Q: Should I use Error.New() with a code and message or just a message?
**A:** Use single-parameter format with just the message for consistency:

```csharp
// DON'T do this:
return Error.New("INVALID_POSITION", $"Position {position} is invalid");

// DO this:
return Error.New($"Position {position} is invalid");
```

### Q: How should I handle validation errors in command handlers?
**A:** Always validate BEFORE executing operations:

```csharp
public async Task<Fin<Result>> Handle(Command command, CancellationToken ct)
{
    // 1. Validate first
    var validationResult = _validationRule.Validate(command);
    if (validationResult.IsFail)
        return validationResult.Match(
            Succ: _ => Fin<Result>.Fail(Error.New("Unexpected")),
            Fail: error => Fin<Result>.Fail(error)
        );
    
    // 2. Then execute
    return await ExecuteOperation();
}
```

---

## LanguageExt Integration

### Q: Should I create wrapper types around LanguageExt types?
**A:** No. Use LanguageExt types directly:

```csharp
// DON'T create wrapper:
public class Fin<T> : LanguageExt.Fin<T> { }

// DO use directly:
public interface ICommand<T> : IRequest<LanguageExt.Fin<T>> { }
```

### Q: When should I use Option<T> vs nullable references?
**A:** Always prefer `Option<T>` for domain logic:
- Use `Option<T>` for all domain model returns
- Use `Option<T>` when absence of value is a valid business case
- Only use nullable references for framework integration points

---

## Clean Architecture Boundaries

### Q: Can Core project reference Godot types?
**A:** NEVER. Core must remain pure C# with no Godot dependencies:

```csharp
// In Core project - DON'T do this:
using Godot; // ❌ NEVER in Core

// DO this instead:
using BlockLife.Core.Domain.Common; // ✓ Use domain types
public record Vector2Int(int X, int Y); // ✓ Create your own types
```

### Q: Where do Presenters belong?
**A:** Presenters belong in Core but must not have Godot dependencies:
- Define presenter classes in `Core/Features/[Feature]/Presenters/`
- Define view interfaces (IGridView) in Core
- Implement view interfaces in the Godot project

### Q: How do I handle Godot-specific logic?
**A:** Use the View pattern:
1. Define capability interfaces in Core (e.g., `IGridView`)
2. Implement these interfaces in Godot project with actual Godot nodes
3. Presenters coordinate through interfaces, never touching Godot types

---

## Testing

### Q: Why are my tests failing with "service not registered"?
**A:** Check if:
1. You're trying to resolve MediatR handlers directly (they're registered by interface)
2. You're trying to resolve presenters (use PresenterFactory instead)
3. Your test is creating the service provider with validation enabled

### Q: How do I test command handlers?
**A:** Follow the Three Pillars pattern:
1. Mock all dependencies except the handler
2. Test both success and failure paths
3. Verify side effects (notifications, logging)

```csharp
[Fact]
public async Task Handle_WhenValid_ReturnsSuccess()
{
    // Arrange
    var command = new PlaceBlockCommand(...);
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSucc.Should().BeTrue();
    _mockMediator.Verify(m => m.Publish(
        It.IsAny<BlockPlacedNotification>(), 
        It.IsAny<CancellationToken>()), 
        Times.Once);
}
```

---

## Common Patterns

### Q: How do I implement a new vertical slice?
**A:** Follow this structure:
```
src/Features/[Domain]/[Feature]/
├── Commands/
│   ├── [Feature]Command.cs
│   └── [Feature]CommandHandler.cs
├── Queries/
│   ├── [Feature]Query.cs
│   └── [Feature]QueryHandler.cs
├── Rules/
│   └── [Feature]ValidationRule.cs
├── Effects/
│   └── [Feature]Notification.cs
└── Presenters/
    ├── I[Feature]View.cs
    └── [Feature]Presenter.cs
```

### Q: When should I publish notifications?
**A:** Publish notifications:
- AFTER successful state changes
- NEVER on validation failures
- For cross-cutting concerns (other features need to react)

### Q: How do I handle concurrent operations?
**A:** Use thread-safe collections and atomic operations:
```csharp
// Use ConcurrentDictionary for shared state
private readonly ConcurrentDictionary<Vector2Int, Block> _blocks = new();

// Ensure atomic operations
if (!_blocks.TryAdd(position, block))
    return Error.New("Concurrent modification detected");
```

---

## Performance Considerations

### Q: Should all services be singletons for performance?
**A:** No, follow these guidelines:
- **Singleton**: Stateful services (GridStateService), expensive resources
- **Transient**: Handlers, validation rules, stateless services
- **Scoped**: Per-request services (rarely used in game context)

### Q: How do I avoid memory leaks with events?
**A:** Use MediatR notifications instead of C# events:
- MediatR handles subscription lifecycle
- Weak references prevent leaks
- Async handling is built-in

---

## Debugging Tips

### Q: How do I trace command execution?
**A:** The LoggingBehavior automatically logs all commands:
1. Check the console/log file for command execution
2. Look for "Handling command" and "Command handled" messages
3. Failures are logged with full error details

### Q: Why is my validation not being called?
**A:** Ensure:
1. Validation rule is called explicitly in your handler
2. Validation rule is registered in DI if it has dependencies
3. You're not accidentally bypassing validation on certain code paths

---

## Migration & Refactoring

### Q: How do I migrate from direct state manipulation to CQRS?
**A:** Follow these steps:
1. Identify the state change
2. Create a Command representing the intent
3. Move logic to CommandHandler
4. Replace direct calls with MediatR.Send()
5. Add validation rules
6. Publish notifications for side effects

### Q: How do I refactor a God class?
**A:** Apply vertical slice architecture:
1. Group related functionality
2. Extract to feature folders
3. Create focused handlers for each operation
4. Use validation rules for business logic
5. Keep presenters humble (coordination only)

---

*This FAQ is a living document. Add new Q&As as architectural patterns emerge and issues are resolved.*