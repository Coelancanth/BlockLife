# Backlog Workflow

## 🧠 Owner-Based Ultra-Think Protocol

### Core Principle
Each backlog item has a **single Owner** who is responsible for decisions and progress. When embodying a persona:

1. **Filter** for items you own
2. **Ultra-Think** if Status=Proposed (automatic 5-15 min deep analysis)
3. **Quick Scan** other owned items (<2 min updates)
4. **Update** backlog with decisions
5. **Reassign** owner when handing off

### Ultra-Think Triggers
- **Automatic**: Owner + Status=Proposed
- **Markers**: [ARCHITECTURE], [ROOT-CAUSE], [SAFETY-CRITICAL], [COMPLEX]
- **Output**: Decision rationale documented in item

## 🚀 OPTIMIZED: Focus + Delegation Pattern

### The Efficiency Breakthrough
**Discovery**: Personas waste 30% of time on mechanical backlog tasks instead of their core expertise.

### The Solution: Delegate to backlog-assistant
```bash
# OLD WAY (inefficient):
Tech Lead reviews → Manually updates backlog → Formats items → Archives
Time: 20 minutes (5 min thinking + 15 min mechanical work)

# NEW WAY (optimized):
Tech Lead reviews → Makes decision → Delegates to backlog-assistant
Time: 7 minutes (5 min thinking + 2 min delegation)
```

### How to Delegate
```bash
# After making your decision:
/task backlog-assistant "Update backlog:
- Move TD_013 to Critical
- Update status to Approved
- Add my decision notes
- Archive completed items"
```

### What Each Persona Delegates
| Persona | Focus On (High-Value) | Delegate (Mechanical) |
|---------|----------------------|----------------------|
| **Product Owner** | Feature definition, value prop | Creating VS items, formatting |
| **Tech Lead** | Technical decisions, architecture | Moving items, status updates |
| **Dev Engineer** | Writing code, solving problems | Progress updates, creating TD |
| **Test Specialist** | Finding bugs, validation | Creating BR items, formatting |
| **Debugger Expert** | Root cause analysis | Updating BR status, archiving |
| **DevOps** | CI/CD, automation | TD proposals, documentation |

### The Results
- **30% time savings** per persona per session
- **Better focus** on core expertise
- **Consistent formatting** across backlog
- **Reduced context switching**
- **Higher quality decisions**

## 🎯 Strategic Prioritizer - The Meta Layer

### When to Use the Strategic Prioritizer
The Strategic Prioritizer is your **architectural advisor** that helps decide WHAT to work on:

1. **Start of each work session**: "What should I work on today?"
2. **After completing a task**: "What's next?"
3. **When feeling overwhelmed**: "Help me focus"
4. **Weekly planning**: "What's the strategic view?"

### How the Prioritizer Works
```
embody strategic-prioritizer
    ↓
Scans ALL items (Backlog + Ideas + Archive)
    ↓
Analyzes with architectural knowledge
    ↓
Outputs Top 3 recommendations with reasoning
    ↓
You pick one and embody the owner persona
```

### The Prioritizer's Knowledge Evolution
- **Learns from outcomes**: Updates velocity metrics
- **Remembers failures**: Prevents repeated mistakes
- **Tracks patterns**: Knows what works
- **Resurrects items**: Brings back relevant archived work
- **Maintains memory**: PrioritizerKnowledge.md is its brain

### Integration with Workflow
```
Strategic Prioritizer: "Work on TD_003 next (Score: 95)"
    ↓
You: embody dev-engineer
    ↓
Dev Engineer: Implements TD_003
    ↓
Strategic Prioritizer: Learns from outcome, updates knowledge
```

## Work Item Types & Default Ownership

| Type | Description | Creator | Initial Owner | Handoff Flow |
|------|-------------|---------|---------------|---------------|
| **VS** | Vertical Slice (Feature) | Product Owner | Product Owner → Tech Lead → Dev Engineer | Define → Break down → Implement |
| **BR** | Bug Report | Test Specialist | Test Specialist → Debugger Expert | Discover → Investigate |
| **TD** | Technical Debt | Anyone | Tech Lead → Dev Engineer | Review → Implement |

