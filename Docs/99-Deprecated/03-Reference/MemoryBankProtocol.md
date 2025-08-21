# Memory Bank Protocol

**Last Updated**: 2025-08-21  
**Status**: Active  
**Purpose**: Define when, how, and what to persist across Claude sessions

## 🎯 Core Purpose

The Memory Bank maintains critical context between Claude sessions, reducing re-establishment time by 50-70% and preventing decision amnesia.

## 📂 Memory Bank Structure

```
.claude/memory-bank/
├── activeContext.md     # Current work state (expires: 7 days)
├── patterns.md          # Established patterns (persistent)
├── decisions.md         # Architectural choices (persistent)
├── lessons.md          # Bug fixes & gotchas (persistent)
└── SESSION_LOG.md       # Update history (rolling 30 days)
```

## 🔄 Update Triggers & Responsibility

### Automatic Updates (Claude-initiated)

| Trigger | File Updated | What Gets Recorded | Example |
|---------|--------------|-------------------|---------|
| **Work Item Completed** | activeContext.md | Item closed, next item | "Completed VS_003, moving to BR_012" |
| **Pattern Discovered** | patterns.md | New pattern + example | "Found: Use Fin for all async operations" |
| **Bug Fixed (>30min)** | lessons.md | Root cause + solution | "GdUnit4 requires [TestSuite] on class" |
| **Architecture Decision** | decisions.md | Decision + rationale | "Chose Husky.NET over pre-commit" |
| **Session Start** | SESSION_LOG.md | Timestamp + context loaded | "2025-08-21 09:00 - Loaded activeContext" |
| **Session End** | activeContext.md | Current state snapshot | "Working on TD_042, 50% complete" |
| **Persona Switch** | activeContext.md | Handoff state | "DevOps → Tech Lead: PR #123 ready" |

### Manual Updates (User-requested)

```bash
# Explicit commands that trigger updates:
"Update memory bank"          # Full update of all files
"Save context"                # Update activeContext.md only
"Record pattern: [pattern]"   # Add to patterns.md
"Record decision: [decision]" # Add to decisions.md
"Checkpoint"                  # Quick activeContext snapshot
```

## 📝 File Specifications

### activeContext.md
```markdown
# Active Context
**Last Updated**: 2025-08-21 09:30
**Session**: DevOps Engineer
**Expires**: 2025-08-28

## Current Work
- **Active**: TD_042 - Consolidate archives (50% complete)
- **Blocked**: Waiting for PR #122 review
- **Next**: TD_038 - Validation system

## Open Branches
- feat/TD_042-consolidate-archives (current)
- fix/BR_011-test-flake (pending push)

## Key Decisions This Session
- Switched from vs-003 to VS_003 naming
- Implemented Husky.NET for hooks
- Removed Sacred Sequence

## Handoff Notes
Tech Lead needs to review TD_042 implementation
```

### patterns.md
```markdown
# Established Patterns
**Last Updated**: 2025-08-21

## Async Error Handling
**Pattern**: Always use Fin<T> for async operations
**Example**: `src/Features/Block/Move/MoveBlockService.cs:45`
**Rationale**: Consistent error propagation
**Added**: 2025-08-15

## Test Organization
**Pattern**: One test file per service
**Example**: `tests/BlockLife.Tests/Features/Block/`
**Rationale**: Maintainable, discoverable
**Added**: 2025-08-10
```

### decisions.md
```markdown
# Architectural Decisions
**Last Updated**: 2025-08-21

## 2025-08-21: Git Hooks via Husky.NET
**Decision**: Use Husky.NET instead of manual hooks
**Rationale**: 
- Zero-config across 6 persona clones
- Automatic installation via dotnet restore
- Consistent with .NET ecosystem
**Rejected**: pre-commit (Python), manual scripts
**Impact**: All personas get same hooks automatically

## 2025-08-20: Multi-Clone over Worktrees
**Decision**: Separate clones for each persona
**Rationale**: Complete isolation, standard git
**ADR**: ADR-002-persona-system-architecture.md
```

### lessons.md
```markdown
# Lessons Learned (Bug Fixes & Gotchas)
**Last Updated**: 2025-08-21

## GdUnit4 Test Discovery
**Issue**: Tests not found despite correct structure
**Root Cause**: Missing [TestSuite] attribute on class
**Solution**: All test classes need [TestSuite]
**Time Wasted**: 45 minutes
**Date**: 2025-08-18

## Godot Compilation vs C# Tests
**Issue**: Tests pass but game won't compile
**Root Cause**: Tests only validate C# layer
**Solution**: Build command must include Godot project
**Fixed In**: build.ps1 update
**Date**: 2025-08-15
```

