# Established Patterns
**Last Updated**: 2025-08-21

## Git Hook Auto-Installation
**Pattern**: Configure Husky in .csproj for zero-config setup
**Example**: `BlockLife.Core.csproj:22-25`
**Rationale**: Hooks auto-install on `dotnet tool restore` across all clones
**Added**: 2025-08-21

## Branch Naming Convention
**Pattern**: Use underscores for work items (VS_003 not vs-003)
**Example**: `feat/VS_003-save-system`
**Rationale**: Matches Backlog.md format exactly
**Added**: 2025-08-21

## CI Branch Freshness Check
**Pattern**: Fail PRs that are >20 commits behind main
**Example**: `.github/workflows/ci.yml:49-99`
**Rationale**: Prevents surprise conflicts during merge
**Added**: 2025-08-21

## Move Block Reference Pattern
**Pattern**: Copy src/Features/Block/Move/ for new features
**Example**: All new feature implementations
**Rationale**: Proven Clean Architecture implementation
**Added**: 2025-08-15

## Async Error Handling
**Pattern**: Use Fin<T> for all async operations
**Example**: `MoveBlockService.cs` async methods
**Rationale**: Consistent error propagation with LanguageExt
**Added**: 2025-08-10