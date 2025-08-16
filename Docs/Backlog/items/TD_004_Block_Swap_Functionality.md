# TD_004: Block Swap Functionality for Occupied Positions

**Type**: Technical Debt  
**Priority**: 📈 Important  
**Category**: Architecture  
**Status**: Not Started  
**Impact**: Medium

---

## 🔧 The Problem
**What's Wrong**: MoveBlockCommandHandler only allows moves to empty positions. When dragging to occupied positions, move fails instead of swapping blocks.

**Where**: `src/Features/Block/Commands/MoveBlockCommandHandler.cs` line 124 - `IsPositionEmpty` validation

**Why It Matters**: Intuitive UX expects dragging block A onto block B to swap their positions. Current behavior breaks user mental model and limits interaction possibilities.

**Cost of Delay**: Poor user experience, confusion when moves to occupied positions fail, missed opportunities for interesting game mechanics.

---

## 💡 The Solution
**Approach**: Enhance MoveBlockCommandHandler to detect occupied positions and perform atomic swaps instead of rejecting moves

**Key Changes**:
- `src/Features/Block/Commands/MoveBlockCommandHandler.cs` - Modify ValidateMove and ExecuteMove for swap logic
- Add `BlocksSwappedNotification` for proper event handling
- `godot_project/features/block/placement/` - Visual feedback for swap preview

**Benefits**: 
- Intuitive drag-and-swap interaction
- Enables more dynamic block arrangements
- Consistent with user expectations from other block-based games
- Atomic operation ensures state consistency

---

## 🧪 Testing & Migration
**Migration Strategy**: Incremental - enhance existing move logic without breaking current functionality

**Testing Requirements**:
- [ ] All existing move tests continue to pass (empty positions)
- [ ] New tests for swap scenarios (occupied positions)
- [ ] Integration tests for swap notifications and view updates
- [ ] Concurrency tests for atomic swap operation

**Rollback Plan**: Revert to original empty-position-only validation if swap logic causes issues

---

## ✅ Acceptance Criteria
- [ ] Dragging block A to position with block B swaps their positions
- [ ] Swap operation is atomic (both blocks move or neither moves)
- [ ] BlocksSwappedNotification is published for proper view updates
- [ ] Swap respects movement range constraints (if block A can't reach B's position)
- [ ] Clear visual preview indicates swap will occur

---

## 🔄 Dependencies & Impact
**Depends On**: 
- VS_001 (drag interaction provides ideal UX for swapping)
- TD_003 (movement range affects swap validity)

**Blocks**: Nothing critical - pure enhancement

**Side Effects**: 
- Changes core move command behavior (needs careful testing)
- May affect game balance if swapping enables new strategies
- Requires animation coordination for dual-block movement

---

## 📝 Progress & Notes

**Current Status**: Design complete in tech-lead plan, awaiting implementation

**Agent Updates**:
- 2025-08-17 - Tech Lead: Specified atomic swap implementation in TD_002 technical plan (Phase 3)

**Blockers**: None - ready for implementation after VS_001 and TD_003

---

## 📚 References
- **Technical Plan**: TD_002_Block_Interaction_Technical_Plan.md (Phase 3 specification)
- **Related Work**: VS_001 (drag interaction), TD_003 (movement constraints)
- **Command Pattern**: Follow existing MoveBlockCommand architecture

---

*Focus on improving code quality and development velocity.*