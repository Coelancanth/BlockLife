# Architecture Guide (Practical)

**★ Insight ─────────────────────────────────────**
**Daily Reference**: This practical guide focuses on essential architectural rules and patterns for daily development. For comprehensive theoretical background, see the archived comprehensive guide in `Docs/_archive/comprehensive_guides/`.
**─────────────────────────────────────────────────**

## 🎯 Reference Implementation (COPY THIS)
**Move Block Feature**: `src/Features/Block/Move/` - GOLD STANDARD for all new work
- File structure and naming conventions
- Command/Handler/Service patterns  
- Test organization and coverage
- MVP integration with Godot

## 🚨 Core Rules (Non-Negotiable)

### 1. **Domain Purity** 
```csharp
// ❌ FORBIDDEN in src/ folder
using Godot;

// ✅ REQUIRED in src/ folder  
using LanguageExt;
using MediatR;
```
**Rule**: No Godot dependencies in `src/` - keeps domain testable and portable.

### 2. **CQRS Pattern**
```csharp
// ✅ Commands for state changes
public sealed record PlaceBlockCommand(Vector2Int Position) : IRequest<Fin<Unit>>;

// ✅ Queries for data retrieval  
public sealed record GetBlockQuery(Vector2Int Position) : IRequest<Option<Block>>;
```
**Rule**: Commands change state, Queries read state. Never mix them.

### 3. **Error Handling**
```csharp
// ✅ Use Fin<T> for operations that can fail
public async Task<Fin<Unit>> Handle(PlaceBlockCommand request, CancellationToken ct)
{
    if (request.Position.IsInvalid)
        return Error.New("Invalid position");
        
    var result = await _gridService.PlaceBlock(request.Position);
    return result.Match(
        success => Unit.Default,
        error => error
    );
}
```
**Rule**: Use `Fin<T>` for business logic, never throw exceptions.

### 4. **MVP Pattern**
```csharp
// ✅ Presenter coordinates between Model and View
public class BlockPresenter : PresenterBase<IBlockView>
{
    public async Task HandlePlaceBlock(Vector2Int position)
    {
        var command = new PlaceBlockCommand(position);
        var result = await _mediator.Send(command);
        
        result.Match(
            success => View.UpdateGrid(),
            error => View.ShowError(error.Message)
        );
    }
}
```
**Rule**: Presenters handle coordination, Views handle display, Models handle logic.

### 5. **Dependency Injection**
```csharp
// ✅ Constructor injection - all dependencies explicit
public class PlaceBlockCommandHandler : IRequestHandler<PlaceBlockCommand, Fin<Unit>>
{
    private readonly IGridStateService _gridService;
    private readonly IMediator _mediator;
    
    public PlaceBlockCommandHandler(IGridStateService gridService, IMediator mediator)
    {
        _gridService = gridService;
        _mediator = mediator;
    }
}
```
**Rule**: All dependencies through constructor - no service locator pattern.

## 🏗️ Standard Patterns

### Command Handler Template
```csharp
public class FeatureCommandHandler : IRequestHandler<FeatureCommand, Fin<Unit>>
{
    private readonly IRequiredService _service;
    private readonly IMediator _mediator;
    
    public async Task<Fin<Unit>> Handle(FeatureCommand request, CancellationToken ct)
    {
        // 1. Validation
        var validation = await _validator.Validate(request);
        if (validation.IsFail) return validation.Error;
        
        // 2. Business logic
        var result = await _service.Execute(request);
        if (result.IsFail) return result.Error;
        
        // 3. Notification
        await _mediator.Publish(new FeatureCompletedNotification(result.Value));
        return Unit.Default;
    }
}
```

### View Interface Pattern
```csharp
public interface IFeatureView
{
    void UpdateDisplay(FeatureData data);
    void ShowError(string message);
    void ShowLoading(bool isLoading);
}
```

### Validation Rule Pattern
```csharp
public class FeatureValidationRule : IValidationRule<FeatureData>
{
    public Fin<Unit> Validate(FeatureData data)
    {
        if (data.IsInvalid)
            return Error.New("INVALID_DATA", "Data validation failed");
            
        return Unit.Default;
    }
}
```

## 📂 VSA File Structure

### Feature Organization
```
src/Features/[Domain]/[Feature]/
├── Commands/           # User intentions
│   ├── FeatureCommand.cs
│   └── FeatureCommandHandler.cs
├── Notifications/      # State change events
│   └── FeatureNotification.cs
├── Services/          # Domain operations
│   └── IFeatureService.cs
├── Validation/        # Business rules
│   └── FeatureValidationRule.cs
└── ViewInterfaces/    # UI contracts
    └── IFeatureView.cs
```

### Test Organization
```
tests/BlockLife.Core.Tests/Features/[Feature]/
├── Commands/
│   └── FeatureCommandHandlerTests.cs
├── Services/
│   └── FeatureServiceTests.cs
└── Validation/
    └── FeatureValidationTests.cs
```

## 🔒 Critical Constraints

### Thread Safety
- **Use ConcurrentDictionary** for shared state
- **Test with concurrent operations** (100+ simultaneous)
- **Avoid dual state management** (single source of truth)

### Performance
- **Operations must complete <16ms** for 60fps
- **No blocking main thread** with async operations
- **Dispose resources properly** to prevent memory leaks
- **Pre-warm async/await patterns** to avoid JIT compilation delays

#### Async/Await Pre-warming (Critical for Godot/Mono)
```csharp
// ⚠️ GOTCHA: First async operation can take 200-300ms due to JIT compilation
// Solution: Pre-warm during startup (_Ready method)

private async Task PreWarmAsyncPatterns()
{
    // Pre-warm Option<T>.Match with async lambdas (BF_001: caused 271ms delay)
    var testOption = Some(new Vector2Int(0, 0));
    await testOption.Match(
        Some: async pos => await Task.CompletedTask,
        None: async () => await Task.CompletedTask
    );
    
    // Pre-warm other async patterns used in your code
    await Task.Delay(1);
}

// In _Ready():
PreWarmAsyncPatterns().Wait(500); // Allow up to 500ms for pre-warming
```
**Lesson from BF_001**: Mono/.NET async state machines have significant first-time initialization costs in Godot runtime.

### Godot Integration
- **Presenters can use Godot APIs** (allowed in presentation layer)
- **Views forward events to Presenters** (minimal logic in views)
- **Use signals for animation completion** (not async/await)

## 🧪 Architecture Validation

### Essential Tests
```csharp
[Test] 
public void Core_ShouldNotReference_Godot()
{
    // Ensures domain purity
}

[Test]
public void Handlers_ShouldFollow_NamingConvention()
{
    // Ensures consistency
}

[Test]
public void Commands_ShouldBe_Immutable()
{
    // Ensures predictability
}
```

## 🔗 Quick Reference Links

- **Gold Standard**: `src/Features/Block/Move/` (copy this structure)
- **Technical Patterns**: [Technical_Patterns.md](../../Technical_Patterns.md) (debugging, state management)
- **Development Workflow**: [Development_Workflows.md](../../Development_Workflows.md) (daily process)
- **Comprehensive Theory**: `Docs/_archive/comprehensive_guides/` (when deep understanding needed)

---

*This practical guide focuses on daily development needs. For comprehensive architectural theory and detailed rationale, consult the archived comprehensive guide.*