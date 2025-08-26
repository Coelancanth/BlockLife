#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Intelligent git sync that handles squash merges automatically
.DESCRIPTION
    Detects if your PR was squash-merged and uses reset instead of rebase.
    For normal situations, uses rebase to maintain clean history.
    Zero friction - just run this instead of manual git commands.
.EXAMPLE
    smart-sync
    Automatically syncs current branch with main using the appropriate strategy
.EXAMPLE
    smart-sync -Check
    Preview what would happen without making changes
#>

param(
    [switch]$Check,      # Preview mode - don't make changes
    [switch]$Verbose     # Show detailed decision logic
)

# Configuration
$mainBranch = "main"
$workBranch = "dev/main"

# Colors for output
function Write-Status($message) { Write-Host "🔍 $message" -ForegroundColor Cyan }
function Write-Decision($message) { Write-Host "🎯 $message" -ForegroundColor Yellow }
function Write-Success($message) { Write-Host "✅ $message" -ForegroundColor Green }
function Write-Warning($message) { Write-Host "⚠️  $message" -ForegroundColor Yellow }
function Write-Error($message) { Write-Host "❌ $message" -ForegroundColor Red }
function Write-Info($message) { Write-Host "ℹ️  $message" -ForegroundColor Gray }

# Get current branch
$currentBranch = git branch --show-current
Write-Status "Current branch: $currentBranch"

# Fetch latest without merging
Write-Status "Fetching latest from origin..."
git fetch origin $mainBranch --quiet

# Function to detect if PR was squash-merged
function Test-SquashMerge {
    # Strategy 1: Check if there's a merged PR from this branch
    if (Get-Command gh -ErrorAction SilentlyContinue) {
        $mergedPR = gh pr list --state merged --head $currentBranch --limit 1 --json number,mergedAt --jq '.[0]' 2>$null
        if ($mergedPR) {
            $mergedAt = $mergedPR | ConvertFrom-Json | Select-Object -ExpandProperty mergedAt -ErrorAction SilentlyContinue
            if ($mergedAt) {
                # Check if merge was recent (within last hour)
                $mergeTime = [DateTime]::Parse($mergedAt)
                $hourAgo = (Get-Date).AddHours(-1)
                if ($mergeTime -gt $hourAgo) {
                    if ($Verbose) { Write-Info "Found recently merged PR from this branch" }
                    return $true
                }
            }
        }
    }
    
    # Strategy 2: Check commit count difference
    $localAhead = git rev-list --count "origin/$mainBranch..HEAD"
    $localBehind = git rev-list --count "HEAD..origin/$mainBranch"
    
    if ($localAhead -gt 5 -and $localBehind -eq 1) {
        # Many local commits but only 1 commit behind - likely squash merge
        if ($Verbose) { Write-Info "Detected pattern: many local commits, one commit behind (likely squash)" }
        
        # Additional check: Does the one commit on main look like a squash?
        $lastMainCommit = git log "origin/$mainBranch" -1 --pretty=format:"%s"
        if ($lastMainCommit -match '\(#\d+\)') {  # Has PR number
            if ($Verbose) { Write-Info "Main's latest commit looks like squashed PR: $lastMainCommit" }
            return $true
        }
    }
    
    # Strategy 3: Check if attempting rebase would conflict
    if ($localAhead -gt 0) {
        # Do a dry-run rebase to check for conflicts
        $rebaseTest = git rebase origin/$mainBranch --dry-run 2>&1
        if ($LASTEXITCODE -ne 0 -or $rebaseTest -match "conflict") {
            # Check if our commits are semantically in main already
            $firstLocalCommit = git log --oneline -1 --skip=$($localBehind) HEAD
            $commitMessage = $firstLocalCommit -replace '^\w+\s+', ''
            
            $mainHasContent = git log "origin/$mainBranch" --grep="$commitMessage" --oneline 2>$null
            if ($mainHasContent) {
                if ($Verbose) { Write-Info "Your commits appear to be in main already (as squashed)" }
                return $true
            }
        }
    }
    
    return $false
}

# Function to detect uncommitted changes
function Test-UncommittedChanges {
    $status = git status --porcelain
    return [bool]$status
}

