# ADR-004: Single-Repo Persona Context Management

## Status
Enhanced (2025-08-22) - v3.0 Auto-Sync

## Context

ADR-002 implemented a multi-clone architecture for the Persona System, creating 6 separate repository clones (one per persona). This was designed to provide isolation between personas and support parallel development.

However, real-world usage revealed this was over-engineered for solo development:

1. **Unnecessary Complexity**: Managing 6 repositories for one developer creates cognitive overhead
2. **Sync Burden**: Constant need to sync changes between repos
3. **Disk Waste**: 1.5GB for what's essentially one developer's work
4. **Workflow Mismatch**: Solo dev inherently works sequentially, not in parallel
5. **Context Loss**: Switching between repos loses Claude's context anyway

The fundamental realization: **We were solving a multi-developer problem for a solo developer.**

Claude's stateless nature between conversations means context must be preserved through files regardless of the repository structure. The `/clear` command already provides the clean slate we need between personas.

## Decision

**We will use a single repository with context management through the Memory Bank system.**

Key architectural decisions:

1. **Single Repository**: One clone, one .git, one set of hooks
2. **Context Preservation**: Memory Bank files maintain state between `/clear` commands
3. **Sequential Workflow**: Embrace the natural solo dev pattern
4. **Automatic Sync**: Pull latest on every embodiment
5. **Session Logging**: Immutable audit trail of all persona actions

### Context Management Structure (Refined)

```
.claude/
├── memory-bank/
│   ├── active/                   # Persona-specific working memory
│   │   ├── dev-engineer.md       # "Currently implementing VS_003A validation"
│   │   ├── tech-lead.md          # "Reviewing TD_014 complexity score"
│   │   ├── test-specialist.md    # "Designing test strategy for..."
│   │   ├── debugger-expert.md    # "Investigating race condition in..."
│   │   ├── product-owner.md      # "Refining VS_004 requirements"
│   │   └── devops-engineer.md    # "Optimizing CI pipeline stage 3"
│   └── session-log.md            # Chronological history (append-only)

Docs/01-Active/
└── Backlog.md                    # THE shared context (decisions, statuses)
```

**Key Insight**: The Backlog IS our shared context - no need to duplicate information in separate files.

### Embodiment Protocol

When running `./scripts/persona/embody.ps1 [persona]`:

1. **Pull Latest**: `git pull origin main --ff-only` (enforced)
2. **Check State**: Warn about uncommitted changes (not blocking)
3. **Set Identity**: Configure git identity for the persona
4. **Load Context**: Display active/[persona].md content
5. **Show Backlog**: Display items owned by this persona

When Claude embodies a persona:

1. **Run Command**: Execute `./scripts/persona/embody.ps1 [persona]`
2. **Load Contexts**: Read active/[persona].md and check Backlog.md
3. **Present State**: Show current work and await direction

### Auto-Sync Enhancement (v3.0 - 2025-08-22)

The embody.ps1 script was enhanced to automatically handle common git synchronization scenarios, removing friction from the persona workflow:

**Auto-Sync Capabilities**:
1. **Automatic Stashing**: Detects and stashes uncommitted changes before sync
2. **Smart Rebase**: When branches diverge, automatically rebases local commits
3. **Duplicate Detection**: Drops commits already merged upstream
4. **Auto-Push**: Pushes unpushed commits when safe
5. **Conflict Recovery**: Gracefully aborts on conflicts, restores state, provides options

**Sync Flow Logic**:
```
Uncommitted changes? → Stash
Can fast-forward? → Pull
Branches diverged? → Rebase (drops duplicates)
Only local ahead? → Auto-push
Only remote ahead? → Pull
Rebase conflicts? → Abort, restore, show manual options
Success? → Restore stashed changes
```

This enhancement makes persona switching truly friction-free. The script handles 95% of sync scenarios automatically while preserving safety for complex situations requiring manual intervention.

### Conflict Prevention Strategy

**Core Principle**: Frequent commits prevent conflicts better than complex stashing.

1. **Commit Often**: Every 20-30 minutes or at natural boundaries
2. **Push Regularly**: After every 2-3 commits
3. **PR Strategically**: Only for complete features/tasks (not every push)

The script encourages but doesn't enforce commits, treating developers as responsible adults who understand their workflow.

## Consequences

### Positive

