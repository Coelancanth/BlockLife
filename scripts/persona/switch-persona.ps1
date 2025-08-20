<#
.SYNOPSIS
    Persona Worktree System - Phase 1 Implementation
    Elegant workspace isolation for BlockLife development personas

.DESCRIPTION
    Creates and switches between isolated git worktree environments for different
    development personas. Each persona gets a complete, conflict-free workspace.

.PARAMETER Persona
    The persona to activate. Phase 1 supports: dev-engineer, tech-lead

.EXAMPLE
    .\switch-persona.ps1 dev-engineer
    Creates/switches to the dev-engineer isolated workspace

.NOTES
    Author: DevOps Engineer
    Date: 2025-08-20
    Version: 1.0.0 (Phase 1)
    
    TD_023 Implementation - Automated Isolation Workspaces
    Designed for solo developer with frequent persona switching
#>

param(
    [Parameter(Mandatory=$false, Position=0)]
    [ValidateSet('dev-engineer', 'tech-lead', 'product-owner', 'test-specialist', 'debugger-expert', 'devops-engineer', '', $null)]
    [string]$Persona,
    
    [Parameter(Mandatory=$false)]
    [switch]$NoLaunchClaude
)

# Configuration
$script:Config = @{
    PersonasDirectory = "personas"
    DefaultBranch = "main"
    SupportedPersonas = @(
        'dev-engineer', 
        'tech-lead',
        'product-owner',
        'test-specialist',
        'debugger-expert',
        'devops-engineer'
    )
    Colors = @{
        Success = 'DarkGreen'      # Gruvbox green
        Warning = 'DarkYellow'      # Gruvbox yellow
        Error = 'DarkRed'           # Gruvbox red
        Info = 'Gray'               # Gruvbox gray
        Muted = 'DarkGray'          # Gruvbox dark gray
        Highlight = 'DarkYellow'    # Gruvbox orange/yellow
    }
}

# Helper Functions
function Write-PersonaMessage {
    param(
        [string]$Message,
        [string]$Type = 'Info'
    )
    
    $color = $script:Config.Colors[$Type]
    Write-Host $Message -ForegroundColor $color
}

function Test-GitRepository {
    if (-not (Test-Path ".git" -PathType Container)) {
        Write-PersonaMessage "[ERROR] Not in a git repository. Please run from the BlockLife project root." 'Error'
        return $false
    }
    return $true
}

function Get-ProjectRoot {
    # Ensure we're at the project root
    $gitRoot = git rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -eq 0) {
        return $gitRoot.Replace('/', '\')
    }
    return $null
}

function Get-WorktreePath {
    param([string]$PersonaName)
    
    $projectRoot = Get-ProjectRoot
    if (-not $projectRoot) {
        return $null
    }
    
    $personasPath = Join-Path $projectRoot $script:Config.PersonasDirectory
    return Join-Path $personasPath $PersonaName
}

function Test-WorktreeExists {
    param([string]$WorktreePath)
    
    # Check if directory exists
    if (-not (Test-Path $WorktreePath -PathType Container)) {
        return $false
    }
    
    # Verify it's a valid git worktree
    $worktrees = git worktree list --porcelain 2>$null
    if ($LASTEXITCODE -eq 0) {
        $normalizedPath = $WorktreePath.Replace('\', '/')
        return $worktrees -match [regex]::Escape($normalizedPath)
    }
    
    return $false
}

function New-PersonaWorktree {
    param(
        [string]$PersonaName,
        [string]$WorktreePath
    )
    
    Write-PersonaMessage "Creating isolated workspace for $PersonaName..." 'Warning'
    
    # Ensure the personas directory exists
    $personasDir = Split-Path $WorktreePath -Parent
    if (-not (Test-Path $personasDir)) {
        New-Item -ItemType Directory -Path $personasDir -Force | Out-Null
    }
    
    # Create a unique branch for this persona to avoid conflicts
    $personaBranch = "persona/$PersonaName/workspace"
    
    # Create the worktree with a new branch based on main
    $relativePath = Join-Path $script:Config.PersonasDirectory $PersonaName
    $output = git worktree add -b $personaBranch $relativePath $script:Config.DefaultBranch 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        # Try alternative: use existing branch if it exists
        $output = git worktree add $relativePath $personaBranch 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-PersonaMessage "[ERROR] Failed to create worktree:" 'Error'
            Write-Host $output -ForegroundColor DarkRed
            return $false
        }
    }
    
    Write-PersonaMessage "[OK] Successfully created isolated workspace" 'Success'
    Write-PersonaMessage "  Location: $WorktreePath" 'Muted'
    Write-PersonaMessage "  Branch: $personaBranch" 'Muted'
    return $true
}

