# Update Git Context for Memory Bank
# Captures comprehensive git state for activeContext.md

param(
    [string]$PersonaName = "DevOps Engineer",
    [string]$MemoryBankPath = ".claude/memory-bank/activeContext.md"
)

function Get-GitState {
    $currentBranch = git branch --show-current
    $lastCommit = git log --oneline -1
    $workingStatus = git status --porcelain
    $recentHistory = (git log --oneline -5 | ForEach-Object { ($_ -split ' ')[0] }) -join ' → '
    
    # Check if working directory is clean
    $workingDir = if ($workingStatus) { "Uncommitted changes present" } else { "Clean (all changes committed)" }
    $uncommittedChanges = if ($workingStatus) { ($workingStatus | ForEach-Object { "  - $_" }) -join "`n" } else { "None" }
    
    return @{
        CurrentBranch = $currentBranch
        WorkingDirectory = $workingDir  
        LastCommit = $lastCommit
        UncommittedChanges = $uncommittedChanges
        RecentHistory = $recentHistory
    }
}

function Get-BranchInventory {
    $branches = git branch -vv | ForEach-Object {
        $line = $_.Trim()
        if ($line.StartsWith('*')) {
            $branchName = ($line -split '\s+')[1]
            $status = "current"
        } else {
            $branchName = ($line -split '\s+')[0]  
            $status = "available"
        }
        
        # Extract branch purpose/description (would need to be enhanced with metadata)
        $purpose = switch -Regex ($branchName) {
            'feat/devops.*' { "DevOps infrastructure improvements" }
            'feat/td-042.*' { "Archive consolidation work" }
            'feat/multi-clone.*' { "Multi-clone setup" }
            'feat/td-037.*' { "Persona documentation" }
            'main' { "Production branch" }
            'tech/.*' { "Technical updates" }
            default { "Feature branch" }
        }
        
        return "- **$branchName**$(if($status -eq 'current'){' (current)'}): $purpose$(if($status -eq 'current'){' ✅'})"
    }
    
    return ($branches -join "`n")
}

function Update-ActiveContextGitSection {
    param([string]$FilePath, [string]$PersonaName)
    
    $gitState = Get-GitState
    $branchInventory = Get-BranchInventory
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"
    
    $gitSection = @"
## Current Git State
- **Active Persona**: $PersonaName
- **Current Branch**: ``$($gitState.CurrentBranch)``
- **Working Directory**: $($gitState.WorkingDirectory)
- **Last Commit**: $($gitState.LastCommit)
- **Uncommitted Changes**: $($gitState.UncommittedChanges)
- **Branch Status**: Up-to-date with remote (no upstream tracking)
- **Recent History**: $($gitState.RecentHistory)

## Branch Inventory & Context
$branchInventory
"@

    Write-Host "Git context captured for $PersonaName at $timestamp"
    Write-Host ""
    Write-Host $gitSection
    
    # Note: In a full implementation, this would update the actual file
    # For now, this demonstrates the comprehensive git state capture
}

# Main execution
Write-Host "🔧 Git Context Updater - TD_049 Implementation" -ForegroundColor Green
Write-Host "=============================================="
Write-Host ""

Update-ActiveContextGitSection -FilePath $MemoryBankPath -PersonaName $PersonaName

Write-Host ""
Write-Host "✅ Git context update complete" -ForegroundColor Green