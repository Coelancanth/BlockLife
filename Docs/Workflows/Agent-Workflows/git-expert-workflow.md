# Git Expert Workflow

## Purpose
Define safe procedures for the Git Expert agent to handle complex version control scenarios, resolve conflicts, manage releases, and maintain repository health.

---

## Core Workflow Actions

### 1. Resolve Complex Merge Conflicts

**Trigger**: "Merge conflict help" or "Can't resolve conflict"

**Input Required**:
- Conflicting branches
- Conflict description
- Desired outcome
- File types involved

**Steps**:

1. **Assess Conflict Scope**
   ```bash
   # Check conflict status
   git status
   
   # See conflict markers
   git diff
   
   # Check both versions
   git diff HEAD...MERGE_HEAD
   
   # List conflicted files
   git diff --name-only --diff-filter=U
   ```

2. **Create Safety Backup**
   ```bash
   # Always backup before resolution
   git branch conflict-backup-$(date +%Y%m%d-%H%M%S)
   ```

3. **Analyze Conflict Type**
   ```
   Types to handle differently:
   - Code conflicts (semantic merge)
   - Binary files (choose version)
   - Generated files (regenerate)
   - Dependencies (merge both)
   ```

4. **Resolution Strategies**
   ```bash
   # Strategy 1: Take specific version
   git checkout --theirs path/to/file  # Take incoming
   git checkout --ours path/to/file    # Keep current
   
   # Strategy 2: Manual merge
   # Edit file to combine changes
   # Remove conflict markers
   
   # Strategy 3: Three-way merge tool
   git mergetool
   
   # Strategy 4: Semantic merge (for code)
   # Ensure code compiles and tests pass
   ```

5. **Validate Resolution**
   ```bash
   # Mark as resolved
   git add resolved-file
   
   # Test the merge
   dotnet build
   dotnet test
   
   # Complete merge
   git commit
   ```

**Output Format**:
```
✅ Merge Conflict Resolved

Strategy used: [Manual merge/Take theirs/etc]
Files resolved: [count]
Tests status: Passing

Validation complete - safe to push.
Backup branch: conflict-backup-[timestamp]
```

---

### 2. Clean Up Commit History

**Trigger**: "Clean up commits" or "Squash commits"

**Input Required**:
- Branch to clean
- Commits to organize
- Desired structure

**Steps**:

1. **Backup Current State**
   ```bash
   git branch history-backup-$(date +%Y%m%d-%H%M%S)
   ```

2. **Interactive Rebase**
   ```bash
   # Rebase last N commits
   git rebase -i HEAD~N
   
   # Or rebase from branch point
   git rebase -i main
   ```

3. **Rebase Operations**
   ```
   Commands in rebase:
   pick   = use commit
   reword = change message
   edit   = amend commit
   squash = combine with previous
   fixup  = combine, discard message
   drop   = remove commit
   
   Example sequence:
   pick abc1234 feat: Add inventory system
   squash def5678 WIP: inventory fixes
   fixup ghi9012 typo fix
   pick jkl3456 test: Add inventory tests
   ```

4. **Resolve Conflicts During Rebase**
   ```bash
   # If conflicts occur
   git status
   # Fix conflicts
   git add .
   git rebase --continue
   
   # Or abort if needed
   git rebase --abort
   ```

5. **Force Push Safely**
   ```bash
   # Use --force-with-lease for safety
   git push --force-with-lease origin branch-name
   
   # Never use plain --force on shared branches
   ```

**Output**: Clean, logical commit history

---

### 3. Design Branch Strategy

**Trigger**: "Setup branching strategy" or "How should we branch?"

**Input Required**:
- Team size
- Release cadence
- Environment structure
- CI/CD requirements

**Steps**:

1. **Analyze Requirements**
   ```
   Consider:
   - Release frequency
   - Hotfix needs
   - Feature complexity
   - Team coordination
   ```

