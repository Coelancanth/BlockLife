# ADR-005: Persona Completion Authority

## Status
Accepted

## Context
AI personas were marking backlog items as "completed" without user verification, effectively making autonomous decisions about work acceptance. This bypassed critical quality gates, removed user control over definition of done, and allowed work to be considered finished without proper review or testing validation.

The core issue: Personas were acting as autonomous agents rather than specialized advisors, making final judgments about their own work quality.

## Decision
We will implement a single, clear rule: **Personas cannot mark items as Done or Completed**. 

Personas are expert advisors who execute specialized work and make recommendations, but the user retains exclusive authority over work acceptance. When personas complete work, they present it for review and suggest next steps, but cannot make the final determination of completion.

## Consequences

### Positive
- User maintains complete control over quality standards and acceptance criteria
- Explicit review gates prevent defects from being marked as "complete"
- Clear separation between execution (persona) and acceptance (user)
- Improved quality through mandatory review checkpoint
- Prevents accumulation of hidden technical debt

### Negative
- Slightly slower item closure (requires user review cycle)
- User becomes potential bottleneck for completion
- Requires user availability for review and acceptance

### Neutral
- Changes workflow from autonomous to advisory model
- Shifts completion tracking from personas to user

## Alternatives Considered

### Alternative 1: Multi-tier Approval System
Create different approval levels where some personas can approve others' work.
- **Pros**: Distributed decision making, reduced user bottleneck
- **Cons**: Complex hierarchy, unclear authority boundaries, potential for circular approvals
- **Reason not chosen**: Over-engineered for the problem; adds complexity without clear benefit

### Alternative 2: Automated Quality Gates
Let personas mark complete if all automated tests pass.
- **Pros**: Faster completion, objective criteria
- **Cons**: Tests don't catch all issues, no review for design/architecture decisions
- **Reason not chosen**: Automated tests are necessary but not sufficient for completion

### Alternative 3: Conditional Completion Authority
Allow personas to mark simple items complete, require review for complex ones.
- **Pros**: Reduces review burden for trivial items
- **Cons**: Ambiguous boundaries, judgment calls about "simple" vs "complex"
- **Reason not chosen**: Creates grey areas and interpretation issues

## Implementation Notes

### Persona File Update
Add to each persona's protocol section:
```markdown
### Completion Authority Protocol
**Status Transitions I CAN Make:**
- Any Status → "In Progress" (when starting work)
- "In Progress" → Present for review (work complete, awaiting decision)

**Status Transitions I CANNOT Make:**
- ❌ Any Status → "Completed" or "Done" (only user)
- ❌ Any Status → "Approved" (only user)

**Work Presentation Format:**
When my work is ready:
"✅ **Work Complete**: [One-line summary]

**Validation Performed**:
- [x] Tests pass (if applicable)
- [x] Linting clean (if applicable)
- [x] Acceptance criteria met

**Suggested Next Step**:
→ Option A: Mark complete if satisfied
→ Option B: [Specific persona] review for [specific concern]

Awaiting your decision."
```

### Backlog Protocol Update
The backlog should reflect this with clear ownership transitions but no unilateral completion.

## References
- [HANDBOOK.md](../HANDBOOK.md) - Persona system documentation
- [Workflow.md](../../01-Active/Workflow.md) - Backlog management protocols
- Persona files in `Docs/04-Personas/` - Implementation locations