- **Radical Simplicity**: One repo, one truth, one workflow
- **Natural Git Flow**: Standard commands, no special knowledge needed
- **Zero Sync Issues**: No multi-repo coordination required
- **Context Preserved**: Memory Bank maintains continuity
- **Conflict Prevention**: Sequential work naturally avoids conflicts
- **Instant Switching**: `/clear` + `embody` takes seconds
- **Audit Trail**: Complete history through session logs
- **Disk Efficient**: 250MB instead of 1.5GB

### Negative

- **No True Parallelism**: Can't have multiple personas working simultaneously
  - *Mitigation*: Solo dev can't actually work in parallel anyway
- **Requires Discipline**: Should commit frequently to prevent conflicts
  - *Mitigation*: Natural boundaries encourage commits, script provides reminders
- **Single Point of Failure**: One corrupted repo affects all work
  - *Mitigation*: Frequent pushes to remote, standard git recovery

### Neutral

- **Git Identity Switching**: Must update config per persona
- **Mental Model Shift**: From "repos as personas" to "context as personas"
- **Migration Effort**: One-time cleanup of multi-clone setup

## Alternatives Considered

### Alternative 1: Keep Multi-Clone Architecture (ADR-002)
- **Pros**: Already implemented, true isolation
- **Cons**: Over-engineered for solo dev, sync complexity, disk waste
- **Reason not chosen**: Solves a problem that doesn't exist

### Alternative 2: Git Worktrees (Original Attempt)
- **Pros**: Shared .git, lighter than clones
- **Cons**: Can't checkout same branch, complex commands, alias issues
- **Reason not chosen**: Already failed in practice

### Alternative 3: Branch-Based Personas
- **Pros**: Minimal setup
- **Cons**: Branch explosion, no real isolation, messy history
- **Reason not chosen**: Loses the persona boundary benefits

### Alternative 4: No Persona System
- **Pros**: Ultimate simplicity
- **Cons**: Loses role-based thinking, no context separation
- **Reason not chosen**: Personas provide valuable mental models

## Pull Request Strategy for Solo Dev

Unlike team development where PRs gate code review, solo dev PRs serve as meaningful milestones in the development journey.

### When to Create PRs

1. **Feature Complete**: VS item fully implemented
2. **Task Complete**: TD item resolved
3. **Logical Milestone**: Significant unit of work done

### When NOT to Create PRs

1. **WIP Commits**: Frequent saves for conflict prevention
2. **Minor Fixes**: Typos, small documentation updates
3. **Intermediate Progress**: Partial implementations

### PR Workflow

```bash
# Work on feature branch
git checkout -b feat/VS_003A-block-matching

# Multiple frequent commits (conflict prevention)
git commit -am "feat: add matcher interface"
git push origin feat/VS_003A-block-matching

# ... more commits throughout implementation ...

# Feature complete - create PR
gh pr create --title "feat(VS_003A): Block Matching System"

# Squash merge to keep main clean
gh pr merge --squash --delete-branch
```

This approach provides:
- Conflict prevention through frequent commits
- Clean main branch history through squash merges
- Meaningful checkpoints through PRs
- Simple workflow without bureaucracy

## Implementation

### Phase 1: Context Structure
```bash
# Create Memory Bank structure
mkdir -p .claude/memory-bank/active

# Initialize persona-specific active contexts
for persona in dev-engineer tech-lead test-specialist debugger-expert product-owner devops-engineer; do
    echo "# $persona Active Context" > .claude/memory-bank/active/$persona.md
    echo "Last updated: $(date)" >> .claude/memory-bank/active/$persona.md
done

# Create session log
echo "# BlockLife Session Log" > .claude/memory-bank/session-log.md
echo "Chronological record of all persona work" >> .claude/memory-bank/session-log.md
```

