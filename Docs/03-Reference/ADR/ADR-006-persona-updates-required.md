# ADR-006: Required Persona Documentation Updates

## Overview
This document provides EXACT updates required for each persona file to implement the Model-First Protocol. Each change is marked with line numbers and specific edits.

## 🎨 Product Owner Updates

### File: `Docs/04-Personas/product-owner.md`

#### Add after "Work Intake Criteria" section:
```markdown
## 📐 Model-First Protocol Responsibilities

### Phase-Based Acceptance Criteria
When creating VS items, define acceptance criteria for EACH phase:
- **Phase 1 (Domain)**: Business rules correctly implemented
- **Phase 2 (Application)**: Commands process as expected
- **Phase 3 (Infrastructure)**: State persists correctly
- **Phase 4 (Presentation)**: UI behaves as designed

### Phase Review Protocol
- Review test results for Phases 1-3 (no UI yet)
- Only review UI in Phase 4
- Trust Tech Lead's phase gate validations

### VS Template Update
```
VS_XXX: [Feature Name]
Acceptance by Phase:
  1. Domain: [What rules must work]
  2. Application: [What commands must do]
  3. Infrastructure: [What must persist]
  4. Presentation: [What user sees]
```
```

## 🏗️ Tech Lead Updates

### File: `Docs/04-Personas/tech-lead.md`

#### Replace "Standard Phase Breakdown" section (lines 250-275):
```markdown
## Standard Phase Breakdown (Model-First Protocol)

### Phase 1: Domain Logic [GATE: All tests GREEN]
- Write failing domain tests
- Implement pure C# business logic
- No dependencies, no Godot, no services
- Fin<T> for error handling
- **Commit**: `feat(X): domain model [Phase 1/4]`

### Phase 2: Application Layer [GATE: Handlers work]
- Write handler tests
- Implement CQRS commands/queries
- Wire up MediatR pipeline
- Mock repositories only
- **Commit**: `feat(X): handlers [Phase 2/4]`

### Phase 3: Infrastructure [GATE: Integration passes]
- Write integration tests
- Implement state services
- Add real repositories
- Verify data flow
- **Commit**: `feat(X): infrastructure [Phase 3/4]`

### Phase 4: Presentation [GATE: UI works]
- Create MVP presenter
- Wire Godot signals
- Manual testing in editor
- Performance validation
- **Commit**: `feat(X): presentation [Phase 4/4]`

### Phase Gate Enforcement
- Review each phase completion
- Block progression if tests fail
- Validate commit messages include phase markers
- No exceptions for "simple" features
```

#### Add to "TD Approval" section:
```markdown
### TD Related to Phases
- **"Skip Phase 1"**: ALWAYS REJECT - violates ADR-006
- **"Combine phases"**: ALWAYS REJECT - breaks isolation
- **"UI first for demo"**: ALWAYS REJECT - technical debt trap
- **"Phase tooling improvement"**: Route to DevOps
```

## 💻 Dev Engineer Updates

### File: `Docs/04-Personas/dev-engineer.md`

#### Add after "Core Process" section:
```markdown
## 🔄 Model-First Implementation (MANDATORY)

### Your Phase Workflow
1. **Receive VS from Tech Lead** with phase breakdown
2. **Start Phase 1**: Pure domain only
3. **Run tests**: Must be GREEN before proceeding
4. **Commit with marker**: `feat(X): domain [Phase 1/4]`
5. **Proceed sequentially** through phases
6. **Never skip ahead** even if "obvious"

### Phase Checklist Template
```bash
# Phase 1 Checklist
□ Domain entities created
□ Business rules implemented
□ Unit tests passing (100%)
□ No external dependencies
□ Committed with phase marker

# Phase 2 Checklist  
□ Commands/queries created
□ Handlers implemented
□ Handler tests passing
□ Fin<T> error handling
□ Committed with phase marker

# Phase 3 Checklist
□ State service implemented
□ Repositories working
□ Integration tests passing
□ Data flow verified
□ Committed with phase marker

# Phase 4 Checklist
□ Presenter created
□ Godot nodes wired
□ Manual testing complete
□ Performance acceptable
□ Committed with phase marker
```

### Common Phase Violations (DON'T DO)
- ❌ Creating Godot scenes in Phase 1
- ❌ Adding database in Phase 2
- ❌ Skipping tests to "save time"
- ❌ Combining phases in one commit
- ❌ Starting Phase 4 for "quick demo"
```

## 🧪 Test Specialist Updates

### File: `Docs/04-Personas/test-specialist.md`

#### Replace test categories section:
```markdown
## Test Categories by Phase

### Phase 1: Domain Tests
- **Type**: Unit tests only
- **Speed**: <100ms per test
- **Dependencies**: None
- **Coverage**: >80% required
- **Location**: `Tests/Unit/Domain/`

### Phase 2: Handler Tests  
- **Type**: Unit with mocked repos
- **Speed**: <500ms per test
- **Dependencies**: Mocked only
- **Coverage**: All handlers
- **Location**: `Tests/Unit/Handlers/`

### Phase 3: Integration Tests
- **Type**: Real services, test DB
- **Speed**: <2s per test
- **Dependencies**: Infrastructure
- **Coverage**: Data flow paths
- **Location**: `Tests/Integration/`

### Phase 4: UI Tests
- **Type**: Manual or E2E
- **Speed**: Variable
- **Dependencies**: Full Godot
- **Coverage**: User scenarios
- **Location**: `Tests/E2E/`

### Phase Gate Validation
When Dev completes a phase:
1. Run phase-specific tests only
2. Validate speed requirements
3. Check coverage thresholds
4. Approve phase completion
5. Allow next phase to start
```

