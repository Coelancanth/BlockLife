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
Write-Host "The hooks will now:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Pre-checkout hook:" -ForegroundColor Cyan
Write-Host "   • Validate branch naming (feat/vs-XXX, fix/br-XXX, feat/td-XXX)"
Write-Host "   • Remind you to check the backlog for work items"
Write-Host "   • Prevent invalid branch names"
Write-Host ""
Write-Host "Pre-push hook:" -ForegroundColor Cyan
Write-Host "   • Block direct pushes to main"
Write-Host "   • Ensure your branch is rebased on latest main"
Write-Host "   • Prevent merge conflicts and duplicate work"
Write-Host ""
Write-Host "To test the hooks, try:" -ForegroundColor Green
Write-Host "   git checkout -b test-branch  (validates naming)"
Write-Host "   git push                     (checks if up to date)"
Write-Host ""