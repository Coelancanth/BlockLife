# Architect Agent - Documentation References

## 🗺️ Quick Navigation
**START HERE**: [DOCUMENTATION_CATALOGUE.md](../DOCUMENTATION_CATALOGUE.md) - Complete index of all BlockLife documentation

## Your Domain-Specific Documentation

### 🧠 **Your Living Wisdom Documents** (YOU OWN THESE)
- **[LWP_002_Integration_Testing_Patterns.md](../Living-Wisdom/Playbooks/LWP_002_Integration_Testing_Patterns.md)** - ⭐ **YOUR RESPONSIBILITY** ⭐
  - Integration testing architecture patterns and GdUnit4 best practices
  - Update this with new integration testing insights and solutions
  - Critical for preventing phantom test failures

### Agent-Specific Reference Documents
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
- `Docs/Shared/Architecture/Reference-Implementations/000_Vertical_Slice_Architecture_Plan.md` - VSA foundations (archived reference)
- `Docs/Backlog/templates/VS_Template.md` - Current VSA template with embedded implementation planning
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