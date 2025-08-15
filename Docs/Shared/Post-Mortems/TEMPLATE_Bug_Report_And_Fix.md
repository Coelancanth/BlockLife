# Bug Report & Fix Template

**Purpose**: Standard template for documenting bugs and their fixes to ensure proper regression testing and learning capture.

---

## 📋 Bug Information

### Basic Details
- **Bug ID**: [Unique identifier, e.g., BUG-001]
- **Date Discovered**: [YYYY-MM-DD]
- **Discovered By**: [Name/Role]
- **Severity**: [Critical/High/Medium/Low]
- **Status**: [Open/In Progress/Fixed/Verified]

### Description
**Symptom**: [What the user/developer observed]

**Expected Behavior**: [What should have happened]

**Actual Behavior**: [What actually happened]

### Impact Assessment
- **User Impact**: [How does this affect end users?]
- **System Impact**: [Does this affect other systems/features?]
- **Business Impact**: [Any business consequences?]

### Bug Triage Matrix
| Criterion | Value | Notes |
|-----------|-------|-------|
| **Severity** | [Critical/High/Medium/Low] | [Impact level explanation] |
| **Frequency** | [Always/Often/Sometimes/Rare] | [How often it occurs] |
| **Priority** | [P0/P1/P2/P3] | [When to fix] |
| **Users Affected** | [Percentage/Count] | [Scope of impact] |

**Triage Decision**: [Fix immediately / Next sprint / Backlog]

## 🔍 Investigation

### Reproduction Steps
1. [Step 1]
2. [Step 2]  
3. [Step 3]
4. [Expected result vs actual result]

### Environment
- **Platform**: [Windows/Mac/Linux]
- **Godot Version**: [e.g., 4.4.1]
- **Build**: [Debug/Release]
- **Branch/Commit**: [Git reference]

### Root Cause Analysis

#### **The "5 Whys" Technique**
1. **Why did [symptom] occur?**
   → [Answer that leads to next why]

2. **Why did [previous answer] happen?**
   → [Answer that leads to next why]

3. **Why did [previous answer] happen?**
   → [Answer that leads to next why]

4. **Why did [previous answer] happen?**
   → [Answer that leads to next why]

5. **Why did [previous answer] happen? (ROOT CAUSE)**
   → [The fundamental systemic issue]

**Primary Cause**: [Summarize the root cause from 5 Whys]

**Contributing Factors**: [Other conditions that enabled the bug]

**Why It Wasn't Caught**: [Analysis of why existing tests/reviews missed this]

### Code Investigation
**Affected Files**:
- `[FilePath]`: [Brief description of issue in this file]
- `[FilePath]`: [Brief description of issue in this file]

**Key Code Snippet** (problematic):
```csharp
// ❌ PROBLEMATIC CODE
public Guid BlockId => RequestedId ?? Guid.NewGuid(); // Generates new GUID each time!
```

## 🛠️ Fix Implementation

### Solution Design
**Fix Strategy**: [High-level approach to solving the problem]

**Alternatives Considered**: [Other solutions that were evaluated]

**Why This Approach**: [Reasoning for chosen solution]

### Implementation
**Fixed Code**:
```csharp
// ✅ FIXED CODE  
private readonly Lazy<Guid> _blockId = new(() => Guid.NewGuid());
public Guid BlockId => RequestedId ?? _blockId.Value; // Stable GUID generation
```

**Files Modified**:
- `[FilePath]`: [Description of changes made]
- `[FilePath]`: [Description of changes made]

### Testing Strategy
**Regression Tests Added**:
```csharp
/// <summary>
/// REGRESSION TEST: [Brief description of what this prevents]
/// 
/// BUG CONTEXT:
/// - Date: [Date]  
/// - Issue: [Brief issue description]
/// - Symptom: [What users saw]
/// - Root Cause: [Technical cause]
/// - Fix: [How it was fixed]
/// 
/// This test prevents regression by verifying:
/// 1. [Specific behavior 1]
/// 2. [Specific behavior 2]
/// 3. [Specific behavior 3]
/// </summary>
[Fact]
public void [TestName]_[Scenario]_[ExpectedBehavior]()
{
    // Arrange
    // Act  
    // Assert - Ensure the bug cannot reoccur
}
```

**Test Files Modified**:
- `[TestFilePath]`: [Tests added/modified]

### Validation Results
- **All Tests Pass**: [✅/❌] 
- **Manual Testing**: [✅/❌] - [Description of manual verification]
- **Performance Impact**: [None/Minimal/Measured improvement]
- **Breaking Changes**: [None/List any breaking changes]

## 📚 Learning & Prevention

### Lessons Learned
1. **Technical**: [What we learned about the codebase/architecture]
2. **Process**: [What we learned about our development process]  
3. **Testing**: [What gaps this revealed in our testing strategy]

### Architecture Implications  
**Should Architecture Change?**: [Yes/No - reasoning]

**Similar Vulnerabilities**: [Are there other places with similar risks?]

**Prevention Strategies**: [How can we prevent this class of bugs?]

### Process Improvements
**Code Review**: [Should review process be enhanced?]

**Testing Gaps**: [What types of tests would have caught this?]

**Monitoring**: [Should we add monitoring/alerts for this scenario?]

### Future Actions
- [ ] [Action item 1]
- [ ] [Action item 2]  
- [ ] [Action item 3]

## 📊 Impact Metrics

### Before Fix
- **Bug Frequency**: [How often did this occur?]
- **Time to Discover**: [How long between introduction and discovery?]
- **Time to Fix**: [How long to implement fix?]

### After Fix  
- **Test Coverage**: [New test coverage added]
- **Similar Issues**: [Number of similar issues found and fixed]
- **Confidence Level**: [High/Medium/Low confidence this won't reoccur]

## 🔗 References

### Related Issues
- **Similar Bugs**: [Links to related bug reports]
- **Architecture Decisions**: [Relevant ADRs]
- **Documentation**: [Related docs that were updated]

### External References
- **Stack Overflow**: [Any external research links]
- **GitHub Issues**: [Related open source issues]  
- **Documentation**: [Framework/library docs consulted]

---

## ✅ Verification Checklist

**Before Marking as Complete**:
- [ ] Root cause fully understood and documented
- [ ] Fix implemented and tested
- [ ] Regression tests added with detailed bug context
- [ ] All existing tests still pass
- [ ] Manual testing confirms fix works
- [ ] Documentation updated if needed
- [ ] Similar code locations reviewed for same vulnerability
- [ ] Team informed of lessons learned
- [ ] Process improvements identified and tracked

**Quality Gates**:
- [ ] **Tests as Living Documentation**: Regression tests clearly document the bug and prevent reoccurrence
- [ ] **Knowledge Preservation**: Sufficient detail for future developers to understand the issue  
- [ ] **Prevention Focus**: Clear actions taken to prevent similar issues

---

**Template Version**: 1.0  
**Last Updated**: 2025-08-14  
**Next Review**: After 5 bug reports using this template