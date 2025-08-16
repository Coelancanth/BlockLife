#!/usr/bin/env python3
"""
Documentation Status Sync Script for BlockLife

This script automatically synchronizes documentation status across all tracking files
to reduce cognitive load and ensure consistency in the comprehensive documentation system.

Features:
- Syncs action item status from Master_Action_Items.md
- Updates implementation progress in Implementation_Status_Tracker.md
- Validates documentation links and references
- Maintains consistency across all documentation files
- Provides structured logging for integration with CI/CD

Usage:
    python scripts/sync_documentation_status.py [--dry-run] [--verbose]
    python scripts/sync_documentation_status.py --check-links
    python scripts/sync_documentation_status.py --update-all
"""

import re
import json
import sys
import argparse
from pathlib import Path
from typing import Dict, List, Optional, Tuple, Set
from dataclasses import dataclass
from datetime import datetime
import logging

# Configure logging for cognitive load reduction with Unicode support
def setup_logging():
    import sys
    import codecs
    
    # Setup UTF-8 output for Windows
    if sys.platform == 'win32':
        sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')
        sys.stderr = codecs.getwriter('utf-8')(sys.stderr.buffer, 'strict')
    
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(levelname)s - %(message)s',
        handlers=[
            logging.StreamHandler(sys.stdout),
            logging.FileHandler('scripts/doc_sync.log', mode='a', encoding='utf-8')
        ]
    )

setup_logging()
logger = logging.getLogger(__name__)

@dataclass
class ActionItem:
    """Action item data structure following functional programming principles"""
    id: str
    status: str  # COMPLETED, PENDING, IN_PROGRESS
    title: str
    source: str
    date: str
    notes: str

@dataclass
class ImplementationPlan:
    """Implementation plan status tracking"""
    name: str
    file_path: str
    status: str  # COMPLETED, IN_PROGRESS, NOT_STARTED, REFERENCE
    progress: int  # Percentage
    next_action: str
    last_updated: str

@dataclass
class DocumentationStatus:
    """Overall documentation status"""
    total_action_items: int
    completed_action_items: int
    total_implementation_plans: int
    completed_implementation_plans: int
    in_progress_implementation_plans: int
    broken_links: List[str]
    last_sync: str

