# BlockLife Husky Git Hooks Guide

**Last Updated**: 2025-08-21  
**Maintained by**: DevOps Engineer

> **TL;DR**: Husky hooks run automatically to format code, validate commits, and ensure quality. Bypass with `--no-verify` only in emergencies.

## 🎯 What Are Husky Hooks?

Husky is a Git hook manager that runs scripts automatically at specific git events. BlockLife uses 3 hooks to maintain code quality and workflow consistency:

1. **`pre-commit`** - Runs before `git commit` (formats code)
2. **`commit-msg`** - Validates commit message format  
3. **`pre-push`** - Runs before `git push` (builds, tests, warnings)

**Location**: `.husky/` directory in project root

## 🔧 Hook Details

### **pre-commit** Hook
**Triggers**: Every `git commit`  
**Purpose**: AI persona guidance for atomic commits

**What it does:**
1. **Educational reminder** - Displays atomic commit checklist (~0.3 seconds)
2. **AI persona guidance** - Helps ensure commits represent single logical changes
3. **Left-shift quality** - Prevents bad commits rather than catching them later

**Example output:**
```bash
💡 AI Persona Reminder: Ensure Atomic Commits
   ✓ This commit does exactly ONE logical thing
   ✓ All staged files relate to the same change  
   ✓ Could be described in a single sentence
   ✓ Tests updated for this specific change only
```

**Architecture principle:** No validation overhead - purely educational. Formatting enforced by build system (TreatWarningsAsErrors=true), static analysis runs in pre-push for comprehensive validation.

### **commit-msg** Hook  
**Triggers**: Every `git commit`  
**Purpose**: Enforces conventional commit message format

**Valid formats:**
```bash
# Feature work
feat(VS_003): add save system
fix(BR_012): resolve race condition

# Technical debt
tech(TD_042): consolidate duplicate archives
tech(td_042): lowercase also accepted

# Other types
docs: update README with build instructions
test: add integration tests for drag system
perf: optimize block rendering performance
chore: update dependencies
refactor: extract common utilities
```

**Pattern**: `<type>(<scope>): <description>`
- **Types**: feat, fix, tech, docs, test, perf, build, ci, chore, refactor
- **Scope**: Optional work item (VS_003, TD_042, BR_012) or descriptive scope
- **Description**: 1-72 characters, what the commit does

**Automatically allowed:**
- Merge commits: `Merge branch 'feature/...'`
- Revert commits: `Revert "..."`

**Common failures:**
```bash
❌ "Added new feature"          # Missing type
❌ "feat: Added new feature"    # "Added" should be "add"  
❌ "feat(VS003): add save"      # Missing underscore
✅ "feat(VS_003): add save system"
✅ "feat: add save system"      # Scope optional
```

### **pre-push** Hook
**Triggers**: Every `git push`  
**Purpose**: Quality validation and helpful warnings

**Flow:**
1. **Branch Check**: Warns about direct pushes to main
2. **Smart Build**: Builds + enforces code style (EnforceCodeStyleInBuild=true)
3. **Advanced Analysis**: Extra analyzer checks beyond build system  
4. **Fast Tests**: Runs unit tests (excludes slow integration tests)
5. **Branch Staleness**: Warns if branch is >5 commits behind main
6. **Context Reminder**: Suggests updating Memory Bank if significant changes

**Example output:**
```bash
🔍 Checking for code changes...
🔨 Building solution (code changes detected)...
   (Build enforces formatting with EnforceCodeStyleInBuild=true)
🔍 Running comprehensive static analysis...
🧪 Running fast unit tests...
🔄 Checking branch freshness...
ℹ️  Branch is 3 commits behind main (consider updating)

────────────────────────────────────────────────────────
💡 CONTEXT REMINDER: Consider updating activeContext.md

  If this work is significant, update:
  .claude/memory-bank/activeContext.md
────────────────────────────────────────────────────────

✅ All checks passed - ready to push!
```

**Performance optimizations:**
- **Silent commits**: Pre-commit runs instantly with no output
- **Build-system enforcement**: Formatting caught by build system (no redundancy)
- **Smart detection**: Pre-push only runs when code actually changed (skip docs-only changes)  
- **Comprehensive analysis**: Advanced analyzers beyond build system (~10 seconds)
- **Fast tests only**: Excludes `Category=Integration&Performance`
- **Robust fallbacks**: Handles missing origin/main gracefully

## 🚨 Troubleshooting

### Pre-commit Issues

**Pre-commit hook provides guidance and rarely fails**
```bash
# If pre-commit fails (very rare), it's likely a system issue
# Check hook file for syntax errors:
cat .husky/pre-commit

# Test manually:
.husky/pre-commit

# Bypass if urgent:
git commit --no-verify
```

**Note**: Since pre-commit only displays guidance (no validation), failures are typically system-level issues (missing shell, permission problems, etc.) rather than code quality issues. The atomic commit reminders are educational only.

### Commit Message Issues

**"Invalid commit message format!"**
```bash
# Check your message against pattern
# Common fixes:
git commit -m "feat(VS_003): add user authentication"  # ✅
git commit -m "fix(BR_012): resolve null pointer error"  # ✅  
git commit -m "docs: update installation guide"  # ✅
git commit -m "tech(TD_042): refactor data layer"  # ✅
```

**Interactive rebase commit message editing**
```bash
# During rebase, follow same format rules
# Each commit message must pass validation
git rebase -i HEAD~3
# Edit each message to match pattern
```

### Pre-push Issues

**"Build failed!"**
```bash
# Fix build issues first
./scripts/core/build.ps1 test

# Or see detailed errors
dotnet build BlockLife.sln

# Bypass only for hotfixes
git push --no-verify
```

