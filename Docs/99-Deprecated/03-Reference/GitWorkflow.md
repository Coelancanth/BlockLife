# BlockLife Git Workflow

**Last Updated**: 2025-08-21  
**Status**: Active  
**Replaces**: All previous git workflow documentation including "Sacred Sequence"

## 🎯 Core Philosophy

**Standard Git + Smart Automation = Quality Without Friction**

- Use standard git commands everyone knows
- Automate quality checks, not git operations  
- Fast local feedback, comprehensive CI validation
- One source of truth: the Backlog

## 🏗️ Architecture: Multiple Independent Clones

Each persona has its own complete repository clone with unique git identity:
- `blocklife-dev-engineer/` - dev-eng@blocklife
- `blocklife-test-specialist/` - test-spec@blocklife  
- `blocklife-debugger-expert/` - debugger@blocklife
- `blocklife-tech-lead/` - tech-lead@blocklife
- `blocklife-product-owner/` - product@blocklife
- `blocklife-devops-engineer/` - devops-eng@blocklife

## 🔧 Initial Setup (Automated via Project)

### Centralized Husky Configuration

**Key Innovation**: Husky.NET is configured at the project level and the `.husky` folder is committed to the repository. This means:
- No per-clone installation needed
- Hooks automatically activate on `dotnet tool restore`
- Consistent hooks across all persona clones
- Zero manual configuration

### First-Time Clone Process
```bash
# Clone any persona repository
git clone https://github.com/blocklife/main.git blocklife-dev
cd blocklife-dev

# Restore tools and auto-install hooks
dotnet tool restore  # This automatically installs Husky hooks!

# Verify hooks are installed
ls .git/hooks/  # Should show pre-commit, commit-msg, pre-push
```

**That's it!** No manual hook installation needed for any persona clone.

## 🌿 Branch Strategy

### Branch Types & Naming

| Type | Pattern | Example | Purpose |
|------|---------|---------|---------|
| **Main** | `main` | `main` | Production-ready code |
| **Feature** | `feat/XX_NNN-description` | `feat/VS_003-save-system` | New features (VS items) |
| **Bug Fix** | `fix/BR_NNN-description` | `fix/BR_012-race-condition` | Bug fixes |
| **Tech Debt** | `tech/TD_NNN-description` | `tech/TD_042-consolidate-archives` | Refactoring |
| **Hotfix** | `hotfix/description` | `hotfix/critical-crash` | Emergency production fixes |
| **Docs** | `docs/description` | `docs/update-readme` | Documentation only |

**Key Decision**: Use underscores to match Backlog.md format (`VS_003`, not `vs-003`)

## 📖 Standard Git Workflow (Simple & Effective)

### Starting New Work
```bash
# 1. Ensure you're up to date
git checkout main
git pull origin main

# 2. Create feature branch (use proper naming)
git checkout -b feat/VS_003-save-system

# 3. Make changes and commit (hooks validate automatically)
git add -A
git commit -m "feat(VS_003): implement auto-save mechanism"

# 4. Stay synchronized (daily minimum)
git fetch origin
git rebase origin/main

# 5. Push to remote (pre-push hook runs tests)
git push -u origin feat/VS_003-save-system

# 6. Create PR via GitHub CLI
gh pr create --title "feat(VS_003): Save System" --body "Implements auto-save as defined in Backlog.md"
```

### Syncing All Personas
```bash
# Fetch latest for all personas at once
.\scripts\persona\sync-personas.ps1

# Or fetch and pull (if no local changes)
.\scripts\persona\sync-personas.ps1 -Pull
```

## 📝 Commit Standards

### Conventional Commits Format
```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

### Types
| Type | Description | Work Item | Example |
|------|-------------|-----------|---------|
| `feat` | New feature | VS items | `feat(VS_003): add save system` |
| `fix` | Bug fix | BR items | `fix(BR_012): resolve race condition` |
| `tech` | Technical debt | TD items | `tech(TD_042): consolidate archives` |
| `docs` | Documentation | - | `docs: update README` |
| `test` | Tests | - | `test: add integration tests` |
| `perf` | Performance | - | `perf: optimize block rendering` |
| `build` | Build system | - | `build: update dependencies` |
| `ci` | CI/CD | - | `ci: add freshness check` |

### Commit Message Rules
- First line: max 72 characters
- Include work item ID when relevant
- Use present tense ("add" not "added")
- Be specific ("fix race condition" not "fix bug")

## 🪝 Git Hooks (Automated via Husky.NET)

### Hook Strategy

| Hook | Purpose | Speed | Bypass |
|------|---------|-------|---------|
| **pre-commit** | Format check | <3 sec | `--no-verify` |
| **commit-msg** | Message validation | Instant | `--no-verify` |
| **pre-push** | Build & test | <45 sec | `--no-verify` |
| **post-checkout** | Freshness check | Instant | N/A |

### Project Configuration for Automatic Hooks

#### `.config/dotnet-tools.json`
```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "husky": {
      "version": "0.6.2",
      "commands": ["husky"]
    },
    "dotnet-format": {
      "version": "5.1.250801",
      "commands": ["dotnet-format"]
    }
  }
}
```

#### `BlockLife.Core.csproj` Addition
```xml
<Target Name="husky" BeforeTargets="Restore;CollectPackageReferences" Condition="'$(HUSKY)' != 0">
  <Exec Command="dotnet tool restore" StandardOutputImportance="Low" StandardErrorImportance="High" />
  <Exec Command="dotnet husky install" StandardOutputImportance="Low" StandardErrorImportance="High" WorkingDirectory="../../" />
