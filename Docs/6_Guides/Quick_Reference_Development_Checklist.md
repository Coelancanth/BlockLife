# Quick Reference: BlockLife Development Checklist

## Daily Development Workflow

### Starting a New Feature

#### 1. Pre-Development Checklist
```bash
# Update and verify environment
git checkout main
git pull origin main
dotnet restore
dotnet build
dotnet test --filter "Category=Architecture"  # Ensure architecture is clean
```

#### 2. Create Feature Branch
```bash
git checkout -b feature/[feature-name]
```

#### 3. Feature Slice Setup
```powershell
# Use the template (if available)
dotnet new blocklife-feature -n [FeatureName]

# Or manually create structure:
mkdir src/Features/[Feature]/Commands
mkdir src/Features/[Feature]/Presenters
mkdir src/Features/[Feature]/Rules
mkdir tests/Features/[Feature]
mkdir godot_project/features/[feature]
```

### TDD Cycle (Red-Green-Refactor)

#### Red Phase: Write Failing Test
```csharp
// 1. Architecture test
[Fact]
public void Feature_Commands_Are_Immutable() { }

// 2. Property test
[Property]
public Property Feature_Invariants_Hold() { }

// 3. Unit test
[Fact]
public async Task Handler_ValidInput_Success() { }
```

#### Green Phase: Make It Pass
```csharp
// Minimal implementation to pass tests
public async Task<Fin<Unit>> Handle(Command request, CancellationToken ct)
{
    return FinSucc(Unit.Default);  // Start simple
}
```

#### Refactor Phase: Improve Design
```csharp
// Add proper implementation
return await (
    from validated in Validate(request)
    from result in Execute(validated)
    from effect in QueueEffect(result)
    select Unit.Default
).AsTask();
```

### Command Implementation Checklist

#### Command Definition
- [ ] Record type with init-only properties
- [ ] Implements ICommand interface
- [ ] Includes validation data
- [ ] Has meaningful parameter names

```csharp
public sealed record PlaceBlockCommand(
    Vector2Int Position,
    BlockType Type = BlockType.Basic,
    Guid? RequestedId = null
) : ICommand;
```

#### Handler Implementation
- [ ] Returns Fin<Unit> or Fin<T>
- [ ] Uses functional composition
- [ ] Validates input first
- [ ] Queues effects (not executes)
- [ ] Has structured logging
- [ ] No side effects in handler

```csharp
public class PlaceBlockCommandHandler : IRequestHandler<PlaceBlockCommand, Fin<Unit>>
{
    public async Task<Fin<Unit>> Handle(PlaceBlockCommand request, CancellationToken ct)
    {
        _logger.LogDebug("Handling {Command}", nameof(PlaceBlockCommand));
        
        return await (
            from valid in _validator.Validate(request)
            from placed in _gridState.PlaceBlock(request)
            from effect in _simulation.QueueEffect(new BlockPlacedEffect(request))
            select Unit.Default
        ).AsTask();
    }
}
```

### Presenter Implementation Checklist

#### Presenter Setup
- [ ] Inherits from PresenterBase<TView>
- [ ] Implements notification handlers
- [ ] No mutable fields (use Subject/Observable)
- [ ] Proper disposal of subscriptions
- [ ] No Godot references

```csharp
public class GridPresenter : PresenterBase<IGridView>,
    INotificationHandler<BlockPlacedNotification>
{
    private readonly CompositeDisposable _subscriptions = new();
    
    public override async Task InitializeAsync()
    {
        View.CellClicked
            .Subscribe(OnCellClicked)
            .DisposeWith(_subscriptions);
    }
    
    public override async Task CleanupAsync()
    {
        _subscriptions.Dispose();
    }
}
```

### View Implementation Checklist

#### Godot View Setup
- [ ] Implements view interface
- [ ] Implements IPresenterContainer<T>
- [ ] Creates presenter in _Ready
- [ ] Disposes presenter in _ExitTree
- [ ] No business logic in view

```csharp
public partial class GridView : Control, IGridView, IPresenterContainer<GridPresenter>
{
    private GridPresenter _presenter;
    
    public override void _Ready()
    {
        _presenter = SceneRoot.Instance.CreatePresenterFor(this);
    }
    
    public override void _ExitTree()
    {
        _presenter?.DisposeAsync();
    }
}
```

### Testing Checklist

#### Unit Test Coverage
- [ ] Happy path tested
- [ ] Error cases tested
- [ ] Boundary conditions tested
- [ ] Null/empty inputs tested
- [ ] Concurrent scenarios tested

#### Property Test Coverage
- [ ] Invariants defined
- [ ] Generators created
- [ ] 100+ test cases run
- [ ] Edge cases covered

#### Architecture Test Coverage
- [ ] No Godot in Core
- [ ] Commands are immutable
- [ ] Handlers return Fin<T>
- [ ] Presenters have no state
- [ ] Services use interfaces

### Pre-Commit Checklist

#### Code Quality
- [ ] All tests pass
- [ ] No compiler warnings
- [ ] Code formatted (dotnet format)
- [ ] TODOs addressed or ticketed
- [ ] No commented code
- [ ] No debug code

