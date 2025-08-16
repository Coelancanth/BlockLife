#!/usr/bin/env python3
"""
Git Workflow Enforcer for BlockLife

This script enforces the mandatory Git workflow requirements defined in CLAUDE.md
to prevent the #1 workflow violation: working directly on main branch.

Features:
- Blocks commits to main branch (MANDATORY requirement)
- Validates branch naming conventions  
- Checks for proper PR template usage
- Provides clear error messages with corrective actions
- Integrates with pre-commit hooks for automatic enforcement
- Cleans up after PR merges to maintain clean git history

Usage:
    # As pre-commit hook (recommended):
    python scripts/enforce_git_workflow.py --hook pre-commit
    
    # Manual validation:
    python scripts/enforce_git_workflow.py --validate-branch
    
    # Setup pre-commit hooks:
    python scripts/enforce_git_workflow.py --setup-hooks
    
    # Clean up after PR merge (prevents messy history):
    python scripts/enforce_git_workflow.py --cleanup-after-merge
"""

import subprocess
import sys
import argparse
import re
from pathlib import Path
from typing import List, Tuple, Optional
import logging

# Configure logging for clear error reporting with Unicode support
def setup_logging():
    import sys
    import codecs
    
    # Setup UTF-8 output for Windows
    if sys.platform == 'win32':
        sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')
        sys.stderr = codecs.getwriter('utf-8')(sys.stderr.buffer, 'strict')
    
    logging.basicConfig(
        level=logging.INFO,
        format='%(levelname)s: %(message)s',
        handlers=[logging.StreamHandler(sys.stdout)]
    )

setup_logging()
logger = logging.getLogger(__name__)

