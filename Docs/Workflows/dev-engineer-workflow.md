# Dev Engineer Workflow

## Purpose
Define exact procedures for the Dev Engineer agent to implement code during the TDD GREEN phase, following test specifications without over-engineering.

---

## Core Workflow Actions

### 1. Implement Code to Pass Tests (GREEN Phase)

**Trigger**: "Make this test pass" or "Implement for this failing test"

**Input Required**:
- Failing test code
- Test location and context
- Reference patterns to follow
- Architecture constraints

**Steps**:

1. **Analyze Failing Test**
   ```csharp
   // Read test to understand:
   - What behavior is expected?
   - What interfaces/contracts are needed?
   - What the assertion checks for
   - Any mocks or dependencies
   ```

2. **Identify Implementation Needs**
   - Commands/Queries needed
   - Handlers required
   - Services to implement
   - View interfaces to define

3. **Write Minimal Implementation**
   ```csharp
   // ONLY write code to make test pass
   public class FeatureHandler : IRequestHandler<FeatureCommand, Fin<Unit>>
   {
       public Task<Fin<Unit>> Handle(FeatureCommand request, CancellationToken ct)
       {
           // Minimal logic to satisfy test assertions
           // No extra features
           // No premature optimization
       }
   }
   ```

4. **Follow Existing Patterns**
   - Check reference implementation (Move Block)
   - Use same folder structure
   - Apply same naming conventions
   - Match error handling patterns

5. **Verify Test Passes**
   - Run the specific test
   - Ensure GREEN status
   - No other tests broken

**Output Format**:
```
✅ Test now passing: [TestName]

Implemented:
- Command: FeatureCommand.cs
- Handler: FeatureCommandHandler.cs
- Service: FeatureService.cs (if needed)

All tests GREEN. Ready for refactoring phase.
```

---

### 2. Create Command/Query Objects

**Trigger**: Test requires command or query

**Steps**:

1. **Define Command Structure**
   ```csharp
   public record FeatureCommand(
       EntityId Id,
       RequiredData Data,
       OptionalData? Options = null
   ) : IRequest<Fin<Unit>>;
   ```

2. **Add Validation Attributes**
   ```csharp
   public record FeatureCommand(
       [property: Required] EntityId Id,
       [property: Range(0, 100)] int Value
   ) : IRequest<Fin<Unit>>;
   ```

3. **Follow Naming Conventions**
   - Commands: VerbNounCommand (MoveBlockCommand)
   - Queries: GetNounQuery (GetBlocksQuery)
   - DTOs: NounDto (BlockDto)

**Output**: Command/Query objects that match test expectations

---

### 3. Implement Handlers

**Trigger**: Test requires handler implementation

**Steps**:

1. **Create Handler Class**
   ```csharp
   public class FeatureCommandHandler : IRequestHandler<FeatureCommand, Fin<Unit>>
   {
       private readonly IRequiredService _service;
       
       public FeatureCommandHandler(IRequiredService service)
       {
           _service = service;
       }
   }
   ```

2. **Implement Handle Method**
   ```csharp
   public async Task<Fin<Unit>> Handle(
       FeatureCommand request, 
       CancellationToken cancellationToken)
   {
       // 1. Validation (if not done by pipeline)
       // 2. Business logic (minimal for test)
       // 3. State changes
       // 4. Publish notifications
       // 5. Return result
       
       return Fin<Unit>.Succ(Unit.Default);
   }
   ```

3. **Error Handling Pattern**
   ```csharp
   return await _service.TryOperation()
       .Match(
           Succ: _ => Fin<Unit>.Succ(Unit.Default),
           Fail: error => Fin<Unit>.Fail(error)
       );
   ```

**Output**: Handler that satisfies test requirements

---

### 4. Create Services

**Trigger**: Handler needs service implementation

**Steps**:

1. **Define Interface** (if not exists)
   ```csharp
   public interface IFeatureService
   {
       Task<Fin<Result>> PerformOperation(Parameters params);
   }
   ```

2. **Implement Service**
   ```csharp
   public class FeatureService : IFeatureService
   {
       // Only implement methods called by tests
       public Task<Fin<Result>> PerformOperation(Parameters params)
       {
           // Minimal implementation for test
       }
   }
   ```

3. **Register in DI Container**
   ```csharp
   // In GameStrapper or registration module
   services.AddSingleton<IFeatureService, FeatureService>();
   ```

**Output**: Service implementation matching test needs

---

### 5. Create Presenters and Views

**Trigger**: UI-related test requires presenter

**Steps**:

