# Git Workflow for Multiple Clone Architecture

**Status**: Design Proposal for TD_035  
**Date**: 2025-08-20  
**Author**: Tech Lead  

## 🎯 Core Philosophy: Each Persona is a Separate Developer

The multiple clone architecture treats each persona as if they were a **separate developer** working on the same project. This mental model makes everything intuitive:

- **Dev Engineer Clone** = "Developer Alice's machine"
- **Test Specialist Clone** = "Developer Bob's machine"  
- **Debugger Expert Clone** = "Developer Carol's machine"

Just like real developers on a team, each persona:
- Works in their own repository clone
- Pushes to the shared remote (GitHub)
- Pulls updates from others via the remote
- Never directly touches another's local files

## 🔄 The Elegant Sync Pattern

### Universal Commands (Work in Every Clone)

```bash
# These Sacred Sequence commands work identically in every clone:
git sacred          # Check sync status
git newbranch       # Create branch from fresh main
git syncmain        # Update current branch with latest
git checkfresh      # Verify branch is current
```

### The Daily Workflow

#### Starting Work in Any Persona
```bash
# 1. Enter the persona's workspace
cd C:/Projects/blocklife-dev-engineer

# 2. Always start with sync
git syncmain              # Pulls latest main, rebases current branch

# 3. Create or switch to your work branch
git newbranch feat/new-feature    # If starting new work
# OR
git checkout feat/existing-work   # If continuing work
git syncmain                       # Then sync it

# 4. Do your work
[make changes, test, commit]

# 5. Push to share with other personas
git push origin feat/new-feature
```

#### Switching Personas Mid-Work
```bash
# Scenario: Dev Engineer → Test Specialist

# In Dev Engineer clone:
git add -A
git commit -m "wip: saving work before persona switch"
git push origin feat/current-work

# In Test Specialist clone:
git syncmain                        # Get latest everything
git checkout feat/current-work      # Pull the branch
git pull origin feat/current-work   # Get the WIP commit
# Continue working...
```

## 🛠️ Sync Tooling Suite

### 1. Global Sync Script (`sync-all-personas.ps1`)
```powershell
# Syncs all persona clones with remote
function Sync-AllPersonas {
    $personas = @(
        "blocklife-dev-engineer",
        "blocklife-test-specialist",
        "blocklife-debugger-expert",
        "blocklife-tech-lead",
        "blocklife-product-owner",
        "blocklife-devops-engineer"
    )
    
    foreach ($persona in $personas) {
        Write-Host "📦 Syncing $persona..." -ForegroundColor Cyan
        Push-Location "C:/Projects/$persona"
        
        # Fetch all updates
        git fetch origin --all --prune
        
        # Update main if we're on it
        $currentBranch = git branch --show-current
        if ($currentBranch -eq "main") {
            git pull origin main --ff-only
        }
        
        # Show status
        Write-Host "  Branch: $currentBranch"
        Write-Host "  Status: $(git status -s)"
        
        Pop-Location
    }
    Write-Host "✅ All personas synced!" -ForegroundColor Green
}
```

### 2. Work Transfer Helper (`transfer-work.ps1`)
```powershell
# Transfer uncommitted work between personas
function Transfer-Work {
    param(
        [string]$From,
        [string]$To,
        [string]$Branch
    )
    
    # Create patch in source
    Push-Location "C:/Projects/blocklife-$From"
    git add -A
    git diff --cached > "../transfer.patch"
    Pop-Location
    
    # Apply patch in destination
    Push-Location "C:/Projects/blocklife-$To"
    git checkout $Branch
    git apply "../transfer.patch"
    Remove-Item "../transfer.patch"
    Pop-Location
    
    Write-Host "✅ Work transferred from $From to $To" -ForegroundColor Green
}
```

### 3. Persona Status Dashboard (`persona-status.ps1`)
```powershell
# Show all personas' current state
function Show-PersonaStatus {
    Write-Host "`n🎭 PERSONA STATUS DASHBOARD" -ForegroundColor Magenta
    Write-Host "═══════════════════════════════════════" -ForegroundColor Magenta
    
    $personas = Get-ChildItem "C:/Projects" -Directory -Filter "blocklife-*"
    
    foreach ($persona in $personas) {
        Push-Location $persona.FullName
        
        $branch = git branch --show-current
        $status = git status --porcelain
        $ahead = git rev-list --count origin/$branch..$branch 2>$null
        $behind = git rev-list --count $branch..origin/$branch 2>$null
        
        $icon = switch ($persona.Name) {
            "blocklife-dev-engineer" { "⚙️" }
            "blocklife-test-specialist" { "🧪" }
            "blocklife-debugger-expert" { "🐛" }
            "blocklife-tech-lead" { "📋" }
            "blocklife-product-owner" { "📦" }
            "blocklife-devops-engineer" { "🚀" }
            default { "📁" }
        }
        
        Write-Host "`n$icon $($persona.Name)" -ForegroundColor Cyan
        Write-Host "  Branch: $branch" -ForegroundColor White
        
        if ($status) {
            Write-Host "  Status: Modified files" -ForegroundColor Yellow
        } else {
            Write-Host "  Status: Clean" -ForegroundColor Green
        }
        
        if ($ahead -gt 0) {
            Write-Host "  Ahead:  ↑$ahead commits" -ForegroundColor Yellow
        }
        if ($behind -gt 0) {
            Write-Host "  Behind: ↓$behind commits" -ForegroundColor Magenta
        }
        
        Pop-Location
    }
    Write-Host "`n═══════════════════════════════════════`n" -ForegroundColor Magenta
}
```

## 📋 Workflow Rules for Multiple Clones

### The Golden Rules

1. **Each Clone is Independent**
   - Think of it as a separate developer's machine
   - Never directly modify files in another clone
   - Communicate through git push/pull only

2. **Remote is Truth**
   - GitHub is the single source of truth
   - Always push important work
   - Always pull before starting

3. **Persona Branch Ownership**
   ```
   feat/dev-*       → Dev Engineer primary
   test/br-*        → Test Specialist primary
   fix/debug-*      → Debugger Expert primary
   feat/td-*        → Tech Lead approved
   feat/vs-*        → Product Owner defined
   ci/devops-*      → DevOps Engineer primary
   ```
   But any persona can work on any branch when needed!

4. **Sacred Sequence Everywhere**
   - Every clone has the same git aliases
   - Every clone enforces the same rules
   - Pre-push hooks work identically

### Work Patterns

#### Pattern 1: Clean Handoff
```bash
# Dev Engineer finishes feature
git add -A
git commit -m "feat: complete block rotation"
git push origin feat/block-rotation

