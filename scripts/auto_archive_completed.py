#!/usr/bin/env python3
"""
Automatic Archive on 100% Completion Script
Automates the archival process for completed backlog items

This script monitors backlog items and automatically moves files to archive
when they reach 100% completion, following the established naming convention.

Usage: python scripts/auto_archive_completed.py [options]
"""

import os
import sys
import re
import shutil
import json
import logging
from pathlib import Path
from datetime import datetime, date
from typing import Dict, List, Tuple, Optional
import click
from rich.console import Console
from rich.progress import track

console = Console()
logger = logging.getLogger(__name__)

class AutoArchiver:
    """Automatically archives completed backlog items."""
    
    def __init__(self, base_path: str = "."):
        self.base_path = Path(base_path)
        self.backlog_path = self.base_path / "Docs" / "Backlog"
        self.items_path = self.backlog_path / "items"
        self.archive_path = self.backlog_path / "archive" / "completed"
        self.backlog_file = self.backlog_path / "Backlog.md"
        self.operation_log = []
        self.errors = []
        
        # Setup logging
        logging.basicConfig(
            level=logging.INFO,
            format='%(asctime)s - %(levelname)s - %(message)s'
        )
    
    def find_completed_items(self) -> List[Dict[str, str]]:
        """Find all items marked as 100% complete in Backlog.md."""
        
        if not self.backlog_file.exists():
            self.errors.append("Backlog.md not found")
            return []
        
        content = self.backlog_file.read_text()
        completed_items = []
        
        # Regex to match backlog table rows with 100% progress
        # Pattern: | P# | [ID](path) | Type | Title | Status | 100% | Complexity | Notes |
        pattern = r'\|\s*P[0-5]\s*\|\s*\[([A-Z]{2}_\d{3})\]\(([^)]+)\)[^|]*\|[^|]*\|[^|]*\|[^|]*\|\s*100%\s*\|'
        
        matches = re.finditer(pattern, content)
        for match in matches:
            item_id = match.group(1)
            file_path = match.group(2)
            
            # Skip if already archived (path contains 'archive')
            if 'archive' in file_path.lower():
                continue
                
            completed_items.append({
                'id': item_id,
                'current_path': file_path,
                'full_path': self.backlog_path / file_path
            })
        
        logger.info(f"Found {len(completed_items)} items ready for archival")
        return completed_items
    
    def gather_item_metadata(self, item_path: Path) -> Dict[str, str]:
        """Gather metadata needed for archive naming convention."""
        
        metadata = {
            'completion_date': None,
            'priority': 'P3',  # Default
            'tags': []
        }
        
        # Get file modification date (completion date)
        if item_path.exists():
            mod_time = datetime.fromtimestamp(item_path.stat().st_mtime)
            metadata['completion_date'] = mod_time.strftime('%Y_%m_%d')
        else:
            metadata['completion_date'] = datetime.now().strftime('%Y_%m_%d')
        
        # Extract priority from Backlog.md
        if self.backlog_file.exists():
            content = self.backlog_file.read_text()
            item_id = item_path.stem.split('_')[0] + '_' + item_path.stem.split('_')[1]
            priority_match = re.search(rf'\|\s*(P[0-5])\s*\|\s*\[{item_id}\]', content)
            if priority_match:
                metadata['priority'] = priority_match.group(1)
        
        # Determine tags from file content and metadata
        metadata['tags'] = self.determine_tags(item_path, metadata['priority'])
        
        return metadata
    
    def determine_tags(self, item_path: Path, priority: str) -> List[str]:
        """Determine archive tags based on file content and metadata."""
        
        tags = []
        filename = item_path.name
        
        # Type tags (from item prefix)
        if filename.startswith('BF_'):
            tags.append('[bug]')
        elif filename.startswith('VS_'):
            tags.append('[feature]')
        elif filename.startswith('TD_'):
            # Check content to determine if it's refactor or test
            if item_path.exists():
                content = item_path.read_text()
                if any(word in content.lower() for word in ['test', 'testing', 'verification']):
                    tags.append('[test]')
                else:
                    tags.append('[refactor]')
            else:
                tags.append('[refactor]')
        elif filename.startswith('HF_'):
            tags.append('[bug]')
            tags.append('[critical]')
        
        # Impact tags (from priority and content)
        if priority == 'P0':
            tags.append('[critical]')
        
        if item_path.exists():
            content = item_path.read_text().lower()
            if 'data loss' in content or 'dataloss' in content:
                tags.append('[dataloss]')
            if 'security' in content:
                tags.append('[security]')
            if 'breaking' in content:
                tags.append('[breaking]')
        
        # Area tags (from title/content)
        title_lower = filename.lower()
        if any(word in title_lower for word in ['archive', 'file', 'move']):
            tags.append('[fileops]')
        elif any(word in title_lower for word in ['ui', 'interface', 'view']):
            tags.append('[ui]')
        elif any(word in title_lower for word in ['core', 'engine', 'system']):
            tags.append('[core]')
        elif any(word in title_lower for word in ['workflow', 'process']):
            tags.append('[workflow]')
        elif any(word in title_lower for word in ['agent', 'ai']):
            tags.append('[agent]')
        
        # Detail tags
        if any(word in title_lower for word in ['automation', 'auto']):
            tags.append('[automation]')
        if any(word in title_lower for word in ['pattern', 'framework']):
            tags.append('[pattern]')
        
        # Status tag (always last)
        if filename.startswith(('BF_', 'HF_')):
            tags.append('[resolved]')
        else:
            tags.append('[completed]')
        
        return tags
    
    def generate_archive_filename(self, item_path: Path, metadata: Dict[str, str]) -> str:
        """Generate archive filename following naming convention."""
        
        # Extract components
        parts = item_path.stem.split('_')
        item_type = parts[0]
        item_number = parts[1]
        item_id = f"{item_type}_{item_number}"
        
        # Convert title to kebab-case (take 2-4 words)
        title_parts = parts[2:]
        if len(title_parts) > 4:
            title_parts = title_parts[:4]
        elif len(title_parts) < 2:
            title_parts = title_parts + ['item']
        
        description = '-'.join(word.lower() for word in title_parts)
        
        # Format tags
        tags_str = ''.join(metadata['tags'])
        
        # Format: YYYY_MM_DD-[TYPE_ID]-description-[tag1][tag2][...].md
        return f"{metadata['completion_date']}-{item_id}-{description}-{tags_str}.md"
    
    def get_archive_directory(self, completion_date: str) -> Path:
        """Get the archive directory based on completion date."""
        
        # Parse date and determine quarter
        year, month, day = completion_date.split('_')
        month_int = int(month)
        quarter = (month_int - 1) // 3 + 1
        
        quarter_dir = f"{year}-Q{quarter}"
        archive_dir = self.archive_path / quarter_dir
        
        return archive_dir
    
    def verify_file_operation(self, operation_type: str, source_path: Path, dest_path: Path = None) -> bool:
        """Verify file operations completed successfully."""
        
        import time
        time.sleep(1)  # Allow file system operations to complete
        
        if operation_type == "MOVE":
            if not dest_path.exists():
                self.errors.append(f"Destination {dest_path} not found after move")
                return False
            if source_path.exists():
                self.errors.append(f"Source {source_path} still exists after move")
                return False
            if dest_path.stat().st_size == 0:
                self.errors.append(f"Destination {dest_path} is empty")
                return False
            
            logger.info(f"✅ File verified at destination: {dest_path}")
            return True
        
        elif operation_type == "CREATE_DIR":
            if not source_path.exists():
                self.errors.append(f"Directory {source_path} not created")
                return False
            return True
        
        return False
    
    def archive_item(self, item: Dict[str, str]) -> bool:
        """Archive a single completed item."""
        
        item_id = item['id']
        source_path = Path(item['full_path'])
        
        console.print(f"📦 Archiving {item_id}...")
        
        # Verify source file exists
        if not source_path.exists():
            error_msg = f"Source file not found: {source_path}"
            self.errors.append(error_msg)
            logger.error(error_msg)
            return False
        
        # Gather metadata
        metadata = self.gather_item_metadata(source_path)
        
        # Generate archive filename
        archive_filename = self.generate_archive_filename(source_path, metadata)
        
        # Determine archive directory
        archive_dir = self.get_archive_directory(metadata['completion_date'])
        
        # Create archive directory if needed
        archive_dir.mkdir(parents=True, exist_ok=True)
        if not self.verify_file_operation("CREATE_DIR", archive_dir):
            return False
        
        # Full destination path
        dest_path = archive_dir / archive_filename
        
        # Check if destination already exists
        if dest_path.exists():
            error_msg = f"Destination already exists: {dest_path}"
            self.errors.append(error_msg)
            logger.error(error_msg)
            return False
        
        try:
            # Perform the move operation
            logger.info(f"Moving {source_path} → {dest_path}")
            shutil.move(str(source_path), str(dest_path))
            
            # Verify move completed successfully
            if not self.verify_file_operation("MOVE", source_path, dest_path):
                # Try to restore if possible
                if dest_path.exists() and not source_path.exists():
                    shutil.move(str(dest_path), str(source_path))
                    logger.error(f"Restored {source_path} due to verification failure")
                return False
            
            # Update Backlog.md
            relative_archive_path = f"archive/completed/{archive_dir.name}/{archive_filename}"
            if self.update_backlog_reference(item_id, relative_archive_path):
                self.operation_log.append({
                    'item_id': item_id,
                    'source': str(source_path),
                    'destination': str(dest_path),
                    'archive_path': relative_archive_path,
                    'timestamp': datetime.now().isoformat()
                })
                
                console.print(f"✅ {item_id} archived as {archive_filename}")
                logger.info(f"✅ {item_id} successfully archived")
                return True
            else:
                # Backlog update failed, restore file
                shutil.move(str(dest_path), str(source_path))
                logger.error(f"Restored {source_path} due to backlog update failure")
                return False
        
        except Exception as e:
            error_msg = f"Failed to archive {item_id}: {e}"
            self.errors.append(error_msg)
            logger.error(error_msg)
            return False
    
    def update_backlog_reference(self, item_id: str, new_path: str) -> bool:
        """Update the item reference in Backlog.md to point to archive."""
        
        try:
            content = self.backlog_file.read_text()
            
            # Find and update the item link
            pattern = rf'(\[{item_id}\])\(([^)]+)\)'
            
            def replace_link(match):
                return f"{match.group(1)}({new_path})"
            
            updated_content = re.sub(pattern, replace_link, content)
            
            # Also update status to indicate archived
            row_pattern = rf'(\|\s*P[0-5]\s*\|\s*\[{item_id}\]\([^)]+\)[^|]*\|[^|]*\|[^|]*\|\s*)([^|]+)(\s*\|\s*100%\s*\|)'
            
            def replace_status(match):
                return f"{match.group(1)}📦 ARCHIVED{match.group(3)}"
            
            updated_content = re.sub(row_pattern, replace_status, updated_content)
            
            # Write back to file
            self.backlog_file.write_text(updated_content)
            logger.info(f"Updated Backlog.md reference for {item_id}")
            return True
        
        except Exception as e:
            error_msg = f"Failed to update Backlog.md for {item_id}: {e}"
            self.errors.append(error_msg)
            logger.error(error_msg)
            return False
    
    def run_auto_archive(self, dry_run: bool = False) -> Dict[str, any]:
        """Run the automatic archival process."""
        
        console.print("[bold green]🤖 Starting Automatic Archive Process...[/]")
        
        results = {
            'timestamp': datetime.now().isoformat(),
            'dry_run': dry_run,
            'items_processed': 0,
            'items_archived': 0,
            'items_failed': 0,
            'errors': [],
            'operations': []
        }
        
        # Find completed items
        completed_items = self.find_completed_items()
        results['items_processed'] = len(completed_items)
        
        if not completed_items:
            console.print("ℹ️  No items ready for archival")
            return results
        
        console.print(f"Found {len(completed_items)} items ready for archival")
        
        # Process each item
        for item in track(completed_items, description="Archiving items..."):
            if dry_run:
                console.print(f"[yellow]Would archive: {item['id']}[/]")
                results['items_archived'] += 1
            else:
                if self.archive_item(item):
                    results['items_archived'] += 1
                else:
                    results['items_failed'] += 1
        
        # Collect results
        results['errors'] = self.errors
        results['operations'] = self.operation_log
        
        # Summary
        if dry_run:
            console.print(f"[yellow]DRY RUN: Would archive {results['items_archived']} items[/]")
        else:
            if results['items_failed'] == 0:
                console.print(f"[bold green]✅ Successfully archived {results['items_archived']} items![/]")
            else:
                console.print(f"[bold yellow]⚠️  Archived {results['items_archived']}, failed {results['items_failed']}[/]")
        
        return results
    
    def save_operation_log(self, results: Dict[str, any]):
        """Save operation log for audit trail."""
        
        log_file = Path("scripts") / "auto_archive_log.json"
        
        # Load existing log or create new
        if log_file.exists():
            with open(log_file) as f:
                log_data = json.load(f)
        else:
            log_data = {'runs': []}
        
        # Add this run
        log_data['runs'].append(results)
        
        # Keep only last 50 runs
        log_data['runs'] = log_data['runs'][-50:]
        
        # Save updated log
        with open(log_file, 'w') as f:
            json.dump(log_data, f, indent=2)
        
        console.print(f"📝 Operation log saved to: {log_file}")

