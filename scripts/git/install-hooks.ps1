# Install git hooks for BlockLife project
# This script copies git hooks to the .git/hooks directory

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$hooksSource = Join-Path $scriptPath "hooks"
$hooksTarget = Join-Path (Split-Path -Parent (Split-Path -Parent $scriptPath)) ".git\hooks"

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
Write-Host "Pre-commit hook:" -ForegroundColor Cyan
Write-Host "   • Run build + tests before each commit"
Write-Host "   • Prevent bad commits from entering history"
Write-Host "   • Save CI resources with fast local validation"
Write-Host ""
Write-Host "Pre-checkout hook:" -ForegroundColor Cyan
Write-Host "   • Validate branch naming (feat/vs-XXX, fix/br-XXX, feat/td-XXX)"
Write-Host "   • Link branches to backlog work items"
Write-Host "   • Guide developers through workflow"
Write-Host ""
Write-Host "Note:" -ForegroundColor Yellow
Write-Host "   GitHub branch protection handles main branch security"
Write-Host "   These hooks focus on quality and workflow guidance"
Write-Host ""
Write-Host "To test the hooks, try:" -ForegroundColor Green
Write-Host "   git checkout -b feat/vs-001-test  (validates naming)"
Write-Host "   git commit -m \"test\"              (runs build+tests)"
Write-Host ""