2. **Select Strategy**
   ```
   Options:
   
   Git Flow (complex projects):
   - main (production)
   - develop (integration)
   - feature/* (features)
   - release/* (releases)
   - hotfix/* (urgent fixes)
   
   GitHub Flow (simple):
   - main (stable)
   - feature branches
   - PR-based integration
   
   GitLab Flow (environments):
   - main (development)
   - pre-production
   - production
   ```

3. **Document Workflow**
   ```markdown
   # Branch Strategy
   
   ## Branch Types
   - main: Production-ready code
   - feat/*: New features
   - fix/*: Bug fixes
   - hotfix/*: Critical production fixes
   
   ## Workflow
   1. Create feature branch from main
   2. Develop and test
   3. Create PR for review
   4. Merge to main after approval
   5. Deploy from main
   
   ## Protection Rules
   - main: Requires PR + review
   - No direct commits to main
   - Must pass CI/CD
   ```

4. **Setup Branch Protection**
   ```bash
   # GitHub CLI example
   gh repo edit --default-branch main
   gh api repos/:owner/:repo/branches/main/protection \
     --method PUT \
     --field required_status_checks='{"strict":true,"contexts":["continuous-integration"]}' \
     --field enforce_admins=true \
     --field required_pull_request_reviews='{"required_approving_review_count":1}'
   ```

**Output**: Branch strategy documentation and setup

---

### 4. Manage Releases and Tags

**Trigger**: "Create release" or "Tag version"

**Input Required**:
- Version number
- Release notes
- Branch to release from

**Steps**:

1. **Prepare Release Branch**
   ```bash
   # Create release branch
   git checkout -b release/v1.2.0 main
   
   # Update version files
   # Update CHANGELOG.md
   # Run final tests
   ```

2. **Create Annotated Tag**
   ```bash
   # Create tag with message
   git tag -a v1.2.0 -m "Release version 1.2.0
   
   Features:
   - Feature 1
   - Feature 2
   
   Fixes:
   - Bug fix 1
   - Bug fix 2"
   
   # Verify tag
   git show v1.2.0
   ```

3. **Push Release**
   ```bash
   # Push branch and tag
   git push origin release/v1.2.0
   git push origin v1.2.0
   
   # Or push all tags
   git push --tags
   ```

4. **Create GitHub Release**
   ```bash
   gh release create v1.2.0 \
     --title "Release v1.2.0" \
     --notes "Release notes here" \
     --target main
   ```

**Output**: Tagged release with documentation

---

### 5. Recover Lost Work

**Trigger**: "Lost commits" or "Accidentally deleted"

**Input Required**:
- What was lost
- When it was lost
- Last known good state

**Steps**:

1. **Check Reflog**
   ```bash
   # See all recent operations
   git reflog
   
   # Filter by date
   git reflog --since="2 hours ago"
   
   # Find specific commit
   git reflog | grep "commit message"
   ```

2. **Recover Commits**
   ```bash
   # Recover specific commit
   git checkout -b recovery <commit-sha>
   
   # Cherry-pick lost commits
   git cherry-pick <commit-sha>
   
   # Recover deleted branch
   git checkout -b recovered-branch <sha-from-reflog>
   ```

3. **Recover Staged Changes**
   ```bash
   # If accidentally reset
   git fsck --lost-found
   
   # Check dangling blobs
   ls .git/lost-found/other/
   
   # Examine content
   git show <blob-sha>
   ```

4. **Recover Uncommitted Files**
   ```bash
   # Check for auto-saved files
   # IDE recovery folders
   # Temporary directories
   
   # If file was ever staged
   git fsck --unreachable | grep blob
   ```

**Output**: Recovered work with verification

---

### 6. Optimize Repository

**Trigger**: "Repository too large" or "Git is slow"

**Input Required**:
- Repository size
- Performance issues
- Large file identification

**Steps**:

1. **Analyze Repository**
   ```bash
   # Check size
   du -sh .git
   
   # Find large objects
   git rev-list --objects --all | \
     git cat-file --batch-check='%(objecttype) %(objectname) %(objectsize) %(rest)' | \
     sort -k3 -n -r | head -20
   
   # Check for large files
   find . -size +50M -type f
   ```

