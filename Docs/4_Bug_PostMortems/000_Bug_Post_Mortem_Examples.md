# Bug Post-Mortem Examples

This document provides examples of common bug patterns and their post-mortem analyses to help with writing future post-mortems.

## Example 1: Fin<T> Type Ambiguity (2025-01-13)

**Summary**: LanguageExt.Fin<T> conflicted with System.Runtime.Intrinsics.Arm.AdvSimd.Arm64

**Architectural Cause**: Violated Explicit Dependencies - implicit global using statements created hidden coupling to System namespaces.

**Prevention**: 
- Always use fully qualified names for functional types in public APIs
- Consider type aliases: `using ValidationResult = LanguageExt.Fin<Unit>;`

## Example 2: DI Registration Presenter/View Coupling (2025-01-13)

**Summary**: Presenters couldn't be resolved due to circular dependency on Views

**Architectural Cause**: Violated Dependency Inversion - Presenter depending on concrete View instead of interface.

**Prevention**:
- Presenters MUST depend only on IView interfaces
- Use factory pattern for bidirectional relationships
- Register Views before Presenters in DI container

## Example 3: Inconsistent Error Creation (2025-01-13)

**Summary**: Mixed Error.New(code, message) with Error.New(message) causing test failures

**Architectural Cause**: Violated Single Responsibility - error creation logic scattered across handlers.

**Prevention**:
- Centralize error creation in domain-specific error factories
- Use strongly-typed error codes enum
- Example: `BlockErrors.InvalidPosition(x, y)` not `Error.New("BLOCK_001", $"Invalid position")`

## Example 4: Notification Pipeline Broken (2025-08-13)

**Summary**: Block placement commands executed successfully but UI didn't update

**Architectural Cause**: Violated Command-Query Separation - notifications weren't being published from handlers

**Prevention**:
- Always publish notifications after successful command execution
- Use static event bridge pattern for presenter notification
- Add integration tests for complete command→notification→view flow

## Example 5: Architecture Boundary Violation (Common)

**Summary**: Core project accidentally imported Godot namespace

**Architectural Cause**: Violated Dependency Rule - inner layer depending on outer layer

**Prevention**:
- Add architecture tests to prevent Godot imports in Core
- Use IDE analyzers to catch boundary violations
- Regular architecture fitness function runs in CI

---

*These examples show common patterns to help diagnose and prevent similar issues in future development.*