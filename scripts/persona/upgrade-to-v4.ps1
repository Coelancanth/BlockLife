#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Upgrade persona system to v4.0 with automatic sync
.DESCRIPTION
    Migrates from embody.ps1 v3.0 to embody-v4.ps1 with full automatic git sync
#>

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  🚀 UPGRADING TO PERSONA SYSTEM v4.0" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path

# Step 1: Backup existing embody.ps1
Write-Host "📦 Backing up existing embody.ps1..." -ForegroundColor Cyan
if (Test-Path "$scriptPath\embody.ps1") {
    Copy-Item "$scriptPath\embody.ps1" "$scriptPath\embody-v3-backup.ps1" -Force
    Write-Host "✅ Backup created: embody-v3-backup.ps1" -ForegroundColor Green
}

# Step 2: Install smart-sync tools
Write-Host ""
Write-Host "🔧 Installing smart-sync tools..." -ForegroundColor Cyan
& "$scriptPath\..\git\install-smart-sync.ps1"

# Step 3: Create symbolic link or copy
Write-Host ""
Write-Host "🔄 Setting up v4.0 as default..." -ForegroundColor Cyan

# Try to create symlink (requires admin on Windows)
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")

if ($isAdmin) {
    # Remove old embody.ps1 if it exists
    if (Test-Path "$scriptPath\embody.ps1") {
        Remove-Item "$scriptPath\embody.ps1" -Force
    }
    
    # Create symlink
    New-Item -ItemType SymbolicLink -Path "$scriptPath\embody.ps1" -Target "$scriptPath\embody-v4.ps1" -Force | Out-Null
    Write-Host "✅ Created symlink: embody.ps1 → embody-v4.ps1" -ForegroundColor Green
} else {
    # Just rename/copy for non-admin
    if (Test-Path "$scriptPath\embody.ps1") {
        Move-Item "$scriptPath\embody.ps1" "$scriptPath\embody-v3-original.ps1" -Force
    }
    Copy-Item "$scriptPath\embody-v4.ps1" "$scriptPath\embody.ps1" -Force
    Write-Host "✅ Installed v4.0 as embody.ps1" -ForegroundColor Green
}

# Step 4: Test the installation
Write-Host ""
Write-Host "🧪 Testing installation..." -ForegroundColor Cyan

# Test git sync alias
$gitSyncWorks = $false
$testOutput = git sync --help 2>&1
if ($LASTEXITCODE -eq 0 -or $testOutput -match "Smart Sync") {
    Write-Host "✅ git sync command working" -ForegroundColor Green
    $gitSyncWorks = $true
} else {
    Write-Host "⚠️  git sync command not available (restart terminal)" -ForegroundColor Yellow
}

# Test PR command
$prCommandExists = Get-Command pr -ErrorAction SilentlyContinue
if ($prCommandExists) {
    Write-Host "✅ pr command available" -ForegroundColor Green
} else {
    Write-Host "⚠️  pr command not available (restart terminal)" -ForegroundColor Yellow
}

# Step 5: Show summary
Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✅ UPGRADE COMPLETE!" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""

Write-Host "🎯 What's New in v4.0:" -ForegroundColor Cyan
Write-Host "  • Automatic squash merge detection and resolution" -ForegroundColor White
Write-Host "  • Self-healing git state management" -ForegroundColor White
Write-Host "  • Intelligent sync strategies" -ForegroundColor White
Write-Host "  • Zero-friction persona switching" -ForegroundColor White
Write-Host "  • Automatic Memory Bank updates" -ForegroundColor White
Write-Host ""

Write-Host "📚 New Commands Available:" -ForegroundColor Cyan
Write-Host "  git sync       - Smart sync with automatic conflict resolution" -ForegroundColor White
Write-Host "  pr create      - Create PR from current branch" -ForegroundColor White
Write-Host "  pr merge       - Merge PR and auto-sync" -ForegroundColor White
Write-Host "  pr status      - Check PR and sync status" -ForegroundColor White
Write-Host ""

Write-Host "🤖 For Claude:" -ForegroundColor Cyan
Write-Host "  When you type 'embody [persona]', it now runs v4.0 automatically" -ForegroundColor White
Write-Host "  No more manual conflict resolution needed!" -ForegroundColor White
Write-Host ""

if (-not $gitSyncWorks -or -not $prCommandExists) {
    Write-Host "⚠️  IMPORTANT: Restart your terminal to enable all commands" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "💡 Try it now:" -ForegroundColor Blue
Write-Host "  embody tech-lead" -ForegroundColor Gray
Write-Host "  git sync" -ForegroundColor Gray
Write-Host "  pr status" -ForegroundColor Gray
Write-Host ""