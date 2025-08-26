# Git Sync Logic Consolidation Plan

**Created**: 2025-08-27  
**Author**: DevOps Engineer  
**Status**: PLANNED  

## Problem Statement

We have duplicate git sync logic in multiple scripts:
- `scripts/persona/embody.ps1` (lines 70-191)
- `scripts/git/smart-sync.ps1` (entire file)
- `scripts/git/pr.ps1` (calls smart-sync at line 103)

This duplication leads to:
- Maintenance burden (bugs must be fixed in multiple places)
- Inconsistent behavior
- Risk of data loss if one script is updated but not others

## Proposed Solution

Create a shared PowerShell module: `scripts/git/sync-core.psm1`

### Module Structure
```powershell
# sync-core.psm1
Export-ModuleMember -Function @(
    'Test-SquashMerge',
    'Get-SyncStrategy',
    'Preserve-LocalCommits',
    'Sync-Branch'
)
```

### Key Functions

1. **Test-SquashMerge**
   - Detect if a PR was squash-merged
   - Check GitHub API if available
   - Fallback to commit pattern analysis

2. **Get-SyncStrategy**
   - Input: Current branch, remote state
   - Output: Strategy enum (SquashReset, FastForward, Rebase, Merge)
   - Consistent logic across all scripts

3. **Preserve-LocalCommits**
   - CRITICAL: Check for new commits after squash merge
   - Create temp branch for safety
   - Cherry-pick only NEW commits
   - Handle conflicts gracefully
   - Return success/failure with preserved commits

4. **Sync-Branch**
   - High-level orchestration
   - Stash uncommitted changes
   - Apply appropriate strategy
   - Restore stashed changes
   - Handle all error cases

## Migration Plan

### Phase 1: Create Module (Low Risk)
1. Extract common logic to `sync-core.psm1`
2. Add comprehensive tests
3. Document all functions

### Phase 2: Update smart-sync.ps1 (Medium Risk)
1. Import sync-core module
2. Replace inline logic with module calls
3. Test all scenarios:
   - Normal rebase
   - Squash merge detection
   - New commits preservation
   - Conflict handling

### Phase 3: Update embody.ps1 (Medium Risk)
1. Import sync-core module
2. Replace Resolve-GitState with module calls
3. Maintain backward compatibility
4. Test persona switching scenarios

### Phase 4: Update pr.ps1 (Low Risk)
1. Already calls smart-sync
2. Will automatically benefit from improvements
3. Consider direct module usage for efficiency

## Testing Requirements

Create `scripts/test/test-sync-core.ps1`:

1. **Unit Tests**
   - Test-SquashMerge with various scenarios
   - Get-SyncStrategy for all branch states
   - Preserve-LocalCommits with conflicts

2. **Integration Tests**
   - Full sync workflow
   - Data preservation after squash merge
   - Error recovery paths

3. **Regression Tests**
   - Ensure no behavior changes
   - Test with real git history
   - Verify all edge cases

## Risk Assessment

- **Risk Level**: Medium
- **Impact if Failed**: Could lose commits (critical)
- **Mitigation**: 
  - Extensive testing before deployment
  - Keep old code as fallback
  - Deploy in phases with monitoring

## Success Criteria

1. Zero duplicate sync logic
2. All scripts use same module
3. 100% test coverage for critical paths
4. No data loss incidents
5. Easier maintenance going forward

## Timeline

- **Week 1**: Create module and tests
- **Week 2**: Migrate smart-sync.ps1
- **Week 3**: Migrate embody.ps1
- **Week 4**: Monitor and refine

## Notes

- This consolidation was prompted by finding the same critical bug in multiple scripts
- The fix for commit preservation MUST be applied consistently
- Consider using this pattern for other shared script logic

---

**Next Step**: Review and approve this plan before implementation