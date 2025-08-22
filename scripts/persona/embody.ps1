#!/usr/bin/env pwsh
#Requires -Version 5.1

<#
.SYNOPSIS
    Smart persona embodiment for single-repo workflow (ADR-004).

.DESCRIPTION
    Sets git identity, ensures clean sync, and loads persona context.
    Designed for solo dev sequential workflow with intelligent guidance.
    Encourages but doesn't enforce frequent commits for conflict prevention.

.PARAMETER Persona
    The persona to embody: dev-engineer, test-specialist, tech-lead, 
    devops-engineer, product-owner, debugger-expert

.EXAMPLE
    .\embody.ps1 dev-engineer
    Syncs latest code, sets identity, loads context, shows assigned work

.NOTES
    Version: 2.0 - Refined with active context structure
    ADR-004: Single-repo persona context management
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('dev-engineer', 'test-specialist', 'tech-lead', 
                 'devops-engineer', 'product-owner', 'debugger-expert')]
    [string]$Persona
)

$ErrorActionPreference = 'Stop'

# Persona configurations with colors and focus areas
$personas = @{
    "dev-engineer" = @{
        name = "Dev Engineer"
        email = "dev@blocklife"
        color = "Green"
        focus = "Implementation and core mechanics"
        tips = @(
            "Check for approved VS items ready for implementation",
            "Review TD items assigned to Dev Engineer",
            "Run tests: ./scripts/core/build.ps1 test"
        )
    }
    "test-specialist" = @{
        name = "Test Specialist"
        email = "test@blocklife"
        color = "Yellow"
        focus = "Quality assurance and test coverage"
        tips = @(
            "Review recent implementations needing test coverage",
            "Check test infrastructure TD items",
            "Run coverage: dotnet test /p:CollectCoverage=true"
        )
    }
    "tech-lead" = @{
        name = "Tech Lead"
        email = "tech-lead@blocklife"
        color = "Cyan"
        focus = "Architecture and technical decisions"
        tips = @(
            "Review backlog for items needing technical breakdown",
            "Check TD items awaiting approval",
            "Validate VS items for architectural integrity"
        )
    }
    "devops-engineer" = @{
        name = "DevOps Engineer"
        email = "devops@blocklife"
        color = "Magenta"
        focus = "Automation and developer experience"
        tips = @(
            "Check CI/CD pipeline status",
            "Review automation TD items",
            "Optimize build and deployment scripts"
        )
    }
    "product-owner" = @{
        name = "Product Owner"
        email = "product@blocklife"
        color = "Blue"
        focus = "Requirements and prioritization"
        tips = @(
            "Define new vertical slices from Ideas backlog",
            "Refine VS items marked 'Needs Refinement'",
            "Prioritize backlog items by player value"
        )
    }
    "debugger-expert" = @{
        name = "Debugger Expert"
        email = "debugger@blocklife"
        color = "Red"
        focus = "Complex debugging and performance"
        tips = @(
            "Check for bugs awaiting investigation",
            "Review flaky test reports",
            "Analyze recent CI failures"
        )
    }
}

$config = $personas[$Persona]

# Header
Write-Host "`n🎭 EMBODYING $($config.name.ToUpper())" -ForegroundColor $config.color
Write-Host ("=" * 60) -ForegroundColor $config.color
Write-Host "Focus: $($config.focus)" -ForegroundColor DarkGray
Write-Host ""

# Step 1: ALWAYS pull latest first (enforced per ADR-004)
Write-Host "📥 Syncing with latest main..." -ForegroundColor Yellow
$pullOutput = git pull origin main --ff-only 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n❌ Cannot fast-forward merge!" -ForegroundColor Red
    Write-Host "   You have local commits that diverge from main." -ForegroundColor Red
    Write-Host "`n📝 Your options:" -ForegroundColor Yellow
    Write-Host "   1. Push your commits: " -NoNewline -ForegroundColor White
    Write-Host "git push origin main" -ForegroundColor Cyan
    Write-Host "   2. Rebase on main: " -NoNewline -ForegroundColor White
    Write-Host "git pull --rebase origin main" -ForegroundColor Cyan
    Write-Host "   3. Create a PR: " -NoNewline -ForegroundColor White
    Write-Host "gh pr create" -ForegroundColor Cyan
    Write-Host ""
    git status
    exit 1
} elseif ($pullOutput -match "Already up to date") {
    Write-Host "  ✅ Already up to date with main" -ForegroundColor Green
} else {
    Write-Host "  ✅ Pulled latest changes from main" -ForegroundColor Green
}

