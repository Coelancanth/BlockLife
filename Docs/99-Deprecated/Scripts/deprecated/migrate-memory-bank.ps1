# One-time migration to new Memory Bank structure
Write-Host "📦 Migrating Memory Bank to new structure..." -ForegroundColor Cyan

$memoryPath = ".claude/memory-bank"

# 1. Rename patterns.md to patterns-recent.md
if (Test-Path "$memoryPath/patterns.md") {
    Move-Item "$memoryPath/patterns.md" "$memoryPath/patterns-recent.md" -Force
    Write-Host "✅ Renamed patterns.md → patterns-recent.md" -ForegroundColor Green
}

# 2. Remove decisions.md (these belong in backlog)
if (Test-Path "$memoryPath/decisions.md") {
    $content = Get-Content "$memoryPath/decisions.md"
    $lineCount = ($content | Measure-Object -Line).Lines
    
    if ($lineCount -gt 5) {
        Write-Host "⚠️  decisions.md has $lineCount lines - review before deleting:" -ForegroundColor Yellow
        Write-Host "   These should be TD items in the backlog" -ForegroundColor Gray
        
        # Archive instead of delete for safety
        $archivePath = "$memoryPath/decisions-archived-$(Get-Date -Format 'yyyy-MM-dd').md"
        Move-Item "$memoryPath/decisions.md" $archivePath
        Write-Host "📁 Archived to $archivePath" -ForegroundColor Yellow
    } else {
        Remove-Item "$memoryPath/decisions.md"
        Write-Host "✅ Removed decisions.md (use backlog for decisions)" -ForegroundColor Green
    }
}

# 3. Compact activeContext if needed
$contextFile = "$memoryPath/activeContext.md"
if (Test-Path $contextFile) {
    $size = (Get-Item $contextFile).Length
    if ($size -gt 5000) {
        Write-Host "🗜️ activeContext is $([math]::Round($size/1024, 1))KB - needs compacting" -ForegroundColor Yellow
        powershell -ExecutionPolicy Bypass -File scripts/memory-sync.ps1 -Operation compact
    }
}

# 4. Clean up SESSION_LOG (keep only last 3 days)
$logFile = "$memoryPath/SESSION_LOG.md"
if (Test-Path $logFile) {
    $content = Get-Content $logFile
    $today = Get-Date
    $filtered = @()
    $removed = 0
    
    foreach ($line in $content) {
        if ($line -match "^\*\*(\d{4}-\d{2}-\d{2})") {
            $date = [DateTime]::Parse($Matches[1])
            if (($today - $date).Days -le 3) {
                $filtered += $line
            } else {
                $removed++
            }
        } else {
            $filtered += $line
        }
    }
    
    if ($removed -gt 0) {
        $filtered | Set-Content $logFile
        Write-Host "✅ Rotated $removed old log entries (>3 days)" -ForegroundColor Green
    }
}

# 5. Extract valuable lessons to PostMortems if any
if (Test-Path "$memoryPath/lessons.md") {
    $content = Get-Content "$memoryPath/lessons.md"
    $lineCount = ($content | Measure-Object -Line).Lines
    
    if ($lineCount -gt 20) {
        Write-Host "📚 lessons.md has $lineCount lines - consider extracting to:" -ForegroundColor Yellow
        Write-Host "   Docs/06-PostMortems/ for bug fixes" -ForegroundColor Gray
        Write-Host "   Docs/03-Reference/Lessons.md for patterns" -ForegroundColor Gray
        
        # Move to troubleshooting for now
        Move-Item "$memoryPath/lessons.md" "$memoryPath/troubleshooting.md" -Force
        Write-Host "✅ Moved lessons.md → troubleshooting.md" -ForegroundColor Green
    }
}

Write-Host "`n🎯 Migration complete! New structure:" -ForegroundColor Cyan
Write-Host "  activeContext.md    - Current work state" -ForegroundColor Gray
Write-Host "  SESSION_LOG.md      - 3-day activity log" -ForegroundColor Gray
Write-Host "  patterns-recent.md  - Recent discoveries" -ForegroundColor Gray
Write-Host "  troubleshooting.md  - Active issues" -ForegroundColor Gray
Write-Host "`n💡 Decisions now go in Backlog as TD items" -ForegroundColor Yellow