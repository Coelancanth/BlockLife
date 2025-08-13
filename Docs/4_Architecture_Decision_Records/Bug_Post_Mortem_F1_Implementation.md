# Bug Post-Mortem: F1 Block Placement Implementation

## Date: 2025-08-13
## Feature: F1 Block Placement Vertical Slice
## Severity: High (Build Breaking)

## Summary
Multiple critical issues encountered during F1 implementation that broke compilation and tests, requiring systematic fixes to restore functionality.

## Issues Encountered

### 1. LanguageExt.Fin<T> Type Ambiguity
**Symptom**: Compiler error - ambiguous reference between custom `Fin<A>` wrapper and `LanguageExt.Fin<A>`

**Root Cause**: Attempted to create custom wrapper class around LanguageExt.Fin<T> to add marker interface

**Architecture Principle Violated**: 
- Don't wrap third-party types unnecessarily
- Favor composition over inheritance for external libraries

**Fix Applied**:
- Removed custom Fin<A> wrapper class
- Used LanguageExt.Fin<T> directly in all interfaces
- Updated ICommand interfaces to use fully qualified type names

**Prevention**:
- Always use third-party types directly unless wrapping provides clear value
- Avoid name collisions with external library types

### 2. Presenter DI Registration Issues
**Symptom**: DI container validation failure - GridPresenter requires IGridView which isn't registered

**Root Cause**: 
- Presenters were being auto-registered by MediatR as INotificationHandler
- Presenters were also manually registered in DI container
- Presenters require view dependencies that only exist at runtime

**Architecture Principle Violated**:
- Presenters should be created via factory pattern with runtime dependencies
- Don't register types that have unresolvable dependencies

**Fix Applied**:
- Removed INotificationHandler implementation from GridPresenter
- Removed manual presenter registration from GameStrapper
- Presenters now only created via PresenterFactory

**Prevention**:
- Keep presenter creation separate from DI container
- Use factory pattern for types with runtime dependencies
- Don't implement MediatR interfaces on types that shouldn't be auto-registered

**See Also**: 
- BPM-003: Detailed analysis of presenter notification handler constraints
- BPM-004: Related SceneRoot autoload configuration issues

### 3. Error Message Format Inconsistency
**Symptom**: Test failures due to error message format mismatches

**Root Cause**: 
- Error.New() used with both single parameter (message) and dual parameter (code, message) formats
- Tests expected specific formats that didn't match implementation

**Architecture Principle Violated**:
- Consistency in error handling patterns
- Clear contract between implementation and tests

**Fix Applied**:
- Standardized all Error.New() calls to single-parameter format
- Updated all test assertions to match actual error messages

**Prevention**:
- Establish clear error handling guidelines in architecture docs
- Use constants for error messages when possible
- Consider error factory methods for consistency

### 4. Missing Validation in RemoveBlockCommandHandler
**Symptom**: Test failure - "cannot remove last block" rule not being enforced

**Root Cause**: Handler was bypassing RemovalValidationRule and going directly to GridStateService

**Architecture Principle Violated**:
- All business rules must be enforced through validation rules
- Handlers should always validate before executing operations

**Fix Applied**:
- Added validation rule check before removal operation
- Ensured all validation failures return appropriate errors

**Prevention**:
- Always validate commands before execution
- Consider validation pipeline behavior for automatic validation
- Unit test both success and validation failure paths

## Lessons Learned

1. **Type System Clarity**: Don't create wrapper types that conflict with library types
2. **DI Boundaries**: Not everything needs to be in the DI container - use factories for runtime dependencies
3. **Error Consistency**: Establish and document error handling patterns early
4. **Validation First**: Always validate before executing state changes
5. **Test-Implementation Alignment**: Keep test assertions synchronized with actual implementation

## Action Items

- [x] Remove custom Fin wrapper
- [x] Fix presenter registration
- [x] Standardize error format
- [x] Add validation to RemoveBlockCommandHandler
- [ ] Document error handling patterns in architecture guide
- [ ] Create architecture tests to prevent similar issues
- [ ] Add validation pipeline behavior for automatic validation

## Time Impact
- Initial implementation: 2 hours
- Debugging and fixing: 3 hours
- Total impact: 5 hours

## Team Members Involved
- Developer: User
- AI Assistant: Claude

---
*This post-mortem serves as a learning document to prevent similar issues in future implementations.*