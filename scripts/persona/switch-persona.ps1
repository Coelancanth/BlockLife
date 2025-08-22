#!/usr/bin/env pwsh
#Requires -Version 5.1

<#
.SYNOPSIS
    Prepare for persona switch with clean handoff (ADR-004).

.DESCRIPTION
    Updates active context, logs session work, and ensures clean state
    before switching to another persona. Guides through commit workflow
    if needed and updates all tracking files.

.PARAMETER From
    Current persona (auto-detected if not specified)

.PARAMETER To
    Target persona to switch to

.PARAMETER Message
    Optional handoff message for session log

.EXAMPLE
    .\switch-persona.ps1 -To dev-engineer
    Prepares handoff and switches to Dev Engineer

.EXAMPLE
    .\switch-persona.ps1 -From tech-lead -To dev-engineer -Message "TD_014 approved, ready for implementation"
    Explicit handoff with message

.NOTES
    Version: 1.0 - Clean handoff workflow
    ADR-004: Single-repo persona context management
#>

[CmdletBinding()]
param(
    [ValidateSet('dev-engineer', 'test-specialist', 'tech-lead', 
                 'devops-engineer', 'product-owner', 'debugger-expert', '')]
    [string]$From = '',
    
    [Parameter(Mandatory=$true)]
    [ValidateSet('dev-engineer', 'test-specialist', 'tech-lead', 
                 'devops-engineer', 'product-owner', 'debugger-expert')]
    [string]$To,
    
    [string]$Message = ''
)

$ErrorActionPreference = 'Stop'

# Persona name mappings
$personaNames = @{
    "dev-engineer" = "Dev Engineer"
    "test-specialist" = "Test Specialist"
    "tech-lead" = "Tech Lead"
    "devops-engineer" = "DevOps Engineer"
    "product-owner" = "Product Owner"
    "debugger-expert" = "Debugger Expert"
}

# Auto-detect current persona from git config if not specified
if (-not $From) {
    $currentGitName = git config user.name
    foreach ($persona in $personaNames.GetEnumerator()) {
        if ($persona.Value -eq $currentGitName) {
            $From = $persona.Key
            break
        }
    }
    
    if (-not $From) {
        Write-Host "⚠️  Could not detect current persona from git config" -ForegroundColor Yellow
        Write-Host "   Current git user: $currentGitName" -ForegroundColor DarkGray
        Write-Host "   Please specify -From parameter" -ForegroundColor Yellow
        exit 1
    }
}

$fromName = $personaNames[$From]
$toName = $personaNames[$To]

Write-Host "`n🔄 SWITCHING PERSONAS" -ForegroundColor Cyan
Write-Host ("=" * 60) -ForegroundColor DarkGray
Write-Host "From: $fromName → To: $toName" -ForegroundColor White
Write-Host ""

# Step 1: Check for uncommitted changes
$changes = git status --porcelain
$hasChanges = $false

if ($changes) {
    $hasChanges = $true
    $stagedCount = ($changes | Where-Object { $_ -match "^[AM]" }).Count
    $modifiedCount = ($changes | Where-Object { $_ -match "^ M" }).Count
    $untrackedCount = ($changes | Where-Object { $_ -match "^\?\?" }).Count
    
    Write-Host "📝 Uncommitted changes detected:" -ForegroundColor Yellow
    
    if ($stagedCount -gt 0) {
        Write-Host "   📦 $stagedCount file(s) staged" -ForegroundColor Green
    }
    if ($modifiedCount -gt 0) {
        Write-Host "   ✏️  $modifiedCount file(s) modified" -ForegroundColor Yellow
    }
    if ($untrackedCount -gt 0) {
        Write-Host "   🆕 $untrackedCount new file(s)" -ForegroundColor Blue
    }
    
    Write-Host "`n💡 For clean handoff, please commit your changes:" -ForegroundColor Cyan
    Write-Host "   Quick commit: " -NoNewline -ForegroundColor DarkGray
    Write-Host "git add -A && git commit -m 'feat: description'" -ForegroundColor Cyan
    Write-Host "   Review first: " -NoNewline -ForegroundColor DarkGray
    Write-Host "git diff" -ForegroundColor Cyan
    Write-Host ""
    
    # Ask if they want to commit now
    $response = Read-Host "Do you want to commit now? (y/n/skip)"
    
    if ($response -eq 'y') {
        Write-Host "`nEnter commit message (or press Enter for default):" -ForegroundColor Yellow
        $commitMsg = Read-Host "Message"
        
        if (-not $commitMsg) {
            $commitMsg = "WIP: $fromName work before switching to $toName"
        }
        
        Write-Host "Committing..." -ForegroundColor Green
        git add -A
        git commit -m $commitMsg
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Changes committed!" -ForegroundColor Green
            $hasChanges = $false
        }
    } elseif ($response -eq 'skip') {
        Write-Host "⚠️  Continuing with uncommitted changes..." -ForegroundColor Yellow
        Write-Host "   These will remain for $toName to handle" -ForegroundColor DarkGray
    } else {
        Write-Host "ℹ️  Please commit manually before continuing" -ForegroundColor Cyan
        exit 0
    }
}

