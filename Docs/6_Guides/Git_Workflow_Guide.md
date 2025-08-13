# Git Workflow Guide - MANDATORY for All Changes

## ⚠️ CRITICAL RULE: NEVER WORK DIRECTLY ON MAIN

**NO EXCEPTIONS**: All changes, no matter how small, MUST go through the proper branch workflow.

## Overview

This document establishes the **mandatory Git workflow** for the BlockLife project. Following this workflow ensures code quality, enables proper review, and maintains a clean project history.

## Status
- **Created**: 2025-08-13
- **Last Updated**: 2025-08-13  
- **Status**: **MANDATORY** - Must be followed by all contributors
- **Related Documents**: [Pull_Request_Guide.md](Pull_Request_Guide.md), [Comprehensive_Development_Workflow.md](Comprehensive_Development_Workflow.md)

## 🚫 What NOT to Do (Common Mistakes)

### ❌ **NEVER DO THIS:**
```bash
# Working directly on main branch
git checkout main
# Making changes...
git add .
git commit -m "some changes"
git push origin main
```

**This is FORBIDDEN and will cause:**
- No code review opportunity
- Potential breaking changes in main
- Lost history and context
- Team collaboration issues

## ✅ Proper Git Workflow

### **Step 1: Start from Updated Main**
```bash
# Ensure you're on main and it's up to date
git checkout main
git pull origin main
```

### **Step 2: Create Feature Branch**
```bash
# Create and switch to new branch
git checkout -b <branch-type>/<descriptive-name>
```

### **Step 3: Make Changes**
- Work on your feature/fix/documentation
- Make commits as needed on your branch
- Test your changes thoroughly

### **Step 4: Commit Changes**
```bash
# Stage and commit your changes
git add .
git commit -m "descriptive commit message"
```

### **Step 5: Push and Create PR**
```bash
# Push branch to remote
git push -u origin <branch-name>

# Create pull request (using GitHub CLI)
gh pr create --title "descriptive title" --body "detailed description"
```

### **Step 6: Review and Merge**
- Wait for code review and approval
- Address any review feedback
- Merge only after approval and CI checks

## Branch Naming Conventions

### **Pattern**: `<type>/<description>`

### **Branch Types:**
- **`feat/`** - New features
- **`fix/`** - Bug fixes  
- **`docs/`** - Documentation updates
- **`refactor/`** - Code refactoring
- **`test/`** - Adding or updating tests
- **`chore/`** - Maintenance tasks
- **`hotfix/`** - Critical production fixes

### **Examples:**
```bash
# Good branch names
feat/move-block-animation
fix/notification-pipeline-bug
docs/naming-conventions-update
refactor/command-handler-cleanup
test/property-based-validation
chore/dependency-updates
hotfix/critical-memory-leak

# Bad branch names
feature1
fix
my-changes
temp
test-branch
```

## Commit Message Guidelines

### **Format**: `<type>: <description>`

### **Types:**
- **`feat:`** - New feature
- **`fix:`** - Bug fix
- **`docs:`** - Documentation changes
- **`refactor:`** - Code refactoring
- **`test:`** - Adding tests
- **`chore:`** - Maintenance

### **Examples:**
```bash
feat: implement block movement with drag and drop
fix: resolve notification pipeline race condition
docs: add comprehensive Git workflow guide
refactor: extract command validation logic
test: add property tests for grid validation
chore: update dependencies to latest versions
```

## Pull Request Requirements

### **Every PR Must Include:**

1. **Clear Title**: Descriptive summary of changes
2. **Detailed Description**: What, why, and how
3. **Testing Evidence**: How changes were tested
4. **Documentation Updates**: If applicable
5. **Breaking Changes**: Clearly marked if any

### **PR Template Usage:**
Always use the repository's PR template (`.github/pull_request_template.md`)

### **Review Requirements:**
- **At least 1 approving review** before merge
- **All CI checks** must pass
- **No merge conflicts** with target branch
- **Documentation** updated if needed

## Protected Branch Rules

### **Main Branch Protection:**
- **Direct pushes**: FORBIDDEN
- **Required reviews**: 1 minimum
- **Dismiss stale reviews**: On new commits
- **Require branches up to date**: Before merge
- **Required status checks**: All CI must pass

## Emergency Procedures

