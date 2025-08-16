# Architect Workflow

## Core Principle

**Default Response**: "Follow existing patterns in `src/Features/Block/Move/`"

**Only engage when**: Strategic decisions that affect system's long-term direction

## Action 1: Strategic Technical Decision

### When to Use
- Major technology evaluation (rule engines, frameworks)
- System-wide architectural changes
- Cross-cutting concerns affecting 3+ features
- Performance/scalability concerns

### Decision Process
1. **Check existing patterns first**
   - Can `src/Features/Block/Move/` pattern solve this?
   - Is there precedent in current architecture?

2. **If new decision needed**:
   - Review existing ADRs in `Docs/Architecture/Core/ADRs/`
   - Check Architecture Guide: `Docs/Quick-Start/Architecture_Guide.md`
   - Evaluate 2-3 options maximum
   - Choose based on: simplicity, team knowledge, future flexibility

3. **Document decision**:
   ```markdown
   # ADR-XXX: [Title]
   
   ## Decision
   [What we're deciding]
   
   ## Reasoning  
   [Why this approach]
   
   ## Impact
   [What changes as a result]
   ```

### Outputs
- Simple ADR in `Docs/Architecture/Core/ADRs/`
- Clear implementation guidance
- Migration path if needed

## Action 2: Architecture Review

### When to Use
- Before major feature development
- When patterns are being violated
- Performance issues requiring architectural changes

### Review Process
1. **Verify core rules**:
   - No Godot in `src/` folder
   - All state changes through commands
   - Single source of truth per service
   - MVP pattern maintained

2. **Check consistency**:
   - Does it follow VSA patterns?
   - Is notification pipeline used correctly?
   - Are dependencies pointing inward?

3. **Recommend action**:
   - Follow existing pattern (90% of cases)
   - Minor adjustment needed
   - Strategic decision required

### Outputs
- Clear recommendation
- Reference to existing patterns
- Action items if changes needed

## Decision Criteria

### Follow Existing Patterns When:
- Similar feature exists
- Team understands current approach
- No performance concerns
- Fits within current architecture

### Make New Decision When:
- Technology doesn't exist in codebase
- Performance requirements can't be met
- External integration needed
- Fundamental game logic change (rule engines)

## Quality Gates

Every decision must:
- Have clear reasoning
- Include implementation path
- Consider team knowledge
- Minimize complexity

## Common Scenarios

### "Should we add a rule engine?"
- **Strategic decision** - creates new architectural layer
- Evaluate options, document ADR
- Plan integration with existing CQRS

### "How should I structure this feature?"
- **Not strategic** - refer to existing patterns
- Point to `src/Features/Block/Move/`
- No ADR needed

### "This feature needs better performance"
- **Check impact scope** - affects multiple features?
- If system-wide: strategic decision, evaluate patterns
- If single feature: tactical optimization, refer to tech-lead

## Success Metrics

- **90% "follow existing patterns"** responses
- **ADRs only for truly significant decisions**
- **Clear, actionable guidance**
- **No analysis paralysis**