#!/usr/bin/env pwsh
#Requires -Version 5.1

<#
.SYNOPSIS
    Sets up multiple clone structure for BlockLife persona system.

.DESCRIPTION
    Creates independent git clones for each persona with unique identities.
    Each persona gets its own repository clone with configured git identity.

.PARAMETER Path
    Base directory where persona clones will be created. Defaults to parent of current directory.

.PARAMETER SkipExisting
    Skip clones that already exist instead of erroring out.

.EXAMPLE
    .\setup-personas.ps1
    Creates all 6 persona clones in the parent directory

.EXAMPLE
    .\setup-personas.ps1 -Path "C:\Projects" -SkipExisting
    Creates clones in C:\Projects, skipping any that exist
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$Path = (Split-Path (Get-Location) -Parent),
    
    [Parameter()]
    [switch]$SkipExisting
)

$ErrorActionPreference = 'Stop'

# Configuration
$repoUrl = "https://github.com/mikeschulze/blocklife.git"  # TODO: Update with actual repo URL
$personas = @{
    "dev-engineer"     = @{ email = "dev-eng@blocklife"; name = "Dev Engineer" }
    "test-specialist"  = @{ email = "test-spec@blocklife"; name = "Test Specialist" }
    "debugger-expert"  = @{ email = "debugger@blocklife"; name = "Debugger Expert" }
    "tech-lead"        = @{ email = "tech-lead@blocklife"; name = "Tech Lead" }
    "product-owner"    = @{ email = "product@blocklife"; name = "Product Owner" }
    "devops-engineer"  = @{ email = "devops-eng@blocklife"; name = "DevOps Engineer" }
}

Write-Host "`n🚀 BlockLife Persona Setup Script" -ForegroundColor Cyan
Write-Host "==================================`n" -ForegroundColor Cyan

# Validate git is installed
try {
    $null = git --version
} catch {
    Write-Host "❌ Git is not installed or not in PATH" -ForegroundColor Red
    exit 1
}

# Validate target directory
if (-not (Test-Path $Path)) {
    Write-Host "❌ Target directory does not exist: $Path" -ForegroundColor Red
    exit 1
}

Write-Host "📍 Setting up personas in: $Path" -ForegroundColor Yellow
Write-Host "🔗 Repository: $repoUrl`n" -ForegroundColor Yellow

$successful = 0
$skipped = 0
$failed = 0