class DocumentationStatusSync:
    """
    Synchronizes documentation status using functional programming patterns
    Follows Clean Architecture principles with clear separation of concerns
    """
    
    def __init__(self, project_root: Path):
        self.project_root = project_root
        self.docs_path = project_root / "Docs"
        self.backlog_path = self.docs_path / "Backlog"
        
    def sync_all_documentation(self, dry_run: bool = False) -> bool:
        """
        Synchronize all documentation status files
        Returns True if successful, follows functional error handling
        """
        logger.info("🔄 Starting comprehensive documentation synchronization...")
        
        try:
            # 1. Parse current action items
            action_items = self._parse_action_items()
            logger.info(f"📋 Found {len(action_items)} action items")
            
            # 2. Parse implementation plans
            impl_plans = self._parse_implementation_plans()
            logger.info(f"📊 Found {len(impl_plans)} implementation plans")
            
            # 3. Check documentation links
            broken_links = self._check_documentation_links()
            if broken_links:
                logger.warning(f"🔗 Found {len(broken_links)} broken links")
            
            # 4. Create status summary
            status = self._create_status_summary(action_items, impl_plans, broken_links)
            
            # 5. Update tracking files
            if not dry_run:
                self._update_master_action_items(action_items, status)
                self._update_implementation_status_tracker(impl_plans, status)
                self._update_documentation_catalogue(status)
            else:
                logger.info("🔄 DRY RUN: Would update tracking files")
            
            logger.info("✅ Documentation synchronization completed successfully")
            return True
            
        except Exception as e:
            logger.error(f"❌ Documentation synchronization failed: {e}")
            return False

    def _parse_action_items(self) -> List[ActionItem]:
        """Parse action items from current backlog structure"""
        backlog_file = self.backlog_path / "Backlog.md"
        
        if not backlog_file.exists():
            logger.warning(f"❌ Backlog file not found: {backlog_file}")
            return []
        
        content = backlog_file.read_text(encoding='utf-8')
        action_items = []
        
        # Parse table rows from action items
        table_pattern = r'\| \*\*(.*?)\*\* \| (.*?) \| (.*?) \| \[(.*?)\]\((.*?)\) \| (.*?) \| (.*?) \|'
        
        for match in re.finditer(table_pattern, content):
            item_id, status, title, source_text, source_link, date, notes = match.groups()
            
            # Clean up status (remove emoji and markdown)
            clean_status = re.sub(r'[✅📋🔄]|\*\*', '', status).strip()
            
            action_items.append(ActionItem(
                id=item_id.strip(),
                status=clean_status,
                title=title.strip(),
                source=f"{source_text}({source_link})",
                date=date.strip(),
                notes=notes.strip()
            ))
        
        return action_items

    def _parse_implementation_plans(self) -> List[ImplementationPlan]:
        """Parse implementation plans from current backlog structure"""
        # Implementation plans are now embedded in VS items in Backlog/items/
        items_dir = self.backlog_path / "items"
        
        if not items_dir.exists():
            logger.warning(f"❌ Backlog items directory not found: {items_dir}")
            return []
        
        plans = []
        
        # Parse VS items (Vertical Slice items contain implementation plans)
        vs_files = list(items_dir.glob("VS_*.md"))
        
        for vs_file in vs_files:
            try:
                content = vs_file.read_text(encoding='utf-8')
                vs_name = vs_file.stem.replace('VS_', '').replace('_', ' ')
                
                # Simple status extraction based on content
                if "✅ COMPLETED" in content or "Phase 1 COMPLETED" in content:
                    status = "COMPLETED"
                    progress = 100
                elif "🚧 IN PROGRESS" in content or "ACTIVE" in content:
                    status = "IN_PROGRESS"
                    progress = 50  # Default progress for in-progress items
                else:
                    status = "NOT_STARTED"
                    progress = 0
                
                plans.append(ImplementationPlan(
                    name=vs_name,
                    file_path=str(vs_file.relative_to(self.project_root)),
                    status=status,
                    progress=progress,
                    next_action="See VS item for details",
                    last_updated=datetime.now().strftime('%Y-%m-%d')
                ))
            except Exception as e:
                logger.warning(f"⚠️ Could not parse VS file {vs_file}: {e}")
        
        return plans

    def _check_documentation_links(self) -> List[str]:
        """Check for broken internal documentation links"""
        broken_links = []
        
        # Get all markdown files in docs directory
        markdown_files = list(self.docs_path.rglob("*.md"))
        
        for md_file in markdown_files:
            try:
                content = md_file.read_text(encoding='utf-8')
                
                # Find all internal links [text](path)
                link_pattern = r'\[([^\]]+)\]\(([^)]+)\)'
                
                for match in re.finditer(link_pattern, content):
                    link_text, link_path = match.groups()
                    
                    # Skip external links
                    if link_path.startswith(('http', 'https', 'mailto')):
                        continue
                    
                    # Convert relative path to absolute
                    if link_path.startswith('#'):
                        continue  # Skip anchor links
                    
                    target_path = md_file.parent / link_path
                    
                    if not target_path.exists():
                        broken_links.append(f"{md_file.relative_to(self.docs_path)}: {link_path}")
                        
            except Exception as e:
                logger.warning(f"⚠️ Could not check links in {md_file}: {e}")
        
        return broken_links

    def _create_status_summary(self, action_items: List[ActionItem], 
                             impl_plans: List[ImplementationPlan],
                             broken_links: List[str]) -> DocumentationStatus:
        """Create comprehensive status summary"""
        
        completed_actions = sum(1 for item in action_items if item.status == "COMPLETED")
        completed_plans = sum(1 for plan in impl_plans if plan.status in ["COMPLETED", "REFERENCE IMPLEMENTATION"])
        in_progress_plans = sum(1 for plan in impl_plans if plan.status == "IN PROGRESS")
        
        return DocumentationStatus(
            total_action_items=len(action_items),
            completed_action_items=completed_actions,
            total_implementation_plans=len(impl_plans),
            completed_implementation_plans=completed_plans,
            in_progress_implementation_plans=in_progress_plans,
            broken_links=broken_links,
            last_sync=datetime.now().isoformat()
        )

    def _update_master_action_items(self, action_items: List[ActionItem], 
                                  status: DocumentationStatus):
        """Update the backlog with current statistics"""
        backlog_file = self.backlog_path / "Backlog.md"
        
        if not backlog_file.exists():
            logger.warning(f"❌ Backlog file not found: {backlog_file}")
            return
            
        # For now, just log the statistics - the new backlog structure is maintained differently
        completion_rate = (status.completed_action_items / status.total_action_items * 100) if status.total_action_items > 0 else 0
        logger.info(f"📝 Backlog statistics: {completion_rate:.0f}% completion rate ({status.completed_action_items}/{status.total_action_items})")

    def _update_implementation_status_tracker(self, impl_plans: List[ImplementationPlan],
                                            status: DocumentationStatus):
        """Log implementation status metrics"""
        # Implementation status is now tracked in individual VS items
        planned_features = status.total_implementation_plans - status.completed_implementation_plans - status.in_progress_implementation_plans
        
        logger.info(f"📊 Implementation status: {status.completed_implementation_plans} completed, {status.in_progress_implementation_plans} in progress, {planned_features} planned")

    def _update_documentation_catalogue(self, status: DocumentationStatus):
        """Update documentation catalogue with sync status"""
        catalogue_file = self.docs_path / "DOCUMENTATION_CATALOGUE.md"
        content = catalogue_file.read_text(encoding='utf-8')
        
        # Update maintenance notes with last sync time
        maintenance_pattern = r'\*\*Last Updated\*\*: [^\n]+'
        new_maintenance = f"**Last Updated**: {datetime.now().strftime('%Y-%m-%d')} (Auto-synced)"
        
        updated_content = re.sub(maintenance_pattern, new_maintenance, content)
        catalogue_file.write_text(updated_content, encoding='utf-8')
        
        logger.info("📚 Updated documentation catalogue with sync timestamp")

    def generate_status_report(self) -> Dict:
        """Generate comprehensive status report for CI/CD integration"""
        try:
            action_items = self._parse_action_items()
            impl_plans = self._parse_implementation_plans()
            broken_links = self._check_documentation_links()
            status = self._create_status_summary(action_items, impl_plans, broken_links)
            
            return {
                "status": "success",
                "timestamp": status.last_sync,
                "action_items": {
                    "total": status.total_action_items,
                    "completed": status.completed_action_items,
                    "completion_rate": (status.completed_action_items / status.total_action_items * 100) if status.total_action_items > 0 else 100
                },
                "implementation_plans": {
                    "total": status.total_implementation_plans,
                    "completed": status.completed_implementation_plans,
                    "in_progress": status.in_progress_implementation_plans
                },
                "documentation_health": {
                    "broken_links": len(status.broken_links),
                    "broken_link_details": status.broken_links
                }
            }
        except Exception as e:
            return {
                "status": "error",
                "error": str(e),
                "timestamp": datetime.now().isoformat()
            }

