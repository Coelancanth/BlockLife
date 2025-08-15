# Architect Agent - Documentation References

## Your Domain-Specific Documentation
Location: `Docs/Agent-Specific/Architect/`

- `core-architecture.md` - Core architectural principles and patterns
- `critical-patterns.md` - Critical implementation patterns (DI, CQRS, MVP)

## Shared Documentation You Should Know

### Architecture Guides
- `Docs/Shared/Architecture/Architecture_Guide.md` - Complete architecture overview
- `Docs/Shared/Architecture/Standard_Patterns.md` - Standard implementation patterns
- `Docs/Shared/Architecture/Integration_Testing_Guide.md` - Testing architecture patterns

### Architecture Decision Records
- `Docs/Shared/ADRs/` - All architectural decision records
- Focus on: ADR_007 (Functional Validation), ADR_008 (Rule Engine Architecture)

### Post-Mortems for Architecture Insights
- `Docs/Shared/Post-Mortems/Architecture_Stress_Testing_Lessons_Learned.md` - Critical production lessons
- `Docs/Shared/Post-Mortems/Critical_Architecture_Fixes_Post_Mortem.md` - Major architecture fixes
- `Docs/Shared/Post-Mortems/Integration_Test_Architecture_Deep_Dive.md` - Testing architecture lessons

### Implementation References
- `Docs/Shared/Implementation-Plans/00_Vertical_Slice_Architecture_Plan.md` - VSA foundations
- `src/Features/Block/Move/` - Gold standard implementation example

## Quick Architecture Validation Checklist

When making architectural decisions, reference:
1. Clean Architecture boundaries (no Godot in Core)
2. Single Source of Truth principle (F1 stress test lesson)
3. CQRS command/query separation
4. MVP pattern with humble presenters
5. Dependency injection container patterns

## Common Architecture Questions
- "Should this be shared or slice-specific?" → VSA refactoring agent
- "How do I enforce this architecturally?" → Architecture fitness tests
- "What's the testing strategy?" → QA agent collaboration