# Step 2: Check for uncommitted work (advisory, not blocking)
$changes = git status --porcelain
if ($changes) {
    # Analyze the type of changes
    $stagedCount = ($changes | Where-Object { $_ -match "^[AM]" }).Count
    $modifiedCount = ($changes | Where-Object { $_ -match "^ M" }).Count
    $untrackedCount = ($changes | Where-Object { $_ -match "^\?\?" }).Count
    
    Write-Host "`n📝 Uncommitted changes detected:" -ForegroundColor Yellow
    
    if ($stagedCount -gt 0) {
        Write-Host "   📦 $stagedCount file(s) staged for commit" -ForegroundColor Green
        Write-Host "      You prepared a commit! Finish it:" -ForegroundColor White
        Write-Host "      git commit -m 'your message'" -ForegroundColor Cyan
    }
    
    if ($modifiedCount -gt 0) {
        Write-Host "   ✏️  $modifiedCount file(s) modified" -ForegroundColor Yellow
    }
    
    if ($untrackedCount -gt 0) {
        Write-Host "   🆕 $untrackedCount new file(s)" -ForegroundColor Blue
    }
    
    # Calculate time since last commit
    $lastCommitTime = git log -1 --format="%ar" 2>$null
    if ($lastCommitTime) {
        Write-Host "`n⏰ Last commit: $lastCommitTime" -ForegroundColor DarkGray
        
        if ($lastCommitTime -match "hour|hours") {
            Write-Host "   💡 It's been a while! Consider committing to prevent conflicts." -ForegroundColor Yellow
        }
    }
    
    Write-Host "`n💡 Tip: Frequent commits (every 20-30 min) prevent conflicts!" -ForegroundColor Green
    Write-Host "   Continuing with persona switch..." -ForegroundColor DarkGray
    Write-Host ""
    
    # Small pause to let user read the message
    Start-Sleep -Seconds 2
}

# Step 3: Set git identity
Write-Host "🔧 Setting git identity..." -ForegroundColor Green
git config user.name $config.name
git config user.email $config.email
Write-Host "  ✅ $($config.name) <$($config.email)>" -ForegroundColor Green

# Step 4: Show repository state
Write-Host "`n📊 Repository state:" -ForegroundColor Green
$branch = git rev-parse --abbrev-ref HEAD
$ahead = git rev-list --count origin/$branch..HEAD 2>$null
$behind = git rev-list --count HEAD..origin/$branch 2>$null

Write-Host "  📍 Branch: " -NoNewline -ForegroundColor White
Write-Host $branch -ForegroundColor Cyan

if ($ahead -gt 0 -or $behind -gt 0) {
    if ($ahead -gt 0) {
        Write-Host "  ↑ $ahead commit(s) ahead of origin/$branch" -ForegroundColor Yellow
        Write-Host "     Consider: git push origin $branch" -ForegroundColor DarkGray
    }
    if ($behind -gt 0) {
        Write-Host "  ↓ $behind commit(s) behind origin/$branch" -ForegroundColor Yellow
        Write-Host "     Consider: git pull origin $branch" -ForegroundColor DarkGray
    }
} else {
    Write-Host "  ✅ In sync with origin/$branch" -ForegroundColor Green
}

# Step 5: Load active context (new structure per ADR-004)
$activeContextPath = ".claude/memory-bank/active/$Persona.md"
if (Test-Path $activeContextPath) {
    Write-Host "`n📚 Active context for $($config.name):" -ForegroundColor $config.color
    
    # Extract key information from context
    $contextContent = Get-Content $activeContextPath -Raw
    
    # Look for current work section
    if ($contextContent -match "## Current Work\s*\n([\s\S]*?)(?=##|$)") {
        $currentWork = $matches[1].Trim()
        if ($currentWork -and $currentWork -ne "- No active implementation tasks") {
            Write-Host "  Current Work:" -ForegroundColor White
            $currentWork -split "`n" | Select-Object -First 3 | ForEach-Object {
                if ($_.Trim()) {
                    Write-Host "    $_" -ForegroundColor DarkGray
                }
            }
        }
    }
    
    # Look for next actions
    if ($contextContent -match "## Next Actions\s*\n([\s\S]*?)(?=##|$)") {
        $nextActions = $matches[1].Trim()
        if ($nextActions) {
            Write-Host "  Next Actions:" -ForegroundColor White
            $nextActions -split "`n" | Select-Object -First 3 | ForEach-Object {
                if ($_.Trim()) {
                    Write-Host "    $_" -ForegroundColor DarkGray
                }
            }
        }
    }
} else {
    Write-Host "`n📚 Creating active context file..." -ForegroundColor Yellow
    New-Item -Path $activeContextPath -ItemType File -Force | Out-Null
    @"
# $($config.name) Active Context

**Last Updated**: $(Get-Date -Format 'yyyy-MM-dd')
**Current Focus**: [Starting fresh session]

## Current Work
- No active tasks yet

## Next Actions
- [ ] Check backlog for assigned items
- [ ] Review recent session log entries

## Notes
[Session notes will go here]
"@ | Set-Content $activeContextPath
    Write-Host "  ✅ Created: $activeContextPath" -ForegroundColor Green
}

