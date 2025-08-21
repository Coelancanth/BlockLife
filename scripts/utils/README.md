# Utility Scripts

General-purpose utility and helper scripts.

## 🔧 Available Scripts

### validate-scripts.ps1
**Purpose**: Validate script consistency and standards compliance
```powershell
# Check all scripts for issues
./scripts/utils/validate-scripts.ps1

# Auto-fix issues where possible
./scripts/utils/validate-scripts.ps1 -Fix

# Verbose output
./scripts/utils/validate-scripts.ps1 -Verbose
```

**What it checks**:
- ✅ Shebang line (`#!/usr/bin/env pwsh`)
- ✅ Error handling (`$ErrorActionPreference = 'Stop'`)
- ✅ Naming convention (verb-noun format)
- ✅ Documentation headers (.SYNOPSIS/.DESCRIPTION)
- ✅ Cross-platform support for critical scripts

## Planned Utilities

### Cross-Platform Functions
- `common.ps1/.sh` - Shared functions for script standardization
- `colors.ps1/.sh` - Consistent color output across scripts
- `validation.ps1/.sh` - Input validation and error handling helpers

### Common Patterns
- Error handling and logging
- User input validation  
- Cross-platform path handling
- Process execution with proper error codes

## Current Status

**Status**: Placeholder - Utilities to be extracted from existing scripts

**Current Approach**: Each script handles common tasks independently

## Benefits of Shared Utilities

### Code Quality
- Consistent error handling across all scripts
- Standardized user feedback and messaging
- Reduced code duplication

### Maintainability  
- Single place to update common functionality
- Easier testing of shared logic
- Consistent behavior across automation

### Developer Experience
- Predictable script behavior
- Standardized color coding and messaging
- Common validation patterns

## Implementation Strategy

1. **Extract Common Patterns**: Identify repeated code in existing scripts
2. **Create Utility Functions**: Build reusable function libraries  
3. **Update Existing Scripts**: Refactor to use shared utilities
4. **Document Usage**: Provide clear examples for new scripts