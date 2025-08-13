# ADR-007: Enhanced Functional Validation Pattern

**Status**: ✅ **ACCEPTED**  
**Date**: 2025-08-13  
**Supersedes**: None  
**Participants**: Development Team, Tech Lead Agent  
**Tags**: `validation`, `functional-programming`, `architecture`, `languageext`

## Context

Our BlockLife project uses a functional programming approach with LanguageExt for validation, built on `Fin<T>` monadic error handling and CQRS command validation. As we scale the project, we need to enhance our validation patterns while maintaining architectural consistency.

### Current Validation Approach

We currently implement validation through discrete rule services:

```csharp
public interface IPositionIsValidRule
{
    Fin<Unit> Validate(Vector2Int position);
}

// Used in command handlers with monadic composition:
public async Task<Fin<Unit>> Handle(PlaceBlockCommand request, CancellationToken cancellationToken)
{
    return await (
        from validPosition in _positionValidRule.Validate(request.Position)
        from emptyPosition in _positionEmptyRule.Validate(request.Position)
        from block in CreateBlock(request)
        from placed in PlaceBlockInGrid(block)
        select Unit.Default
    ).AsTask();
}
```

### Problem Statement

While our current approach works well, we identified areas for improvement:

1. **Repetitive Validation Code**: Similar validation patterns repeated across handlers
2. **Limited Composability**: Difficulty combining multiple validation rules dynamically
3. **Verbose Rule Definitions**: Each rule requires interface + implementation
4. **Complex Conditional Validation**: Hard to express validation that depends on multiple conditions

### Alternative Considered: FluentValidation

We evaluated introducing FluentValidation but rejected it due to:
- **Paradigm Mismatch**: Imperative/exception-based vs functional patterns
- **Type Safety Loss**: String-based errors vs typed `Fin<T>` errors  
- **Architecture Violation**: Would require complex adapters to integrate with `Fin<T>`
- **Testing Complexity**: Less precise test isolation compared to current approach

## Decision

**We will enhance our current functional validation pattern** by building composable validation combinators and utilities while maintaining our `Fin<T>` monadic approach.

## Detailed Design

### 1. Validation Extension Methods

Create composable validation pipelines:

```csharp
// Core validation extension
public static class ValidationExtensions
{
    public static Fin<T> Validate<T>(
        this T value,
        params Func<T, Fin<Unit>>[] validators) =>
        validators.Aggregate(
            FinSucc(value),
            (acc, validator) => acc.Bind(_ => validator(value).Map(_ => value))
        );

    public static Fin<T> ValidateAsync<T>(
        this T value,
        params Func<T, Task<Fin<Unit>>>[] validators) =>
        validators.AggregateAsync(
            FinSucc(value).AsTask(),
            async (acc, validator) => 
                await acc.BindAsync(async _ => 
                    (await validator(value)).Map(_ => value))
        );
}
```

### 2. Higher-Order Validation Combinators

Build reusable validation functions:

```csharp
public static class Validators
{
    // Position validation combinators
    public static Func<Vector2Int, Fin<Unit>> WithinBounds(Vector2Int maxBounds) =>
        pos => pos.X >= 0 && pos.X < maxBounds.X && pos.Y >= 0 && pos.Y < maxBounds.Y
            ? FinSucc(Unit.Default)
            : FinFail<Unit>(Error.New("POSITION_OUT_OF_BOUNDS", 
                $"Position {pos} is outside bounds {maxBounds}"));

    // Generic validation combinators
    public static Func<T, Fin<Unit>> NotNull<T>() where T : class =>
        value => value != null
            ? FinSucc(Unit.Default)
            : FinFail<Unit>(Error.New("NULL_VALUE", "Value cannot be null"));

    public static Func<T, Fin<Unit>> Must<T>(
        Func<T, bool> predicate, 
        string errorCode, 
        string errorMessage) =>
        value => predicate(value)
            ? FinSucc(Unit.Default)
            : FinFail<Unit>(Error.New(errorCode, errorMessage));

    // Conditional validation
    public static Func<T, Fin<Unit>> When<T>(
        Func<T, bool> condition,
        Func<T, Fin<Unit>> validator) =>
        value => condition(value) ? validator(value) : FinSucc(Unit.Default);
}
```

