# BlockLife Development Backlog

**Last Updated**: 2025-08-27 13:53
**Last Aging Check**: 2025-08-22
> 📚 See BACKLOG_AGING_PROTOCOL.md for 3-10 day aging rules

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 017 (Last: BR_016 - 2025-08-26 23:25)
- **Next TD**: 089 (Last: TD_088 - 2025-08-26 23:25)  
- **Next VS**: 009 (Last: VS_008 - 2025-08-27 13:53)

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

### VS_006: Core Turn System
**Status**: Ready for Dev
**Owner**: Dev Engineer
**Size**: S (4h)
**Priority**: Critical
**Created**: 2025-08-27 13:53
**Reviewed**: 2025-08-27 14:05

**What**: Implement turn counter with one-action-per-turn limitation
**Why**: Creates time pressure that makes the game challenging and meaningful

**How**:
- Add TurnManager service to track current turn number
- Display turn counter in UI (starts at 1)
- Only VALID MOVES advance the turn (not placements or failed moves)
- Turn advances after move completes (including all chains)
- Fire TurnStartNotification and TurnEndNotification events

**Done When**:
- Turn counter visible in game UI showing "Turn: X"
- Only successful block movements advance the turn
- Turn waits for all chain reactions to complete before advancing
- Turn events allow other systems to hook in
- Turn number persists in PlayerDataService

**Depends On**: None

**Tech Lead Decision** (2025-08-27 14:05):
- Complexity: 3/10 - Follows existing patterns exactly
- Pattern: Copy from MoveBlockCommand/Handler structure
- Integration: Hook after ProcessPatternsAfterPlacement completes
- Risk: Low - well-established integration points

---

### VS_007: Auto-Spawn System  
**Status**: Ready for Dev
**Owner**: Dev Engineer
**Size**: S (4.5h)
**Priority**: Critical
**Created**: 2025-08-27 13:53
**Reviewed**: 2025-08-27 14:05

**What**: Automatically spawn new blocks at the start of each turn
**Why**: Forces space management decisions and prevents infinite planning

**How**:
- Create SpawnService with IAutoSpawnStrategy interface
- Hook into TurnStartNotification from VS_006
- Spawn one random Tier-1 block at turn start
- Select random empty position for spawn
- Trigger game over if no empty positions available

**Done When**:
- One block spawns automatically when turn starts
- Block appears in random empty grid position
- Block type is randomly selected from available types
- Always spawns at Tier 1
- Visual/sound effect plays on spawn
- Game over triggers when grid is full

**Depends On**: VS_006 (Turn System)

**Tech Lead Decision** (2025-08-27 14:05):
- Complexity: 4/10 - Strategy pattern adds slight complexity
- Pattern: Strategy for spawn logic, reuse PlaceBlockCommand
- Safety-critical: Game over detection must be bulletproof
- Risk: Medium - game over is critical feature


## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*

### VS_008: Godot Resource-Based Rewards
**Status**: Ready for Dev
**Owner**: Dev Engineer
**Size**: S (4h)
**Priority**: Important
**Created**: 2025-08-27 13:53
**Reviewed**: 2025-08-27 14:05

**What**: Migrate hardcoded reward values to Godot Resource files
**Why**: Enables rapid balancing and debugging without recompiling

**How**:
- Create BlockRewardResource.tres for each block type
- Define tier-based reward scaling in resources
- Add debug overlay (F12) showing current reward values
- Implement GodotResourceBridge service for C# integration
- Support hot reload in debug builds

**Done When**:
- Each block type has a .tres resource file defining rewards
- Resources specify base values and tier multipliers
- Debug overlay displays loaded reward values
- Reward calculator uses resource data instead of hardcoded values
- Values can be modified without recompiling

**Depends On**: None (but more useful after VS_007)

**Tech Lead Decision** (2025-08-27 14:05):
- Complexity: 5/10 - New architectural boundary (C# ↔ Godot)
- Pattern: Bridge service pattern, first of its kind
- Can run parallel with VS_006 - no dependencies
- Risk: Medium - sets precedent for resource integration

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