**"Code style issues found!" or "Code analysis issues found!"**
```bash
# Fix style issues automatically
dotnet format style BlockLife.sln

# Fix analyzer issues automatically (if possible)
dotnet format analyzers BlockLife.sln

# Check what specific issues exist
dotnet format style BlockLife.sln --verify-no-changes --verbosity diagnostic

# Bypass if urgent (not recommended)
git push --no-verify
```

**"Unit tests failed!"**
```bash
# Run tests with details
./scripts/core/build.ps1 test
# OR
dotnet test BlockLife.sln --verbosity normal

# Fix failing tests and retry
git push
```

**Hook timeouts or hangs**
```bash
# Tests have 30-second timeout, but build might not
# Kill hanging process: Ctrl+C
# Check for build issues:
./scripts/core/build.ps1 clean
./scripts/core/build.ps1 build

# Bypass if system issue:
git push --no-verify
```

## 🛠️ Hook Management

### Bypass Hooks (Emergency Only)
```bash
# Skip all pre-commit hooks
git commit --no-verify

# Skip all pre-push hooks  
git push --no-verify

# Skip specific hook (not possible with Husky)
# Must bypass all hooks for that event
```

### Test Hooks Manually
```bash
# Test pre-commit
.husky/pre-commit

# Test commit message (create test file)
echo "feat(VS_003): test message" > test-msg
.husky/commit-msg test-msg
rm test-msg

# Test pre-push (won't actually push)
.husky/pre-push
```

### Disable Hooks Temporarily
```bash
# Disable for repository (not recommended)
rm -rf .husky

# Disable for user (not recommended)  
git config --global core.hooksPath /dev/null

# Re-enable
git config --global --unset core.hooksPath
npm run prepare  # Reinstall hooks
```

## 🎯 Best Practices

### Working WITH the Hooks

**Commit Workflow:**
1. Write code
2. `./scripts/core/build.ps1 test` (optional, but recommended)
3. `git add -A` 
4. `git commit -m "feat(VS_003): add feature"` (formatting happens automatically)
5. `git push` (quality checks happen automatically)

**Commit Message Strategy:**
```bash
# Feature development
feat(VS_003): add user authentication system
feat(VS_003): implement login validation  
feat(VS_003): add password reset flow

# Bug fixes
fix(BR_012): resolve null pointer in login
fix(BR_012): handle empty username input

# Technical debt  
tech(TD_042): extract authentication utilities
tech(TD_042): add error handling patterns
tech(TD_042): refactor user service interface
```

### Performance Tips

**Faster commits:**
- Pre-commit is instant (no validation overhead)
- Pre-push only builds when C# files change
- Use `git commit --amend` for quick fixes (no new pre-push)

**Batch commits:**
```bash
# Multiple related changes
git add file1.cs
git commit -m "feat(VS_003): add authentication model"

git add file2.cs  
git commit -m "feat(VS_003): implement login service"

git add tests/
git commit -m "test(VS_003): add authentication tests"

# Single push runs all quality checks once
git push
```

## 🔍 Hook Debugging

### Verbose Output
```bash
# See what hooks are doing
export HUSKY=1
git commit -m "test: debug hooks"

# Or check hook files directly
cat .husky/pre-commit
cat .husky/commit-msg  
cat .husky/pre-push
```

### Common Hook Failures
```bash
# Pre-commit: Check hook file (minimal validation)
cat .husky/pre-commit
.husky/pre-commit  # Test manually

# Commit-msg: Test regex pattern
echo "feat(VS_003): test" | grep -E '^(feat|fix|tech)(\([A-Za-z]{2}_[0-9]{3}\))?: .{1,72}$'

# Pre-push: Check build system
./scripts/core/build.ps1 test
dotnet --version
```

### Hook Modification
**⚠️ Caution**: Modifying hooks affects all developers

```bash
# Edit hooks (DevOps Engineer only)
vim .husky/pre-commit
vim .husky/commit-msg
vim .husky/pre-push

# Test changes
.husky/pre-commit  # Test manually first
git commit --no-verify -m "test: hook changes"  # Bypass during testing
git push --no-verify  # Bypass during testing

# Document changes in commit
git commit -m "tech(infrastructure): improve pre-push timeout handling"
```

## 💡 Pro Tips

### Efficiency
- **Format during build**: Build system automatically enforces formatting
- **Build before pushing**: `./scripts/core/build.ps1 test` catches issues early  
- **Conventional commits**: Set up IDE snippets for common patterns
- **Batch work**: Group related commits to minimize pre-push runs

### Team Workflow
- **Hook errors are normal**: They catch issues before they reach remote
- **Don't bypass habitually**: Fix the underlying issue
- **Document bypasses**: Always explain why in commit message
- **Share patterns**: Help team learn conventional commit format

### IDE Integration
```bash
# VS Code: Add to snippets
{
  "feat commit": {
    "prefix": "gfeat",
    "body": "feat(${1:VS_XXX}): ${2:description}"
  },
  "fix commit": {
    "prefix": "gfix", 
    "body": "fix(${1:BR_XXX}): ${2:description}"
  },
  "tech commit": {
    "prefix": "gtech",
    "body": "tech(${1:TD_XXX}): ${2:description}"
  }
}
```

---

## 📋 Hook Quick Reference

| Hook | Trigger | Purpose | Bypass |
|------|---------|---------|---------|
| `pre-commit` | `git commit` | Atomic commit guidance | `git commit --no-verify` |
| `commit-msg` | `git commit` | Validate message format | `git commit --no-verify` |  
| `pre-push` | `git push` | Build, test, warnings | `git push --no-verify` |

**Remember**: Hooks exist to help maintain code quality. When they fail, they're usually catching real issues that would cause problems later.

**Need Help?** Check specific error messages, test hooks manually, or ask the DevOps Engineer persona for infrastructure questions.