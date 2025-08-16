# TD_026: Backlog Maintainer Critical Reliability Analysis

## Executive Summary

The backlog-maintainer agent has comprehensive verification code documented in its workflow (lines 699-735) but **this code is NOT being executed**. This is causing critical reliability failures with false success reports.

## Critical Findings

### 1. The Verification Code EXISTS but ISN'T CALLED

**Location**: `backlog-maintainer-workflow.md` lines 699-735
```python
def verify_file_operation(operation_type, source_path, dest_path=None):
    """MUST be called after EVERY file operation"""
    # Excellent verification logic here
```

**PROBLEM**: This is pseudo-code documentation, NOT executable code that agents follow!

### 2. The Archive Action Workflow is Incomplete

**Location**: Lines 438-543 describe the archive process
- Step 4 says "MANDATORY VERIFICATION (MUST NOT SKIP)"
- But this is just documentation text, not enforced code
- The agent reads this as guidance but doesn't have executable verification

### 3. Python Scripts Have Verification but Agent Doesn't Use Them

**Finding**: Two Python scripts exist with proper verification:
1. `auto_archive_completed.py` - Has verify_file_operation() at line 209
2. `verify_backlog_archive.py` - Has comprehensive verification

**PROBLEM**: The backlog-maintainer agent doesn't actually call these scripts!

## Root Cause Analysis

### Why Verification Isn't Executing

1. **Documentation vs Implementation Gap**
   - The workflow file contains pseudo-code and instructions
   - Agents interpret these as guidelines, not executable code
   - No enforcement mechanism ensures verification runs

2. **Agent Execution Model**
   - Agents use Edit/Write/Read tools directly
   - They follow workflow "steps" but skip verification
   - Success is reported based on Edit tool success, not actual file state

3. **Silent Failure Pattern**
   ```
   Agent thinks: "I used Edit tool → It returned success → Archive complete!"
   Reality: Edit updated Backlog.md but file wasn't actually moved
   ```

4. **Platform-Specific Issues**
   - Windows file operations aren't atomic
   - PowerShell Move-Item can fail silently
   - No explicit error checking after moves

## Failure Points Identified

### Point 1: Archive Item Action (Line 369)
```
4. Move and Rename File with Mandatory Verification
   # Windows PowerShell:
   Move-Item -Path "items/{old_name}.md" -Destination "archive/..."
```
**ISSUE**: This is a documentation example, not enforced code

### Point 2: Update Backlog Reference (Line 480)
```
5. Update Backlog.md
   - Delete row from tracker
   - Update reference to archive path
```
**ISSUE**: Agent updates Backlog.md BEFORE verifying file actually moved

### Point 3: Success Reporting
```
Outputs:
- Minimal: "✓ {item_id} archived as {new_filename}"
```
**ISSUE**: Agent reports success based on Backlog.md update, not file verification

## Why Agent Reports Success on Failures

1. **Edit Tool Success ≠ Operation Success**
   - Edit tool updates Backlog.md successfully
   - Agent interprets this as "archive complete"
   - No check if actual file operation succeeded

2. **Missing Execution Flow**
   ```
   Current Flow:
   1. Read workflow instructions
   2. Update Backlog.md with Edit tool ✓
   3. Report success ✓
   
   What's Missing:
   - Actually move the file
   - Verify file at destination
   - Verify source deleted
   - Rollback on failure
   ```

3. **Cognitive Interpretation Issue**
   - Agent reads "MANDATORY VERIFICATION" as important note
   - But doesn't translate to actual verification execution
   - Treats workflow as documentation, not executable spec

## Specific Code Fixes Required

### Fix 1: Make Verification Executable

**Current** (Documentation):
```markdown
# Step 4: MANDATORY VERIFICATION (MUST NOT SKIP)
if NOT exists(archive/completed/YYYY-QN/{new_name}.md):
    ERROR: "File not found at destination after move"
```

**Required** (Executable):
```python
# In agent's actual execution path
result = bash_tool("test -f archive/completed/2025-Q3/file.md && echo EXISTS")
if "EXISTS" not in result:
    raise FileOperationError("Verification failed: file not at destination")
```

### Fix 2: Force Verification Through Tool Usage

Instead of documentation, enforce through tool constraints:
```python
# Create new VerifyArchive tool that MUST be called
@mandatory_after("archive_item")
def verify_archive(item_id, source_path, dest_path):
    # This tool MUST execute or operation fails
    if file_exists(source_path):
        return "ERROR: Source still exists"
    if not file_exists(dest_path):
        return "ERROR: Destination missing"
    return "VERIFIED"
```

