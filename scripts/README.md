# BlockLife Automation Scripts

Organized automation tools for building, testing, git workflow, and development productivity.

📚 **Essential Reading:**
- **[Developer Guide](DEVELOPER_GUIDE.md)** - Complete usage documentation
- **[Quick Reference](QUICK_REFERENCE.md)** - Print-friendly cheat sheet  
- **[Husky Guide](HUSKY_GUIDE.md)** - Git hooks comprehensive guide

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
**Note**: Git hooks are managed by Husky.NET (installed automatically during build).  
No manual installation required - hooks are in `.husky/` directory.

### Persona System Setup
```powershell
# Set up all 6 persona clones (one-time)
./scripts/persona/setup-personas.ps1

# Sync all persona repositories (weekly or when switching between personas)
./scripts/persona/sync-personas.ps1

# Install Claude protection for main directory (optional)
./scripts/protection/install-claude-protection.ps1
```

### Branch Management Tools
```powershell
# Intelligent branch status checking (for AI persona embodiment)
./scripts/branch-status-check.ps1

# Automated cleanup for merged PRs
./scripts/branch-cleanup.ps1
./scripts/branch-cleanup.ps1 feat/VS_003-specific-branch

# Check current branch alignment with work items
source ./scripts/branch-status-check.sh    # Linux/Mac
```

## 📁 Directory Structure

### **core/** - Build System
- `build.ps1/.sh` - Build automation (build, test, clean, run)
- Complete build lifecycle management
- [📖 Documentation](core/README.md)

### **persona/** - Multi-Clone System  
- `setup-personas.ps1` - Creates 6 isolated persona clones
- `sync-personas.ps1` - Syncs all persona repositories
- Complete isolation for persona-based development
- [📖 Documentation](persona/README.md)

### **protection/** - Development Safeguards
- `install-claude-protection.ps1` - Prevents accidental main directory usage
- Encourages proper persona workspace usage

### **Root Utilities**
- `verify-subagent.ps1` - Trust-but-verify subagent work completion
- `verify-backlog-update.ps1` - Quick backlog update verification

### **deprecated/** - Archived Scripts
- Obsolete scripts from previous architectural approaches
- Maintained for recovery if needed
- [📖 Documentation](deprecated/README.md)

### **Future Expansion Areas**
- `test/`, `dev/`, `deploy/`, `utils/` - Placeholder directories for growth

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

## Developer Onboarding

### New to the project?
1. Read the **[Developer Guide](DEVELOPER_GUIDE.md)** (5-minute quick start)
2. Set up persona workspaces: `./scripts/persona/setup-personas.ps1`
3. Test the build system: `./scripts/core/build.ps1 test`
4. Keep the **[Quick Reference](QUICK_REFERENCE.md)** handy

### Adding New Scripts
When adding automation:
1. Keep it simple - avoid over-engineering  
2. Document usage clearly
3. Support both Windows and Linux when possible
4. Test locally before committing
5. Follow the existing patterns in `core/` and `persona/` directories