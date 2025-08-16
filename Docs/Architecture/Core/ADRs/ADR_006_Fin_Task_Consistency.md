# ADR-006: Fin<T> vs Task<T> Consistency in Async Operations

## Status
**IMPLEMENTED** - Phase 1 completed successfully ✅

## Context

During the Block Placement Display Bug fix (BPM-005), we discovered an architectural inconsistency that forces imperative error handling in a functional codebase:

```csharp
// Current mixed approach (problematic)
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

### The Inconsistency

**Current State:**
- **Command Handlers**: Return `Task<Fin<Unit>>` ✅ (Functional)
- **Business Logic**: Uses `Fin<T>` monads ✅ (Functional)
- **Infrastructure Services**: Return `Task<T>` ❌ (Imperative)
- **External Integrations**: Return `Task<T>` ❌ (Imperative)

**This creates:**
1. **Forced imperative bridges** with try-catch blocks
2. **Inconsistent error handling** patterns
3. **Loss of composability** in functional chains
4. **Railway-oriented programming breaks**

## Decision Options

### Option 1: Status Quo (Keep Mixed Approach)

**Pros:**
- No breaking changes required
- Works with existing third-party libraries
- Minimal refactoring effort

**Cons:**
- Continued functional/imperative inconsistency
- More try-catch boilerplate needed
- Cannot compose full functional pipelines
- Railway-oriented programming benefits lost

### Option 2: Pure Fin<T> Infrastructure

**Convert all infrastructure to return `Fin<T>`:**

```csharp
public interface ISimulationManager
{
    Fin<Unit> QueueEffect<TEffect>(TEffect effect) where TEffect : class;
    Task<Fin<Unit>> ProcessQueuedEffectsAsync(); // ✅ Changed
    Task<Fin<Unit>> PublishNotificationAsync<TNotification>(TNotification notification); // ✅ Changed
}
```

**Pros:**
- Complete functional consistency
- Full railway-oriented programming benefits  
- Pure functional composition throughout
- No try-catch boilerplate in business logic
- Better error handling and traceability

**Cons:**
- **Major breaking change** across infrastructure
- Requires extensive refactoring
- Third-party library integration complexity
- Learning curve for team members

### Option 3: Hybrid with Extension Methods

**Create extension methods to bridge gaps:**

```csharp
public static class TaskFinExtensions
{
    public static async Task<Fin<T>> ToFin<T>(this Task<T> task)
    {
        try
        {
            var result = await task;
            return FinSucc(result);
        }
        catch (Exception ex)
        {
            return FinFail<T>(Error.New(ex));
        }
    }
    
    public static async Task<Fin<Unit>> ToFin(this Task task)
    {
        try
        {
            await task;
            return FinSucc(Unit.Default);
        }
        catch (Exception ex)
        {
            return FinFail<Unit>(Error.New(ex));
        }
    }
}

// Usage in handlers:
return await result.Match(
    Succ: async block =>
    {
        var notification = new BlockPlacedNotification(/* ... */);
        return await _mediator.Publish(notification).ToFin()
            .Map(_ => Unit.Default);
    },
    Fail: error => Task.FromResult(FinFail<Unit>(error))
);
```

**Pros:**
- Maintains functional composition
- Non-breaking incremental adoption
- Works with existing third-party APIs
- Clean business logic without try-catch

**Cons:**
- Still mixed patterns in codebase
- Extension methods hide complexity
- Not pure functional architecture

## Recommendation

**🎯 RECOMMENDED: Option 3 (Hybrid with Extension Methods) - PHASE 1**

**Then evolve to Option 2 (Pure Fin<T>) - PHASE 2**

### Implementation Strategy

#### Phase 1: Immediate (Next Sprint)
1. **Create `TaskFinExtensions`** with `.ToFin()` methods
2. **Refactor existing try-catch blocks** to use extensions
3. **Update command handlers** to use clean functional composition
4. **Add architectural tests** to prevent regression

```csharp
// Before (current broken pattern)
try
{
    await _simulation.ProcessQueuedEffectsAsync();
    return FinSucc(Unit.Default);
}
catch (Exception ex)
{
    return FinFail<Unit>(Error.New("FAILED", ex.Message));
}

