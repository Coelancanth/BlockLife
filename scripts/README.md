# BlockLife Automation Scripts

Organized automation tools for building, testing, git workflow, and development productivity.

## 🚀 Quick Start

### Essential Build Commands
```powershell
# Windows
./scripts/core/build.ps1 test    # Build + run tests (safe default)
./scripts/core/build.ps1 build   # Build only
./scripts/core/build.ps1 all     # Clean, build, and test

# Linux/Mac
./scripts/core/build.sh test     # Build + run tests (safe default)
./scripts/core/build.sh build    # Build only
./scripts/core/build.sh all      # Clean, build, and test
```

### Git Workflow Setup
```powershell
# Windows
./scripts/git/install-hooks.ps1   # Install git hooks

# Linux/Mac
./scripts/git/install-hooks.sh    # Install git hooks
```

### Verification Scripts (NEW)
```powershell
# Verify subagent work completion
./scripts/verify-subagent.ps1 -Type backlog
./scripts/verify-subagent.ps1 -Type general -CheckFiles "*.cs"

# Quick backlog update verification
./scripts/verify-backlog-update.ps1
./scripts/verify-backlog-update.ps1 -ItemNumber "TD_041"
```

## 📁 Directory Structure

### **core/** - Essential Operations
- `build.ps1/.sh` - Build automation (build, test, clean, run)
- Complete build lifecycle management
- [📖 Documentation](core/README.md)

### **git/** - Git Workflow  
- `hooks/` - Pre-commit build validation, pre-checkout naming
- `install-hooks.ps1/.sh` - Hook installation
- Branch naming and workflow enforcement
- [📖 Documentation](git/README.md)

### **test/** - Testing Automation *(Future)*
- Comprehensive test execution and reporting
- Code coverage and performance benchmarking
- [📖 Documentation](test/README.md)

### **dev/** - Development Utilities *(Future)*
- Environment setup and health checks
- Multi-persona development tools (TD_023)
- [📖 Documentation](dev/README.md)

### **deploy/** - Deployment Automation *(Future)*
- Game packaging and release management
- Multi-platform deployment
- [📖 Documentation](deploy/README.md)

### **utils/** - Shared Utilities *(Future)*
- Common functions and cross-platform helpers
- Standardized error handling and validation
- [📖 Documentation](utils/README.md)

## CI/CD Integration

These scripts are used by:
- GitHub Actions CI pipeline (`.github/workflows/ci.yml`)
- Local development workflow
- Pre-commit testing

## Design Principles

1. **Simple** - No complex dependency management
2. **Fast** - Direct dotnet commands, no overhead
3. **Portable** - Works on Windows, Linux, and Mac
4. **Focused** - Does one thing well

## Adding New Scripts

When adding automation:
1. Keep it simple - avoid over-engineering
2. Document usage clearly
3. Support both Windows and Linux when possible
4. Test locally before committing