def main():
    """Main entry point following functional programming principles"""
    parser = argparse.ArgumentParser(
        description="Synchronize BlockLife documentation status"
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Run in dry-run mode (no actual changes)"
    )
    parser.add_argument(
        "--check-links",
        action="store_true",
        help="Only check for broken documentation links"
    )
    parser.add_argument(
        "--update-all",
        action="store_true",
        help="Force update all documentation files"
    )
    parser.add_argument(
        "--report",
        action="store_true",
        help="Generate status report in JSON format"
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
    
    logger.info("🔄 BlockLife Documentation Status Sync Starting...")
    logger.info(f"📂 Project root: {project_root}")
    
    if args.dry_run:
        logger.info("🔄 Running in DRY RUN mode - no changes will be made")
    
    # Initialize sync manager
    sync_manager = DocumentationStatusSync(project_root)
    
    # Handle different modes
    if args.report:
        report = sync_manager.generate_status_report()
        print(json.dumps(report, indent=2))
        return
    
    if args.check_links:
        logger.info("🔗 Checking documentation links...")
        broken_links = sync_manager._check_documentation_links()
        if broken_links:
            logger.error(f"❌ Found {len(broken_links)} broken links:")
            for link in broken_links:
                logger.error(f"   {link}")
            sys.exit(1)
        else:
            logger.info("✅ All documentation links are valid")
        return
    
    # Default: sync all documentation
    success = sync_manager.sync_all_documentation(dry_run=args.dry_run)
    
    if not success:
        logger.error("❌ Documentation synchronization failed")
        sys.exit(1)
    
    logger.info("✅ Documentation synchronization completed successfully")

if __name__ == "__main__":
    main()