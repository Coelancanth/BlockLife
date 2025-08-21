# Memory Bank Synchronization Script
# Simple, elegant, automatic via git hooks
param(
    [ValidateSet("pull", "push", "sync", "compact", "status")]
    [string]$Operation = "sync"
)

$ErrorActionPreference = "SilentlyContinue"
$memoryPath = ".claude/memory-bank"
$maxLogDays = 3
$maxPatternsLines = 20
$maxContextSize = 5000  # 5KB

# Ensure Memory Bank exists
if (-not (Test-Path $memoryPath)) {
    New-Item -ItemType Directory -Path $memoryPath -Force | Out-Null
}

function Invoke-Pull {
    git fetch origin main --quiet 2>$null
    git checkout origin/main -- "$memoryPath/*.md" 2>$null
    if ($?) {
        Write-Host "✅ Memory loaded" -ForegroundColor Green -NoNewline
    }
}

function Invoke-Push {
    # Rotate old SESSION_LOG entries (>3 days)
    $logFile = "$memoryPath/SESSION_LOG.md"
    if (Test-Path $logFile) {
        $content = Get-Content $logFile
        $today = Get-Date
        $filtered = @()
        
        foreach ($line in $content) {
            if ($line -match "^\*\*(\d{4}-\d{2}-\d{2})") {
                $date = [DateTime]::Parse($Matches[1])
                if (($today - $date).Days -le $maxLogDays) {
                    $filtered += $line
                }
            } else {
                $filtered += $line
            }
        }
        
        if ($filtered.Count -ne $content.Count) {
            $filtered | Set-Content $logFile
            Write-Host " (rotated old logs)" -ForegroundColor Gray -NoNewline
        }
    }
    
    # Check patterns-recent size
    $patternsFile = "$memoryPath/patterns-recent.md"
    if (Test-Path $patternsFile) {
        $lineCount = (Get-Content $patternsFile | Measure-Object -Line).Lines
        if ($lineCount -gt $maxPatternsLines) {
            Write-Host "`n⚠️  Extract patterns-recent to Patterns.md ($lineCount lines)" -ForegroundColor Yellow
        }
    }
    
    # Commit and push if changes exist
    git add "$memoryPath/*.md" 2>$null
    $changes = git status --porcelain "$memoryPath" 2>$null
    if ($changes) {
        git commit -m "chore: memory sync $(Get-Date -Format 'yyyy-MM-dd HH:mm')" --quiet 2>$null
        git push origin HEAD:main --quiet 2>$null
        Write-Host " ✅ Saved" -ForegroundColor Green -NoNewline
    }
}

function Invoke-Compact {
    # Compact activeContext if too large
    $contextFile = "$memoryPath/activeContext.md"
    if (Test-Path $contextFile) {
        $size = (Get-Item $contextFile).Length
        if ($size -gt $maxContextSize) {
            Write-Host "🗜️ Compacting activeContext ($([math]::Round($size/1024, 1))KB → <5KB)..." -ForegroundColor Cyan
            
            # Keep only essential sections
            $content = Get-Content $contextFile -Raw
            $compacted = @"
# Active Context - BlockLife Development

**Last Updated**: $(Get-Date -Format "yyyy-MM-dd HH:mm")
**Compacted**: Previous context archived

## Current Work
- Branch: $(git branch --show-current)
- Status: Compacted (see git log for history)

## Recent Items
[Context compacted - check git history for details]
"@
            $compacted | Set-Content $contextFile
            Write-Host "✅ Compacted" -ForegroundColor Green
        }
    }
}

function Get-Status {
    Write-Host "`n📊 Memory Bank Status" -ForegroundColor Cyan
    
    Get-ChildItem $memoryPath -Filter "*.md" | ForEach-Object {
        $size = [math]::Round($_.Length / 1024, 1)
        $lines = (Get-Content $_.FullName | Measure-Object -Line).Lines
        $age = (Get-Date) - $_.LastWriteTime
        
        Write-Host "  $($_.Name): " -NoNewline
        Write-Host "$size KB, $lines lines, " -NoNewline -ForegroundColor Gray
        
        if ($age.Days -gt 7) {
            Write-Host "$($age.Days) days old" -ForegroundColor Yellow
        } else {
            Write-Host "$($age.Days) days old" -ForegroundColor Green
        }
    }
}

# Execute operation
switch ($Operation) {
    "pull" { Invoke-Pull }
    "push" { Invoke-Push }
    "sync" { 
        Invoke-Pull
        Invoke-Push
    }
    "compact" { Invoke-Compact }
    "status" { Get-Status }
}