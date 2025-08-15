# BF_XXX: [Bug Description]

**Status**: Backlog
**Severity**: [Critical/High/Medium/Low]
**Priority**: TBD
**Size**: [S/M/L] ([1-5] days)
**Sprint**: TBD
**Found In**: [Version/Commit/Date]
**Reported By**: [Agent/Tester/User]

## Problem Description
[Clear description of the bug and its impact]

## Reproduction Steps
1. [Step 1]
2. [Step 2]
3. [Step 3]
4. Expected: [What should happen]
5. Actual: [What actually happens]

## Root Cause Analysis
- Component affected: [Component name]
- Cause: [Technical explanation]
- Why it wasn't caught: [Testing gap analysis]

## Fix Approach

### Code Changes
- File: `[file path]`
  - Change: [Description]
- File: `[file path]`
  - Change: [Description]

### Affected Components
- Commands: [List if any]
- Handlers: [List if any]
- Presenters: [List if any]
- Views: [List if any]

## Test Requirements

### Regression Test
```csharp
[Test]
public void Should_[TestName]_When_[Condition]()
{
    // Test that would have caught this bug
}
```

### Additional Tests
- [ ] Unit test for fix
- [ ] Integration test for scenario
- [ ] Stress test if performance-related
- [ ] Property test if invariant-related

## Acceptance Criteria
- [ ] Bug no longer reproducible
- [ ] Regression test passes
- [ ] No new issues introduced
- [ ] Performance not degraded

## Verification Steps
1. [How to verify fix works]
2. [Additional verification]
3. [Edge cases to check]

## Risk Assessment
- **Regression risk**: [Low/Medium/High]
- **Related areas**: [List areas that might be affected]
- **Testing needed**: [Specific test scenarios]

## Definition of Done
- [ ] Root cause identified
- [ ] Fix implemented
- [ ] Regression test added
- [ ] All tests passing
- [ ] Code reviewed
- [ ] Verified in test environment

## References
- Original feature: [VS_XXX if applicable]
- Bug report: [Link to detailed report]
- Related bugs: [List if any]
- Architecture stress test: [Link if found by stress test]