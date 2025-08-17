# Post-Mortem: View Layer Lag Investigation

**Date**: 2025-08-17
**Issue**: BF_001 - First-click lag in block movement interactions
**Severity**: High - Blocking good UX
**Resolution**: Fixed - Async/await pre-warming optimization

## Executive Summary

User reported 282ms lag on first block movement despite backend executing in 0-4ms. Investigation revealed root cause was **Mono/.NET async state machine JIT compilation** occurring on first execution of `Option<T>.Match` with async lambdas. Fixed by pre-warming the async machinery during startup.

## Timeline of Investigation

### Initial Symptoms
- Backend MoveBlockCommand executes in 0-4ms (excellent performance)
- First block move shows 282ms delay in UI
- Subsequent moves are instant (0-1ms)
- Pattern consistent: first operation slow, all others fast

### Failed Hypotheses (Learning Process)

1. **V1 - Serilog Template Compilation** (Failed)
   - Hypothesis: First-time message template compilation
   - Fix: Pre-warm Serilog templates
   - Result: 295ms lag persisted

2. **V2 - Godot Tween Initialization** (Failed)
   - Hypothesis: Tween subsystem first-time initialization
   - Fix: Pre-warm Godot Tween system
   - Result: 294ms lag persisted

3. **Animation Timing** (Misdiagnosed by first debugger agent)
   - Changed animation from 400ms to 150ms
   - User reported lag still present
   - Animation timing was never the issue

## Root Cause Analysis

### Evidence That Led to Discovery

Phase 1 instrumentation revealed the smoking gun:
```
[05:11:16] "🔴" "V3_Check_DifferentPosition" took 271ms
```

This timer wrapped a simple boolean comparison:
```csharp
var isDifferentPosition = _selectedBlockPosition != Some(position);
```

A boolean check should take <1ms. The 271ms delay indicated **JIT compilation of the async state machine** specific to:
- `Option<T>.Match` with async lambdas
- First execution in Godot's Mono runtime context
- Async continuation state machine initialization

### Supporting Evidence
- Pre-warm test showed: `"PreWarm_AsyncAwait_Verify" took 268ms`
- Consistent pattern: ~270ms for first async operation in specific contexts
- Only occurs once per application lifetime

## Solution Implemented

### Enhanced Async Pre-warming

Added comprehensive pre-warming in `BlockInputManager._Ready()`:

```csharp
// 1. Pre-warm the exact Option<T>.Match async pattern
var testOption = Some(new Vector2Int(0, 0));
await testOption.Match(
    Some: async pos => {
        var differentPos = testOption != Some(new Vector2Int(1, 1));
        await Task.CompletedTask;
    },
    None: async () => await Task.CompletedTask
);

// 2. Execute dummy OnCellClicked to warm complete code path
CallDeferred(nameof(PreWarmOnCellClicked));

// 3. Wait synchronously for pre-warming to complete
preWarmTask.Wait(500);
```

### Impact
- **Before**: 282ms delay on first block move
- **After**: <1ms on all block moves (including first)
- **Tradeoff**: 270ms added to startup time (not user-visible)

## Lessons Learned

### Technical Insights

1. **Async/await has hidden initialization costs** in Mono/.NET runtime
   - JIT compilation of state machines happens on first execution
   - Can cause 200-300ms delays in complex async patterns
   - Particularly affects functional patterns like `Option<T>.Match` with async

2. **Instrumentation granularity matters**
   - Initial instrumentation was too coarse
   - V3 micro-operation timing revealed the exact line causing delay
   - Always instrument at the finest granularity when hunting performance issues

3. **Pre-warming must match exact patterns**
   - Generic `await Task.CompletedTask` wasn't sufficient
   - Had to pre-warm the specific `Option<T>.Match` async lambda pattern
   - Context matters for JIT compilation

### Process Improvements

1. **Agent context is critical**
   - Initial debugger agent attempts failed due to missing context
   - Stateless agents need complete historical evidence
   - Enhanced CLAUDE.md with comprehensive context protocol

2. **Evidence-based debugging**
   - Don't guess at root causes
   - Add instrumentation first, analyze data second
   - Multiple hypotheses may fail before finding truth

3. **User validation essential**
   - Multiple "fixes" appeared to work but didn't
   - Always validate with user before marking resolved
   - Trust user reports over assumptions

## Prevention Strategies

1. **Establish pre-warming patterns** for Godot projects:
   - Async/await machinery
   - Functional programming patterns (Option, Either, etc.)
   - Godot subsystems (Tween, Physics, etc.)

2. **Add startup performance budget**:
   - Allow up to 1 second for pre-warming operations
   - Log pre-warming timings for monitoring
   - Make pre-warming configurable for debugging

3. **Document known Mono/Godot performance gotchas**:
   - First-time async costs
   - JIT compilation patterns
   - Subsystem initialization delays

## Files Modified

- `BlockInputManager.cs` - Added comprehensive async pre-warming
- `BF_001_View_Layer_Lag_Investigation.md` - Complete investigation history
- `CLAUDE.md` - Enhanced with stateless agent context protocol

## Validation

User confirmed fix successful:
- First block move now instant
- No performance regression
- Pre-warming completes during startup

## Regression Test Requirements

Create test to ensure first-operation performance:
```csharp
[Test]
public async Task FirstBlockMove_ShouldCompleteWithin16ms()
{
    // Arrange - Fresh input manager
    var inputManager = new BlockInputManager();
    inputManager._Ready();
    
    // Act - First click operation
    var stopwatch = Stopwatch.StartNew();
    await inputManager.OnCellClicked(new Vector2Int(0, 0));
    stopwatch.Stop();
    
    // Assert
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(16),
        "First click should complete within one frame (16ms)");
}
```

## Key Takeaway

**Performance bottlenecks aren't always where you expect.** A simple boolean comparison taking 271ms revealed deep runtime initialization costs. Systematic instrumentation and evidence-based analysis are essential for finding these hidden issues.