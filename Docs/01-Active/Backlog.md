# BlockLife Development Backlog

**Last Updated**: 2025-08-28 00:50
**Last Aging Check**: 2025-08-22
> 📚 See BACKLOG_AGING_PROTOCOL.md for 3-10 day aging rules

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 017 (Last: BR_016 - 2025-08-26 23:25)
- **Next TD**: 090 (Last: TD_089 - 2025-08-28 00:50)  
- **Next VS**: 011 (Last: VS_010 - 2025-08-29 01:00)

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

### VS_010: Infant Stage Foundation (Natural Tutorial)
**Status**: Proposed
**Owner**: Product Owner → Tech Lead
**Size**: M (6-8h)
**Priority**: Critical (defines entire game onboarding)
**Created**: 2025-08-29 01:00

**What**: Implement infant life stage with 5x5 grid, center parent block, and proximity effect system
**Why**: Natural tutorial through baby nurturing - teaches through emotion, not instruction

**Core Mechanics**:
1. **5x5 Grid**: Fixed size with Parent Block at CENTER (2,2)
2. **Parent Block**: Unmovable center block that spawns help based on baby's state
3. **Need System**: Cry action creates Need blocks (red)
4. **Care Matching**: Need + Care + Care = Comfort (3-match required)
5. **No Energy Cost**: Babies explore freely (no turn limits)

**Block Types & Rules** (Simple Same-Type Matching):
- **Need** 😢: Match 3 = Tantrum (parent helps) | Proximity to Care = Transform to Happy
- **Care** 🤗: Match 3 = Comfort | Proximity to Need = Healing (2 turn resolution)
- **Love** 💕: Match 3 = Joy burst | Acts as Care in proximity effects
- **Toy** 🧸: Match 3 = Fun | ONLY type that merges (teaching mechanic)
- **Sleep** 😴: Match 3 = Rest | Auto-matches after 5 turns (patience lesson)

**Parent Block AI** (Center Position):
```
Every 3 actions:
  If stress > 2 → Spawn 2 Care blocks adjacent
  If happy → Spawn 1 Love block adjacent
  Default → Spawn 1 Care block adjacent
On chain → "Proud parent" → Spawn Love
Grid > 20 blocks → "Cleanup time" → Remove old Toys
```

**Learning Progression** (Cognitive Load Optimized):
1. **Turns 1-10**: Basic same-type matching only
2. **Turns 11-20**: Proximity effects visible (glowing blocks)
3. **Turns 21-30**: Toy blocks + chains possible
4. **Turns 31-40**: Toy merge unlocked + Sleep mechanics
5. **Turns 41-50**: All systems active → Graduate at 50 comfort

**Acceptance Criteria**:
- 5x5 grid with Parent Block fixed at center (2,2)
- Same-type matching only (cognitive load reduction)
- Proximity effects: Adjacent blocks interact over 2-3 turns
- Context-sensitive tooltips guide learning
- Parent AI with 3 states: Happy/Worried/Calm
- Visual feedback: Glowing = proximity active
- No failure state (infinite play)
- Graduate at 50 comfort ("Baby is growing up!")

**Product Owner Notes**:
- Proximity effects solve mixed-type matching complexity
- Tooltips are THE critical UX element (parent's voice)
- One rule (match same colors) + emergence = depth
- Visual language teaches without reading

**Tech Lead Review Needed**:
- Proximity effect system (new architecture)
- Parent AI state machine implementation
- Tooltip system (context-sensitive)
- Block transformation animations (Need → Happy)

📖 **Full Design Document**: [Docs/02-Design/Game/InfantStage.md](../02-Design/Game/InfantStage.md)

---

### TD_089: Event Logging Foundation (Analytics + Life-Review)
**Status**: Approved ✅
**Owner**: Dev Engineer  
**Size**: S (3-4h)
**Priority**: Critical (enables analytics AND future life-review)
**Created**: 2025-08-28 00:50
**Approved**: 2025-08-28 01:30 by Tech Lead
**Complexity Score**: 2/10 - Simple JSON logging with dual purpose

**What**: Event logging system for analytics and future life-review mechanics
**Why**: Immediate need for balance data + foundation for Vision.md life-review feature

**Tech Lead Decision**: 
- Recognized as lightweight event sourcing pattern
- Serves dual purpose: analytics now, life-review later
- Keep implementation simple, design for extensibility

**Implementation Approach (Simple & Extensible)**:
- Create `SessionLogger` service that hooks into existing game events
- Log actions with turn, timestamp, action type, details
- Add "significance" field for future Memory Palace feature
- **NEW FILE per session**: `Analytics/Sessions/session_YYYYMMDD_HHmmss_[guid8].json`
- Never overwrite - every attempt matters (even rage quits provide data)
- Auto-cleanup: Delete files >30 days old (configurable)
- Design to support future narrative tags

**Data Structure**:
```json
{
  "sessionId": "uuid",
  "startTime": "ISO-8601",
  "events": [
    {"turn": 1, "time": "ISO-8601", "action": "place_block", "details": {...}},
    {"turn": 1, "time": "ISO-8601", "action": "match_3", "details": {...}, "reward": {"Money": 30}},
    {"turn": 47, "time": "ISO-8601", "action": "first_tier4", "significance": 0.8}
  ],
  "snapshots": [
    {"turn": 10, "resources": {...}, "gridDensity": 0.35}
  ],
  "summary": {
    "totalTurns": 47,
    "totalMatches": 23,
    "totalMerges": 8,
    "maxChain": 4,
    "keyMoments": ["first_tier4", "biggest_chain"],
    "finalResources": {...}
  }
}
```

**Done When**:
- Every player action logged with context
- Resource flow tracked per turn
- "Significance" scores for major events
- Each session creates new timestamped JSON file
- Auto-save on game exit AND every 50 turns (crash protection)
- Old sessions auto-cleanup after 30 days
- Can query/aggregate across multiple session files
- Foundation ready for life-review feature

**Product Owner Notes**:
- Enables data-driven balance decisions
- Foundation for Vision.md Memory Palace & Life-Review
- Start simple, evolve as needed

---



## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*



## 🗄️ Backup (Complex Features for Later)
*Advanced mechanics postponed until core loop is proven fun*

### VS_009: Transmutation System
**Status**: Backup (Too Complex for Current Stage)
**Owner**: TBD
**Size**: M (7h)
**Priority**: Future
**Created**: 2025-08-28 00:38
**Moved to Backup**: 2025-08-29 01:00
**Reason**: Complexity doesn't match infant stage simplicity

**What**: Transmute 3 different block types into strategic combinations
**Why**: Adds strategic depth for experienced players
**When to Reconsider**: After infant stage proves fun and we need depth

**Original Spec Summary**:
- Select 3 different block types
- Choose resulting type from inputs
- Creates higher tier block
- Costs resources (adds economy pressure)

**Product Owner Note**: Great mechanic for later, but infant stage proves we can find depth through simplicity first. Revisit after core loop validation.

---

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