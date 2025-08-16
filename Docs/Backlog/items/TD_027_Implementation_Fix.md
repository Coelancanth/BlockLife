# TD_027: Backlog Maintainer Implementation Fix

## Immediate Fix Implementation

### The Core Problem
The workflow has pseudo-code verification that agents don't execute. We need to translate documentation into executable commands.

## Fix 1: Replace Pseudo-Code with Executable Bash Commands

### BEFORE (Current - Not Executed):
```markdown
# Step 4: MANDATORY VERIFICATION (MUST NOT SKIP)
if NOT exists(archive/completed/YYYY-QN/{new_name}.md):
    ERROR: "File not found at destination after move"
```

### AFTER (Fixed - Will Execute):
```bash
# Step 4: Execute Mandatory Verification
# For Windows PowerShell:
$verification_result = Test-Path "archive/completed/2025-Q3/{new_name}.md"
if (-not $verification_result) {
    Write-Error "VERIFICATION FAILED: Destination file not found"
    # Restore from backup
    Move-Item "backup/{old_name}.md" "items/{old_name}.md"
    exit 1
}

$source_still_exists = Test-Path "items/{old_name}.md"
if ($source_still_exists) {
    Write-Error "VERIFICATION FAILED: Source file still exists (not moved, only copied)"
    # Remove duplicate
    Remove-Item "archive/completed/2025-Q3/{new_name}.md"
    exit 1
}
```

## Fix 2: Enforce Verification Through Agent Execution Flow

### Current Agent Thinking:
```
1. Read workflow
2. See "archive item" instruction
3. Update Backlog.md
4. Report success
```

### Required Agent Execution:
```python
def archive_item_with_verification(item_id):
    # Step 1: Backup
    bash_tool(f"cp items/{item_id}_*.md backup/")
    
    # Step 2: Move
    move_result = bash_tool(f"mv items/{item_id}_*.md archive/2025-Q3/")
    
    # Step 3: MANDATORY VERIFICATION
    dest_check = bash_tool("test -f archive/2025-Q3/{renamed}.md && echo FOUND")
    if "FOUND" not in dest_check:
        # ROLLBACK
        bash_tool(f"mv backup/{item_id}_*.md items/")
        return "FAILED: File not at destination"
    
    source_check = bash_tool(f"test -f items/{item_id}_*.md && echo STILL_EXISTS")
    if "STILL_EXISTS" in source_check:
        # ROLLBACK
        bash_tool(f"rm archive/2025-Q3/{renamed}.md")
        return "FAILED: Source not deleted (duplicate created)"
    
    # Step 4: Only NOW update Backlog.md
    edit_tool("Backlog.md", old_ref, new_ref)
    
    return "SUCCESS: Verified at destination, source removed"
```

## Fix 3: Add Verification Hooks to Workflow

### Add to backlog-maintainer-workflow.md:

```markdown
## CRITICAL: Verification Execution Protocol

### MANDATORY After Every File Operation:

**YOU MUST EXECUTE THESE BASH COMMANDS - NOT JUST READ THEM**

#### After Move Operation:
```bash
# Windows PowerShell - EXECUTE THIS:
$dest_exists = Test-Path "archive/path/file.md"
$source_gone = -not (Test-Path "items/file.md")
if (-not ($dest_exists -and $source_gone)) {
    throw "Verification failed"
}

# Linux/Mac - EXECUTE THIS:
if [ -f "archive/path/file.md" ] && [ ! -f "items/file.md" ]; then
    echo "VERIFIED"
else
    echo "FAILED" && exit 1
fi
```

**CRITICAL**: These are COMMANDS TO RUN, not documentation to read!
```

## Fix 4: Create Verification Wrapper Script

Create `scripts/verify_file_operation.py`:

```python
#!/usr/bin/env python3
"""
Mandatory verification wrapper for file operations.
Agent MUST call this after EVERY file operation.
"""

import sys
import os
from pathlib import Path
import json
from datetime import datetime

def verify_move(source_path, dest_path):
    """Verify move operation completed successfully."""
    errors = []
    
    # Check destination exists
    if not Path(dest_path).exists():
        errors.append(f"Destination missing: {dest_path}")
    
    # Check source removed
    if Path(source_path).exists():
        errors.append(f"Source still exists: {source_path}")
    
    # Check file integrity
    if Path(dest_path).exists() and Path(dest_path).stat().st_size == 0:
        errors.append(f"Destination is empty: {dest_path}")
    
    # Log operation
    log_entry = {
        "timestamp": datetime.now().isoformat(),
        "operation": "MOVE",
        "source": source_path,
        "destination": dest_path,
        "errors": errors,
        "verified": len(errors) == 0
    }
    
    with open("file_operations.log", "a") as f:
        f.write(json.dumps(log_entry) + "\n")
    
    if errors:
        print(f"VERIFICATION FAILED: {'; '.join(errors)}")
        return 1
    
    print(f"VERIFIED: {source_path} -> {dest_path}")
    return 0

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: verify_file_operation.py <source> <dest>")
        sys.exit(1)
    
    sys.exit(verify_move(sys.argv[1], sys.argv[2]))
```

## Fix 5: Update Agent Workflow with Executable Commands

### In backlog-maintainer-workflow.md, replace abstract instructions with:

```markdown
## Archive Item Action - EXECUTABLE VERSION

### Step-by-Step Commands to Execute:

1. **Create Backup** (EXECUTE THIS):
   ```bash
   cp "items/TD_019_*.md" "backup/"
   ```

2. **Move File** (EXECUTE THIS):
   ```bash
   mv "items/TD_019_*.md" "archive/2025-Q3/2025_01_16-TD_019-name.md"
   ```

3. **Verify Move** (MUST EXECUTE THIS):
   ```bash
   python scripts/verify_file_operation.py \
       "items/TD_019_*.md" \
       "archive/2025-Q3/2025_01_16-TD_019-name.md"
   ```
   
   **If verification fails, EXECUTE ROLLBACK**:
   ```bash
   mv "backup/TD_019_*.md" "items/"
   ```

4. **Update Backlog ONLY if verification passed** (EXECUTE THIS):
   ```bash
   # Use Edit tool to update Backlog.md
   # Change link from items/ to archive/
   ```

5. **Report Result** (EXECUTE THIS):
   ```bash
   if [ $? -eq 0 ]; then
       echo "SUCCESS: TD_019 archived and verified"
   else
       echo "FAILED: TD_019 archive failed verification"
   fi
   ```
```

## Fix 6: Add Self-Test to Workflow

```markdown
## Self-Test Before Operations

**EXECUTE THIS BEFORE ANY FILE OPERATION**:

```bash
# Test we can create, move, and delete files
touch test_file_$$.tmp
mv test_file_$$.tmp test_moved_$$.tmp
if [ -f test_moved_$$.tmp ] && [ ! -f test_file_$$.tmp ]; then
    rm test_moved_$$.tmp
    echo "File operations working"
else
    echo "ERROR: File operations not working correctly"
    exit 1
fi
```
```

## Implementation Checklist

### For Agent Developers:
- [ ] Replace all pseudo-code with executable bash commands
- [ ] Add python verify_file_operation.py calls after moves
- [ ] Make verification results determine success/failure
- [ ] Add rollback commands for failures
- [ ] Log all operations with timestamps

### For Backlog Maintainer:
- [ ] Execute bash commands, don't just read them
- [ ] Call verification script after every move
- [ ] Only update Backlog.md after verification passes
- [ ] Report actual verification results, not assumptions
- [ ] Keep backup until verification confirms success

### For Testing:
- [ ] Test with file that fails to move
- [ ] Test with file that gets duplicated
- [ ] Test rollback when verification fails
- [ ] Verify logging captures all operations
- [ ] Confirm no false success reports

## Key Insight

The problem isn't missing verification logic - it's that agents treat workflow documentation as reading material instead of executable instructions. By replacing pseudo-code with actual bash commands and making verification script calls mandatory, we force execution of the verification that already exists.

**The fix is simple: Make agents EXECUTE verification commands, not just READ about them.**