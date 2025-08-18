# BlockLife Development Backlog

**Last Updated**: 2025-08-18

## 📖 How to Use This Backlog

### 🧠 Owner-Based Ultra-Think Protocol

**Each item has a single Owner persona responsible for decisions and progress.**

#### When You Embody a Persona:
1. **Filter** for items where `Owner: [Your Persona]`
2. **Ultra-Think** if `Status: Proposed` and you're the owner (5-15 min deep analysis)
3. **Quick Scan** for other statuses you own (<2 min updates)
4. **Update** the backlog before ending your session
5. **Reassign** owner when handing off to next persona

#### Ultra-Think Triggers:
- **Automatic**: When you own a "Proposed" item
- **Markers**: [ARCHITECTURE], [ROOT-CAUSE], [SAFETY-CRITICAL], [COMPLEX]
- **Output**: Document decision rationale directly in the item

### Default Ownership Rules
| Item Type | Status | Default Owner | Next Owner |
|-----------|--------|---------------|------------|
| **VS** | Proposed | Product Owner | → Tech Lead (breakdown) |
| **VS** | Approved | Tech Lead | → Dev Engineer (implement) |
| **BR** | New | Test Specialist | → Debugger Expert (complex) |
| **TD** | Proposed | Tech Lead | → Dev Engineer (approved) |

### Pragmatic Documentation Approach
- **Quick items (<1 day)**: 5-10 lines inline below
- **Medium items (1-3 days)**: 15-30 lines inline (like VS_001-003 below)
- **Complex items (>3 days)**: Create separate doc and link here

**Rule**: Start inline. Only extract to separate doc if it grows beyond 30 lines or needs diagrams.

### Adding New Items
```markdown
### [Type]_[Number]: Short Name
**Status**: Proposed | Approved | In Progress | Done
**Owner**: [Persona Name]  ← Single responsible persona
**Size**: S (<4h) | M (4-8h) | L (1-3 days) | XL (>3 days)
**Priority**: Critical | Important | Ideas
**Markers**: [ARCHITECTURE] [SAFETY-CRITICAL] etc. (if applicable)

**What**: One-line description
**Why**: Value in one sentence  
**How**: 3-5 technical approach bullets (if known)
**Done When**: 3-5 acceptance criteria
**Depends On**: Item numbers or None

**[Owner] Decision** (date):  ← Added after ultra-think
- Decision rationale
- Risks considered
- Next steps
```

## 🔥 Critical (Do First)
*Blockers preventing other work, production bugs, dependencies for other features*

### TD_013: Fix Drag Range Visual/Logic Mismatch [Score: 80/100]
**Status**: Done ✅
**Owner**: Dev Engineer ← Tech Lead (approved)
**Size**: S (2-3 hours for immediate fix)
**Priority**: Critical (visual/logic mismatch is a bug)
**Created**: 2025-08-18
**Completed**: 2025-08-18
**Markers**: [BUG] [UX-CRITICAL]

**What**: Fix visual/logic mismatch - visual shows square but validation uses diamond
**Why**: CRITICAL BUG - UI shows invalid moves as valid, breaking user trust

**Tech Lead Analysis** (2025-08-18):
- **CRITICAL BUG FOUND**: Visual indicator draws square (DragView.cs:310-313) but validation uses Manhattan/diamond (DragStateService.cs:104-105)
- **User Impact**: Players see they can move to corners but validation rejects it
- **Root Cause**: Mismatch between visual feedback and actual validation logic

**Immediate Fix Required** (2-3 hours):
1. Fix DragView.ShowRangeIndicators to draw diamond shape matching Manhattan distance
2. Update visual to show only valid positions (no corners at max range)
3. Add test to prevent regression

**Future Enhancement** (defer to later):
```csharp
interface IMovementShape {
    bool IsValidMove(Vector2Int from, Vector2Int to);
    IEnumerable<Vector2Int> GetValidPositions(Vector2Int from);
    void DrawShape(IRangeRenderer renderer); // Ensures visual matches logic
}
```