</Target>
```

The `.husky/` folder with all hooks is committed to the repository, so hooks are automatically installed when developers run `dotnet tool restore`.

## 🔄 Pull Request Process

### PR Branch Naming Must Match Work Item
```bash
# Correct - matches Backlog.md format
feat/VS_003-save-system
fix/BR_012-race-condition
tech/TD_042-archive-consolidation

# Wrong - will be flagged by CI
feat/vs-003-save-system  # lowercase
feature/VS_003-save      # wrong prefix
```

### PR Review Workflow
1. Developer creates PR from feature branch
2. CI runs automated checks (5-10 min)
3. Reviewer checks code and design
4. Developer addresses feedback
5. Maintainer merges using "Squash and merge"

## 🤖 CI/CD Pipeline

### GitHub Actions Checks
| Check | Timing | Blocking |
|-------|--------|----------|
| **Quick Validation** | 2 min | Yes |
| **Build & Test** | 5 min | Yes |
| **Code Quality** | 3 min | Yes (PR only) |
| **Branch Freshness** | 10 sec | Yes (if >20 commits behind) |

### Branch Protection Rules
- Require PR reviews: 1
- Dismiss stale reviews: true
- Require status checks: build, quality, freshness
- No direct push to main
- No force push allowed

## 🎯 Key Benefits of This Workflow

1. **Zero Setup** - Hooks auto-install via `dotnet tool restore`
2. **Complete Isolation** - Each persona has independent git state
3. **Self-Documenting** - Commits show persona identity
4. **Standard Git** - No custom commands or complexity
5. **Fast Feedback** - Local hooks catch issues in seconds
6. **Escape Hatches** - `--no-verify` for emergencies

## ⚡ Quick Reference

### Essential Commands
```bash
# Start work
git checkout -b feat/VS_003-description

# Commit
git commit -m "feat(VS_003): description"

# Stay updated
git rebase origin/main

# Push
git push -u origin feat/VS_003-description

# Create PR
gh pr create --fill
```

### Navigation Shortcuts
```bash
# Persona workspaces
blocklife-dev      # → Dev Engineer
blocklife-test     # → Test Specialist
blocklife-debug    # → Debugger Expert
blocklife-tech     # → Tech Lead
blocklife-product  # → Product Owner
blocklife-devops   # → DevOps Engineer

# Check all status
blocklife-status
```

## 🚫 What NOT to Do

- ❌ **Don't skip `dotnet tool restore`** - Hooks won't install
- ❌ **Don't work directly on main** - Always branch
- ❌ **Don't use old branch naming** - Use `VS_003` not `vs-003`
- ❌ **Don't commit .husky changes** - Unless updating hooks for everyone

## 🆘 Troubleshooting

### "Format check fails"
```bash
dotnet format  # Fix formatting
git add -A
git commit --amend  # Update last commit
```

### "Commit message rejected"
```bash
# Check format: type(scope): description
git commit --amend -m "feat(VS_003): proper message"
```

### "Branch too far behind"
```bash
git fetch origin
git rebase origin/main
# Fix any conflicts
git rebase --continue
```

### "Hooks not working"
```bash
# Re-install Husky
dotnet tool restore
dotnet husky install

# Verify hooks exist
ls .git/hooks/
```

### "Wrong persona committed"
```bash
# Get commit from wrong clone
cd /path/to/wrong-clone
git log -1 --format="%H"

# Cherry-pick to correct clone
cd /path/to/correct-clone
git cherry-pick <commit-sha>
```

## 🚀 Special Scenarios

### Hotfix Process
```bash
# Branch from main
git checkout main
git pull origin main
git checkout -b hotfix/critical-crash

# Fix, test, commit
git commit -m "fix: resolve critical crash"

# Push and PR directly to main
git push -u origin hotfix/critical-crash
gh pr create --base main --label hotfix
```

### Work Handoff Between Personas
```bash
# Dev pushes work
cd /path/to/dev-engineer
git push -u origin feat/VS_003-save

# Test picks up
cd /path/to/test-specialist
git fetch origin
git checkout feat/VS_003-save
```

## 📊 Health Indicators

```bash
# Check branch health
git fetch origin
echo "Behind: $(git rev-list --count HEAD..origin/main)"
echo "Ahead: $(git rev-list --count origin/main..HEAD)"
echo "Age: $(git log -1 --format=%cr)"
```

Good: PRs merge within 24 hours  
Warning: Branches >10 commits behind  
Critical: Branches >20 commits behind

## 📚 References

- [Husky.NET Documentation](https://github.com/alirezanet/Husky.Net)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Setup Script](../../scripts/persona/setup-personas.ps1)
- [ADR-002](ADR/ADR-002-persona-system-architecture.md)

---
**Remember**: Hooks auto-install via `dotnet tool restore`. No manual setup required!