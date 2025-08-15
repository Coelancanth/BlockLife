# VS_012: Ghost Preview for Move Block

**Status**: Backlog
**Priority**: P2
**Size**: S (4-6 hours)
**Sprint**: After HF_002/HF_003
**Domain**: Block/UI

## User Story
As a player
I want to see a semi-transparent preview of my block at the target position
So that I can accurately place blocks without mistakes or needing to undo

## Vertical Slice Components

### Commands
- No new commands needed - extends existing `MoveBlockCommand`
- Preview state managed locally in presenter/view

### Handlers
- Existing `MoveBlockCommandHandler` remains unchanged
- Preview is purely a presentation concern

### Queries
- `GetPreviewPositionQuery` → Returns `Option<GridPosition>`
  - Calculates valid target position from current input
  - Returns None if position is invalid/occupied

### Notifications
- No new notifications needed
- Preview updates on input events, not domain events

### Domain/DTOs
- `PreviewState`: Record containing:
  - `BlockId`: The block being moved
  - `TargetPosition`: Where it would move to
  - `IsValid`: Whether the position is legal
  - `Opacity`: Float for transparency level (0.3-0.5 typical)

### Presenters
- Update `BlockMovePresenter`:
  - Track preview state during move mode
  - Update preview on mouse movement
  - Clear preview on move completion/cancellation
  - Validate positions using existing validation rules

### Views
- Interface: `IBlockPreviewView` (extends `IBlockAnimationView`)
- Methods:
  - `ShowPreview(BlockId blockId, GridPosition position, float opacity)`
  - `UpdatePreviewPosition(GridPosition position)`
  - `HidePreview()`
  - `SetPreviewValidity(bool isValid)` (red tint if invalid)
- Godot implementation: `godot_project/features/block/move/BlockPreviewView.cs`

## Acceptance Criteria
- [ ] Given a block is selected for moving, When the mouse hovers over a valid position, Then a semi-transparent preview appears at that position
- [ ] Given a preview is showing, When the mouse moves to an invalid position, Then the preview shows with a red tint or different opacity
- [ ] Given a preview is showing, When the player confirms the move, Then the preview disappears and the block moves to that position
- [ ] Given a preview is showing, When the player cancels the move (ESC/right-click), Then the preview disappears and no move occurs
- [ ] Given multiple rapid mouse movements, When updating preview position, Then performance remains above 60 FPS

## Test Requirements

### Architecture Tests
- [ ] Preview logic contained in presentation layer only
- [ ] No domain logic depends on preview state
- [ ] View interface follows existing patterns

### Unit Tests
- [ ] Preview position calculation tests
- [ ] Validity checking against grid boundaries
- [ ] Opacity value range validation (0.0-1.0)
- [ ] Preview state transitions (show/update/hide)

### Property Tests
- [ ] Preview position always matches current mouse grid position
- [ ] Preview never shows at occupied positions (unless cancellable)

### Integration Tests
- [ ] Full mouse movement to preview update flow
- [ ] Preview appearance/disappearance timing
- [ ] Performance under rapid mouse movement
- [ ] Memory cleanup when preview disposed

## Dependencies
- Depends on: VS_000 Phase 1-2 (Move Block core) ✅ COMPLETE
- Depends on: Godot shader/material system for transparency
- Blocks: None

## Implementation Notes
- Reference pattern: `src/Features/Block/Move/` for existing move logic
- Use Godot's `modulate` property or custom shader for transparency
- Consider caching preview mesh/sprite to avoid recreation
- Performance critical: Updates on every mouse move event
- May want to throttle updates to every 2-3 frames if performance issues

## Technical Approach
1. **Rendering Strategy**:
   - Duplicate block's visual representation
   - Apply transparency through Godot's modulate.a or shader
   - Position at target grid position using existing grid-to-world conversion

2. **Input Handling**:
   - Hook into existing GridInteractionController
   - Calculate grid position from mouse using existing methods
   - Update preview position on _input or _process

3. **State Management**:
   - Preview state lives in presenter only
   - No persistence needed
   - Clear on scene changes or mode switches

## Definition of Done
- [ ] All tests passing
- [ ] 60+ FPS with preview active and moving
- [ ] Visual feedback clear and responsive
- [ ] Code follows BlockLife MVP patterns
- [ ] No memory leaks from preview objects
- [ ] PR approved and merged

## References
- Parent Feature: [VS_000 Move Block](VS_000_Move_Block_Feature.md)
- Similar implementations: Most building games (Minecraft, Terraria, etc.)
- Godot transparency docs: [Godot CanvasItem Modulate](https://docs.godotengine.org/en/stable/classes/class_canvasitem.html#class-canvasitem-property-modulate)