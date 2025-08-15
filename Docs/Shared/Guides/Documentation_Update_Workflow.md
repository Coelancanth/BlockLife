# Documentation Update Workflow

## 📋 When to Update Documentation

Documentation updates are **MANDATORY** at specific points in the development workflow. This ensures knowledge is captured while it's fresh and accurate.

## 🔄 Documentation Update Points

### 1. **BEFORE Starting Work** (Planning Phase)
**When**: After creating feature branch, before coding

**Update**:
- [ ] Check if implementation plan exists in `Docs/3_Implementation_Plans/`
- [ ] Create/update implementation plan if needed
- [ ] Update `Docs/0_Global_Tracker/Implementation_Status_Tracker.md` status to "In Progress"

**Example**:
```bash
git checkout -b feat/block-animation
# STOP! Update docs first:
# 1. Create Docs/3_Implementation_Plans/Block_Animation_Implementation_Plan.md
# 2. Update tracker status
# THEN start coding
```

### 2. **DURING Development** (As You Code)
**When**: While implementing features

**Update**:
- [ ] Add inline code comments for complex logic
- [ ] Document new patterns discovered
- [ ] Note any deviations from original plan
- [ ] Create bug reports for issues found

**What to Document**:
```csharp
/// <summary>
/// PATTERN: Static Event Bridge for MediatR to Presenter communication
/// This bridges the gap between MediatR notifications and Presenter events
/// </summary>
public static class BlockPlacementNotificationBridge
{
    public static event Action<BlockPlacedNotification>? BlockPlacedEvent;
    // Document WHY this pattern is needed
}
```

### 3. **AFTER Fixing Bugs** (Bug-to-Test Protocol)
**When**: Immediately after fixing any bug

**MANDATORY Updates**:
1. [ ] Create bug report in `Docs/4_Post_Mortems/`
2. [ ] Add regression test with bug context
3. [ ] Update relevant documentation that was incorrect
4. [ ] Update CLAUDE.md if new patterns emerged

**Example**:
```csharp
/// <summary>
/// REGRESSION TEST: Ensures BlockId remains stable across multiple accesses
/// 
/// BUG CONTEXT:
/// - Date: 2025-08-14
/// - Issue: BlockId was generating new GUID on each property access
/// - Fix: Use Lazy<Guid> for stable ID generation
/// </summary>
[Fact]
public void BlockId_RemainsStable() { }
```

### 4. **BEFORE Committing** (Pre-Commit Checklist)
**When**: After code complete, before git commit

**Update Checklist**:
- [ ] Implementation plan status (mark phases complete)
- [ ] API documentation (XML comments)
- [ ] Test documentation (explain what/why)
- [ ] CLAUDE.md if new patterns/constraints
- [ ] Architecture docs if design changed

**Verification**:
```bash
# Before commit, ask yourself:
# 1. Would a new developer understand this?
# 2. Are all decisions documented?
# 3. Is the "why" explained?
```

### 5. **AFTER Feature Complete** (Knowledge Capture)
**When**: Feature fully implemented and tested

**Final Updates**:
- [ ] Mark implementation plan as "Completed"
- [ ] Update `Implementation_Status_Tracker.md`
- [ ] Add to `DOCUMENTATION_CATALOGUE.md` if new docs created
- [ ] Update architecture guides if patterns changed
- [ ] Create ADR if significant decisions made

## 📝 Documentation Types & When to Update

### Implementation Plans
**Update**: BEFORE starting, AFTER completing phases
```markdown
## Phase 1: Core Command Implementation ✅ COMPLETED (2025-08-14)
- [x] Create MoveBlockCommand
- [x] Implement handler with validation
```

### Bug Reports
**Update**: IMMEDIATELY after fixing bugs
```markdown
# Bug Report: [Description]
**Bug ID**: BUG-XXX
**Status**: ✅ Fixed & Verified
```

### CLAUDE.md
**Update**: When patterns/constraints change
```markdown
## Critical Patterns
**NEW PATTERN**: SimulationManager owns all notification publishing
**Why**: Prevents duplicate notifications (see BUG-003)
```

### Test Documentation
**Update**: WITH the test code
```csharp
/// Document:
/// - What scenario is tested
/// - Why this test exists
/// - Bug context if regression test
```

### Architecture Guides
**Update**: When design patterns evolve
```markdown
### Notification Pipeline (Updated 2025-08-14)
Changed: SimulationManager is single source of notification publishing
Reason: Prevent duplicate notifications
```

## 🚀 Quick Documentation Workflow

### For New Features
```bash
# 1. Create branch
git checkout -b feat/new-feature

# 2. DOCUMENT FIRST
vim Docs/3_Implementation_Plans/New_Feature_Implementation_Plan.md

# 3. Code with TDD
# ... implement feature ...

# 4. UPDATE DOCS
# - Mark phases complete
# - Document patterns found
# - Update CLAUDE.md if needed

# 5. Commit with docs
git add .
git commit -m "feat: implement new feature

- Implementation plan updated
- New patterns documented
- Tests include context"
```

### For Bug Fixes
```bash
# 1. Fix the bug
# ... fix code ...

# 2. IMMEDIATELY document
vim Docs/4_Post_Mortems/Bug_XXX_Report.md

# 3. Add regression test WITH context
# ... write test with bug documentation ...

# 4. Commit with documentation
git commit -m "fix: resolve bug XXX

See Docs/4_Post_Mortems/Bug_XXX_Report.md for details
Added regression test to prevent recurrence"
```

## 📊 Documentation Quality Checklist

Before marking work complete:

### Code Documentation
- [ ] All public APIs have XML comments
- [ ] Complex logic has inline comments
- [ ] Test methods explain scenarios
- [ ] Bug context in regression tests

### Project Documentation
- [ ] Implementation plans updated
- [ ] Bug reports created for fixes
- [ ] Architecture guides current
- [ ] CLAUDE.md reflects reality

### Knowledge Preservation
- [ ] The "why" is documented
- [ ] Decisions are captured
- [ ] Patterns are explained
- [ ] Lessons learned recorded

## 🔴 Common Documentation Mistakes

### ❌ DON'T: Update docs "later"
```bash
git commit -m "feat: add feature"
# "I'll document it tomorrow" <- Never happens
```

### ✅ DO: Document as you go
```bash
# Update plan: "Starting Phase 1"
# Code feature
# Update plan: "Phase 1 complete"
git commit -m "feat: add feature (Phase 1 complete)"
```

### ❌ DON'T: Only document the "what"
```csharp
// Sets the block position <- Useless
block.Position = position;
```

### ✅ DO: Document the "why"
```csharp
// Position must be set before validation to ensure
// grid bounds checking has current location
block.Position = position;
```

### ❌ DON'T: Create orphan documentation
```
Docs/RandomNotes.md <- No one will find this
```

### ✅ DO: Put docs in standard locations
```
Docs/3_Implementation_Plans/Feature_Plan.md
Docs/4_Post_Mortems/Bug_Report.md
```

## 🎯 Key Principle

**Documentation is part of the deliverable, not an afterthought.**

Every commit should include:
1. **Code changes** (the what)
2. **Tests** (proof it works)
3. **Documentation** (the why and how)

## 📈 Success Metrics

Good documentation workflow means:
- New developers can understand decisions
- Bugs don't repeat (documented in tests)
- Patterns are discoverable
- Knowledge isn't lost when developers leave

---

**Remember**: If it's not documented, it didn't happen. If the "why" isn't documented, it will be questioned.

**Document Version**: 1.0  
**Created**: 2025-08-14  
**Audience**: All BlockLife developers  
**Review**: With each retrospective