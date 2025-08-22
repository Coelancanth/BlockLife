# ADR-002: Persona System Architecture - Multiple Clones Over Worktrees

## Status
~~Accepted (2025-08-20)~~
**Superseded by [ADR-004](ADR-004-single-repo-persona-context-management.md) (2025-01-21)**

> **Note**: The multi-clone approach proved over-engineered for solo development. ADR-004 implements a simpler single-repo solution with context management.

## Context

The BlockLife project implemented a Persona System to provide isolated development environments for different roles (Dev Engineer, Test Specialist, Debugger Expert, etc.). The initial implementation (TD_023) used Git worktrees to create separate working directories while sharing a single .git directory.

However, production usage revealed critical issues:

1. **BR_012**: Git prevents the same branch from being checked out in multiple worktrees, blocking the fundamental workflow of working on feature branches from persona-specific environments
2. **BR_013**: Git aliases (particularly Sacred Sequence commands) fail with syntax errors in worktree environments due to complex path handling
3. **Developer Friction**: Worktree commands are unfamiliar to most developers, creating a learning curve
4. **Mental Model Complexity**: The concept of a "sacred main folder" that shouldn't be touched confuses developers
5. **Terminal Restart Requirement**: Our workflow requires terminal restart for persona switching, negating worktrees' main benefit (shared .git for fast switching)

The fundamental assumption that worktrees would provide benefit was invalidated by our actual usage patterns.

## Decision

**We will migrate from Git worktrees to multiple independent clones for the Persona System.**

Each persona will have its own complete repository clone:
- `blocklife/` - Main repository (protected, rarely used directly)
- `blocklife-dev-engineer/` - Dev Engineer workspace
- `blocklife-test-specialist/` - Test Specialist workspace
- `blocklife-debugger-expert/` - Debugger Expert workspace
- `blocklife-tech-lead/` - Tech Lead workspace
- `blocklife-product-owner/` - Product Owner workspace
- `blocklife-devops-engineer/` - DevOps Engineer workspace

## Consequences

### Positive
- **Complete Isolation**: Each persona has truly independent git state, preventing cross-contamination
- **Standard Git Workflow**: Every developer already understands clones; zero learning curve
- **Branch Freedom**: Multiple clones can checkout the same branch simultaneously (solves BR_012)
- **Alias Compatibility**: Standard git config works consistently across all clones (solves BR_013)
- **Persona-Specific Config**: Each clone can have unique .gitconfig, aliases, and hooks
- **Simple Disaster Recovery**: Corrupted workspace? Just re-clone. No complex worktree repair
- **Clear Mental Model**: "Each persona owns a repository" is instantly understood
- **No Sacred Folders**: Main repo becomes just another clone, no special handling needed

### Negative
- **Disk Space**: ~250MB per clone × 6 personas = 1.5GB total (vs 250MB for worktrees)
- **Network Usage**: Initial setup downloads 6× the repository data
- **Sync Complexity**: Sharing uncommitted work between personas requires explicit coordination
- **Migration Disruption**: One-time effort to migrate existing worktree setups

### Neutral
- **Fetch Operations**: Each clone fetches independently (can be scripted for bulk operations)
- **Global Hooks**: Must be installed per clone (addressable with setup script)
- **Documentation Updates**: All references to worktrees must be updated

## Alternatives Considered

### Alternative 1: Fix Worktrees
Keep worktrees but fix the issues through tooling and scripts.
- **Pros**: 
  - No migration needed
  - Preserves disk space efficiency
  - Shared .git means single fetch updates all
- **Cons**: 
  - Cannot solve BR_012 (Git fundamental limitation)
  - Alias issues (BR_013) require complex workarounds
  - Maintains high conceptual complexity
  - Fighting against Git's design
- **Reason not chosen**: Core issues are Git limitations, not configuration problems

### Alternative 2: Single Repository with Branches
Use one repository with persona-specific branches (dev-engineer/feat-X, test-specialist/feat-X).
- **Pros**: 
  - Minimal disk usage
  - No new tooling needed
  - Simple to understand
