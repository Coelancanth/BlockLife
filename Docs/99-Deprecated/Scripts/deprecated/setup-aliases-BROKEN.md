# setup-aliases.ps1 - BROKEN ARCHIVE NOTE

**Archived**: 2025-08-21  
**Reason**: References obsolete worktree system and non-existent scripts

## Critical Issues Found:
1. **Line 104**: Calls `switch-persona.ps1` which was archived (uses obsolete worktree system)
2. **Lines 131-133**: Uses `git worktree list` commands for system that no longer exists  
3. **Lines 169-170**: `git worktree prune` for non-existent worktrees
4. **Architecture mismatch**: Entire script assumes worktree-based personas vs current clone-based system

## What it tried to do:
- Create PowerShell aliases like `blocklife dev`, `bl-dev`, etc.
- Provide convenience functions for persona switching
- Add profile integration for persistent aliases

## Why it failed:
- Built for worktree architecture that was replaced by multi-clone system
- References scripts that were archived due to architectural changes
- Would create broken commands that don't work with current setup

## Current alternative:
The `setup-personas.ps1` script already creates convenience functions in `persona-functions.ps1`:
```powershell
function blocklife-dev {
    cd "$PSScriptRoot\blocklife-dev-engineer"
    Write-Host "📦 Dev Engineer Workspace" -ForegroundColor Cyan
}
```

## Recovery notes:
If persona aliases are needed, create new ones that work with clone system:
- Simple `cd` commands to persona directories
- No worktree references
- No calls to archived scripts

This script is a perfect example of infrastructure debt - it became obsolete when the underlying architecture changed but wasn't updated or removed.