### 3. Domain-Specific Validation Specifications

Create validation specifications for complex business rules:

```csharp
// Block placement validation specification
public sealed record BlockPlacementValidation(
    IGridStateService GridState,
    IBlockRepository BlockRepo)
{
    public Fin<Unit> ValidatePlacement(PlaceBlockCommand cmd) =>
        from _ in ValidatePosition(cmd.Position)
        from __ in ValidateAvailability(cmd.Position)
        from ___ in ValidateBlockType(cmd.Type)
        select Unit.Default;

    private Fin<Unit> ValidatePosition(Vector2Int pos) =>
        Validators.WithinBounds(GridState.GridSize)(pos);

    private Fin<Unit> ValidateAvailability(Vector2Int pos) =>
        !GridState.IsPositionOccupied(pos)
            ? FinSucc(Unit.Default)
            : FinFail<Unit>(Error.New("POSITION_OCCUPIED", 
                $"Position {pos} is already occupied"));

    private Fin<Unit> ValidateBlockType(BlockType type) =>
        Enum.IsDefined(typeof(BlockType), type)
            ? FinSucc(Unit.Default)
            : FinFail<Unit>(Error.New("INVALID_BLOCK_TYPE", 
                $"Block type {type} is not valid"));
}
```

### 4. Enhanced Command Handler Pattern

Updated command handlers using enhanced validation:

```csharp
public class PlaceBlockCommandHandler : IRequestHandler<PlaceBlockCommand, Fin<Unit>>
{
    private readonly BlockPlacementValidation _validator;
    private readonly IGridStateService _gridState;
    private readonly ISimulationManager _simulation;

    public async Task<Fin<Unit>> Handle(PlaceBlockCommand request, CancellationToken cancellationToken)
    {
        return await (
            from _ in _validator.ValidatePlacement(request)
            from block in CreateBlock(request)
            from placed in PlaceBlockInGrid(block)
            from effectQueued in QueueEffect(block)
            select Unit.Default
        ).AsTask();
    }
}
```

### 5. Validation DSL (Future Enhancement)

Optional fluent DSL for complex validation scenarios:

```csharp
public static class ValidationDSL
{
    public static ValidationChain<T> For<T>(T value) => new ValidationChain<T>(value);
}

public class ValidationChain<T>
{
    private readonly T _value;
    private readonly List<Func<T, Fin<Unit>>> _validators = new();

    internal ValidationChain(T value) => _value = value;

    public ValidationChain<T> Must(Func<T, bool> predicate, string error)
    {
        _validators.Add(val => predicate(val) 
            ? FinSucc(Unit.Default) 
            : FinFail<Unit>(Error.New("VALIDATION_FAILED", error)));
        return this;
    }

    public ValidationChain<T> When(Func<T, bool> condition, Func<ValidationChain<T>, ValidationChain<T>> configure)
    {
        if (condition(_value))
        {
            var conditionalChain = new ValidationChain<T>(_value);
            configure(conditionalChain);
            _validators.AddRange(conditionalChain._validators);
        }
        return this;
    }

    public Fin<T> Validate() => _value.Validate(_validators.ToArray());
}

// Usage:
var result = ValidationDSL.For(command)
    .Must(cmd => cmd.Position.X >= 0, "X coordinate must be non-negative")
    .Must(cmd => cmd.Position.Y >= 0, "Y coordinate must be non-negative")
    .When(cmd => cmd.Type == BlockType.Special, 
          chain => chain.Must(cmd => HasPermission(cmd), "No permission for special blocks"))
    .Validate();
```

## Implementation Strategy

### Phase 1: Core Extensions (Week 1)
1. Create `ValidationExtensions` class with basic combinators
2. Implement `Validators` static class with common validation functions
3. Update architecture tests to validate new patterns

