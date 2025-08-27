# Post-Mortem: Merge Pattern Over-Engineering Incident

**Date**: 2025-08-25 20:05
**Author**: Dev Engineer
**Severity**: Medium (Architectural Debt)
**Impact**: 369 lines of unnecessary code, wrong game mechanics

## Timeline of Events

1. **2025-08-25 ~18:00** - Started implementing VS_003B-1 "TierUp Pattern Recognition"
2. **2025-08-25 ~19:00** - Created TierUpPatternRecognizer with "exactly 3 blocks" logic
3. **2025-08-25 ~19:40** - Marked VS_003B-1 as complete with 29 passing tests
4. **2025-08-25 20:00** - User identified fundamental issues:
   - Should support 3+ blocks, not exactly 3
   - Should reuse match patterns, not create new ones
   - Wrong terminology (TierUp vs Merge)

## What Went Wrong

### Root Cause: Assumed Complexity Without Checking Requirements

**Primary Failure**: Built a complex separate pattern recognizer when the solution was a simple boolean check on existing patterns.

**Contributing Factors**:
1. **Didn't consult Glossary first** - Used "TierUp" terminology from backlog instead of canonical "Merge" term
2. **Misunderstood requirements** - Assumed "exactly 3" was intentional design instead of questioning it
3. **Pattern blindness** - Saw "new pattern type" and immediately created new recognizer class
4. **Over-architected** - Built 369 lines for what should have been a 5-line unlock check

## The Incorrect Implementation

```csharp
// What we built (WRONG):
public class TierUpPatternRecognizer : IPatternRecognizer {
    // 369 lines of complex "exactly 3" detection logic
    private Seq<Seq<Vector2Int>> FindExactlyThreeConnected(...) {
        // Complex algorithm to find ONLY groups of 3
    }
}
```

## The Correct Solution

```csharp
// What we should have built:
public class PatternExecutionResolver {
    public IPatternExecutor ResolveExecutor(IPattern pattern) {
        if (pattern is MatchPattern match) {
            // Just check if merge is unlocked for this tier!
            if (mergeUnlockService.IsMergeUnlocked(match.BlockTier)) {
                return mergeExecutor;  // Reuse match pattern, different executor
            }
            return matchExecutor;
        }
    }
}
```

## Specific Mistakes

1. **Terminology Error**: Used "TierUp" throughout when Glossary clearly states "Merge"
2. **Logic Error**: Exactly 3 blocks makes no sense - why would 4 blocks not merge?
3. **Architecture Error**: Created parallel pattern system instead of reusing existing one
4. **Process Error**: Didn't challenge obviously questionable requirements

## Impact Assessment

- **Code Debt**: 369 lines of unnecessary code to maintain
- **Bug Risk**: Players will report 4+ blocks not merging as a bug
- **Confusion**: Wrong terminology throughout codebase
- **Testing Burden**: 29 tests for the wrong behavior

## How It Should Have Been Caught

1. **Glossary Check**: Should have verified terminology BEFORE coding
2. **Requirements Review**: "Exactly 3" should have triggered immediate questioning
3. **Design Review**: Should have asked "Can we reuse MatchPattern?"
4. **Simplicity Check**: 369 lines for a feature? Red flag!

## Lessons Learned

### For Dev Engineer (Me)
1. **ALWAYS check Glossary before naming anything**
2. **Question requirements that seem arbitrary** (exactly 3? why?)
3. **Look for reuse opportunities before creating new systems**
4. **If solution > 100 lines, stop and reconsider approach**

### For the Team Process
1. **Backlog items should reference Glossary terms**
2. **Requirements should explain "why" for unusual constraints**
3. **Complex features need design review before implementation**

## Prevention Measures

1. **Pre-coding Checklist**:
   - [ ] Checked Glossary for correct terminology?
   - [ ] Questioned any "exactly N" requirements?
   - [ ] Looked for existing patterns to reuse?
   - [ ] Estimated lines of code (>100 = review first)?

2. **Code Review Focus**:
   - New pattern classes = automatic design review
   - Check terminology against Glossary
   - Question complexity for "simple" features

3. **Backlog Improvements**:
   - Add "Glossary Terms" field to backlog items
   - Require "Why" explanation for specific constraints
   - Flag items that create new patterns for Tech Lead review

## Fix Plan

1. Delete TierUpPatternRecognizer entirely
2. Rename all "TierUp" references to "Merge"
3. Add merge unlock check to existing match execution
4. Update tests to verify 3+ blocks work correctly
5. Update backlog with correct terminology

## Key Takeaway

**The best code is often no code.** Before building a complex solution, always ask: "Can I solve this by adding one condition to existing code?"

In this case, we built an entire pattern recognition system when all we needed was:
```csharp
if (mergeUnlocked) { merge(); } else { match(); }
```

---

*Remember: Questioning requirements and pushing for simplicity isn't resistance - it's engineering excellence.*