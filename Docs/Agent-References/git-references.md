# Git Expert Agent - Documentation References
n## 🗺️ Quick Navigation
**START HERE**: [DOCUMENTATION_CATALOGUE.md](../DOCUMENTATION_CATALOGUE.md) - Complete index of all BlockLife documentation


## Your Domain-Specific Documentation
Location: `Docs/Agent-Specific/Git/`

- `workflow-requirements.md` - Mandatory Git workflow patterns
- `pr-guide.md` - Pull request templates and requirements
- `advanced-operations.md` - Complex Git operations and recovery

## Shared Documentation You Should Know

### 🧠 **Living Wisdom System** (For Git Context)
- **[Living-Wisdom Index](../Living-Wisdom/index.md)** - Master index to all living documents
- **[LWP_004_Production_Readiness_Checklist.md](../Living-Wisdom/Playbooks/LWP_004_Production_Readiness_Checklist.md)** - Pre-merge validation requirements
- All Living Wisdom documents are managed in Git - understand their evolution patterns

### Git Workflow Integration
- `Docs/Shared/Guides/Git_Workflow_Guide.md` - Complete Git workflow
- `Docs/Shared/Guides/Pull_Request_Guide.md` - PR creation and review process
- `.github/pull_request_template.md` - PR template requirements

### Automation Integration
- `scripts/setup_git_hooks.py` - Git workflow enforcement automation
- Git hooks prevent working on main branch
- Pre-commit and pre-push validation

### Post-Mortems for Git Insights
- `Docs/Shared/Post-Mortems/Line_Ending_CI_Conflicts_Investigation.md` - CI/Git integration lessons

## Critical Git Workflow Rules

### NEVER WORK ON MAIN BRANCH
- **Always create feature branches first**
- Branch types: `feat/`, `fix/`, `docs/`, `refactor/`, `test/`, `chore/`, `hotfix/`
- Use descriptive branch names

### Pull Request Requirements
**MUST have these sections or CI fails:**
- ✅ **## Changes** - Features Added, Bugs Fixed, Refactoring
- ✅ **## Testing** - TDD workflow, test coverage, testing instructions
- ✅ **## Checklist** - Documentation, Architecture, Code Quality, Final Checks

### Git Safety Patterns
- Use `--force-with-lease`, never `--force`
- Create backups before dangerous operations
- Use interactive rebase for history cleanup
- Tag releases with semantic versioning

## Common Git Operations

### Merge Conflict Resolution
1. Analyze conflict context and intent
2. Understand both branch histories
3. Resolve conflicts preserving both intents
4. Validate resolution with tests
5. Document resolution strategy

### History Management
- Interactive rebase for commit cleanup
- Squash related commits before PR
- Maintain clean, readable history
- Preserve semantic commit messages

### Recovery Operations
- Use `git reflog` for lost commits
- Branch recovery strategies
- Repository optimization and cleanup
- Submodule management

## Integration Points
- **DevOps Engineer**: CI/CD pipeline integration
- **Tech Lead**: Release management and tagging
- **All Agents**: Branch creation and PR workflows

## Git Configuration Standards
- Consistent line endings (autocrlf=true on Windows)
- Proper `.gitignore` patterns for C# and Godot
- Git hooks for workflow enforcement
- Commit message templates