### **For Critical Hotfixes:**
1. **Still use branches**: `hotfix/critical-issue-description`
2. **Expedited review**: Can be reviewed by any senior team member
3. **Fast-track merge**: After single approval and CI pass
4. **Immediate communication**: Notify team of hotfix deployment

### **NEVER bypass workflow** even for emergencies

## Workflow Enforcement

### **For AI Agents (Like Claude):**
```markdown
MANDATORY INSTRUCTIONS:
1. ALWAYS create feature branch before ANY changes
2. NEVER work directly on main branch
3. ALWAYS create PR for review
4. INCLUDE proper commit messages and PR descriptions
5. VERIFY CI passes before requesting merge
```

### **For Human Developers:**
- **Code review requirements** enforced via GitHub settings
- **Branch protection rules** prevent direct main pushes
- **CI/CD pipeline** runs on all PRs
- **Team agreement** to follow this workflow

## Common Scenarios

### **Documentation Updates:**
```bash
git checkout main
git pull origin main
git checkout -b docs/update-architecture-guide
# Make documentation changes
git add .
git commit -m "docs: update architecture guide with new patterns"
git push -u origin docs/update-architecture-guide
gh pr create --title "docs: update architecture guide" --body "Details..."
```

### **Bug Fixes:**
```bash
git checkout main
git pull origin main
git checkout -b fix/block-placement-display-issue
# Fix the bug
git add .
git commit -m "fix: resolve block placement display synchronization"
git push -u origin fix/block-placement-display-issue
gh pr create --title "fix: block placement display issue" --body "Details..."
```

### **New Features:**
```bash
git checkout main
git pull origin main
git checkout -b feat/inventory-system
# Implement feature
git add .
git commit -m "feat: implement basic inventory system with CRUD operations"
git push -u origin feat/inventory-system
gh pr create --title "feat: implement inventory system" --body "Details..."
```

## Quality Gates

### **Before Creating PR:**
- [ ] All tests pass locally
- [ ] Code follows project conventions
- [ ] Documentation updated if needed
- [ ] No debug code or commented-out sections
- [ ] Commit messages are descriptive

### **Before Merging:**
- [ ] PR approved by reviewer(s)
- [ ] All CI checks pass
- [ ] No merge conflicts
- [ ] Documentation updated
- [ ] Test coverage maintained

## Commit Squashing Guidelines

### **When to Squash Commits**

**✅ SQUASH in these scenarios:**
- **Feature completion**: Multiple work-in-progress commits for a single logical feature
- **Documentation updates**: Multiple iterative documentation commits
- **Bug fix iterations**: Several attempts/refinements to fix the same issue
- **Cleanup commits**: Commits that fix typos, formatting, or minor issues from the same PR
- **Large features**: When commit history shows development process rather than logical changes

**❌ DON'T SQUASH when:**
- **Logically distinct changes**: Each commit represents a complete, different change
- **Architectural milestones**: Major structural changes that should be preserved in history
- **Review-driven changes**: When individual commits aid in understanding the evolution
- **Rollback potential**: When you might need to revert specific parts independently

### **Squashing Methods**

#### **Method 1: Interactive Rebase (Recommended)**
```bash
# Squash last N commits (replace N with number of commits)
git rebase -i HEAD~N

# In the editor that opens:
# - Keep 'pick' for the first commit (the one to keep)
# - Change 'pick' to 'squash' (or 's') for commits to squash
# - Save and close

# Then edit the commit message in the next editor
```

#### **Method 2: Soft Reset and Recommit**
```bash
# Reset to N commits ago, keeping changes staged
git reset --soft HEAD~N

# Create new commit with all changes
git commit -m "feat: comprehensive commit message describing all changes"
```

#### **Method 3: GitHub PR Squash Merge**
```bash
# When merging PR on GitHub, select "Squash and merge"
# Edit the commit message to be descriptive
# This automatically squashes all commits in the PR
```

### **Squash Commit Message Guidelines**

**Format**: `<type>: <comprehensive description>`

**Examples of good squashed commit messages:**
```bash
feat: implement automation scripts for cognitive load reduction

- Add test metrics collector for automatic documentation updates
- Add Git workflow enforcer with pre-commit hooks
- Add documentation status sync for consistency maintenance
- Update workflow guides with automation integration
- Includes Windows Unicode compatibility fixes

🤖 Generated with [Claude Code](https://claude.ai/code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

```bash
docs: major documentation restructure with resilient action item tracking system

