# BF_006: Backlog Archive Duplication Issue

**Status**: Active  
**Severity**: High  
**Priority**: P1  
**Size**: S (2-3 hours)  
**Sprint**: Current  
**Found In**: 2025-08-16 Debug Investigation  
**Reported By**: Debugger Expert Analysis  

## Problem Description
Investigation of BF_005 revealed that while most archive operations succeed, some items end up DUPLICATED in both `items/` and `archive/` folders instead of being MOVED. This creates confusion and inconsistent state tracking.

## Evidence from Verification Script
```
DUPLICATES FOUND:
- TD_019: Exists in both items/ and archive/
- BF_003: Exists in both items/ and archive/

TRACKING ERRORS:
- TD_002: In archive but marked as active
- TD_003: In archive but marked as active  
- TD_005: In archive but marked as active
```

## Root Cause Analysis
1. **Copy vs Move**: The backlog-maintainer sometimes performs a COPY operation instead of MOVE
2. **No Delete Verification**: After "moving" a file, there's no check that source was deleted
3. **Timing Issues**: File operations may not complete before verification
4. **Platform Differences**: Windows vs Unix file operations behave differently

## Fix Implementation

### Immediate Actions
1. Remove duplicate files from items/ folder (TD_019, BF_003)
2. Update Backlog.md to correctly reflect archived status
3. Implement mandatory delete verification

### Code Changes Required

#### 1. Enhanced Move Operation
```python
def move_with_verification(source, destination):
    # Step 1: Copy to destination
    shutil.copy2(source, destination)
    
    # Step 2: Verify copy succeeded
    if not os.path.exists(destination):
        raise Exception("Copy failed")
    
    # Step 3: Verify sizes match
    if os.path.getsize(source) != os.path.getsize(destination):
        raise Exception("Copy corrupted")
    
    # Step 4: Delete source
    os.remove(source)
    
    # Step 5: Verify source deleted
    if os.path.exists(source):
        raise Exception("Delete failed")
    
    return True
```

#### 2. Backlog Maintainer Workflow Update
- Add explicit DELETE verification after move
- Use atomic operations where possible
- Add retry logic for locked files
- Log all file operations with timestamps

## Test Requirements

### Regression Test
```python
def test_archive_move_not_copy():
    """Ensure archive operations MOVE not COPY files"""
    # Given: File in items/
    # When: Archive operation
    # Then: File ONLY in archive/, NOT in items/
    # And: No duplicates exist
```

### Verification Script
Created `scripts/verify_backlog_archive.py` that:
- Detects duplicate files
- Verifies archive operations
- Checks Backlog.md consistency
- Prevents false positive bug reports

## Acceptance Criteria
- [ ] No duplicate files in items/ and archive/
- [ ] Backlog.md accurately reflects file locations
- [ ] Verification script passes with 0 failures
- [ ] Move operations verified with delete check
- [ ] No similar issues in next 20 archive operations

## Prevention Strategy
1. **Mandatory Verification**: Every file operation must be verified
2. **Atomic Operations**: Use platform-specific atomic move when possible
3. **Verification Script**: Run after every archive session
4. **Clear Logging**: Log source→destination with timestamps
5. **Rollback Capability**: Keep backup until operation verified

## Lessons Learned
1. **File operations are not atomic** - Must verify each step
2. **Copy ≠ Move** - Explicit delete verification required
3. **Trust but Verify** - Agent reports can be wrong
4. **Duplicates cause confusion** - Single source of truth critical
5. **Verification scripts prevent false bugs** - Automate checking

## References
- Original Issue: BF_005 (false positive about TD_014, TD_015, TD_020)
- Related: BF_003, BF_004 (file operation issues)
- Verification Script: `scripts/verify_backlog_archive.py`
- Enhanced Workflow: `Docs/Workflows/Agent-Workflows/backlog-maintainer-workflow.md`

## Definition of Done
- [ ] All duplicate files removed
- [ ] Backlog.md updated with correct status
- [ ] Verification script shows 0 failures
- [ ] Workflow updated with delete verification
- [ ] No recurrence in subsequent operations