## The Flow

```
Product Owner → Tech Lead → Dev Engineer → Test Specialist → DevOps
     (VS)       (Validate)     (BUILD)      (BR/TD)       (CI/CD)
                    ↓                           ↓
                (TD approve)            Debugger Expert (OWNS BR)
```

## VS (Vertical Slice) Flow with Ownership
```
Product Owner creates VS (Status: Proposed, Owner: Product Owner)
    ↓ [Ultra-Think: Define scope and value]
Product Owner completes definition (Owner: Tech Lead)
    ↓ [Ultra-Think: Architecture review]
Tech Lead reviews (Status: Under Review)
    ↓
[Validates: thin, independent, shippable]
    ↓                    ↓
Approved               Needs Refinement
(Owner: Dev Engineer)  (Owner: Product Owner)
    ↓                    ↓
Ready for Dev      (back to refine scope)
    ↓
Dev Engineer implements (Status: In Progress)
    ↓ [Quick Scan mode]
[Runs ./scripts/build.ps1 test locally]
    ↓
Creates PR → CI/CD runs (Owner: Test Specialist)
    ↓
Test Specialist validates (Status: Testing)
    ↓ [Ultra-Think if complex edge cases]
[Checks functionality AND code quality]
    ↓                    ↓
Passes                Quality Issues
    ↓                    (Owner: Tech Lead for TD)
CI passes & merged   Proposes TD item
    ↓                    ↓
(Status: Done)      (Continues testing)
```

## TD (Tech Debt) Flow with Ownership
```
Anyone proposes TD (Status: Proposed, Owner: Tech Lead)
(Including Test Specialist during quality validation)
    ↓ [Ultra-Think: Architecture review]
Tech Lead reviews (automatic ultra-think trigger)
    ↓
Approved                Rejected
(Owner: Dev Engineer)   (Owner: Closed)
    ↓                  ↓
Implements              Document reason
    ↓
(Status: Done)
```

## BR (Bug) Flow with Ownership
```
Test Specialist creates BR (Status: Reported, Owner: Test Specialist)
    ↓ [Quick assessment]
Hands off to Debugger (Owner: Debugger Expert)
    ↓ [Ultra-Think: Root cause analysis]
Debugger Expert investigates (Status: Investigating)
    ↓ [Deep investigation mode]
Debugger proposes fix (Status: Fix Proposed)
    ↓
User approves → (Owner: Dev Engineer)
    ↓
Dev Engineer implements (Status: Fix Applied)
    ↓
(Owner: Test Specialist) verifies
    ↓
[If significant bug]
    ↓
Debugger Expert creates post-mortem
    ↓
Debugger consolidates lessons → Updates docs
    ↓
Debugger AUTOMATICALLY archives (mandatory)
```

## Status Updates & Ownership

### Who Updates What (Based on Ownership)
- **Product Owner**: Creates VS, defines scope (owns until handed to Tech Lead)
- **Tech Lead**: Reviews all Proposed items they own, approves/rejects TD
- **Dev Engineer**: Updates items they own from Approved → In Progress → Done
- **Test Specialist**: Creates BR, validates features they own
- **Debugger Expert**: Owns all BR investigations (automatic ultra-think)
- **DevOps**: Updates CI/CD status for items they own

### Ownership Transfer Points
- VS: Product Owner → Tech Lead → Dev Engineer → Test Specialist
- BR: Test Specialist → Debugger Expert → Dev Engineer → Test Specialist
- TD: Anyone → Tech Lead → Dev Engineer
- **Anyone**: Can propose TD items (Test Specialist commonly does during testing)

### Status Progression

**VS Items:**
```
Proposed → Under Review → Ready for Dev → In Progress → Testing → Done
    ↓           ↓
    ↓    Needs Refinement
    ↓           ↓
    └───────────┘ (back to Product Owner)
```

**TD Items:**
```
Proposed → Approved → In Progress → Done
    ↓
Rejected
```

**BR Items:**
```
Reported → Investigating → Fix Proposed → Fix Applied → Verified
                ↓
        (Can loop back if fix fails)
```

