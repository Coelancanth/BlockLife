# BF_004: Backlog-Maintainer File Overwrite Bug

## Overview
The backlog-maintainer agent overwrites existing work item files with simplified versions, destroying detailed content.

## Bug Details

### Symptoms
- Existing detailed work item files get replaced with simplified versions
- Original content lost without warning
- Agent doesn't check if file already exists
- No merge or append logic, just overwrites

### Evidence
**TD_020 Original (Created by Claude Code):**
- Had 4 detailed naming options with pros/cons
- Included Option 1-4 with examples
- Comprehensive analysis of each approach
- ~70 lines of detailed content

**TD_020 After backlog-maintainer:**
- Simplified to basic template
- Lost all 4 naming options
- Reduced to ~38 lines
- Critical design decisions erased

### Root Cause
The backlog-maintainer agent appears to:
1. Generate its own template for work items
2. Write directly to file path without checking existing content
3. Not have a "file exists" check before writing
4. Lack append/merge capabilities

## Reproduction Steps
1. Create detailed work item file (e.g., TD_020)
2. Trigger backlog-maintainer to add same item to backlog
3. Observe original file gets overwritten with template

## Expected Behavior
- Agent should CHECK if file exists first
- If exists: Update backlog tracker only, don't touch file
- If not exists: Create new file
- Never overwrite existing content

## Actual Behavior
- Agent blindly writes template to file path
- Destroys existing content
- No warning or error

## Impact
- **CRITICAL**: Data loss of design decisions
- Loss of detailed specifications
- Wasted effort on documentation
- Trust issue with agent operations

## Fix Required

### Immediate Fix
Update backlog-maintainer workflow to:
```python
if file_exists(item_path):
    # DO NOT overwrite
    log("File exists, updating tracker only")
    update_backlog_tracker()
else:
    # Safe to create
    create_new_file(item_path)
    update_backlog_tracker()
```

### Long-term Fix
1. Add file existence checks to ALL write operations
2. Implement merge logic for updates
3. Create backup before any overwrites
4. Add verification step after file operations

## Acceptance Criteria
- [ ] Backlog-maintainer checks file existence before writing
- [ ] Existing files never get overwritten
- [ ] Agent logs when skipping file creation
- [ ] Test with 5 scenarios without data loss

## Priority
P1 - CRITICAL: Data loss bug

## Related Items
- BF_003: Archive path failure (another file operation bug)
- TD_019: Double Verification Protocol (would catch this)
- TD_020: The victim of this bug

## Notes
This is the SECOND file operation bug from backlog-maintainer (after BF_003). The agent needs better file handling logic across all operations.

**Created**: 2025-08-16
**Status**: Active
**Progress**: 0%