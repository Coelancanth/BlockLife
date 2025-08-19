# Git Workflow Automation

Git hooks, branch management, and workflow automation for BlockLife.

## Git Hooks

### Available Hooks
- **pre-commit**: Runs build+tests before each commit (prevents bad commits)
- **pre-checkout**: Validates branch naming conventions (feat/vs-XXX, fix/br-XXX, feat/td-XXX)

### Installation
```powershell
# Windows
.\git\install-hooks.ps1

# Linux/Mac  
./git/install-hooks.sh
```

### Hook Behavior

#### Pre-Checkout Hook
- Validates work item branch naming: `feat/vs-003-description`, `fix/br-006-description`, `feat/td-009-description`
- Allows standard branch prefixes: `feat/`, `fix/`, `docs/`, `chore/`, `test/`, `perf/`, `refactor/`
- Shows work item context and reminds to check backlog

#### Pre-Push Hook  
- Blocks direct pushes to main branch
- Ensures branch is up-to-date with origin/main
- Prevents merge conflicts and duplicate work
- Provides clear rebase instructions when needed

## Branch Management Guidelines

### Work Item Branches
```bash
feat/vs-042-save-system      # VS (Vertical Slice) items
fix/br-007-parallel-work     # BR (Bug Report) items  
feat/td-023-persona-system   # TD (Technical Debt) items
```

### General Purpose Branches
```bash
feat/new-feature            # New features
fix/bug-description         # Bug fixes
docs/update-readme          # Documentation  
chore/dependency-update     # Maintenance
test/integration-tests      # Testing improvements
perf/optimization-work      # Performance improvements
refactor/code-cleanup       # Refactoring
```

## Workflow Integration

These hooks integrate with:
- Local development workflow
- GitHub Actions CI pipeline  
- Code review process
- Branch protection rules

## Troubleshooting

### Bypassing Hooks (Emergency Only)
```bash
git commit --no-verify      # Skip pre-commit hooks
git push --no-verify        # Skip pre-push hooks
```

**Note**: Use `--no-verify` sparingly and only for emergency situations.