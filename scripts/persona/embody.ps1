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

# Capture timestamp at script start for consistent timestamps throughout (TD_078)
$scriptStartTime = Get-Date
$timestampFormatted = $scriptStartTime.ToString("yyyy-MM-dd HH:mm")

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
        $stashMessage = "embody-auto-stash-$($scriptStartTime.ToString('yyyyMMdd-HHmmss'))"
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
            Write-Decision "Detected squash merge - checking for local commits"
            
            # CRITICAL: Check for unpushed local commits AFTER the squash merge
            $localOnly = git log origin/$branch..HEAD --oneline 2>$null
            if ($localOnly) {
                Write-Warning "Found unpushed local commits after squash merge:"
                $localOnly | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
                Write-Warning "Preserving local commits - will rebase onto main"
                
                # Save the local commits
                $tempBranch = "temp-save-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
                git branch $tempBranch
                
                # Reset to main
                git reset --hard origin/main
                
                # Cherry-pick the local commits back
                $commits = git rev-list --reverse origin/$branch...$tempBranch
                foreach ($commit in $commits) {
                    git cherry-pick $commit
                    if ($LASTEXITCODE -ne 0) {
                        Write-Error "Failed to preserve commit $commit - manual intervention required"
                        Write-Host "Your commits are saved in branch: $tempBranch" -ForegroundColor Yellow
                        return $false
                    }
                }
                
                # Clean up temp branch
                git branch -D $tempBranch
                Write-Success "Preserved local commits after squash merge reset"
            } else {
                # No local commits, safe to reset
                git reset --hard origin/main
                git push origin $branch --force-with-lease 2>$null
                Write-Success "Branch reset to match main after squash merge"
            }
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
                    Write-Warning "Hidden squash merge detected - checking for local commits"
                    
                    # CRITICAL: Check for unpushed local commits AFTER the squash merge
                    $localOnly = git log origin/$branch..HEAD --oneline 2>$null
                    if ($localOnly) {
                        Write-Warning "Found unpushed local commits:"
                        $localOnly | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
                        Write-Warning "Preserving local commits - will rebase onto main"
                        
                        # Save the local commits
                        $tempBranch = "temp-save-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
                        git branch $tempBranch
                        
                        # Reset to main
                        git reset --hard origin/main
                        
                        # Cherry-pick the local commits back
                        $commits = git rev-list --reverse origin/$branch...$tempBranch
                        foreach ($commit in $commits) {
                            git cherry-pick $commit
                            if ($LASTEXITCODE -ne 0) {
                                Write-Error "Failed to preserve commit $commit - manual intervention required"
                                Write-Host "Your commits are saved in branch: $tempBranch" -ForegroundColor Yellow
                                return $false
                            }
                        }
                        
                        # Clean up temp branch
                        git branch -D $tempBranch
                        Write-Success "Preserved local commits after hidden squash merge"
                    } else {
                        # No local commits, safe to reset
                        git reset --hard origin/main
                        git push origin $branch --force-with-lease 2>$null
                        Write-Success "Resolved via reset"
                    }
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

# Step 3: Auto-fix Session Log Order (Silent)
$fixScriptPath = Join-Path $scriptRoot "..\utils\fix-session-log-order.ps1"
if (Test-Path $fixScriptPath) {
    # Run completely silently - redirect all output to null
    & $fixScriptPath *>$null
    # Only report if there's an actual failure (non-zero exit code)
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Session log auto-fix encountered an issue (non-critical)"
    }
    # Success is completely silent - true zero friction!
}

# Step 4: Check Session Log - TEMPORARILY DISABLED
# $memoryBankPath = Join-Path (Split-Path (Split-Path $scriptRoot)) ".claude\memory-bank"
# $sessionLogPath = Join-Path $memoryBankPath "session-log.md"
# if (Test-Path $sessionLogPath) {
#     $recentLogs = Get-Content $sessionLogPath | Select-Object -Last 10
#     if ($recentLogs) {
#         Write-Info "Recent session activity:"
#         $recentLogs | ForEach-Object {
#             Write-Host "  $_" -ForegroundColor Gray
#         }
#     }
# }

# Step 5: Show Quick Reference Card
Write-Phase "Quick Reference Card"

