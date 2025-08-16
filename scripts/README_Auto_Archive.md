# Automatic Archive System for Backlog Items

## Overview

The automatic archive system eliminates manual intervention when backlog items reach 100% completion. This automation ensures completed work items are immediately moved to the archive with proper naming conventions and verification.

## Problem Solved

**Before**: Manual archival process required:
- Developer to manually move files from `items/` to `archive/`
- Manual naming convention application
- Manual Backlog.md updates
- Risk of inconsistent state between file location and backlog status

**After**: Automatic process handles:
- Detection of 100% completion during progress updates
- Automatic file archival with proper naming
- Backlog.md reference updates
- Full verification of archival success

## Architecture

### Core Components

1. **`auto_archive_completed.py`** - Main automation script
2. **`backlog_integration.py`** - Integration helper for workflow
3. **Enhanced backlog maintainer workflow** - Triggers automatic archival
4. **Verification system** - Ensures archival success

### Integration Flow

```
Progress Update → 100% Detected → Trigger Auto-Archive → Verify → Update Backlog
     ↓              ↓                    ↓               ↓         ↓
   Edit file    if progress==100   Run script      Check files  Update links
```

## Usage

### Automatic Usage (Preferred)
The system automatically triggers when the backlog maintainer detects 100% completion:

```python
# In backlog maintainer workflow
if new_progress == 100:
    trigger_automatic_archival(item_id)
```

### Manual Usage
For manual archival or testing:

```bash
# Dry run to preview what would be archived
python scripts/auto_archive_completed.py --dry-run

# Archive all completed items
python scripts/auto_archive_completed.py

# Verbose output with detailed logging
python scripts/auto_archive_completed.py --verbose

# Force archival even if minor errors detected
python scripts/auto_archive_completed.py --force
```

### Integration Testing
Test the integration pattern:

```bash
# Demonstrate the integration workflow
python scripts/backlog_integration.py
```

## Features

### Intelligent Archive Naming
Follows the established naming convention:
```
YYYY_MM_DD-[TYPE_ID]-description-[tag1][tag2][...][status].md
```

**Example**:
```
Original: TD_019_Double_Verification_Protocol.md
Archive:  2025_01_16-TD_019-verification-protocol-[test][critical][automation][completed].md
```

### Automatic Tag Generation
- **Type tags**: `[bug]` `[feature]` `[refactor]` `[test]`
- **Impact tags**: `[critical]` `[dataloss]` `[security]` `[breaking]`
- **Area tags**: `[fileops]` `[ui]` `[core]` `[workflow]` `[agent]`
- **Detail tags**: `[automation]` `[pattern]` `[framework]`
- **Status tags**: `[resolved]` `[completed]`

### Quarterly Organization
Archives are organized by quarter:
```
Docs/Backlog/archive/completed/
├── 2025-Q1/
├── 2025-Q2/
├── 2025-Q3/
└── 2025-Q4/
```

### File Operation Safety
- **Verification Protocol**: Mandatory checks after every file operation
- **Rollback Support**: Automatic restoration if verification fails
- **Non-destructive**: Never overwrites existing files
- **Atomic Operations**: All-or-nothing approach

## Configuration

### Required Dependencies
All dependencies are in `scripts/requirements.txt`:
- `click>=8.0.0` - CLI framework
- `rich>=13.0.0` - Terminal formatting
- `PyYAML>=6.0` - Configuration support

Install with:
```bash
pip install -r scripts/requirements.txt
```

### File Structure Requirements
```
Docs/Backlog/
├── Backlog.md              # Main backlog file
├── items/                  # Active work items
├── archive/completed/      # Archived items by quarter
└── templates/              # Item templates
```

## Verification System

### Automatic Verification
The system automatically verifies:
- Source file removed from `items/` folder
- Archive file created in correct location
- Archive file is not empty
- Backlog.md updated with archive reference

### Manual Verification
Use the existing verification script:
```bash
python scripts/verify_backlog_archive.py
```

### Verification Checklist
- [ ] Source file no longer exists in `items/`
- [ ] Archive file exists in `archive/completed/YYYY-QN/`
- [ ] Archive file size > 0 bytes
- [ ] Backlog.md link points to archive location
- [ ] Item status shows "📦 ARCHIVED"

## Error Handling

### Common Issues and Solutions

1. **File Already Exists in Archive**
   - **Error**: Destination file already exists
   - **Solution**: Check for duplicate archival, manual resolution needed

2. **Verification Failure**
   - **Error**: File operation verification failed
   - **Solution**: Automatic rollback, item stays at 100% for manual review

3. **Backlog Update Failure**
   - **Error**: Cannot update Backlog.md reference
   - **Solution**: File restored to `items/`, manual intervention required

### Recovery Procedures
1. Check operation log: `scripts/auto_archive_log.json`
2. Run verification: `python scripts/verify_backlog_archive.py`
3. Manual intervention for failed items
4. Re-run automation: `python scripts/auto_archive_completed.py`

## Time Savings

### ROI Analysis
- **Manual Process**: 5-10 minutes per item
  - Find completed items
  - Create archive filename
  - Move file
  - Update Backlog.md
  - Verify operation

- **Automated Process**: ~10 seconds per item
  - Automatic detection
  - Instant archival
  - Verified operation

- **Break-even**: After 1 use (immediate ROI)
- **Monthly Savings**: ~30-60 minutes (assuming 6-12 completed items)

## Integration Points

### Backlog Maintainer Workflow
Enhanced progress update action automatically triggers archival:
```python
def update_progress(item_id, increment):
    new_progress = current_progress + increment
    if new_progress == 100:
        trigger_auto_archive()  # NEW: Automatic trigger
```

### GitHub Actions (Future)
Can be integrated into CI/CD pipeline:
```yaml
- name: Auto-archive completed items
  run: python scripts/auto_archive_completed.py
```

## Monitoring

### Operation Logs
- **Location**: `scripts/auto_archive_log.json`
- **Retention**: Last 50 operations
- **Content**: Timestamps, file paths, success/failure status

### Metrics Tracked
- Items processed per run
- Success/failure rates
- Average processing time
- Error patterns

## Troubleshooting

### Debug Mode
Enable verbose logging:
```bash
python scripts/auto_archive_completed.py --verbose
```

### Dry Run Testing
Test without making changes:
```bash
python scripts/auto_archive_completed.py --dry-run
```

### Force Mode
Skip minor error checks:
```bash
python scripts/auto_archive_completed.py --force
```

### Manual Override
If automation fails, use manual archive process:
1. Move file manually to `archive/completed/YYYY-QN/`
2. Rename using naming convention
3. Update Backlog.md reference
4. Verify with `verify_backlog_archive.py`

## Future Enhancements

### Planned Features
- **Batch Processing**: Archive multiple items efficiently
- **Smart Scheduling**: Run automatically on schedule
- **Email Notifications**: Alert on archival failures
- **Metrics Dashboard**: Visual progress tracking
- **Rollback Command**: Unarchive items if needed

### Integration Opportunities
- **Git Hooks**: Pre-commit archival verification
- **VS Code Extension**: Archive directly from editor
- **Slack Notifications**: Team updates on completions
- **Analytics**: Completion velocity tracking

## Support

For issues or questions:
1. Check operation logs in `scripts/auto_archive_log.json`
2. Run verification: `python scripts/verify_backlog_archive.py`
3. Review error messages in verbose mode
4. Consult the backlog maintainer workflow documentation

---

**Created by**: DevOps Engineer agent
**Purpose**: Eliminate manual archival work and ensure consistency
**Time Saved**: ~5-10 minutes per completed item