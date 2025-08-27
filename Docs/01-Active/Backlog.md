# BlockLife Development Backlog

**Last Updated**: 2025-08-27 16:10
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
**Status**: In Progress (Phase 1/4 ✅ COMPLETE)
**Owner**: Dev Engineer
**Size**: S (4h - 1h complete, 3h remaining)
**Priority**: Critical
**Created**: 2025-08-27 13:53
**Reviewed**: 2025-08-27 14:05
**Updated**: 2025-08-27 16:10 - Phase 1 COMPLETE with 48 passing tests

**What**: Implement turn counter with one-action-per-turn limitation
**Why**: Creates time pressure that makes the game challenging and meaningful

**Phase Breakdown (Model-First Protocol)**:

#### Phase 1: Domain Model ✅ COMPLETE (1h)
**Acceptance**: Turn logic correctly tracks and increments ✅
- ✅ Create `Turn` value object with number and validation
- ✅ Create `TurnManager` domain service with business rules
- ✅ Unit tests for turn increment logic (48 tests passing)
- ✅ Unit tests for validation rules (only moves advance turn)
**Commit**: `e82c58b feat(VS_006): implement turn system domain model [Phase 1/4]`
**Completed**: 2025-08-27 16:13
**Files Created**: 5 files (Turn.cs, ITurnManager.cs, TurnManager.cs, 2 test files)
**Key Learnings**: LanguageExt patterns require Context7 first, functional error handling with Fin<T>

#### Phase 2: Application Layer 🎯 READY TO START (1h)
**Acceptance**: Commands process turn advancement
- Create `AdvanceTurnCommand` and handler
- Create `TurnStartNotification` and `TurnEndNotification`
- Wire into existing move completion flow
- Handler tests with mocked services
**Commit**: `feat(VS_006): command handlers [Phase 2/4]`
**Pattern**: Copy from MoveBlockCommand/Handler structure
**Integration**: Hook after ProcessPatternsAfterPlacement completes

#### Phase 3: Infrastructure (1h)
**Acceptance**: Turn state persists correctly
- Implement turn persistence in `PlayerDataService`
- Add turn tracking to game state
- Integration tests for save/load
- Verify turn survives game restart
**Commit**: `feat(turn): state persistence [Phase 3/4]`

#### Phase 4: Presentation (1h)
**Acceptance**: UI displays current turn
- Create `TurnCounterView` and presenter
- Wire to Godot UI scene
- Display "Turn: X" in game HUD
- Manual test all turn advancement scenarios
**Commit**: `feat(turn): UI integration [Phase 4/4]`

**Depends On**: None

**Tech Lead Decision** (2025-08-27 14:05):
- Complexity: 3/10 - Follows existing patterns exactly
- Pattern: Copy from MoveBlockCommand/Handler structure
- Integration: Hook after ProcessPatternsAfterPlacement completes
- Risk: Low - well-established integration points
- **Phase Gates**: Each phase must have GREEN tests before proceeding

---

### VS_007: Auto-Spawn System  
**Status**: Blocked - Depends on VS_006 completion
**Owner**: Dev Engineer
**Size**: S (4.5h)
**Priority**: Critical
**Created**: 2025-08-27 13:53
**Reviewed**: 2025-08-27 14:05
**Updated**: 2025-08-27 16:10 - Blocked until VS_006 Phases 2-4 complete

**What**: Automatically spawn new blocks at the start of each turn
**Why**: Forces space management decisions and prevents infinite planning

**Phase Breakdown (Model-First Protocol)**:

#### Phase 1: Domain Model (1.5h)
**Acceptance**: Spawn logic and game over detection work correctly
- Create `SpawnPosition` selection logic (pure function)
- Create `IAutoSpawnStrategy` interface and `RandomSpawnStrategy`
- Implement game over detection logic (no empty spaces)
- Unit tests for spawn position selection
- Unit tests for game over conditions
**Commit**: `feat(spawn): domain model [Phase 1/4]`