### Phase 2: Embodiment Script
```powershell
# scripts/persona/embody.ps1
param([string]$persona)

# 1. Sync with latest (enforced)
git pull origin main --ff-only
if ($LASTEXITCODE -ne 0) {
    Write-Error "Cannot sync - resolve conflicts first"
    exit 1
}

# 2. Check for uncommitted work (advisory only)
if (git status --porcelain) {
    Write-Warning "You have uncommitted changes"
    Write-Host "Consider committing if work is complete"
    Write-Host "Tip: Frequent commits prevent conflicts!"
    # Continue anyway - not blocking
}

# 3. Set git identity
git config user.name "$persona"
git config user.email "$persona@blocklife"

# 4. Load and display context
$contextPath = ".claude/memory-bank/active/$persona.md"
if (Test-Path $contextPath) {
    Get-Content $contextPath | Select-Object -First 10
}

# 5. Show owned backlog items
Select-String -Path "Docs/01-Active/Backlog.md" -Pattern "Owner:.*$persona"

Write-Host "✅ Ready as $persona - remember to commit frequently!"
```

### Phase 3: Update Persona Docs
Each persona document gets updated with:
```markdown
## 🚀 Embodiment Command
When embodied, run: `git pull origin main --ff-only && git status`
```

### Phase 4: Migration from Multi-Clone
```bash
# Archive old repos (don't delete yet)
mv blocklife-* archived-personas/

# Update any scripts referencing old paths
# Update documentation
# Communicate change to user
```

## Success Metrics

1. **Switching Speed**: Persona switch < 10 seconds
2. **Context Preservation**: No lost work between sessions
3. **Conflict Rate**: Near-zero merge conflicts
4. **User Satisfaction**: Reduced complexity complaints
5. **Git Hygiene**: Clean, linear history maintained

## Decision Rationale

This decision embraces the reality of solo development rather than fighting it. The multi-clone architecture (ADR-002) was a well-intentioned but ultimately unnecessary complexity for a single developer.

Key insights driving this decision:

1. **Claude is stateless**: Context must be preserved in files regardless of repo structure
2. **Humans are sequential**: Solo devs can't truly parallelize anyway
3. **Simplicity scales down**: Enterprise patterns don't fit solo projects
4. **Conflicts are communication**: They reveal design issues, not workflow problems
5. **Backlog is shared context**: No need to duplicate information already tracked
6. **Frequent commits prevent conflicts**: Better than complex stashing strategies
7. **PRs mark milestones**: Not every push needs review in solo dev

By aligning our architecture with how solo development actually works, we eliminate entire categories of problems while preserving all the benefits of the persona system.

## References

- [ADR-002](ADR-002-persona-system-architecture.md) - Superseded multi-clone approach
- [ADR-003](ADR-003-ai-persona-git-workflow.md) - Git workflow decisions (still active)
- [HANDBOOK.md](../HANDBOOK.md) - Technical reference
- [Memory Bank System](../../.claude/memory-bank/) - Context preservation implementation

## Quote

> "Perfection is achieved not when there is nothing more to add, but when there is nothing left to take away." - Antoine de Saint-Exupéry

