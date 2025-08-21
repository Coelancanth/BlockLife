<#
.SYNOPSIS
    Sets up convenient aliases for the Persona Worktree System
    
.DESCRIPTION
    Creates PowerShell aliases and functions for quick persona switching.
    Can be run manually or added to your PowerShell profile for persistence.
    
.EXAMPLE
    .\setup-aliases.ps1
    Sets up aliases in current session
    
.EXAMPLE
    .\setup-aliases.ps1 -AddToProfile
    Adds aliases to PowerShell profile for permanent use
#>

param(
    [switch]$AddToProfile
)

# Get the BlockLife project root
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
# Go up TWO levels from scripts/persona/ to get to project root
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)

# Create the main blocklife function
$blocklifeFunction = @'
function blocklife {
    <#
    .SYNOPSIS
        Quick persona switcher for BlockLife development
    .DESCRIPTION
        Switches to a persona workspace or shows available personas
    .PARAMETER Persona
        The persona to switch to. If not specified, shows usage.
    .EXAMPLE
        blocklife dev
        Switches to dev-engineer persona
    .EXAMPLE
        blocklife
        Shows available personas
    #>
    param(
        [Parameter(Position=0)]
        [string]$Persona
    )
    
    # Map short names to full persona names
    $personaMap = @{
        'dev' = 'dev-engineer'
        'engineer' = 'dev-engineer'
        'tech' = 'tech-lead'
        'lead' = 'tech-lead'
        'product' = 'product-owner'
        'owner' = 'product-owner'
        'po' = 'product-owner'
        'test' = 'test-specialist'
        'tester' = 'test-specialist'
        'debug' = 'debugger-expert'
        'debugger' = 'debugger-expert'
        'devops' = 'devops-engineer'
        'ops' = 'devops-engineer'
    }
    
    # If no persona specified, show help
    if (-not $Persona) {
        Write-Host ""
        Write-Host "BlockLife Persona Switcher" -ForegroundColor Magenta
        Write-Host "═══════════════════════════════════════" -ForegroundColor Magenta
        Write-Host ""
        Write-Host "Usage: blocklife <persona>" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Quick Aliases:" -ForegroundColor Yellow
        Write-Host "  dev, engineer     → dev-engineer" -ForegroundColor Green
        Write-Host "  tech, lead        → tech-lead" -ForegroundColor Green
        Write-Host "  product, owner, po → product-owner" -ForegroundColor Green
        Write-Host "  test, tester      → test-specialist" -ForegroundColor Green
        Write-Host "  debug, debugger   → debugger-expert" -ForegroundColor Green
        Write-Host "  devops, ops       → devops-engineer" -ForegroundColor Green
        Write-Host ""
        Write-Host "Examples:" -ForegroundColor Yellow
        Write-Host "  blocklife dev     # Switch to dev-engineer" -ForegroundColor DarkGray
        Write-Host "  blocklife tech    # Switch to tech-lead" -ForegroundColor DarkGray
        Write-Host "  blocklife test    # Switch to test-specialist" -ForegroundColor DarkGray
        Write-Host ""
        Write-Host "Other Commands:" -ForegroundColor Yellow
        Write-Host "  bl-status         # Show all persona workspaces" -ForegroundColor DarkGray
        Write-Host "  bl-clean          # Clean up unused worktrees" -ForegroundColor DarkGray
        Write-Host "  bl-return         # Return to main directory" -ForegroundColor DarkGray
        Write-Host ""
        return
    }
    
    # Resolve the persona name
    $fullPersona = if ($personaMap.ContainsKey($Persona.ToLower())) {
        $personaMap[$Persona.ToLower()]
    } else {
        $Persona
    }
    
    # Find the project root and switch
    $projectRoot = "PROJECT_ROOT_PLACEHOLDER"
    $switchScript = Join-Path $projectRoot "scripts\persona\switch-persona.ps1"
    
    if (Test-Path $switchScript) {
        # Pass through any additional parameters (like -NoLaunchClaude if needed)
        & $switchScript $fullPersona
    } else {
        Write-Host "Error: Cannot find switch-persona.ps1 at $switchScript" -ForegroundColor Red
        Write-Host "Make sure you're in the BlockLife project directory" -ForegroundColor Yellow
    }
}
'@

# Replace the placeholder with actual project root
$blocklifeFunction = $blocklifeFunction.Replace('PROJECT_ROOT_PLACEHOLDER', $projectRoot)

# Create additional helper functions
$helperFunctions = @'
function bl-status {
    <#
    .SYNOPSIS
        Shows status of all persona workspaces
    #>
    Write-Host ""
    Write-Host "BlockLife Persona Workspaces" -ForegroundColor Magenta
    Write-Host "═══════════════════════════════════════" -ForegroundColor Magenta
    Write-Host ""
    
    $worktrees = git worktree list 2>$null
    if ($LASTEXITCODE -eq 0) {
        $worktrees | ForEach-Object {
            if ($_ -match 'personas[/\\]([^/\\]+)') {
                $persona = $matches[1]
                $line = $_
                
                # Extract branch name
                $branch = if ($line -match '\[([^\]]+)\]') { $matches[1] } else { 'unknown' }
                
                # Check for changes
                Push-Location ($line -split ' ')[0] -ErrorAction SilentlyContinue
                $changes = git status --porcelain 2>$null | Measure-Object -Line
                Pop-Location
                
                $status = if ($changes.Lines -gt 0) { 
                    "($($changes.Lines) changes)" 
                } else { 
                    "(clean)" 
                }
                
                Write-Host "  🎭 $persona" -NoNewline -ForegroundColor Green
                Write-Host " on " -NoNewline
                Write-Host "$branch" -NoNewline -ForegroundColor Yellow
                Write-Host " $status" -ForegroundColor DarkGray
            }
        }
    } else {
        Write-Host "  Not in a git repository" -ForegroundColor Red
    }
    Write-Host ""
}