**Revised Approach**:
- **Phase 1** (NOW): Fix the bug - make visual match current Manhattan validation
- **Phase 2** (LATER): If needed for different block types, implement shape system

**Tech Lead Decision**:
✅ APPROVED as CRITICAL BUG FIX
- Fix visual/logic mismatch immediately
- Defer shape system abstraction until actually needed
- Principle: Fix bugs first, enhance later

**Dev Engineer Implementation** (2025-08-18):
✅ **COMPLETED** - Fixed visual to match Manhattan distance validation
- Modified DragView.cs:304-318 to store range center and distance
- Implemented DrawManhattanDiamond method (DragView.cs:402-426) drawing diamond shape
- Added regression test VisualAndLogicConsistency_UseManhattanDistance_NotChebyshev
- All 71 tests passing, no breakage

**Solution Details**:
- Visual now draws cells where Manhattan distance ≤ range (diamond shape)
- Each valid cell is highlighted individually with borders
- Corner positions at max range correctly excluded (e.g., (8,2) from (5,5) with range 3)
- Edge positions at max range correctly included (e.g., (8,5) from (5,5) with range 3)

## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*

### BR_004: AI Violated Critical Git Workflow - No Fetch/Rebase Before Push [Score: 90/100]
**Status**: Confirmed 🔥
**Owner**: Tech Lead (AI behavior correction needed)
**Size**: XS (workflow reinforcement)
**Priority**: Critical
**Created**: 2025-08-19
**Category**: Workflow/Process/AI-Behavior

**Bug Description**: 
AI assistant (Dev Engineer) pushed feature branch without following documented CRITICAL workflow steps: failed to fetch, update main, and rebase before pushing.

**What Happened**:
1. Created feature branch directly from potentially stale main
2. Made all changes and committed
3. Pushed to remote WITHOUT:
   - `git fetch origin` (checking for updates)
   - `git pull origin main` (updating local main)
   - `git rebase origin/main` (rebasing feature on latest)

**Impact**:
- **Immediate**: Potential merge conflicts on PR
- **Data Loss Risk**: Could overwrite recent main changes
- **CI Failures**: Tests passed locally but may fail against latest main
- **Duplicate Work**: May have "fixed" already-fixed bugs
- **Team Disruption**: Others may need to resolve conflicts

**Root Cause**:
- AI ignored explicit "CRITICAL" and "NEVER SKIP" warnings in CLAUDE.md
- No systematic enforcement of documented workflows
- AI optimized for task completion over process compliance

**Evidence**:
```bash
# What CLAUDE.md mandates (CRITICAL):
git fetch origin
git checkout main && git pull origin main
git checkout -b feat/your-feature

# What AI actually did:
git stash  # (from uncommitted changes on main)
git checkout -b feat/vs001-phase3-swap-mechanic
git stash pop
# ... made commits and pushed without syncing
```

**Severity**: HIGH - This undermines team workflow and can cause integration issues

**Proposed Solution**:
1. **Immediate**: Add pre-commit hook that checks if branch is up-to-date
2. **AI Training**: Reinforce that "CRITICAL" steps are non-negotiable
3. **Workflow Update**: Add checklist AI must confirm before push
4. **Consider**: Git alias that combines fetch+rebase+push safely

**Prevention**:
- AI should output each workflow step as it executes
- Add "workflow-check" command AI must run before push
- Document this incident as a lesson learned

**Done When**:
- Workflow reinforcement documented
- AI consistently follows fetch/rebase pattern
- No more stale branch pushes

**User Quote**: "I think you made a mistake by not using git fetch and git rebase before you push!"

### BR_005: Personas Should Present Options, Not Auto-Execute [Score: 95/100]
**Status**: Done ✅
**Owner**: Completed
**Completed**: 2025-08-19
**Size**: M (4-6 hours to update all personas)
**Priority**: Critical
**Created**: 2025-08-19
**Category**: Workflow/Process/Persona-Behavior
**Markers**: [WORKFLOW] [UX-CRITICAL]

