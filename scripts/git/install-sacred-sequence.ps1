# Install Sacred Sequence Enforcement for BlockLife
# Version: 1.0.0
# Created: 2025-08-20
# Description: Installs git aliases and hooks to enforce proper git workflow

param(
    [switch]$Force = $false,
    [switch]$Uninstall = $false
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }
function Write-Error { Write-Host $args -ForegroundColor Red }
function Write-Step { Write-Host "  $args" -ForegroundColor Gray }

# Banner
function Show-Banner {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "    🔒 Sacred Sequence Enforcement Installer" -ForegroundColor Cyan
    Write-Host "       Preventing git conflicts since 2025" -ForegroundColor Gray
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
}

# Check if we're in a git repository
function Test-GitRepository {
    try {
        git rev-parse --git-dir 2>&1 | Out-Null
        return $true
    } catch {
        return $false
    }
}

# Get the git hooks directory
function Get-GitHooksDir {
    $gitDir = git rev-parse --git-dir 2>$null
    if (-not $gitDir) {
        throw "Not in a git repository"
    }
    return Join-Path $gitDir "hooks"
}

# Install git aliases
function Install-GitAliases {
    Write-Info "📝 Installing Smart Git Aliases..."
    
    $aliasFile = Join-Path $PSScriptRoot "smart-aliases.gitconfig"
    
    if (-not (Test-Path $aliasFile)) {
        Write-Error "Alias configuration file not found: $aliasFile"
        return $false
    }
    
    # Read the aliases from our config file
    $aliasNames = @(
        "newbranch"
        "nb"
        "syncmain"
        "checkfresh"
        "startwork"
        "finishwork"
        "sacred"
        "safe-checkout"
        "checkout-unsafe"
    )
    
    $aliases = @{}
    
    # Parse the gitconfig file to extract alias values
    $content = Get-Content $aliasFile -Raw
    
    foreach ($aliasName in $aliasNames) {
        # Use regex to extract the alias value (handles multiline aliases)
        if ($content -match "(?ms)^\s*$aliasName\s*=\s*(.+?)(?=^\s*\w+\s*=|\[|\z)") {
            $aliasValue = $matches[1].Trim()
            # Clean up the value (remove extra whitespace, fix line continuations)
            $aliasValue = $aliasValue -replace '\s+\\', ' \'
            $aliasValue = $aliasValue -replace '\s+', ' '
            $aliases[$aliasName] = $aliasValue
        }
    }
    
    # Install each alias
    $installed = 0
    foreach ($aliasName in $aliases.Keys) {
        if ($aliases[$aliasName]) {
            try {
                # Check if alias already exists
                $existing = git config --global --get "alias.$aliasName" 2>$null
                
                if ($existing -and -not $Force) {
                    Write-Warning "   ⚠️  Alias '$aliasName' already exists (use -Force to overwrite)"
                } else {
                    # Set the alias
                    git config --global "alias.$aliasName" $aliases[$aliasName]
                    Write-Step "✓ Installed: git $aliasName"
                    $installed++
                }
            } catch {
                Write-Error "   ✗ Failed to install alias: $aliasName"
            }
        }
    }
    
    Write-Success "   ✅ Installed $installed aliases"
    return $true
}

# Install pre-push hook
function Install-PrePushHook {
    Write-Info "🔨 Installing Pre-Push Hook..."
    
    $hookSource = Join-Path $PSScriptRoot "hooks\pre-push"
    $hooksDir = Get-GitHooksDir
    $hookDest = Join-Path $hooksDir "pre-push"
    
    if (-not (Test-Path $hookSource)) {
        Write-Error "Hook source file not found: $hookSource"
        return $false
    }
    
    # Create hooks directory if it doesn't exist
    if (-not (Test-Path $hooksDir)) {
        New-Item -ItemType Directory -Path $hooksDir -Force | Out-Null
    }
    
    # Check if hook already exists
    if ((Test-Path $hookDest) -and -not $Force) {
        Write-Warning "   ⚠️  Pre-push hook already exists"
        Write-Warning "      Use -Force to overwrite or manually merge"
        return $false
    }
    
    # Copy the hook
    Copy-Item $hookSource $hookDest -Force
    
    # Make it executable (on Unix-like systems via Git Bash if available)
    if (Get-Command bash -ErrorAction SilentlyContinue) {
        bash -c "chmod +x '$hookDest'" 2>$null
    }
    
    Write-Step "✓ Installed pre-push hook"
    Write-Success "   ✅ Hook installed at: $hookDest"
    return $true
}

