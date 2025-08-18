# Post-Mortem Archive Index

## Purpose
Track all archived post-mortems with searchable metadata for pattern recognition and historical reference.

## Archive Entries

### 2025-08-18: CI Timing Test Failures
**Location**: `2025-08-18-CI-Timing/`  
**Original Count**: 1 post-mortem  
**Key Issues**: False CI failures from timing-sensitive tests  
**Primary Lessons**:
- Never test wall-clock time in virtualized CI
- Task.Delay can be 7x slower in CI (300ms → 2177ms)
- PR builds vs main builds have different resources
- 100% false positive rate on timing tests

**Solutions Applied**:
- ✅ Skip timing tests in CI environments
- ✅ TD_006 proposed for test categorization
- ✅ Environment detection pattern documented

**Impact**: Eliminated ~50% false failure rate on main branch

---

### 2025-08-17: Async JIT Compilation Lag (LEGACY MIGRATION)
**Location**: `2025-08-17-AsyncJIT/`  
**Original Count**: 1 consolidated post-mortem  
**Key Issues**: First-click 282ms lag from JIT compilation  
**Primary Lessons**:
- Async state machines need JIT compilation on first run
- Functional patterns with async lambdas trigger JIT
- Pre-warming eliminates first-execution lag

**Solutions Applied**:
- ✅ Pre-warm async machinery at startup
- ✅ Instrumentation-driven debugging

**Impact**: Eliminated 282ms first-click lag

---

### 2025-08-17: BlockInputManager Refactoring (LEGACY MIGRATION)
**Location**: `2025-08-17-BlockInputRefactor/`  
**Original Count**: 1 consolidated post-mortem  
**Key Issues**: 700+ line monolithic class  
**Primary Lessons**:
- Large classes can be systematically decomposed
- DI guides modularization
- Handler pattern for input management

**Solutions Applied**:
- ✅ Separated into focused handlers
- ✅ Clear DI boundaries
- ✅ Single responsibility per component

**Impact**: Improved maintainability and testability

---

### 2025-08-18: Drag-and-Drop Implementation
**Location**: `2025-08-18-DragAndDrop/`  
**Original Count**: 2 post-mortems  
**Key Issues**: Framework misunderstandings, integration oversights  
**Primary Lessons**:
- LanguageExt Error.Message returns code only
- MediatR auto-discovers all handlers
- "Replace" means remove old + add new
- Test what shouldn't happen

**Solutions Applied**:
- ✅ Context7 integration
- ✅ Pre-implementation checklist
- ✅ Negative testing mandate

**Impact**: Preventing 60% of assumption-based bugs

---

## Search Tags

### By Issue Type
- **framework-confusion**: 2025-08-18
- **integration-bugs**: 2025-08-18
- **assumption-errors**: 2025-08-18
- **performance-lag**: 2025-08-17 (AsyncJIT)
- **jit-compilation**: 2025-08-17 (AsyncJIT)
- **monolithic-class**: 2025-08-17 (BlockInput)
- **ci-failures**: 2025-08-18 (CI-Timing)
- **timing-tests**: 2025-08-18 (CI-Timing)
- **false-positives**: 2025-08-18 (CI-Timing)

### By Solution
- **context7**: 2025-08-18
- **checklists**: 2025-08-18
- **testing-improvements**: 2025-08-18
- **pre-warming**: 2025-08-17 (AsyncJIT)
- **modularization**: 2025-08-17 (BlockInput)
- **handler-pattern**: 2025-08-17 (BlockInput)
- **test-skipping**: 2025-08-18 (CI-Timing)
- **environment-detection**: 2025-08-18 (CI-Timing)
- **test-categorization**: 2025-08-18 (CI-Timing)

### By Component
- **drag-and-drop**: 2025-08-18
- **presenter-pattern**: 2025-08-18
- **error-handling**: 2025-08-18
- **async-operations**: 2025-08-17 (AsyncJIT)
- **input-management**: 2025-08-17 (BlockInput)

## Statistics

**Total Archived**: 5 post-mortems (2 legacy migrated)  
**Lessons Extracted**: 15+  
**Documents Updated**: 4  
**Systemic Improvements**: 3  

## Archive Protocol

Each entry must include:
1. Date and topic
2. Original post-mortem count
3. Key issues identified
4. Primary lessons learned
5. Solutions applied
6. Expected impact

## Usage

To find similar issues:
1. Search by tags above
2. Check dated folder for details
3. Review EXTRACTED_LESSONS.md
4. Check IMPACT_METRICS.md for effectiveness

---
*Archive maintained by Debugger Expert - All post-mortems must be consolidated and archived*