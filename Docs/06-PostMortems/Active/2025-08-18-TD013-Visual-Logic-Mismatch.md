# Post-Mortem: The Square That Should Have Been a Diamond
**How Visual/Logic Mismatch Breaks User Trust**

**Date**: 2025-08-18  
**Incident**: TD_013 - Drag Range Visual/Logic Mismatch  
**Severity**: Critical (UX Trust Violation)  
**Duration**: Unknown (discovered during Phase 2 implementation)  
**Fixed**: 2025-08-18 (2.5 hours)  

## Executive Summary
The drag range visual indicator displayed a square pattern while validation logic enforced a diamond pattern (Manhattan distance). Users could see invalid moves as valid, breaking the fundamental UI contract: "what you see is what you can do."

## The Bug That Matters

### What Happened
- **Visual**: Drew square range (Chebyshev distance: max(|Δx|, |Δy|) ≤ range)
- **Logic**: Validated diamond range (Manhattan distance: |Δx| + |Δy| ≤ range)
- **Impact**: Corner cells appeared valid but were rejected on drop

### Why It Matters
This wasn't just a rendering bug - it was a **trust violation**. When UI promises something the system rejects, users lose confidence in the entire interface.

## Timeline of Decisions

1. **Phase 1 Implementation**: Drag system built with Manhattan distance validation
2. **Visual Indicator Added**: Square drawn using simple bounds calculation
3. **Phase 2 Started**: Range limits enabled, mismatch became visible
4. **Bug Discovered**: Tech Lead identified during code review
5. **Fix Applied**: Visual updated to match Manhattan distance

## Root Causes

### Immediate Cause
```csharp
// What was implemented (square):
_rangeIndicator.Position = GridToScreenPosition(
    new Vector2Int(centerPosition.X - range, centerPosition.Y - range));
_rangeIndicator.SetSize(new Vector2(rangeSize, rangeSize));

// What was needed (diamond):
for each cell in grid:
    if (ManhattanDistance(cell, center) <= range)
        DrawCell(cell)
```

### Systemic Causes
1. **No Visual Contract**: Acceptance criteria didn't specify visual must match validation
2. **Separated Implementation**: Visual and logic implemented independently
3. **Missing Integration Test**: No test verified visual cells = valid moves
4. **Pattern Confusion**: Manhattan vs Chebyshev not explicitly documented

## Detection Gaps

### What We Had
✅ Unit tests for validation logic  
✅ Visual rendering tests  
❌ Visual/logic consistency tests  
❌ Integration tests between View and Service  
❌ Explicit distance metric documentation  

### What Would Have Caught It
- Test: "All visually indicated cells should pass validation"
- Code review checklist: "Does visual feedback match validation rules?"
- Acceptance criteria: "Range indicator must show exactly the valid move positions"

## Lessons Learned

### 1. Visual Promises Are Contracts
**Principle**: Every visual indicator is a promise to the user. Breaking that promise breaks trust.

**Action**: Add to Definition of Done:
- [ ] Visual feedback matches validation logic
- [ ] Test verifies visual/logic consistency

### 2. Distance Metrics Are Design Decisions
**Manhattan Distance** (diamond):
- Natural for grid movement
- No diagonal shortcuts
- Used in tactics games

**Chebyshev Distance** (square):
- Allows diagonal movement
- King movement in chess
- Used in RTS games

**Action**: Document chosen metric in Architecture.md

### 3. Integration Points Need Integration Tests
**Pattern**: When Component A (View) depends on Component B (Service), test them together.

**Action**: Create test pattern:
```csharp
[Fact]
public void VisualIndicator_MatchesValidationLogic()
{
    // For every cell shown as valid in UI
    // Assert validation service also considers it valid
}
```

## Preventive Measures

### Immediate (This Week)
1. ✅ Add regression test `VisualAndLogicConsistency_UseManhattanDistance_NotChebyshev`
2. 📝 Update VS template to include visual/logic consistency in acceptance criteria
3. 📝 Add "Visual matches logic?" to PR review checklist

### Short-term (This Sprint)
1. 🔍 Audit all existing visual indicators for similar mismatches
2. 📚 Document Manhattan vs Chebyshev in Architecture.md
3. 🧪 Create integration test suite for View/Service pairs

### Long-term (Next Quarter)
1. 🏗️ Consider architectural pattern that couples visual and logic:
```csharp
interface IRangeValidator
{
    bool IsValidPosition(Vector2Int from, Vector2Int to);
    IEnumerable<Vector2Int> GetValidPositions(Vector2Int from);
    void DrawValidArea(IRangeRenderer renderer); // Forces consistency
}
```

## The Bigger Picture

This bug represents a class of issues that will recur in every feature with visual feedback:
- Block abilities with area effects
- Enemy movement previews  
- Building placement zones
- Spell/skill ranges

**Cost Analysis**:
- Cost to fix this bug: 2.5 hours
- Cost if pattern repeats 10 times: 25 hours
- Cost to prevent via process: 1 hour
- **ROI: 24 hours saved**

## Key Takeaways

### For Product Owners
- Include "visual accuracy" in acceptance criteria
- Specify whether ranges are diamond or square shaped

### For Tech Leads  
- Design interfaces that prevent visual/logic drift
- Consider coupling visual and validation code architecturally

### For Developers
- Always verify UI promises match system behavior
- When implementing visuals, read the validation code first

### For Test Specialists
- Add visual/logic consistency to test scenarios
- Create integration tests for View/Service pairs

## Success Metrics

We'll know we've learned from this when:
- Zero visual/logic mismatches in next 5 features
- All PR reviews check visual/logic consistency
- Integration tests exist for all View/Service pairs

## Final Thought

**"The user experiences the UI, not the code."**

A perfect algorithm with wrong visualization is a broken feature. This simple bug reminds us that visual feedback isn't decoration - it's the primary communication channel with our users. When we break visual promises, we break user trust.

---

*This post-mortem follows blameless culture principles. We focus on systems and processes, not individuals. The goal is learning and prevention, not assignment of fault.*