1. **Define View Interface**
   ```csharp
   public interface IFeatureView : IView
   {
       void UpdateDisplay(DisplayData data);
       event Action<UserInput> UserAction;
   }
   ```

2. **Implement Presenter**
   ```csharp
   public class FeaturePresenter : PresenterBase<IFeatureView>
   {
       private readonly IMediator _mediator;
       
       public override void Initialize()
       {
           View.UserAction += OnUserAction;
           // Subscribe to notifications
       }
       
       private async void OnUserAction(UserInput input)
       {
           // Send command via mediator
           // Update view based on result
       }
   }
   ```

3. **Wire Up in Godot** (if needed)
   ```csharp
   public partial class FeatureView : Control, IFeatureView
   {
       private IPresenter? _presenter;
       
       public override void _Ready()
       {
           _presenter = PresenterFactory.CreateFor(this);
       }
   }
   ```

**Output**: MVP triad satisfying test requirements

---

### 6. Handle Dependency Injection

**Trigger**: Implementation needs DI setup

**Steps**:

1. **Register Services**
   ```csharp
   services.AddSingleton<IService, ServiceImpl>();
   services.AddScoped<IScopedService, ScopedImpl>();
   ```

2. **Register Handlers** (MediatR)
   ```csharp
   services.AddMediatR(cfg => 
       cfg.RegisterServicesFromAssembly(typeof(GameStrapper).Assembly));
   ```

3. **Verify Resolution**
   - Ensure all dependencies resolve
   - No circular dependencies
   - Proper lifetimes (Singleton/Scoped/Transient)

**Output**: Properly configured DI container

---

## Implementation Patterns Reference

### Pattern Priority
1. **Exact Match**: Use identical pattern from reference
2. **Adapt Pattern**: Modify reference for current need
3. **New Pattern**: Only if no existing pattern fits

### Common Patterns

**Command Handler Pattern**:
```csharp
public class XCommandHandler : IRequestHandler<XCommand, Fin<Unit>>
{
    private readonly IStateService _state;
    private readonly IMediator _mediator;
    
    // Constructor injection
    // Handle method with Fin<T> return
    // Publish notifications after state change
}
```

**Service Pattern**:
```csharp
public class XService : IXService
{
    private readonly IRepository _repository;
    
    // Async methods returning Fin<T>
    // Proper error handling
    // No business logic (that's in handlers)
}
```

**Presenter Pattern**:
```csharp
public class XPresenter : PresenterBase<IXView>
{
    // Initialize: Subscribe to events
    // Dispose: Unsubscribe from events
    // Handle view events -> Send commands
    // Handle notifications -> Update view
}
```

---

## Quality Checklist

Before marking implementation complete:
- [ ] All specified tests pass?
- [ ] No additional tests broken?
- [ ] Follows existing patterns?
- [ ] Minimal code (no over-engineering)?
- [ ] Proper error handling with Fin<T>?
- [ ] DI properly configured?
- [ ] No untested code added?

---

## Common Pitfalls to Avoid

1. **Over-Engineering**: Adding features not required by tests
2. **Pattern Deviation**: Creating new patterns when existing ones work
3. **Premature Optimization**: Optimizing before refactor phase
4. **Missing DI**: Forgetting to register services
5. **Broken Tests**: Making target test pass but breaking others

---

## Integration Points

### With TDD Commander
- Receive test specifications
- Report when tests pass
- Request clarification on ambiguous tests
- Collaborate during refactor phase

### With Tech Lead
- Follow implementation plan phases
- Use recommended patterns
- Respect architectural decisions

### With QA Engineer
- Code ready for integration testing
- No obvious bugs or issues
- Performance considerations noted

---

## Response Templates

### When implementation complete:
"✅ Test passing: [TestName]

Implemented:
- Location: src/Features/[Domain]/[Feature]/
- Pattern: [Pattern used]
- Lines of code: [Count]
- All tests GREEN

Ready for refactoring or next test."

### When clarification needed:
"❓ Test ambiguity in [TestName]:
- Test expects: [interpretation 1]
- Could also mean: [interpretation 2]
- Recommended: [suggestion]

Please clarify expected behavior."

### When pattern unclear:
"⚠️ No existing pattern for:
- Scenario: [description]
- Closest pattern: [reference]
- Proposed approach: [suggestion]

Proceed with proposed approach?"

---

## TDD Cycle Position

```
             YOU ARE HERE
                  ↓
┌─────┐      ┌─────┐      ┌──────────┐
│ RED │ ---→ │GREEN│ ---→ │ REFACTOR │
└─────┘      └─────┘      └──────────┘
   ↑                           │
   └───────────────────────────┘
```

Your responsibility: Make RED become GREEN with minimal, clean code.