@click.command()
@click.option('--dry-run', is_flag=True, help='Preview archival without executing')
@click.option('--verbose', is_flag=True, help='Detailed output')
@click.option('--force', is_flag=True, help='Force archival even if errors detected')
def main(dry_run, verbose, force):
    """Automatically archive completed backlog items."""
    
    # Setup logging level
    if verbose:
        logging.getLogger().setLevel(logging.DEBUG)
    
    try:
        archiver = AutoArchiver()
        
        # Run the archival process
        results = archiver.run_auto_archive(dry_run=dry_run)
        
        # Save operation log
        if not dry_run:
            archiver.save_operation_log(results)
        
        # Print summary
        if verbose or results['errors']:
            console.print("\n📊 Detailed Results:")
            console.print(f"  Items processed: {results['items_processed']}")
            console.print(f"  Items archived: {results['items_archived']}")
            console.print(f"  Items failed: {results['items_failed']}")
            
            if results['errors']:
                console.print("\n❌ Errors:")
                for error in results['errors']:
                    console.print(f"  • {error}")
        
        # Exit code
        if results['items_failed'] > 0 and not force:
            console.print("\n⚠️  Some items failed to archive. Use --force to ignore errors.")
            return 1
        
        if not dry_run and results['items_archived'] > 0:
            console.print("\n🎯 Time Saved: ~5-10 minutes of manual archival work per item")
        
        return 0
    
    except Exception as e:
        console.print(f"[bold red]❌ Fatal error: {e}[/]")
        logger.exception("Auto-archive failed")
        return 1

if __name__ == "__main__":
    sys.exit(main())