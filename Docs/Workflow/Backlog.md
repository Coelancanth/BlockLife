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

### VS_001 Phase 2: Drag Range Limits [Score: 85/100]
**Status**: Ready for Review 🔍
**Owner**: Test Specialist (for validation) ← Dev Engineer (implementation complete)
**Size**: M (4-6 hours)
**Branch**: feat/drag-to-move-blocks

**What**: Add movement range validation to drag system
**Why**: Prevents unrealistic teleportation, adds strategic depth

**Dev Engineer Implementation Summary** (2025-08-18):
- ✅ Enabled range validation in DragPresenter.cs:206-213
- ✅ Visual indicators already working in DragView.cs:304-327
- ✅ Manhattan distance calculation in DragStateService.cs:96-108
- ✅ Created comprehensive test suite: DragRangeValidationTests.cs (14 tests passing)
- ✅ All existing tests still pass

**Technical Decisions Made**:
- Used Manhattan distance (grid-based) instead of Chebyshev (diagonal)
- Default range: 3 cells (configurable via GetDragRange method)
- Range enforcement happens at both drag update and completion

**Ready for Test Specialist Validation**:
- Implementation follows existing patterns from Phase 1
- No breaking changes to existing functionality
- Performance impact minimal (simple distance calculation)

**Phase 3 - Swap Mechanic** (future):
- Detect drop on occupied cell
- Swap both blocks with animation
- Validate swap against both block ranges

### TD_005: Add Missing Drag Integration Tests [Score: 75/100]
**Status**: Approved ✓
**Owner**: Test Specialist
**Size**: M (4-6 hours)  
**Priority**: Important
**Found By**: Test Specialist during VS_001 Phase 1 review

**Tech Lead Decision** (2025-08-18):
✅ APPROVED - Valid gap in test coverage
- Need end-to-end validation before Phase 2
- Create GdUnit4 tests following existing patterns

**What**: Create GdUnit4 integration tests for drag operations
**Why**: Only unit tests exist, need end-to-end validation before UI hookup

**Approach**:
- Create `tests/GdUnit4/Features/Block/Drag/DragIntegrationTest.cs`
- Test complete drag flow from UI to state changes
- Add stress test for concurrent drag attempts
- Add performance test for 60fps requirement

**Done When**:
- Integration tests cover all drag scenarios
- Stress test validates no race conditions
- Performance test confirms <16ms operations


## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

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