## Priority Tiers

- **🔥 Critical**: Blockers, crashes, data loss, dependencies
- **📈 Important**: Current milestone, active work
- **💡 Ideas**: Future considerations

**Note**: Critical bugs are just BR items with 🔥 priority - no special "hotfix" type needed.

## Quick Rules

1. **One owner per item** - No shared responsibility
2. **Update on state change** - Not every minor step
3. **BR for bugs** - Not BF (Bug Fix)
4. **TD for quality issues** - Test Specialist proposes during testing (Tech Lead approves)
5. **User approves fixes** - Debugger can't autonomously fix
6. **Single source**: `Docs/Workflow/Backlog.md`
7. **Quality gates**: Test Specialist blocks if untestable, proposes TD if messy
8. **CI/CD gates**: All tests must pass locally (`./scripts/build.ps1 test`) before commit
9. **PR requirements**: CI must pass on GitHub before merge

## 🔧 Build Error Troubleshooting

### Common Build Errors & Solutions

| Error | Cause | Solution |
|-------|-------|----------|
| **"Type or namespace not found"** | Wrong namespace assumption | Run `Grep "class ClassName"` to find actual location |
| **"Ambiguous reference between X and Y"** | Multiple types with same name | Add type alias: `using LangError = LanguageExt.Common.Error;` |
| **"Cannot convert lambda expression"** | Match branches return different types | Ensure all branches return same type (e.g., `Unit.Default`) |
| **"Error is an ambiguous reference"** | Godot.Error vs LanguageExt.Error | Use fully qualified: `LanguageExt.Common.Error.New()` |

### Prevention Checklist
Before building after refactoring:
- [ ] Verified namespaces with `Grep "class ClassName"`
- [ ] Added type aliases for any ambiguous types
- [ ] Checked all Match branches return consistent types
- [ ] Ran `./scripts/build.ps1 build` locally first
- [ ] Fixed any namespace conflicts with type aliases

### Pre-Implementation Checklist (Context7 Integration)
Before writing code with unfamiliar APIs:
- [ ] **Query Context7 for framework documentation** (prevents assumption bugs)
  - [ ] LanguageExt error handling patterns if using `Fin<T>` or `Error`
  - [ ] MediatR handler registration if creating new handlers
  - [ ] Godot lifecycle methods if overriding Node methods
- [ ] **Verify methods exist** before overriding (`mcp__context7__get-library-docs`)
- [ ] **Check DI registration requirements** for new services
- [ ] **Review existing patterns** in `src/Features/Block/Move/`
- [ ] **Map integration points** if replacing features (find ALL old code)

### Quick Commands
```bash
# Find where a class is actually defined
Grep "class PlaceBlockCommand" src/

# Find all usages of a type
Grep "PlaceBlockCommand" --type cs

# Build locally before committing
./scripts/build.ps1 build

# Run tests to verify
./scripts/build.ps1 test
```

## Post-Mortem Lifecycle (Debugger Expert Owns)

**MANDATORY FLOW**: Create → Consolidate → Archive

1. **Create** post-mortem for significant bugs (>30min fix or systemic issue)
2. **Consolidate** lessons into:
   - Framework gotchas → QuickReference.md
   - Process improvements → Workflow.md
   - API examples → Context7Examples.md
3. **Archive AUTOMATICALLY** to `Post-Mortems/Archive/YYYY-MM-DD-Topic/`
   - Run `date` command first for folder naming
   - Include EXTRACTED_LESSONS.md
   - Track with IMPACT_METRICS.md

**Iron Rule**: "A post-mortem in active directory = Debugger Expert failure"

## Templates

- `Docs/Workflow/Templates/VerticalSlice_Template.md` - Features
- `Docs/Workflow/Templates/BugReport_Template.md` - Bugs  
- `Docs/Workflow/Templates/TechnicalDebt_Template.md` - Tech Debt
- `Docs/Post-Mortems/PostMortem_Template.md` - Post-mortems

---
*This workflow ensures clear ownership and smooth handoffs through the development lifecycle.*