# Subagent Verification Quick Reference

## 🎯 Trust but Verify Protocol

When a subagent completes work, perform these lightweight checks based on the subagent type:

### backlog-assistant Verification
```bash
# Quick 10-second verification:
git status                    # Was Backlog.md modified?
grep "TD_041" Backlog.md      # Is the new item actually there?
grep "Status: Completed"       # Did status actually change?
```

### general-purpose Verification
```bash
# Verify output matches request:
- Did it find the files you expected?
- Are the code changes where they should be?
- Does the analysis match the codebase state?
```

### output-style-setup Verification
```bash
# Check style is active:
- Look for style confirmation in response
- Verify formatting matches requested style
- Check if style persists across messages
```

### statusline-setup Verification
```bash
# Confirm settings applied:
- Check statusline displays requested info
- Verify configuration was saved
- Test persistence across sessions
```

## 🚀 Quick Verification Patterns

### Pattern 1: File Modification Check
**When to use**: Subagent claims to have modified files
**How to verify**: `git status` or `ls -la [filename]`
**Red flag**: No changes shown when changes were reported

### Pattern 2: Content Presence Check
**When to use**: Subagent claims to have added/updated content
**How to verify**: `grep` for specific strings or review the file
**Red flag**: Expected content missing or unchanged

### Pattern 3: Status Consistency Check
**When to use**: Subagent reports status changes
**How to verify**: Check if old status is gone and new status exists
**Red flag**: Both old and new status present (duplicate)

## ⚡ 10-Second Verification Rule

If verification takes more than 10 seconds, it's too complex. The goal is a quick sanity check, not a full audit.

### Good Verification (Quick)
```bash
git status | grep Backlog.md  # 2 seconds
```

### Bad Verification (Too Complex)
```bash
# Don't do full diffs and detailed analysis
git diff HEAD~1 Backlog.md | analyze...  # Too much
```

## 🎨 Integration with Personas

Each persona should:
1. Read the subagent report
2. Perform 10-second verification
3. Summarize findings to user
4. Note any discrepancies found

### Example Integration
```markdown
After reading subagent report:
- ✅ Verified: Backlog.md was modified
- ✅ Verified: TD_041 added to Important section
- ⚠️ Note: Status still shows "Proposed" not "Approved"

Summary: Subagent completed most updates but missed status change.
```

## 📊 Common False Completions

### Type 1: Partial Updates
**Symptom**: Some changes made, others missed
**Verification**: Check each claimed change individually
**Example**: "Created TD item" but forgot to increment counter

### Type 2: Wrong Location
**Symptom**: Changes made in wrong section/file
**Verification**: Check expected vs actual location
**Example**: Added to Ideas instead of Important

### Type 3: Format Issues
**Symptom**: Content added but formatting broken
**Verification**: Visual scan for structure
**Example**: Missing required fields in template

## 🔑 Key Principle

**Trust but Verify** means:
- Trust the subagent did its best
- Verify the critical changes happened
- Report discrepancies without blame
- Focus on what needs fixing, not what went wrong

Remember: The goal is confidence in completion, not perfection in process.