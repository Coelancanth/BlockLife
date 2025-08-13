# Bug Post-Mortem Template

## Bug ID: [YYYY-MM-DD-ShortName]

### Summary
One sentence description of the bug and its architectural impact.

### Timeline
- **Introduced**: [Commit/Date when bug was likely introduced]
- **Discovered**: [How it was found - test failure, runtime, code review]
- **Fixed**: [Commit reference]

### Root Cause Analysis

#### Immediate Cause
What broke in the code.

#### Architectural Cause
Which architectural principle was violated:
- [ ] Dependency Inversion (concrete dependency where abstraction needed)
- [ ] Single Responsibility (class doing too much)
- [ ] Interface Segregation (fat interface)
- [ ] Explicit Dependencies (hidden dependency)
- [ ] Command/Query Separation (state change outside handler)

#### Process Cause
How did this slip through:
- [ ] Missing test coverage
- [ ] Inadequate type constraints
- [ ] Documentation gap
- [ ] Code review miss

### Impact Analysis
- **Scope**: Which layers affected (Core/Presentation/Infrastructure)
- **Blast Radius**: Other components that could have similar issues
- **Technical Debt**: What refactoring is now needed

### Fix Details
```csharp
// Before (problematic code)

// After (fixed code)
```

### Prevention Measures

#### Immediate Actions
- [ ] Add specific test case
- [ ] Update style guide rule
- [ ] Add compiler warning/analyzer

#### Systemic Changes
- [ ] Architectural constraint to enforce
- [ ] Testing strategy adjustment
- [ ] DI registration pattern change

### Lessons Learned
Key takeaway for the team about Clean Architecture in practice.

---

## Recent Post-Mortems

### 2025-01-13: Fin<T> Type Ambiguity

**Summary**: LanguageExt.Fin<T> conflicted with System.Runtime.Intrinsics.Arm.AdvSimd.Arm64

**Architectural Cause**: Violated Explicit Dependencies - implicit global using statements created hidden coupling to System namespaces.

**Prevention**: 
- Always use fully qualified names for functional types in public APIs
- Consider type aliases: `using ValidationResult = LanguageExt.Fin<Unit>;`

### 2025-01-13: DI Registration Presenter/View Coupling

**Summary**: Presenters couldn't be resolved due to circular dependency on Views

**Architectural Cause**: Violated Dependency Inversion - Presenter depending on concrete View instead of interface.

**Prevention**:
- Presenters MUST depend only on IView interfaces
- Use factory pattern for bidirectional relationships
- Register Views before Presenters in DI container

### 2025-01-13: Inconsistent Error Creation

**Summary**: Mixed Error.New(code, message) with Error.New(message) causing test failures

**Architectural Cause**: Violated Single Responsibility - error creation logic scattered across handlers.

**Prevention**:
- Centralize error creation in domain-specific error factories
- Use strongly-typed error codes enum
- Example: `BlockErrors.InvalidPosition(x, y)` not `Error.New("BLOCK_001", $"Invalid position")`