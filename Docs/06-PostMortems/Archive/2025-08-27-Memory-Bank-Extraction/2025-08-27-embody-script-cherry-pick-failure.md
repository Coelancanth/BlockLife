# Post-Mortem: Critical Data Loss Bug in embody.ps1 Cherry-Pick Logic

**Date**: 2025-08-27 01:47  
**Severity**: CRITICAL  
**Impact**: Near-catastrophic data loss - 28 commits at risk  
**Author**: DevOps Engineer  
**Status**: FIXED  

## Executive Summary

The embody.ps1 script had a critical bug that caused it to fail when preserving commits after a squash merge, potentially losing hours or days of work. The bug was in the cherry-pick logic using incorrect git rev-list syntax (three dots instead of two), causing conflicts with already-squashed commits.

## Timeline

- **2025-08-26 ~23:00**: User completed significant work with multiple commits after PR #75 was squash-merged
- **2025-08-27 01:41**: User ran embody.ps1 which detected squash merge but failed during cherry-pick
- **2025-08-27 01:43**: DevOps Engineer identified 28 commits at risk of permanent loss
- **2025-08-27 01:44**: Created rescue branches to preserve at-risk commits
- **2025-08-27 01:46**: Fixed the bug in embody.ps1
- **2025-08-27 01:47**: Verified fix works correctly

## Root Cause Analysis

### Surface Problem
The embody.ps1 script failed with error:
```
error: could not apply 0c810bd... feat(VS_003): Add BlockLibrary resource pattern to merge system
Failed to preserve commit 0c810bd8afc78dbb99a64988c61710b8d4c3ec58 - manual intervention required
```

### Root Cause
**Incorrect git rev-list syntax** in embody.ps1:
```powershell
# BUG: Three dots (...) returns symmetric difference
$commits = git rev-list --reverse origin/$branch...$tempBranch

# CORRECT: Two dots (..) returns commits only in tempBranch
$commits = git rev-list --reverse origin/main..$tempBranch
```

The three-dot syntax (`...`) returns the symmetric difference between branches, including commits that exist in different forms on both branches. After a squash merge, this caused the script to try cherry-picking commits that were already included in the squashed commit, creating conflicts.

### Contributing Factors
1. **Inadequate error recovery**: When cherry-pick failed, the script just errored out without helping users recover
2. **Poor conflict detection**: Didn't distinguish between real conflicts vs already-applied commits
3. **Destructive cleanup**: Would delete temp branch even when recovery failed
4. **Insufficient testing**: The bug wasn't caught because it only manifests after squash merges with continued work

## Impact Assessment

### What Could Have Been Lost
- 28 commits of work spanning multiple features and bug fixes
- Approximately 8-10 hours of development work
- Critical fixes including:
  - Same-tier matching enforcement
  - Visual system debug cleanup
  - Test compilation fixes (TD_082)
  - Defensive programming improvements

### Actual Impact
- No permanent data loss (commits recoverable via reflog)
- ~30 minutes of user disruption
- Loss of trust in automation tooling

## The Fix

### Changes Made
1. **Fixed git rev-list syntax**: Changed from three dots to two dots
2. **Added graceful error handling**: Script now skips conflicts and reports them
3. **Preserve rescue branches**: Temp branches kept when recovery partially fails
4. **Better user guidance**: Clear instructions for manual recovery if needed

### Code Changes
```powershell
# BEFORE (BROKEN):
$commits = git rev-list --reverse origin/$branch...$tempBranch
foreach ($commit in $commits) {
    git cherry-pick $commit
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to preserve commit $commit - manual intervention required"
        return $false
    }
}

# AFTER (FIXED):
$commits = git rev-list --reverse origin/main..$tempBranch
$failedCommits = @()
$successCount = 0

foreach ($commit in $commits) {
    $commitMsg = git log -1 --pretty=format:"%h %s" $commit
    git cherry-pick $commit 2>$null
    if ($LASTEXITCODE -ne 0) {
        $conflicts = git diff --name-only --diff-filter=U
        if ($conflicts) {
            Write-Warning "Conflict in commit: $commitMsg"
            git cherry-pick --skip 2>$null
            $failedCommits += $commitMsg
        } else {
            git cherry-pick --skip 2>$null
            Write-Warning "Skipped (possibly already applied): $commitMsg"
        }
    } else {
        $successCount++
        Write-Success "Preserved: $commitMsg"
    }
}

# Keep temp branch if recovery was partial
if ($failedCommits.Count -gt 0) {
    Write-Host "Your original commits are safe in branch: $tempBranch"
    Write-Host "To manually recover: git cherry-pick <commit-hash>"
} else {
    git branch -D $tempBranch 2>$null
}
```

## Lessons Learned

### What Went Wrong
1. **Assumption about git syntax**: Three dots vs two dots is a subtle but critical difference
2. **Insufficient error scenarios testing**: Didn't test squash merge + continued work scenario
3. **All-or-nothing approach**: Script would fail completely instead of partial recovery
4. **No safety net**: No automatic backup before destructive operations

### What Went Right
1. **Quick detection**: User noticed immediately and reported the issue
2. **Git reflog saved us**: All commits were recoverable despite the bug
3. **Fast resolution**: Fixed within 15 minutes of discovery
4. **Comprehensive fix**: Addressed root cause and added defensive measures

## Prevention Measures

### Immediate Actions Taken
1. ✅ Fixed the cherry-pick syntax bug
2. ✅ Added graceful error handling
3. ✅ Preserved rescue branches on partial failure
4. ✅ Added clear recovery instructions

### Future Improvements Needed
1. **Add automated tests** for squash merge scenarios
2. **Create backup branch** before any reset operations
3. **Add dry-run mode** for testing sync operations
4. **Implement transaction-like behavior** - all or rollback
5. **Add telemetry** to detect how often this scenario occurs

## Recovery Instructions (If This Happens Again)

If embody.ps1 fails during cherry-pick:

1. **DON'T PANIC** - Your commits are safe in git reflog
2. **Find lost commits**: `git reflog -20`
3. **Create rescue branch**: `git branch rescue-YYYY-MM-DD <last-good-commit>`
4. **List commits to recover**: `git log --oneline <last-good-commit> ^origin/main`
5. **Cherry-pick selectively**: `git cherry-pick <commit-hash>`
6. **Skip conflicts**: `git cherry-pick --skip` for already-applied commits

## Recommendations

1. **HIGH PRIORITY**: Add comprehensive test suite for embody.ps1
2. **MEDIUM PRIORITY**: Implement automatic backup before destructive operations
3. **LOW PRIORITY**: Add telemetry to track sync scenarios and success rates

## Conclusion

This incident revealed a critical flaw in our most important automation tool. While no permanent data loss occurred, the potential for catastrophic loss was very real. The swift resolution and comprehensive fix demonstrate good incident response, but we must implement better testing and safeguards to prevent similar issues.

**Key Takeaway**: Even "simple" git operations require careful consideration of edge cases, especially around squash merges which fundamentally rewrite history.

---

*This post-mortem focuses on learning and prevention, not blame. The bug existed since the squash merge handler was added, affecting all users of the embody script.*