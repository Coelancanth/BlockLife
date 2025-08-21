# ADR-003: AI Persona Git Workflow - Educational Over Restrictive

## Status
Accepted

## Context
AI personas working with BlockLife were experiencing ambiguity around fundamental git operations:
- "New work = new branch" was oversimplified and undefined
- No clear definition of "atomic commits" in the BlockLife context
- Personas didn't know when to create branches vs continue on existing ones
- Traditional git hooks assumed human developers making mistakes

The previous approach of adding more validation and restrictions was causing:
- Slow commit times (>2 seconds per commit)
- Frustrated development flow with redundant checks
- AI personas generating clean code being treated like error-prone humans
- Quality gates that couldn't prevent issues retroactively (pre-push can't fix bad commits)

## Decision
We will implement an **educational three-layer decision system** for AI persona git workflows:

1. **Educational Guidance Layer**: Pre-commit hooks that teach rather than block
2. **Intelligence Layer**: Branch status checking with context-aware recommendations
3. **Protocol Layer**: Clear decision trees and criteria for branch/commit decisions

Key principles adopted:
- **Trust over Control**: AI generates clean code by default
- **Education over Enforcement**: Guide decisions at decision time, not validation time
- **Performance over Paranoia**: Remove redundant validation layers
- **Pragmatism over Purity**: Allow flexibility for quick fixes and real-world scenarios

## Consequences

### Positive
- **85% faster commits** (~0.3s vs previous ~2s) with zero quality reduction
- **Clear decision criteria** eliminate ambiguity for AI personas
- **Context-aware workflows** through branch status intelligence
- **Educational approach** improves AI persona understanding over time
- **Reduced friction** maintains developer flow state
- **Atomic commit clarity** with single-sentence rule

### Negative
- **Less enforcement** means relying more on AI persona judgment
- **Requires discipline** from personas to follow protocols
- **Initial learning curve** for personas adapting to decision trees
- **Manual branch cleanup** still needed (though scripts help)

### Neutral
- **Shifts responsibility** from tools to AI personas
- **Changes debugging approach** - fewer gates mean issues caught later
- **Documentation dependency** - protocols must be kept current
- **Performance monitoring** needed to ensure gains are maintained

## Alternatives Considered

### Alternative 1: Restrictive Validation Hooks
Traditional approach with strict pre-commit validation
- **Pros**: Catches all issues before commit, enforces standards absolutely
- **Cons**: Slow (2+ seconds), blocks legitimate work, treats AI like humans
- **Reason not chosen**: AI personas generate clean code; validation is redundant

### Alternative 2: Fully Automated Branching
Automatic branch creation based on work item detection
- **Pros**: No decisions needed, consistent branch names, foolproof
- **Cons**: Inflexible, can't handle edge cases, prevents quick fixes
- **Reason not chosen**: Real development needs pragmatic flexibility

### Alternative 3: No Guidance System
Rely entirely on persona training and documentation
- **Pros**: Simplest approach, no tooling needed, maximum flexibility
- **Cons**: Inconsistent behavior, no learning reinforcement, ambiguity remains
- **Reason not chosen**: Personas benefit from just-in-time guidance

### Alternative 4: Complex Multi-Stage Validation
Comprehensive validation at commit, push, and PR stages
- **Pros**: Multiple safety nets, catches everything eventually
- **Cons**: Extremely slow, redundant checks, complex maintenance
- **Reason not chosen**: Over-engineering for AI-generated code

## Implementation Notes

### Atomic Commit Definition
```
An atomic commit does exactly ONE logical thing that can be described in a single sentence.
```

### Branch Decision Quick Reference
```
On main? → Always create feature branch
Quick fix (<30min, <3 files)? → Can stay on current branch
Different work item? → New branch
Different persona context? → New branch
Multi-day work? → New branch
Discovered unrelated issue? → New branch
```

### Key Scripts
- `scripts/branch-status-check.ps1` - Provides branch lifecycle intelligence
- `scripts/branch-cleanup.ps1` - Safe cleanup for merged branches
- `.husky/pre-commit` - Educational guidance (not enforcement)

### Integration Points
1. Persona embodiment triggers branch status check
2. Pre-commit hook provides atomic commit reminders
3. Memory Bank tracks branch alignment with work items
4. Branch cleanup after PR merge completion

## References
- [BranchAndCommitDecisionProtocols.md](../../02-Design/BranchAndCommitDecisionProtocols.md) - Full implementation details
- [TD_055](../../01-Active/Backlog.md) - Original problem statement and solution
- [HUSKY_GUIDE.md](../../../scripts/HUSKY_GUIDE.md) - Git hooks documentation
- [Memory Bank activeContext.md](../../../.claude/memory-bank/activeContext.md) - DevOps session insights on AI paradigm shift