This ADR embodies that principle: removing unnecessary complexity to reveal the simple, elegant solution that was always there.



  📖 Complete Persona Workflow

  The Full Flow Diagram

  Start Fresh Day
        ↓
  [1. CLAUDE] → "embody tech-lead" (runs embody.ps1)
        ↓
  [2. WORK] → Review, Decide, Commit frequently
        ↓
  [3. SWITCH] → /clear → "embody dev-engineer" (runs embody.ps1)
        ↓
  [4. LOOP] → Back to step 2 with new persona

  Detailed Step-by-Step Workflow

  ---
  🌅 PHASE 1: Starting Your Day

  Step 1: Check Current State

  # See where you left off
  git status
  git log --oneline -5

  # Check which persona was last active
  git config user.name

  Step 2: Start Claude Session with Chosen Persona

  # In Claude, simply type:
  embody tech-lead

  Claude automatically:
  - Executes: ./scripts/persona/embody.ps1 tech-lead
  - Which handles:
    - ✅ Pulls latest from main (with auto-sync v3.0)
    - 📝 Stashes/restores uncommitted work if needed
    - 🔧 Sets git identity to Tech Lead
    - 📚 Shows .claude/memory-bank/active/tech-lead.md
    - 📜 Shows recent session log entries
    - 📋 Lists backlog items assigned to Tech Lead
  - Reads persona definition from Docs/04-Personas/tech-lead.md
  - Loads active context and backlog
  - Presents current state and awaits direction

  ---
  💼 PHASE 2: Working as Persona

  The Work Cycle (Every 20-30 minutes)

  Mini-cycle 1: Small Task

  # Tech Lead reviews TD_014
  # Makes decision
  git add Docs/01-Active/Backlog.md
  git commit -m "decision: approve TD_014 with complexity score 3"

  Mini-cycle 2: Continue Work

  # Reviews VS_003
  # Updates documentation
  git add Docs/03-Reference/ADR/ADR-005.md
  git commit -m "docs: add ADR-005 for validation strategy"

  Mini-cycle 3: Push Progress

  # After 2-3 commits
  git push origin main

  Updating Context During Work

  # Claude updates active/tech-lead.md with:
  - Current decisions in progress
  - Blockers encountered
  - Next items to review

  ---
  🔄 PHASE 3: Switching Personas

  Step 1: Finish Current Work

  As current persona (e.g., Tech Lead):
  - Commit any valuable work: git commit -m "decision: approved TD_014"
  - Update active context: .claude/memory-bank/active/tech-lead.md
  - Add session log entry:
    ### 10:45 - Tech Lead
    **Did**: Approved TD_014 with complexity score 3
    **Next**: Ready for Dev Engineer implementation
    **Note**: Pattern framework approach recommended

  Step 2: Switch in Claude

  # Clear Claude's context and embody new persona
  /clear
  embody dev-engineer

  Claude automatically:
  - Executes: ./scripts/persona/embody.ps1 dev-engineer
  - Which handles:
    - Auto-sync with main (stash/pull/rebase as needed)
    - Updates git identity to Dev Engineer
    - Shows dev-engineer active context
    - Lists Dev Engineer's backlog items
  - Loads Dev Engineer persona definition
  - Sees Tech Lead's handoff in session log
  - Ready to implement TD_014

  ---
  🔁 PHASE 4: The Daily Rhythm

  Typical Day Flow

  Morning: Planning & Architecture
  embody product-owner        # Define new VS items
  /clear → embody tech-lead   # Review and break down

  Midday: Implementation
  /clear → embody dev-engineer  # Build features
  # Multiple commits during implementation

  Afternoon: Quality & Testing
  /clear → embody test-specialist  # Add test coverage
  # Debug if issues found
  /clear → embody debugger-expert  # Investigate complex issues

  End of Day: Cleanup & Automation
  /clear → embody devops-engineer  # Optimize scripts, CI/CD
  # Final push and PR if feature complete

  ---
  📋 PHASE 5: PR Creation (Milestone Complete)

  When Feature/Task Complete

  # As Dev Engineer, VS_003A complete
  git log --oneline  # Review commits
  gh pr create --title "feat(VS_003A): Block Matching System" \
               --body "Implements complete block matching with tests"

  # Squash merge for clean history
  gh pr merge --squash --delete-branch

  ---
  🎯 Key Workflow Principles

  Commit Frequency

  - Every 20-30 minutes: Natural save points
  - At role boundaries: Before switching personas
  - When stuck: Commit WIP before investigating

  Context Management

  - active/[persona].md: Personal working memory
  - Backlog.md: Shared truth for all personas
  - session-log.md: Chronological audit trail

  Conflict Prevention

  - Frequent commits: Minimize uncommitted changes
  - Clean handoffs: Commit before switching
  - Sequential work: One persona at a time

  PR Strategy

  - Work on main: For most development
  - Feature branches: For larger changes
  - PR on completion: Not every push
  - Squash merge: Clean main history

  ---
  🚀 Quick Command Reference

  # Start work as persona (in Claude)
  embody tech-lead

  # Switch personas (in Claude)
  /clear
  embody dev-engineer

  # Emergency switch with uncommitted work
  # (embody.ps1 v3.0 auto-handles stashing)
  /clear
  embody debugger-expert

  # Manual git operations if needed
  git commit -m "WIP: stopping point"
  git push origin main

  ---
  📊 Workflow Success Metrics

  ✅ Good workflow indicators:
  - Commits every 20-30 minutes
  - Clean switches between personas
  - Session log tells clear story
  - PRs represent complete features
  - No merge conflicts

  ⚠️ Warning signs:
  - Commits hours apart
  - Uncommitted changes during switch
  - Session log gaps
  - Many WIP commits in PR
  - Frequent conflicts

  ---
  This complete workflow ensures:
  1. Context preservation between sessions
  2. Clean handoffs between personas
  3. Conflict prevention through frequent commits
  4. Clear audit trail of all work
  5. Natural rhythm for solo development