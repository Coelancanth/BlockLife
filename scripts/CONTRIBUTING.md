# Contributing to BlockLife Scripts

Guidelines for adding and maintaining automation scripts in the BlockLife project.

## 🏗️ Directory Organization

### Where to Add New Scripts

| Purpose | Directory | Examples |
|---------|-----------|----------|
| Build, clean, run operations | `core/` | build, clean, test execution |
| Git workflow and hooks | `git/` | branch utilities, hook management |
| Test execution and reporting | `test/` | test runners, coverage, performance |
| Development utilities | `dev/` | environment setup, persona tools |
| Packaging and releases | `deploy/` | packaging, release automation |
| Shared functions | `utils/` | common functions, validation |

### Naming Conventions

#### Cross-Platform Scripts
- **PowerShell**: `script-name.ps1` (Windows)
- **Bash**: `script-name.sh` (Linux/Mac)  
- **Python**: `script-name.py` (when shell scripts insufficient)

#### File Organization
- One script per logical function
- Matching functionality between Windows/Linux versions
- Clear, descriptive names with hyphens (not underscores)

## 📝 Script Standards

### Required Elements

#### 1. Header Comments
```bash
#!/bin/bash
# Script Name: Brief description
# Purpose: Detailed explanation of what this does
# Usage: ./script-name.sh [parameters]
```

#### 2. Error Handling
```bash
set -e  # Exit on error (bash)
# OR
$ErrorActionPreference = "Stop"  # PowerShell
```

#### 3. User Feedback
- Use consistent color coding (see `utils/colors.*` when available)
- Provide clear progress indicators
- Show meaningful error messages with resolution guidance

#### 4. Help/Usage Information
```bash
if [[ "$1" == "--help" || "$1" == "-h" ]]; then
    echo "Usage: $0 [parameters]"
    echo "Description: What this script does"
    exit 0
fi
```

### Code Quality Guidelines

#### Cross-Platform Compatibility
- Test on both Windows (Git Bash/PowerShell) and Linux
- Use relative paths that work on both platforms
- Handle path separators correctly
- Use appropriate shebang lines

#### Error Recovery
- Check for required dependencies before execution
- Provide clear error messages with resolution steps
- Exit with appropriate error codes (0 = success, 1+ = error)
- Clean up temporary files on failure

#### Performance
- Avoid unnecessary operations
- Use native commands where possible
- Cache expensive operations when beneficial
- Provide progress feedback for long-running operations

## 🧪 Testing Requirements

### Before Committing Scripts

1. **Functionality Testing**
   ```bash
   # Test normal operation
   ./your-script.sh normal-params
   
   # Test error conditions  
   ./your-script.sh invalid-params
   
   # Test help display
   ./your-script.sh --help
   ```

2. **Cross-Platform Testing**
   - Test on Windows (PowerShell + Git Bash)
   - Test on Linux/Mac if possible
   - Verify path handling works correctly

3. **Integration Testing**
   - Ensure script works from project root
   - Test integration with existing CI/CD
   - Verify no conflicts with other scripts

### Adding Tests

For complex scripts, consider adding:
- Unit tests for individual functions
- Integration tests for full workflow
- Error condition tests

## 📚 Documentation Requirements

### Script Documentation

#### In-Script Documentation
- Header with purpose and usage
- Complex logic sections commented
- Parameter validation with clear error messages
- Example usage in help text

#### README Updates  
When adding scripts to a category:
1. Update the category's `README.md`
2. Add usage examples
3. Document any dependencies or requirements
4. Update main `scripts/README.md` if needed

### Integration Documentation
- Update relevant workflow documentation
- Add to CI/CD pipeline if applicable
- Document any new dependencies

## 🔧 Common Patterns

### Parameter Handling
```bash
# Bash
command=${1:-default}
if [[ -z "$command" ]]; then
    echo "Usage: $0 <command>"
    exit 1
fi

# PowerShell  
param([string]$Command = "default")
if ([string]::IsNullOrEmpty($Command)) {
    Write-Host "Usage: script.ps1 -Command <value>"
    exit 1
}
```

### Progress Feedback
```bash
# Use consistent messaging patterns
echo "🔧 Starting operation..."
echo "✅ Operation completed successfully"
echo "❌ Operation failed: reason"
```

### Error Handling
```bash
# Bash
if ! command_that_might_fail; then
    echo "❌ Command failed"
    exit 1
fi

# PowerShell
try {
    Command-That-Might-Fail
} catch {
    Write-Host "❌ Command failed: $($_.Exception.Message)"
    exit 1
}
```

## 🚀 Deployment Guidelines

### Before Merging

1. **Code Review**
   - Functionality review
   - Security review (no secrets, safe operations)
   - Performance review
   - Documentation review

2. **Testing Validation**
   - All tests pass
   - Cross-platform compatibility verified
   - Integration with existing workflows confirmed

3. **Documentation Updates**
   - All relevant documentation updated
   - Examples provided and tested
   - Dependencies documented

### After Merging

1. **Update CI/CD** (if applicable)
   - Add to GitHub Actions if needed
   - Update workflow documentation
   - Test in CI environment

2. **User Communication**
   - Update team on new capabilities
   - Provide usage examples
   - Document any breaking changes

## 💡 Best Practices

### Script Design
- **Single Responsibility**: Each script does one thing well
- **Composability**: Scripts work together cleanly
- **Idempotency**: Safe to run multiple times
- **Validation**: Check inputs and environment before executing

### User Experience
- **Discoverability**: Clear naming and documentation
- **Predictability**: Consistent behavior and output
- **Reliability**: Handle edge cases gracefully
- **Performance**: Fast execution and clear progress

### Maintenance
- **Simplicity**: Prefer simple solutions over complex ones
- **Standards**: Follow established patterns
- **Dependencies**: Minimize external dependencies
- **Evolution**: Design for future enhancement

---

**Questions?** Consult the DevOps Engineer or review existing scripts in `core/` and `git/` for examples of good practices.