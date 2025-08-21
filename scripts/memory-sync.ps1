# Memory Bank Synchronization Script
# Simple, elegant, automatic via git hooks
param(
    [ValidateSet("pull", "push", "sync", "status")]
    [string]$Operation = "sync"
)

$ErrorActionPreference = "SilentlyContinue"
$memoryPath = ".claude/memory-bank"
$maxLogDays = 7  # Weekly rotation
$maxPatternsLines = 20

# Ensure Memory Bank exists
if (-not (Test-Path $memoryPath)) {
    New-Item -ItemType Directory -Path $memoryPath -Force | Out-Null
}

function Invoke-Pull {
    # Only sync if Memory Bank exists in origin/main
    git fetch origin main --quiet 2>$null
    $files = git ls-tree -r origin/main --name-only "$memoryPath" 2>$null
    if ($files) {
        git checkout origin/main -- "$memoryPath/*.md" 2>$null
        Write-Host "✅ Memory loaded" -ForegroundColor Green -NoNewline
    }
}

function Invoke-Push {
    # Rotate old SESSION_LOG entries (>7 days)
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

# Compact function removed - activeContext naturally stays small

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
    "status" { Get-Status }
}