#### Phase 2: Application Layer (1h)
**Acceptance**: Spawn triggered by turn events
- Create `AutoSpawnHandler` for `TurnStartNotification`
- Implement `GameOverCommand` and handler
- Wire spawn to use existing `PlaceBlockCommand`
- Handler tests with mocked grid state
**Commit**: `feat(spawn): command handlers [Phase 2/4]`

#### Phase 3: Infrastructure (1h)
**Acceptance**: Grid state correctly tracks spawns
- Integrate with `GridStateService` for empty position queries
- Add spawn tracking to game statistics
- Integration tests for spawn → place flow
- Verify game over triggers correctly
**Commit**: `feat(spawn): state integration [Phase 3/4]`

#### Phase 4: Presentation (1h)
**Acceptance**: Visual/audio feedback for spawns
- Create spawn animation in Godot
- Add spawn sound effect
- Create game over screen
- Manual test spawn variations
- Manual test game over scenarios
**Commit**: `feat(spawn): UI and effects [Phase 4/4]`

**Depends On**: VS_006 Phase 2+ (Turn System commands/handlers needed for TurnStartNotification)

**Tech Lead Decision** (2025-08-27 14:05):
- Complexity: 4/10 - Strategy pattern adds slight complexity
- Pattern: Strategy for spawn logic, reuse PlaceBlockCommand
- Safety-critical: Game over detection must be bulletproof
- Risk: Medium - game over is critical feature
- **Phase Gates**: Game over logic MUST be thoroughly tested in Phase 1


## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*

### VS_008: Godot Resource-Based Rewards
**Status**: Ready for Dev (Phase 0/4) - Can run in parallel with VS_006
**Owner**: Dev Engineer
**Size**: S (4h)
**Priority**: Important
**Created**: 2025-08-27 13:53
**Reviewed**: 2025-08-27 14:05
**Updated**: 2025-08-27 16:10 - Available now, no dependencies on VS_006

**What**: Migrate hardcoded reward values to Godot Resource files
**Why**: Enables rapid balancing and debugging without recompiling

**Phase Breakdown (Model-First Protocol)**:

#### Phase 1: Domain Model (1h)
**Acceptance**: Reward calculation logic abstracted from data source
- Create `IRewardDataProvider` interface
- Create `RewardCalculator` with provider dependency
- Implement tier-based scaling logic (pure functions)
- Unit tests with mock data provider
- Unit tests for all reward calculations
**Commit**: `feat(rewards): domain model [Phase 1/4]`

#### Phase 2: Application Layer (0.5h)
**Acceptance**: Commands use abstracted reward system
- Update existing reward handlers to use new calculator
- Create `ReloadRewardsCommand` for hot reload
- Handler tests with mocked provider
**Commit**: `feat(rewards): command integration [Phase 2/4]`

#### Phase 3: Infrastructure (1.5h)
**Acceptance**: Bridge to Godot resources works
- Create `GodotResourceBridge` service
- Implement `GodotRewardDataProvider` using bridge
- Create `.tres` resource files for each block type
- Integration tests loading from resources
- Verify hot reload in debug builds
**Commit**: `feat(rewards): Godot bridge [Phase 3/4]`

#### Phase 4: Presentation (1h)
**Acceptance**: Debug overlay shows live values
- Create F12 debug overlay UI
- Display current reward values from resources
- Show tier multipliers and calculations
- Manual test hot reload changes
- Manual test all block type rewards
**Commit**: `feat(rewards): debug overlay [Phase 4/4]`

**Depends On**: None (but more useful after VS_007)

**Tech Lead Decision** (2025-08-27 14:05):
- Complexity: 5/10 - New architectural boundary (C# ↔ Godot)
- Pattern: Bridge service pattern, first of its kind
- Can run parallel with VS_006 - no dependencies
- Risk: Medium - sets precedent for resource integration
- **Phase Gates**: Bridge pattern must be clean and reusable

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