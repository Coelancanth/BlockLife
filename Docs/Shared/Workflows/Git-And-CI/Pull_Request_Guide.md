# Pull Request Guide

## ⚠️ PREREQUISITE: Proper Git Workflow

**BEFORE creating ANY PR, you MUST follow the Git workflow:**

1. **Never work on main branch** - Always use feature branches
2. **Branch naming**: `<type>/<description>` (e.g., `feat/inventory-system`)
3. **Complete guide**: [Git_Workflow_Guide.md](Git_Workflow_Guide.md)

**This guide assumes you've already:**
- Created a proper feature branch
- Made your changes on that branch
- Tested your changes locally

## 🚨 CRITICAL: PR Template Compliance

**ALL Pull Requests MUST use the repository's PR template** located at `.github/pull_request_template.md`. Failure to include required sections will cause CI to fail.

## Required Sections (CI Enforced)

### ✅ **## Changes** 
Must include these subsections:
- **🎯 Features Added** - List new features or functionality
- **🐛 Bugs Fixed** - List any bugs fixed  
- **🔧 Refactoring** - Describe any refactoring done

### ✅ **## Testing**
Must include these subsections:
- **🧪 TDD Workflow Followed** - Checkboxes for RED-GREEN-REFACTOR cycle
- **📊 Test Coverage** - Number of tests added and categories
- **🔬 Testing Instructions** - Step-by-step testing commands

### ✅ **## Checklist**
Must include these subsections:
- **📚 Documentation** - Documentation updates
- **🏗️ Architecture** - Clean Architecture compliance
- **🎨 Code Quality** - Code quality standards
- **✅ Final Checks** - Ready-for-review checklist

## Common CI Failures & Solutions

### ❌ "PR description missing required sections: ## Changes, ## Testing, ## Checklist"

**Solution**: Ensure your PR body includes ALL three required sections with proper markdown headers.

**Example of correct structure**:
```markdown
## Changes
### 🎯 Features Added
- Feature 1
- Feature 2

## Testing  
### 🧪 TDD Workflow Followed
- [x] Architecture fitness tests pass

## Checklist
### 📚 Documentation
- [x] CLAUDE.md updated
```

### ❌ Missing checkboxes in required sections

**Solution**: Each subsection must have at least one checkbox item.

## Git-Commit-Writer Agent Instructions

When using the `@agent-git-commit-writer` agent to create PRs, ALWAYS provide this template-compliant format:

```bash
gh pr create --title "feat: descriptive title" --body "$(cat <<'EOF'
## 📋 PR Description

### What does this PR do?
[Clear description]

### Which issue does this PR address?
[Link or reference]

## Changes

### 🎯 Features Added
- [List features]

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
- **Tests added:** [number]
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

## Quick Reference Checklist

Before creating any PR:

1. ✅ **Template Compliance**: Use `.github/pull_request_template.md`
2. ✅ **Required Sections**: Changes, Testing, Checklist
3. ✅ **All Checkboxes**: Every required section has checkboxes
4. ✅ **Testing Commands**: Include specific `dotnet test` commands
5. ✅ **Architecture Validation**: Confirm Clean Architecture compliance
6. ✅ **Documentation Updates**: Update CLAUDE.md if adding patterns

## Agent-Specific Instructions

### For Git-Commit-Writer Agent
**ALWAYS** use the full template format shown above. Never use simplified PR descriptions that omit required sections.

### For Implementation-Planner Agent  
Include PR creation as the final step in implementation plans with template requirements noted.

### For Code-Review-Expert Agent
Validate that PR descriptions follow the template before approving architectural reviews.

## Git Workflow Violations

### ❌ **Common Violations That Will Be Rejected:**

1. **Direct commits to main** - PRs from main branch will be immediately closed
2. **Wrong branch naming** - Must follow `<type>/<description>` format
3. **No feature branch** - All changes must come from proper feature branches
4. **Merge commits in PR** - Keep branch history clean

### 🚨 **Enforcement:**
- PRs violating Git workflow will be **automatically rejected**
- Contributors must create proper feature branch and new PR
- No exceptions for "small" or "urgent" changes

### 📚 **Resources:**
- **Complete workflow**: [Git_Workflow_Guide.md](Git_Workflow_Guide.md)
- **Development process**: [Comprehensive_Development_Workflow.md](Comprehensive_Development_Workflow.md)
- **Branch naming**: See Git_Workflow_Guide.md for conventions

---

**Remember**: Both Git workflow compliance AND template compliance are enforced by CI. These are not optional - they are build gates that will block merging if not followed correctly.