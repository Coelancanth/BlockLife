# BlockLife Development Backlog

**Last Updated**: 2025-08-26 19:21
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

### BR_015: Fix Failing PurchaseMergeUnlockCommandHandler Tests
**Status**: ~~Proposed~~ **Resolved** 2025-08-26 23:41
**Owner**: Debugger Expert
**Size**: S (2-3h actual: 40 min)
**Priority**: Critical
**Created**: 2025-08-26 23:25
**Markers**: [TEST-FAILURE] [BLOCKING-PR]

**What**: 3 tests failing in PurchaseMergeUnlockCommandHandler due to incorrect test setup
**Why**: Tests block PR merge and may indicate logic issues in merge unlock implementation

**Root Cause Found**:
1. PlayerState.CreateNew() defaulted MaxUnlockedTier = 2 (for testing)
2. This broke purchase validation - player already had T2 unlocked
3. Test helper also needed proper UpdatePlayer() call with version tracking

**Fix Applied**:
- Changed default MaxUnlockedTier from 2 to 1 in PlayerState
- Fixed SetupTestPlayerWithMoneyAndUnlockedTier helper to use UpdatePlayer()
- Enabled 2 previously skipped tests that now work

**Result**: All 9 PurchaseMergeUnlockCommandHandler tests now pass

---

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
**Status**: Proposed
**Owner**: Tech Lead
**Size**: M (4-6h)
**Priority**: Important
**Created**: 2025-08-26 23:25
**Markers**: [ARCHITECTURE] [FUNCTIONAL-PARADIGM]

**What**: Replace System.Collections.Generic with LanguageExt immutable collections
**Why**: Violates functional programming paradigm, causes potential mutation bugs

**Current Violations** (10+ files):
- MergePatternExecutor uses List<Block> (mutable)
- MatchPatternRecognizer uses HashSet<Vector2Int> (mutable)
- PlayerState uses Dictionary<ResourceType, int> (should use Map)

**Proposed Solution**:
- Replace List<T> with Lst<T> or Seq<T>
- Replace HashSet<T> with Set<T>
- Replace Dictionary<K,V> with Map<K,V>
- Update all LINQ operations to LanguageExt equivalents

**Pattern Match**: Follow existing patterns in IGridStateService which correctly uses Map

**Done When**:
- No System.Collections.Generic imports in Core domain
- All collections are immutable
- Tests still pass

### BR_016: MergePatternExecutor Missing Error Handling for Edge Cases
**Status**: ~~Proposed~~ **Resolved** 2025-08-26 23:50
**Owner**: Debugger Expert  
**Size**: S (2-3h actual: 25 min)
**Priority**: Important
**Created**: 2025-08-26 23:25

**What**: MergePatternExecutor lacks defensive programming for several edge cases
**Why**: Could cause runtime exceptions in production

**Defensive Programming Added**:
1. ✅ Null checks for context and context.GridService
2. ✅ Empty collection validation before First()
3. ✅ Created defensive copy of positions to avoid concurrent modification
4. ✅ Tier bounds validation (max T4) with proper error messages
5. ✅ Null pattern checks in CanExecute and EstimateExecutionTime

**Tests Added**: 8 new defensive programming tests covering all edge cases
**Result**: MergePatternExecutor now gracefully handles all edge cases with Fin<T> errors

**Done When**:
- All edge cases return proper Error results
- No possibility of NullReferenceException
- Unit tests cover edge cases

### TD_085: Add Comprehensive Logging and Telemetry to Pattern System
**Status**: Proposed
**Owner**: Dev Engineer
**Size**: M (4-6h)
**Priority**: Important
**Created**: 2025-08-26 23:25

**What**: Pattern recognition and execution lacks sufficient observability
**Why**: Cannot debug production issues or understand performance bottlenecks

**Missing Telemetry**:
- Pattern recognition timing metrics
- Match vs Merge execution counts
- Tier progression statistics
- Failed pattern execution reasons
- Performance counters for grid operations

**Proposed Solution**:
- Add structured logging with correlation IDs
- Implement performance counters
- Track pattern success/failure rates
- Add debug visualizations for pattern detection

**Pattern Match**: Follow logging patterns in ProcessPatternsAfterPlacementHandler

**Done When**:
- Every pattern operation is logged
- Performance metrics available
- Can trace full pattern lifecycle


## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### TD_086: Implement Property-Based Testing for Pattern Recognition
**Status**: Proposed
**Owner**: Test Specialist
**Size**: L (8-12h)
**Priority**: Ideas
**Created**: 2025-08-26 23:25

**What**: Add FsCheck property tests for pattern recognition invariants
**Why**: Ensure pattern detection is mathematically correct under all conditions

**Properties to Test**:
- Pattern recognition is deterministic (same grid = same patterns)
- No overlapping patterns (each position in max 1 pattern)
- Merge patterns preserve block count invariants
- Pattern execution is idempotent when repeated