class GitWorkflowEnforcer:
    """
    Enforces Git workflow requirements following functional programming principles
    Maintains Clean Architecture with clear separation of concerns
    """
    
    # Valid branch prefixes from CLAUDE.md Git Workflow Guide
    VALID_BRANCH_PREFIXES = [
        'feat/',     # New features
        'fix/',      # Bug fixes  
        'docs/',     # Documentation updates
        'refactor/', # Code refactoring
        'test/',     # Adding tests
        'chore/',    # Maintenance
        'hotfix/'    # Emergency fixes
    ]
    
    FORBIDDEN_BRANCHES = ['main', 'master']
    
    def __init__(self, project_root: Path):
        self.project_root = project_root
        self.git_dir = project_root / '.git'
        
    def get_current_branch(self) -> Optional[str]:
        """Get current Git branch name"""
        try:
            result = subprocess.run(
                ['git', 'rev-parse', '--abbrev-ref', 'HEAD'],
                cwd=self.project_root,
                capture_output=True,
                text=True,
                check=True
            )
            return result.stdout.strip()
        except subprocess.CalledProcessError:
            logger.error("❌ Failed to get current branch. Are you in a Git repository?")
            return None
    
    def validate_branch_name(self, branch_name: str) -> Tuple[bool, str]:
        """
        Validate branch name against CLAUDE.md conventions
        Returns (is_valid, error_message)
        """
        if not branch_name:
            return False, "Branch name is empty"
        
        # Check if on forbidden branch
        if branch_name in self.FORBIDDEN_BRANCHES:
            return False, self._create_main_branch_error_message()
        
        # Check branch naming convention
        has_valid_prefix = any(
            branch_name.startswith(prefix) 
            for prefix in self.VALID_BRANCH_PREFIXES
        )
        
        if not has_valid_prefix:
            return False, self._create_naming_convention_error_message(branch_name)
        
        # Additional validation: ensure descriptive name after prefix
        parts = branch_name.split('/', 1)
        if len(parts) < 2 or len(parts[1]) < 3:
            return False, (
                f"❌ Branch name '{branch_name}' needs a more descriptive name after the prefix.\n"
                f"Example: 'feat/user-authentication' or 'fix/grid-rendering-bug'"
            )
        
        return True, ""
    
    def _create_main_branch_error_message(self) -> str:
        """Create comprehensive error message for main branch violations"""
        return """
🚨 CRITICAL ERROR: Working on main branch is FORBIDDEN!

As specified in CLAUDE.md Git Workflow Guide:
"NEVER WORK DIRECTLY ON MAIN BRANCH - NO EXCEPTIONS"

MANDATORY Git Workflow for ALL Changes:
1. Create feature branch FIRST: git checkout -b <type>/<description>
2. Make changes on branch: Never on main
3. Create Pull Request: Always for review
4. Wait for approval: Before merging

IMMEDIATE ACTION REQUIRED:
1. Stash your changes:     git stash
2. Create feature branch:  git checkout -b feat/your-feature-name
3. Apply your changes:     git stash pop
4. Continue development on the feature branch

Valid branch types: feat/, fix/, docs/, refactor/, test/, chore/, hotfix/

Example: git checkout -b feat/block-animation-system
"""
    
    def _create_naming_convention_error_message(self, branch_name: str) -> str:
        """Create error message for branch naming violations"""
        return f"""
❌ Invalid branch name: '{branch_name}'

Branch names must follow CLAUDE.md conventions:

REQUIRED FORMAT: <type>/<description>

Valid Types:
- feat/      New features
- fix/       Bug fixes  
- docs/      Documentation updates
- refactor/  Code refactoring
- test/      Adding tests
- chore/     Maintenance
- hotfix/    Emergency fixes

EXAMPLES:
✅ feat/move-block-animation
✅ fix/notification-pipeline-bug
✅ docs/update-architecture-guide
✅ refactor/clean-command-handlers

CREATE PROPER BRANCH:
git checkout -b feat/your-feature-description
"""
    
    def check_pre_commit_requirements(self) -> Tuple[bool, List[str]]:
        """
        Check all pre-commit requirements
        Returns (all_passed, error_messages)
        """
        errors = []
        
        # 1. Branch name validation
        current_branch = self.get_current_branch()
        if current_branch:
            is_valid, error_msg = self.validate_branch_name(current_branch)
            if not is_valid:
                errors.append(error_msg)
        else:
            errors.append("❌ Could not determine current branch")
        
        # 2. Check for staged changes (prevent empty commits)
        if not self._has_staged_changes():
            errors.append(
                "❌ No staged changes detected. Use 'git add <files>' to stage your changes."
            )
        
        # 3. Check if branch is up to date with main (optional warning)
        if current_branch and current_branch not in self.FORBIDDEN_BRANCHES:
            behind_count = self._get_commits_behind_main()
            if behind_count > 0:
                errors.append(
                    f"⚠️  Warning: Your branch is {behind_count} commits behind main. "
                    f"Consider rebasing: git rebase main"
                )
        
        return len(errors) == 0, errors
    
    def _has_staged_changes(self) -> bool:
        """Check if there are staged changes ready for commit"""
        try:
            result = subprocess.run(
                ['git', 'diff', '--cached', '--quiet'],
                cwd=self.project_root,
                capture_output=True
            )
            # Return code 1 means there are differences (staged changes)
            return result.returncode == 1
        except subprocess.CalledProcessError:
            return False
    
    def _get_commits_behind_main(self) -> int:
        """Get number of commits the current branch is behind main"""
        try:
            # Fetch latest main to ensure accurate comparison
            subprocess.run(
                ['git', 'fetch', 'origin', 'main'],
                cwd=self.project_root,
                capture_output=True,
                check=True
            )
            
            result = subprocess.run(
                ['git', 'rev-list', '--count', 'HEAD..origin/main'],
                cwd=self.project_root,
                capture_output=True,
                text=True,
                check=True
            )
            return int(result.stdout.strip())
        except (subprocess.CalledProcessError, ValueError):
            return 0
    
    def setup_pre_commit_hooks(self) -> bool:
        """
        Setup pre-commit hooks to automatically enforce workflow
        Returns True if successful
        """
        hooks_dir = self.git_dir / 'hooks'
        pre_commit_hook = hooks_dir / 'pre-commit'
        
        try:
            # Ensure hooks directory exists
            hooks_dir.mkdir(exist_ok=True)
            
            # Create pre-commit hook script
            hook_content = self._create_pre_commit_hook_script()
            
            pre_commit_hook.write_text(hook_content, encoding='utf-8')
            
            # Make hook executable (important on Unix-like systems)
            if hasattr(pre_commit_hook, 'chmod'):
                pre_commit_hook.chmod(0o755)
            
            logger.info(f"✅ Pre-commit hook installed: {pre_commit_hook}")
            logger.info("🔒 Git workflow enforcement is now active")
            return True
            
        except Exception as e:
            logger.error(f"❌ Failed to setup pre-commit hooks: {e}")
            return False
    
    def cleanup_after_merge(self) -> bool:
        """
        Clean up after PR merge to maintain clean git history
        Returns True if successful
        """
        try:
            current_branch = self.get_current_branch()
            if not current_branch:
                logger.error("❌ Could not determine current branch")
                return False
            
            if current_branch != "main":
                logger.info(f"📍 Currently on branch: {current_branch}")
                logger.info("🔄 Switching to main branch...")
                
                # Switch to main
                subprocess.run(
                    ['git', 'checkout', 'main'],
                    cwd=self.project_root,
                    check=True,
                    capture_output=True
                )
            
            logger.info("🔄 Fetching latest changes from origin...")
            
            # Fetch latest from origin
            subprocess.run(
                ['git', 'fetch', 'origin'],
                cwd=self.project_root,
                check=True,
                capture_output=True
            )
            
            # Check if local main has diverged from origin/main
            result = subprocess.run(
                ['git', 'rev-list', '--count', 'main..origin/main'],
                cwd=self.project_root,
                capture_output=True,
                text=True,
                check=True
            )
            commits_behind = int(result.stdout.strip())
            
            result = subprocess.run(
                ['git', 'rev-list', '--count', 'origin/main..main'],
                cwd=self.project_root,
                capture_output=True,
                text=True,
                check=True
            )
            commits_ahead = int(result.stdout.strip())
            
            if commits_ahead > 0:
                logger.warning(f"⚠️  Local main is {commits_ahead} commits ahead of origin/main")
                logger.warning("🧹 Performing clean reset to prevent messy merge history...")
                
                # Reset to clean origin state
                subprocess.run(
                    ['git', 'reset', '--hard', 'origin/main'],
                    cwd=self.project_root,
                    check=True,
                    capture_output=True
                )
                logger.info("✅ Reset to clean origin/main state")
            elif commits_behind > 0:
                logger.info(f"📥 Local main is {commits_behind} commits behind, pulling changes...")
                
                # Simple pull since we're not ahead
                subprocess.run(
                    ['git', 'pull', 'origin', 'main'],
                    cwd=self.project_root,
                    check=True,
                    capture_output=True
                )
                logger.info("✅ Successfully pulled latest changes")
            else:
                logger.info("✅ Local main is already up to date with origin/main")
            
            # Clean up remote references
            logger.info("🧹 Cleaning up stale remote references...")
            subprocess.run(
                ['git', 'remote', 'prune', 'origin'],
                cwd=self.project_root,
                check=True,
                capture_output=True
            )
            
            # Show current status
            logger.info("📊 Current git status:")
            result = subprocess.run(
                ['git', 'log', '--graph', '--oneline', '--decorate', '-5'],
                cwd=self.project_root,
                capture_output=True,
                text=True,
                check=True
            )
            print(result.stdout)
            
            logger.info("✅ Post-merge cleanup completed successfully!")
            logger.info("💡 Your git history is now clean and ready for the next development phase")
            return True
            
        except subprocess.CalledProcessError as e:
            logger.error(f"❌ Git operation failed: {e}")
            return False
        except Exception as e:
            logger.error(f"❌ Post-merge cleanup failed: {e}")
            return False

    def _create_pre_commit_hook_script(self) -> str:
        """Create the pre-commit hook script content"""
        return f'''#!/usr/bin/env python3
"""
Auto-generated pre-commit hook for BlockLife Git workflow enforcement
DO NOT EDIT - Regenerate using: python scripts/enforce_git_workflow.py --setup-hooks
"""

import sys
import subprocess
from pathlib import Path

# Add scripts directory to path
script_dir = Path(__file__).parent.parent / "scripts"
sys.path.insert(0, str(script_dir))

try:
    from enforce_git_workflow import GitWorkflowEnforcer
    
    project_root = Path(__file__).parent.parent.parent
    enforcer = GitWorkflowEnforcer(project_root)
    
    passed, errors = enforcer.check_pre_commit_requirements()
    
    if not passed:
        print("\\n".join(errors))
        sys.exit(1)
    
    print("✅ Git workflow validation passed")
    
except Exception as e:
    print(f"❌ Pre-commit hook error: {{e}}")
    # Don't block commit on hook errors, but log them
    import logging
    logging.error(f"Pre-commit hook failed: {{e}}")
'''