$personaDocPath = Join-Path (Split-Path (Split-Path $scriptRoot)) "Docs\04-Personas\$Persona.md"
if (Test-Path $personaDocPath) {
    $content = Get-Content $personaDocPath -Raw
    if ($content -match '### Tier 1: Instant Answers[^\n]*\n((?:^\d+\..+\n)+)' -or 
        $content -match '### Tier 1: Instant Answers[^\n]*\n((?:[^\n]+\n){1,5})') {
        $quickRefs = $matches[1] -split '\n' | Where-Object { $_ -match '^\d+\.' } | Select-Object -First 3
        if ($quickRefs) {
            Write-Info "Top 3 Quick References for ${identity}:"
            $quickRefs | ForEach-Object {
                Write-Host "  $_" -ForegroundColor Cyan
            }
            Write-Host ""
            Write-Host "  📖 Full reference card in: Docs/04-Personas/$Persona.md" -ForegroundColor DarkGray
        }
    }
}

# Step 5.5: Show Persona-Specific Guidance
Write-Phase "Critical Reminders for $identity"

switch ($Persona) {
    'dev-engineer' {
        Write-Host "🔧 " -NoNewline -ForegroundColor Yellow
        Write-Host "Implementation Excellence Standards:" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "  ⚠️  " -NoNewline -ForegroundColor Red
        Write-Host "LanguageExt MANDATORY:" -ForegroundColor Yellow
        Write-Host "    • Use " -NoNewline -ForegroundColor Gray
        Write-Host "Fin<T>" -NoNewline -ForegroundColor White
        Write-Host " not exceptions - chain with " -NoNewline -ForegroundColor Gray
        Write-Host "Bind() Match()" -ForegroundColor White
        Write-Host "    • Query Context7 BEFORE using unfamiliar patterns:" -ForegroundColor Gray
        Write-Host '      mcp__context7__get-library-docs "/louthy/language-ext" --topic "Fin Error bind"' -ForegroundColor DarkGray
        Write-Host ""
        Write-Host "  📋 Before Coding:" -ForegroundColor Cyan
        Write-Host "    • Copy pattern from " -NoNewline -ForegroundColor Gray
        Write-Host "src/Features/Block/Move/" -ForegroundColor White
        Write-Host "    • Write failing test first (TDD)" -ForegroundColor Gray
        Write-Host "    • Check DI registration in " -NoNewline -ForegroundColor Gray
        Write-Host "GameStrapper.cs" -ForegroundColor White
        Write-Host "    • Run " -NoNewline -ForegroundColor Gray
        Write-Host "./scripts/core/build.ps1 test" -NoNewline -ForegroundColor Green
        Write-Host " before ANY commit" -ForegroundColor Gray
    }
    
    'tech-lead' {
        Write-Host "🎯 " -NoNewline -ForegroundColor Yellow
        Write-Host "Architecture & Decision Standards:" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "  📊 TD Complexity Scoring:" -ForegroundColor Cyan
        Write-Host "    • 1-3: Auto-approve (simple, clear benefit)" -ForegroundColor Gray
        Write-Host "    • 4-6: Review necessity (question if needed)" -ForegroundColor Gray
        Write-Host "    • 7-10: Challenge HARD (usually over-engineered)" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  🔪 VS Breakdown Rules:" -ForegroundColor Cyan
        Write-Host "    • >3 days = Split into thinner slices" -ForegroundColor Gray
        Write-Host "    • Each phase independently shippable" -ForegroundColor Gray
        Write-Host "    • Pattern reference: " -NoNewline -ForegroundColor Gray
        Write-Host "src/Features/Block/Move/" -ForegroundColor White
        Write-Host ""
        Write-Host "  💭 Always Challenge:" -ForegroundColor Yellow
        Write-Host '    • "Is this the simplest solution?"' -ForegroundColor Gray
        Write-Host '    • "Does this solve a REAL problem?"' -ForegroundColor Gray
    }
    
    'test-specialist' {
        Write-Host "🧪 " -NoNewline -ForegroundColor Yellow
        Write-Host "Quality & Testing Standards:" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "  📈 Coverage Targets:" -ForegroundColor Cyan
        Write-Host "    • Core logic: 80% minimum" -ForegroundColor Gray
        Write-Host "    • UI components: 60% minimum" -ForegroundColor Gray
        Write-Host "    • Critical paths: 100% required" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  🎲 FsCheck 3.x Patterns:" -ForegroundColor Cyan
        Write-Host "    • Use " -NoNewline -ForegroundColor Gray
        Write-Host "Gen<T>" -NoNewline -ForegroundColor White
        Write-Host " with " -NoNewline -ForegroundColor Gray
        Write-Host ".ToArbitrary()" -ForegroundColor White
        Write-Host "    • Property tests for edge cases" -ForegroundColor Gray
        Write-Host "    • Reference: " -NoNewline -ForegroundColor Gray
        Write-Host "FsCheck3xMigrationGuide.md" -ForegroundColor White
        Write-Host ""
        Write-Host "  🐛 Bug Handling:" -ForegroundColor Cyan
        Write-Host "    • <30min fix = Do it directly" -ForegroundColor Gray
        Write-Host "    • >30min = Create BR_XXX for Debugger Expert" -ForegroundColor Gray
    }
    
    'debugger-expert' {
        Write-Host "🔍 " -NoNewline -ForegroundColor Yellow
        Write-Host "Investigation & Root Cause Standards:" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "  🎯 Common Patterns:" -ForegroundColor Cyan
        Write-Host "    • DI failures → Check " -NoNewline -ForegroundColor Gray
        Write-Host "GameStrapper.cs" -ForegroundColor White
        Write-Host "    • MediatR issues → Verify " -NoNewline -ForegroundColor Gray
        Write-Host "BlockLife.Core.*" -NoNewline -ForegroundColor White
        Write-Host " namespace" -ForegroundColor Gray
        Write-Host "    • Threading → Use " -NoNewline -ForegroundColor Gray
        Write-Host "CallDeferred()" -NoNewline -ForegroundColor White
        Write-Host " for UI updates" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  📝 Post-Mortem Protocol:" -ForegroundColor Cyan
        Write-Host "    • Document ROOT CAUSE not surface fix" -ForegroundColor Gray
        Write-Host "    • Extract patterns to HANDBOOK.md" -ForegroundColor Gray
        Write-Host "    • Archive completed post-mortems" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  ⏱️  Time Limits:" -ForegroundColor Yellow
        Write-Host "    • 2-hour investigation timebox" -ForegroundColor Gray
        Write-Host "    • Create TD if refactor needed" -ForegroundColor Gray
    }
    
    'product-owner' {
        Write-Host "📦 " -NoNewline -ForegroundColor Yellow
        Write-Host "Feature Definition Standards:" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "  ✂️  VS Size Rules:" -ForegroundColor Cyan
        Write-Host "    • Maximum 3 days work per VS" -ForegroundColor Gray
        Write-Host "    • Must be: Independent, Shippable, Valuable" -ForegroundColor Gray
        Write-Host "    • Split large features: VS_003A, VS_003B pattern" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  📖 Terminology (from Glossary):" -ForegroundColor Cyan
        Write-Host '    • Use "Match" not "Clear"' -ForegroundColor Gray
        Write-Host '    • Use "Tier" not "Level"' -ForegroundColor Gray
        Write-Host '    • Use "Turn" not "Round"' -ForegroundColor Gray
        Write-Host ""
        Write-Host "  🔥 Priority Framework:" -ForegroundColor Cyan
        Write-Host "    • Critical: Blocks other work" -ForegroundColor Gray
        Write-Host "    • Important: Current milestone" -ForegroundColor Gray
        Write-Host "    • Ideas: Everything else" -ForegroundColor Gray
    }
    
    'devops-engineer' {
        Write-Host "🤖 " -NoNewline -ForegroundColor Yellow
        Write-Host "Zero-Friction Automation Standards:" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "  🎯 Automation Criteria:" -ForegroundColor Cyan
        Write-Host "    • Happens twice = Automate it" -ForegroundColor Gray
        Write-Host "    • Causes friction = Eliminate it" -ForegroundColor Gray
        Write-Host "    • Takes >15min/week = Script it" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  ✨ Script Excellence:" -ForegroundColor Cyan
        Write-Host "    • Silent operation is best (use " -NoNewline -ForegroundColor Gray
        Write-Host "*>$null" -NoNewline -ForegroundColor White
        Write-Host ")" -ForegroundColor Gray
        Write-Host "    • Idempotent (safe to run multiple times)" -ForegroundColor Gray
        Write-Host "    • Self-documenting progress messages" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  📊 Current Metrics:" -ForegroundColor Cyan
        Write-Host "    • Build time: 2-3 min (target <5 min)" -ForegroundColor Gray
        Write-Host "    • Pre-commit: <0.5s" -ForegroundColor Gray
        Write-Host "    • Automation saves: ~60 min/month" -ForegroundColor Gray
    }
}

Write-Host ""

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