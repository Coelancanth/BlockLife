# Git Workflow - Multi-Clone Architecture

**Updated**: 2025-08-20 - Simplified for multiple clone setup

## 🏗️ Architecture: Multiple Independent Clones

Each persona has its own complete repository clone with unique git identity:
- `blocklife-dev-engineer/` - dev-eng@blocklife
- `blocklife-test-specialist/` - test-spec@blocklife  
- `blocklife-debugger-expert/` - debugger@blocklife
- `blocklife-tech-lead/` - tech-lead@blocklife
- `blocklife-product-owner/` - product@blocklife
- `blocklife-devops-engineer/` - devops-eng@blocklife

## 🚀 Initial Setup

```bash
# One-time setup for all personas
.\scripts\persona\setup-personas.ps1

# Load helper functions in your PowerShell profile
. "C:\Projects\persona-functions.ps1"
```

## 📖 Standard Git Workflow (Simple & Effective)

### Starting New Work
```bash
# 1. Switch to appropriate persona workspace
blocklife-dev  # or blocklife-test, blocklife-debug, etc.

# 2. Ensure you're up to date
git checkout main
git pull origin main

# 3. Create feature branch
git checkout -b feat/your-feature

# 4. Make changes and commit
git add -A
git commit -m "feat: implement new feature"

# 5. Push to remote
git push -u origin feat/your-feature

# 6. Create PR via GitHub or CLI
gh pr create --title "feat: your feature" --body "Description"
```

### Syncing All Personas
```bash
# Fetch latest for all personas at once
.\scripts\persona\sync-personas.ps1

# Or fetch and pull (if no local changes)
.\scripts\persona\sync-personas.ps1 -Pull
```

### Checking Status Across All Clones
```bash
# Show branch and modification status for all personas
blocklife-status
```

## 🎯 Key Benefits of Multi-Clone

1. **Complete Isolation** - Each persona has independent git state
2. **Self-Documenting** - Commits show persona email (dev-eng@blocklife)
3. **No Branch Conflicts** - Multiple clones can use same branch name
4. **Standard Git** - No custom commands or worktree complexity
5. **Simple Recovery** - Corrupted clone? Just delete and re-clone

## ⚡ Quick Commands

```bash
# Navigation shortcuts
blocklife-dev      # → Dev Engineer workspace
blocklife-test     # → Test Specialist workspace
blocklife-debug    # → Debugger Expert workspace
blocklife-tech     # → Tech Lead workspace
blocklife-product  # → Product Owner workspace
blocklife-devops   # → DevOps Engineer workspace

# Status check
blocklife-status   # Show all personas status
```

## 🔒 GitHub Protection (Enforced Server-Side)

GitHub enforces these rules automatically:
- **No direct pushes to main** - Must use PRs
- **Branches must be up-to-date** - Prevents merge conflicts
- **CI must pass** - All tests green before merge
- **Required reviews** - At least one approval needed

## 📝 Commit Message Format

```bash
feat:     # New feature
fix:      # Bug fix
refactor: # Code restructuring
test:     # Test additions/changes
docs:     # Documentation only
perf:     # Performance improvement
chore:    # Maintenance tasks
```

Examples:
- `feat: add block rotation with Q/E keys`
- `fix: prevent blocks from overlapping`
- `refactor: extract validation to service`

## 🚫 What NOT to Do

- ❌ **Don't work directly on main** - Always branch
- ❌ **Don't force push** - Except for your own feature branches
- ❌ **Don't share clones** - Each persona owns its repository
- ❌ **Don't skip tests** - Run locally before pushing

## 🆘 Troubleshooting

### "I have merge conflicts"
```bash
# Update your branch with latest main
git fetch origin
git rebase origin/main
# Resolve conflicts, then continue
git rebase --continue
```

### "I committed to wrong persona"
```bash
# Get commit SHA from wrong persona
git log -1 --format="%H"

# Switch to correct persona
blocklife-dev  # or appropriate persona

# Cherry-pick the commit
git cherry-pick <commit-sha>
```

### "I need to reset everything"
```bash
# Nuclear option - just re-clone
cd ..
rm -rf blocklife-dev-engineer
.\scripts\persona\setup-personas.ps1 -SkipExisting
```

## 📚 References

- [Setup Script](../../scripts/persona/setup-personas.ps1) - Initial setup
- [Sync Script](../../scripts/persona/sync-personas.ps1) - Bulk operations
- [ADR-002](ADR/ADR-002-persona-system-architecture.md) - Architecture decision

---
*Simple, standard git workflow. No custom commands, no complexity, just git.*