## 🚦 Update Rules

### WHEN to Update

1. **ALWAYS Update After**:
   - Completing any work item (VS/BR/TD)
   - Making architectural decisions
   - Fixing bugs that took >30 minutes
   - Discovering new patterns
   - Before ending a session

2. **NEVER Update For**:
   - Trivial changes (formatting, typos)
   - Temporary experiments
   - Work in progress (unless checkpointing)

3. **Update Immediately When**:
   - Context would be lost otherwise
   - Handing off work to another persona
   - Finding critical information

### HOW to Update

1. **Append, Don't Replace** (except activeContext.md)
2. **Include Timestamps** on all entries
3. **Reference Code Locations** when applicable
4. **Keep Entries Concise** (max 5 lines per entry)
5. **Link to ADRs/Docs** for details

### WHAT to Include

✅ **DO Include**:
- Decisions and their rationale
- Non-obvious solutions
- Failed approaches (prevent repetition)
- Performance discoveries
- Integration gotchas

❌ **DON'T Include**:
- Obvious information
- Code snippets (reference locations instead)
- Temporary TODOs
- Personal opinions
- Sensitive information

## 🔍 Reading Protocol

### Session Start
```bash
1. Check activeContext.md age (>7 days = stale)
2. Verify current branch matches context
3. Load patterns.md if doing implementation
4. Check lessons.md if debugging
```

### Context Conflicts
```bash
If memory says "Working on VS_003" but it's completed:
1. Trust current state (Backlog.md)
2. Update activeContext.md immediately
3. Log conflict in SESSION_LOG.md
```

## 📊 Maintenance

### Pruning Rules
- **activeContext.md**: Clear items older than 7 days
- **SESSION_LOG.md**: Keep last 30 days only
- **patterns.md**: Review quarterly, archive unused
- **decisions.md**: Never delete, mark deprecated
- **lessons.md**: Keep forever (valuable history)

### Size Limits
- Each file max 500 lines
- When exceeded, archive with date suffix
- Example: `patterns_2025Q1.md`

## 🎯 Success Metrics

- **Context Reload Time**: <30 seconds (was 5+ minutes)
- **Decision Recall**: 100% (was 60%)
- **Pattern Reuse**: 80%+ (was 40%)
- **Bug Repeat Rate**: <5% (was 20%)

## 💡 Implementation Examples

### Example: After Completing TD_042
```markdown
# Update to activeContext.md
## Completed Work
- ✅ TD_042: Archive consolidation (2025-08-21)
  - Merged Archive.md into Completed_Backlog.md
  - Updated all persona references
  - Removed duplicate file

## Current Work
- **Active**: TD_038 - Validation system (0% complete)
```

### Example: After Finding Pattern
```markdown
# Addition to patterns.md
## Hook Installation Pattern
**Pattern**: Use .csproj targets for auto-installation
**Example**: `BlockLife.Core.csproj:22-25`
**Rationale**: Zero-config across all clones
**Discovered**: 2025-08-21 during TD_039
```

### Example: After Complex Bug
```markdown
# Addition to lessons.md
## Husky.NET Hook Path Issue
**Issue**: Hooks not executing despite installation
**Root Cause**: Git config core.hookspath not set
**Solution**: Husky install sets path to .husky
**Debug Command**: git config --get core.hookspath
**Time Wasted**: 35 minutes
**Date**: 2025-08-21
```

## 🚀 Quick Reference

### Update Commands
```bash
# Full update
"Update memory bank with: [context]"

# Specific updates
"Add pattern: [pattern description]"
"Record decision: [what and why]"
"Save lesson: [bug and solution]"
"Checkpoint context"

# Read operations
"Load memory bank"
"What patterns exist for [topic]?"
"Any lessons about [error]?"
```

### Persona Handoff Template
```markdown
## Handoff: [From] → [To]
**Time**: 2025-08-21 10:30
**Work Item**: TD_042
**Status**: Implementation complete, needs review
**Branch**: feat/TD_042-consolidate
**Next Steps**: Run validation, merge if clean
**Notes**: Archive.md deleted, all content preserved
```

---

**Remember**: The Memory Bank is only valuable if it's maintained. Update early, update often, but update purposefully.