# Dev Engineer Workflow

## Core Principle

**Make tests pass fast** with clean C# code. Copy existing patterns, adapt minimally.

## Action 1: Implement Code to Pass Test

### When to Use
User says: "Make this test pass" or provides failing test

### Process
1. **Analyze failing test**
   - What behavior is expected?
   - What interfaces/contracts needed?
   - What assertion checks for?

2. **Copy existing pattern** from `src/Features/Block/Move/`
   - Same folder structure
   - Same naming conventions
   - Same error handling (Fin<T>)

3. **Adapt pattern** to match test
   - Change class/method names
   - Modify business logic
   - Update return values

4. **Verify test passes**
   - Run specific test
   - Ensure GREEN status
   - No other tests broken

### Standard Implementation Template
```csharp
public class FeatureCommandHandler : IRequestHandler<FeatureCommand, Fin<Unit>>
{
    private readonly IRequiredService _service;
    
    public FeatureCommandHandler(IRequiredService service)
    {
        _service = service;
    }
    
    public async Task<Fin<Unit>> Handle(FeatureCommand request, CancellationToken ct)
    {
        // Minimal logic to pass test
        return Unit.Default;
    }
}
```

## Action 2: Create Supporting Infrastructure

### When to Use
Test requires commands, services, or view interfaces

### Commands/Queries
```csharp
// Copy from Move Block pattern
public record FeatureCommand(
    EntityId Id,
    RequiredData Data
) : IRequest<Fin<Unit>>;
```

### Services
```csharp
public interface IFeatureService
{
    Task<Fin<Result>> DoOperation(Input input);
}

public class FeatureService : IFeatureService
{
    public async Task<Fin<Result>> DoOperation(Input input)
    {
        // Minimal implementation for test
    }
}
```

### Presenters (if UI tests)
```csharp
public class FeaturePresenter : PresenterBase<IFeatureView>
{
    public override void Initialize()
    {
        // Subscribe to view events
    }
    
    public override void Dispose()
    {
        // Unsubscribe from events
        base.Dispose();
    }
}
```

## C# Best Practices

### Error Handling
```csharp
// Always use Fin<T>
return result.IsSucc 
    ? Unit.Default
    : Error.New("OPERATION_FAILED", result.Error);
```

### Dependency Injection
```csharp
// Register in GameStrapper
services.AddSingleton<IFeatureService, FeatureService>();
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
```

### Async Patterns
```csharp
// Proper async/await usage
public async Task<Fin<Unit>> Handle(Command request, CancellationToken ct)
{
    var result = await _service.DoAsync(request.Data);
    return result.Match(
        Succ: _ => Unit.Default,
        Fail: error => error
    );
}
```

## Godot Integration

### View Implementation
```csharp
public partial class FeatureView : Control, IFeatureView
{
    private IPresenter? _presenter;
    
    public override void _Ready()
    {
        _presenter = PresenterFactory.CreateFor(this);
    }
    
    public override void _ExitTree()
    {
        _presenter?.Dispose();
    }
}
```

### Signal Handling
```csharp
// In presenter
public override void Initialize()
{
    View.ButtonPressed += OnButtonPressed;
}

private async void OnButtonPressed()
{
    var command = new FeatureCommand(...);
    await _mediator.Send(command);
}
```

## Quality Gates

- **Test passes** with minimal code
- **Follows existing patterns** exactly
- **No compiler warnings**
- **Clean C# syntax** and naming
- **Proper DI registration**
- **No Godot in src/ folder**

## Success Metrics

- **Implementation in <10 minutes** for standard patterns
- **Copy-paste-adapt workflow** for 90% of cases
- **Clean, readable C# code**
- **All tests GREEN after implementation**