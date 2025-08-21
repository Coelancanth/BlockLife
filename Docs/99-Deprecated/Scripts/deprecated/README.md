# Deprecated Scripts Archive

**Archive Date**: 2025-08-21  
**Archived by**: DevOps Engineer  
**Reason**: Infrastructure cleanup - removing obsolete scripts that no longer match current architecture

## Archived Scripts

### `migrate-memory-bank.ps1`
- **Reason**: Obsolete after TD_053/TD_054 (Memory Bank is now local-only)
- **Issue**: References non-existent `scripts/memory-sync.ps1`
- **Replacement**: None needed - Memory Bank requires no migration with local-only architecture

### `switch-persona.ps1` 
- **Reason**: Uses worktree system replaced by multi-clone architecture
- **Issue**: Conflicts with current clone-based `setup-personas.ps1` system
- **Replacement**: Use `setup-personas.ps1` with navigation functions

### `update-git-context.ps1`
- **Reason**: TD_049 implementation artifact, work completed
- **Issue**: Not integrated with current Memory Bank local-only system
- **Replacement**: Manual activeContext.md updates when pushing (per pre-push hook reminder)

## Recovery Instructions

If any of these scripts are needed:
1. Move back to parent directory: `mv deprecated/[script-name] .`
2. Update script for current architecture if needed
3. Document the recovery in this file

## Architecture Context

These scripts were created for previous architectural approaches:
- **Memory Bank Sync Era**: When Memory Bank was tracked in git
- **Worktree Era**: When personas used git worktrees for isolation
- **Current Era**: Local-only Memory Bank + Multi-clone personas

The current architecture eliminated the complexity these scripts addressed.