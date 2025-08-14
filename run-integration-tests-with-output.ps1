# BlockLife Integration Test Runner with File Output
# This script runs tests in Godot editor and captures output for automated analysis

param(
    [string]$TestPath = "test/integration",
    [string]$OutputFile = "test-results.json",
    [string]$LogFile = "test-output.log",
    [switch]$Watch
)

$GodotBin = "C:\Users\Coel\Downloads\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64.exe"
$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "BlockLife Integration Test Runner" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Build first
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build BlockLife.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Please fix compilation errors first." -ForegroundColor Red
    exit $LASTEXITCODE
}

# Clean previous outputs
if (Test-Path $OutputFile) { Remove-Item $OutputFile }
if (Test-Path $LogFile) { Remove-Item $LogFile }

Write-Host ""
Write-Host "Running integration tests (Editor Mode)..." -ForegroundColor Green
Write-Host "Output will be saved to: $OutputFile" -ForegroundColor Gray
Write-Host "Logs will be saved to: $LogFile" -ForegroundColor Gray
Write-Host ""

# Run tests in editor mode with JSON output
$testCommand = @"
& '$GodotBin' --path . --editor --quit-after 10 --script addons/gdUnit4/bin/GdUnitCmdTool.gd -- --add $TestPath -rd reports --format json 2>&1 | Tee-Object -FilePath '$LogFile'
"@

# Execute and capture output
$output = Invoke-Expression $testCommand

# Parse test results from log
$testResults = @{
    timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    success = $false
    totalTests = 0
    passed = 0
    failed = 0
    skipped = 0
    duration = 0
    suites = @()
}

# Simple parsing of output for key metrics
$lines = Get-Content $LogFile
foreach ($line in $lines) {
    if ($line -match "(\d+) tests cases \| (\d+) errors \| (\d+) failures") {
        $testResults.totalTests = [int]$Matches[1]
        $testResults.failed = [int]$Matches[2] + [int]$Matches[3]
        $testResults.passed = $testResults.totalTests - $testResults.failed
    }
    if ($line -match "Total execution time: (\d+)ms") {
        $testResults.duration = [int]$Matches[1]
    }
    if ($line -match "Exit code: (\d+)") {
        $testResults.success = ([int]$Matches[1] -eq 0)
    }
}

# Save results as JSON
$testResults | ConvertTo-Json -Depth 10 | Out-File $OutputFile

# Display summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Results Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total Tests: $($testResults.totalTests)" -ForegroundColor White
Write-Host "Passed: $($testResults.passed)" -ForegroundColor Green
Write-Host "Failed: $($testResults.failed)" -ForegroundColor $(if ($testResults.failed -gt 0) { "Red" } else { "Gray" })
Write-Host "Duration: $($testResults.duration)ms" -ForegroundColor Gray
Write-Host ""

if ($testResults.success) {
    Write-Host "✅ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "❌ Some tests failed. Check $LogFile for details." -ForegroundColor Red
}

# Watch mode - continuously monitor for file changes
if ($Watch) {
    Write-Host ""
    Write-Host "Watching for file changes... (Press Ctrl+C to stop)" -ForegroundColor Yellow
    
    $watcher = New-Object System.IO.FileSystemWatcher
    $watcher.Path = "."
    $watcher.Filter = "*.cs"
    $watcher.IncludeSubdirectories = $true
    $watcher.EnableRaisingEvents = $true
    
    Register-ObjectEvent -InputObject $watcher -EventName "Changed" -Action {
        Write-Host "File changed: $($Event.SourceEventArgs.FullPath)" -ForegroundColor Yellow
        & $PSCommandPath
    }
    
    while ($true) { Start-Sleep -Seconds 1 }
}