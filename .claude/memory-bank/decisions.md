# Architectural Decisions - BlockLife

**Last Updated**: 2025-08-21

## 🏗️ Major Decisions

### 1. Husky.NET for Git Hooks (2025-08-21)
**Decision**: Use Husky.NET instead of manual hooks or pre-commit
**Rationale**: 
- Zero-config across 6 persona clones
- Automatic installation via dotnet restore
- Native to .NET ecosystem
- Committed .husky folder ensures consistency
**Rejected**: 
- pre-commit (Python dependency)
- Manual scripts (maintenance burden)
- Sacred Sequence (over-engineered)
**Impact**: All personas get identical hooks automatically
**Reference**: TD_039 implementation, PR #56

### 2. Branch Naming Standardization (2025-08-21)
**Decision**: Use underscores (VS_003) not hyphens (vs-003)
**Rationale**: Match Backlog.md format exactly
**Impact**: Updated CI, PR templates, design-guard
**Migration**: All old branches must be renamed

### 3. Multi-Clone Architecture (2025-08)
**Decision**: Use 6 separate git clones for persona isolation
**Rationale**: 
- Prevents merge conflicts between personas
- Allows parallel development
- Simulates real team workflow
- Standard git commands
- No worktree complexity
- Easy recovery (just re-clone)
**Trade-offs**: 
- More disk space usage
- Need to manage multiple clones
**Result**: ✅ Working well, clean separation
**ADR**: ADR-002-persona-system-architecture.md

### 4. Clean Architecture with MVP (2025-07)
**Decision**: Strict layer separation with MVP for UI
**Rationale**:
- Testable domain logic (pure C#)
- Godot integration isolated to views
- Clear boundaries and responsibilities
**Trade-offs**:
- More boilerplate code
- Learning curve for team
**Result**: ✅ Excellent testability, clear structure
**Reference**: Architecture.md

### 5. Functional Error Handling (LanguageExt)
**Decision**: Use Fin<T> for all error handling
**Rationale**:
- No exceptions in domain logic
- Composable error chains
- Explicit error paths
**Trade-offs**:
- Learning curve for functional concepts
- More verbose than try-catch
**Result**: ✅ Fewer runtime errors, better error tracking

### 6. Persona System for Development
**Decision**: 6 specialized personas with clear boundaries
**Rationale**:
- Simulates real team dynamics
- Clear ownership and responsibilities
- Prevents context switching
**Trade-offs**:
- Overhead in persona switching
- Need clear handoff protocols
**Result**: ✅ Better focus, clearer ownership

### 7. Move Block as Reference Pattern
**Decision**: One perfect implementation to copy
**Rationale**:
- Consistency across features
- Proven working pattern
- Reduces decision fatigue
**Trade-offs**:
- May not fit all use cases
- Risk of cargo cult programming
**Result**: ✅ Fast feature development, consistent codebase

## 📊 Technology Choices

### Build System: MSBuild + PowerShell
**Why**: Cross-platform support, familiar to C# developers
**Alternative Considered**: Make, CMake
**Decision Factor**: Windows-first development environment

### Test Framework: XUnit + GdUnit4
**Why**: XUnit for domain, GdUnit4 for Godot integration
**Alternative Considered**: NUnit, MSTest
**Decision Factor**: Better async support in XUnit

### DI Container: Microsoft.Extensions.DependencyInjection
**Why**: Standard, well-documented, integrated with .NET
**Alternative Considered**: Autofac, Unity
**Decision Factor**: Simplicity and standards compliance

### State Management: Service-based with DI
**Why**: Simple, testable, no hidden dependencies
**Alternative Considered**: Redux-style, Event sourcing
**Decision Factor**: Complexity not justified for current scope

## 🔄 Process Decisions

### Git Strategy: Feature Branches + Squash Merge
**Why**: Clean main history, detailed feature history
**Alternative**: GitFlow, GitHub Flow
**Decision Factor**: Simplicity with safety

### Documentation: Inline for <30 lines
**Why**: Keeps docs close to discussion
**Alternative**: Separate docs for everything
**Decision Factor**: Reduces navigation overhead

### Testing: TDD for Domain, Test-After for UI
**Why**: Domain logic critical, UI more flexible
**Alternative**: Full TDD, No TDD
**Decision Factor**: Pragmatic balance

## ❌ Rejected Approaches

### Microservices Architecture
**Rejected Because**: Over-engineering for a game
**Complexity**: 9/10
**Benefit**: 2/10 for current scope

### Event Sourcing
**Rejected Because**: Unnecessary complexity
**Complexity**: 8/10
**Benefit**: 1/10 for current needs

### Custom Build System
**Rejected Because**: MSBuild works fine
**Complexity**: 7/10
**Benefit**: 1/10 marginal gains

### Worktree-based Development (Sacred Sequence)
**Rejected Because**: Conflicts and complexity
**Replaced With**: Multi-clone architecture
**Result**: Much cleaner workflow

## 🔮 Future Decisions Pending

### Multiplayer Architecture
**Options**: Client-server, P2P, Dedicated servers
**Defer Until**: Single-player is feature complete

### Monetization Model
**Options**: Premium, F2P, Ads, DLC
**Defer Until**: Core gameplay validated

### Platform Expansion
**Options**: Mobile, Console, Web
**Defer Until**: PC version stable

---
*Document key decisions to understand "why" not just "what"*