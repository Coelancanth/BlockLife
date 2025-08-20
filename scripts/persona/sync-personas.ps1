#!/usr/bin/env pwsh
#Requires -Version 5.1

<#
.SYNOPSIS
    Syncs all persona clones with remote repository.

.DESCRIPTION
    Fetches latest changes from origin for all persona clones.
    Shows status and divergence information for each clone.

.PARAMETER Path
    Base directory containing persona clones. Defaults to parent of current directory.

.PARAMETER Pull
    Also pull changes (not just fetch). Use with caution if you have local changes.

.EXAMPLE
    .\sync-personas.ps1
    Fetches latest from origin for all personas

.EXAMPLE
    .\sync-personas.ps1 -Pull
    Fetches and pulls latest changes (careful with uncommitted work!)
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$Path = (Split-Path (Get-Location) -Parent),
    
    [Parameter()]
    [switch]$Pull
)

$ErrorActionPreference = 'Stop'

$personas = @(
    "blocklife-dev-engineer",
    "blocklife-test-specialist", 
    "blocklife-debugger-expert",
    "blocklife-tech-lead",
    "blocklife-product-owner",
    "blocklife-devops-engineer"
)

Write-Host "`n🔄 Syncing Persona Repositories" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

$results = @()

foreach ($persona in $personas) {
    $fullPath = Join-Path $Path $persona
    
    if (-not (Test-Path $fullPath)) {
        Write-Host "⏭️  $persona - Not found" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "📍 $persona" -ForegroundColor Cyan
    
    Push-Location $fullPath
    try {
        # Get current branch
        $branch = git branch --show-current 2>$null
        Write-Host "  Branch: $branch" -ForegroundColor Gray
        
        # Check for uncommitted changes
        $status = git status --porcelain 2>$null
        if ($status) {
            Write-Host "  ⚠️  Has uncommitted changes" -ForegroundColor Yellow
            if ($Pull) {
                Write-Host "  ⏭️  Skipping pull due to uncommitted changes" -ForegroundColor Yellow
            }
        }
        
        # Fetch from origin
        Write-Host "  📥 Fetching..." -NoNewline
        git fetch origin 2>&1 | Out-Null
        Write-Host " Done" -ForegroundColor Green
        
        # Check divergence
        $ahead = git rev-list --count origin/main..HEAD 2>$null
        $behind = git rev-list --count HEAD..origin/main 2>$null
        
        if ($behind -gt 0) {
            Write-Host "  📊 Behind origin/main by $behind commits" -ForegroundColor Yellow
            
            if ($Pull -and -not $status) {
                Write-Host "  🔄 Pulling changes..." -NoNewline
                git pull origin main 2>&1 | Out-Null
                Write-Host " Done" -ForegroundColor Green
            }
        } elseif ($ahead -gt 0) {
            Write-Host "  📊 Ahead of origin/main by $ahead commits" -ForegroundColor Cyan
        } else {
            Write-Host "  ✅ Up to date with origin/main" -ForegroundColor Green
        }
        
        $results += @{
            Persona = $persona
            Branch = $branch
            Status = if ($status) { "Modified" } else { "Clean" }
            Ahead = $ahead
            Behind = $behind
        }
        
    } catch {
        Write-Host "  ❌ Error: $_" -ForegroundColor Red
    } finally {
        Pop-Location
    }
    
    Write-Host ""
}

# Summary
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "📊 Sync Summary" -ForegroundColor Cyan
Write-Host ""

$behindCount = ($results | Where-Object { $_.Behind -gt 0 }).Count
$aheadCount = ($results | Where-Object { $_.Ahead -gt 0 }).Count
$modifiedCount = ($results | Where-Object { $_.Status -eq "Modified" }).Count

if ($behindCount -gt 0) {
    Write-Host "  ⚠️  $behindCount personas behind origin/main" -ForegroundColor Yellow
}
if ($aheadCount -gt 0) {
    Write-Host "  📤 $aheadCount personas ahead of origin/main" -ForegroundColor Cyan
}
if ($modifiedCount -gt 0) {
    Write-Host "  📝 $modifiedCount personas have uncommitted changes" -ForegroundColor Yellow
}

if ($behindCount -eq 0 -and $aheadCount -eq 0 -and $modifiedCount -eq 0) {
    Write-Host "  ✅ All personas in sync and clean!" -ForegroundColor Green
}

Write-Host ""