#!/usr/bin/env python3
"""
Git Hooks Setup Script for BlockLife

Automatically sets up git hooks to enforce branch protection and code quality.
"""

import os
import shutil
import stat
from pathlib import Path

def main():
    project_root = Path(__file__).parent.parent
    hooks_dir = project_root / ".git" / "hooks"
    
    if not hooks_dir.exists():
        print("ERROR: .git/hooks directory not found. Are you in a git repository?")
        return 1
    
    print("Setting up git hooks for BlockLife...")
    
    # Pre-commit hook content
    pre_commit_content = '''#!/bin/bash

# Pre-commit hook for BlockLife
# Prevents commits to main branch and ensures code quality

echo "Running pre-commit checks..."

# Get current branch name
current_branch=$(git symbolic-ref --short HEAD)

# Prevent direct commits to main/master
if [[ "$current_branch" == "main" || "$current_branch" == "master" ]]; then
    echo ""
    echo "BLOCKED: Direct commits to '$current_branch' are not allowed!"
    echo ""
    echo "Please create a feature branch:"
    echo "  git checkout -b feat/your-feature-name"
    echo ""
    echo "Or switch to an existing branch:"
    echo "  git checkout your-branch-name"
    echo ""
    echo "See CLAUDE.md for the complete Git workflow."
    exit 1
fi

echo "Branch check passed: $current_branch"

# Check if there are staged files
if git diff --cached --quiet; then
    echo "⚠️  No staged changes detected"
    exit 0
fi

# Run formatting check on staged files only
echo "🎨 Checking code formatting..."
if ! dotnet format BlockLife.sln --include src/ tests/ --verify-no-changes --verbosity quiet; then
    echo ""
    echo "🚫 Code formatting issues detected!"
    echo ""
    echo "Please run this command to fix formatting:"
    echo "  dotnet format BlockLife.sln --include src/ tests/"
    echo ""
    echo "Then stage your changes and commit again:"
    echo "  git add ."
    echo "  git commit"
    exit 1
fi

echo "Code formatting check passed"

# Run quick architecture tests
echo "Running architecture fitness tests..."
if ! dotnet test tests/BlockLife.Core.Tests.csproj --filter "Category=Architecture" --verbosity quiet --nologo; then
    echo ""
    echo "🚫 Architecture tests failed!"
    echo "Please fix architecture violations before committing."
    exit 1
fi

echo "Architecture tests passed"

# Run unit tests for modified files
echo "🧪 Running unit tests..."
if ! dotnet test tests/BlockLife.Core.Tests.csproj --filter "Category=Unit" --verbosity quiet --nologo; then
    echo ""
    echo "🚫 Unit tests failed!"
    echo "Please fix failing tests before committing."
    exit 1
fi

echo "Unit tests passed"

echo ""
echo "🎉 All pre-commit checks passed!"
echo "   Branch: $current_branch"
echo "   Ready to commit"
echo ""
'''

    # Pre-push hook content
    pre_push_content = '''#!/bin/bash

# Pre-push hook for BlockLife
# Additional protection against pushing to main and ensures quality

echo "🚀 Running pre-push checks..."

# Check if we're pushing to main/master
while read local_ref local_sha remote_ref remote_sha; do
    if [[ "$remote_ref" == "refs/heads/main" || "$remote_ref" == "refs/heads/master" ]]; then
        echo ""
        echo "🚫 BLOCKED: Direct push to main/master branch is not allowed!"
        echo ""
        echo "Please create a pull request instead:"
        echo "  gh pr create --title \"your title\" --body \"your description\""
        echo ""
        echo "Or push to a feature branch:"
        echo "  git push origin your-branch-name"
        exit 1
    fi
done

echo "Push target check passed"

# Run full test suite before push
echo "🧪 Running full test suite..."
if ! dotnet test tests/BlockLife.Core.Tests.csproj --verbosity quiet --nologo; then
    echo ""
    echo "🚫 Tests failed!"
    echo "Please fix all failing tests before pushing."
    exit 1
fi

echo "All tests passed"

echo ""
echo "🎉 Pre-push checks completed successfully!"
echo "   Ready to push"
echo ""
'''

    # Write hooks
    hooks = [
        ("pre-commit", pre_commit_content),
        ("pre-push", pre_push_content)
    ]
    
    for hook_name, content in hooks:
        hook_path = hooks_dir / hook_name
        
        # Backup existing hook if it exists
        if hook_path.exists():
            backup_path = hooks_dir / f"{hook_name}.backup"
            shutil.copy2(hook_path, backup_path)
            print(f"Backed up existing {hook_name} to {backup_path.name}")
        
        # Write new hook
        with open(hook_path, 'w', encoding='utf-8', newline='\n') as f:
            f.write(content)
        
        # Make executable
        st = os.stat(hook_path)
        os.chmod(hook_path, st.st_mode | stat.S_IEXEC)
        
        print(f"Installed {hook_name} hook")
    
    print()
    print("Git hooks setup completed!")
    print()
    print("The following protections are now active:")
    print("  - Prevents direct commits to main/master")
    print("  - Enforces code formatting")
    print("  - Validates architecture constraints")
    print("  - Runs tests before commit/push")
    print()
    print("To test the hooks:")
    print("  git checkout main  # Should be blocked")
    print("  git checkout -b test/hook-test")
    print("  echo 'test' >> README.md")
    print("  git add README.md")
    print("  git commit -m 'test commit'  # Should run checks")
    print()
    
    return 0

if __name__ == "__main__":
    exit(main())