### Phase 2: Domain Specifications (Week 2)
1. Create validation specifications for existing command handlers
2. Migrate `PlaceBlockCommandHandler` to use new pattern
3. Update unit tests to test validation specifications separately

### Phase 3: Enhanced Patterns (Week 3)
1. Implement conditional validation support
2. Create async validation pipeline support
3. Document patterns in architecture guide

### Phase 4: Optional DSL (Future)
1. Implement `ValidationDSL` if complex scenarios emerge
2. Evaluate adoption and refinement based on usage

## Consequences

### Positive Consequences

✅ **Enhanced Composability**: Validation rules can be easily combined and reused  
✅ **Maintained Type Safety**: All validation remains within `Fin<T>` monadic approach  
✅ **Improved Testability**: Validation specifications can be tested independently  
✅ **Better Readability**: Complex validation logic becomes more declarative  
✅ **Reduced Boilerplate**: Less interface/implementation pairs needed  
✅ **Functional Consistency**: Maintains alignment with LanguageExt patterns  
✅ **No New Dependencies**: Builds on existing functional programming foundation  

### Negative Consequences

⚠️ **Learning Curve**: Developers may need time to adopt new validation patterns  
⚠️ **Migration Effort**: Existing validation rules need gradual migration  
⚠️ **Pattern Complexity**: Advanced combinators may be harder to understand initially  

### Risk Mitigation

1. **Gradual Migration**: Migrate one command handler at a time
2. **Clear Documentation**: Create comprehensive examples and patterns guide
3. **Architecture Tests**: Ensure new patterns maintain architectural constraints
4. **Team Training**: Provide functional programming guidance for validation patterns

## Validation Metrics

Success criteria for this enhancement:
- ✅ All existing validation functionality preserved
- ✅ Reduced code duplication in validation logic
- ✅ Maintained 100% test coverage
- ✅ No architecture fitness test violations
- ✅ Improved validation performance (measured via benchmarks)

## Alternatives Considered

### Alternative 1: FluentValidation
- **Pros**: Industry standard, rich feature set, good documentation
- **Cons**: Imperative paradigm mismatch, requires adapter layer, breaks functional composition
- **Verdict**: ❌ Rejected due to architectural inconsistency

### Alternative 2: Data Annotations
- **Pros**: Built into .NET, attribute-based, simple syntax
- **Cons**: Reflection-based, exception-throwing, limited composability
- **Verdict**: ❌ Rejected due to performance and functional programming mismatch

### Alternative 3: Custom Validation Framework
- **Pros**: Perfect fit for our needs, full control
- **Cons**: Significant development effort, maintenance burden
- **Verdict**: ⚖️ Considered but deemed unnecessary given LanguageExt capabilities

## Related Decisions

- **ADR-006**: [Fin_Task_Consistency](ADR-006_Fin_Task_Consistency.md) - Establishes `Fin<T>` as standard error handling
- **Architecture Guide**: [Architecture_Guide.md](../1_Architecture/Architecture_Guide.md) - Clean Architecture principles
- **Standard Patterns**: [Standard_Patterns.md](../1_Architecture/Standard_Patterns.md) - Established architectural patterns

## References

- [LanguageExt Documentation](https://github.com/louthy/language-ext) - Functional programming library
- [F1 Block Placement Implementation](../3_Implementation_Plans/1_F1_Block_Placement_Implementation_Plan.md) - Reference implementation patterns
- [Move Block Feature Implementation](../3_Implementation_Plans/2_Move_Block_Feature_Implementation_Plan.md) - Current validation usage

---

**Approved By**: Development Team  
**Implementation Start**: 2025-08-13  
**Review Date**: 2025-09-13 (1 month)  
**Status**: ✅ **ACCEPTED** - Implementation in progress

## Update Log

- **2025-08-13**: Initial ADR created based on tech-lead architectural assessment
- **2025-08-13**: Approved for implementation with phased approach