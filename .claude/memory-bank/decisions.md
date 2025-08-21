# Architectural Decisions
**Last Updated**: 2025-08-21

## 2025-08-21: Husky.NET for Git Hooks
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
**Reference**: TD_039 implementation

## 2025-08-21: Branch Naming Standardization
**Decision**: Use underscores (VS_003) not hyphens (vs-003)
**Rationale**: Match Backlog.md format exactly
**Impact**: Updated CI, PR templates, design-guard
**Migration**: All old branches must be renamed

## 2025-08-20: Multi-Clone over Worktrees
**Decision**: Separate repository clones for each persona
**Rationale**: 
- Complete isolation
- Standard git commands
- No worktree complexity
- Easy recovery (just re-clone)
**ADR**: ADR-002-persona-system-architecture.md

## 2025-08-15: Clean Architecture with CQRS
**Decision**: Strict separation of Core, Application, Infrastructure
**Rationale**: Testability, maintainability, Godot independence
**Reference**: Architecture.md