#### Architecture
- [ ] Clean Architecture maintained
- [ ] CQRS pattern followed
- [ ] MVP pattern correct
- [ ] No static service locators
- [ ] Constructor injection used

#### Documentation
- [ ] Public APIs documented
- [ ] Complex logic explained
- [ ] Feature plan updated
- [ ] ADR written (if applicable)

### Common Commands

```powershell
# Build
dotnet build                           # Debug build
dotnet build -c Release               # Release build

# Test
dotnet test                            # All tests
dotnet test --filter "Category=Unit"   # Unit tests only
dotnet test --filter "Category=Architecture"  # Architecture tests
dotnet test --filter "FullyQualifiedName~PlaceBlock"  # Specific feature

# Godot
$env:GODOT_BIN="C:\path\to\godot.exe"
addons\gdUnit4\runtest.cmd            # Run GdUnit tests

# Git
git status                             # Check changes
git add -A                            # Stage all
git commit -m "feat: implement block placement"
git push origin feature/block-placement

# Code Quality
dotnet format                          # Format code
dotnet build /p:TreatWarningsAsErrors=true  # Strict build
```

### Error Patterns to Avoid

#### ❌ Anti-Pattern: Godot in Core
```csharp
// BAD: Core project
using Godot;  // Never in src/
public class Block : Node2D  // Wrong layer
```

#### ✅ Correct Pattern
```csharp
// GOOD: Core project
public sealed record Block(
    Guid Id,
    Vector2Int Position,
    BlockType Type
);
```

#### ❌ Anti-Pattern: Mutable Commands
```csharp
// BAD
public class PlaceBlockCommand
{
    public Vector2Int Position { get; set; }  // Mutable
}
```

#### ✅ Correct Pattern
```csharp
// GOOD
public sealed record PlaceBlockCommand(
    Vector2Int Position  // Immutable
) : ICommand;
```

#### ❌ Anti-Pattern: Side Effects in Handlers
```csharp
// BAD
public async Task<Fin<Unit>> Handle(Command request)
{
    _database.Save(entity);  // Direct side effect
    _eventBus.Publish(event);  // Direct publishing
}
```

#### ✅ Correct Pattern
```csharp
// GOOD
public async Task<Fin<Unit>> Handle(Command request)
{
    return await _simulation.QueueEffect(new EntitySavedEffect(entity));
    // Effects processed by SimulationManager
}
```

### Debugging Tips

#### Enable Verbose Logging
```csharp
// In GameStrapper.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()  // For debugging
    .WriteTo.GodotConsole()
    .CreateLogger();
```

#### Use Debug Commands
```csharp
#if DEBUG
// Quick test commands
var debugCommand = new DebugFillGridCommand(50);
await mediator.Send(debugCommand);
#endif
```

#### Trace Command Flow
```csharp
_logger.LogDebug("Command: {Command}", command);
_logger.LogDebug("Validation: {Result}", validationResult);
_logger.LogDebug("Effect: {Effect}", effect);
_logger.LogDebug("Notification: {Notification}", notification);
```

### Performance Quick Wins

#### 1. Cache Expensive Calculations
```csharp
private readonly Lazy<IReadOnlyList<Block>> _allBlocks = 
    new(() => CalculateAllBlocks());
```

#### 2. Use Spatial Indexing
```csharp
private readonly SpatialHashGrid<Block> _spatialIndex = new(cellSize: 1);
```

#### 3. Batch Operations
```csharp
// Instead of multiple commands
await Task.WhenAll(commands.Select(cmd => mediator.Send(cmd)));

// Use batch command
await mediator.Send(new BatchPlaceBlocksCommand(positions));
```

### When to Break the Rules

#### Acceptable Pragmatism
1. **Performance Critical Path**: Profile first, then optimize
2. **Godot Limitations**: Document workaround in ADR
3. **Third-Party Integration**: Isolate in adapter layer
4. **Prototyping**: Mark with TODO and tech debt ticket

#### Never Compromise On
1. **Architecture Boundaries**: Core stays pure
2. **Test Coverage**: Always write tests
3. **Error Handling**: Always use Fin<T>
4. **Immutability**: Commands/Events stay immutable

### Getting Help

#### Resources
- Architecture Guide: `Docs/1_Architecture/Architecture_Guide.md`
- Test Guide: `Docs/1_Architecture/Test_Guide.md`
- Style Guide: `Docs/1_Architecture/Style_Guide.md`
- Implementation Plans: [3_Implementation_Plans/](../3_Implementation_Plans/)

#### Common Issues
- **"Cannot find presenter"**: Check DI registration in GameStrapper
- **"Position occupied"**: Ensure grid state is cleared between tests
- **"Godot reference in Core"**: Move to presentation layer
- **"Handler not found"**: Register in MediatR configuration

---

**Quick Reference Version**: 1.0  
**Last Updated**: 2025-08-13  
**Print-Friendly**: Yes  
**Keep Handy**: On second monitor or printed