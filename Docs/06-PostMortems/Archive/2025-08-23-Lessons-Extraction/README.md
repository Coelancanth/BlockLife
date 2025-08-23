# 2025-08-23 Lessons Extraction Archive

## Overview
This archive contains 10 post-mortems and 3 extraction documents from a comprehensive knowledge consolidation effort.

## File Naming Convention

### Post-Mortems (Date-Based)
Format: `YYYY-MM-DD-[Type]-Description.md`
- Date in ISO format (YYYY-MM-DD)
- Type prefix when applicable (TD, BR, etc.)
- Title-Case description with hyphens
- Examples:
  - `2025-08-23-Namespace-DI-Resolution.md`
  - `2025-08-19-TD021-Namespace-Collision-Fix.md`
  - `2025-08-19-BR011-Archive-Data-Loss.md`

### Extraction Documents (PM-Based)
Format: `PM_XXX-Description.md`
- PM_XXX sequential numbering
- Title-Case description with hyphens
- Documents:
  - `PM_004-Memory-Bank-Lessons-Extraction.md` - All persona memory bank lessons
  - `PM_005-Consolidation-Summary.md` - Summary of extraction work
  - `PM_006-Extracted-Lessons-Master.md` - Master list of all lessons

## Contents Summary

### Original Post-Mortems (10)
1. **2025-08-18-TD013-Visual-Logic-Mismatch.md** - Visual/logic sync issues
2. **2025-08-19-BR011-Archive-Data-Loss.md** - Archive data loss bug
3. **2025-08-19-TD015-TD016-Architecture-Safeguards.md** - Architecture protection
4. **2025-08-19-TD021-Namespace-Collision-Fix.md** - Namespace collision resolution
5. **2025-08-19-View-Notification-Pattern.md** - View notification patterns
6. **2025-08-20-TD026-028-Systematic-Technical-Debt-Implementation.md** - TD implementation
7. **2025-08-21-README-Documentation-Integrity-Failure.md** - Documentation issues
8. **2025-08-21-False-Simplicity-Trap.md** - Over-simplification problems
9. **2025-08-22-Protocol-Violation-Direct-Main-Push.md** - Process violation
10. **2025-08-23-Namespace-DI-Resolution.md** - MediatR discovery failure

### Extraction Documents (3)
- **PM_004**: Extracted 50+ lessons from 6 persona memory banks
- **PM_005**: Documents the consolidation process and results
- **PM_006**: Master list with action items for documentation updates

## Key Lessons Extracted

### Technical
- Namespace MUST be `BlockLife.Core.*` for MediatR discovery
- Single DI registration miss causes 30+ test failures
- Modern C# uses `required` properties, not constructors
- Project uses Moq, not NSubstitute

### Process
- Phase-based implementation prevents context exhaustion
- Complex features take 1.5x estimated time
- False simplicity trap: undefined ≠ simple
- Post-mortems must be consolidated and archived

### Testing
- FsCheck 3.x: Generators return `Gen<T>`, use `.ToArbitrary()`
- Regression tests immediately after bug fixes
- Property tests find edge cases unit tests miss

## Impact

- **Knowledge Preserved**: 50+ critical lessons
- **Documentation Updated**: HANDBOOK.md, Testing.md created
- **Time Saved**: Estimated 100+ hours of future debugging
- **Quality Improved**: Regression tests prevent recurrence

## Archive Date
**Consolidated**: 2025-08-23 17:00
**Consolidator**: Debugger Expert

---

*"A post-mortem in the active directory is a failure of the Debugger Expert" - Successfully achieved*