2. **Clean Up Repository**
   ```bash
   # Garbage collection
   git gc --aggressive --prune=now
   
   # Remove old reflog entries
   git reflog expire --expire=now --all
   
   # Clean unreachable objects
   git prune --expire=now
   ```

3. **Handle Large Files**
   ```bash
   # Setup Git LFS
   git lfs track "*.psd"
   git lfs track "*.zip"
   git add .gitattributes
   
   # Migrate existing files
   git lfs migrate import --include="*.psd,*.zip"
   ```

4. **Remove History (if needed)**
   ```bash
   # WARNING: Rewrites history
   # Remove file from all history
   git filter-branch --force --index-filter \
     "git rm --cached --ignore-unmatch path/to/large-file" \
     --prune-empty --tag-name-filter cat -- --all
   ```

**Output**: Optimized repository with size reduction

---

## Safety Protocols

### Before Any Dangerous Operation
```bash
# 1. Create backup
git branch backup-$(date +%Y%m%d-%H%M%S)

# 2. Verify branch
git branch --show-current

# 3. Check status
git status

# 4. Ensure updated
git fetch --all
```

### Recovery Plan Template
```
If operation fails:
1. Stop immediately (Ctrl+C)
2. Abort operation (git [operation] --abort)
3. Return to backup (git checkout backup-branch)
4. Assess damage
5. Try alternative approach
```

---

## Common Patterns

### Safe Force Push
```bash
# Always use --force-with-lease
git push --force-with-lease origin branch

# Never use plain --force on shared branches
```

### Conflict Resolution Flow
```bash
git pull origin main          # Get latest
git checkout feature-branch   # Your branch
git rebase main               # Rebase onto main
# Fix conflicts
git add .
git rebase --continue
git push --force-with-lease
```

### Release Flow
```bash
git checkout main
git pull origin main
git checkout -b release/v1.0.0
# Prepare release
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin release/v1.0.0
git push origin v1.0.0
```

---

## Pull Request Template Requirements

### PR Template Enforcement

**Mandatory PR Template Sections**:

When creating a Pull Request, the git-expert MUST include the following structured sections:

```markdown
## Changes

[Concise summary of changes introduced by this PR]

- List specific modifications
- Highlight key implementation details
- Reference related work items or issues

## Testing

### Verification Steps
- [ ] Ran unit tests
- [ ] Ran integration tests
- [ ] Manually tested in [relevant environment]

### Test Coverage
- Lines added: [X]
- Lines modified: [Y]
- Test cases added: [Z]

## Checklist

### Code Quality
- [ ] Code follows project style guidelines
- [ ] Added/updated relevant documentation
- [ ] No unnecessary changes

### Review Readiness
- [ ] PR is small and focused
- [ ] All automated checks passing
- [ ] Ready for detailed code review

### Potential Impact
- [ ] Backwards compatibility maintained
- [ ] No known regressions
- [ ] Performance implications considered
```

**Enforcement Criteria**:
1. MUST have all three sections (Changes, Testing, Checklist)
2. Each section MUST have at least 2-3 meaningful entries
3. Checklist items MUST be actionable and checked off
4. NO empty sections allowed

**Failure Modes**:
- Incomplete template will trigger automatic PR rejection
- GitHub Actions will fail PR if template is not fully completed

## Response Templates

### When resolving conflicts:
"🔧 Conflict Resolution Plan

Conflicts in: [N files]
Strategy: [Manual merge/Take theirs/etc]

Steps:
1. Created backup: [branch-name]
2. [Resolution steps]
3. Tests passing: [Yes/No]

Safe to proceed with merge."

### When cleaning history:
"🔧 History Cleanup Complete

Before: [N commits]
After: [M commits]

Changes:
- Squashed WIP commits
- Organized by feature
- Clean commit messages

Ready to force-push with --force-with-lease."

### When recovering work:
"🔧 Recovery Successful

Found: [Lost commits/branches]
Recovered to: [branch-name]
Verification: [Tests pass/Code compiles]

Lost work restored - please verify contents."