# Git Workflow - The Single Source of Truth

**IMPORTANT**: This is the ONLY git workflow guide. All personas and documentation reference this file.

## 🚨 The Iron Rules (Enforced by Git Hooks)

1. **NEVER work directly on main** - Always use feature branches
2. **ALWAYS fetch before creating branches** - Start from latest code
3. **ALWAYS rebase before pushing** - Stay current with main
4. **COMMIT frequently** - Small, focused commits are better

## 📜 The Sacred Sequence

This is the ONLY workflow you need to memorize:

```bash
# 1. Start fresh (MANDATORY first step)
git fetch origin
git checkout main && git pull origin main

# 2. Create your feature branch FROM UPDATED MAIN
git checkout -b feat/descriptive-name

# 3. Do your work, commit frequently
git add .
git commit -m "feat: clear description"

# 4. Before pushing, REBASE on latest main
git fetch origin
git rebase origin/main

# 5. Push your branch
git push -u origin feat/descriptive-name

# 6. Create PR via GitHub
gh pr create --title "feat: title" --body "description"
```

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

### "The pre-push hook blocked me!"
```bash
# This means you're behind main. Fix it:
git fetch origin
git rebase origin/main
git push  # Now it will work
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
./scripts/install-hooks.ps1

# Linux/Mac
./scripts/install-hooks.sh
```

The pre-push hook will:
- ❌ Block pushes to main
- ❌ Block pushes from outdated branches  
- ✅ Allow pushes from up-to-date branches
- 📝 Provide clear fix instructions when blocking

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

---

*This workflow is enforced by git hooks. Attempting to bypass it will fail.*