function bl-clean {
    <#
    .SYNOPSIS
        Cleans up unused persona worktrees
    #>
    Write-Host "Cleaning up unused worktrees..." -ForegroundColor Yellow
    git worktree prune
    Write-Host "✅ Cleanup complete" -ForegroundColor Green
}

function bl-return {
    <#
    .SYNOPSIS
        Returns to the main BlockLife directory
    #>
    $projectRoot = "PROJECT_ROOT_PLACEHOLDER"
    if (Test-Path $projectRoot) {
        Set-Location $projectRoot
        Write-Host "📂 Returned to main BlockLife directory" -ForegroundColor Green
        Write-Host "   $projectRoot" -ForegroundColor DarkGray
    } else {
        Write-Host "Error: Cannot find BlockLife root at $projectRoot" -ForegroundColor Red
    }
}

# Short aliases for the most common personas
function bl-dev { blocklife dev }
function bl-tech { blocklife tech }
function bl-test { blocklife test }
function bl-debug { blocklife debug }
function bl-devops { blocklife devops }
function bl-product { blocklife product }
'@

# Replace placeholders
$helperFunctions = $helperFunctions.Replace('PROJECT_ROOT_PLACEHOLDER', $projectRoot)

# Function to add to PowerShell profile
function Add-ToProfile {
    param([string]$Content)
    
    $profilePath = $PROFILE.CurrentUserAllHosts
    
    # Create profile if it doesn't exist
    if (-not (Test-Path $profilePath)) {
        New-Item -ItemType File -Path $profilePath -Force | Out-Null
        Write-Host "Created PowerShell profile at: $profilePath" -ForegroundColor Green
    }
    
    # Check if already added
    $profileContent = Get-Content $profilePath -Raw -ErrorAction SilentlyContinue
    if ($profileContent -match "BlockLife Persona Aliases") {
        Write-Host "[WARN] BlockLife aliases already in profile. Updating..." -ForegroundColor DarkYellow
        # Remove old version
        $profileContent = $profileContent -replace '(?s)# BlockLife Persona Aliases.*?# End BlockLife Aliases\r?\n', ''
        Set-Content -Path $profilePath -Value $profileContent.TrimEnd()
    }
    
    # Add new content
    $marker = @"

# BlockLife Persona Aliases
$Content
# End BlockLife Aliases
"@
    
    Add-Content -Path $profilePath -Value $marker
    Write-Host "[OK] Added BlockLife aliases to PowerShell profile" -ForegroundColor DarkGreen
    Write-Host "   Location: $profilePath" -ForegroundColor DarkGray
}

# Execute the functions to load them in current session
Invoke-Expression $blocklifeFunction
Invoke-Expression $helperFunctions

Write-Host ""
Write-Host "BLOCKLIFE PERSONA ALIASES SETUP" -ForegroundColor DarkYellow
Write-Host "--------------------------------------------------------" -ForegroundColor DarkYellow
Write-Host ""

if ($AddToProfile) {
    # Add to profile for persistence
    $fullContent = $blocklifeFunction + "`n`n" + $helperFunctions
    Add-ToProfile -Content $fullContent
    Write-Host ""
    Write-Host "[OK] Aliases added to your PowerShell profile (permanent)" -ForegroundColor DarkGreen
    Write-Host "   They will be available in all future PowerShell sessions" -ForegroundColor DarkGray
} else {
    Write-Host "[OK] Aliases loaded for current session only" -ForegroundColor DarkYellow
    Write-Host ""
    Write-Host "To make permanent, run:" -ForegroundColor Cyan
    Write-Host "  .\scripts\persona\setup-aliases.ps1 -AddToProfile" -ForegroundColor White
}

Write-Host ""
Write-Host "Available Commands:" -ForegroundColor Cyan
Write-Host ""
Write-Host "Main Command:" -ForegroundColor Yellow
Write-Host "  blocklife [persona]  - Switch to a persona or show help" -ForegroundColor Green
Write-Host ""
Write-Host "Quick Shortcuts:" -ForegroundColor Yellow
Write-Host "  bl-dev              - Switch to dev-engineer" -ForegroundColor Green
Write-Host "  bl-tech             - Switch to tech-lead" -ForegroundColor Green  
Write-Host "  bl-test             - Switch to test-specialist" -ForegroundColor Green
Write-Host "  bl-debug            - Switch to debugger-expert" -ForegroundColor Green
Write-Host "  bl-devops           - Switch to devops-engineer" -ForegroundColor Green
Write-Host "  bl-product          - Switch to product-owner" -ForegroundColor Green
Write-Host ""
Write-Host "Utility Commands:" -ForegroundColor Yellow
Write-Host "  bl-status           - Show all persona workspaces" -ForegroundColor Green
Write-Host "  bl-clean            - Clean up unused worktrees" -ForegroundColor Green
Write-Host "  bl-return           - Return to main directory" -ForegroundColor Green
Write-Host ""
Write-Host "Try it now:" -ForegroundColor Cyan
Write-Host "  blocklife dev" -ForegroundColor White
Write-Host ""