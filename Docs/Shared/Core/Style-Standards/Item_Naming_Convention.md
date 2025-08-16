# Archive Naming Convention

## Overview
A standardized naming system for archived work items that maximizes searchability, maintains chronological ordering, and integrates seamlessly with command-line text tools like grep.

## Format Specification

### Core Pattern
```
YYYY_MM_DD-[TYPE_ID]-description-[tag1][tag2][tag3][...].md
```

### Component Breakdown

#### 1. Date Stamp: `YYYY_MM_DD`
- **Format**: Year_Month_Day with underscores
- **Example**: `2025_01_16`
- **Purpose**: Natural chronological sorting

#### 2. Item ID: `[TYPE_ID]`
- **Format**: Original work item identifier
- **Examples**: `TD_019`, `BF_003`, `VS_000`, `HF_004`
- **Purpose**: Maintains traceability to original item

#### 3. Description: `description`
- **Format**: Kebab-case summary (2-4 words)
- **Examples**: `verification-protocol`, `archive-path`, `move-block`
- **Purpose**: Human-readable summary of what was done

#### 4. Tags: `[tag1][tag2][tag3]`
- **Format**: Multiple bracketed tags in priority order
- **Examples**: `[bug][critical][fileops][resolved]`
- **Purpose**: Multi-dimensional searchability

## Tag Priority System

Tags MUST be ordered by priority to ensure consistent and predictable grep patterns.

### Priority Levels

| Priority | Category | Tags | Position | Required |
|----------|----------|------|----------|----------|
| 1 | Type | `[bug]` `[feature]` `[refactor]` `[docs]` `[test]` | FIRST | Yes |
| 2 | Impact | `[critical]` `[dataloss]` `[security]` `[breaking]` | SECOND | If applicable |
| 3 | Area | `[fileops]` `[ui]` `[core]` `[workflow]` `[agent]` | THIRD | Recommended |
| 4 | Details | `[automation]` `[pattern]` `[framework]` `[migration]` | MIDDLE | Optional |
| 99 | Status | `[resolved]` `[completed]` `[partial]` `[deprecated]` | LAST | Yes |

### Tag Definitions

#### Type Tags (Mutually Exclusive)
- `[bug]` - Bug fixes and error corrections
- `[feature]` - New functionality or enhancements
- `[refactor]` - Code reorganization without behavior change
- `[docs]` - Documentation updates
- `[test]` - Test creation or updates

#### Impact Tags
- `[critical]` - Critical priority or system-breaking issue
- `[dataloss]` - Potential or actual data loss involved
- `[security]` - Security implications
- `[breaking]` - Breaking changes to API or behavior

#### Area Tags
- `[fileops]` - File operations (create, move, delete)
- `[ui]` - User interface components
- `[core]` - Core system functionality
- `[workflow]` - Process or workflow related
- `[agent]` - AI agent related

#### Detail Tags
- `[automation]` - Automation or scripting
- `[pattern]` - Design patterns or architectural patterns
- `[framework]` - Framework or infrastructure
- `[migration]` - Data or system migration

#### Status Tags (Mutually Exclusive)
- `[resolved]` - Issue resolved (for bugs)
- `[completed]` - Work completed (for features/tasks)
- `[partial]` - Partially completed
- `[deprecated]` - Deprecated or abandoned

## Real-World Examples

```
2025_01_16-TD_019-verification-protocol-[test][critical][automation][completed].md
2025_01_16-BF_003-archive-path-[bug][critical][fileops][resolved].md
2025_01_16-BF_004-overwrite-protection-[bug][dataloss][fileops][resolved].md
2025_01_15-TD_012-orchestration-pattern-[refactor][workflow][automation][completed].md
2025_01_14-VS_000-move-block-[feature][core][ui][completed].md
2025_01_14-HF_002-thread-safety-[bug][critical][core][resolved].md
```

## Search Patterns

### Basic Searches

```bash
# Find all items from specific date
ls 2025_01_16-*.md

# Find all bugs (type tag always first)
ls *-\[bug\]*.md

# Find all resolved items (status tag always last)
ls *\[resolved\].md

# Find all critical items
ls *\[critical\]*.md
```

### Advanced Searches

```bash
# Find all critical bugs
ls *-\[bug\]\[critical\]*.md

# Find all resolved fileops bugs
ls *-\[bug\]*\[fileops\]\[resolved\].md

# Find all completed features from January 2025
ls 2025_01_*-\[feature\]*\[completed\].md

# Find all data loss issues
grep -l "\[dataloss\]" *.md

# Count items by type
ls *-\[bug\]*.md | wc -l
ls *-\[feature\]*.md | wc -l
```

