# TD_XXX: [Tech Debt Description]

**Status**: Backlog
**Priority**: TBD
**Size**: [S/M/L] ([1-5] days)
**Sprint**: TBD
**Debt Type**: [Performance/Maintainability/Security/Architecture]
**Impact**: [High/Medium/Low]

## Problem Description
[What technical debt exists and why it's problematic]

## Current State
- Location: `[file/component/system]`
- Issue: [Technical description]
- Impact on development: [How it slows/complicates work]
- Risk if not addressed: [What could go wrong]

## Proposed Solution

### Refactoring Approach
1. [Step 1]
2. [Step 2]
3. [Step 3]

### Code Changes
- File: `[file path]`
  - Current: [Current approach]
  - Proposed: [New approach]
  - Benefit: [Why it's better]

### Architecture Changes (if any)
- Pattern change: [From X to Y]
- Dependency updates: [List changes]
- Interface modifications: [List changes]

## Benefits
- **Performance**: [Improvement expected]
- **Maintainability**: [How it helps]
- **Testability**: [Testing improvements]
- **Developer Experience**: [DX improvements]

## Migration Strategy
- [ ] Can be done incrementally: [Yes/No]
- [ ] Backward compatible: [Yes/No]
- [ ] Feature flag needed: [Yes/No]
- Migration steps:
  1. [Step 1]
  2. [Step 2]

## Test Requirements

### Existing Tests
- [ ] All existing tests must pass
- [ ] Performance benchmarks before/after

### New Tests
- [ ] Unit tests for refactored code
- [ ] Integration tests for new patterns
- [ ] Architecture tests for new constraints

## Acceptance Criteria
- [ ] Code follows new pattern
- [ ] No functionality regression
- [ ] Performance improved/maintained
- [ ] Documentation updated
- [ ] Team trained on new approach

## Risk Assessment
- **Breaking changes**: [List potential breaks]
- **Performance risk**: [Could it get worse?]
- **Rollback plan**: [How to revert if needed]

## Measurement
- Metric 1: [What to measure]
- Metric 2: [What to measure]
- Success criteria: [Specific targets]

## Dependencies
- Must complete before: [List dependent work]
- Can be done alongside: [List compatible work]
- Blocks: [What this enables]

## Definition of Done
- [ ] Refactoring complete
- [ ] All tests passing
- [ ] Performance validated
- [ ] Code reviewed by tech lead
- [ ] Documentation updated
- [ ] Team notified of changes

## References
- Related ADR: [Architecture decisions]
- Original implementation: [VS_XXX if applicable]
- Tech debt analysis: [Link to analysis]
- Similar refactoring: [Previous similar work]