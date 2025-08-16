# VS_001: Block Drag-and-Drop Interaction System

**Type**: Vertical Slice  
**Priority**: 📈 Important  
**Size**: Medium  
**Status**: Not Started  
**Domain**: Block/Input

---

## 📋 What & Why *(Product Owner)*

**User Story**: As a player I want to drag blocks directly to new positions so that I can move blocks intuitively with a single action instead of click-select-then-click-move.

**Value Proposition**: Transforms clunky two-click interaction into smooth single-action drag-and-drop, improving game feel and reducing cognitive load.

**Success Criteria**: Players can grab any block and drag it smoothly to a new position with immediate visual feedback.

---

## 🏗️ Implementation Notes *(Tech Lead)*

**Architecture Pattern**: Follow `src/Features/Block/Move/` as gold standard

**Key Components**:
- Commands: Reuse existing MoveBlockCommand infrastructure
- Handlers: Existing MoveBlockCommandHandler (no changes needed)
- Presenters: Enhance BlockInputManager with drag event handling
- Views: GridInteractionController drag events (foundation already built)

**Integration Points**: 
- Existing MoveBlockCommand pipeline
- Current grid interaction system
- BlockVisualizationController for drag preview

**Risks/Concerns**: 
- Must maintain click-based fallback for accessibility
- Drag threshold tuning for good UX
- Visual feedback performance during drag

---

## ✅ Acceptance Criteria *(Product Owner)*
- [ ] Given a block exists, When I click and drag it, Then it follows my cursor
- [ ] Given I'm dragging a block, When I release over valid position, Then block moves there
- [ ] Given I'm dragging a block, When I release over invalid position, Then block returns to origin
- [ ] Given drag-and-drop is implemented, When I use click-select-move, Then it still works (backward compatibility)
- [ ] Given I start dragging, When I move 5+ pixels, Then drag mode activates with visual feedback

---

## 🧪 Testing Approach *(Test Designer + QA Engineer)*

**Unit Tests**: 
- [ ] Drag event detection and threshold
- [ ] Drag state management in presenter
- [ ] Command integration (reuses existing tests)

**Integration Tests**:
- [ ] Full drag-and-drop flow (grab → drag → drop)
- [ ] Backward compatibility with click-based movement
- [ ] Drag cancellation (ESC key, drag outside grid)

---

## 🔄 Dependencies
- **Depends on**: BF_001 resolution (lag investigation for smooth UX)
- **Blocks**: TD_003, TD_004 (movement constraints and swapping use drag infrastructure)

---

## 📝 Implementation Progress & Notes

**Current Status**: Foundation complete (drag events implemented), ready for handler connection

**Agent Updates**:
- 2025-08-17 - Tech Lead: Phase 1 drag infrastructure complete (interface + detection)
- 2025-08-17 - Main Agent: Drag events added to GridInteractionController, awaiting handler implementation

**Blockers**: None technical - prioritized after BF_001 lag investigation

**Next Steps**: Implement drag handlers in BlockInputManager following tech-lead plan

---

## 📚 References
- **Gold Standard**: `src/Features/Block/Move/` 
- **Technical Plan**: TD_002_Block_Interaction_Technical_Plan.md (Phase 2)
- **Related Work**: BF_001 (performance), TD_003 (constraints), TD_004 (swapping)

---

*Simple, maintainable, actually used. Focus on what matters for getting work done.*