**Bug Description**: 
Personas currently may auto-execute tasks without clear user consent. They should instead review backlog, create todo lists, present capabilities, and await user direction.

**Current Problems**:
1. **Lack of Transparency**: User doesn't see what persona plans to do until it's happening
2. **No User Control**: Personas may start implementing without explicit go-ahead
3. **Ultra-Think Confusion**: Automated "deep thinking" removes user agency over when to analyze deeply
4. **Missing Context**: User doesn't get clear summary of persona's responsibilities and current state

**Impact**:
- User feels loss of control over development flow
- Unexpected actions taken by personas
- Difficulty understanding what each persona can/should do
- No clear checkpoint before work begins

**Proposed Solution - New Persona Workflow**:
```markdown
When embodied, each persona should:

1. **Auto-Review Backlog** ✅
   - Scan for items assigned to them
   - Identify relevant work items
   - Note dependencies and blockers

2. **Create Todo List** ✅
   - Based on backlog items found
   - Prioritized by importance
   - Clear, actionable items

3. **Present to User** ✅
   - "I am [Persona Name]"
   - "My responsibilities: [list]"
   - "Current backlog items for me: [list]"
   - "Suggested todo list: [show todos]"
   - "Recommended next action: [specific suggestion]"

4. **Await User Signal** 🛑
   - NEVER auto-start implementation
   - Wait for user to say "proceed", "go", "start", etc.
   - User can modify todos before starting

5. **Remove Ultra-Think** ❌
   - Delete all ultra-think protocol sections
   - User decides when deep analysis needed
   - Can request "think deeply about X" explicitly
```

**Example Interaction**:
```
User: embody test specialist

AI: I am the Test Specialist for BlockLife.

**My Responsibilities:**
- Write unit, integration, and property tests
- Validate features meet acceptance criteria
- Find edge cases and stress test
- Create BR items for bugs found

**Current Backlog Items:**
- VS_002: Needs test strategy (Proposed)
- BR_002: Awaiting regression test

**Todo List Created:**
1. Review VS_002 requirements for test planning
2. Create test strategy for VS_002
3. Write regression test for BR_002
4. Update backlog with test results

**Recommended Action:** Start with VS_002 test strategy since it's blocking development.

Would you like me to proceed with this plan, or would you prefer to adjust the priorities?
```

**Files to Update**:
- All persona files in `Docs/Workflow/Personas/`
- Remove ultra-think protocol sections
- Add new workflow instructions
- Standardize presentation format

**Done When**:
- All personas follow new workflow
- No auto-execution without user signal
- Clear presentation of capabilities and plans
- Ultra-think protocol removed
- User has full control over when work begins

**User Quote**: "each persona should auto review backlog... create corresponding to-do list... tell user its responsibility and summary... but never automatically implement without user's signal"

**Tech Lead Implementation** (2025-08-19):
✅ **COMPLETED** - All 7 persona files updated with new Workflow Protocol
- Removed Ultra-Think Protocol sections from all personas
- Added standardized Workflow Protocol with 4-step process
- Each persona now: reviews backlog → creates todo → presents to user → awaits signal
- Consistent example interactions across all personas
- User maintains full control over execution

**Files Updated**:
1. product-owner.md - Focuses on VS definitions and player value
2. tech-lead.md - Technical decisions and work breakdown  
3. dev-engineer.md - Implementation planning
4. test-specialist.md - Validation and quality assurance
5. debugger-expert.md - Bug investigation priorities
6. devops-engineer.md - Infrastructure and automation
7. strategic-prioritizer.md - Strategic analysis and recommendations

**Verification**: Each persona now presents options and waits for explicit user consent before proceeding

### BR_003: AI Cannot Perform E2E Visual Testing [Score: 85/100]
**Status**: Done ✅
**Owner**: Completed
**Completed**: 2025-08-19
**Size**: XS (documentation update)
**Priority**: Important
**Created**: 2025-08-19
**Category**: Workflow/Process

