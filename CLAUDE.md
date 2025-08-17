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
1. **[Workflow.md](Docs/Workflow/Workflow.md)** ⭐⭐⭐⭐⭐ - Complete development workflow  
2. **[QuickReference.md](Docs/Reference/QuickReference.md)** ⭐⭐⭐⭐⭐ - All agent patterns and **lessons learned**
3. **[Architecture.md](Docs/Reference/Architecture.md)** ⭐⭐⭐⭐ - Core architectural principles
4. **[Templates/](Docs/Workflow/Templates/)** ⭐⭐⭐⭐ - Work item templates

**Navigation**: [README.md](Docs/README.md) for user journey navigation or [DOCUMENTATION_CATALOGUE.md](Docs/DOCUMENTATION_CATALOGUE.md) for detailed catalogue.



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

### Core Team (6 Essential Personas)
When asked to embody a specific role, use these personas in `Docs/Workflow/Personas/`:

1. **Product Owner** - Defines features (creates VS items)
2. **Tech Lead** - Technical planning (breaks down, creates TD)
3. **Dev Engineer** - Implementation (builds features)
4. **Test Specialist** - All testing (unit, integration, stress)
5. **Debugger Expert** - Complex issues (>30min investigations)
6. **DevOps Engineer** - CI/CD and automation

### Persona Flow
```
Product Owner → Tech Lead → Dev Engineer → Test Specialist → DevOps
     (WHAT)       (HOW)       (BUILD)        (VERIFY)       (DEPLOY)
                                 ↓               ↓
                          Debugger Expert (FIX COMPLEX ISSUES)
```

### Backlog Protocol
Each persona has embedded backlog responsibilities - no separate maintainer needed:
- **Product Owner**: Creates VS items, sets priorities
- **Tech Lead**: Reviews/approves TD proposals, breaks down work
- **Dev Engineer**: Updates progress, can propose TD
- **Test Specialist**: Creates BR items when bugs found
- **Debugger Expert**: Owns BR items, can propose TD
- **DevOps**: Monitors CI/CD, can propose TD
- **Anyone**: Can propose TD items (Tech Lead approves)

**Work Item Types**:
- **VS (Vertical Slice)**: New features - Product Owner creates
- **BR (Bug Report)**: Bug investigations - Test Specialist creates, Debugger owns
- **TD (Technical Debt)**: Refactoring/improvements - Anyone proposes, Tech Lead approves

**Notes**: 
- Critical bugs are BR items with 🔥 priority
- TD items start as "Proposed" and need Tech Lead approval to become actionable

**Single Source of Truth**: `Docs/Workflow/Backlog.md`
**Workflow Reference**: `Docs/Workflow/Workflow.md`

### When to Use Personas
- **Start fresh conversation** for each persona (don't switch mid-conversation)
- **Be explicit**: "Act as Tech Lead" or "Use Debugger Expert persona"
- **Let persona guide approach**: Each has specific mindset and responsibilities

## Project Overview

BlockLife is a C# Godot 4.4 game implementing Clean Architecture with MVP pattern. Uses CQRS with functional programming (LanguageExt.Core) and pure C# core.

**🎯 Reference Implementation**: `src/Features/Block/Move/` - COPY THIS for all new work.

## 🚦 Quality Gates & CI/CD

**MANDATORY before committing:**
```bash
# Windows
./scripts/build.ps1 test    # Must pass all tests

# Linux/Mac  
./scripts/build.sh test     # Must pass all tests
```

**Build Commands:**
- `build` - Build the solution
- `test` - Run all tests (REQUIRED before commit)
- `clean` - Clean build artifacts
- `all` - Clean, build, and test

**CI/CD Pipeline:**
- GitHub Actions runs on every PR and push
- All tests must pass for PR to be mergeable
- Use feature branches, never commit directly to main

## ⚠️ CRITICAL: Git Workflow Requirements

**🚫 NEVER WORK DIRECTLY ON MAIN BRANCH**

**MANDATORY for ALL changes:**
```bash
# Always start with branch
git checkout -b feat/your-feature-name

# Work, commit, push
git add . && git commit -m "feat: description"
git push -u origin feat/your-feature-name

# Create PR
gh pr create --title "feat: title" --body "description"
```

### Quick Reference Resources
- **Development workflow**: [Workflow.md](Docs/Workflow/Workflow.md)
- **Architecture guidance**: [Architecture.md](Docs/Reference/Architecture.md)
- **Agent patterns**: [QuickReference.md](Docs/Reference/QuickReference.md)

## 🚀 Git Survival Guide

### The Daily Workflow (90% of your needs)
```bash
# Start fresh work
git checkout main && git pull
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
git checkout -b feat/proper-name  # Create branch now
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
git fetch origin main:main && git rebase main

# Alternative if rebase gets messy:
git merge main
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
git checkout main
git branch -D my-broken-branch     # Delete local branch
git pull                            # Get fresh main
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