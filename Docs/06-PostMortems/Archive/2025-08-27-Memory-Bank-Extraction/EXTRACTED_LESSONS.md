# Extracted Lessons from Memory Bank and Post-Mortems

**Date**: 2025-08-27 13:11
**Extracted by**: Debugger Expert
**Sources**: Memory Bank (all personas), PM_001, Inbox post-mortems

## 🔴 Critical Architecture & Design Patterns

### 1. Simplicity Over Complexity (MOST CRITICAL)
**Source**: Dev Engineer memory, merge over-engineering PM
**Pattern**: The best code is often no code
- **Always ask**: "Can I add one condition to existing code?" BEFORE creating new systems
- **Red flag**: If solution > 100 lines for "simple" feature, stop and reconsider
- **Example**: Merge pattern needed 5 lines, not 369 lines of new recognizer
- **Prevention**: Estimate LOC before coding, >100 = mandatory design review

### 2. Data Flow Completeness 
**Source**: PM_001 (Tier Display Bug), Dev Engineer memory
**Pattern**: When adding fields to domain objects, trace through ENTIRE pipeline
- **Critical points**: Domain → Effect → Notification → Presenter → View
- **Common failure**: Adding field to domain but forgetting effect/notification layer
- **Example**: BlockPlacedEffect missing Tier field broke entire visualization
- **Prevention**: Create data flow checklist for new fields

### 3. Reuse Before Create
**Source**: Tech Lead memory, merge over-engineering PM
**Pattern**: Exhaust reuse opportunities before new abstractions
- **Check existing**: Patterns, services, components before creating new ones
- **Example**: MatchPattern worked perfectly for merge, didn't need TierUpPattern
- **Metric**: 257 lines of focused code vs 500+ with new abstractions

## 🔴 Critical Testing & Quality Patterns

### 4. Test Defaults Must Match Production
**Source**: Debugger Expert memory (BR_015)
**Pattern**: Test helper defaults breaking validation tests
- **Issue**: PlayerState.CreateNew() had MaxUnlockedTier = 2 for testing
- **Impact**: All purchase validation tests failed
- **Fix**: Use UpdatePlayer() helper with version increment for state changes
- **Prevention**: Test defaults should match production defaults

### 5. Start Simple, Expand Incrementally
**Source**: Dev Engineer memory (VS_003B-2)
**Pattern**: Test effectiveness = (Coverage × Maintainability) ÷ Complexity
- **Failed approach**: 21-test suite with complex setup, 45+ min fixing compilation
- **Successful approach**: 7 focused tests covering critical functionality
- **Lesson**: Focused tests > comprehensive tests that don't work

### 6. End-to-End Testing Gaps
**Source**: PM_001, Dev Engineer memory
**Pattern**: UI features need visual verification, not just unit tests
- **Gap**: No automated tests verify visual output
- **Impact**: Tier indicators broke without test coverage
- **Solution**: Need visual regression testing for UI features

## 🔴 Critical Process & Workflow Patterns

### 7. Always Check Glossary FIRST
**Source**: Merge over-engineering PM, Dev Engineer memory
**Pattern**: Terminology errors cascade through entire codebase
- **Example**: Used "TierUp" instead of canonical "Merge" term
- **Impact**: Wrong terminology in 10+ files, tests, documentation
- **Prevention**: Glossary.md is source of truth - check BEFORE coding
- **Enforcement**: Add "Glossary Terms" field to backlog items

### 8. Question Arbitrary Requirements
**Source**: Merge over-engineering PM, Tech Lead memory
**Pattern**: If requirement seems arbitrary, ASK WHY before implementing
- **Example**: "Exactly 3 blocks" for merge made no game sense
- **Correct**: Should be "3+ blocks" like match patterns
- **Prevention**: Requirements must explain "why" for unusual constraints

### 9. Verify All Claims
**Source**: Tech Lead memory (TD_084)
**Pattern**: Don't trust issue descriptions without verification
- **Example**: TD_084 claimed PlayerState used mutable collections - was false
- **Reality**: 40% of claimed violations didn't exist
- **Prevention**: Always check actual code before accepting problem statements

## 🟡 Important Bug Patterns

### 10. Notification Layer Completeness
**Source**: Dev Engineer memory (Match-3), PM_001
**Pattern**: Model changes MUST notify view or nothing happens visually
```csharp
// Step 1: Update model
gridService.RemoveBlock(position);
// Step 2: Tell the view! (CRITICAL)
_mediator.Publish(new BlockRemovedNotification(...));
```

### 11. Conditional Logic Coverage
**Source**: PM_001 (Tier Display Bug)
**Pattern**: New features must work in ALL code paths
- **Bug**: Tier effects only applied in fallback path, not main path
- **Prevention**: Review all conditional branches when adding features