**Bug Description**: 
AI assistants (Dev Engineer, Test Specialist personas) cannot actually run Godot or perform visual E2E testing. Current workflow may incorrectly assume AI can validate visual elements, animations, or user interactions.

**Impact**:
- False confidence in "tested" features
- Visual bugs only caught by user
- Workflow confusion about AI capabilities
- Potential for marking items "Done" without proper E2E testing

**Root Cause**:
- Workflow doesn't clearly distinguish between what AI can test (unit/integration) vs what requires human testing (E2E/visual)
- Test Specialist persona description may imply E2E testing capability

**Proposed Solution**:
1. **Update Workflow.md** to clarify:
   - AI handles: Unit tests, integration tests, code review, domain logic verification
   - Human handles: E2E testing, visual verification, UX validation, Godot runtime testing
   
2. **Update Test Specialist persona** to specify:
   - Can review test code and suggest test cases
   - Can verify unit/integration test results
   - CANNOT run Godot or verify visual elements
   - Must request human validation for E2E scenarios

3. **Add workflow step**: 
   - After AI marks "Code Complete", explicitly require "Human E2E Validation"
   - Items cannot be marked "Done" without human sign-off on visual elements

**Reproduction**:
- Review VS_001 Phase 3 where Test Specialist is expected to "E2E test" 
- Note that AI cannot actually see or interact with Godot UI

**Workaround**:
- Always have human user perform final E2E testing
- AI provides testing guide/checklist for human to follow

**Done When**:
- Workflow.md updated with clear AI/Human responsibilities
- Persona descriptions clarified
- No ambiguity about who performs visual testing

**Tech Lead Solution** (2025-08-19):
✅ **COMPLETED** - Elegant solution with clear separation of responsibilities

**Implementation**:
1. **Added Testing Responsibilities Matrix** to Workflow.md
   - Clear table: AI Can Do ✅ vs Human Must Do 👁️
   - Explains WHY each limitation exists
   - Visual flowchart of testing handoff protocol

2. **New Status: "Ready for Human Testing 👁️"**
   - Inserted into VS status progression flow
   - Indicates unit tests pass, visual validation needed
   - Clear handoff point between AI and human

3. **Human Testing Checklist Generation**
   - Test Specialist generates detailed E2E checklists
   - Template includes: setup, functional, visual, edge cases, performance
   - Checkboxes for human tester to follow
   - Results documented in backlog

4. **Updated Test Specialist Persona**
   - Added prominent "AI Testing Limitations" section
   - Clear list of what AI cannot do (see UI, click, animations)
   - Focus on generating test plans for humans
   - Checklist template and handoff protocol

**Elegant Aspects**:
- Leverages AI strengths (test generation) while respecting limitations
- Creates smooth handoff with no ambiguity
- Visual status indicator (👁️) makes human testing obvious
- Checklists ensure thorough human validation
- No false confidence from "AI tested" claims

### BR_001: Multi-Phase Items Incorrectly Archived Before Completion [Score: 85/100]
**Status**: New
**Owner**: Tech Lead (workflow/process decision needed)
**Size**: S (2-3 hours to update documentation and process)
**Priority**: Important (prevents work item loss)
**Found By**: Test Specialist
**Created**: 2025-08-18
**Markers**: [PROCESS-BUG] [WORKFLOW]

**What**: Multi-phase items (like VS_001 with Phase 1/2/3) get archived when individual phases complete
**Why**: Nearly lost VS_001 Phase 3 when Phase 2 was marked "Done" - causes work items to disappear

**Symptoms**:
- VS_001 Phase 2 marked as "Done" → entire VS_001 moved to archive
- VS_001 Phase 3 disappeared from backlog
- Only caught by user intervention

**Root Cause**: 
- Workflow doesn't distinguish between phase completion and item completion
- Backlog maintenance automation treats "Done" on any phase as complete item

