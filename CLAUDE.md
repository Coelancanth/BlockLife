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

## 📚 REORGANIZED DOCUMENTATION: The Essential Four

**START HERE for 95% of your needs:**
1. **[Workflow.md](Docs/01-Active/Workflow.md)** ⭐⭐⭐⭐⭐ - Complete development workflow  
2. **[QuickReference.md](Docs/03-Reference/QuickReference.md)** ⭐⭐⭐⭐⭐ - All agent patterns and **lessons learned**
3. **[Architecture.md](Docs/03-Reference/Architecture.md)** ⭐⭐⭐⭐ - Core architectural principles
4. **[Templates/](Docs/05-Templates/)** ⭐⭐⭐⭐ - Work item templates

**Architecture Decisions**: [ADR Directory](Docs/03-Reference/ADR/) - Major technical decisions are documented as ADRs
**Navigation**: [README.md](Docs/README.md) for user journey navigation.

## 📖 Language & Terminology

**[Glossary.md](Docs/03-Reference/Glossary.md)** - Authoritative vocabulary for all code and documentation.
- **MANDATORY**: All code must use these exact terms
- **Examples**: "Turn" not "Round", "Tier" not "Level", "Merge" not "Match"
- **Rule**: Check glossary before naming anything



### Priority Decision Framework
```
🤔 Priority Decision Questions:
1. **Blocking other work?** → 🔥 Critical
2. **Current milestone dependency?** → 📈 Important  
3. **Everything else** → 💡 Ideas

🚨 Critical Indicators:
- Production bugs affecting users
- Dependency needed for current work
- Blocker preventing team progress
- Security vulnerability

📈 Important Indicators:  
- Current milestone features
- Technical debt affecting velocity
- Quality improvements needed
- Performance optimizations

💡 Ideas Indicators:
- Nice-to-have features
- Experimental concepts
- Future considerations
- Research spikes
```

## 🤖 Streamlined Persona System

### 🚨 CRITICAL: Persona Subagent Protocol
**ALL PERSONAS MUST SUGGEST, NEVER AUTO-EXECUTE**
- Every persona file now has prominent protocol headers preventing automation misuse
- Personas must ALWAYS propose subagent commands to user for approval
- NO persona can invoke Task tool directly for subagents without explicit user consent
- This prevents protocol violations like BR_011 and maintains user control

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

**🎮 Game Design**: See `Docs/02-Design/` for complete game mechanics and vision. The user is the Game Designer - these docs are sacred reference material.

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

## ⚠️ CRITICAL: Git Workflow Requirements

**🚨 ENFORCED BY GIT HOOKS - VIOLATIONS WILL BE BLOCKED**

See **[GitWorkflow.md](Docs/03-Reference/GitWorkflow.md)** for the SINGLE SOURCE OF TRUTH on git operations.

**AI MANDATORY BEHAVIOR**:
1. **ALWAYS** follow the Sacred Sequence from [GitWorkflow.md](Docs/03-Reference/GitWorkflow.md)
2. **NEVER** skip fetch/pull before creating branches  
3. **OUTPUT** each git command as you execute it
4. **REBASE** immediately if GitHub branch protection blocks you

**Hook Enforcement Active**:
- ❌ Direct pushes to main are BLOCKED
- ❌ Outdated branches are BLOCKED from pushing
- ✅ Only rebased branches can be pushed

**Example Required Output**:
```bash
# You MUST show each step:
> git fetch origin
> git checkout main && git pull origin main
> git checkout -b feat/br-004-fix
# "Branch created from latest main ✅"
```

**Installation**: Run `./scripts/git/install-hooks.ps1` (Windows) or `./scripts/git/install-hooks.sh` (Linux/Mac)

### Quick Reference Resources
- **Development workflow**: [Workflow.md](Docs/01-Active/Workflow.md)
- **Architecture guidance**: [Architecture.md](Docs/03-Reference/Architecture.md)
- **Architecture decisions**: [ADR Directory](Docs/03-Reference/ADR/) - Documented architectural choices
- **Agent patterns**: [QuickReference.md](Docs/03-Reference/QuickReference.md)
- **Context7 examples**: [Context7Examples.md](Docs/03-Reference/Context7/Context7Examples.md)

## 🚀 Git Survival Guide

### The Daily Workflow (90% of your needs)
```bash
# Start fresh work (ALWAYS sync first!)
git fetch origin
git checkout main && git pull origin main
git checkout -b feat/what-im-building

# Save your progress
git add -A && git commit -m "feat: clear description"

# Push to remote
git push -u origin feat/what-im-building

# Create PR (after push)
gh pr create --title "feat: title" --body "description"
```

### 🚨 Panic Buttons (When Things Go Wrong)

#### "I forgot to create a branch!"
```bash
git stash                          # Save uncommitted work
git fetch origin                   # Get latest refs
git checkout main                  # Switch to main
git pull origin main               # Update main
git checkout -b feat/proper-name  # Create branch from updated main
git stash pop                      # Restore your work
```

#### "I messed up my last commit message!"
```bash
# If NOT pushed yet:
git commit --amend -m "feat: better message"

# If already pushed (use sparingly):
git push --force-with-lease
```

#### "I need to undo my last commit!"
```bash
# Keep changes, just undo commit:
git reset HEAD~1

# Nuclear option - destroy commit and changes:
git reset --hard HEAD~1
```

#### "I'm in merge conflict hell!"
```bash
# Accept all their changes:
git checkout --theirs .
git add -A && git commit

# Accept all your changes:
git checkout --ours .
git add -A && git commit

# Or manually fix conflicts in files, then:
git add -A && git commit
```

#### "I need to update my branch with main!"
```bash
# Preferred - keeps history clean:
git fetch origin
git rebase origin/main

# Alternative if rebase gets messy:
git merge origin/main
```

#### "I committed to the wrong branch!"
```bash
# Get commit SHA first:
git log -1 --format="%H"

# Switch to correct branch:
git checkout correct-branch
git cherry-pick <commit-sha>

# Remove from wrong branch:
git checkout wrong-branch
git reset HEAD~1 --hard
```

### 📝 Commit Message Quick Reference
```bash
feat:     # New feature
fix:      # Bug fix
refactor: # Code restructuring (no behavior change)
test:     # Test additions/changes
docs:     # Documentation only
perf:     # Performance improvement
chore:    # Maintenance tasks (deps, configs)
```

**Examples:**
- `feat: add block rotation with Q/E keys`
- `fix: prevent blocks from overlapping on placement`
- `refactor: extract validation logic to separate service`
- `test: add stress tests for concurrent block moves`

### 🎯 Golden Rules
1. **Never work directly on main** - Always branch
2. **Commit often** - Small, focused commits > giant commits
3. **Pull before branch** - Start from latest main
4. **Write clear messages** - Your future self will thank you
5. **When in doubt** - Ask before `--force` pushing

### 🔥 Emergency Escape Hatch
```bash
# "I've completely broken everything and want to start over"
git fetch origin                   # Get latest refs
git checkout main
git branch -D my-broken-branch     # Delete local branch
git pull origin main               # Get fresh main
git checkout -b feat/fresh-start   # Try again
```

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