function Switch-ToWorktree {
    param(
        [string]$PersonaName,
        [string]$WorktreePath
    )
    
    # Change to the worktree directory
    Set-Location $WorktreePath
    
    # Get current branch
    $branch = git branch --show-current 2>$null
    if (-not $branch) {
        $branch = "detached HEAD"
    }
    
    # Check for uncommitted changes
    $changes = git status --porcelain 2>$null | Measure-Object -Line
    
    # Display activation message
    Write-Host ""
    Write-PersonaMessage "--------------------------------------------------------" 'Highlight'
    Write-PersonaMessage "  $($PersonaName.ToUpper()) PERSONA ACTIVATED" 'Highlight'
    Write-PersonaMessage "--------------------------------------------------------" 'Highlight'
    Write-Host ""
    
    Write-PersonaMessage "Workspace: $WorktreePath" 'Info'
    Write-PersonaMessage "Branch: $branch" 'Info'
    
    if ($changes.Lines -gt 0) {
        Write-PersonaMessage "[WARN] Uncommitted changes: $($changes.Lines) file(s)" 'Warning'
    } else {
        Write-PersonaMessage "[OK] Working tree clean" 'Success'
    }
    
    Write-Host ""
    Write-PersonaMessage "Complete isolation achieved - no conflicts possible" 'Success'
}

function Show-OtherWorkspaces {
    param([string]$CurrentPersona)
    
    Write-Host ""
    Write-PersonaMessage "Other persona workspaces:" 'Info'
    
    $projectRoot = Get-ProjectRoot
    $personasDir = Join-Path $projectRoot $script:Config.PersonasDirectory
    
    $found = $false
    if (Test-Path $personasDir) {
        Get-ChildItem $personasDir -Directory | ForEach-Object {
            if ($_.Name -ne $CurrentPersona) {
                $found = $true
                Push-Location $_.FullName -ErrorAction SilentlyContinue
                $branch = git branch --show-current 2>$null
                if (-not $branch) { $branch = "unknown" }
                $changes = git status --porcelain 2>$null | Measure-Object -Line
                Pop-Location
                
                $status = if ($changes.Lines -gt 0) { "($($changes.Lines) changes)" } else { "(clean)" }
                Write-PersonaMessage "  - $($_.Name): $branch $status" 'Muted'
            }
        }
    }
    
    if (-not $found) {
        Write-PersonaMessage "  - No other personas active" 'Muted'
    }
}

function Show-Usage {
    Write-Host ""
    Write-PersonaMessage "PERSONA WORKTREE SYSTEM" 'Highlight'
    Write-Host ""
    Write-PersonaMessage "Usage: .\switch-persona.ps1 <persona-name>" 'Info'
    Write-Host ""
    Write-PersonaMessage "Available personas:" 'Info'
    $script:Config.SupportedPersonas | ForEach-Object {
        Write-PersonaMessage "  - $_" 'Success'
    }
    Write-Host ""
    Write-PersonaMessage "Example:" 'Info'
    Write-PersonaMessage "  .\switch-persona.ps1 dev-engineer" 'Muted'
    Write-Host ""
    Write-PersonaMessage "Each persona gets a completely isolated workspace using git worktrees." 'Muted'
    Write-PersonaMessage "This eliminates all conflicts between concurrent persona sessions." 'Muted'
    Write-Host ""
}

# Main Execution
function Main {
    # Clear any previous error state
    $Error.Clear()
    
    # Show usage if no persona specified
    if (-not $Persona) {
        Show-Usage
        return
    }
    
    # Verify we're in a git repository
    if (-not (Test-GitRepository)) {
        return
    }
    
    # Get the worktree path
    $worktreePath = Get-WorktreePath -PersonaName $Persona
    if (-not $worktreePath) {
        Write-PersonaMessage "[ERROR] Failed to determine project root" 'Error'
        return
    }
    
    Write-PersonaMessage "Activating $Persona persona..." 'Info'
    
    # Create worktree if it doesn't exist
    if (-not (Test-WorktreeExists -WorktreePath $worktreePath)) {
        if (-not (New-PersonaWorktree -PersonaName $Persona -WorktreePath $worktreePath)) {
            return
        }
    } else {
        Write-PersonaMessage "[OK] Using existing workspace" 'Success'
    }
    
    # Switch to the worktree
    Switch-ToWorktree -PersonaName $Persona -WorktreePath $worktreePath
    
    # Show other active workspaces
    Show-OtherWorkspaces -CurrentPersona $Persona
    
    Write-Host ""
    Write-PersonaMessage "Ready for $Persona work in isolated environment" 'Success'
    Write-Host ""
    
    # Launch Claude unless explicitly disabled
    if (-not $NoLaunchClaude) {
        Write-Host ""
        Write-PersonaMessage "Launching Claude Code..." 'Info'
        Write-PersonaMessage "Tip: Use -NoLaunchClaude to skip Claude launch" 'Muted'
        Write-Host ""
        
        # Check if claude command exists
        $claudeExists = Get-Command claude -ErrorAction SilentlyContinue
        if ($claudeExists) {
            # Launch Claude in the current terminal
            claude
        } else {
            Write-PersonaMessage "[WARN] Claude command not found. Please ensure Claude Code is installed." 'Warning'
            Write-PersonaMessage "Visit: https://claude.ai/download" 'Muted'
        }
    }
}

# Execute main function
Main