**Proposed Fix**:
1. Multi-phase items stay in backlog until ALL phases complete
2. Use sub-statuses: "Phase 1 Done", "Phase 2 Done", etc.
3. Only archive when status is "All Phases Done"
4. Update Workflow.md with multi-phase handling rules
5. Consider separate items (VS_001a, VS_001b) instead of phases

**Done When**:
- Workflow.md updated with multi-phase rules
- Backlog template includes phase tracking guidance
- No more accidental archiving of incomplete multi-phase work

**Test Specialist Note**: This is a critical workflow bug that affects backlog integrity. Tech Lead should decide on the best approach for tracking multi-phase work items.

### VS_001 Phase 3: Swap Mechanic [Score: 75/100]
**Status**: Done ✅
**Owner**: Completed
**Size**: M (4-6 hours)
**Priority**: Important
**Created**: 2025-08-18
**Depends On**: VS_001 Phase 2 (completed)

**What**: Implement block swapping when dragging onto occupied cells
**Why**: Adds strategic depth - players can reorganize their board

**Approach**:
- Detect drop on occupied cell in CompleteDragCommand
- Validate both blocks can move (range checks)
- Swap positions with smooth animation
- Handle edge cases (boundaries, invalid swaps)

**Done When**:
- Dragging block A onto block B swaps their positions
- Both blocks animate to new positions
- Swap respects movement ranges of both blocks
- Undo-able operation (future consideration)

**Dev Engineer Implementation** (2025-08-19):
✅ **CODE COMPLETE** - Ready for review
- Modified CompleteDragCommandHandler to detect occupied cells
- Implemented swap logic with proper range validation
- Added 3-step swap process to avoid position conflicts
- Created comprehensive unit tests (all passing)
- 145/145 tests passing, no regressions
- Added block type colors for visual testing (TAB to cycle)

**Bug Fix** (2025-08-19):
- Fixed view update issue during swap
- Added explicit BlockMovedNotification publishing for both blocks
- Created post-mortem documenting notification pattern lesson

**Implementation Details**:
- Swap validates target block can reach original position (Manhattan distance ≤ 3)
- Publishes notifications for view synchronization
- Proper rollback on failure scenarios
- Tests cover: within-range swaps, out-of-range rejection, exact max range

**Test Specialist Review** (2025-08-19):
✅ **ALL TESTING COMPLETE** - Feature validated and working
- [x] Code review of CompleteDragCommandHandler changes - Well-structured 3-step swap process
- [x] Verify notification pattern fix is correct - Dual notifications ensure view sync
- [x] Unit test coverage excellent - 6 swap scenarios tested
- [x] E2E test swap with different colored blocks - ✅ Passed
- [x] Verify animations are smooth for both blocks - ✅ Passed
- [x] Test edge cases visually (max range, diagonal swaps) - ✅ Passed
- [x] Confirm TAB cycles block types correctly - ✅ Passed

**Test Analysis**:
- Swap logic correctly validates target block can reach original position
- Manhattan distance calculation consistent (≤ 3)
- Proper rollback on failure scenarios
- Unit tests cover: within-range, out-of-range, max-range boundary
- Notification pattern properly publishes for both blocks

**Recommended Property Tests** (future enhancement):
- SwapOperation_PreservesBlockCount - ensure no blocks lost
- SwapDistance_IsSymmetric - A↔B swap validation identical
- DoubleSwap_ReturnsToOriginal - reversibility property

**Status**: ✅ DONE - All tests passed, feature complete
**Completed**: 2025-08-19



## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### TD_014: Add Property-Based Tests for Swap Mechanic [Score: 65/100]
**Status**: Proposed
**Owner**: Tech Lead (needs approval)
**Size**: S (2-3 hours)
**Priority**: Ideas (quality improvement)
**Created**: 2025-08-19
**Proposed By**: Test Specialist
**Markers**: [QUALITY] [TESTING]