### 12. Service Lifetime Based on State
**Source**: Dev Engineer memory (DI issues)
**Pattern**: Choose service lifetime based on statefulness
- **Stateless services** → Singleton
- **Request state services** → Scoped  
- **Transient state services** → Transient
- **Common mistake**: Registering stateless services as Scoped

### 13. Defensive Programming Required
**Source**: Debugger Expert memory (BR_016)
**Pattern**: Add comprehensive defensive checks
- **Missing**: Null checks, bounds validation in MergePatternExecutor
- **Solution**: Use Fin<T> error handling for graceful degradation
- **Impact**: Prevents runtime exceptions in production

## 🟡 Important Development Patterns

### 14. Namespace Conflict Resolution
**Source**: Dev Engineer memory
**Pattern**: Common names need careful aliasing in test files
```csharp
using ExecutionContext = BlockLife.Core.Features.Block.Patterns.ExecutionContext;
```
- **Common conflicts**: Context, State, Manager, ExecutionContext

### 15. Handler Convention Enforcement
**Source**: Dev Engineer memory
**Pattern**: ALL handlers go in Commands/Queries/Notifications folders
- **Based on**: MediatR interface type, not logical grouping
- **Wrong**: Creating Patterns/Handlers folder
- **Right**: Block/Notifications for INotificationHandler

### 16. All Entry Points Must Trigger
**Source**: Dev Engineer memory (Match detection)
**Pattern**: Game mechanics need handlers for ALL triggering events
- **Bug**: Pattern recognition only on BlockMovedNotification, not BlockPlacedNotification
- **Fix**: Create handlers for each notification type that should trigger patterns

## 🟢 Process Improvements

### 17. Ultra-Careful Approach Works
**Source**: Dev Engineer memory
**Pattern**: Slow is smooth, smooth is fast
- **Method**: Ultra-think each step, validate before proceeding
- **Result**: VS_003B-2 completed in single session with E2E success
- **Contrast**: Previous sessions had more trial-and-error cycles

### 18. Strategic Deferral is Good
**Source**: Tech Lead memory
**Pattern**: Document what you're NOT fixing to prevent scope creep
- **Example**: VS_003B-2 fixed 2 of 3 limitations, deferred tier detection
- **Benefit**: Delivers value faster, prevents over-engineering

### 19. Date Command First for Archives
**Source**: All personas, CLAUDE.md protocol
**Pattern**: ALWAYS run `date` command before creating dated documents
- **Applies to**: Post-mortems, session logs, archive folders
- **Why**: Prevents incorrect timestamps and confusion

### 20. Visual Debugging Invaluable
**Source**: PM_001, Debugger Expert memory
**Pattern**: Temporary overlays crucial for UI debugging
- **Example**: Yellow overlay confirmed tier display fixes
- **Benefit**: Immediate visual confirmation without complex debugging

## 🟢 LanguageExt Specific Patterns

### 21. Context7 BEFORE Implementation
**Source**: CLAUDE.md, Dev Engineer memory
**Pattern**: MANDATORY Context7 query before ANY LanguageExt code
- **Why**: 80% of bugs from incorrect API assumptions
- **Example**: Assumed IsFail was method, AddOrUpdate took lambdas
- **Prevention**: Query Context7 takes 30 seconds, debugging takes 2 hours

### 22. Record Evolution Requires Tracing
**Source**: PM_001
**Pattern**: Adding fields to records requires finding ALL creation sites
- **Example**: BlockPlacedEffect needed tier field in 5+ creation sites
- **Tool**: Use grep/search for all record instantiations

## 📋 Deduplication Notes

### Patterns Merged/Consolidated:
1. **"Don't create new systems"** + **"Reuse existing patterns"** → Combined as #3
2. **"Check glossary"** appears 3 times → Single entry #7
3. **"Service lifetime"** + **"DI issues"** → Combined as #12
4. **"Notification missing"** from multiple sources → Single pattern #10
5. **"Test complexity"** + **"Start simple"** → Combined as #5
6. **"Verify claims"** + **"Check actual code"** → Combined as #9

### Unique Patterns Preserved:
- Each lesson appears ONCE with all relevant sources cited
- Similar patterns grouped by criticality (🔴/🟡/🟢)
- Examples preserved where they add clarity

## 📊 Summary Statistics
- **Total Unique Lessons**: 22
- **Critical (🔴)**: 9
- **Important (🟡)**: 7  
- **Process (🟢)**: 6
- **Most Common Issue**: Over-engineering/complexity (5 related lessons)
- **Most Impactful Fix**: Simplicity over complexity pattern

## Next Steps
1. Update HANDBOOK.md with critical patterns
2. Add testing patterns to Testing.md
3. Update QuickReference.md with gotchas
4. Archive processed post-mortems
5. Create pre-coding checklist from lessons

---
*Extraction completed: 2025-08-27 13:11 by Debugger Expert*
*Sources processed: 6 memory bank files, 2 post-mortems, session logs*