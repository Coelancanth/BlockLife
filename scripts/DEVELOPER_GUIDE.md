# BlockLife Scripts Developer Guide

**Last Updated**: 2025-08-21  
**Maintained by**: DevOps Engineer

> **TL;DR**: Use `./scripts/core/build.ps1 test` before committing. Use persona clones for development.

## 🚀 Quick Start (5 Minutes to Productivity)

### 1. Essential Build Commands
```bash
# Safe default before any commit (builds + tests)
./scripts/core/build.ps1 test        # Windows
./scripts/core/build.sh test         # Linux/Mac

# Just build (faster iteration)
./scripts/core/build.ps1 build       # Windows  
./scripts/core/build.sh build        # Linux/Mac
```

### 2. Set Up Persona Workspaces (One-time)
```bash
# Creates 6 isolated persona clones
./scripts/persona/setup-personas.ps1

# Navigate between personas
cd ../blocklife-dev-engineer     # Dev work
cd ../blocklife-tech-lead        # Architecture decisions
cd ../blocklife-test-specialist  # Testing focus
```

### 3. Quality Gates (Automatic)
- **Pre-commit**: Instant validation (~0.3s), defers formatting to build system
- **Pre-push**: Builds + enforces formatting, advanced analysis, tests
- **Architecture**: AI generates clean code → build system enforces → no redundant formatting
- **Bypass if needed**: `git commit --no-verify` or `git push --no-verify`
- **Deep dive**: See **[Husky Guide](HUSKY_GUIDE.md)** for complete hook documentation

## 🎯 Core Workflows

### Daily Development Flow
```bash
# Start with persona workspace
cd ../blocklife-dev-engineer

# Work on features...
# Code, commit, repeat

# Before pushing (safety check)
./scripts/core/build.ps1 test

# Push when ready
git push
```

### Build System Deep Dive
```bash
# Available commands
./scripts/core/build.ps1 build      # Build only (fastest)
./scripts/core/build.ps1 test       # Build + tests (safe default) ⭐
./scripts/core/build.ps1 test-only  # Tests only (dev iteration)
./scripts/core/build.ps1 clean      # Clean build artifacts
./scripts/core/build.ps1 run        # Run game (requires Godot)
./scripts/core/build.ps1 all        # Clean + build + test
```

**Critical**: Use `test` (not `test-only`) before committing - it catches Godot compilation issues.

### Persona System Mastery

#### Initial Setup
```bash
# Run once to set up all 6 personas
./scripts/persona/setup-personas.ps1

# Creates these directories (in parent folder):
# blocklife-dev-engineer/
# blocklife-tech-lead/
# blocklife-product-owner/
# blocklife-test-specialist/
# blocklife-debugger-expert/
# blocklife-devops-engineer/
```

#### Daily Usage
```bash
# Sync all personas with remote
./scripts/persona/sync-personas.ps1

# Work in specific persona
cd ../blocklife-dev-engineer
# Your work happens here...

# Switch personas
cd ../blocklife-tech-lead
# Architecture decisions here...
```

## 🛡️ Quality & Safety Features

### Git Hooks (Automatic)
- **Pre-commit**: 
  - Auto-formats code with `dotnet format`
  - Validates commit message format
  - Stages formatting changes automatically

- **Pre-push**:
  - Builds solution (only if code changed)  
  - Runs fast unit tests (excludes slow integration tests)
  - Warns about stale branches (>5 commits behind)
  - Provides Memory Bank context reminder

### Protection Systems
```bash
# Install Claude protection (prevents main directory usage)
./scripts/protection/install-claude-protection.ps1

# Uninstall if needed
./scripts/protection/install-claude-protection.ps1 -Uninstall
```

### Verification Tools
```bash
# Verify subagent work completion  
./scripts/verify-subagent.ps1 -Type backlog
./scripts/verify-subagent.ps1 -Type general -CheckFiles "*.cs"

# Quick backlog verification
./scripts/verify-backlog-update.ps1
./scripts/verify-backlog-update.ps1 -ItemNumber "TD_042"
```

## 🚨 Troubleshooting

### Common Issues

**"Hook failed" errors:**
```bash
# Usually formatting or build issues
git status                    # See what's modified
./scripts/core/build.ps1 test # Fix build/test issues
git add -A && git commit      # Retry commit

# Emergency bypass (use sparingly)
git commit --no-verify
```

**"Build failed" in pre-push:**
```bash
# Fix the build first
./scripts/core/build.ps1 build

# Then retry push
git push

# Or bypass for urgent fixes
git push --no-verify
```

**Persona setup issues:**
```bash
# Check if git is available
git --version

# Verify repo URL in setup-personas.ps1
# Update line 39 with correct repository URL

# Clean retry with skip existing
./scripts/persona/setup-personas.ps1 -SkipExisting
```

### Hook Debugging
```bash
# Test hooks manually
.husky/pre-commit              # Test pre-commit hook
.husky/pre-push                # Test pre-push hook (won't actually push)

# Check Husky installation
npx husky                      # Should show help
```

### Build Debugging  
```bash
# Verbose output
./scripts/core/build.ps1 build
dotnet build BlockLife.sln --verbosity normal

# Clean start
./scripts/core/build.ps1 clean
./scripts/core/build.ps1 build
```

## 📋 Script Reference

### Core Scripts
- `build.ps1/.sh` - Main build system
- Location: `scripts/core/`
- Commands: build, test, test-only, clean, run, all

### Persona Scripts
- `setup-personas.ps1` - One-time setup of 6 persona clones
- `sync-personas.ps1` - Sync all personas with remote
- Location: `scripts/persona/`

### Verification Scripts  
- `verify-subagent.ps1` - Trust-but-verify subagent work
- `verify-backlog-update.ps1` - Quick backlog verification
- Location: `scripts/` (root)

### Protection Scripts
- `install-claude-protection.ps1` - Workspace protection system
- Location: `scripts/protection/`

## 🎯 Best Practices

### Development Flow
1. **Use persona workspaces** - isolated, conflict-free development
2. **Test before commit** - `./scripts/core/build.ps1 test`  
3. **Trust the hooks** - they catch issues automatically
4. **Sync regularly** - `./scripts/persona/sync-personas.ps1`

### Build Commands
- **Daily development**: Use `build` for fast iteration
- **Before commit**: Always use `test` (catches Godot issues)
- **Never use**: `test-only` before commits (doesn't validate Godot)

### Git Workflow
- **Let hooks work** - they format and validate automatically  
- **Bypass sparingly** - `--no-verify` only for emergencies
- **Update regularly** - hooks warn about stale branches

### Persona Usage
- **One persona per workspace** - don't mix concerns
- **Context switching** - each persona has specific focus
- **Clean handoffs** - use Memory Bank activeContext.md

## 💡 Pro Tips

### Efficiency
```bash
# Alias for common commands (add to your shell profile)
alias bt='./scripts/core/build.ps1 test'
alias bb='./scripts/core/build.ps1 build'
alias sync='./scripts/persona/sync-personas.ps1'
```

### Workflow Optimization
- **Hot reloading**: Use `build` command during development  
- **Commit prep**: Use `test` command before committing
- **Persona sync**: Run weekly or when switching major work items
- **Hook bypass**: Only use `--no-verify` for urgent hotfixes

### Memory Bank Integration
- Update `activeContext.md` when hooks remind you
- Focus on significant context (not every small change)
- Use for persona handoffs and complex debugging sessions

---

**Need Help?** Check the specific README files in each subdirectory or ask the DevOps Engineer persona for infrastructure questions.