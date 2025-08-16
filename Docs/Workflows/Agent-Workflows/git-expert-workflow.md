# Git Expert Workflow

## Purpose
Define safe procedures for the Git Expert agent to handle complex version control scenarios, resolve conflicts, manage releases, and maintain repository health.

---

## Core Workflow Actions

### 1. Smart Commit with Auto-Branch Selection

**Trigger**: "Commit", "git time", or any commit request

**Automated Workflow**:

1. **Analyze Context**
   ```bash
   # Check current branch
   current_branch=$(git branch --show-current)
   
   # If on main, MUST create feature branch
   if [ "$current_branch" = "main" ]; then
       # Trigger auto-selection
       → Run Branch Auto-Selection (Section 3)
   fi
   ```

2. **Auto-Select Branch (if needed)**
   ```
   Steps:
   1. Read Backlog.md for active work items
   2. Analyze git diff for change scope
   3. Determine optimal branch type
   4. Create branch with smart naming
   5. Show reasoning to user
   ```

3. **Example Auto-Selection Flow**
   ```
   User: "commit these changes"
   
   Git-Expert: 
   🤖 Analyzing context...
   
   📊 Detected:
   - Currently on: main (PROTECTED)
   - Active work: TD_006 (Advanced Logger)
   - Changes: 3 files in src/Infrastructure/Logging/
   - Type: Technical improvement
   
   🎯 Creating branch: refactor/td006-advanced-logger
   
   ✅ Switched to new branch
   → Proceeding with commit...
   ```

4. **Proceed with Commit**
   ```bash
   # Standard commit flow continues
   git add [files]
   git commit -m "[message]"
   ```

**Integration with Backlog**: Auto-selection always checks Backlog.md first for context.

---

### 2. Resolve Complex Merge Conflicts

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

### 3. Intelligent Branch Auto-Selection

**Trigger**: "Git time", "Commit", "Create PR", or any git operation needing a branch

**Purpose**: Automatically determine the optimal branch type and name based on context, reducing cognitive load and ensuring consistency.

**Auto-Selection Workflow**:

#### Phase 1: Context Gathering

1. **Read Backlog Status**
   ```bash
   # Check current work items in Backlog.md
   # Parse active work items and their status
   # Identify work item type (VS_, BF_, TD_, HF_)
   ```

2. **Analyze Current State**
   ```bash
   # Current branch
   git branch --show-current
   
   # Changed files
   git diff --name-only
   git diff --stat
   
   # Change scope
   git diff --numstat | wc -l  # Number of changes
   ```

3. **Scope Classification**
   ```
   Single File Changes:
   - < 50 lines: minor fix
   - > 50 lines: focused feature
   
   Multi-File Changes:
   - Same directory: feature work
   - Cross-directory: refactor or architecture
   - Only docs: documentation update
   - Only tests: test enhancement
   ```

#### Phase 2: Intelligent Branch Selection Algorithm

```python
def select_branch_type(context):
    """
    Auto-select branch based on comprehensive context
    """
    
    # Priority 1: Work Item Type Mapping
    if work_item_type == "HF_":
        return "hotfix/[description]"  # Critical production fix
    elif work_item_type == "VS_":
        return "feat/[description]"     # Vertical Slice feature
    elif work_item_type == "BF_":
        return "fix/[description]"      # Bug fix
    elif work_item_type == "TD_":
        return "refactor/[description]" # Tech debt
    
    # Priority 2: Change Scope Analysis
    elif only_docs_changed:
        return "docs/[description]"
    elif only_tests_changed:
        return "test/[description]"
    elif ci_files_changed:
        return "chore/ci-[description]"
    
    # Priority 3: Completion Status
    elif work_item_status == "90% complete":
        return "fix/final-[description]"  # Finishing touches
    elif work_item_status == "starting":
        return "feat/[description]"       # New work
    
    # Priority 4: Conservative Default
    else:
        return "feat/[description]"  # Safe default
```

