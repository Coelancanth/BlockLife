# BlockLife Script Guide

## 📁 Script Organization & Purpose

### 🎯 Quick Reference by Task

| Task | Script | Location |
|------|--------|----------|
| **Build & Test** |
| Build project | `build.ps1 build` | `scripts/core/` |
| Run tests | `build.ps1 test` | `scripts/core/` |
| Clean build | `build.ps1 clean` | `scripts/core/` |
| **Git Workflow** |
| Check branch status | `branch-status-check.ps1` | `scripts/git/` |
| Clean merged branches | `branch-cleanup.ps1` | `scripts/git/` |
| **Persona Management** |
| Setup all personas | `setup-personas.ps1` | `scripts/persona/` |
| Sync personas | `sync-personas.ps1` | `scripts/persona/` |
| **Verification** |
| Verify subagent work | `verify-subagent.ps1` | `scripts/verification/` |
| Verify backlog updates | `verify-backlog-update.ps1` | `scripts/verification/` |
| **Setup & Protection** |
| Install main protection | `install-claude-protection.ps1` | `scripts/setup/` |

## 📂 Directory Structure (Proposed Reorganization)

```
scripts/
├── core/                    # Build & test automation
│   ├── build.ps1           # Windows build script
│   ├── build.sh            # Linux/Mac build script
│   └── README.md           # Build documentation
│
├── git/                    # Git workflow tools (NEW)
│   ├── branch-cleanup.ps1     # Clean merged branches
│   ├── branch-cleanup.sh      # (TODO: Add Linux version)
│   ├── branch-status-check.ps1 # Check PR status
│   ├── branch-status-check.sh  # Linux version
│   └── README.md              # Git tools documentation
│
├── persona/                # Multi-clone management
│   ├── setup-personas.ps1    # Create 6 persona clones
│   ├── sync-personas.ps1     # Sync all personas
│   └── README.md             # Persona documentation
│
├── setup/                  # Initial setup & configuration (NEW)
│   ├── install-claude-protection.ps1  # Main directory protection
│   ├── install-hooks.ps1     # (TODO: Manual hook installation)
│   └── README.md             # Setup documentation
│
├── verification/          # Trust-but-verify tools
│   ├── verify-subagent.ps1   # Verify AI subagent work
│   ├── verify-backlog-update.ps1 # Quick backlog checks
│   └── README.md             # Verification documentation
│
├── utils/                 # Utility scripts (future)
│   └── README.md
│
├── SCRIPT_GUIDE.md       # This file - comprehensive guide
├── QUICK_REFERENCE.md    # Print-friendly cheat sheet
└── README.md            # Main scripts documentation
```

## 🔧 Git Hooks (.husky/)

### Pre-commit Hook
**Purpose**: Ensures atomic commits and branch alignment
- Validates commit atomicity
- Checks work item alignment (VS_xxx, TD_xxx, BR_xxx)
- Verifies commit type matches branch type
- Warns about main branch commits

### Pre-push Hook  
**Purpose**: Quality gates before pushing
- **BLOCKS** direct pushes to main (requires PR)
- Runs build if code changes detected
- Executes static analysis
- Validates test coverage

### Commit-msg Hook
**Purpose**: Enforces conventional commit format
- Validates format: `type(scope): description`
- Ensures lowercase type
- Checks description length

### Post-checkout Hook
**Purpose**: Environment maintenance
- Restores NuGet packages
- Cleans stale build artifacts
- Updates git hooks if needed

### Prepare-commit-msg Hook
**Purpose**: Assists with commit message creation
- Pre-fills with branch work item
- Adds co-author for pair programming

## 🚀 Common Workflows

### Daily Development
```powershell
# 1. Check branch status
./scripts/git/branch-status-check.ps1

# 2. Build and test before committing
./scripts/core/build.ps1 test

# 3. Clean up after PR merge
./scripts/git/branch-cleanup.ps1
```

### Persona Setup (One-time)
```powershell
# Create all 6 persona clones
./scripts/persona/setup-personas.ps1

# Weekly sync of all personas
./scripts/persona/sync-personas.ps1
```

### Verification Workflow
```powershell
# After subagent completes work
./scripts/verification/verify-subagent.ps1 -Type backlog

# Quick backlog check
./scripts/verification/verify-backlog-update.ps1
```

## ⚙️ Script Standards

### All Scripts Must:
1. **Have clear headers** - Purpose, usage, examples
2. **Use consistent shebang** - `#!/usr/bin/env pwsh` for PowerShell
3. **Follow naming** - `verb-noun.ps1` format (PowerShell convention)
4. **Include error handling** - Set `$ErrorActionPreference = "Stop"`
5. **Provide help** - Support `-Help` parameter or comments
6. **Be idempotent** - Safe to run multiple times
7. **Support dry-run** - Where applicable, add `-WhatIf` support

### Cross-Platform Requirements
- Primary scripts in PowerShell (.ps1) for Windows
- Provide .sh versions for critical workflows
- Use git bash compatible commands where possible
- Test on both Windows and WSL/Linux

## 📝 Implementation Checklist

### Immediate Actions
- [x] Create this SCRIPT_GUIDE.md
- [ ] Move branch scripts to `scripts/git/`
- [ ] Move protection script to `scripts/setup/`
- [ ] Add missing .sh versions for critical scripts
- [ ] Update main README.md with new structure
- [ ] Create README.md for each subdirectory

### Future Enhancements
- [ ] Add `scripts/deploy/` for deployment automation
- [ ] Create `scripts/test/` for specialized test runners
- [ ] Add `scripts/dev/` for developer productivity tools
- [ ] Implement `scripts/utils/` for common utilities

## 🔍 Script Discovery

### For AI Personas
When embodied as a persona, use these commands to discover available scripts:
```powershell
# List all available scripts
Get-ChildItem -Path scripts -Recurse -Filter "*.ps1" | Select-Object FullName

# Find scripts by keyword
Get-ChildItem -Path scripts -Recurse -Filter "*.ps1" | Select-String "backlog"

# Get script help
Get-Help ./scripts/core/build.ps1
```

### For Developers
```bash
# Find all PowerShell scripts
find scripts -name "*.ps1" -type f

# Find all shell scripts  
find scripts -name "*.sh" -type f

# Search script content
grep -r "TODO" scripts/
```

## 🎯 Philosophy

**Keep It Simple**: Scripts should do one thing well
**Fail Fast**: Exit immediately on errors
**Be Helpful**: Provide clear error messages and suggestions
**Stay Consistent**: Follow established patterns

---
*Last Updated: 2025-08-22*
*Maintained by: DevOps Engineer Persona*