# Post-Mortem: Drag-and-Drop Phase 1 Implementation
**Date**: 2025-01-18  
**Feature**: VS_001 - Drag-to-Move Block System (Phase 1)  
**Duration**: ~2 hours  
**Result**: Backend complete, UI integration pending

## Executive Summary
Successfully implemented the backend for a drag-and-drop system to replace click-then-move mechanics. Encountered and resolved several technical challenges around error handling, dependency injection, and test assertions. The implementation follows established patterns and is ready for UI integration.

## What Went Well ✅

### 1. Pattern Reuse
- **Decision**: Used Move Block implementation as reference
- **Result**: Consistent architecture, minimal design debates
- **Learning**: Established patterns are goldmines - use them ruthlessly

### 2. Phased Approach
- **Decision**: Split into 3 independently shippable phases
- **Result**: Phase 1 backend complete and testable without UI complexity
- **Learning**: Incremental delivery reduces risk and complexity

### 3. Functional Error Handling
- **Decision**: Used `Fin<Unit>` throughout for error handling
- **Result**: No exceptions, clean error propagation
- **Learning**: Functional patterns work well for command/handler architectures

## What Went Wrong ❌

### 1. LanguageExt Error Message Confusion
**Problem**: Tests failed because `Error.Message` returns just the error code, not the full message  
**Root Cause**: Misunderstanding of LanguageExt.Common.Error API  
**Fix**: Changed test assertions to check for error codes instead of message text  
**Prevention**: Document LanguageExt error handling patterns in standards

### 2. MediatR Auto-Discovery Issues
**Problem**: DragPresenter was auto-discovered as INotificationHandler, causing DI failures in tests  
**Root Cause**: Presenter implemented INotificationHandler but required IDragView (not available in tests)  
**Fix**: Removed INotificationHandler from presenter - notifications aren't needed for MVP pattern  
**Prevention**: Presenters should only handle view events, not domain notifications

### 3. PresenterBase Method Names
**Problem**: Used OnViewAttached/OnViewDetached which don't exist  
**Root Cause**: Assumed methods without checking actual PresenterBase implementation  
**Fix**: Changed to Initialize/Dispose which are the actual methods  
**Prevention**: Always verify base class contracts before overriding

### 4. Block Creation in Tests
**Problem**: Used non-existent BlockFactory  
**Root Cause**: Assumed factory pattern without checking test utilities  
**Fix**: Used existing BlockBuilder pattern  
**Prevention**: Check test infrastructure before writing new tests

## Technical Decisions & Rationale

### Why Separate Commands for Drag Lifecycle?
**Decision**: StartDragCommand, CompleteDragCommand, CancelDragCommand (not single DragCommand)  
**Rationale**: 
- Clear separation of concerns
- Each command has different validation rules
- Easier to test individual operations
- Follows CQRS single responsibility

### Why DragStateService as Singleton?
**Decision**: Singleton service to track drag state  
**Rationale**:
- Only one drag can be active at a time
- State must persist across command executions
- Prevents concurrent drag operations
- Simple and effective

### Why Remove Notification Handlers from Presenter?
**Decision**: Presenter doesn't implement INotificationHandler  
**Rationale**:
- Presenters coordinate view/command, not domain events
- Removes dependency on view in unit tests
- Keeps presenter focused on single responsibility
- UI updates happen through view events, not notifications

## Metrics

- **Lines of Code**: ~800 (including tests)
- **Files Created**: 13
- **Tests Written**: 9 (all passing)
- **Compilation Errors Fixed**: 15
- **Test Failures Resolved**: 5

## Lessons Learned 📚

### 1. Error Handling in Functional Patterns
- `Error.New(code, message)` - the Message property returns the code
- Use `ToString()` or custom extraction for full error details
- Consider creating error types instead of string codes

### 2. Dependency Injection Boundaries
- MediatR auto-discovers all handlers - be careful with interfaces
- Presenters shouldn't be auto-discovered (created by factory)
- Test service registration separately from business logic

### 3. MVP Pattern Clarity
- Presenters coordinate between view and commands
- Don't mix domain event handling with view coordination
- Keep view interface focused on UI operations

### 4. Test-First Benefits
- Caught DI issues before UI integration
- Forced clean separation of concerns
- Made refactoring safer

## Action Items 📋

1. **Document LanguageExt patterns** in Standards.md
   - Error creation and extraction
   - Fin<T> test assertions
   - Common pitfalls

2. **Create presenter template** showing correct pattern
   - Constructor accepting view
   - Initialize/Dispose lifecycle
   - Event subscription pattern

3. **Update test utilities documentation**
   - BlockBuilder usage
   - Service setup patterns
   - Mock strategies

## Recommendations for Phase 2-3

1. **Keep phases independent** - Phase 2 (range) shouldn't break Phase 1
2. **Add feature flags** - Allow enabling/disabling phases at runtime
3. **Test UI integration early** - Don't wait until Phase 3
4. **Consider swap preview** - Visual feedback before swap execution

## Quote of the Implementation
> "DRAG_IN_PROGRESS" != "Another drag operation is already in progress"  
> — The test that taught us about Error.Message

## Final Verdict
✅ **Success with lessons** - Backend solid, patterns followed, ready for UI. The issues encountered were learning opportunities that will improve future implementations.

---
*"In the end, we drag not because it is easy, but because click-then-move is harder." - Anonymous Dev*