# Test Specialist picks it up
git fetch origin
git checkout feat/block-rotation
# Begin testing...
```

#### Pattern 2: Emergency Switch
```bash
# Debugger Expert needs to debug Dev's work NOW
# Dev Engineer (uncommitted changes):
git stash
git stash push -m "wip: debugging handoff"

# Debugger Expert:
git checkout feat/problematic-feature
git pull origin feat/problematic-feature
# Debug without Dev's uncommitted changes interfering
```

#### Pattern 3: Parallel Investigation
```bash
# Multiple personas working on different aspects
# Dev Engineer:
git checkout feat/vs-003-implementation

# Test Specialist (simultaneously):
git checkout test/vs-003-validation

# Both work independently, no conflicts!
```

## 🚀 Migration Path from Worktrees

### Phase 1: Setup Script
```powershell
# setup-multi-clone.ps1
param([string]$BaseDir = "C:/Projects")

# 1. Backup existing worktree setup
Write-Host "📦 Backing up existing work..." -ForegroundColor Yellow
# [backup logic]

# 2. Create clones for each persona
$personas = @(
    "dev-engineer",
    "test-specialist", 
    "debugger-expert",
    "tech-lead",
    "product-owner",
    "devops-engineer"
)

foreach ($persona in $personas) {
    Write-Host "🔧 Creating clone for $persona..." -ForegroundColor Cyan
    git clone git@github.com:user/blocklife.git "$BaseDir/blocklife-$persona"
    
    # Set persona-specific git config
    Push-Location "$BaseDir/blocklife-$persona"
    git config user.name "BlockLife $persona"
    git config user.email "$persona@blocklife.local"
    
    # Install Sacred Sequence hooks
    ./scripts/git/install-hooks.ps1
    Pop-Location
}

# 3. Install global commands
Write-Host "🔧 Installing global commands..." -ForegroundColor Cyan
# [Install sync-all-personas, transfer-work, etc.]

Write-Host "✅ Multi-clone setup complete!" -ForegroundColor Green
```

### Phase 2: Update Documentation
- Remove all worktree references
- Update CLAUDE.md files
- Update persona docs
- Archive old worktree guides

### Phase 3: Cleanup
```powershell
# cleanup-worktrees.ps1
Write-Host "🧹 Removing old worktree setup..." -ForegroundColor Yellow
git worktree list | ForEach-Object {
    if ($_ -match "personas/") {
        git worktree remove $_ --force
    }
}
Write-Host "✅ Worktree cleanup complete!" -ForegroundColor Green
```

## 🎯 Success Metrics

1. **Branch Flexibility**: Same branch checkable in multiple clones ✅
2. **Sacred Sequence**: Works identically everywhere ✅
3. **Setup Time**: < 5 minutes for new developer ✅
4. **Sync Time**: < 30 seconds for all personas ✅
5. **Mental Model**: "Like separate developers" - instantly understood ✅

## 📚 Quick Reference Card

```bash
# Essential Commands (memorize these)
git sacred          # Where am I?
git syncmain        # Get latest
git newbranch name  # Start fresh
git checkfresh      # Am I current?

# Persona Commands (helpful aliases)
sync-all           # Sync all clones
transfer-work      # Move uncommitted work
persona-status     # See all states
switch-persona     # Change workspace

# Emergency Commands
git stash          # Save work quickly
git fetch --all    # Get everything
git reset --hard   # Nuclear option
```

## 🔒 Safety Guarantees

1. **No Lost Work**: Each clone is independent
2. **No Conflicts**: Clones don't share working directories
3. **Easy Recovery**: Just re-clone if something breaks
4. **Full Isolation**: Personas can't corrupt each other
5. **Standard Git**: Everything uses normal git commands

---

*This workflow treats each persona as a separate developer, making the mental model simple and the implementation robust.*