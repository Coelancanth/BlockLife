# Git Workflow Requirements

*This content was delegated from CLAUDE.md to the Git Expert agent.*

## ⚠️ CRITICAL: Git Workflow Requirements

**🚫 NEVER WORK DIRECTLY ON MAIN BRANCH - NO EXCEPTIONS**

**MANDATORY Git Workflow for ALL Changes:**
1. **Create feature branch FIRST**: `git checkout -b <type>/<description>`
2. **Make changes on branch**: Never on main
3. **Create Pull Request**: Always for review
4. **Wait for approval**: Before merging
5. **Follow guide**: [Git_Workflow_Guide.md](../../Shared/Guides/Git_Workflow_Guide.md)

**Branch Types**: `feat/`, `fix/`, `docs/`, `refactor/`, `test/`, `chore/`, `hotfix/`

## Pull Request Requirements

**CRITICAL**: Always use the repository's PR template located at `.github/pull_request_template.md`

**Required Sections (will fail CI if missing):**
- ✅ **## Changes** - Features Added, Bugs Fixed, Refactoring
- ✅ **## Testing** - TDD workflow, test coverage, testing instructions  
- ✅ **## Checklist** - Documentation, Architecture, Code Quality, Final Checks

**Example gh pr create command:**
```bash
gh pr create --title "feat: descriptive feature title" --body "$(cat <<'EOF'
## 📋 PR Description

### What does this PR do?
[Clear description of changes]

### Which issue does this PR address?
[Link to issue or implementation plan]

## Changes

### 🎯 Features Added
- [List new features]

### 🐛 Bugs Fixed  
- [List bugs fixed]

### 🔧 Refactoring
- [Describe refactoring]

## Testing

### 🧪 TDD Workflow Followed
- [x] Architecture fitness tests pass
- [x] Tests written BEFORE implementation (RED phase)
- [x] Implementation done to pass tests (GREEN phase) 
- [x] Code refactored while keeping tests green (REFACTOR phase)

### 📊 Test Coverage
- **Tests added:** [Number]
- **Test categories:**
  - [x] Unit tests
  - [x] Architecture tests

### 🔬 Testing Instructions
1. Run `dotnet test --filter "FullyQualifiedName~Architecture"`
2. Run `dotnet test tests/BlockLife.Core.Tests.csproj`

## Checklist

### 📚 Documentation
- [x] CLAUDE.md updated (if adding new patterns)
- [x] Implementation plan updated (if applicable)
- [x] Code comments added where necessary
- [x] No TODO comments left in code

### 🏗️ Architecture  
- [x] Follows Clean Architecture principles
- [x] No Godot dependencies in Core project
- [x] Commands are immutable records
- [x] Handlers return `Fin<T>` for error handling
- [x] Follows vertical slice architecture

### 🎨 Code Quality
- [x] Code follows existing patterns
- [x] No compiler warnings
- [x] Meaningful variable/method names
- [x] SOLID principles followed

### ✅ Final Checks
- [x] All tests pass locally
- [x] Branch is up to date with target branch
- [x] PR has meaningful title
- [x] Ready for review

🤖 Generated with [Claude Code](https://claude.ai/code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

For complete Git workflow documentation, see [Docs/Agent-References/git-references.md](../../Agent-References/git-references.md).