# Post-Mortem: Worktree File Sync Discrepancy

**Date**: 2025-08-20  
**Author**: Tech Lead  
**Severity**: Medium  
**Impact**: Development confusion, ~30 minutes troubleshooting

## Executive Summary

Encountered a file sync issue where TD_034 in Backlog.md showed different content between git commit history and working directory within a persona worktree. The file showed TD_034 as "Approved" despite being on commit `04552cd` which contained it as "Done" with Sacred Sequence implementation details.

## Timeline

1. **16:15** - User requested sync with main branch from `feat/tech-lead-routing-improvements`
2. **16:20** - Successfully rebased onto main (commit `04552cd`)
3. **16:25** - User noticed discrepancy: TD_034 showing as "Approved" locally but "Done" on GitHub
4. **16:30** - Investigation revealed worktree file content didn't match its HEAD commit
5. **16:35** - Multiple attempts to restore correct file content failed (git checkout, reset)
6. **16:37** - Direct download from GitHub raw content finally resolved the issue

## Root Cause Analysis

### Primary Cause
**Worktree file index corruption**: The worktree at `/personas/tech-lead` had stale file content that didn't match its HEAD commit (`04552cd`). Git believed the file was clean but the actual content was from an older version.

### Contributing Factors

1. **Mixed Repository State**: Main repository at `/blocklife` was on branch `docs/persona-and-postmortem-updates` with uncommitted changes to Backlog.md
2. **Worktree Isolation Breakdown**: File operations may have crossed worktree boundaries
3. **Git Index Desync**: Git's index showed file as clean despite content mismatch

## Technical Details

### What Should Have Happened
- Commit `04552cd` contained TD_034 marked as "Done" with full implementation details
- After rebase, working directory should reflect this version

### What Actually Happened
- Working directory showed older version with TD_034 as "Approved"
- Git status reported "nothing to commit, working tree clean"
- Standard git recovery commands (`checkout`, `reset`) failed to restore correct content

### Resolution Method
```bash
# This finally worked - bypassing git entirely
curl -s https://raw.githubusercontent.com/Coelancanth/BlockLife/main/Docs/01-Active/Backlog.md > Docs/01-Active/Backlog.md
```

## Impact Assessment

### What Went Wrong
- Developer confusion about true state of completed work
- Time lost troubleshooting git state issues
- Potential for incorrect decisions based on stale information

### What Went Right
- Issue was identified quickly through GitHub comparison
- No commits were made with incorrect information
- Resolution was achieved without data loss

## Lessons Learned

1. **Worktree Isolation is Critical**: When using persona worktrees, NEVER touch the main repository directory
2. **Trust Remote Source**: When local state seems corrupted, fetch directly from remote source
3. **Git Index Can Lie**: "Working tree clean" doesn't always mean files match HEAD
4. **Worktree File Sync Issues Are Real**: File content can become stale even when git commits are correct

## Action Items

### Immediate (Completed)
- [x] Restore correct file content from GitHub
- [x] Verify TD_034 shows as "Done" with complete details
- [x] Document the issue in post-mortem

### Short-term (Proposed)
- [ ] **TD_035**: Create worktree health check script that verifies file content matches HEAD
- [ ] **TD_036**: Add warning to CLAUDE.md about not mixing main repo and worktree operations
- [ ] **TD_037**: Implement periodic worktree refresh command in build scripts

### Long-term (Ideas)
- [ ] Consider moving to single-worktree-at-a-time model
- [ ] Investigate git sparse-checkout as alternative to worktrees
- [ ] Add automated verification of critical files (Backlog.md) in CI

## Best Practices Reinforced

1. **Exclusive Worktree Usage**: When using worktrees, stay exclusively in them
2. **Verify Critical Files**: Always verify Backlog.md matches remote after branch operations
3. **Direct Source Recovery**: When git commands fail, fetch directly from authoritative source
4. **Question Local State**: If something seems wrong, it probably is - investigate immediately

## Prevention Measures

To prevent recurrence:

1. **Never mix operations** between main repo and worktrees
2. **Always verify Backlog.md** after rebasing or merging
3. **Use `git show HEAD:path/to/file`** to verify what's actually in commits
4. **When in doubt**, download directly from GitHub

## References

- Original issue: TD_034 showing wrong status after rebase
- Related PR: #50 (Sacred Sequence enforcement)
- Worktree system: TD_023
- Commit with correct content: `04552cd`

## Conclusion

This incident highlights the complexity of git worktrees and the importance of maintaining strict separation between the main repository and persona worktrees. While worktrees provide excellent isolation for parallel development, they can introduce subtle file sync issues that are difficult to diagnose. The key takeaway is to treat the main repository as read-only when using the persona worktree system.