foreach ($persona in $personas.GetEnumerator()) {
    $dirName = "blocklife-$($persona.Key)"
    $fullPath = Join-Path $Path $dirName
    
    Write-Host "Setting up $($persona.Key)..." -ForegroundColor Cyan
    
    # Check if directory exists
    if (Test-Path $fullPath) {
        if ($SkipExisting) {
            Write-Host "  ⏭️  Skipped (already exists)" -ForegroundColor Yellow
            $skipped++
            continue
        } else {
            Write-Host "  ❌ Directory already exists: $fullPath" -ForegroundColor Red
            Write-Host "     Use -SkipExisting to skip existing directories" -ForegroundColor Yellow
            $failed++
            continue
        }
    }
    
    try {
        # Clone repository
        Write-Host "  📥 Cloning repository..." -NoNewline
        git clone $repoUrl $fullPath 2>&1 | Out-Null
        Write-Host " Done" -ForegroundColor Green
        
        # Configure git identity
        Push-Location $fullPath
        try {
            Write-Host "  🔧 Configuring identity..." -NoNewline
            git config user.name $persona.Value.name
            git config user.email $persona.Value.email
            Write-Host " Done" -ForegroundColor Green
            
            # Verify configuration
            $configuredName = git config user.name
            $configuredEmail = git config user.email
            
            Write-Host "  ✅ Created: $dirName" -ForegroundColor Green
            Write-Host "     Name:  $configuredName" -ForegroundColor Gray
            Write-Host "     Email: $configuredEmail" -ForegroundColor Gray
            
            $successful++
        } finally {
            Pop-Location
        }
        
    } catch {
        Write-Host "  ❌ Failed: $_" -ForegroundColor Red
        $failed++
        
        # Clean up failed clone
        if (Test-Path $fullPath) {
            Remove-Item $fullPath -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
    
    Write-Host ""
}

# Summary
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "📊 Setup Complete!" -ForegroundColor Cyan
Write-Host "  ✅ Successful: $successful" -ForegroundColor Green
if ($skipped -gt 0) {
    Write-Host "  ⏭️  Skipped: $skipped" -ForegroundColor Yellow
}
if ($failed -gt 0) {
    Write-Host "  ❌ Failed: $failed" -ForegroundColor Red
}

# Create convenience functions file
$functionsFile = Join-Path $Path "persona-functions.ps1"
$functionsContent = @'
# Persona navigation functions
# Source this file in your PowerShell profile for easy persona switching

function blocklife-dev {
    cd "$PSScriptRoot\blocklife-dev-engineer"
    Write-Host "📦 Dev Engineer Workspace" -ForegroundColor Cyan
    git config user.name
}

function blocklife-test {
    cd "$PSScriptRoot\blocklife-test-specialist"
    Write-Host "🧪 Test Specialist Workspace" -ForegroundColor Green
    git config user.name
}

function blocklife-debug {
    cd "$PSScriptRoot\blocklife-debugger-expert"
    Write-Host "🔍 Debugger Expert Workspace" -ForegroundColor Yellow
    git config user.name
}

function blocklife-tech {
    cd "$PSScriptRoot\blocklife-tech-lead"
    Write-Host "🏗️ Tech Lead Workspace" -ForegroundColor Blue
    git config user.name
}

function blocklife-product {
    cd "$PSScriptRoot\blocklife-product-owner"
    Write-Host "📋 Product Owner Workspace" -ForegroundColor Magenta
    git config user.name
}

function blocklife-devops {
    cd "$PSScriptRoot\blocklife-devops-engineer"
    Write-Host "🚀 DevOps Engineer Workspace" -ForegroundColor Red
    git config user.name
}

function blocklife-status {
    Write-Host "`n📊 Persona Repository Status" -ForegroundColor Cyan
    Write-Host "============================`n" -ForegroundColor Cyan
    
    $personas = @(
        "blocklife-dev-engineer",
        "blocklife-test-specialist", 
        "blocklife-debugger-expert",
        "blocklife-tech-lead",
        "blocklife-product-owner",
        "blocklife-devops-engineer"
    )
    
    foreach ($persona in $personas) {
        $path = Join-Path $PSScriptRoot $persona
        if (Test-Path $path) {
            Push-Location $path
            $branch = git branch --show-current 2>$null
            $status = git status --porcelain 2>$null
            $behind = git rev-list --count HEAD..origin/main 2>$null
            
            Write-Host "$persona" -ForegroundColor Yellow
            Write-Host "  Branch: $branch" -ForegroundColor Gray
            if ($status) {
                Write-Host "  Status: Modified" -ForegroundColor Red
            } else {
                Write-Host "  Status: Clean" -ForegroundColor Green
            }
            if ($behind -gt 0) {
                Write-Host "  Behind: $behind commits" -ForegroundColor Yellow
            }
            Pop-Location
        } else {
            Write-Host "$persona" -ForegroundColor Yellow
            Write-Host "  Status: Not found" -ForegroundColor Red
        }
        Write-Host ""
    }
}

Write-Host "Persona functions loaded. Available commands:" -ForegroundColor Green
Write-Host "  blocklife-dev      - Switch to Dev Engineer workspace"
Write-Host "  blocklife-test     - Switch to Test Specialist workspace"
Write-Host "  blocklife-debug    - Switch to Debugger Expert workspace"
Write-Host "  blocklife-tech     - Switch to Tech Lead workspace"
Write-Host "  blocklife-product  - Switch to Product Owner workspace"
Write-Host "  blocklife-devops   - Switch to DevOps Engineer workspace"
Write-Host "  blocklife-status   - Show status of all personas"
'@

if ($successful -gt 0) {
    Write-Host "`n📝 Creating helper functions..." -ForegroundColor Cyan
    $functionsContent | Out-File -FilePath $functionsFile -Encoding UTF8
    Write-Host "  Created: $functionsFile" -ForegroundColor Green
    Write-Host "`n💡 To use persona switching functions, add this to your PowerShell profile:" -ForegroundColor Yellow
    Write-Host "  . `"$functionsFile`"" -ForegroundColor White
}

if ($failed -eq 0) {
    Write-Host "`n✨ All personas ready for use!" -ForegroundColor Green
    exit 0
} else {
    exit 1
}