# Step 6: Check recent session log entries
$sessionLogPath = ".claude/memory-bank/session-log.md"
if (Test-Path $sessionLogPath) {
    Write-Host "`n📜 Recent session activity:" -ForegroundColor Yellow
    
    # Get last 3 entries from any persona
    $logContent = Get-Content $sessionLogPath
    $entries = @()
    $currentEntry = @()
    
    foreach ($line in $logContent) {
        if ($line -match "^### \d+:\d+ - ") {
            if ($currentEntry.Count -gt 0) {
                $entries += ,($currentEntry -join "`n")
            }
            $currentEntry = @($line)
        } elseif ($currentEntry.Count -gt 0) {
            $currentEntry += $line
        }
    }
    if ($currentEntry.Count -gt 0) {
        $entries += ,($currentEntry -join "`n")
    }
    
    $recentEntries = $entries | Select-Object -Last 2
    foreach ($entry in $recentEntries) {
        if ($entry -match "### (\d+:\d+) - (.+)") {
            Write-Host "  $($matches[1]) $($matches[2]):" -ForegroundColor White
            
            # Extract key info from entry
            if ($entry -match "Handoff Notes?: ([^\n]+)") {
                Write-Host "    → $($matches[1])" -ForegroundColor Yellow
            }
            if ($entry -match "Status: ([^\n]+)") {
                Write-Host "    Status: $($matches[1])" -ForegroundColor DarkGray
            }
        }
    }
}

# Step 7: Check backlog ownership
Write-Host "`n📋 Backlog items assigned to you:" -ForegroundColor $config.color
$backlogPath = "Docs/01-Active/Backlog.md"
if (Test-Path $backlogPath) {
    $ownerPattern = "Owner:\s*$($config.name)"
    $ownedItems = Select-String -Path $backlogPath -Pattern $ownerPattern -Context 2,0
    
    if ($ownedItems) {
        $itemCount = 0
        foreach ($item in $ownedItems) {
            $itemCount++
            $title = $item.Context.PreContext[0] -replace '^###\s*', ''
            
            # Try to extract status if present
            $statusLine = $item.Context.PreContext[1]
            if ($statusLine -match "Status:\s*(.+)") {
                $status = $matches[1].Trim()
                Write-Host "  • $title " -NoNewline -ForegroundColor White
                Write-Host "[$status]" -ForegroundColor DarkGray
            } else {
                Write-Host "  • $title" -ForegroundColor White
            }
            
            # Limit to first 5 items
            if ($itemCount -ge 5) {
                $remaining = $ownedItems.Count - 5
                if ($remaining -gt 0) {
                    Write-Host "  ... and $remaining more" -ForegroundColor DarkGray
                }
                break
            }
        }
    } else {
        Write-Host "  ℹ️  No items currently assigned to $($config.name)" -ForegroundColor Gray
        Write-Host "     Check the backlog for available work" -ForegroundColor DarkGray
    }
}

# Step 8: Persona-specific tips
Write-Host "`n💡 Suggested actions for $($config.name):" -ForegroundColor $config.color
foreach ($tip in $config.tips) {
    Write-Host "  • $tip" -ForegroundColor DarkGray
}

# Step 9: Final summary and instructions
Write-Host "`n" + ("=" * 60) -ForegroundColor $config.color
Write-Host "✅ READY TO WORK AS $($config.name.ToUpper())!" -ForegroundColor $config.color
Write-Host ""
Write-Host "📝 Next steps:" -ForegroundColor White
Write-Host "  1. In Claude, use: " -NoNewline -ForegroundColor DarkGray
Write-Host "/clear" -ForegroundColor Cyan
Write-Host "  2. Then tell Claude: " -NoNewline -ForegroundColor DarkGray
Write-Host "embody $Persona" -ForegroundColor Cyan
Write-Host "  3. Claude loads context from:" -ForegroundColor DarkGray
Write-Host "     • .claude/memory-bank/active/$Persona.md" -ForegroundColor DarkGray
Write-Host "     • Docs/01-Active/Backlog.md (shared truth)" -ForegroundColor DarkGray
Write-Host ""
Write-Host "⚡ Remember:" -ForegroundColor Yellow
Write-Host "  • Commit every 20-30 minutes" -ForegroundColor DarkGray
Write-Host "  • Push after 2-3 commits" -ForegroundColor DarkGray
Write-Host "  • PR when feature complete" -ForegroundColor DarkGray
Write-Host "  • Update session-log.md for handoffs" -ForegroundColor DarkGray
Write-Host ""