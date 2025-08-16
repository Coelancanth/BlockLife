# TD_020: Archive Naming Convention Improvement

## Overview
Enhance the archive file naming standard to include more comprehensive metadata for completed work items.

## Current State
- Existing archive files lack consistent, detailed metadata
- Difficult to quickly understand item context from filename

## Naming Convention Options

### Option 1: Prepend Completion Date ⭐ RECOMMENDED
```
2025-01-16_TD_012_Dynamic_PO_Pattern_Implementation.md
YYYY-MM-DD_[TYPE]_[ID]_[Name].md
```
**Pros:**
- Files naturally sort by completion date
- Instantly see when items were completed
- Clean, simple format
- Easy batch operations by date

**Cons:**
- Changes familiar ID-first pattern
- Longer filenames

### Option 2: Append Completion Status
```
TD_012_Dynamic_PO_Pattern_Implementation_COMPLETED_2025Q1.md
[TYPE]_[ID]_[Name]_COMPLETED_[Quarter].md
```
**Pros:**
- Preserves ID-first ordering
- Clear status indicator
- Quarter grouping

**Cons:**
- Very long filenames
- Less precise than exact date
- Redundant (already in completed folder)

### Option 3: Structured Metadata Prefix
```
[2025Q1-DONE]_TD_012_Dynamic_PO_Pattern_Implementation.md
[YYYYQn-STATUS]_[Original_Filename].md
```
**Pros:**
- Compact metadata block
- Flexible status options
- Preserves original name

**Cons:**
- Non-standard bracket notation
- Still changes sort order

### Option 4: Keep Original, Add Metadata File
```
TD_012_Dynamic_PO_Pattern_Implementation.md (unchanged)
TD_012_Dynamic_PO_Pattern_Implementation.meta.json (new)
```
**Pros:**
- No breaking changes
- Rich metadata possible
- Backward compatible

**Cons:**
- Doubles file count
- Metadata separated from content
- More complex to manage

## Complexity
- Estimated Time: 1-2 hours
- Priority: P2

## Acceptance Criteria
- [x] Create naming convention documentation (Archive_Naming_Convention.md)
- [x] Update backlog-maintainer workflow to use new naming
- [x] Add tag determination logic
- [ ] Implement migration script for existing archives
- [ ] Test with next 5 completed items
- [ ] Verify grep patterns work as expected

## Related Work
- See TD_016: Docs Shared Folder Reorganization
- Relates to Automatic Orchestration Pattern

## Notes
Improves long-term maintainability and quick reference for historical work items.