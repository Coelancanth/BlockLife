# ADR-006 Impact Analysis: Model-First Protocol

## Executive Summary
The Model-First Implementation Protocol (ADR-006) requires updates to all personas and workflow documentation to ensure consistency. This document analyzes the impact and provides specific modification requirements.

## 🎯 Critical Changes Required

### Immediate Updates Needed
1. **Workflow.md**: Add phase-based development process
2. **All Personas**: Update work intake and handoff criteria
3. **CLAUDE.md**: Include Model-First as core directive
4. **HANDBOOK.md**: Document phase gates and testing requirements

## 📊 Persona-by-Persona Impact Analysis

### 🎨 Product Owner
**Current Role**: Define VS items and acceptance criteria
**New Responsibilities**:
- Define acceptance criteria PER PHASE
- Understand that Phase 1-3 have no UI to review
- Accept intermediate phases based on test results

**Required Changes**:
```markdown
## VS Definition Template Update
- Phase 1 Criteria: Domain rules defined
- Phase 2 Criteria: Commands process correctly  
- Phase 3 Criteria: State persists properly
- Phase 4 Criteria: UI behaves as expected
```

**Handoff Protocol Change**:
- TO Tech Lead: Include phase-specific acceptance criteria
- FROM Dev Engineer: Review phase completions, not just final

### 🏗️ Tech Lead (Current Persona)
**Current Role**: Break down VS into tasks
**New Responsibilities**:
- Enforce phase gates strictly
- Validate no phase skipping
- Review phase completion before progression
- Create phase-specific task breakdowns

**Required Changes**:
```markdown
## Task Breakdown Template
VS_XXX: [Feature Name]
├─ Phase 1: Domain Model (2h)
│   ├─ Define entities/value objects
│   ├─ Implement business rules  
│   └─ Write unit tests
├─ Phase 2: Application Layer (1h)
│   ├─ Create commands/queries
│   ├─ Implement handlers
│   └─ Write handler tests
├─ Phase 3: Infrastructure (1h)
│   ├─ Implement state service
│   ├─ Add repositories
│   └─ Write integration tests
└─ Phase 4: Presentation (2h)
    ├─ Create presenter
    ├─ Wire Godot nodes
    └─ Manual testing
```

### 💻 Dev Engineer
**Current Role**: Implement features
**New Responsibilities**:
- Complete phases sequentially
- Cannot skip to "interesting" parts
- Must have GREEN tests before proceeding
- Document phase completion in commits

**Required Changes**:
```markdown
## Implementation Checklist
□ Phase 1 Complete
  - [ ] All domain tests GREEN
  - [ ] Coverage >80%
  - [ ] Commit: "feat(X): domain model [Phase 1/4]"
□ Phase 2 Complete  
  - [ ] All handler tests GREEN
  - [ ] Error handling via Fin<T>
  - [ ] Commit: "feat(X): handlers [Phase 2/4]"
□ Phase 3 Complete
  - [ ] Integration tests GREEN
  - [ ] State verified
  - [ ] Commit: "feat(X): infrastructure [Phase 3/4]"
□ Phase 4 Complete
  - [ ] Manual testing passed
  - [ ] UI responsive
  - [ ] Commit: "feat(X): presentation [Phase 4/4]"
```

### 🧪 Test Specialist
**Current Role**: Create and maintain tests
**New Responsibilities**:
- Create phase-specific test suites
- Validate phase gates
- Ensure test categories align with phases

**Required Changes**:
```markdown
## Test Categories Alignment
- Unit Tests → Phase 1 (Domain)
- Handler Tests → Phase 2 (Application)
- Integration Tests → Phase 3 (Infrastructure)
- E2E Tests → Phase 4 (Presentation)

## New Test Validation Rules
- Phase 1: Must run in <100ms
- Phase 2: Must run in <500ms  
- Phase 3: Must run in <2s
- Phase 4: Manual or automated UI tests
```

### 🔍 Debugger Expert
**Current Role**: Fix complex bugs
**New Responsibilities**:
- Identify which phase contains the bug
- Start debugging from innermost phase
- Validate fix doesn't break outer phases

