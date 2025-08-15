# Agile Product Owner Agent - Real Examples

## Example 0: Backlog Maintenance (Primary Responsibility)

### Scenario: Architecture Stress Tester Finds Issues

#### Input (from other agent)
"Architecture Stress Test found 3 critical issues:
1. Race condition in notification pipeline
2. Memory leak in presenter disposal
3. State corruption under rapid input"

#### Agent Response

**Creating Work Items:**
```markdown
I've analyzed the stress test results and created the following work items:

Created: Docs/Product_Backlog/backlog/BF_XXX_Notification_Race_Condition.md
- Severity: High
- Size: M (2-3 days)
- Links to: Architecture_Stress_Test_2024_01.md

Created: Docs/Product_Backlog/backlog/BF_XXX_Presenter_Memory_Leak.md  
- Severity: Medium
- Size: S (1 day)
- Links to: Architecture_Stress_Test_2024_01.md

Created: Docs/Product_Backlog/backlog/HF_001_State_Corruption.md
- Severity: CRITICAL
- Size: S (immediate)
- Moved directly to active/ due to critical nature

Updated: Docs/Product_Backlog/Backlog.md
- Added HF_001 to active sprint
- Added BF items to backlog
- Updated sprint capacity
```

### Scenario: Sprint Planning

#### Input
"Start new sprint planning for next 2 weeks"

#### Agent Response

**Sprint Organization:**
```markdown
Updated: Docs/Product_Backlog/Backlog.md

## Sprint 2024-01-15 to 2024-01-29

### Moved to Active
- VS_001_Block_Rotation.md (from ready/)
- VS_002_Inventory_Basic.md (from ready/)
- BF_001_Grid_Boundary_Fix.md (from ready/)

### Remaining in Ready
- VS_003_Block_Destruction.md (next sprint)
- TD_001_Notification_Refactor.md (lower priority)

### Capacity
- Total: 10 dev days
- Allocated: 8 days (VS_001: 2, VS_002: 4, BF_001: 2)
- Buffer: 2 days for unknowns

### Dependencies
- VS_002 depends on VS_001 completion
- BF_001 can be done in parallel
```

## Example 1: Block Rotation Feature

### Input
"I want players to be able to rotate blocks before placing them, with visual preview"

### Agent Output

**File Created:** `Docs/Product_Backlog/backlog/VS_XXX_Block_Rotation.md`

```markdown
# VS_XXX: Block Rotation

**Status**: Backlog
**Size**: S (1-2 days)
**Domain**: Block

## User Story
As a player
I want to rotate blocks 90 degrees on the Y-axis
So that I can orient blocks in different directions

## Vertical Slice Components

### Commands
- `RotateBlockCommand { BlockId, Angle }`
- Validation: Angle must be multiple of 90

### Handlers
- `RotateBlockCommandHandler` → Returns `Fin<RotatedBlock>`
- Error cases: Invalid angle, block not found

### Notifications
- `BlockRotatedNotification { BlockId, NewRotation }`

### Presenters
- `BlockPlacementPresenter` subscribes to rotation events

### Views
- `IBlockPlacementView.UpdatePreviewRotation(rotation)`

## Acceptance Criteria
- [ ] Press R rotates preview 90° clockwise
- [ ] Rotation persists when placed
- [ ] Invalid rotations show error

## Test Requirements
- [ ] Unit: Rotation validation
- [ ] Property: 4 rotations = original
- [ ] Integration: Full rotation flow
```

**Also Updated:** `Docs/Product_Backlog/Backlog.md` - Added to backlog section

#### Story 2: Multi-Axis Rotation
```
As a player
I want to rotate blocks on all three axes
So that I have complete control over block orientation

Acceptance Criteria:
- Given a block is in rotation mode
- When I press R/T/Y for X/Y/Z axis rotation
- Then the block rotates 90° on the specified axis
- And rotation constraints are applied per block type

Technical Components:
- Command: RotateBlockCommand { BlockId, Axis, Angle }
- Property Tests: Rotation matrix properties
- Domain: BlockRotation value object
- Notification: BlockRotatedNotification
```

## Example 2: Inventory System Breakdown

### Input
"Break down an inventory system where players can store, organize, and quick-access items"

### Agent Output

