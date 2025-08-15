# Tech Lead Agent Workflow

## Purpose
This workflow defines the exact procedures for the Tech Lead agent when creating implementation plans, making technical decisions, and bridging business requirements to technical execution.

---

## Core Workflow Actions

### 1. Add Technical Plan to VS Item

**Trigger**: New VS item assigned or "create implementation plan for VS_XXX"

**Input Required**:
- VS item path (e.g., `Docs/Backlog/ready/VS_XXX_Feature.md`)
- Architecture guide path: `Docs/1_Architecture/Architecture_Guide.md`
- Reference implementation: `src/Features/Block/Move/`

**Steps**:
1. **Read and Analyze VS Item**
   - Read the VS item file
   - Extract acceptance criteria
   - Identify core behaviors requiring implementation
   - Check if technical section already exists

2. **Assess Technical Approach**
   - Determine if feature fits existing patterns
   - Identify new patterns needed
   - Check architectural boundaries

3. **Break Down Into Phases**
   ```
   Phase 1: Core Domain Logic (30% effort)
   - Commands and handlers
   - Domain entities
   - Business rules
   
   Phase 2: Infrastructure (20% effort)
   - Services and repositories
   - State management
   - Integration points
   
   Phase 3: Presentation (30% effort)
   - View interfaces
   - Presenters
   - UI components
   
   Phase 4: Testing & Polish (20% effort)
   - Integration tests
   - Edge cases
   - Performance optimization
   ```

4. **Identify Risks and Dependencies**
   - Technical risks (e.g., "concurrent state modifications")
   - Dependencies on other features
   - Required refactoring
   - Performance considerations

5. **Edit VS Item to Add Technical Section**
   - Use Edit tool to append technical plan to VS item
   - Add section after acceptance criteria
   - Include all phases with task lists
   - Add time estimates per phase
   - Document technical decisions

**Output Format** (appended to existing VS item):
```markdown
[existing VS content remains unchanged]

---

## Technical Implementation Plan
*Added by Tech Lead on [date]*

### Technical Approach
- Pattern: [e.g., Command/Handler]
- State Management: [approach]
- Integration: [boundaries]

### Implementation Phases

#### Phase 1: Core Domain Logic (Est: X hours)
- [ ] Create command: [CommandName]
- [ ] Create handler with business logic
- [ ] Define domain events
- [ ] Write unit tests (TDD)

#### Phase 2: Infrastructure (Est: X hours)
- [ ] Implement state service
- [ ] Create repository if needed
- [ ] Setup DI registration
- [ ] Integration tests

#### Phase 3: Presentation (Est: X hours)
- [ ] Define view interface
- [ ] Create presenter
- [ ] Implement Godot view
- [ ] Wire up UI events

#### Phase 4: Testing & Polish (Est: X hours)
- [ ] End-to-end testing
- [ ] Performance validation
- [ ] Edge case handling
- [ ] Documentation

### Risks & Mitigations
- Risk: [Description]
  Mitigation: [Approach]

### Dependencies
- Requires: [Other features/components]
- Blocks: [What depends on this]

### Estimated Total: X-Y hours
Confidence: [High/Medium/Low]
```

---

### 2. Review Technical Approach

**Trigger**: "Review technical approach for [feature/problem]"

**Steps**:
1. Analyze the problem domain
2. Identify applicable patterns from codebase
3. Consider alternatives with trade-offs
4. Recommend optimal approach
5. Document decision rationale

**Output**: Technical recommendation with reasoning

---

### 3. Estimate Complexity

**Trigger**: "Estimate complexity for VS_XXX"

**Steps**:
1. Read VS item and acceptance criteria
2. Break down into technical tasks
3. Apply estimation based on:
   - Similar past features
   - Technical complexity
   - Testing requirements
   - Integration points
4. Provide range estimate (min-max)
5. State confidence level

**Output Format**:
```
Complexity Estimate for VS_XXX:
- Optimistic: X hours
- Realistic: Y hours  
- Pessimistic: Z hours
- Confidence: [High/Medium/Low]
- Reasoning: [Key factors affecting estimate]
```

