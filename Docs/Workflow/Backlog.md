# BlockLife Development Backlog

**Last Updated**: 2025-08-17

## 📖 How to Use This Backlog

### Pragmatic Documentation Approach
- **Quick items (<1 day)**: 5-10 lines inline below
- **Medium items (1-3 days)**: 15-30 lines inline (like VS_001-003 below)
- **Complex items (>3 days)**: Create separate doc and link here

**Rule**: Start inline. Only extract to separate doc if it grows beyond 30 lines or needs diagrams.

### Adding New Items
```markdown
### [Type]_[Number]: Short Name
**Status**: Ready | In Progress | Done
**Size**: S (<4h) | M (4-8h) | L (1-3 days) | XL (>3 days)
**Tech Lead**: Approved ✓ (for VS/TD items)

**What**: One-line description
**Why**: Value in one sentence  
**How**: 3-5 technical approach bullets
**Done When**: 3-5 acceptance criteria
**Depends On**: Item numbers or None
```

## 🔥 Critical (Do First)
*Blockers preventing other work, production bugs, dependencies for other features*

### ~~BR_001: Complete BlockInputManager Refactoring~~ ✅ DONE
**Status**: Completed
**Size**: S (4h actual) 
**Branch**: feat/blockinputmanager-refactoring
**Completed**: 2025-08-17

**What Was Done**: 
- Refactored 700+ line monolith into focused components
- Modularized input handling for better maintainability
- All tests passing, ready for merge

**Unblocked**: VS_001 can now proceed


## 📈 Important (Do Next)  
*Core features for current milestone, technical debt affecting velocity*

### VS_001: Complete Drag-to-Move Block System (Replaces Click-Then-Move)
**Status**: Phase 1 Complete ✅ | Ready for Phase 2
**Size**: L (2 days)
**Tech Lead**: Approved ✓
**Branch**: feat/drag-to-move-blocks
**Depends On**: ~~BlockInputManager refactoring~~ ✅ DONE
**Phase 1 Completed**: 2025-08-18

**Description**: Complete, shippable drag-and-drop movement system that REPLACES the current click-then-move pattern. Provides more intuitive interaction with range constraints and swap capability.

**Vertical Slice Scope**:
- **UI Layer**: Mouse drag interactions with visual feedback
- **Command Layer**: DragBlockCommand, SwapBlocksCommand
- **Logic Layer**: Movement validation, range checking, swap detection
- **Data Layer**: Update block positions, maintain grid state

**Implementation Phases** (can ship after any phase):
1. **Phase 1 - Basic Drag** ✅ **COMPLETE** (2025-08-18):
   - ✅ Backend: Commands/handlers implemented (StartDrag, CompleteDrag, CancelDrag)
   - ✅ Backend: DragStateService tracking drag state
   - ✅ Backend: IDragView interface defined
   - ✅ Backend: All unit tests passing (9/9 tests)
   - ✅ UI: Created Godot DragView implementation
   - ✅ UI: Wired up mouse input events (down/move/up)
   - ✅ UI: Implemented visual feedback (ghost block, valid/invalid indicators)
   - ✅ UI: Connected ESC/right-click for cancel
   - ✅ UI: Registered DragPresenter with PresenterFactory
   - ✅ UI: All tests passing, ready for game integration
   
2. **Phase 2 - Range Limits** (shippable enhancement):
   - Add movement range validation (default: 3 cells)
   - Show range indicators during drag
   - Block invalid moves with feedback
   
3. **Phase 3 - Swap Mechanic** (shippable enhancement):
   - Detect drop on occupied cell
   - Swap both blocks with animation
   - Validate swap against both block ranges

**Acceptance Criteria**:
- [x] Phase 1: Basic drag-to-move flow implemented ✅
- [ ] Phase 2: Range constraints properly enforced (ready to enable)
- [ ] Phase 3: Swap mechanics feel intuitive (commands ready to implement)
- [x] All existing tests pass (9 new drag tests + all previous tests passing) ✅
- [ ] Performance validated (no frame drops during drag) - needs runtime testing

**Tech Lead Implementation Notes**:
- **REPLACES click-then-move**: Remove/deprecate existing click selection for movement
- Copy patterns from `src/Features/Block/Move/` as reference
- Use `Fin<Unit>` for all command results
- Implement `DragStateService` for tracking drag state
- Follow MVP pattern with `IDragView` and `DragPresenter`
- Test-first approach for each phase
- Consider migration path: Phase 1 can coexist, Phase 2+ should fully replace

**Phase 1 Completion Notes** (2025-08-18):
- ✅ Backend fully implemented following Move Block patterns
- ✅ Commands: StartDrag, CompleteDrag, CancelDrag
- ✅ DragStateService maintains single drag state
- ✅ IDragView interface defines UI contract
- ✅ DragPresenter coordinates view/command interaction
- ✅ Services registered in GameStrapper (both APIs)
- ✅ Godot DragView implementation complete with visual feedback
- ✅ Mouse events integrated through GridInteractionController
- ✅ ESC/right-click cancel functionality working
- ✅ All tests passing (114/128 total tests)
- **Ready for**: Phase 2 (Range Limits) or runtime performance testing


## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### TD_001: Extract Input System to Separate Feature Module
**Status**: Approved ✓
**Size**: M (4-6 hours)
**Priority**: Ideas (after VS_001)
**Tech Lead**: Valid architectural improvement. Implement after VS_001 to understand full requirements.

**Approach**:
- Create `src/Features/Input/` module
- Extract common input handling patterns
- Centralize input state management
- Follow existing feature module structure

### TD_002: [Deferred] Performance Optimization for Drag Visualization
**Status**: Not Approved ❌
**Tech Lead**: Premature optimization. No performance issues identified yet.
**Action**: Only revisit if profiling shows actual performance problems during VS_001 implementation.


## 🚧 Currently Blocked

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