#### Story 1: Basic Inventory Container
```
As a player
I want to open an inventory with fixed slots
So that I can store blocks I've collected

Vertical Slice:
1. View: InventoryPanel.tscn with grid layout
2. Presenter: InventoryPresenter manages slot states
3. Command: OpenInventoryCommand { PlayerId }
4. Query: GetInventoryQuery returns InventoryState
5. Domain: InventorySlot, InventoryCapacity
6. Tests: Capacity limits, slot validation

Priority: HIGH - Foundation for all inventory features
```

#### Story 2: Drag-and-Drop Items
```
As a player
I want to drag items between inventory slots
So that I can organize my inventory

Vertical Slice:
1. View: DraggableSlot nodes with input handling
2. Command: MoveItemCommand { FromSlot, ToSlot }
3. Handler: Validates move, updates state
4. Notification: InventoryUpdatedNotification
5. Property Tests: Slot swap invariants
6. Integration: Full drag-drop cycle test

Priority: MEDIUM - Enhances usability
```

## Example 3: Crafting System Analysis

### Input
"How would a crafting system fit into our CQRS architecture?"

### Agent Output

```
Crafting System - CQRS Architecture Alignment

Command Flow:
1. CraftItemCommand { RecipeId, SourceSlots[] }
   - Immutable record with init setters
   - Validated by CraftItemCommandValidator

2. CraftItemCommandHandler
   - Returns Fin<CraftedItem>
   - Checks recipe requirements
   - Consumes source materials
   - Produces crafted item

3. Notifications:
   - ItemCraftedNotification
   - InventoryUpdatedNotification
   - RecipeUnlockedNotification

4. Queries:
   - GetAvailableRecipesQuery
   - GetRecipeRequirementsQuery
   - CanCraftItemQuery

Architectural Boundaries:
- Core: Recipe logic, crafting rules
- Presentation: Crafting UI, animations
- No Godot dependencies in crafting logic

Reference Pattern:
- Similar to MoveBlockCommand flow
- State changes via commands only
- Read via queries for UI updates
```

## Example 4: Feature Prioritization

### Input
"Prioritize: particle effects, sound system, multiplayer, save/load, achievements"

### Agent Output

```
Priority Order (BlockLife MVP):

1. Save/Load System [MUST HAVE]
   - Critical for player retention
   - Establishes serialization patterns
   - Low architectural risk
   - 3-4 days implementation

2. Sound System [SHOULD HAVE]
   - High player impact
   - Clear architectural boundary
   - Can follow notification pattern
   - 2-3 days implementation

3. Particle Effects [SHOULD HAVE]
   - Visual polish important for feel
   - Integrates with existing systems
   - Uses notification pipeline
   - 2-3 days per effect type

4. Achievements [COULD HAVE]
   - Enhances engagement
   - Builds on notification system
   - Well-defined scope
   - 3-4 days full system

5. Multiplayer [FUTURE]
   - High complexity and risk
   - Requires architecture changes
   - Better after core is stable
   - 2-3 weeks minimum

Rationale:
- Save/Load prevents player frustration
- Audio/Visual before complex systems
- Multiplayer requires stable foundation
```

## Example 5: Bug Fix Story

### Input
"Create a story for fixing blocks that don't update visually after movement"

### Agent Output

```
As a player
I want blocks to update visually immediately after movement
So that the game state matches what I see

Bug Analysis:
- Likely notification pipeline issue
- Handler publishes but presenter doesn't receive

Acceptance Criteria:
- Given a block at position (5,5,5)
- When I move it to (6,5,5)
- Then the visual updates within 1 frame
- And no ghost block remains

Implementation:
1. Add regression test reproducing bug
2. Verify notification subscription in presenter
3. Check static event bridge connection
4. Ensure presenter Dispose() doesn't unsubscribe early

Tests Required:
- Unit: Notification published by handler
- Integration: Full move cycle updates view
- Property: All moves result in visual update

Reference:
- See BlockMovedNotification pattern
- Check Debugging_Notification_Pipeline.md
```

## Usage Tips

1. **Be specific** about gameplay mechanics
2. **Ask for test scenarios** upfront
3. **Request architecture alignment** checks
4. **Get prioritization rationale**
5. **Reference existing patterns** (Move Block)