# Extracted Lessons: Async JIT Compilation Lag
**Migration Date**: 2025-08-18 (from legacy archive)
**Original Date**: 2025-08-17
**Issue**: BF_001 - First-click lag (282ms)

## 📚 Lessons Already Consolidated

### Key Discovery
**.NET/Mono JIT compilation** causes first-execution lag for async state machines, especially with functional patterns like `Option<T>.Match` with async lambdas.

### Lessons Applied (Previously)
This was already consolidated - checking where lessons were applied:

1. **Performance patterns** - Pre-warming async machinery
2. **Debugging approach** - Instrumentation-driven investigation
3. **Framework understanding** - JIT compilation behavior

## 🎯 Critical Insights

### The Pattern
- First execution: 282ms (JIT compilation)
- Subsequent executions: 0-1ms (already compiled)
- Affects: Async lambdas in functional constructs

### The Solution
Pre-warm async state machines during startup:
```csharp
// In startup/initialization
await WarmupAsync();
```

## 💡 Why This Matters

This post-mortem revealed a non-obvious performance issue that affects ALL first-time async operations in functional code. The pre-warming pattern should be standard practice.

## 📊 Impact

- **User Experience**: Eliminated 282ms first-click lag
- **Pattern Recognition**: Now we know to look for JIT issues
- **Standard Practice**: Pre-warming added to initialization

## Status
**LEGACY MIGRATION** - Already consolidated, moved to new archive structure for consistency.