#### Phase 3: Smart Branch Naming

1. **Extract Context Clues**
   ```
   From Work Item:
   - VS_001_User_Authentication → feat/user-authentication
   - BF_003_Login_Crash → fix/login-crash
   - TD_010_Refactor_Services → refactor/services
   
   From Changed Files:
   - src/Features/Auth/* → feat/auth-enhancement
   - tests/* → test/coverage-improvement
   - Docs/* → docs/update-guides
   ```

2. **Generate Descriptive Name**
   ```
   Rules:
   - Use kebab-case
   - Max 50 characters
   - Include work item ID if available
   - Be specific but concise
   
   Examples:
   - feat/vs001-user-authentication
   - fix/bf003-login-crash-resolution
   - refactor/td010-service-consolidation
   ```

#### Phase 4: Transparent Communication

**Always explain the auto-selection reasoning:**

```markdown
🤖 Branch Auto-Selection Analysis:

📊 Context Detected:
- Work Item: VS_001 (User Authentication)
- Status: 30% complete
- Changed Files: 5 files in src/Features/Auth/
- Change Scope: 150 lines added, 20 modified
- File Types: C# implementation + tests

🎯 Recommendation: `feat/vs001-user-authentication`

📝 Reasoning:
- VS_ work item → feature branch
- Active feature development (30% complete)
- Focused changes in single feature area
- Includes both implementation and tests

✅ Proceed with this branch? (Y/n)
```

#### Phase 5: Override Capabilities

**User Control Points:**

1. **Pre-Selection Confirmation**
   ```
   "I detected you're working on VS_001. Should I create:
   → feat/vs001-user-authentication (recommended)
   → Or specify custom: _____"
   ```

2. **Override Commands**
   ```bash
   # Force specific branch type
   --branch-type=hotfix
   
   # Custom naming
   --branch-name=my-custom-name
   
   # Skip auto-selection
   --no-auto-select
   ```

3. **Context Hints**
   ```
   User can provide hints:
   - "This is urgent" → hotfix/
   - "Just cleaning up" → chore/
   - "Experimental work" → experiment/
   ```

### 4. Branch Strategy Management

**Trigger**: "Setup branching strategy" or "How should we branch?"

**BlockLife Standard Strategy** (Auto-Applied):

```
Branch Types & Auto-Selection Rules:
├── main (protected)
│   └── Never work directly here
├── feat/* (features)
│   └── Auto-selected for: VS_ items, new functionality
├── fix/* (bug fixes)
│   └── Auto-selected for: BF_ items, issue resolution
├── hotfix/* (critical fixes)
│   └── Auto-selected for: HF_ items, production issues
├── refactor/* (code improvement)
│   └── Auto-selected for: TD_ items, cleanup work
├── docs/* (documentation)
│   └── Auto-selected when: only *.md files changed
├── test/* (test improvements)
│   └── Auto-selected when: only test files changed
└── chore/* (maintenance)
    └── Auto-selected for: CI/CD, dependencies, configs
```

**Auto-Selection Integration Points:**

1. **Commit Workflow**
   ```bash
   # User says "commit this"
   # Git-expert automatically:
   1. Analyzes changes
   2. Checks Backlog.md
   3. Suggests branch
   4. Creates if needed
   5. Commits with context
   ```

2. **PR Creation**
   ```bash
   # User says "create PR"
   # Git-expert automatically:
   1. Determines target branch
   2. Generates PR title from branch
   3. Links work items
   4. Fills PR template
   ```

3. **Branch Switching**
   ```bash
   # User says "work on user auth"
   # Git-expert automatically:
   1. Finds VS_001 in Backlog
   2. Checks existing branches
   3. Creates/switches to feat/vs001-user-auth
   ```

**Safety Protocols for Auto-Selection:**

1. **Never Auto-Select for:**
   - Direct commits to main
   - Force push operations
   - History rewriting
   - Tag creation

