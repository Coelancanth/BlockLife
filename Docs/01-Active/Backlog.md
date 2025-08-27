# BlockLife Development Backlog

**Last Updated**: 2025-08-28 00:50
**Last Aging Check**: 2025-08-22
> 📚 See BACKLOG_AGING_PROTOCOL.md for 3-10 day aging rules

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 017 (Last: BR_016 - 2025-08-26 23:25)
- **Next TD**: 090 (Last: TD_089 - 2025-08-28 00:50)  
- **Next VS**: 010 (Last: VS_009 - 2025-08-28 00:38)

**Protocol**: Check your type's counter → Use that number → Increment the counter → Update timestamp

## 📖 How to Use This Backlog

### 🧠 Owner-Based Protocol

**Each item has a single Owner persona responsible for decisions and progress.**

#### When You Embody a Persona:
1. **Filter** for items where `Owner: [Your Persona]`
3. **Quick Scan** for other statuses you own (<2 min updates)
4. **Update** the backlog before ending your session
5. **Reassign** owner when handing off to next persona


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

### TD_089: Session Analytics System
**Status**: Proposed
**Owner**: Tech Lead → Dev Engineer  
**Size**: S (3-4h)
**Priority**: Critical (blocks informed balance decisions)
**Created**: 2025-08-28 00:50

**What**: Local JSON session logging for economy analysis
**Why**: Can't balance without data - need to see actual resource flow patterns

**Implementation Approach**:
- Create `SessionAnalyticsService` that hooks into game events
- Log all actions with timestamps and turn numbers
- Track resource changes and grid state
- Save JSON files to `Analytics/Sessions/` folder
- Include session summary for quick analysis

**Data to Capture**:
```json
{
  "sessionId": "uuid",
  "startTime": "ISO-8601",
  "events": [
    {"turn": 1, "action": "place", "details": {...}},
    {"turn": 1, "action": "match", "reward": {"Money": 30}}
  ],
  "economySnapshots": [
    {"turn": 10, "resources": {...}, "gridDensity": 0.35}
  ],
  "summary": {
    "totalTurns": 47,
    "totalMatches": 23,
    "totalMerges": 8,
    "maxChain": 4,
    "finalResources": {...}
  }
}
```

**Done When**:
- Every player action logged with context
- Resource flow tracked per turn
- Session files auto-save to Analytics folder
- Can aggregate data across multiple sessions
- Test with 10+ recorded sessions

**Product Owner Notes**:
- This comes BEFORE VS_009 so we can measure transmutation's impact
- Enables A/B testing (sessions with/without features)
- Real data > assumptions for balance decisions

---

### VS_009: Transmutation System
**Status**: Ready for Dev (Phase 0/4)
**Owner**: Tech Lead
**Size**: M (4-6h)
**Priority**: Critical
**Created**: 2025-08-28 00:38
**Reviewed**: Pending Tech Lead breakdown

**What**: Allow players to transmute blocks of different types into strategic combinations
**Why**: Adds critical strategic depth - transforms matching from reactive to planning gameplay

**Player Experience**:
- See 3 different block types on grid (e.g., Work + Study + Health)
- Hold Shift and click 3 blocks to select for transmutation
- Selected blocks highlight with pulsing outline
- Click "Transmute" button (or press T) to execute
- 3 blocks combine into 1 higher-tier block of player's CHOICE

**Core Mechanic**:
- Requires 3 blocks of DIFFERENT types (not same type like merge)
- Player chooses resulting block type from the 3 inputs
- Result is 1 tier higher than lowest input tier
- Position: Center block of selection (like merge)
- Cost: Small resource fee (10 Money per transmutation)

**Phase Breakdown (Model-First Protocol)**:

#### Phase 1: Domain Model (1.5h)
**Acceptance**: Transmutation rules work in pure C#
- Create `TransmutationPattern` domain model
- Validation: 3 different types required
- Calculate result tier (min input tier + 1)
- Unit tests for all combinations
**Commit**: `feat(VS_009): transmutation domain model [Phase 1/4]`

#### Phase 2: Application Layer (1h)
**Acceptance**: Commands process transmutation requests
- Create `TransmuteBlocksCommand` with 3 positions + chosen type
- Handler validates and executes transmutation
- Integration with existing pattern system
- Handler tests with mock grid
**Commit**: `feat(VS_009): transmutation commands [Phase 2/4]`

