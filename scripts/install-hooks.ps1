# Install git hooks for BlockLife project
# This script copies git hooks to the .git/hooks directory

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$hooksSource = Join-Path $scriptPath "git-hooks"
$hooksTarget = Join-Path (Split-Path -Parent $scriptPath) ".git\hooks"

if (-not (Test-Path $hooksSource)) {
    Write-Error "❌ Git hooks source directory not found: $hooksSource"
    exit 1
}

if (-not (Test-Path $hooksTarget)) {
    Write-Error "❌ Git hooks target directory not found: $hooksTarget"
    Write-Host "   Make sure you're running this from the project root"
    exit 1
}

Write-Host "📦 Installing git hooks..." -ForegroundColor Cyan

# Copy all hook files
$hooks = Get-ChildItem $hooksSource -File
foreach ($hook in $hooks) {
    $targetFile = Join-Path $hooksTarget $hook.Name
    Copy-Item $hook.FullName $targetFile -Force
    Write-Host "   ✅ Installed: $($hook.Name)" -ForegroundColor Green
    
    # Make executable on Unix-like systems (Git Bash on Windows handles this)
    if ($IsLinux -or $IsMacOS) {
        chmod +x $targetFile
    }
}

Write-Host ""
Write-Host "🎉 Git hooks installed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "The pre-push hook will now:" -ForegroundColor Yellow
Write-Host "   • Block direct pushes to main"
Write-Host "   • Ensure your branch is rebased on latest main"
Write-Host "   • Prevent merge conflicts and duplicate work"
Write-Host ""
Write-Host "To test the hook, try:" -ForegroundColor Cyan
Write-Host "   git push  (it will check if you're up to date)"
Write-Host ""