### Fix 3: Add Execution Checkpoints

```python
# Workflow must include executable checkpoints
ARCHIVE_CHECKLIST = [
    ("backup_created", "Backup file before operation"),
    ("source_exists", "Verify source file exists"),
    ("destination_created", "Verify destination created"),
    ("source_removed", "Verify source deleted"),
    ("backlog_updated", "Update tracking only after verification"),
    ("verification_logged", "Log verification results")
]

# Agent must complete ALL checkpoints or rollback
```

### Fix 4: Implement Logging Protocol

```python
# Every file operation MUST log
def log_file_operation(operation, source, dest, result):
    timestamp = datetime.now().isoformat()
    log_entry = {
        "timestamp": timestamp,
        "operation": operation,
        "source": source,
        "destination": dest,
        "result": result,
        "verified": False  # Becomes True only after verification
    }
    append_to_log("agent_file_operations.log", log_entry)
```

## Why Silent Failures Occur

1. **No Exception Propagation**
   - File operations fail silently on Windows
   - No error checking after Move-Item
   - Agent continues despite failures

2. **Optimistic Success Assumption**
   - Agent assumes operations succeed
   - No defensive programming
   - No verification before reporting

3. **Missing Feedback Loop**
   - No mechanism to detect failures
   - No retry logic
   - No rollback procedures

## Comprehensive Fix Proposal

### Solution Architecture

```
┌─────────────────────────────────────┐
│   Backlog Maintainer Agent          │
├─────────────────────────────────────┤
│  1. Parse Request                   │
│  2. Create Backup                   │
│  3. Execute Operation               │
│  4. MANDATORY: Call Verifier ────┐  │
│  5. Update Tracking (if verified)│  │
│  6. Report Result                │  │
└───────────────────────────────────┘  │
                                       │
┌──────────────────────────────────────▼─┐
│        Verification Service            │
├─────────────────────────────────────────┤
│  • Check source deleted                │
│  • Check destination exists            │
│  • Verify file integrity               │
│  • Log all checks                      │
│  • Return PASS/FAIL + details          │
└─────────────────────────────────────────┘
```

### Implementation Steps

1. **Create Verification Service**
   - Standalone verification that agent MUST call
   - Returns structured result with proof
   - Logs all operations independently

2. **Modify Agent Workflow**
   - Add mandatory verification calls
   - Prevent success reporting without verification
   - Add rollback on verification failure

3. **Add Operation Logging**
   - Log before operation (intent)
   - Log after operation (result)
   - Log verification outcome
   - Create audit trail

4. **Implement Rollback Protocol**
   - Keep backup until verified
   - Restore on failure
   - Report failure with details

### Regression Test Suite

```python
def test_archive_verification_mandatory():
    """Verification MUST execute"""
    # Mock file operation to fail
    # Verify agent detects and reports failure
    
def test_false_success_prevention():
    """No success without verification"""
    # Simulate Edit success but move failure
    # Verify agent reports failure not success

def test_duplicate_prevention():
    """No file duplication"""
    # Attempt archive operation
    # Verify source deleted after successful move

def test_rollback_on_failure():
    """Rollback works correctly"""
    # Simulate verification failure
    # Verify file restored to original location
```

## Lessons Learned

1. **Documentation ≠ Implementation**
   - Workflow documentation isn't executable
   - Agents need enforceable mechanisms
   - Verification must be mandatory, not optional

2. **Trust but Verify**
   - Never trust tool success alone
   - Always verify actual outcomes
   - File operations especially need verification

3. **Silent Failures are Deadly**
   - Add comprehensive logging
   - Fail loudly, not silently
   - Make verification impossible to skip

4. **Platform Matters**
   - Windows file operations need special care
   - Test on target platform
   - Don't assume Unix behavior on Windows

## Priority Actions

### IMMEDIATE (Do Now)
1. Add bash verification after EVERY file operation
2. Log all operations with timestamps
3. Never report success without verification

### SHORT TERM (This Week)
1. Create verification service/tool
2. Update workflow with executable checks
3. Add comprehensive logging

### LONG TERM (This Month)
1. Refactor to use Python scripts
2. Add integration tests
3. Create monitoring dashboard

## Conclusion

The backlog-maintainer has excellent verification logic documented but **doesn't execute it**. The fix requires making verification MANDATORY and EXECUTABLE, not just documented. This is a critical architectural issue where documentation is confused with implementation.

**The verification code at lines 699-735 is the solution - it just needs to be EXECUTED, not just documented!**