# VS_001 Phase 3: Swap Mechanic - E2E Testing Guide

## Implementation Summary
VS_001 Phase 3 (Swap Mechanic) has been implemented and is ready for E2E testing in Godot.

### UPDATE: View Update Issue Fixed
- **Issue**: View wasn't updating during swaps
- **Cause**: Direct GridStateService calls bypassed notifications
- **Fix**: Added explicit BlockMovedNotification publishing for both blocks in swap
- **Result**: Both blocks should now animate correctly during swap

## What Was Implemented

### 1. **Swap Mechanic Core Logic**
- When dragging a block onto an occupied cell, blocks now swap positions
- Swap validation ensures both blocks can reach each other's positions (Manhattan distance ≤ 3)
- Atomic 3-step swap process prevents position conflicts

### 2. **Block Type Color System**
- Added distinct colors for different block types for visual testing:
  - **Work** (Royal Blue): #4169E1
  - **Study** (Lime Green): #32CD32  
  - **Health** (Tomato Red): #FF6347
  - **Fun** (Gold): #FFD700
  - **Basic** (Gray): #808080

### 3. **Block Type Cycling**
- **Press TAB** to cycle through block types when placing
- Console will show current block type and color
- Makes it easy to create visually distinct blocks for testing swaps

## How to Test

### Basic Controls
- **Space**: Place a block at cursor position
- **TAB**: Cycle block type (changes color of next placed block)
- **I**: Inspect block at cursor
- **Click & Drag**: Move blocks (with range 3 limitation)

### Testing the Swap Mechanic

1. **Setup Test Scenario**:
   - Place a blue block (Work type) at position A
   - Press TAB to switch to green blocks (Study type)
   - Place a green block at position B (within 3 cells of A)

2. **Test Valid Swap**:
   - Click and drag the blue block
   - Drop it on the green block
   - Both blocks should swap positions smoothly

3. **Test Invalid Swap (Out of Range)**:
   - Place blocks more than 3 cells apart
   - Try to drag one onto the other
   - Swap should be rejected (blocks too far apart)

4. **Test Edge Cases**:
   - Swap at exactly range 3 (should work)
   - Swap with diagonal positions
   - Multiple swaps in sequence

### Expected Behaviors

✅ **Valid Swap**:
- Blocks within Manhattan distance 3 can swap
- Smooth animation for both blocks
- No visual glitches or position conflicts

❌ **Invalid Swap**:
- Blocks beyond range 3 cannot swap
- Dragged block returns to original position
- Clear feedback about why swap failed (check console)

### Visual Verification
- Different colored blocks make swaps obvious
- Animation should be smooth (adjustable with F10/F11)
- No blocks should disappear or duplicate

## Debug Keys
- **F9**: Print performance report
- **F10**: Toggle animations on/off
- **F11**: Cycle animation speed

## Console Output
The console will show:
- Current block type when pressing TAB
- Swap attempts and results
- Range validation messages
- Any errors or warnings

## Known Implementation Details
- Swap uses Manhattan distance (diamond shape) for range checking
- Both blocks must be within range of each other's positions
- Swap is atomic - either both blocks move or neither does

## Test Coverage
Unit tests verify:
- ✅ Swap within range succeeds
- ✅ Swap out of range fails  
- ✅ Swap at exact max range (3) succeeds
- ✅ All 145 tests passing

## Next Steps
After E2E testing:
1. Report any visual issues or unexpected behaviors
2. Test specialist marks VS_001 Phase 3 as Done if tests pass
3. Move to next priority item in backlog

---
*Ready for E2E testing in Godot! Use different colored blocks to clearly see the swap mechanic in action.*