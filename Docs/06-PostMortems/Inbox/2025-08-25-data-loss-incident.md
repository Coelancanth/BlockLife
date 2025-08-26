# Post-Mortem: Data Loss Incident - embody.ps1 Squash Merge Handler
**Date**: 2025-08-25
**Severity**: CRITICAL
**Duration**: ~45 minutes (18:00 - 18:45 local time)
**Author**: DevOps Engineer

---

## Executive Summary

On August 25, 2025, the embody.ps1 script caused data loss by deleting 4 unpushed commits when handling a squash merge. The commits were recovered via git reflog, and a fix was implemented to prevent future occurrences. No permanent data loss occurred, but the incident revealed a critical flaw in our git automation tooling.

---

## Timeline

| Time | Event |
|------|-------|
| 16:34 | PR #74 merged via squash merge to main |
| 16:35-18:00 | User continues working locally, creates 4 new commits |
| ~18:00 | User runs `embody @devops-engineer` |
| 18:01 | embody.ps1 detects squash merge, resets to origin/main |
| 18:02 | User discovers commits are missing |
| 18:05 | Recovery begins using git reflog |
| 18:15 | Commits successfully recovered via cherry-pick |
| 18:30 | Root cause identified in embody.ps1 |
| 18:40 | Fix implemented and tested |
| 18:45 | Fix committed and pushed (commit 2d779e0) |

---

## Impact

### What Was Lost
- 4 local commits temporarily lost:
  - `ca5107b`: feat(VS_003): Add BlockLibrary resource pattern to merge system
  - `f075dcf`: feat(VS_003): Define merge system with replacement mechanics  
  - `08bc8b3`: feat: Enhance persona embodiment scripts and update documentation
  - `ab93f49`: docs: Clarify merge system design and update implementation status

### Who Was Affected
- Primary: 1 developer lost ~45 minutes of productivity
- Secondary: All team members using embody.ps1 were at risk

### What Saved Us
- Git reflog retained the "lost" commits
- Quick detection of the issue
- Git expertise to perform recovery

---

## Root Cause Analysis

### Primary Cause
The embody.ps1 script's "intelligent sync" feature incorrectly handled the post-squash-merge workflow:

```powershell
# BUGGY LOGIC:
if (squash merge detected) {
    git reset --hard origin/main  # DELETES LOCAL COMMITS WITHOUT CHECKING!
}
```

### Contributing Factors

1. **Incorrect Assumption**: Script assumed all local commits after a squash merge were "old commits that got squashed" rather than "new work created after the merge"

2. **Missing Safeguard**: No check for unpushed commits before performing destructive operations

3. **Workflow Mismatch**: Script designed for "create new branch after PR" workflow, but team uses "continuous dev/main" workflow

4. **Silent Failure**: Script provided no warning before deleting commits

### The Five Whys

1. **Why did we lose data?**
   - Because embody.ps1 ran `git reset --hard` without checking for unpushed commits

2. **Why didn't it check for unpushed commits?**
   - Because the script assumed squash merge meant all local commits were already merged

3. **Why did it make this assumption?**
   - Because the original design didn't account for continuing work on the same branch after PR merge

4. **Why didn't we catch this in testing?**
   - Because we tested squash merge handling but not the "continue working after merge" scenario

5. **Why didn't we test this scenario?**
   - Because we didn't recognize it as a common workflow pattern that needed protection

---

## Resolution

### Immediate Fix
Modified embody.ps1 to check for unpushed commits before reset:

```powershell
# FIXED LOGIC:
if (squash merge detected) {
    $localOnly = git log origin/$branch..HEAD --oneline
    if ($localOnly) {
        # Preserve commits via temp branch and cherry-pick
        Save-LocalCommits
        git reset --hard origin/main
        Restore-LocalCommits
    } else {
        git reset --hard origin/main  # Safe - no local commits
    }
}
```

### Recovery Steps Taken
1. Used `git reflog` to identify lost commits
2. Cherry-picked commits back onto dev/main
3. Verified all work was recovered
4. Pushed recovered commits to remote

