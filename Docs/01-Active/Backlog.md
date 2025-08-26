# BlockLife Development Backlog

**Last Updated**: 2025-08-27 03:05
**Last Aging Check**: 2025-08-22
> 📚 See BACKLOG_AGING_PROTOCOL.md for 3-10 day aging rules

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 017 (Last: BR_016 - 2025-08-26 23:25)
- **Next TD**: 089 (Last: TD_088 - 2025-08-26 23:25)  
- **Next VS**: 006 (Last: VS_005 - 2025-08-26 23:43)

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


## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*

### VS_005: User-Facing Merge Unlock UI
**Status**: Proposed
**Owner**: Product Owner
**Size**: M (4-6h)
**Priority**: Important
**Created**: 2025-08-26 23:43

**What**: Create accessible UI for players to purchase merge pattern unlocks without debug keys
**Why**: Players currently can only unlock merge abilities via F8 debug panel - needs proper in-game UI

**Current State**:
- Backend purchase system fully working (VS_003B-3)
- F8 debug panel has functional unlock UI but is developer-only
- Players start with T1 (match-only), need to buy T2+ for merge patterns

**Proposed Solution**:
- Add unlock button/shop to main game UI (not debug panel)
- Show current tier and available upgrades
- Display costs clearly (T2: 100, T3: 500, T4: 2500)
- Visual feedback when unlock succeeds
- Integrate with existing PurchaseMergeUnlockCommand

**Done When**:
- Players can see and purchase merge unlocks in normal gameplay
- No debug keys required for unlock progression
- Clear cost/benefit shown before purchase
- Visual confirmation of successful unlock

---

### TD_084: Refactor to Use LanguageExt Collections Instead of System.Collections.Generic
**Status**: Approved
**Owner**: Dev Engineer
**Size**: S (2-3h)
**Priority**: Important
**Created**: 2025-08-26 23:25
**Markers**: [ARCHITECTURE] [FUNCTIONAL-PARADIGM]

**What**: Replace System.Collections.Generic with LanguageExt immutable collections
**Why**: Violates functional programming paradigm, causes potential mutation bugs

**Current Violations** (verified by Tech Lead):
- MergePatternExecutor uses List<Block> (mutable) ✅
- MatchPatternRecognizer uses HashSet<Vector2Int> and Dictionary (mutable) ✅
- ~~PlayerState~~ already uses Map correctly ❌

**Proposed Solution**:
- Replace List<T> with Lst<T> or Seq<T>
- Replace HashSet<T> with Set<T>
- Replace Dictionary<K,V> with Map<K,V>
- Update all LINQ operations to LanguageExt equivalents

**Pattern Match**: Follow existing patterns in IGridStateService and PlayerState which correctly use Map

**Done When**:
- No System.Collections.Generic imports in Core domain
- All collections are immutable
- Tests still pass

**Tech Lead Decision** (2025-08-27): APPROVED with corrections
- Reduced time estimate from 4-6h to 2-3h
- Removed false PlayerState claim
- Worth doing for consistency


### TD_085: ~~Add Comprehensive Logging and Telemetry to Pattern System~~
**Status**: Rejected
**Owner**: ~~Dev Engineer~~
**Size**: ~~M (4-6h)~~
**Priority**: ~~Important~~
**Created**: 2025-08-26 23:25

**Tech Lead Decision** (2025-08-27): REJECTED
- Pattern executors already have comprehensive logging (verified)
- Visual debugging covered by TD_088
- If metrics needed, that's a separate simpler TD (1-2h for counters)
- Don't mix logging (already done) with telemetry (not needed yet)


## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### TD_086: ~~Implement Property-Based Testing for Pattern Recognition~~
**Status**: Rejected
**Owner**: ~~Test Specialist~~
**Size**: ~~L (8-12h)~~
**Priority**: ~~Ideas~~
**Created**: 2025-08-26 23:25

**Tech Lead Decision** (2025-08-27): REJECTED
- MatchPatternPropertyTests.cs already exists with FsCheck tests
- We already have property-based testing for patterns
- This is duplicate/redundant work

### TD_087: ~~Performance Optimization for Large Grid Pattern Recognition~~
**Status**: Rejected
**Owner**: ~~Dev Engineer~~
**Size**: ~~L (8-12h)~~
**Priority**: ~~Ideas~~
**Created**: 2025-08-26 23:25

**Tech Lead Decision** (2025-08-27): REJECTED
- Current grid is 10x10, not 100x100
- No evidence of performance problems
- Classic premature optimization
- "May lag" = theoretical problem, not real

### TD_088: Add Visual Pattern Recognition Debugging Tools
**Status**: Approved
**Owner**: Dev Engineer
**Size**: S (4h)
**Priority**: Ideas
**Created**: 2025-08-26 23:25

**What**: Create debug overlay showing pattern detection in real-time
**Why**: Developers cannot easily see why patterns aren't triggering

**Features** (simplified scope):
- Overlay showing detected patterns with different colors
- Pattern type labels (Match vs Merge)
- Execution order visualization
- ~~Frame-by-frame pattern state replay~~ (gold plating)
- ~~Export pattern detection logs~~ (unnecessary)

**Done When**:
- F10 key toggles pattern debug overlay
- Clear visual indication of pattern boundaries
- Pattern types visible

**Tech Lead Decision** (2025-08-27): APPROVED with reduced scope
- Reduced from 6-8h to 4h
- Simple overlay only, no fancy replay
- Actually useful for debugging

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