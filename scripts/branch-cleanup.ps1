# Automated Branch Cleanup for Merged PRs
# Usage: .\scripts\branch-cleanup.ps1 [branch-name]
# If no branch specified, cleans up current branch

param(
    [string]$BranchName = ""
)

if ($BranchName -eq "") {
    $BranchName = git rev-parse --abbrev-ref HEAD
}

Write-Host "🧹 Branch Cleanup Tool" -ForegroundColor Cyan
Write-Host "   Target Branch: $BranchName" -ForegroundColor White

# Safety check - don't delete main
if ($BranchName -eq "main") {
    Write-Host "   ❌ Cannot cleanup main branch!" -ForegroundColor Red
    exit 1
}

# Check if PR exists and is merged
try {
    $prInfoJson = gh pr view $BranchName --json state,merged,url,title 2>$null
    
    if ($LASTEXITCODE -eq 0) {
        $prInfo = $prInfoJson | ConvertFrom-Json
        
        if ($prInfo.merged -eq $true) {
            Write-Host "   ✅ PR is merged: $($prInfo.title)" -ForegroundColor Green
            Write-Host "   🔗 $($prInfo.url)" -ForegroundColor Cyan
            
            Write-Host ""
            Write-Host "   🧹 Performing cleanup..." -ForegroundColor Yellow
            
            # Switch to main
            Write-Host "   → Switching to main branch"
            git checkout main
            
            # Pull latest changes
            Write-Host "   → Pulling latest changes"
            git pull origin main
            
            # Delete local branch
            Write-Host "   → Deleting local branch: $BranchName"
            git branch -d $BranchName
            
            # Delete remote branch
            Write-Host "   → Deleting remote branch: $BranchName"
            git push origin --delete $BranchName
            
            Write-Host ""
            Write-Host "   ✅ Cleanup complete! Ready for new work." -ForegroundColor Green
            
        } else {
            Write-Host "   ⚠️  PR exists but is not merged (state: $($prInfo.state))" -ForegroundColor Yellow
            Write-Host "   🔗 $($prInfo.url)" -ForegroundColor Cyan
            Write-Host "   🚨 Manual review required before cleanup" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "   ⚠️  No PR found for branch: $BranchName" -ForegroundColor Yellow
        Write-Host "   🤔 This branch may contain untracked work" -ForegroundColor Cyan
        Write-Host "   💡 Consider creating PR first: gh pr create" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "   ❌ Error checking PR status" -ForegroundColor Red
    Write-Host "   🔍 Check GitHub CLI installation: gh --version" -ForegroundColor Yellow
    exit 1
}