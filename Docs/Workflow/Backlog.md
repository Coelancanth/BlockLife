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

### TD_013: Refactor Range Validation to Shape-Based System [Score: 80/100]
**Status**: Proposed 
**Owner**: Tech Lead (for approval) ← Dev Engineer (proposed)
**Size**: M (6-8 hours)
**Priority**: Important (blocks Phase 3 and future movement patterns)
**Created**: 2025-08-18
**Markers**: [ARCHITECTURE] [EXTENSIBILITY]

**What**: Replace hard-coded range validation with extensible shape system
**Why**: Current Manhattan distance is just one shape - need flexibility for different movement patterns

**Dev Engineer Analysis** (2025-08-18):
- **Current Issue**: IsWithinDragRange uses hard-coded Manhattan distance
- **Opportunity**: Create IMovementShape interface with implementations
- **Enables**: Diamond (Manhattan), Square (Chebyshev), Cross, L-shape, Custom patterns
- **Impact**: More intuitive, extensible, game-design friendly

**Proposed Approach**:
```csharp
interface IMovementShape {
    bool IsValidMove(Vector2Int from, Vector2Int to);
    IEnumerable<Vector2Int> GetValidPositions(Vector2Int from);
}
```
- DiamondShape: Manhattan distance (current)
- SquareShape: Chebyshev distance 
- CrossShape: Cardinal directions only
- Custom shapes for special blocks

**Benefits**:
- Each block type can have different movement patterns
- Visual indicators match actual movement shape
- Extensible without changing core logic
- Better game design flexibility

**Risks**:
- More complex than simple range check
- Need to update UI shape rendering
- Test complexity increases

**Recommendation**: Implement BEFORE Phase 3 since swap mechanics will depend on movement shapes

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

### TD_008: Fix ccstatusline Git Branch Display Issue [Score: 12/100]
**Status**: Deferred ⏸️
**Owner**: DevOps Engineer (when ready)
**Size**: S (2-3 hours)
**Priority**: Ideas
**Found By**: DevOps Engineer during status line configuration
**Created**: 2025-08-18

**Tech Lead Decision** (2025-08-18):
⏸️ DEFERRED - Nice-to-have convenience feature
- Not blocking any real work
- Developers can use `git status` for now

**Strategic Prioritizer Decision** (2025-08-18):
💡 CONFIRMED IN IDEAS - Score: 12/100
- Pure convenience, no business value
- Workaround exists (git status)
- Keep for future quality-of-life improvements

**What**: Fix ccstatusline showing "no git" instead of current branch in Claude Code status line
**Why**: Developers lose git context awareness, affecting workflow efficiency and branch management

**Problem Statement**:
- ccstatusline displays "⎇ no git" instead of actual branch name
- Working directory is correctly in git repository root
- Git commands work normally but status line shows incorrect information
- Affects situational awareness during development

**Approach**:
- Investigate ccstatusline git detection logic in Claude Code context
- Test working directory resolution when executed via Claude settings
- Fix path/environment issues preventing git repository detection
- Verify git integration works in both global and local config modes

**Done When**:
- Status line shows correct git branch: "🌿 docs/improve-git-workflow-documentation"
- Git branch updates when switching branches
- Works consistently in Claude Code status line integration

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

## 📦 Archive
*Completed items - moved here to keep backlog clean*

### VS_001 Phase 1: Basic Drag Implementation ✅ DONE
**Completed**: 2025-08-18
- Backend: All commands/handlers implemented
- UI: Godot DragView with visual feedback
- Integration: Mouse events, ESC/cancel working
- Tests: 9/9 drag tests passing
- Ready for Phase 2 implementation

### TD_003: Verify Context7 Library Access ✅ DONE
**Completed**: 2025-08-18
- ✅ LanguageExt available (/louthy/language-ext, Trust: 9.4)
- ✅ MediatR available (/jbogard/mediatr, Trust: 10.0)
- ✅ Godot available (/godotengine/godot, Trust: 9.9)
- Key Discovery: Error.Message behavior confirmed as post-mortem found

### BR_001: Complete BlockInputManager Refactoring ✅ DONE
**Completed**: 2025-08-17
- Refactored 700+ line monolith into focused components
- Unblocked VS_001

### REJECTED ITEMS (Preserved for Learning)

### TD_002: Performance Optimization ❌ REJECTED
**Rejected**: 2025-08-18
**Reason**: Premature optimization - no performance issues exist
**Learning**: Profile first, optimize second

### TD_007: Git Worktrees ❌ REJECTED  
**Rejected**: 2025-08-18
**Reason**: Massive over-engineering for non-problem
**Learning**: Simple solutions (branch naming) beat complex systems

### TD_010: Dashboard System ❌ REJECTED
**Rejected**: 2025-08-18
**Reason**: Solving wrong problem (visualization vs discipline)
**Learning**: Fix root causes (backlog bloat) not symptoms

### TD_011: Review Gap Automation ✅ IMPLEMENTED
**Completed**: 2025-08-18 (30 minutes)
**Solution**: Created backlog-assistant subagent
**Impact**: Saves 30 min per prioritization session
**Usage**: `/task backlog-assistant "Maintain backlog"`

### TD_006: Separate Performance Tests from CI Pipeline ✅ DONE
**Completed**: 2025-08-18 (1.5 hours)
**Owner**: DevOps Engineer
**Solution**: Added [Trait("Category", "Performance")] to timing tests, excluded from CI
**Impact**: Eliminated 100% false positive rate on timing tests in CI
**Key Changes**:
- 12 timing tests categorized with Performance trait
- Main CI excludes via --filter "Category!=Performance"
- Optional weekly performance pipeline created
- Tests still runnable locally for developers

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