2. **Always Confirm for:**
   - Hotfix branches (critical path)
   - Branch deletion
   - Merge operations
   - Release branches

3. **Fail-Safe Defaults:**
   ```
   If uncertain → feat/ (safest option)
   If multiple work items → ask user
   If no context → request clarification
   If high risk → require explicit confirmation
   ```

---

### 5. Manage Releases and Tags

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

### 6. Recover Lost Work

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

### 7. Optimize Repository

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

## Auto-Selection Edge Cases & Error Handling

### Complex Scenarios

1. **Multiple Active Work Items**
   ```
   Detected: VS_001 (40%), BF_003 (60%), TD_005 (20%)
   
   Resolution:
   - Ask user: "Which work item are these changes for?"
   - Or analyze file paths to match work item scope
   - Default to most recently updated item
   ```

2. **Mixed Change Types**
   ```
   Detected: Code + Docs + Tests + CI changes
   
   Resolution:
   - Determine primary change (most lines)
   - Use primary type for branch
   - Note: "Mixed changes detected, using [type] based on primary focus"
   ```

3. **No Backlog Context**
   ```
   Situation: No active work items found
   
   Resolution:
   - Analyze git diff for intent
   - Ask user for clarification
   - Suggest creating work item first
   ```

4. **Continuing Existing Work**
   ```
   Detected: Already on feat/user-auth branch
   
   Resolution:
   - Stay on current branch
   - Continue with commit
   - Note: "Continuing work on existing branch"
   ```

### Auto-Selection Failures

**Graceful Degradation Path:**

```
1. Try auto-selection
   ↓ (fails)
2. Provide smart suggestions based on partial data
   ↓ (user rejects)
3. Ask for explicit branch type
   ↓ (user unsure)
4. Use safe default (feat/) with descriptive name
```

**Common Failure Modes:**

1. **Can't Read Backlog**
   ```
   Fallback: Analyze git diff only
   Message: "Unable to read Backlog, analyzing changes..."
   ```

2. **Ambiguous Changes**
   ```
   Fallback: List options for user
   Message: "Changes could be [feat/fix/refactor], please specify:"
   ```

3. **No Changes Detected**
   ```
   Fallback: Prevent empty commit
   Message: "No changes detected. Nothing to commit."
   ```

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

---

## Branch Auto-Selection Quick Reference

### Work Item Type → Branch Mapping
```
HF_xxx → hotfix/xxx     (Critical production fixes)
VS_xxx → feat/xxx       (Vertical Slice features)
BF_xxx → fix/xxx        (Bug fixes)
TD_xxx → refactor/xxx   (Technical debt)
```

### File Type → Branch Mapping
```
Only *.md changed → docs/xxx
Only tests/* changed → test/xxx
Only .github/* changed → chore/ci-xxx
Mixed changes → Analyze primary focus
```

### Smart Naming Examples
```
VS_001_User_Auth → feat/vs001-user-auth
BF_003_Login_Crash → fix/bf003-login-crash
TD_010_Refactor_Services → refactor/td010-services
HF_001_Critical_Memory_Leak → hotfix/hf001-memory-leak
```

### User Override Commands
```
"use hotfix" → Forces hotfix/ prefix
"call it experimental-feature" → Uses custom name
"skip auto" → Disables auto-selection
"what branch?" → Shows auto-selection analysis
```

### Auto-Selection Decision Tree
```
Is there an active work item?
├─ YES → Use work item type
├─ NO → Check file changes
    ├─ Docs only? → docs/
    ├─ Tests only? → test/
    ├─ CI only? → chore/
    └─ Mixed/Code → feat/ (default)
```

### Safety Rules
- **NEVER** auto-select for operations on main
- **ALWAYS** confirm before creating hotfix branches
- **DEFAULT** to feat/ when uncertain
- **EXPLAIN** reasoning for every auto-selection