**Pattern Match**: Follow FsCheck 3.x patterns in existing property tests

### TD_087: Performance Optimization for Large Grid Pattern Recognition
**Status**: Proposed
**Owner**: Dev Engineer
**Size**: L (8-12h)
**Priority**: Ideas
**Created**: 2025-08-26 23:25

**What**: Optimize pattern recognition for 100x100+ grids
**Why**: Current O(n²) algorithm may lag with large grids

**Optimization Opportunities**:
- Implement spatial indexing for pattern detection
- Cache adjacent block lookups
- Use parallel pattern recognition for independent regions
- Implement dirty region tracking to avoid full scans

**Done When**:
- Pattern recognition <16ms for 100x100 grid
- Profiling shows no hot spots
- Memory usage remains flat

### TD_088: Add Visual Pattern Recognition Debugging Tools
**Status**: Proposed
**Owner**: Dev Engineer
**Size**: M (6-8h)
**Priority**: Ideas
**Created**: 2025-08-26 23:25

**What**: Create debug overlay showing pattern detection in real-time
**Why**: Developers cannot easily see why patterns aren't triggering

**Features**:
- Overlay showing detected patterns with different colors
- Pattern type labels (Match vs Merge)
- Execution order visualization
- Frame-by-frame pattern state replay
- Export pattern detection logs

**Done When**:
- F10 key toggles pattern debug overlay
- Can step through pattern execution
- Clear visual indication of pattern boundaries

### TD_081: Add Comprehensive Merge System Test Coverage
**Status**: Proposed
**Owner**: Test Specialist
**Size**: M (4-6h)
**Priority**: Important
**Created**: 2025-08-26 20:20
**Complexity Score**: 3/10 (straightforward testing work)

**What**: Add missing test coverage for merge pattern execution system
**Why**: Core game mechanic has only 5 basic tests, missing critical path validation

**Current State**:
- MergePatternExecutorBasicTests: 5 tests (config and error cases only)
- Missing: Actual merge execution tests
- Missing: Tier scaling validation
- Missing: Integration tests

**Proposed Solution**:
- Add 20+ tests to MergePatternExecutorTests
- Test 3 T1 → 1 T2 transformation
- Test tier scaling (3x, 9x, 27x multipliers)
- Test mixed tier validation (should fail)
- Integration tests for full merge flow
- Property-based tests for invariants

**Simpler Alternative**: Just add 5-10 happy path tests (Score: 1/10)
- Would cover basic functionality but miss edge cases

**Pattern Match**: Follow existing test patterns in MatchPatternExecutorTests

### TD_082: Fix Pre-Existing Test Compilation Errors (BlockPlacedEffect Constructor)
**Status**: ~~Proposed~~ **Completed 2025-08-26 23:21**
**Owner**: ~~Test Specialist~~
**Size**: M (4-6h)
**Priority**: Critical
**Created**: 2025-08-26 23:00
**Complexity Score**: 4/10 (systematic test fixing)

**What**: Fix compilation errors preventing test suite execution
**Why**: PR can't merge with failing tests - CI/CD requires clean build

**Current Issue**:
- 14 test files failing with `CS7036: There is no argument given that corresponds to the required parameter 'PlacedAt'`
- BlockPlacedEffect constructor signature changed but tests not updated
- Affects SimulationManagerThreadSafetyTests, SimulationManagerRegressionTests, etc.

**Proposed Solution**:
- Add missing `PlacedAt` parameter to all BlockPlacedEffect constructor calls
- Verify parameter order matches current constructor signature  
- Run test suite to ensure no remaining compilation errors
- Add proper DateTime values for PlacedAt parameter

**Pattern Match**: Follow existing test patterns for effect creation

**Simpler Alternative**: Fix just the failing tests (Score: 2/10)
- Would get tests compiling but might miss parameter usage patterns

### TD_083: Polish Merge System for Production Readiness  
**Status**: Proposed
**Owner**: Dev Engineer
**Size**: S (3-4h)
**Priority**: Important
**Created**: 2025-08-26 23:00
**Complexity Score**: 3/10 (refinement work)

**What**: Polish and refine merge system implementation for production quality
**Why**: Current implementation works but needs final polish before release

**Polish Areas**:
- Performance optimization for pattern recognition with tier checking
- Error message improvements for invalid merge scenarios
- Edge case handling (empty patterns, invalid positions)
- Code documentation for maintenance
- Consider adding merge animation timing controls

**Proposed Solution**:
- Review pattern recognition performance impact of tier checking
- Enhance error messages with more context
- Add comprehensive parameter validation
- Clean up any remaining TODO/FIXME comments
- Performance testing with large grids

**Pattern Match**: Follow existing performance optimization patterns

**Simpler Alternative**: Just add documentation (Score: 1/10)
- Would improve maintainability but miss performance opportunities











## ✅ Completed This Sprint
*Items completed in current development cycle - will be archived monthly*




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