# Main sync logic
function Sync-Branch {
    $hasUncommitted = Test-UncommittedChanges
    $isSquashMerge = Test-SquashMerge
    
    # Stash if needed
    if ($hasUncommitted) {
        Write-Warning "Stashing uncommitted changes..."
        if (-not $Check) {
            git stash push -m "smart-sync auto-stash $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        }
    }
    
    # Decision logic
    if ($currentBranch -eq $workBranch -and $isSquashMerge) {
        Write-Decision "Detected squash merge - checking for new commits..."
        
        # CRITICAL FIX: Check for new commits made AFTER the squash merge
        $currentHead = git rev-parse HEAD
        $mainHead = git rev-parse "origin/$mainBranch"
        
        if ($currentHead -eq $mainHead) {
            Write-Success "Already aligned with main after squash merge"
        } else {
            # Check for NEW commits not part of the squash
            $localOnly = git log "origin/$workBranch..HEAD" --oneline 2>$null
            $newCommits = @()
            
            if ($localOnly) {
                foreach ($line in $localOnly) {
                    if ($line -notmatch '\(#\d+\)$') {
                        # This is a new commit made after the squash
                        $newCommits += $line
                    }
                }
            }
            
            if ($newCommits.Count -gt 0) {
                Write-Warning "Found $($newCommits.Count) NEW commits made after squash merge:"
                $newCommits | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
                Write-Warning "Preserving these new commits..."
                
                if (-not $Check) {
                    # Save commits before reset
                    $tempBranch = "temp-save-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
                    git branch $tempBranch
                    
                    # Reset to main
                    git reset --hard "origin/$mainBranch"
                    
                    # Cherry-pick NEW commits only
                    foreach ($commitLine in $newCommits) {
                        $hash = $commitLine.Split(' ')[0]
                        git cherry-pick $hash 2>$null
                        if ($LASTEXITCODE -eq 0) {
                            Write-Success "Preserved: $commitLine"
                        } else {
                            Write-Warning "Failed to preserve: $commitLine"
                            git cherry-pick --skip 2>$null
                        }
                    }
                    
                    # Clean up temp branch
                    git branch -D $tempBranch 2>$null
                    
                    # Force push to update remote
                    Write-Status "Updating remote $workBranch..."
                    git push origin "$workBranch" --force-with-lease
                    Write-Success "Branch updated with preserved commits!"
                } else {
                    Write-Info "[Preview] Would preserve new commits and reset to main"
                }
            } else {
                Write-Info "No new commits to preserve. Resetting to match main branch."
                
                if (-not $Check) {
                    # Reset local to match main
                    git reset --hard "origin/$mainBranch"
                    
                    # Force push to update remote dev/main
                    Write-Status "Updating remote $workBranch..."
                    git push origin "$workBranch" --force-with-lease
                    
                    Write-Success "Branch reset complete! $workBranch now matches $mainBranch"
                } else {
                    Write-Info "[Preview] Would reset $workBranch to match $mainBranch"
                }
            }
        }
        
    } elseif ($currentBranch -eq $mainBranch) {
        Write-Decision "On main branch - using fast-forward pull"
        
        if (-not $Check) {
            git pull origin $mainBranch --ff-only
            Write-Success "Main branch updated!"
        } else {
            Write-Info "[Preview] Would fast-forward pull main"
        }
        
    } else {
        Write-Decision "Normal feature branch - using rebase strategy"
        Write-Info "Rebasing your commits on top of latest main"
        
        if (-not $Check) {
            $rebaseResult = git rebase "origin/$mainBranch" 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Rebase successful!"
                
                # Push if needed
                if (git status | Select-String "Your branch is ahead") {
                    Write-Status "Pushing rebased commits..."
                    git push origin $currentBranch --force-with-lease
                }
            } else {
                Write-Error "Rebase failed with conflicts"
                Write-Warning "Run 'git rebase --abort' to undo, or resolve conflicts and 'git rebase --continue'"
                return $false
            }
        } else {
            Write-Info "[Preview] Would rebase current branch on main"
        }
    }
    
    # Restore stash if needed
    if ($hasUncommitted -and -not $Check) {
        Write-Status "Restoring stashed changes..."
        git stash pop --quiet
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Couldn't auto-restore stash. Run 'git stash pop' manually"
        }
    }
    
    return $true
}

# Show current state
if ($Verbose) {
    Write-Status "Repository state:"
    $ahead = git rev-list --count "origin/$mainBranch..HEAD"
    $behind = git rev-list --count "HEAD..origin/$mainBranch"
    Write-Info "  Commits ahead of main: $ahead"
    Write-Info "  Commits behind main: $behind"
    
    if ($currentBranch -eq $workBranch) {
        Write-Info "  PR Status: $(if (Test-SquashMerge) { 'Recently merged' } else { 'Not merged or old' })"
    }
}

# Execute sync
$success = Sync-Branch

if ($success) {
    if (-not $Check) {
        Write-Success "Sync complete! Your branch is up to date."
    } else {
        Write-Success "Preview complete. Run without -Check to apply changes."
    }
    
    # Show final status
    git status --short --branch
} else {
    exit 1
}