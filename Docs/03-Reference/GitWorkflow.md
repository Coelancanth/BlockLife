# Git Workflow - The Single Source of Truth

**IMPORTANT**: This is the ONLY git workflow guide. All personas and documentation reference this file.

> 📦 **Multi-Clone Architecture Coming**: TD_035 will migrate from worktrees to multiple clones.  
> See [GitWorkflow-MultiClone.md](GitWorkflow-MultiClone.md) for the new workflow design.  
> This document will be updated when TD_035 is implemented.

## 🆕 Sacred Sequence Enforcement System (TD_034)

**As of 2025-08-20**: Smart git commands and pre-push hooks now ENFORCE the Sacred Sequence automatically.

### Installation (One Time)
```bash
# From BlockLife root directory
.\scripts\git\install-sacred-sequence.ps1
```

### The New Way - Smart Commands
```bash
# ✅ USE THESE COMMANDS (Sacred Sequence built-in):
git newbranch feat/save-system  # Auto-fetches, updates main, creates branch
git syncmain                     # Auto-fetches and rebases current branch
git sacred                       # Shows sync status and available commands
git checkfresh                   # Verifies branch is up-to-date

# ❌ AVOID THESE (will trigger warnings):
git checkout -b                  # Use git newbranch instead
```

## 🚨 The Iron Rules (Now Enforced by Hooks)

1. **NEVER work directly on main** - Branch protection blocks direct pushes
2. **ALWAYS use `git newbranch`** - Enforces fetch before creating branches
3. **Pre-push hook validates freshness** - Blocks stale branches automatically
4. **COMMIT frequently** - Small, focused commits are better
5. **CI MUST PASS** - Merges blocked until build-and-test succeeds
6. **NO PARALLEL WORK** - VS lock system prevents duplicate efforts

## 📜 The Sacred Sequence (Traditional Way)

If you need to do it manually (or smart commands aren't installed):

```bash
# 1. Start fresh (MANDATORY first step)
git fetch origin
git checkout main && git pull origin main

# 2. Create your feature branch FROM UPDATED MAIN
# IMPORTANT: Use proper naming convention!
# VS items: feat/vs-XXX-description
# BR items: fix/br-XXX-description  
# TD items: feat/td-XXX-description
git checkout -b feat/vs-003-match-system  # Example

# 3. Do your work, commit frequently
git add .
git commit -m "feat: clear description"

# 4. Before pushing, REBASE on latest main
git fetch origin
git rebase origin/main

# 5. Push your branch
git push -u origin feat/vs-003-match-system

# 6. Create PR via GitHub (template will guide you)
gh pr create --title "feat: title" --body "description"
```

## 🤖 AI Agent Workflow (MANDATORY)

**AI agents MUST use smart commands to prevent Sacred Sequence violations:**

```bash
# Starting new work:
git sacred                       # Check status first
git newbranch feat/td-034-impl  # Creates from fresh main

# During development:
git add -A
git commit -m "feat: implement Sacred Sequence enforcement"

# Before pushing:
git checkfresh                   # Verify still current
git push -u origin feat/td-034-impl

# If pre-push hook blocks you:
git syncmain                     # Auto-fixes the issue
git push                         # Try again
```

## 🔒 Branch Naming Convention (NEW - Prevents BR_006)

**MANDATORY naming patterns** for work items:
- **VS items**: `feat/vs-XXX-description` (e.g., `feat/vs-003-match-system`)
- **BR items**: `fix/br-XXX-description` (e.g., `fix/br-006-parallel-work`)
- **TD items**: `feat/td-XXX-description` (e.g., `feat/td-009-persona-commands`)
- **Other work**: `feat/`, `fix/`, `docs/`, `chore/` (no restrictions)

**Why this matters**:
- Automated VS lock system prevents parallel work on same item
- Clear intent from branch name
- Automatic backlog validation
- PR template auto-fills from branch name

## ⚠️ What Happens If You Skip Steps?

**Skip Step 1 (fetch/pull)?**
- You branch from outdated code
- You "fix" already-fixed bugs  
- You create conflicts for others

**Skip Step 4 (rebase)?**
- Git hook BLOCKS your push ❌
- You must rebase before pushing
- This is enforced automatically

## 🛠️ Common Scenarios

### "I forgot to create a branch!"
```bash
git stash                          # Save your work
git checkout main                  
git pull origin main               
git checkout -b feat/proper-name  
git stash pop                      # Restore your work
```

### "I need to update my branch with latest main"
```bash
git fetch origin
git rebase origin/main

# If conflicts occur:
# 1. Fix conflicts in your editor
# 2. git add <fixed-files>
# 3. git rebase --continue
```

### "GitHub blocked my push to main!"
```bash
# Create a feature branch instead:
git checkout -b feat/your-feature-name
git push -u origin feat/your-feature-name
# Then create a PR through GitHub
```

## 🤖 For AI Assistants

**MANDATORY BEHAVIOR**:
1. ALWAYS run the sacred sequence when starting work
2. NEVER skip the fetch/pull steps
3. Output each git command as you run it
4. If the hook blocks you, rebase immediately

**Example AI workflow output**:
```
"Starting work on feat/block-rotation..."
> git fetch origin
> git checkout main && git pull origin main  
> git checkout -b feat/block-rotation
"Branch created from latest main ✅"
```

## 🔧 Hook Installation

First time setup (run once):
```bash
# Windows
./scripts/git/install-hooks.ps1

# Linux/Mac
./scripts/git/install-hooks.sh
```

The git hooks will:
- 🚀 **Pre-commit**: Run build+tests before each commit (quality gate)
- 🏷️ **Pre-checkout**: Validate branch naming (workflow guide)
- 🔒 **GitHub Protection**: Block direct main pushes (process gate)
- ✅ Clear separation: Local hooks = quality, Remote protection = process

## 📝 Commit Message Format

```
type: description

Types:
feat:     New feature
fix:      Bug fix
refactor: Code restructuring
test:     Test changes
docs:     Documentation only
perf:     Performance improvement
chore:    Maintenance tasks
```

Examples:
- `feat: add block rotation with Q/E keys`
- `fix: prevent blocks from overlapping`
- `refactor: extract validation logic`

## 🚀 Why This Workflow?

1. **Prevents conflicts** - Always working from latest code
2. **Clean history** - Rebase keeps history linear
3. **No surprises** - Everyone follows same rules
4. **Automated safety** - Hooks catch mistakes
5. **Fast integration** - PRs merge cleanly

## 🤖 Automated Protections (Active Now!)

### Branch Protection (GitHub)
- ❌ **Direct pushes to main blocked** - Must use PR
- ❌ **Merges blocked if CI fails** - Tests must pass
- ❌ **Merges blocked if branch outdated** - Must rebase first
- ✅ **Review required** - At least 1 approval needed

### VS Lock System (Design Guard)
When you open a PR for a work item (VS/BR/TD):
1. **Checks for conflicts** - Fails if someone else is working on same item
2. **Adds lock label** - Shows item is in progress
3. **Validates backlog** - Warns if item not found
4. **Releases lock** - Automatically when PR closes

### What This Prevents
- ✅ **No more BR_006** - Parallel incompatible features impossible
- ✅ **No broken builds on main** - CI must pass
- ✅ **No merge conflicts** - Branches must be current
- ✅ **Clear ownership** - Lock labels show who's working on what

---

*This workflow is enforced by GitHub Actions and branch protection. Attempting to bypass it will fail.*