// After Phase 1 (clean functional)
return await _simulation.ProcessQueuedEffectsAsync().ToFin();
```

#### Phase 2: Strategic Refactor (Future Release)
1. **Update ISimulationManager** to return `Task<Fin<Unit>>`
2. **Refactor infrastructure services** to use `Fin<T>`
3. **Create functional wrappers** for third-party libraries
4. **Achieve pure functional architecture**

### Benefits of This Approach

1. **Immediate functional consistency** without breaking changes
2. **Gradual migration path** to pure functional architecture
3. **Clean business logic** without try-catch boilerplate
4. **Railway-oriented programming** restored in command handlers
5. **Architectural evolution** rather than revolution

### Architecture Tests Required

```csharp
[Fact]
public void CommandHandlers_ShouldNotContain_TryCatchBlocks()
{
    var handlerTypes = GetAllCommandHandlerTypes();
    
    foreach (var handlerType in handlerTypes)
    {
        var methods = handlerType.GetMethods();
        var sourceCode = GetSourceCode(handlerType);
        
        sourceCode.Should().NotContain("try {")
            .And.NotContain("catch (")
            .Because("handlers should use functional error handling with Fin<T>");
    }
}

[Fact] 
public void InfrastructureServices_ShouldEventually_ReturnFin()
{
    // Test that gradually ensures infrastructure returns Fin<T>
    // Can start as [Ignore] and enable as services are migrated
}
```

## Impact Assessment

### Breaking Changes: Phase 1
- **None** - Extension methods are additive

### Breaking Changes: Phase 2  
- **High** - Major infrastructure refactor required
- All infrastructure service interfaces change
- All infrastructure service implementations change
- Integration tests need updates

### Migration Effort

#### Phase 1: **Low** (1-2 days)
- Create extension methods
- Update 3-4 command handlers
- Add architectural tests

#### Phase 2: **High** (1-2 weeks)
- Refactor all infrastructure services (~10-15 classes)
- Update all integration points
- Comprehensive testing required

## Decision

**✅ APPROVED for Phase 1 Implementation**

Proceed with Hybrid Extension Method approach to:
1. Immediately fix functional consistency issues
2. Eliminate try-catch boilerplate in business logic  
3. Restore railway-oriented programming benefits
4. Provide migration path to pure functional architecture

Phase 2 decision to be revisited after Phase 1 completion and team feedback.

---

## Implementation Summary (Phase 1)

**Date Completed:** 2025-08-13  
**Branch:** `feature/fin-task-consistency-phase1`

### What Was Implemented:

1. **TaskFinExtensions Class** (`src/Core/Infrastructure/Extensions/TaskFinExtensions.cs`)
   - ✅ 6 extension methods for Task<T> → Fin<T> conversion
   - ✅ Support for custom error codes and messages
   - ✅ Proper exception handling and resource cleanup

2. **Command Handler Refactoring**
   - ✅ **RemoveBlockCommandHandler**: Eliminated try-catch blocks, uses functional composition
   - ✅ **PlaceBlockCommandHandler**: Enhanced with functional error handling for notifications
   - ✅ **MoveBlockCommandHandler**: Complete functional rewrite, no try-catch blocks

3. **Architecture Tests** (`tests/Architecture/ArchitectureFitnessTests.cs`)
   - ✅ `CommandHandlers_ShouldNotContain_TryCatchBlocks()` - Prevents regression to imperative patterns
   - ✅ `TaskFinExtensions_ShouldBe_Available()` - Validates extension methods exist

### Results:
- ✅ **70 tests passing** (all existing tests maintained)
- ✅ **18 architecture tests passing** (including new ADR-006 tests)  
- ✅ **Zero try-catch blocks** in command handlers
- ✅ **Clean functional composition** throughout async operations
- ✅ **Railway-oriented programming** restored in all handlers

### Before vs After:

**Before (Imperative):**
```csharp
try {
    await _simulation.ProcessQueuedEffectsAsync();
    return FinSucc(Unit.Default);
} catch (Exception ex) {
    return FinFail<Unit>(Error.New("FAILED", ex.Message));
}
```

**After (Functional):**
```csharp
return await _simulation.ProcessQueuedEffectsAsync()
    .ToFin("PROCESS_EFFECTS_FAILED", "Failed to process queued effects");
```

### Next Steps:
Phase 2 implementation (Future): Migrate infrastructure services to return `Task<Fin<T>>` directly.

---

**Related:**
- [Block_Placement_Display_Bug_Report.md](../4_Post_Mortems/Block_Placement_Display_Bug_Report.md) - Where this issue was discovered
- [Architecture_Guide.md](../1_Architecture/Architecture_Guide.md) - Functional programming principles