**What**: Implement FSCheck property tests for swap operation invariants
**Why**: Catch edge cases that example-based tests might miss, ensure mathematical properties hold

**Approach**:
1. Create `tests/Features/Block/Properties/SwapProperties.cs`
2. Implement three core property tests
3. Add custom generators for valid swap scenarios
4. Integrate with existing test suite

**Proposed Properties**:
```csharp
// 1. Swap preserves total block count
[Property]
public Property SwapOperation_PreservesBlockCount()

// 2. Swap validation is symmetric
[Property]
public Property SwapDistance_IsSymmetric()
// If A can swap with B, then B can swap with A

// 3. Double swap returns to original state
[Property]
public Property DoubleSwap_ReturnsToOriginal()
// Swapping twice = identity operation
```

**Additional Properties to Consider**:
- No blocks occupy same position after swap
- Swap respects grid boundaries
- Failed swaps leave grid unchanged
- Concurrent swaps don't corrupt state

**Done When**:
- Property tests integrated into test suite
- All properties pass with 1000+ generated cases
- Edge cases discovered are documented
- CI pipeline includes property test execution

**Test Specialist Note**: These properties would have caught the notification bug earlier through systematic testing. FSCheck's shrinking would provide minimal failing cases for easier debugging.

### TD_001: Extract Input System to Separate Feature Module [Score: 45/100]
**Status**: Approved ✓
**Owner**: Dev Engineer
**Size**: M (4-6 hours)
**Priority**: Ideas (after VS_001)
**Tech Lead Decision** (2025-08-18): Valid architectural improvement. Implement after VS_001 to understand full requirements.

**Approach**:
- Create `src/Features/Input/` module
- Extract common input handling patterns
- Centralize input state management
- Follow existing feature module structure


### TD_009: Refine Persona Command Implementation for Production [Score: 40/100]
**Status**: Approved ✓
**Owner**: DevOps Engineer
**Size**: M (4-6 hours)  
**Priority**: Ideas
**Found By**: DevOps Engineer during persona system testing
**Created**: 2025-08-18

**Tech Lead Decision** (2025-08-18):
✅ APPROVED - Valid improvements but not urgent
- Do after critical items (TD_003, TD_004)
- Focus on error handling and silent mode

**What**: Improve persona command system robustness and user experience
**Why**: Current implementation works but needs refinement for reliable production automation

**Current Issues**:
- Local vs global config precedence unclear and inconsistent
- No error handling for corrupted state files
- Manual testing required to verify which config is active
- Command output verbose for status line usage

**Approach**:
- Add config detection and validation to persona commands
- Implement graceful error handling for missing/corrupted state files
- Create diagnostic commands to show active configuration
- Add silent mode for status line integration (no console output)
- Document config precedence rules clearly

**Technical Improvements**:
- Add `--quiet` flag for status line usage
- Validate .claude/current-persona file format
- Add config source detection (global vs local)
- Implement fallback behavior for missing configs

**Done When**:
- Commands work reliably regardless of config setup
- Clear error messages for configuration issues
- Silent mode works properly with ccstatusline
- Documentation explains config precedence clearly
- No false negatives in persona detection

## 🚧 Currently Blocked
*None*

---

## 📋 Quick Reference

**Priority Decision Framework:**
1. **Blocking other work?** → 🔥 Critical
2. **Current milestone?** → 📈 Important  
3. **Everything else** → 💡 Ideas

**Work Item Types:**
- **VS_xxx**: Vertical Slice (new feature) - Product Owner creates
- **BR_xxx**: Bug Report (investigation) - Test Specialist creates, Debugger owns
- **TD_xxx**: Technical Debt (refactoring) - Anyone proposes → Tech Lead approves

*Notes:*
- *Critical bugs are BR items with 🔥 priority*
- *TD items need Tech Lead approval to move from "Proposed" to actionable*

---
*Single Source of Truth for all BlockLife development work. Simple, maintainable, actually used.*