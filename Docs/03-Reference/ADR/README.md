# Architecture Decision Records (ADRs)

This directory contains Architecture Decision Records (ADRs) for the BlockLife project. ADRs document significant architectural decisions made during development, including the context, decision, and consequences.

## What is an ADR?

An Architecture Decision Record captures:
- **Context**: The situation and forces at play
- **Decision**: What we decided to do
- **Consequences**: The results of that decision
- **Alternatives**: What else we considered

ADRs are **immutable** - once accepted, they don't change. If a decision needs to be reversed or modified, we create a new ADR that supersedes the old one.

## ADR Index

| ADR | Title | Status | Date | Summary |
|-----|-------|--------|------|---------|
| [ADR-001](ADR-001-pattern-recognition-framework.md) | Pattern Recognition Framework | Accepted | 2025-08-19 | Extensible framework for recognizing and executing game patterns (match-3, tier-ups, chains) |
| [ADR-002](ADR-002-persona-system-architecture.md) | Persona System Architecture | Accepted | 2025-08-20 | Migration from Git worktrees to multiple clones for true persona isolation |
| [ADR-003](ADR-003-ai-persona-git-workflow.md) | AI Persona Git Workflow | Accepted | 2025-08-22 | Educational three-layer decision system for git operations, trust over control |
| [ADR-004](ADR-004-single-repo-persona-context-management.md) | Single-Repo Persona Context Management | Accepted | 2025-08-23 | Memory Bank system for maintaining persona context in single repository |
| [ADR-005](ADR-005-persona-completion-authority.md) | Persona Completion Authority | Accepted | 2025-08-25 | Personas cannot mark items complete; user retains acceptance authority |

## When to Create an ADR

Create an ADR when:
- Making a significant architectural decision
- Choosing between multiple viable approaches
- Introducing new frameworks or patterns
- Making decisions that will be hard to reverse
- Establishing practices that affect the whole codebase

## Who Creates ADRs

- **Tech Lead**: Primary responsibility for architectural decisions
- **Dev Engineer**: Can propose ADRs for technical improvements
- **Any Team Member**: Can suggest an ADR-worthy decision

## ADR Lifecycle

1. **Proposed**: Initial draft, open for discussion
2. **Accepted**: Decision made and will be implemented
3. **Deprecated**: No longer relevant but kept for history
4. **Superseded**: Replaced by a newer ADR

## How to Create an ADR

1. Copy `template.md` to `ADR-XXX-brief-description.md`
2. Fill in all sections thoroughly
3. Submit for review (typically by Tech Lead)
4. Once accepted, update this README index
5. Reference the ADR in relevant code and documentation

## Template

See [template.md](template.md) for the standard ADR format.

## References

- [Michael Nygard's ADR Introduction](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
- [ADR GitHub Organization](https://adr.github.io/)
- [HANDBOOK.md](../HANDBOOK.md) - Our architectural principles and practices