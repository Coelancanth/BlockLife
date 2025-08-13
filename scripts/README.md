# 🤖 BlockLife Automation Scripts

This directory contains Python automation scripts designed to reduce cognitive load and maintain consistency across the BlockLife project's comprehensive documentation and workflow systems.

## 📋 Available Scripts

### 🧪 Test Metrics Collector (`collect_test_metrics.py`)
**Purpose**: Automatically collect test metrics and update documentation statistics  
**Reduces Cognitive Load**: Eliminates manual test counting and documentation updates

```bash
# Collect metrics and update documentation
python scripts/collect_test_metrics.py --update-docs

# Dry run to see what would be updated
python scripts/collect_test_metrics.py --update-docs --dry-run

# Just collect metrics without updating docs
python scripts/collect_test_metrics.py --verbose
```

**What it does**:
- Runs the complete TDD workflow test sequence from CLAUDE.md
- Parses dotnet test output for comprehensive metrics
- Updates `DOCUMENTATION_CATALOGUE.md` test statistics (lines 156-160)
- Updates `Implementation_Status_Tracker.md` test coverage metrics
- Provides structured logging for CI/CD integration

**Integration with Quality Gates**:
```bash
# Add to your existing test workflow
dotnet test tests/BlockLife.Core.Tests.csproj
python scripts/collect_test_metrics.py --update-docs
```

### 🚨 Git Workflow Enforcer (`enforce_git_workflow.py`)
**Purpose**: Enforce mandatory Git workflow requirements from CLAUDE.md  
**Reduces Cognitive Load**: Prevents the #1 workflow violation automatically

```bash
# Setup automatic enforcement (recommended)
python scripts/enforce_git_workflow.py --setup-hooks

# Validate current branch manually
python scripts/enforce_git_workflow.py --validate-branch

# Check all pre-commit requirements
python scripts/enforce_git_workflow.py --hook pre-commit
```

**What it enforces**:
- **CRITICAL**: Blocks commits to main branch (NEVER work on main)
- Validates branch naming conventions (feat/, fix/, docs/, etc.)
- Checks for staged changes (prevents empty commits)
- Warns if branch is behind main
- Provides clear error messages with corrective actions

**Pre-commit Hook Setup**:
Once setup, this automatically runs before every commit to enforce workflow requirements.

### 🔄 Documentation Status Sync (`sync_documentation_status.py`)
**Purpose**: Synchronize status across all documentation tracking files  
**Reduces Cognitive Load**: Maintains consistency without manual updates

```bash
# Sync all documentation status
python scripts/sync_documentation_status.py

# Check for broken documentation links
python scripts/sync_documentation_status.py --check-links

# Generate status report for CI/CD
python scripts/sync_documentation_status.py --report

# Dry run to see what would be updated
python scripts/sync_documentation_status.py --dry-run
```

**What it synchronizes**:
- Action item completion rates in `Master_Action_Items.md`
- Implementation progress in `Implementation_Status_Tracker.md`
- Documentation catalogue maintenance timestamps
- Validates internal documentation links
- Provides comprehensive status reporting

## 🚀 Quick Setup Guide

### 1. Install Dependencies
```bash
# All scripts use Python standard library - no additional dependencies required
python --version  # Ensure Python 3.7+ is available
```

### 2. Setup Git Workflow Enforcement (Recommended First Step)
```bash
# This prevents the most common workflow violation
python scripts/enforce_git_workflow.py --setup-hooks
```

### 3. Integrate with Your Workflow
Add to your quality gates sequence:
```bash
# Complete quality workflow
dotnet build BlockLife.sln
dotnet test tests/BlockLife.Core.Tests.csproj
python scripts/collect_test_metrics.py --update-docs
python scripts/sync_documentation_status.py
```

## 🎯 Integration Points

### With Existing CLAUDE.md Workflow
These scripts integrate seamlessly with the established workflow:

1. **Git Workflow**: Enforces branch creation requirements
2. **TDD Workflow**: Collects metrics from test execution
3. **Documentation**: Maintains tracking system consistency
4. **Quality Gates**: Provides automation for manual steps

### With CI/CD Pipelines
All scripts support:
- Structured logging for CI integration
- JSON status reports for automated monitoring
- Dry-run modes for validation
- Exit codes for pipeline control

### With Development Tools
- Pre-commit hooks for automatic enforcement
- Verbose logging for debugging
- Integration with existing build commands

## 📊 Benefits & Cognitive Load Reduction

### Before Automation
- ❌ Manual test counting and documentation updates
- ❌ Easy to forget Git workflow requirements
- ❌ Documentation becomes stale over time
- ❌ Inconsistent status across tracking files

### After Automation
- ✅ Test metrics automatically updated
- ✅ Git workflow violations prevented automatically  
- ✅ Documentation stays synchronized
- ✅ Consistent status across all files
- ✅ Focus on development, not maintenance

## 🔧 Configuration

### Logging
All scripts log to both console and files:
- Console: Real-time feedback
- Log files: `scripts/test_metrics.log`, `scripts/doc_sync.log`

### Customization
Scripts follow functional programming principles and can be easily extended:
- Clear separation of concerns
- Functional error handling patterns
- Configurable via command-line arguments

## 🛠️ Troubleshooting

### Common Issues

**Git workflow enforcer not working**:
```bash
# Ensure hooks are executable (Unix/Mac)
chmod +x .git/hooks/pre-commit

# Re-setup hooks if needed
python scripts/enforce_git_workflow.py --setup-hooks
```

**Test metrics collection fails**:
```bash
# Ensure you're in the project root
cd /path/to/blocklife

# Verify test project exists
ls tests/BlockLife.Core.Tests.csproj

# Run with verbose logging
python scripts/collect_test_metrics.py --verbose
```

**Documentation sync issues**:
```bash
# Check for broken links first
python scripts/sync_documentation_status.py --check-links

# Run in dry-run mode to see what would change
python scripts/sync_documentation_status.py --dry-run
```

### Getting Help
- Use `--verbose` flag for detailed logging
- Check log files in `scripts/` directory
- Ensure you're running from project root
- All scripts support `--help` for usage information

## 🔄 Maintenance

### Updating Scripts
When modifying scripts:
1. Follow functional programming patterns
2. Maintain error handling consistency
3. Update this README with new features
4. Test with `--dry-run` before deploying

### Monitoring
Scripts provide metrics for monitoring:
- Test execution success/failure
- Documentation synchronization status
- Git workflow compliance
- Link validation results

## 📚 Related Documentation

- **[CLAUDE.md](../CLAUDE.md)** - Project instructions and workflow
- **[Git_Workflow_Guide.md](../Docs/6_Guides/Git_Workflow_Guide.md)** - Detailed Git workflow requirements
- **[Comprehensive_Development_Workflow.md](../Docs/6_Guides/Comprehensive_Development_Workflow.md)** - Complete development process
- **[Master_Action_Items.md](../Docs/0_Global_Tracker/Master_Action_Items.md)** - Action item tracking
- **[Implementation_Status_Tracker.md](../Docs/0_Global_Tracker/Implementation_Status_Tracker.md)** - Implementation progress

---

> **Philosophy**: These scripts embody the BlockLife principle of reducing cognitive load through automation while maintaining the rigorous quality standards and architectural discipline that make the project successful.