## 🔍 Debugger Expert Updates

### File: `Docs/04-Personas/debugger-expert.md`

#### Add bug investigation section:
```markdown
## Phase-Aware Debugging

### Bug Localization Protocol
1. **Identify symptoms** in UI/behavior
2. **Work backwards** through phases:
   - UI issue? Start Phase 4
   - State wrong? Check Phase 3
   - Command fails? Check Phase 2
   - Logic error? Check Phase 1
3. **Fix innermost phase** first
4. **Validate outward** to ensure fix propagates

### Phase-Specific Bug Patterns
| Symptom | Likely Phase | Investigation |
|---------|--------------|---------------|
| Wrong calculation | Phase 1 | Check domain logic |
| Command timeout | Phase 2 | Handler implementation |
| Data not saved | Phase 3 | Repository/service |
| UI not updating | Phase 4 | Presenter/signals |

### Debugging Commands by Phase
```bash
# Phase 1: Run domain tests only
dotnet test --filter Category=Domain

# Phase 2: Run handler tests
dotnet test --filter Category=Handlers

# Phase 3: Check integration
dotnet test --filter Category=Integration

# Phase 4: Manual in Godot editor
```
```

## 🔧 DevOps Engineer Updates

### File: `Docs/04-Personas/devops-engineer.md`

#### Add CI/CD section:
```markdown
## Phase Gate CI/CD Implementation

### Pipeline Configuration
```yaml
stages:
  - phase-1-domain
  - phase-2-application  
  - phase-3-infrastructure
  - phase-4-presentation

phase-1-domain:
  script:
    - dotnet test --filter Category=Domain
    - verify-coverage.ps1 -Phase 1 -Threshold 80
  rules:
    - if: $CI_COMMIT_MESSAGE =~ /\[Phase 1/
    
phase-2-application:
  needs: ["phase-1-domain"]
  script:
    - dotnet test --filter Category=Handlers
  rules:
    - if: $CI_COMMIT_MESSAGE =~ /\[Phase 2/
```

### Git Hook Implementation
```bash
# .husky/pre-commit
#!/bin/sh
# Verify phase marker in commit message
if ! grep -q "\[Phase [1-4]/4\]" "$1"; then
  echo "❌ Commit must include phase marker: [Phase X/4]"
  exit 1
fi

# Verify tests for current phase
PHASE=$(grep -oP "Phase \K[0-9]" "$1")
./scripts/test/phase-$PHASE.ps1 || exit 1
```

### Use Existing Test Commands
No new scripts needed! Use our existing test infrastructure:
- **Phase 1**: `dotnet test --filter Category=Unit`
- **Phase 2**: `dotnet test --filter Category=Handlers`
- **Phase 3**: `dotnet test --filter Category=Integration`
- **Phase 4**: Manual testing in Godot editor
- **Quick validation**: `./scripts/test/quick.ps1`
- **Full validation**: `./scripts/core/build.ps1 test`
```

## 📋 CLAUDE.md Updates

Add after Context7 section:
```markdown
## 🔄 MANDATORY: Model-First Protocol (ADR-006)

**YOU MUST implement all features in strict phases:**
1. Domain (Pure C#) → 2. Application (Handlers) → 3. Infrastructure (Services) → 4. Presentation (UI)

**NEVER:**
- Skip phases for "simple" features
- Start with UI for "quick demos"
- Combine phases to "save time"
- Proceed without GREEN tests

**ALWAYS:**
- Complete each phase before starting next
- Commit with phase markers: `[Phase X/4]`
- Run phase-appropriate tests
- Follow Move Block pattern as reference

**Reference**: [ADR-006](Docs/03-Reference/ADR/ADR-006-model-first-implementation-protocol.md)
```

## 📚 HANDBOOK.md Updates

Add new section after Architecture:
```markdown
## Model-First Implementation Protocol

### Why Model-First?
Our codebase follows Clean Architecture with strict phase-based development. This prevents:
- UI bugs masking logic errors
- Slow test suites
- Refactoring nightmares
- Integration complexity

### The Four Phases
1. **Domain**: Pure business logic, no dependencies
2. **Application**: Commands/handlers, mocked repos
3. **Infrastructure**: Real services, data persistence
4. **Presentation**: UI/Godot integration

### Phase Gates
Each phase must have:
- All tests GREEN
- Appropriate test speed (<100ms, <500ms, <2s)
- Proper commit marker
- Tech Lead validation

### Reference Implementation
See `src/Features/Block/Move/` for perfect example of phase separation.
```

## ✅ Implementation Checklist

### Immediate Actions
1. [ ] Update all 6 persona files with changes above
2. [ ] Update CLAUDE.md with Model-First directive
3. [ ] Update HANDBOOK.md with protocol details
4. [ ] Update commit message guidelines
5. [ ] Configure CI to check phase markers

### Validation Steps
1. [ ] All personas understand their phase responsibilities
2. [ ] Workflow.md includes phase progression
3. [ ] Test categories align with phases
4. [ ] CI enforces phase gates
5. [ ] Commit hooks validate phase markers

### Success Metrics
- Zero phase-skipping violations in first month
- 50% reduction in integration bugs
- Test suite stays <5s total
- Refactoring time reduced by 75%

## 🚨 Critical Reminders

**For User/Team:**
- This is a BREAKING CHANGE in workflow
- Requires team training session
- First few features will be slower
- Long-term benefits are massive

**For Claude/AI:**
- ALWAYS enforce phases
- NEVER allow shortcuts
- Reference ADR-006 when challenged
- Use Move Block as example