- Reorganize all documentation into logical categories
- Create centralized action item tracking system
- Add implementation status tracker for project visibility
- Update cross-references and navigation
- Establish maintenance protocols for documentation consistency
```

### **Squashing Workflow Integration**

#### **During Development:**
1. **Work normally**: Make commits as needed during development
2. **Before PR review**: Consider if squashing would improve clarity
3. **After review feedback**: Squash if multiple "fix review comments" commits exist
4. **Before merge**: Final opportunity to squash for clean history

#### **Team Coordination:**
- **Communicate intent**: Let reviewers know if you plan to squash
- **Preserve review context**: Don't squash until after review approval
- **Force push carefully**: Use `git push --force-with-lease` after squashing

### **Decision Framework**

**Ask yourself:**
1. **"Does each commit tell a complete story?"** → If no, consider squashing
2. **"Would a newcomer understand the changes better with fewer, more comprehensive commits?"** → If yes, squash
3. **"Are there 'WIP', 'fix typo', or 'address review' type commits?"** → Definitely squash
4. **"Does the commit history reflect the logical progression of the feature?"** → If no, squash

**Project-specific preferences:**
- **Feature branches**: Usually squashed into 1-3 logical commits
- **Documentation changes**: Usually squashed into single commit
- **Bug fixes**: Keep separate if fixing multiple unrelated issues
- **Architecture changes**: Preserve major milestones, squash implementation details

### **Example Scenarios**

#### **Scenario 1: Feature Development**
```bash
# Before squashing (development commits):
feat: add basic test metrics collector structure
feat: implement dotnet test parsing
fix: handle Windows Unicode encoding issues
feat: add documentation update functionality
docs: update CLAUDE.md with automation info
fix: typo in script comments
feat: add comprehensive error handling

# After squashing (clean history):
feat: implement test metrics automation for cognitive load reduction
```

#### **Scenario 2: Documentation Work**
```bash
# Before squashing:
docs: start Git workflow guide
docs: add branch naming conventions  
docs: add commit message guidelines
docs: add PR requirements
docs: fix formatting issues
docs: add squashing guidelines

# After squashing:
docs: create comprehensive Git workflow guide with squashing standards
```

## Recovery from Mistakes

### **If You Accidentally Work on Main:**
```bash
# If you haven't pushed yet
git branch feat/your-feature-name  # Save work to new branch
git checkout feat/your-feature-name
git checkout main
git reset --hard origin/main  # Reset main to remote state

# If you already pushed to main (EMERGENCY)
# Contact team lead immediately for assistance
```

## Tooling and Automation

### **Git Hooks (Recommended):**
```bash
# Pre-commit hook to prevent main branch commits
#!/bin/bash
branch=$(git symbolic-ref HEAD | sed -e 's,.*/\(.*\),\1,')
if [ "$branch" = "main" ]; then
  echo "❌ Direct commits to main branch are not allowed!"
  echo "Create a feature branch: git checkout -b feat/your-feature"
  exit 1
fi
```

### **GitHub CLI Setup:**
```bash
# Install GitHub CLI
# Set up authentication
gh auth login

# Configure default PR behavior
gh config set prompt disabled
```

## Monitoring and Compliance

### **Regular Audits:**
- Weekly review of commit history
- Ensure all changes went through proper workflow
- Identify and address any workflow violations

### **Team Training:**
- New team members must review this guide
- Regular refresher sessions on Git best practices
- Share examples of good vs bad workflow practices

## Related Documentation

- **[Pull_Request_Guide.md](Pull_Request_Guide.md)** - Detailed PR creation and review process
- **[Comprehensive_Development_Workflow.md](Comprehensive_Development_Workflow.md)** - TDD+VSA development process
- **[Naming_Conventions.md](../1_Architecture/Naming_Conventions.md)** - Branch and commit naming standards

## Exceptions

**There are NO exceptions to this workflow.** Every change, regardless of size or urgency, must follow the branch → PR → review → merge process.

---

**Remember**: Good Git workflow prevents bugs, enables collaboration, and maintains code quality. Following these practices protects the entire team and project.

**Last Updated**: 2025-08-13  
**Next Review**: 2025-09-13  
**Status**: **MANDATORY COMPLIANCE REQUIRED**