# Git Workflow Documentation

## Standard Git Workflow

### Multi-Clone Architecture
The project uses multiple independent clones for persona isolation. Each persona works in its own repository clone with standard git commands.

### Daily Workflow (90% of your needs)

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

### Key Points
- Use standard git commands (no custom aliases needed)
- Always branch from updated main
- GitHub branch protection enforces safety
- Each persona has its own isolated clone
- Commits automatically use persona identity (e.g., dev-eng@blocklife)

## 🚨 Git Survival Guide - Panic Buttons

### "I forgot to create a branch!"
```bash
git stash                          # Save uncommitted work
git fetch origin                   # Get latest refs
git checkout main                  # Switch to main
git pull origin main               # Update main
git checkout -b feat/proper-name  # Create branch from updated main
git stash pop                      # Restore your work
```

### "I messed up my last commit message!"
```bash
# If NOT pushed yet:
git commit --amend -m "feat: better message"

# If already pushed (use sparingly):
git push --force-with-lease
```

### "I need to undo my last commit!"
```bash
# Keep changes, just undo commit:
git reset HEAD~1

# Nuclear option - destroy commit and changes:
git reset --hard HEAD~1
```

### "I'm in merge conflict hell!"
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

### "I need to update my branch with main!"
```bash
# Preferred - keeps history clean:
git fetch origin
git rebase origin/main

# Alternative if rebase gets messy:
git merge origin/main
```

### "I committed to the wrong branch!"
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

## 📝 Commit Message Quick Reference

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

## 🎯 Golden Rules

1. **Never work directly on main** - Always branch
2. **Commit often** - Small, focused commits > giant commits
3. **Pull before branch** - Start from latest main
4. **Write clear messages** - Your future self will thank you
5. **When in doubt** - Ask before `--force` pushing

## 🔥 Emergency Escape Hatch

```bash
# "I've completely broken everything and want to start over"
git fetch origin                   # Get latest refs
git checkout main
git branch -D my-broken-branch     # Delete local branch
git pull origin main               # Get fresh main
git checkout -b feat/fresh-start   # Try again
```

## Branch Management

### Naming Conventions
- `feat/` - New features
- `fix/` - Bug fixes
- `refactor/` - Code restructuring
- `docs/` - Documentation updates
- `test/` - Test additions
- `chore/` - Maintenance tasks

### Branch Cleanup
```bash
# Delete local branch after merge
git branch -d feat/completed-feature

# Delete remote tracking branches
git remote prune origin

# See all branches (local and remote)
git branch -a

# Clean up old branches
git branch --merged | grep -v main | xargs -n 1 git branch -d
```

## PR Best Practices

### Creating Good PRs
1. Keep PRs focused on a single change
2. Write descriptive titles and descriptions
3. Reference related issues/tickets
4. Include test coverage
5. Update documentation if needed

### PR Template
```markdown
## Summary
Brief description of changes

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed

## Checklist
- [ ] Code follows project standards
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No console errors
```