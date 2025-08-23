#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Embody a BlockLife persona with v4.0 Intelligent Auto-Sync
.DESCRIPTION
    Complete persona embodiment with automatic git state resolution:
    - Detects and handles squash merges automatically
    - Resolves conflicts intelligently
    - Preserves uncommitted work
    - Ensures clean persona switches every time
.PARAMETER Persona
    The persona to embody (dev-engineer, tech-lead, etc.)
.EXAMPLE
    embody-v4 tech-lead
    Embodies Tech Lead with automatic sync resolution
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('dev-engineer', 'tech-lead', 'test-specialist', 'debugger-expert', 'product-owner', 'devops-engineer')]
    [string]$Persona
)

# Import smart-sync functions
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$gitScripts = Join-Path (Split-Path $scriptRoot) "git"

# Color functions
function Write-Phase($message) { 
    Write-Host ""
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
    Write-Host "  $message" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
}

function Write-Status($message) { Write-Host "🔄 $message" -ForegroundColor Cyan }
function Write-Success($message) { Write-Host "✅ $message" -ForegroundColor Green }
function Write-Warning($message) { Write-Host "⚠️  $message" -ForegroundColor Yellow }
function Write-Info($message) { Write-Host "ℹ️  $message" -ForegroundColor Gray }
function Write-Decision($message) { Write-Host "🎯 $message" -ForegroundColor Yellow }

# Function to detect git state and apply appropriate strategy
function Resolve-GitState {
    Write-Phase "Intelligent Git Sync v4.0"
    
    # Get current state
    $branch = git branch --show-current
    $hasUncommitted = [bool](git status --porcelain)
    
    # Stash if needed
    if ($hasUncommitted) {
        Write-Warning "Stashing uncommitted changes..."
        $stashMessage = "embody-auto-stash-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        git stash push -m $stashMessage --include-untracked
    }
    
    # Fetch latest
    Write-Status "Fetching latest from origin..."
    git fetch origin main --quiet
    git fetch origin $branch --quiet 2>$null
    
    # Detection cascade
    $syncStrategy = Get-SyncStrategy -Branch $branch
    
    switch ($syncStrategy) {
        "squash-reset" {
            Write-Decision "Detected squash merge - resetting to main"
            git reset --hard origin/main
            git push origin $branch --force-with-lease 2>$null
            Write-Success "Branch reset to match main after squash merge"
        }
        
        "fast-forward" {
            Write-Decision "Fast-forward available"
            git merge origin/$branch --ff-only 2>$null
            if ($LASTEXITCODE -ne 0) {
                git pull origin main --ff-only
            }
            Write-Success "Fast-forwarded to latest"
        }
        
        "smart-rebase" {
            Write-Decision "Rebasing local commits"
            $rebaseResult = git rebase origin/main 2>&1
            
            if ($LASTEXITCODE -ne 0) {
                # Conflict during rebase - try alternative strategies
                git rebase --abort 2>$null
                
                # Check if it's actually a squash merge situation we missed
                if (Test-HiddenSquashMerge) {
                    Write-Warning "Hidden squash merge detected - switching to reset"
                    git reset --hard origin/main
                    git push origin $branch --force-with-lease 2>$null
                    Write-Success "Resolved via reset"
                } else {
                    # Real conflicts - try merge instead
                    Write-Warning "Rebase has conflicts - using merge strategy"
                    git merge origin/main --no-edit
                    if ($LASTEXITCODE -ne 0) {
                        Write-Warning "Auto-merge failed - manual intervention needed"
                        return $false
                    }
                    Write-Success "Resolved via merge"
                }
            } else {
                Write-Success "Rebase successful"
                # Push if needed
                if ($branch -ne "main" -and (git status | Select-String "ahead")) {
                    git push origin $branch --force-with-lease 2>$null
                }
            }
        }
        
        "up-to-date" {
            Write-Success "Already up to date"
        }
        
        default {
            Write-Warning "Complex state detected - using safe merge"
            git pull origin main --no-rebase
        }
    }
    
    # Restore stash if needed
    if ($hasUncommitted) {
        Write-Status "Restoring stashed changes..."
        git stash pop --quiet
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Stash restoration had conflicts - review with 'git stash list'"
        } else {
            Write-Success "Restored uncommitted changes"
        }
    }
    
    return $true
}

function Get-SyncStrategy {
    param([string]$Branch)
    
    $ahead = git rev-list --count origin/main..HEAD
    $behind = git rev-list --count HEAD..origin/main
    
    # Check for squash merge indicators
    if ($Branch -eq "dev/main") {
        # Check GitHub for merged PRs
        if (Get-Command gh -ErrorAction SilentlyContinue) {
            $mergedPR = gh pr list --state merged --head $Branch --limit 1 --json mergedAt --jq '.[0].mergedAt' 2>$null
            if ($mergedPR) {
                $mergeTime = [DateTime]::Parse($mergedPR)
                if ($mergeTime -gt (Get-Date).AddHours(-2)) {
                    return "squash-reset"
                }
            }
        }
        
        # Check commit patterns
        if ($ahead -gt 5 -and $behind -eq 1) {
            $lastMainCommit = git log origin/main -1 --pretty=format:"%s"
            if ($lastMainCommit -match '\(#\d+\)') {
                return "squash-reset"
            }
        }
    }
    
    # Determine strategy based on state
    if ($ahead -eq 0 -and $behind -eq 0) {
        return "up-to-date"
    } elseif ($ahead -eq 0 -and $behind -gt 0) {
        return "fast-forward"
    } elseif ($ahead -gt 0) {
        return "smart-rebase"
    } else {
        return "merge"
    }
}

