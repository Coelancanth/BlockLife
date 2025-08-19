# Architecture Guide (Practical)


## 🎯 Reference Implementation
**Move Block Feature**: `src/Features/Block/Move/`
- File structure and naming conventions
- Command/Handler/Service patterns  
- Test organization and coverage
- MVP integration with Godot

## 📝 Architecture Decision Records (ADRs)
**[ADR Directory](ADR/)**: Documented architectural decisions
- Significant patterns and frameworks
- Technology choices and trade-offs
- See [ADR-001](ADR/ADR-001-pattern-recognition-framework.md) for Pattern Recognition Framework

When making architectural decisions:
1. Check existing ADRs for precedent
2. Create new ADR for significant decisions
3. Reference ADRs in code comments

## 📐 Grid Coordinate Convention (TD_016)

**CRITICAL**: All grid operations MUST follow this single coordinate system to prevent bugs.

### Coordinate System Rules
```markdown
- **Origin**: (0,0) at bottom-left corner
- **X-axis**: Increases rightward (0 → GridWidth-1)  
- **Y-axis**: Increases upward (0 → GridHeight-1)
- **Access pattern**: grid[x,y] consistently
- **Godot alignment**: Y+ is up (not down like screen coords)
```

### Visual Reference
```
Y
↑
3 □ □ □ □
2 □ □ □ □  
1 □ □ □ □
0 □ □ □ □
  0 1 2 3 → X

Position (2,1) is third column, second row from bottom
```

### Code Enforcement
```csharp
// Use this helper for all grid access
public static class GridCoordinates
{
    public static void AssertValid(Vector2Int pos, Vector2Int gridSize, string context)
    {
        if (pos.X < 0 || pos.X >= gridSize.X)
            throw new ArgumentException($"{context}: X={pos.X} out of range [0,{gridSize.X})");
        if (pos.Y < 0 || pos.Y >= gridSize.Y)
            throw new ArgumentException($"{context}: Y={pos.Y} out of range [0,{gridSize.Y})");
    }
    
    // Convert to array index (if using 1D array storage)
    public static int ToIndex(Vector2Int pos, int gridWidth) => pos.Y * gridWidth + pos.X;
    
    // Convert from array index
    public static Vector2Int FromIndex(int index, int gridWidth) => 
        new Vector2Int(index % gridWidth, index / gridWidth);
}
```

### Common Mistakes to Avoid
- ❌ **DON'T** use top-left origin (screen coordinates)
- ❌ **DON'T** mix row-major vs column-major access
- ❌ **DON'T** use Y-down convention from UI frameworks
- ✅ **DO** validate coordinates before array access
- ✅ **DO** use consistent [x,y] ordering everywhere
- ✅ **DO** document any coordinate transformations

## ⚠️ Namespace Design Rules

**CRITICAL**: Never name a class the same as its containing namespace!

### The Problem
```csharp
// ❌ NEVER DO THIS - Creates ambiguity
namespace BlockLife.Core.Domain.Block
{
    public class Block { }  // Block is both namespace AND class
}

// Results in compilation errors:
using BlockLife.Core.Domain.Block;
var block = new Block();  // ERROR: 'Block' is a namespace but used like a type
```

### The Solution
```csharp
// ✅ DO THIS - Use plural for namespace
namespace BlockLife.Core.Domain.Blocks  // Plural
{
    public class Block { }  // Singular
}

// Or use different naming
namespace BlockLife.Core.Domain.BlockManagement
{
    public class Block { }
}
```

### Why This Matters
- Prevents 20+ file compilation failures (learned from experience!)
- Avoids need for fully qualified names everywhere
- Makes code more readable and maintainable
- Reduces cognitive load on developers

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

### 6. **Error Handling Context**
```csharp
// ✅ Business Logic: Use Fin<T> for domain errors
public async Task<Fin<Unit>> Handle(PlaceBlockCommand request)
{
    var validation = await _validator.Validate(request);
    if (validation.IsFail) return validation;
    
    var result = await _service.PlaceBlock(request.Position);
    if (result.IsFail) return result;
    
    // NO try-catch in business logic!
    return Unit.Default;
}

// ✅ Infrastructure: Use try-catch for stability
public static Task PreWarmSystem()
{
    try
    {
        // Optional optimization that shouldn't crash app
        var tween = CreateTween();
        tween?.Kill();
    }
    catch (Exception ex)
    {
        _logger?.Warning(ex, "Pre-warm failed, continuing");
        return Task.CompletedTask;
    }
}

// ✅ Type Aliases: Prevent namespace conflicts
using LangError = LanguageExt.Common.Error;
using GodotError = Godot.Error;
```
**Rule**: Infrastructure can use try-catch for stability. Business logic must use `Fin<T>` for errors.

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
- **Technical Patterns**: [Patterns.md](Patterns.md) (implementation patterns and debugging approaches)
- **Development Workflow**: [Workflow.md](../01-Active/Workflow.md) (daily development process)
- **Post-Mortem Lessons**: `../06-PostMortems/Archive/` (learned from real issues)

---

*This practical guide focuses on daily development needs. For comprehensive architectural theory and detailed rationale, consult the archived comprehensive guide.*