- **Cons**: 
  - Branch explosion (6× branches for every feature)
  - Complex merging strategies
  - No actual isolation between personas
  - Git log becomes unreadable
- **Reason not chosen**: Destroys the isolation benefit that motivated the Persona System

### Alternative 3: Docker Containers per Persona
Run each persona in a Docker container with mounted volumes.
- **Pros**: 
  - Ultimate isolation
  - Consistent environments
  - Could include persona-specific tools
- **Cons**: 
  - Massive complexity increase
  - Performance overhead on Windows/macOS
  - Requires Docker knowledge
  - File permission issues with mounted volumes
- **Reason not chosen**: Over-engineering for a simple isolation need

### Alternative 4: Git Submodules
Use submodules to create persona-specific checkouts.
- **Pros**: 
  - Git-native solution
  - Some isolation benefits
- **Cons**: 
  - Submodules are notoriously complex
  - Doesn't solve branch checkout issue
  - Adds another layer of Git complexity
- **Reason not chosen**: Increases complexity rather than reducing it

## Implementation Notes

### Migration Script Structure
```powershell
# backup-and-migrate.ps1
1. Detect existing worktree setup
2. Backup any uncommitted changes
3. Create new clone directories
4. Set up persona-specific git configs
5. Install Sacred Sequence hooks in each clone
6. Verify migration success
7. Offer to remove old worktree setup
```

### Directory Structure
```
C:/Projects/
├── blocklife/                    # Main repo (protected)
├── blocklife-dev-engineer/       # Dev workspace
├── blocklife-test-specialist/    # Test workspace
├── blocklife-debugger-expert/    # Debug workspace
├── blocklife-tech-lead/          # Tech Lead workspace
├── blocklife-product-owner/      # PO workspace
└── blocklife-devops-engineer/    # DevOps workspace
```

### Sacred Sequence Integration
Each clone will have identical Sacred Sequence scripts:
- `git newbranch` - Creates branch from fresh main
- `git syncmain` - Updates current branch with main
- `git sacred` - Shows Sacred Sequence status
- `git checkfresh` - Verifies branch is current

### Persona Switching
```powershell
# Simple terminal function
function blocklife-dev {
    cd C:/Projects/blocklife-dev-engineer
    echo "📦 Dev Engineer Workspace"
}
```

### Success Metrics
1. **Branch Flexibility**: `git checkout feat/same-branch` works in all clones
2. **Sacred Sequence**: All aliases work without errors
3. **Setup Time**: New developer fully configured < 5 minutes
4. **Documentation**: Zero references to `git worktree` commands
5. **User Satisfaction**: Reduced confusion and support requests

## References
- [TD_035: Migration Task](../../01-Active/Backlog.md#td_035)
- [BR_012: Worktree Branch Conflict](../../01-Active/Backlog.md#br_012)
- [BR_013: Sacred Alias Errors](../../01-Active/Backlog.md#br_013)
- [GitWorkflow-MultiClone.md](../GitWorkflow-MultiClone.md) - Detailed workflow design
- [Git Worktree Documentation](https://git-scm.com/docs/git-worktree)
- [Original Persona System (TD_023)](../../01-Active/Backlog.md#td_023)

## Decision Rationale

The decision to migrate to multiple clones is driven by the principle of **simplicity over cleverness**. While worktrees seemed elegant in theory, they introduced complexity without delivering proportional value. 

The 1.5GB disk space cost is negligible in 2024 (less than a single node_modules folder), while the developer experience improvement is substantial. By choosing the solution that every developer already understands (clones), we eliminate an entire class of problems and reduce cognitive load.

This aligns with our core architectural principles:
- **Simplicity First**: Standard Git operations over niche commands
- **Developer Experience**: Reduce friction and learning curves
- **Pragmatic Choices**: Solve real problems, not theoretical ones
- **Clear Mental Models**: "One persona, one repo" is instantly understood

The migration effort is a one-time cost that permanently eliminates ongoing friction. This is a clear win for long-term maintainability and developer happiness.