**Required Changes**:
```markdown
## Bug Investigation Protocol
1. Reproduce issue
2. Identify phase:
   - Logic error? → Phase 1
   - Command failure? → Phase 2
   - State corruption? → Phase 3
   - UI glitch? → Phase 4
3. Fix in identified phase
4. Validate all outer phases still work
```

### 🔧 DevOps Engineer
**Current Role**: Maintain CI/CD pipeline
**New Responsibilities**:
- Implement phase gate validation in CI
- Create phase-specific test runs
- Block PR merge if phases incomplete

**Required Changes**:
```markdown
## CI Pipeline Updates
stages:
  - phase1-domain    # Must pass first
  - phase2-application  # Only if phase1 passes
  - phase3-infrastructure  # Only if phase2 passes
  - phase4-presentation    # Only if phase3 passes

## Git Hooks
- pre-commit: Verify phase completion markers
- pre-push: Run phase-appropriate tests
```

## 📝 Workflow.md Modifications Required

### New Section: Phase-Based Development
```markdown
## 🔄 Model-First Implementation Protocol (ADR-006)

All features MUST be implemented in phases:

### Phase Progression (MANDATORY)
```
Domain → Application → Infrastructure → Presentation
  ↓         ↓              ↓                ↓
Tests    Handlers      Integration      UI/Manual
(100ms)   (500ms)         (2s)          (Variable)
```

### Phase Gates
- ❌ CANNOT skip phases
- ❌ CANNOT start next phase until current is GREEN
- ✅ MUST commit at each phase completion
- ✅ MUST include phase marker in commit message

### Example Timeline
- Morning: Phase 1 (Domain) - 2 hours
- Afternoon: Phase 2-3 (App/Infra) - 2 hours  
- Next Day: Phase 4 (UI) - 2 hours
```

## 📋 Backlog Impact

### VS Item Updates
```markdown
## Vertical Slices
VS_XXX: [Feature Name]
Status: Phase 2/4 Complete  ← NEW: Phase tracking
Phases:
  ✅ Domain Model (tests passing)
  ✅ Application Layer (handlers done)
  ⏳ Infrastructure (in progress)
  ⬜ Presentation (not started)
```

### TD Item Considerations
- TD for skipping phases: ALWAYS REJECT
- TD for "faster development": Point to ADR-006
- TD for phase tooling: Route to DevOps

## 🚨 Risk Mitigation

### Adoption Risks
1. **Developer Resistance**: "This is too slow!"
   - Mitigation: Show Move Block success metrics
   - Emphasize: Fewer bugs, faster refactoring

2. **Partial Implementation**: Starting Phase 4 before 1-3 done
   - Mitigation: CI blocks, code review enforcement
   - Tech Lead validates phase gates

3. **Confusion**: Not understanding phase boundaries  
   - Mitigation: Clear examples, reference implementations
   - Move Block as template

### Success Metrics
- **Bug Rate**: Expect 50% reduction in integration bugs
- **Test Speed**: Unit tests stay <1s even as codebase grows
- **Refactoring Time**: 75% faster when phases are clean
- **Onboarding**: New devs productive in 2 days vs 2 weeks

## 🎯 Rollout Plan

### Week 1: Documentation Updates
- [ ] Update all persona docs with phase responsibilities
- [ ] Modify Workflow.md with phase protocol
- [ ] Update HANDBOOK.md with implementation guide

### Week 2: Tooling
- [ ] DevOps creates CI phase gates
- [ ] Test Specialist aligns test categories
- [ ] Create phase validation scripts

### Week 3: First Implementation
- [ ] Choose simple feature for reference
- [ ] Full team follows phases
- [ ] Document lessons learned

### Week 4: Full Adoption
- [ ] All new features use Model-First
- [ ] Retrospective on process
- [ ] Adjust based on feedback

## ✅ Validation Checklist

Before marking ADR-006 as "Accepted":
- [ ] All persona docs updated
- [ ] Workflow.md includes phase protocol
- [ ] HANDBOOK.md has implementation examples
- [ ] CI pipeline validates phases
- [ ] Team trained on new process
- [ ] Reference implementation complete

## 📚 References
- [ADR-006](ADR-006-model-first-implementation-protocol.md) - The decision
- [Move Block](../../../src/Features/Block/Move/) - Reference implementation
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - Principles