# Step 2: Update active context for current persona
$activeContextPath = ".claude/memory-bank/active/$From.md"
Write-Host "`n📚 Updating $fromName active context..." -ForegroundColor Yellow

if (Test-Path $activeContextPath) {
    # Read current content
    $contextContent = Get-Content $activeContextPath -Raw
    
    # Update last updated date
    $newDate = Get-Date -Format 'yyyy-MM-dd HH:mm'
    $contextContent = $contextContent -replace "(\*\*Last Updated\*\*: ).*", "`${1}$newDate"
    
    # Ask for any notes to add
    if (-not $Message) {
        Write-Host "Any notes for your active context? (Enter to skip):" -ForegroundColor Cyan
        $contextNote = Read-Host
        
        if ($contextNote) {
            # Add note to the Notes section
            if ($contextContent -match "## Notes\s*\n") {
                $contextContent = $contextContent -replace "(## Notes\s*\n)", "`$1- $(Get-Date -Format 'HH:mm'): $contextNote`n"
            }
        }
    }
    
    Set-Content -Path $activeContextPath -Value $contextContent
    Write-Host "  ✅ Updated: $activeContextPath" -ForegroundColor Green
}

# Step 3: Add entry to session log
$sessionLogPath = ".claude/memory-bank/session-log.md"
Write-Host "`n📜 Adding session log entry..." -ForegroundColor Yellow

$logEntry = @"

### $(Get-Date -Format 'HH:mm') - $fromName
- **Switching to**: $toName
"@

# Add work status
if ($hasChanges) {
    $logEntry += "`n- **Work Status**: Uncommitted changes present"
} else {
    $lastCommit = git log -1 --format="%s" 2>$null
    if ($lastCommit) {
        $logEntry += "`n- **Last Commit**: ``$lastCommit``"
    }
}

# Add handoff message
if ($Message) {
    $logEntry += "`n- **Handoff Notes**: $Message"
} elseif (-not $hasChanges) {
    Write-Host "Handoff message for $toName? (Enter to skip):" -ForegroundColor Cyan
    $handoffMsg = Read-Host
    if ($handoffMsg) {
        $logEntry += "`n- **Handoff Notes**: $handoffMsg"
    }
}

# Check current branch
$branch = git rev-parse --abbrev-ref HEAD
if ($branch -ne 'main') {
    $logEntry += "`n- **Branch**: $branch"
}

# Add status
$logEntry += "`n- **Status**: Switching personas"

# Append to log
Add-Content -Path $sessionLogPath -Value $logEntry
Write-Host "  ✅ Session logged" -ForegroundColor Green

# Step 4: Push if there are unpushed commits
$ahead = git rev-list --count origin/main..HEAD 2>$null
if ($ahead -gt 0) {
    Write-Host "`n📤 You have $ahead unpushed commit(s)" -ForegroundColor Yellow
    Write-Host "Push now? (y/n):" -ForegroundColor Cyan
    $pushResponse = Read-Host
    
    if ($pushResponse -eq 'y') {
        Write-Host "Pushing to origin/main..." -ForegroundColor Green
        git push origin main
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Pushed successfully!" -ForegroundColor Green
        }
    } else {
        Write-Host "  ⚠️  Remember to push later!" -ForegroundColor Yellow
    }
}

# Step 5: Show quick summary
Write-Host "`n📊 Handoff Summary:" -ForegroundColor Cyan
Write-Host "  From: $fromName" -ForegroundColor White
Write-Host "  To: $toName" -ForegroundColor White
Write-Host "  Time: $(Get-Date -Format 'HH:mm')" -ForegroundColor DarkGray

if ($hasChanges) {
    Write-Host "  ⚠️  Uncommitted changes remain" -ForegroundColor Yellow
} else {
    Write-Host "  ✅ Clean working directory" -ForegroundColor Green
}

# Step 6: Execute the embody script for target persona
Write-Host "`n🎭 Embodying $toName..." -ForegroundColor $personaColors[$To]
Write-Host ("=" * 60) -ForegroundColor DarkGray

# Call embody script
& "$PSScriptRoot\embody.ps1" -Persona $To

# Final message
Write-Host "`n✨ Persona switch complete!" -ForegroundColor Green
Write-Host "   You are now working as: $toName" -ForegroundColor White
Write-Host ""