---

### 4. Identify Technical Debt

**Trigger**: "Assess technical debt for [area/feature]"

**Steps**:
1. Review code in specified area
2. Identify violations of patterns
3. Find areas needing refactoring
4. Assess impact on future development
5. Prioritize by risk/value ratio

**Output**: Prioritized list of technical debt items with impact assessment

---

### 5. Sequence Work Items

**Trigger**: "Sequence work for [list of VS items]"

**Steps**:
1. Analyze dependencies between items
2. Consider technical risk ordering
3. Optimize for incremental delivery
4. Account for learning/discovery needs
5. Create recommended sequence

**Output Format**:
```
Recommended Sequence:
1. VS_XXX - [Reason: Foundation for others]
2. VS_YYY - [Reason: Depends on XXX]
3. VS_ZZZ - [Reason: Independent, can parallelize]
```

---

## Decision Patterns

### Pattern Selection Matrix

| Scenario | Recommended Pattern | Reasoning |
|----------|-------------------|-----------|
| State changes | Command/Handler | Maintains CQRS, testable |
| Read operations | Query/Handler | Separates reads from writes |
| Cross-cutting concerns | Notification/Bridge | Decouples components |
| UI updates | Presenter pattern | Keeps views humble |
| Async operations | Fin<T> with async/await | Error handling built-in |

### Estimation Heuristics

| Task Type | Base Estimate | Modifiers |
|-----------|--------------|-----------|
| New command/handler | 2-4 hours | +50% if complex validation |
| New presenter | 3-5 hours | +100% if complex UI |
| Service implementation | 4-8 hours | +50% if external integration |
| Test suite | 2-3 hours | +100% if property tests needed |

---

## Integration Points

### With Product Owner
- Receive VS items for planning
- Clarify requirements when ambiguous
- Report technical blockers
- Negotiate scope based on complexity

### With Backlog Maintainer
- Trigger updates when phases complete
- Report actual vs estimated times
- Update status to "in_development"

### With QA/Test Engineer
- Provide test scenarios from plan
- Identify areas needing stress testing
- Document edge cases to test

---

## Quality Checklist

Before completing any action:
- [ ] Plan follows TDD workflow?
- [ ] Respects Clean Architecture?
- [ ] Estimates include testing time?
- [ ] Risks documented with mitigations?
- [ ] Dependencies clearly identified?
- [ ] Phases independently deliverable?
- [ ] Pattern consistency maintained?

---

## Common Pitfalls to Avoid

1. **Over-engineering**: Don't add complexity for future possibilities
2. **Under-estimating**: Always include test writing and refactoring time
3. **Ignoring patterns**: Check existing code before inventing new patterns
4. **Skipping risks**: Every plan needs risk assessment
5. **Tight coupling**: Ensure phases can be delivered independently

---

## Reference Documents

Always consult these when creating plans:
- Architecture Guide: `Docs/1_Architecture/Architecture_Guide.md`
- Standard Patterns: `Docs/1_Architecture/Standard_Patterns.md`
- Testing Strategy: `Docs/2_Testing_Strategy/*`
- Reference Implementation: `src/Features/Block/Move/`

---

## Response Templates

### When plan is complete:
"✅ Technical plan added to VS_XXX
- Location: Docs/Backlog/ready/VS_XXX_Feature.md
- Section added: Technical Implementation Plan
- Estimated effort: X-Y hours
- Phases: 4 (Domain, Infrastructure, Presentation, Testing)
- Key risks: [Summary]
- Ready for development"

### When clarification needed:
"❓ Need clarification on VS_XXX:
- [Specific question about requirements]
- This affects [technical decision]
- Blocking plan completion"

### When technical blocker found:
"⚠️ Technical blocker identified:
- Issue: [Description]
- Impact: [What it blocks]
- Options: [Possible solutions]
- Recommendation: [Preferred approach]"