def main():
    """Main entry point following functional programming principles"""
    parser = argparse.ArgumentParser(
        description="Enforce BlockLife Git workflow requirements"
    )
    parser.add_argument(
        "--hook",
        choices=["pre-commit"],
        help="Run as a Git hook"
    )
    parser.add_argument(
        "--validate-branch",
        action="store_true",
        help="Validate current branch name"
    )
    parser.add_argument(
        "--setup-hooks",
        action="store_true", 
        help="Setup pre-commit hooks for automatic enforcement"
    )
    parser.add_argument(
        "--cleanup-after-merge",
        action="store_true",
        help="Clean up after PR merge to maintain clean git history"
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Enable verbose logging"
    )
    
    args = parser.parse_args()
    
    if args.verbose:
        logging.getLogger().setLevel(logging.DEBUG)
    
    # Determine project root
    script_dir = Path(__file__).parent
    project_root = script_dir.parent
    
    enforcer = GitWorkflowEnforcer(project_root)
    
    # Handle different modes
    if args.setup_hooks:
        logger.info("🔧 Setting up Git workflow enforcement hooks...")
        success = enforcer.setup_pre_commit_hooks()
        sys.exit(0 if success else 1)
    
    if args.cleanup_after_merge:
        logger.info("🧹 Starting post-merge cleanup to maintain clean git history...")
        success = enforcer.cleanup_after_merge()
        sys.exit(0 if success else 1)
    
    if args.validate_branch:
        current_branch = enforcer.get_current_branch()
        if current_branch:
            is_valid, error_msg = enforcer.validate_branch_name(current_branch)
            if is_valid:
                logger.info(f"✅ Branch name '{current_branch}' is valid")
            else:
                logger.error(error_msg)
                sys.exit(1)
        else:
            sys.exit(1)
    
    if args.hook == "pre-commit":
        passed, errors = enforcer.check_pre_commit_requirements()
        if not passed:
            for error in errors:
                print(error)
            sys.exit(1)
        else:
            logger.info("✅ Pre-commit validation passed")
    
    # Default: validate current state
    if not any([args.hook, args.validate_branch, args.setup_hooks]):
        current_branch = enforcer.get_current_branch()
        if current_branch:
            is_valid, error_msg = enforcer.validate_branch_name(current_branch)
            if is_valid:
                logger.info(f"✅ Current branch '{current_branch}' follows workflow requirements")
            else:
                logger.error(error_msg)
                logger.info("\n💡 TIP: Use --setup-hooks to automatically enforce these requirements")
                sys.exit(1)

if __name__ == "__main__":
    main()