#### Phase 3: Infrastructure (1h)
**Acceptance**: State updates correctly
- Update GridStateService for transmutation
- Track transmutation count for stats
- Integration tests with real grid
**Commit**: `feat(VS_009): transmutation state [Phase 3/4]`

#### Phase 4: Presentation (1.5h)
**Acceptance**: Players can transmute via UI
- Shift+Click selection mode (3 blocks max)
- Visual selection feedback (pulsing outline)
- "Transmute" button appears when valid
- Type selection popup/dropdown
- Success animation (convergence effect)
**Commit**: `feat(VS_009): transmutation UI [Phase 4/4]`

**Done When**:
- Can select 3 different block types
- Can choose resulting block type
- Transmutation creates higher tier block
- Visual feedback throughout process
- 20+ tests covering domain/application/integration

**Depends On**: None (uses existing pattern infrastructure)

**Product Owner Notes**:
- This is THE strategic feature that makes the game interesting
- Without this, players just react to RNG
- With this, players PLAN their moves 3-4 turns ahead
- Creates meaningful choice: "Do I match for resources or transmute for positioning?"

---



## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*



## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*


### TD_081: Add Merge System Test Coverage
**Status**: Approved
**Owner**: Test Specialist
**Size**: S (2-3h)
**Priority**: Important
**Created**: 2025-08-26 20:20
**Complexity Score**: 2/10 (straightforward testing work)

**What**: Add missing test coverage for merge pattern execution system
**Why**: Core game mechanic has only edge case tests, missing actual merge logic tests

**Current State** (verified by Tech Lead):
- MergePatternExecutorBasicTests: 14 tests (ALL edge cases/errors)
- MergePatternExecutionTests: 6 integration tests
- Missing: Unit tests for actual merge transformation logic

**Focused Solution** (5-10 tests):
- Test 3 T1 → 1 T2 transformation
- Test tier scaling (3x, 9x, 27x multipliers)
- Test resource reward calculations
- Test mixed tier validation (should fail)
- Test position preservation at trigger

**Pattern Match**: Follow existing test patterns in MatchPatternExecutorTests

**Tech Lead Decision** (2025-08-27): APPROVED with focus
- Reduced from 20+ tests to 5-10 focused tests
- Reduced from 4-6h to 2-3h
- Focus on missing unit tests, not redundant edge cases













## ✅ Completed This Sprint
*Items completed in current development cycle - will be archived monthly*

### VS_006: Turn System 
**Status**: COMPLETED ✅
**Completed**: 2025-08-27 (All 4 phases)
**Size**: M (actual: ~4h)
**Owner**: Dev Engineer

**What**: Full turn system with counter and proper game flow
**Result**: Turn counter working, advances on block moves, proper spawn→action→resolve flow

### VS_007: Auto-Spawn System
**Status**: COMPLETED ✅ 
**Completed**: 2025-08-27 (Phases 1-3, UI deferred)
**Size**: M (actual: ~3.5h)
**Owner**: Dev Engineer

**What**: Automatic block spawning at turn start
**Result**: Blocks spawn correctly each turn, full domain/handler/state implementation
**Note**: Phase 4 UI polish intentionally deferred for post-prototype

### BR_016: MergePatternExecutor Missing Error Handling for Edge Cases
**Status**: Resolved
**Completed**: 2025-08-26 23:50 (actual: 25 min)
**Owner**: Debugger Expert  
**Size**: S (estimated 2-3h, actual: 25 min)

**What**: Added defensive programming to MergePatternExecutor
**Resolution**: Added null checks, empty validation, tier bounds, defensive copying
**Tests Added**: 8 new defensive programming tests
**Result**: MergePatternExecutor now gracefully handles all edge cases with Fin<T> errors

### TD_082: Fix Pre-Existing Test Compilation Errors
**Status**: Completed
**Completed**: 2025-08-26 23:21
**Owner**: Test Specialist
**Size**: M (estimated 4-6h)

**What**: Fixed compilation errors in test suite
**Resolution**: Added missing PlacedAt parameters to BlockPlacedEffect constructors
**Result**: All tests compile and pass (verified with quick.ps1)




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