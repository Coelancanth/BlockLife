# Backlog Maintainer Verification System - Usage Guide

## Overview
This verification system was created to fix TD_021 - critical reliability issues in the backlog-maintainer agent that caused false success reports (BF_003, BF_005, BF_006).

## The Problem It Solves
The backlog-maintainer workflow had verification code (lines 699-735) but it **wasn't being executed**, leading to:
- ❌ Files reported as moved but still in original location
- ❌ Duplicate files in both items/ and archive/ folders
- ❌ Silent failures without error messages
- ❌ False success reports to users

## The Solution
The `backlog_maintainer_verification.py` module provides:
- ✅ **MANDATORY** verification after every file operation
- ✅ Comprehensive logging at every step
- ✅ Loud failures instead of silent ones
- ✅ Automatic rollback on verification failure
- ✅ Health checks before operations

## Quick Start

### Basic Usage
```python
from backlog_maintainer_verification import FileOperationVerifier

# Initialize the verifier
verifier = FileOperationVerifier()

# Move a file with mandatory verification
source = Path("Docs/Backlog/items/TD_001_Example.md")
destination = Path("Docs/Backlog/archive/2025-Q3/2025_08_16-TD_001-example-[completed].md")

success = verifier.move_with_verification(source, destination)

if not success:
    print(verifier.get_error_report())
```

### Archive Operation
```python
# Archive a completed backlog item
item_id = "TD_014"
source_file = Path("Docs/Backlog/items/TD_014_Agent_Architecture.md")
archive_dir = Path("Docs/Backlog/archive/2025-Q3")

success, archive_path = verifier.archive_with_verification(
    item_id, 
    source_file, 
    archive_dir
)

if success:
    print(f"✅ {item_id} archived to {archive_path}")
else:
    print(f"❌ Archive failed: {verifier.get_error_report()}")
```

## Key Features

### 1. Mandatory Verification
Every operation is verified with a 1.5-second delay for filesystem sync:
- **MOVE**: Verifies source deleted, destination exists, checksums match
- **CREATE**: Verifies file exists and is not empty
- **DELETE**: Verifies file no longer exists
- **COPY**: Verifies both files exist with matching sizes

### 2. Comprehensive Logging
All operations are logged with:
- Timestamp
- Operation type
- Source and destination paths
- Success/failure status
- Error details if failed
- Checksums for integrity

Logs are saved to: `scripts/verification_log.json`

### 3. Automatic Rollback
If verification fails:
1. Attempts to reverse the operation
2. Restores from backup if available
3. Logs rollback attempt
4. Returns clear error message

### 4. Health Checks
Before operations, verifies:
- Base path exists
- Backup directory is writable
- Necessary permissions available
- Filesystem is responsive

## Integration with Backlog Maintainer

### Step 1: Import the Verifier
```python
from scripts.backlog_maintainer_verification import FileOperationVerifier
```

### Step 2: Initialize at Start
```python
def initialize_backlog_maintainer():
    verifier = FileOperationVerifier(
        base_path=".", 
        enable_rollback=True
    )
    return verifier
```

### Step 3: Replace File Operations
**BEFORE (problematic):**
```python
# This doesn't verify!
shutil.move(source, destination)
print("File moved successfully")  # May be false!
```

**AFTER (verified):**
```python
# This ACTUALLY verifies!
success = verifier.move_with_verification(source, destination)
if not success:
    raise Exception(f"Move failed: {verifier.get_error_report()}")
```

## Testing

### Run Comprehensive Tests
```bash
python scripts/test_backlog_verification.py
```

This tests:
- BF_005 scenario (false archive success)
- BF_006 scenario (duplicate prevention)
- Rollback capability
- Comprehensive logging

### Manual Verification
```bash
python scripts/backlog_maintainer_verification.py test
```

## Monitoring & Debugging

### View Verification Report
```python
verifier.print_verification_report()
```

Output:
```
📊 VERIFICATION REPORT
====================
Total Operations: 10
Successful: 9
Failed: 1
Success Rate: 90.0%
Verification Failures: 1
```

### Get Error Details
```python
error_report = verifier.get_error_report()
print(error_report)
```

### Check Logs
```bash
cat scripts/verification_log.json | python -m json.tool
```

## Best Practices

### DO ✅
- Always use `move_with_verification()` instead of raw `shutil.move()`
- Check return value and handle failures
- Keep rollback enabled for critical operations
- Review logs after batch operations
- Run health checks periodically

### DON'T ❌
- Skip verification "for performance" (1.5s delay is critical)
- Ignore failed operations
- Delete backups immediately
- Assume operations succeeded without checking
- Use raw file operations without verification

## Error Recovery

### If Operations Fail
1. Check the error report: `verifier.get_error_report()`
2. Review verification log: `scripts/verification_log.json`
3. Check for backups: `.backlog_backups/` directory
4. Run health check: `verifier._perform_health_check()`

### Common Issues
- **"Destination exists"**: File already at target location
- **"Source still exists after move"**: Move failed, file not deleted
- **"Checksum mismatch"**: File corrupted during transfer
- **"Filesystem slow"**: System under heavy load

## Performance Impact

The verification system adds:
- 1.5 seconds per operation (filesystem sync)
- ~50ms for checksum calculation
- ~10ms for logging

This is a small price for **100% reliability** vs silent failures.

## Maintenance

### Update Verification Logic
Edit `scripts/backlog_maintainer_verification.py`:
- `_verify_file_operation()` method for verification rules
- `_perform_health_check()` for pre-flight checks
- `_perform_rollback()` for recovery logic

### Adjust Timing
Change filesystem sync delay (default 1.5s):
```python
# In _verify_file_operation()
time.sleep(1.5)  # Adjust if needed
```

## Summary

This verification system ensures that:
1. **No more false success reports** - Operations are ACTUALLY verified
2. **No more silent failures** - Errors are logged and reported
3. **No more duplicates** - Move operations are atomic
4. **Recovery is possible** - Automatic rollback on failure

Remember: **ALWAYS VERIFY, NEVER ASSUME**

---

Created to fix TD_021 and prevent BF_003, BF_005, BF_006 incidents.
For questions, see the test suite or error logs.