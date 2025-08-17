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


## 📈 Important (Do Next)  
*Core features for current milestone, technical debt affecting velocity*

### VS_001: Complete Drag-to-Move Block System
**Status**: Ready
**Size**: L (2-3 days)
**Technical Lead**: @Tech Lead approved

**Description**: Complete, shippable drag-and-drop movement system with range constraints and swap capability.

**Vertical Slice Scope**:
- **UI Layer**: Mouse/touch drag interactions with visual feedback
- **Command Layer**: DragBlockCommand, SwapBlocksCommand
- **Logic Layer**: Movement validation, range checking, swap detection
- **Data Layer**: Update block positions, maintain grid state

**Implementation Phases** (can ship after any phase):
1. **Phase 1 - Basic Drag** (shippable):
   - Drag blocks to empty cells
   - Visual feedback during drag
   - Cancel on ESC/right-click
   
2. **Phase 2 - Range Limits** (shippable enhancement):
   - Add movement range validation (default: 3 cells)
   - Show range indicators during drag
   - Block invalid moves with feedback
   
3. **Phase 3 - Swap Mechanic** (shippable enhancement):
   - Detect drop on occupied cell
   - Swap both blocks with animation
   - Validate swap against both block ranges

**Acceptance Criteria**:
- [ ] Complete drag-to-move flow works end-to-end
- [ ] Range constraints properly enforced
- [ ] Swap mechanics feel intuitive
- [ ] Mobile touch support included
- [ ] All existing tests pass

**Dependencies**: None - complete vertical slice


## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### TD_001: [Proposed] Extract Input System to Separate Feature Module
**Tech Lead Notes**: After implementing drag-and-drop, we should consider extracting all input handling into a dedicated Feature module following our Clean Architecture pattern. This would centralize input state management and make it easier to add new interaction modes.

### TD_002: [Proposed] Performance Optimization for Drag Visualization
**Tech Lead Notes**: Once drag-and-drop is working, profile the performance impact of continuous visual updates during drag operations. May need to implement throttling or frame-skipping for lower-end devices.


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