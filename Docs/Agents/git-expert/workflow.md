# Git Expert Workflow

## Purpose
Safe procedures for complex Git operations when basic Git commands aren't sufficient.

---

## Core Actions

### 1. Complex Merge Conflicts

**When**: Multiple files, semantic conflicts, binary conflicts that resist simple resolution

**Steps**:
1. **Safety First**
   ```bash
   git branch conflict-backup-$(date +%Y%m%d-%H%M%S)
   git status  # See all conflicted files
   ```

2. **Resolution Strategy**
   ```bash
   # Strategy A: Take specific version
   git checkout --theirs conflicted-file    # Take incoming
   git checkout --ours conflicted-file      # Keep current
   
   # Strategy B: Manual merge
   # Edit file to combine both changes carefully
   # Remove <<<<<<< ======= >>>>>>> markers
   
   # Strategy C: Three-way merge tool
   git mergetool
   ```

3. **Validate & Complete**
   ```bash
   git add resolved-file
   dotnet build && dotnet test  # Ensure it works
   git commit
   ```

### 2. Repository Recovery

**When**: Lost commits, deleted branches, corrupted repository

**Recovery Steps**:
```bash
# Find lost commits
git reflog --since="1 day ago"

# Recover specific commit
git checkout -b recovery-branch <commit-sha>

# Recover deleted branch
git checkout -b recovered-branch <sha-from-reflog>

# Last resort: find dangling commits
git fsck --lost-found
```

## Safety Protocols

**Before ANY dangerous operation:**
```bash
# 1. Create backup
git branch backup-$(date +%Y%m%d-%H%M%S)

# 2. Verify you're not on main
git branch --show-current

# 3. Check for uncommitted work
git status
```

**Emergency abort commands:**
```bash
git merge --abort
git rebase --abort
git reset --soft HEAD~1  # Undo last commit, keep changes
```

## Response Format

```
🔧 [Operation] Complete

Strategy: [approach used]
Files: [affected files]
Safety: [backup created]
Status: [tests pass/build ok]
```