# Uninstall function
function Uninstall-SacredSequence {
    Write-Info "🗑️  Uninstalling Sacred Sequence Enforcement..."
    
    # Remove aliases
    $aliases = @("newbranch", "nb", "syncmain", "checkfresh", "startwork", 
                 "finishwork", "sacred", "safe-checkout", "checkout-unsafe")
    
    foreach ($alias in $aliases) {
        git config --global --unset "alias.$alias" 2>$null
    }
    Write-Step "✓ Removed git aliases"
    
    # Remove pre-push hook
    $hooksDir = Get-GitHooksDir
    $hookPath = Join-Path $hooksDir "pre-push"
    
    if (Test-Path $hookPath) {
        # Check if it's our hook by looking for our signature
        $content = Get-Content $hookPath -Raw
        if ($content -match "Sacred Sequence Validation") {
            Remove-Item $hookPath -Force
            Write-Step "✓ Removed pre-push hook"
        } else {
            Write-Warning "   ⚠️  Pre-push hook exists but wasn't installed by us, keeping it"
        }
    }
    
    Write-Success "   ✅ Uninstall complete"
}

# Test the installation
function Test-Installation {
    Write-Info "🧪 Testing Installation..."
    
    $tests = @(
        @{
            Name = "Git newbranch alias"
            Test = { git config --global --get "alias.newbranch" }
        },
        @{
            Name = "Git syncmain alias"
            Test = { git config --global --get "alias.syncmain" }
        },
        @{
            Name = "Pre-push hook exists"
            Test = { Test-Path (Join-Path (Get-GitHooksDir) "pre-push") }
        }
    )
    
    $passed = 0
    foreach ($test in $tests) {
        try {
            $result = & $test.Test
            if ($result) {
                Write-Step "✓ $($test.Name)"
                $passed++
            } else {
                Write-Step "✗ $($test.Name)"
            }
        } catch {
            Write-Step "✗ $($test.Name)"
        }
    }
    
    if ($passed -eq $tests.Count) {
        Write-Success "   ✅ All tests passed!"
        return $true
    } else {
        Write-Warning "   ⚠️  Some tests failed"
        return $false
    }
}

# Main execution
function Main {
    Show-Banner
    
    # Check prerequisites
    if (-not (Test-GitRepository)) {
        Write-Error "❌ Not in a git repository!"
        Write-Host "   Please run this script from the BlockLife project root"
        exit 1
    }
    
    if ($Uninstall) {
        Uninstall-SacredSequence
        exit 0
    }
    
    Write-Info "📦 Installing Sacred Sequence components..."
    Write-Host ""
    
    # Install components
    $aliasesOk = Install-GitAliases
    Write-Host ""
    
    $hookOk = Install-PrePushHook
    Write-Host ""
    
    # Test installation
    $testOk = Test-Installation
    Write-Host ""
    
    # Summary
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    if ($aliasesOk -and $hookOk -and $testOk) {
        Write-Success "✅ Sacred Sequence Enforcement Installed Successfully!"
        Write-Host ""
        Write-Info "🎯 Quick Start Commands:"
        Write-Host "   git sacred         - Check Sacred Sequence status"
        Write-Host "   git newbranch name - Create branch from fresh main"
        Write-Host "   git syncmain       - Update current branch with main"
        Write-Host "   git checkfresh     - Check if branch is current"
        Write-Host ""
        Write-Info "📚 Next Steps:"
        Write-Host "   1. Try: git sacred"
        Write-Host "   2. Read: Docs\03-Reference\GitWorkflow.md"
        Write-Host "   3. Use: git newbranch for all new branches"
    } else {
        Write-Warning "⚠️  Installation completed with warnings"
        Write-Host "   Some components may need manual configuration"
        Write-Host "   Run with -Force to overwrite existing components"
    }
    
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
}

# Run main
Main