---

## Lessons Learned

### What Went Well
- **Quick Detection**: Issue was noticed immediately
- **Successful Recovery**: Git reflog saved the day
- **Fast Resolution**: Fix implemented within 45 minutes
- **No Permanent Loss**: All work was recovered

### What Went Poorly
- **Destructive Operation**: Script performed data deletion without safeguards
- **No Warning**: User had no indication data would be lost
- **Incomplete Testing**: Common workflow wasn't tested
- **Documentation Gap**: Workflow assumptions weren't documented

### Where We Got Lucky
- User knew how to use git reflog
- Commits were recent enough to be in reflog
- Only one person was affected
- Issue was caught before more work was lost

---

## Action Items

### Completed
- [x] Fix embody.ps1 to check for unpushed commits (commit 2d779e0)
- [x] Document the incident in Memory Bank
- [x] Create TD_080 to track the fix
- [x] Write this post-mortem

### Short-term (This Week)
- [ ] Add confirmation prompt for any destructive git operations
- [ ] Create automated test for "continue after squash merge" scenario
- [ ] Add pre-operation backup branch (e.g., `backup/pre-embody-{timestamp}`)
- [ ] Document supported workflows in README

### Medium-term (This Month)
- [ ] Implement comprehensive git state validation before any reset
- [ ] Add `--dry-run` flag to embody.ps1 for testing
- [ ] Create recovery documentation for common git issues
- [ ] Set up automated testing for all git sync scenarios

### Long-term (This Quarter)
- [ ] Design git workflow guidelines for the team
- [ ] Implement git hooks to prevent dangerous operations
- [ ] Consider moving to feature branch workflow if issues persist
- [ ] Create comprehensive DevOps runbook

---

## Prevention Measures

### Technical Safeguards
1. **Always Check Before Destroying**: Any `git reset --hard` must check for unpushed commits
2. **Backup Before Reset**: Create temporary backup branches before destructive operations
3. **Confirmation Prompts**: Require explicit confirmation for data loss operations
4. **Dry Run Options**: Allow users to preview what will happen

### Process Improvements
1. **Workflow Documentation**: Clearly document supported git workflows
2. **Test Scenarios**: Include "continue after merge" in test suite
3. **Code Review**: All git automation changes require careful review
4. **Monitoring**: Log all destructive operations for audit

### Cultural Changes
1. **Assume Nothing**: Never assume git state - always verify
2. **Fail Safe**: When uncertain, preserve data and ask for confirmation
3. **User Trust**: Scripts should protect users from data loss, not cause it

---

## Appendix

### Detection Queries
```bash
# Check for lost commits
git reflog --format='%h %gs: %s' | grep -E 'reset.*origin/main'

# Find commits not on remote
git log origin/dev/main..HEAD --oneline

# Verify recovery
git log --oneline -10
```

### Recovery Procedure
```bash
# 1. Find lost commits
git reflog -20

# 2. Create recovery branch
git branch recovery-backup

# 3. Cherry-pick lost commits
git cherry-pick <commit-hash>

# 4. Push to remote
git push origin dev/main
```

### Testing the Fix
```bash
# Simulate squash merge scenario
git checkout -b test-branch
echo "test" > test.txt && git add . && git commit -m "test"
git checkout main && git merge --squash test-branch
git commit -m "Squashed commit"
git checkout dev/main
echo "new work" > new.txt && git add . && git commit -m "new work"
./scripts/persona/embody.ps1 devops-engineer  # Should preserve "new work"
```

---

## Sign-off

**DevOps Engineer**: This incident revealed a critical gap in our automation tooling. The fix has been implemented and tested. The broader lesson is that automation must respect user workflows and never cause data loss.

**Recommendation**: All personas should update to the latest embody.ps1 immediately. Consider running `git fetch && git branch backup/$(date +%s)` before using embody.ps1 until we implement automatic backups.

---

*"Every incident is a learning opportunity. We don't have failures, we have lessons."*