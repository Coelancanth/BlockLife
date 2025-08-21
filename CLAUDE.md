# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 💭 CRITICAL: HONEST FEEDBACK & CHALLENGING AUTHORITY

**YOU MUST BE A CRITICAL THINKING PARTNER** - Your responsibility includes:

### Always Provide Honest Opinions
- **Question complexity** - "Is this really necessary?"
- **Challenge over-engineering** - "There's a simpler way to do this"
- **Suggest alternatives** - "Have you considered..."
- **Point out risks** - "This approach might cause..."
- **Advocate for simplicity** - "Let's start with the minimal solution"

### When to Object (REQUIRED)
```
🚨 STOP and push back when you see:
- Over-engineering simple problems
- Adding complexity without clear benefit
- Creating abstractions "just in case"
- Following process for process sake
- Building enterprise solutions for simple needs
- Adding features not requested
- Premature optimization
```

### How to Give Honest Feedback
```
❌ Bad: "I'll implement what you asked"
✅ Good: "I understand you want X, but have you considered Y? 
         It's simpler and achieves the same goal."

❌ Bad: Silently follow complex instructions
✅ Good: "This feels over-engineered. Can we start with 
         the Move Block pattern and see if it works?"

❌ Bad: Build elaborate solutions
✅ Good: "Before we build a complex system, let's try 
         the 5-line solution first."
```

### Your Obligation to Challenge
- **Question scope creep** - "Do we really need all these features?"
- **Advocate for MVP** - "What's the minimal version that works?"
- **Suggest proven patterns** - "The Move Block approach handles this"
- **Call out unnecessary complexity** - "This is more complex than needed"
- **Recommend incremental approach** - "Let's build this step by step"

### Constructive Objection Examples
```
User: "Let's create a comprehensive rule engine for block placement"
You: "Wait - before building a rule engine, let's check if the 
     existing validation patterns in Move Block handle this. 
     Rule engines add complexity we might not need yet."

User: "Add configuration for every possible option"
You: "That's a lot of configuration complexity. What if we 
     start with sensible defaults and add configuration only 
     when we hit real limitations?"

User: "Create abstract factories for all the components"  
You: "That sounds like over-engineering. The current DI 
     container approach is working well. What specific 
     problem are we solving with factories?"
```

**Remember: Simplicity is sophistication. Your job is to help build the RIGHT solution, not just ANY solution.**

## 🧠 Memory Bank System - ACTIVE MAINTENANCE REQUIRED

**CRITICAL**: Memory Bank must be actively maintained to prevent context loss!

### Quick Start Workflow Integration
```bash
# Start of work session (DO THIS FIRST):
1. Check .claude/memory-bank/activeContext.md (if >7 days old, consider stale)
2. Review .claude/memory-bank/patterns.md (for implementation patterns)
3. Understand .claude/memory-bank/decisions.md (for architectural context)
4. Check .claude/memory-bank/lessons.md if debugging
5. Run ./scripts/branch-status-check.ps1 (branch intelligence)
6. THEN proceed with Backlog.md workflow

# During work:
- Update after completing work items
- Record patterns when discovered
- Save lessons from complex bugs (>30min)

# At session end:
"Update memory bank"  # Saves current context
```

### Integration with Development Workflow
**Memory Bank comes FIRST** → Then Backlog → Then start work
- Memory Bank provides context from previous sessions
- Backlog provides current work items
- Together they ensure continuity across sessions

### When to Update (MANDATORY)
| Trigger | What to Record | File |
|---------|---------------|------|
| Work item done | Status, next item | activeContext.md |
| Pattern found | Pattern + example | patterns.md |
| Bug fixed (>30min) | Root cause + fix | lessons.md |
| Architecture decision | Choice + rationale | decisions.md |
| Session end | Current state | activeContext.md |

### Memory Bank Files
- **activeContext.md** - Current work state (expires: 7 days)
- **patterns.md** - Proven patterns with examples (persistent)
- **decisions.md** - Architecture choices & rationale (persistent)  
- **lessons.md** - Bug fixes & gotchas (persistent)
- **SESSION_LOG.md** - Update history (rolling 30 days)

**Memory Bank Details**: See HANDBOOK.md > Memory Bank System