function Test-HiddenSquashMerge {
    # Secondary detection for squash merges that weren't caught initially
    $branch = git branch --show-current
    if ($branch -ne "dev/main") { return $false }
    
    # Get the first different commit between branches
    $divergePoint = git merge-base HEAD origin/main
    $localCommits = git log --oneline "$divergePoint..HEAD" | Measure-Object -Line
    $mainCommits = git log --oneline "$divergePoint..origin/main" | Measure-Object -Line
    
    # Many local commits but only 1 on main suggests squash
    if ($localCommits.Lines -gt 5 -and $mainCommits.Lines -eq 1) {
        # Check if main commit message references a PR
        $mainCommit = git log origin/main -1 --pretty=format:"%s"
        if ($mainCommit -match '#\d+') {
            return $true
        }
    }
    
    return $false
}

# Main embodiment flow
Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  🎭 EMBODYING: $($Persona.ToUpper())" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan

# Step 1: Intelligent Sync
$syncSuccess = Resolve-GitState

if (-not $syncSuccess) {
    Write-Error "Sync failed - please resolve manually and try again"
    exit 1
}

# Step 2: Set Git Identity
Write-Phase "Setting Persona Identity"

$identities = @{
    'dev-engineer' = 'Dev Engineer'
    'tech-lead' = 'Tech Lead'
    'test-specialist' = 'Test Specialist'
    'debugger-expert' = 'Debugger Expert'
    'product-owner' = 'Product Owner'
    'devops-engineer' = 'DevOps Engineer'
}

$identity = $identities[$Persona]
git config user.name $identity
git config user.email "$Persona@blocklife"

Write-Success "Git identity set to: $identity"

# Step 3: Load Memory Bank Context
Write-Phase "Loading Memory Bank"

$memoryBankPath = Join-Path (Split-Path (Split-Path $scriptRoot)) ".claude\memory-bank"
$activeContextPath = Join-Path $memoryBankPath "active\$Persona.md"

if (Test-Path $activeContextPath) {
    Write-Info "Active context for $Persona:"
    Get-Content $activeContextPath | Select-Object -First 20 | ForEach-Object {
        Write-Host "  $_" -ForegroundColor Gray
    }
} else {
    Write-Warning "No active context found for $Persona"
    New-Item -Path $activeContextPath -Force -Value "# $identity Active Context`n`nLast updated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n`n## Current Work`n`n" | Out-Null
    Write-Info "Created new context file"
}

# Step 4: Check Session Log
$sessionLogPath = Join-Path $memoryBankPath "session-log.md"
if (Test-Path $sessionLogPath) {
    $recentLogs = Get-Content $sessionLogPath | Select-Object -Last 10
    if ($recentLogs) {
        Write-Info "Recent session activity:"
        $recentLogs | ForEach-Object {
            Write-Host "  $_" -ForegroundColor Gray
        }
    }
}

# Step 5: Show Backlog Items
Write-Phase "Checking Backlog"

$backlogPath = Join-Path (Split-Path (Split-Path $scriptRoot)) "Docs\01-Active\Backlog.md"
if (Test-Path $backlogPath) {
    $ownedItems = Select-String -Path $backlogPath -Pattern "Owner:\s*$identity" -Context 2,0
    
    if ($ownedItems) {
        Write-Info "Your backlog items:"
        foreach ($item in $ownedItems) {
            $lines = $item.Context.PreContext + $item.Line
            foreach ($line in $lines) {
                if ($line -match '^#{2,3}\s+(.+)$') {
                    Write-Host "  📌 $($Matches[1])" -ForegroundColor Yellow
                } elseif ($line -match 'Status:\s*(.+)$') {
                    Write-Host "     Status: $($Matches[1])" -ForegroundColor Cyan
                }
            }
        }
    } else {
        Write-Info "No items currently assigned to $identity"
    }
}

# Step 6: Final Status
Write-Phase "Ready to Work!"

$branch = git branch --show-current
$status = git status --short

Write-Host ""
Write-Host "📍 Current branch: " -NoNewline -ForegroundColor Cyan
Write-Host $branch -ForegroundColor Yellow

if ($status) {
    Write-Host "📝 Uncommitted changes:" -ForegroundColor Cyan
    Write-Host $status -ForegroundColor Gray
} else {
    Write-Host "✨ Working directory clean" -ForegroundColor Green
}

Write-Host ""
Write-Host "🎭 You are now: " -NoNewline -ForegroundColor Cyan
Write-Host $identity -ForegroundColor Yellow
Write-Host ""

# Step 7: Show smart hints based on context
if ($branch -eq "main") {
    Write-Host "💡 Tip: Create a feature branch for new work" -ForegroundColor Blue
    Write-Host "   git checkout -b feat/VS_XXX-description" -ForegroundColor Gray
} elseif ($branch -eq "dev/main") {
    Write-Host "💡 Tip: Your working branch is ready" -ForegroundColor Blue
    Write-Host "   Commit frequently to prevent conflicts" -ForegroundColor Gray
}

# Check if we should suggest creating a PR
$unpushedCommits = git log origin/$branch..$branch --oneline 2>$null | Measure-Object -Line
if ($unpushedCommits.Lines -gt 3) {
    Write-Host "💡 Tip: You have $($unpushedCommits.Lines) unpushed commits" -ForegroundColor Blue
    Write-Host "   Consider creating a PR: pr create" -ForegroundColor Gray
}

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✅ $identity embodiment complete!" -ForegroundColor Green  
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""