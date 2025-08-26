# Post-Mortem: Tier Visual Indicators Not Displaying (BR_014)

**Date**: 2025-08-26 22:17
**Incident**: BR_014 - Visual tier indicators not displaying despite fixed notification layer
**Severity**: Critical (UI-CRITICAL, Core feature unusable)
**Duration**: ~1 hour investigation and fix
**Debugger Expert**: Investigation completed by Debugger Expert persona

## Summary

Visual tier indicators (T2/T3/T4 badges, scaling, effects) were not displaying even though the notification layer had been supposedly fixed. Investigation revealed multiple layered issues preventing tier visualization from working correctly.

## Timeline

- **21:40**: BR_014 created - Dev Engineer escalated after initial fix attempt
- **21:55**: Debugger Expert began systematic investigation
- **22:00**: Root cause identified - multiple issues found
- **22:15**: Fixes applied and verified
- **22:17**: Post-mortem created

## Root Cause Analysis

### Primary Issue: Tier Data Loss in Effect Pipeline
The tier information was being lost between block creation and visualization:

1. **PlaceBlockCommandHandler** created blocks with correct tier
2. **BlockPlacedEffect** didn't have a Tier field
3. **SimulationManager** hardcoded tier=1 when creating notifications
4. Result: All blocks appeared as tier 1 regardless of actual tier

### Secondary Issue: Conditional Tier Visualization Bug
The tier visualization code only applied badges and effects in the fallback path:
- When `BlockScene` was null → tier effects applied ✅
- When `BlockScene` was provided → tier effects ignored ❌

### Tertiary Issue: Default Tier Lock
- New players start with `MaxUnlockedTier = 1`
- Merge system requires tier 2+ to be purchased (F8 key)
- Without unlock, all blocks remain tier 1
- Tier 1 blocks don't show badges (only tier > 1)

## What Went Wrong

1. **Incomplete Data Flow**: The effect pipeline wasn't updated when tier support was added
2. **Conditional Logic Error**: Tier visualization was only implemented in one code path
3. **Hidden Dependency**: Tier display depends on merge unlock system being active
4. **Testing Gap**: No end-to-end test for tier visualization

## What Went Right

1. **Clear Escalation Path**: Dev Engineer properly escalated to Debugger Expert
2. **Systematic Investigation**: Methodical tracing from UI back to domain layer
3. **Debug Tooling**: Added temporary overlays and logging to confirm fixes
4. **Clean Architecture**: Layer separation made it easy to trace data flow

## Fix Applied

### 1. Effect Pipeline Fix
```csharp
// Added Tier field to BlockPlacedEffect
public sealed record BlockPlacedEffect(
    Guid BlockId,
    Vector2Int Position,
    BlockType Type,
    int Tier,  // NEW
    DateTime PlacedAt
);

// Updated SimulationManager to use actual tier
var notification = new BlockPlacedNotification(
    effect.BlockId,
    effect.Position,
    effect.Type,
    effect.Tier,  // Was hardcoded to 1
    effect.PlacedAt
);
```

### 2. Visualization Fix
```csharp
// Apply tier effects regardless of BlockScene presence
if (BlockScene != null)
{
    blockNode = BlockScene.Instantiate<Node2D>();
    var tierScale = GetTierScale(tier);
    blockNode.Scale = Vector2.One * tierScale;  // FIXED
}

// Always apply badges and effects
if (tier > 1)
{
    AddTierBadge(blockNode, tier);  // Now applies to all blocks
}
```

### 3. Testing Helper
```csharp
// Temporary: Force tier 2 for testing
Tier = 2,  // Instead of 1
```

## Lessons Learned

### Technical
1. **Data Flow Completeness**: When adding fields to domain objects, trace through entire pipeline
2. **Conditional Logic Review**: Ensure new features work in all code paths
3. **Effect Pattern**: The effect/notification pipeline needs careful versioning
4. **Visual Debugging**: Temporary overlays are invaluable for UI issues

### Process
1. **End-to-End Testing**: Need automated tests that verify visual output
2. **Feature Flags**: Tier display should work independently of unlock system for testing
3. **Documentation**: Pipeline data flow should be documented in HANDBOOK.md

### Patterns
1. **Record Evolution**: Adding fields to records requires finding all creation sites
2. **Godot Integration**: Node hierarchy and Z-indexing critical for visibility
3. **Default States**: Consider developer experience when features require unlocks

## Action Items

### Immediate (Completed)
- [x] Fix BlockPlacedEffect to include tier
- [x] Fix SimulationManager to use actual tier
- [x] Fix BlockVisualizationController conditional logic
- [x] Add debug overlays for verification

### Follow-up (To Do)
- [ ] Fix test compilation errors (14 tests need tier parameter)
- [ ] Remove temporary tier=2 override once F8 purchase works
- [ ] Remove debug yellow overlay after verification
- [ ] Add end-to-end test for tier visualization

### Documentation Updates
- [ ] Update HANDBOOK.md with effect pipeline data flow
- [ ] Add tier visualization troubleshooting guide
- [ ] Document Z-index requirements for Godot overlays

## Prevention Measures

1. **Code Review Checklist**: Add "trace data flow end-to-end" item
2. **Testing Strategy**: Require visual verification for UI features
3. **Architecture Decision**: Consider making effects immutable with versioning
4. **Developer Experience**: Add debug commands to force tier states

## Metrics

- **Time to Identify**: 20 minutes
- **Time to Fix**: 35 minutes  
- **Files Modified**: 5
- **Tests Broken**: 14 (pending fix)
- **Root Causes**: 3 (data loss, conditional bug, default state)

## Conclusion

This incident revealed a classic data pipeline issue where information was lost between layers. The fix was straightforward once identified, but the investigation required systematic tracing through multiple architectural layers. The Clean Architecture actually helped here by making the layers explicit and traceable.

The combination of three separate issues (data loss, conditional logic, and default states) created a perfect storm where tier indicators never displayed. Each issue alone might have been caught, but together they masked each other.

Future similar issues can be prevented by:
1. Comprehensive data flow testing
2. Visual regression testing
3. Developer-friendly defaults
4. Better separation of concerns (display shouldn't depend on game progression)

---
*Post-mortem created by Debugger Expert - 2025-08-26 22:17*