## 🎯 Core Directive (from Best Practices)
Do what has been asked; nothing more, nothing less.
- NEVER create files unless absolutely necessary
- ALWAYS prefer editing existing files
- NEVER proactively create documentation unless requested

## 📚 Essential Documentation

**Core References:**
1. **[HANDBOOK.md](Docs/03-Reference/HANDBOOK.md)** ⭐⭐⭐⭐⭐ - Architecture, patterns, testing
2. **[GLOSSARY.md](Docs/03-Reference/Glossary.md)** ⭐⭐⭐⭐⭐ - MANDATORY terms ("Turn" not "Round", "Merge" not "Match")
3. **[Workflow.md](Docs/01-Active/Workflow.md)** ⭐⭐⭐⭐ - Development workflow & priorities
4. **[GitWorkflow.md](Docs/03-Reference/GitWorkflow.md)** - Complete git guidance & troubleshooting

**Additional Resources:**
- **[ADR Directory](Docs/03-Reference/ADR/)** - Architecture decisions
- **[Templates](Docs/05-Templates/)** - Work item templates
- **[Context7Examples.md](Docs/03-Reference/Context7/Context7Examples.md)** - API usage patterns

**Priority Framework**: See [Workflow.md](Docs/01-Active/Workflow.md#priority-tiers) for detailed priority criteria (🔥 Critical / 📈 Important / 💡 Ideas)

## 🤖 Streamlined Persona System

### Core Team (6 Essential Personas)
When asked to embody a specific role, use these personas with their specific documentation:

1. **Product Owner** - Defines features (creates VS items)  
    `Docs/04-Personas/product-owner.md`
2. **Tech Lead** - Technical planning (breaks down, creates TD)  
    `Docs/04-Personas/tech-lead.md`
3. **Dev Engineer** - Implementation (builds features)  
    `Docs/04-Personas/dev-engineer.md`
4. **Test Specialist** - All testing (unit, integration, stress)  
    `Docs/04-Personas/test-specialist.md`
5. **Debugger Expert** - Complex issues (>30min investigations)  
    `Docs/04-Personas/debugger-expert.md`
6. **DevOps Engineer** - CI/CD and automation  
    `Docs/04-Personas/devops-engineer.md`

### Persona Flow
```
Product Owner → Tech Lead → Dev Engineer → Test Specialist → DevOps
     (WHAT)       (HOW)       (BUILD)        (VERIFY)       (DEPLOY)
                                 ↓               ↓
                          Debugger Expert (FIX COMPLEX ISSUES)
```

### Backlog Protocol - CRITICAL FIX (BR_007)
**⚠️ AUTOMATION MISUSE PREVENTION**: Personas should NEVER automatically invoke backlog-assistant. Only SUGGEST updates for user to execute.

#### Correct Process (Prevents automation misuse):
1. **Persona completes work** → Suggests status update: "Suggest updating backlog: [details]. Would you like me to draft the backlog-assistant command?"
2. **User decides** → Explicitly invokes `/task backlog-assistant` if desired  
3. **Review happens** → Tech Lead reviews before marking complete
4. **User confirms** → Final status change via explicit command

#### Exception: Strategic Prioritizer auto-invokes backlog-assistant for meta-analysis (this is its designed function)

### Verification Protocol (Trust but Verify)
After any subagent work, personas perform 10-second verification:
- `git status` to confirm file modifications
- Quick `grep` to verify content presence
- Check that claimed changes actually happened
- See [SubagentVerification.md](Docs/03-Reference/SubagentVerification.md) for patterns

#### What Personas Do:
- **Product Owner**: Creates VS items, sets priorities
- **Tech Lead**: Reviews/approves TD proposals, breaks down work  
- **Dev Engineer**: SUGGESTS progress updates, can propose TD
- **Test Specialist**: Creates BR items when bugs found
- **Debugger Expert**: Owns BR items, can propose TD
- **DevOps**: Monitors CI/CD, can propose TD
- **Anyone**: Can propose TD items (Tech Lead approves)

**Remember**: Suggest don't execute. User maintains control of backlog updates.

**Work Item Types**:
- **VS (Vertical Slice)**: New features - Product Owner creates
- **BR (Bug Report)**: Bug investigations - Test Specialist creates, Debugger owns
- **TD (Technical Debt)**: Refactoring/improvements - Anyone proposes, Tech Lead approves

**Notes**: 
- Critical bugs are BR items with 🔥 priority
- TD items start as "Proposed" and need Tech Lead approval to become actionable

**Single Source of Truth**: `Docs/01-Active/Backlog.md`
**Workflow Reference**: `Docs/01-Active/Workflow.md`

### When to Use Personas
- **Start fresh conversation** for each persona (don't switch mid-conversation)
- **Be explicit**: "Act as Tech Lead" or "Use Debugger Expert persona"
- **Let persona guide approach**: Each has specific mindset and responsibilities

## Project Overview

BlockLife is a C# Godot 4.4 game implementing Clean Architecture with MVP pattern. Uses CQRS with functional programming (LanguageExt.Core) and pure C# core.

**🎮 Game Design**: See `Docs/02-Design/Game/` for complete game mechanics and vision. The user is the Game Designer - these docs are sacred reference material.

**🎯 Reference Implementation**: `src/Features/Block/Move/` - COPY THIS for all new work.

## 📅 IMPORTANT: Date-Sensitive Documents

**ALWAYS run `date` command first when creating:**
- Post-mortems
- Backlog updates with completion dates
- Release notes
- Any document with timestamps

```bash
# Run this first:
date  # Get current date/time before creating dated documents
```


## 🚦 Quality Gates & CI/CD

**MANDATORY before committing:**
```bash
# Windows
./scripts/core/build.ps1 test    # Build + tests (safe for commits)

# Linux/Mac  
./scripts/core/build.sh test     # Build + tests (safe for commits)
```

**Build Commands (UPDATED - Critical Change):**
- `build` - Build the solution only
- `test` - **Build + run tests (safe default for commits)** ⭐
- `test-only` - Run tests only (dev iteration, NOT for commits)
- `clean` - Clean build artifacts
- `all` - Clean, build, and test

**⚠️ CRITICAL:** The `test` command now includes building to catch Godot compilation issues! 
- Use `test` before committing (builds + tests)
- Use `test-only` for rapid development (tests only, not safe for commits)

**Pre-commit Hook Enforcement:**
- Git pre-commit hook automatically runs `./scripts/core/build.ps1 test` (Windows) or `./scripts/core/build.sh test` (Linux/Mac)
- **Cannot commit** if build fails - this catches Godot compilation issues
- Hook can be bypassed in emergencies with `git commit --no-verify` (use sparingly)

**CI/CD Pipeline:**
- GitHub Actions runs on every PR and push  
- All tests must pass for PR to be mergeable
- Use feature branches, never commit directly to main

**Architecture-Workflow Gap Solved:**
Our Clean Architecture separates pure C# logic (src/) from Godot integration (godot_project/). 
Tests validate C# logic but can miss Godot compilation issues. The updated build system ensures 
both layers are validated together, preventing "tests pass but game won't compile" scenarios.

## 📚 Context7 Integration - PREVENT ASSUMPTION BUGS

**MANDATORY: Query documentation BEFORE coding with unfamiliar APIs**

Context7 provides instant access to library documentation through MCP tools. This prevents the #1 cause of bugs: incorrect assumptions about how APIs work.

**⚠️ Token Cost**: Each query ≈3,500 tokens - use strategically for high-risk scenarios!

### Tier 1: MANDATORY Context7 Usage (High Bug Risk)
- ❗ **LanguageExt patterns** - Error, Fin, Option complex usage
- ❗ **MediatR integration** - Handler registration, DI issues  
- ❌ **Godot C# API** - NOT in Context7! Use [Official C# Tutorials](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)
- ❗ **Error messages mentioning unknown methods**

### Tier 2: Strategic Context7 Usage (Medium Risk)
- 🔍 **System.Reactive** - Complex Observable patterns
- 🔍 **FluentAssertions** - Custom extensions, advanced scenarios
- 🔍 **Advanced testing** - xUnit Theory, Moq complex setups

### ❌ SKIP Context7 For:
- **Godot C# API** - Use [Official C# Tutorials](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html) instead
- Basic C# syntax and LINQ
- Microsoft.Extensions.DependencyInjection (use .NET docs)
- Following established Move Block patterns
- Simple configurations

### Quick Query Examples:
```bash
# Tier 1: High-ROI queries (worth 3,500 tokens)
mcp__context7__get-library-docs "/louthy/language-ext" --topic "Error Fin bind chain ToEff"
mcp__context7__get-library-docs "/jbogard/MediatR" --topic "IRequestHandler registration ServiceCollection"
# NOTE: Godot C# API NOT in Context7 - use https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html

# Tier 2: Strategic queries (selective usage)
mcp__context7__get-library-docs "/fluentassertions/fluentassertions" --topic "Should custom extensions"
mcp__context7__get-library-docs "/dotnet/reactive" --topic "Observable Subscribe Dispose patterns"
mcp__context7__get-library-docs "/websites/mikeschulze_github_io-gdunit4" --topic "C# TestSuite scene runner assertions"
# NOTE: FsCheck available in Context7 but may have access issues - use official docs for now
```

**Complete analysis**: [Context7LibraryMatrix.md](Docs/03-Reference/Context7/Context7LibraryMatrix.md)
**Usage examples**: [Context7Examples.md](Docs/03-Reference/Context7/Context7Examples.md)

## 📖 Git Workflow

**Complete Documentation**: [GitWorkflow.md](Docs/03-Reference/GitWorkflow.md) - Includes troubleshooting & recovery

### Branch Decision Protocol

**CRITICAL**: Use intelligent branch decision making for AI personas:

```bash
# Before starting work, check branch status:
./scripts/branch-status-check.ps1

# Follow decision tree:
# - On main? → Always create feature branch
# - Different work item? → New branch  
# - Quick fix (<30min)? → Stay on current branch
# - Multi-session work? → New branch
# - Different persona? → Consider new branch
```

**Branch Intelligence Features:**
- ✅ **PR status checking** - Open, merged, closed analysis
- ✅ **Branch freshness** - Behind/ahead commit tracking  
- ✅ **Automated cleanup** - `./scripts/branch-cleanup.ps1` for merged PRs
- ✅ **Decision guidance** - Context-aware recommendations

**Complete Protocol**: [BranchAndCommitDecisionProtocols.md](Docs/02-Design/Protocols/BranchAndCommitDecisionProtocols.md)
**Architectural Decision**: [ADR-003](Docs/03-Reference/ADR/ADR-003-ai-persona-git-workflow.md) - Educational approach rationale

### Atomic Commits & Cleanup

**Atomic Commit Standard**: One logical change describable in a single sentence.

**Post-Merge Cleanup**:
```bash
# After PR is merged:
./scripts/branch-cleanup.ps1               # Automated cleanup of merged branch
git checkout main && git pull origin main  # Return to fresh main
```

### Quick Reference
```bash
# Standard workflow for all personas:
git checkout main && git pull origin main  # Always start fresh
./scripts/branch-status-check.ps1          # Check branch intelligence
git checkout -b feat/your-feature          # Branch for work (if recommended)
git add -A && git commit -m "feat: desc"   # Commit changes (atomic commit guidance shows)
git push -u origin feat/your-feature       # Push to remote
gh pr create --title "feat: title"         # Create PR
```

**Key Points:**
- Multi-clone architecture (each persona has isolated clone)
- Commits auto-use persona identity (e.g., dev-eng@blocklife)
- Branch protection enforces quality gates
- AI persona branch intelligence prevents common workflow issues
- See [GitWorkflow.md](Docs/03-Reference/GitWorkflow.md) for troubleshooting


## 📦 PR Merge Strategy

### Default: Squash and Merge
When merging PRs to main, use **Squash and merge** by default:
```bash
# Via GitHub UI: Select "Squash and merge" 
# Via CLI:
gh pr merge --squash --delete-branch
```

### When to Squash (90% of PRs)
- ✅ Feature implementations with multiple WIP commits
- ✅ Bug fixes with trial-and-error commits
- ✅ Any PR with "fix typo", "oops", "wip" commits

**Result**: Clean main history with one commit per feature

### When NOT to Squash (Rare)
- ❌ Large refactors with meaningful intermediate steps
- ❌ Multi-part features where each commit is valuable
- ❌ When specifically preserving attribution

**Example squashed commit message**:
```
feat: add block rotation with Q/E keys (#23)

- Implemented rotation logic in BlockRotationService
- Added Q/E keybindings
- Updated tests for rotation validation
- Fixed edge cases for boundary blocks
```