### Complex Grep Patterns

```bash
# Find items with both automation and critical tags
ls *\[critical\]*\[automation\]*.md

# Find all fileops issues that caused data loss
ls *\[dataloss\]*\[fileops\]*.md

# Find resolved critical bugs in core
grep -l "\[bug\].*\[critical\].*\[core\].*\[resolved\]" *.md
```

## Migration Guide

### From Old Format
```
TD_012_Dynamic_PO_Pattern_Implementation.md
```

### To New Format
```
2025_01_15-TD_012-orchestration-pattern-[refactor][workflow][automation][completed].md
```

### Migration Script Template

```python
#!/usr/bin/env python3
"""Archive naming migration script"""

from pathlib import Path
from datetime import datetime

def migrate_filename(old_name, completion_date, tags):
    """Convert old naming to new prioritized format"""
    
    # Tag priority map
    tag_priority = {
        # Type (1)
        'bug': 1, 'feature': 1, 'refactor': 1, 'docs': 1, 'test': 1,
        # Impact (2)
        'critical': 2, 'dataloss': 2, 'security': 2, 'breaking': 2,
        # Area (3)
        'fileops': 3, 'ui': 3, 'core': 3, 'workflow': 3, 'agent': 3,
        # Details (4-98)
        'automation': 4, 'pattern': 4, 'framework': 4, 'migration': 4,
        # Status (99)
        'resolved': 99, 'completed': 99, 'partial': 99, 'deprecated': 99
    }
    
    # Extract components from old name
    parts = old_name.replace('.md', '').split('_')
    item_id = f"{parts[0]}_{parts[1]}"
    description = '-'.join(parts[2:]).lower().replace('_', '-')
    
    # Sort tags by priority
    sorted_tags = sorted(tags, key=lambda t: (tag_priority.get(t, 50), t))
    tag_string = ''.join(f'[{tag}]' for tag in sorted_tags)
    
    # Format date
    date_str = completion_date.strftime('%Y_%m_%d')
    
    # Build new name
    return f"{date_str}-{item_id}-{description}-{tag_string}.md"

# Example usage
old = "TD_012_Dynamic_PO_Pattern_Implementation.md"
date = datetime(2025, 1, 15)
tags = ['refactor', 'completed', 'workflow', 'automation']

new = migrate_filename(old, date, tags)
print(f"Old: {old}")
print(f"New: {new}")
# Output: 2025_01_15-TD_012-dynamic-po-pattern-implementation-[refactor][workflow][automation][completed].md
```

## Benefits

1. **Chronological Visibility**: Instant timeline of completed work
2. **Multi-dimensional Search**: Search by date, type, area, impact, or status
3. **Predictable Patterns**: Tag priority ensures consistent grep patterns
4. **Command-line Friendly**: No special characters needing escape
5. **Self-documenting**: Filename tells complete story without opening file
6. **Batch Operations**: Easy to select groups for operations

## Validation Rules

1. **Date**: Must be valid YYYY_MM_DD format
2. **ID**: Must match original pattern (TYPE_NNN)
3. **Description**: Lowercase kebab-case, 2-4 words
4. **Tags**: 
   - Must have at least one type tag (first position)
   - Must have exactly one status tag (last position)
   - Tags must be ordered by priority
   - No duplicate tags

## Quick Reference Card

```
Format: YYYY_MM_DD-[ID]-description-[type][impact][area][...][status].md

Types:    [bug] [feature] [refactor] [docs] [test]
Impact:   [critical] [dataloss] [security] [breaking]
Area:     [fileops] [ui] [core] [workflow] [agent]
Details:  [automation] [pattern] [framework] [migration]
Status:   [resolved] [completed] [partial] [deprecated]

Search Examples:
  All bugs:           ls *-\[bug\]*.md
  Critical items:     ls *\[critical\]*.md
  Resolved today:     ls 2025_01_16-*\[resolved\].md
  Feature count:      ls *-\[feature\]*.md | wc -l
```

## Maintenance

- Review tag taxonomy quarterly
- Add new tags as needed, maintaining priority structure
- Document any new tags in this guide
- Run validation script after bulk migrations

---

**Version**: 1.0
**Created**: 2025-08-16
**Status**: Active
**Author**: Solo Developer + Claude Code