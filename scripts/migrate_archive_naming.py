#!/usr/bin/env python3
"""
Archive Naming Convention Migration Script

Migrates archived work items from old naming format to the new standardized format:
Old: TD_012_Dynamic_PO_Pattern_Implementation.md  
New: YYYY_MM_DD-[TYPE_ID]-description-[tag1][tag2][status].md

Features:
- Detects old format files and skips already migrated ones
- Intelligent tag determination based on file type and content
- Comprehensive migration logging with before/after tracking
- Dry-run mode for safe previewing
- Reusable for future archive migrations

Usage:
    python scripts/migrate_archive_naming.py --help
    python scripts/migrate_archive_naming.py --dry-run /path/to/archive
    python scripts/migrate_archive_naming.py /path/to/archive --date 2025-01-15

Author: DevOps Engineer  
Purpose: Complete TD_020 Archive Naming Convention Implementation
"""

import sys
import os
import logging
import re
from pathlib import Path
from datetime import datetime, date
from typing import List, Dict, Optional, Tuple
import click
from rich.console import Console
from rich.table import Table
from rich.progress import track
from rich.panel import Panel
from rich import print as rprint

# Setup rich console for beautiful output
console = Console()

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('scripts/migration.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

class ArchiveMigrator:
    """Handles migration of archive files to new naming convention."""
    
    def __init__(self, dry_run: bool = False):
        self.dry_run = dry_run
        self.migration_log = []
        self.errors = []
        
        # Tag priority mapping for consistent ordering
        self.tag_priority = {
            # Type (1)
            'bug': 1, 'feature': 1, 'refactor': 1, 'docs': 1, 'test': 1,
            # Impact (2)  
            'critical': 2, 'dataloss': 2, 'security': 2, 'breaking': 2,
            # Area (3)
            'fileops': 3, 'ui': 3, 'core': 3, 'workflow': 3, 'agent': 3,
            # Details (4-98)
            'automation': 4, 'pattern': 4, 'framework': 4, 'migration': 4,
            # Status (99)
            'resolved': 99, 'completed': 99, 'partial': 99, 'deprecated': 99
        }
        
        # Work item type mapping
        self.type_mapping = {
            'TD': 'refactor',  # Tech Debt -> refactor
            'BF': 'bug',       # Bug Fix -> bug  
            'VS': 'feature',   # Vertical Slice -> feature
            'HF': 'bug'        # Hotfix -> bug
        }
        
    def is_old_format(self, filename: str) -> bool:
        """Check if filename uses old naming convention."""
        # Old format: TD_012_Dynamic_PO_Pattern_Implementation.md
        pattern = r'^(TD|BF|VS|HF)_\d+_[A-Za-z_]+\.md$'
        return bool(re.match(pattern, filename))
    
    def is_new_format(self, filename: str) -> bool:
        """Check if filename already uses new naming convention."""
        # New format: 2025_01_15-TD_012-description-[tag1][tag2].md
        pattern = r'^\d{4}_\d{2}_\d{2}-(TD|BF|VS|HF)_\d+-[a-z-]+-(\[[a-z]+\])+\.md$'
        return bool(re.match(pattern, filename))
    
    def extract_old_components(self, filename: str) -> Dict[str, str]:
        """Extract components from old format filename."""
        # Remove .md extension
        base = filename.replace('.md', '')
        parts = base.split('_')
        
        if len(parts) < 3:
            raise ValueError(f"Invalid old format: {filename}")
            
        work_type = parts[0]
        work_id = parts[1]
        description_parts = parts[2:]
        
        return {
            'type': work_type,
            'id': work_id,
            'full_id': f"{work_type}_{work_id}",
            'description': '_'.join(description_parts)
        }
    
    def determine_tags(self, components: Dict[str, str], file_path: Path, completion_date: date) -> List[str]:
        """Intelligently determine tags based on file type and content analysis."""
        tags = []
        
        # 1. Type tag (required, priority 1)
        work_type = components['type']
        if work_type in self.type_mapping:
            tags.append(self.type_mapping[work_type])
        else:
            logger.warning(f"Unknown work type: {work_type}, defaulting to 'refactor'")
            tags.append('refactor')
        
        # 2. Impact tags (priority 2) - analyze content if file exists
        try:
            if file_path.exists():
                content = file_path.read_text(encoding='utf-8').lower()
                
                # Check for critical indicators
                critical_indicators = [
                    'critical', 'urgent', 'blocking', 'crash', 'failure', 
                    'memory leak', 'deadlock', 'race condition', 'thread safety'
                ]
                if any(indicator in content for indicator in critical_indicators):
                    tags.append('critical')
                
                # Check for data loss indicators
                dataloss_indicators = [
                    'data loss', 'corruption', 'overwrite', 'delete', 'lost data',
                    'file overwrite', 'data corruption'
                ]
                if any(indicator in content for indicator in dataloss_indicators):
                    tags.append('dataloss')
                    
                # Check for security indicators
                security_indicators = ['security', 'vulnerability', 'exploit', 'auth']
                if any(indicator in content for indicator in security_indicators):
                    tags.append('security')
                    
        except Exception as e:
            logger.warning(f"Could not analyze file content for {file_path}: {e}")
        
        # 3. Area tags (priority 3) - based on description and content
        description = components['description'].lower()
        
        # File operations
        if any(term in description for term in ['file', 'archive', 'path', 'folder', 'directory']):
            tags.append('fileops')
        # UI related
        elif any(term in description for term in ['ui', 'interface', 'display', 'view']):
            tags.append('ui')
        # Core system
        elif any(term in description for term in ['core', 'engine', 'system', 'grid', 'state']):
            tags.append('core')
        # Workflow  
        elif any(term in description for term in ['workflow', 'process', 'pattern', 'orchestration']):
            tags.append('workflow')
        # Agent related
        elif any(term in description for term in ['agent', 'po', 'maintainer', 'trigger']):
            tags.append('agent')
        
        # 4. Detail tags (priority 4-98) - based on description
        if any(term in description for term in ['automation', 'script', 'pipeline']):
            tags.append('automation')
        if any(term in description for term in ['pattern', 'architecture', 'design']):
            tags.append('pattern')
        if any(term in description for term in ['framework', 'infrastructure']):
            tags.append('framework')
        if any(term in description for term in ['migration', 'move', 'reorganization']):
            tags.append('migration')
        
        # 5. Status tag (required, priority 99)
        # Default to completed for tech debt and features, resolved for bugs
        if work_type in ['BF', 'HF']:
            tags.append('resolved')
        else:
            tags.append('completed')
        
        # Remove duplicates while preserving order
        seen = set()
        unique_tags = []
        for tag in tags:
            if tag not in seen:
                seen.add(tag)
                unique_tags.append(tag)
        
        return unique_tags
    
    def create_description(self, original_description: str) -> str:
        """Convert original description to kebab-case format."""
        # Convert underscores to hyphens and make lowercase
        description = original_description.replace('_', '-').lower()
        
        # Remove common words and shorten if needed
        replacements = {
            'implementation': 'impl',
            'development': 'dev',
            'application': 'app',
            'documentation': 'docs',
            'verification': 'verify',
            'optimization': 'opt',
            'configuration': 'config'
        }
        
        for old, new in replacements.items():
            description = description.replace(old, new)
        
        # Limit to reasonable length (max 4 words)
        parts = description.split('-')
        if len(parts) > 4:
            description = '-'.join(parts[:4])
        
        return description
    
    def build_new_filename(self, components: Dict[str, str], completion_date: date, 
                          tags: List[str]) -> str:
        """Build new filename according to convention."""
        
        # Sort tags by priority
        sorted_tags = sorted(tags, key=lambda t: (self.tag_priority.get(t, 50), t))
        
        # Format components
        date_str = completion_date.strftime('%Y_%m_%d')
        item_id = components['full_id']
        description = self.create_description(components['description'])
        tag_string = ''.join(f'[{tag}]' for tag in sorted_tags)
        
        return f"{date_str}-{item_id}-{description}-{tag_string}.md"
    
    def migrate_file(self, file_path: Path, completion_date: date) -> Tuple[bool, str]:
        """Migrate a single file to new naming convention."""
        
        try:
            old_filename = file_path.name
            
            # Skip if already in new format
            if self.is_new_format(old_filename):
                return True, f"Already migrated: {old_filename}"
            
            # Skip if not in old format (shouldn't migrate)
            if not self.is_old_format(old_filename):
                return True, f"Not old format, skipping: {old_filename}"
            
            # Extract components
            components = self.extract_old_components(old_filename)
            
            # Determine tags
            tags = self.determine_tags(components, file_path, completion_date)
            
            # Build new filename
            new_filename = self.build_new_filename(components, completion_date, tags)
            new_path = file_path.parent / new_filename
            
            # Log the migration
            migration_info = {
                'old_name': old_filename,
                'new_name': new_filename,
                'path': str(file_path.parent),
                'components': components,
                'tags': tags,
                'date': completion_date.isoformat()
            }
            self.migration_log.append(migration_info)
            
            # Perform the migration (unless dry run)
            if not self.dry_run:
                file_path.rename(new_path)
                logger.info(f"Migrated: {old_filename} -> {new_filename}")
            else:
                logger.info(f"[DRY RUN] Would migrate: {old_filename} -> {new_filename}")
            
            return True, f"Migrated: {old_filename} -> {new_filename}"
            
        except Exception as e:
            error_msg = f"Failed to migrate {file_path.name}: {str(e)}"
            self.errors.append(error_msg)
            logger.error(error_msg)
            return False, error_msg
    
    def migrate_directory(self, directory: Path, completion_date: date) -> Dict[str, int]:
        """Migrate all eligible files in a directory."""
        
        if not directory.exists():
            raise ValueError(f"Directory does not exist: {directory}")
        
        # Find all markdown files
        md_files = list(directory.glob("*.md"))
        
        results = {
            'total_files': len(md_files),
            'migrated': 0,
            'skipped': 0, 
            'errors': 0
        }
        
        console.print(f"\n📁 Processing directory: {directory}")
        console.print(f"Found {len(md_files)} markdown files")
        
        for file_path in track(md_files, description="Migrating files..."):
            success, message = self.migrate_file(file_path, completion_date)
            
            if success:
                if "Migrated:" in message:
                    results['migrated'] += 1
                else:
                    results['skipped'] += 1
            else:
                results['errors'] += 1
        
        return results
    
    def generate_migration_report(self) -> None:
        """Generate a comprehensive migration report."""
        
        if not self.migration_log and not self.errors:
            console.print("📄 No migrations performed - nothing to report")
            return
        
        # Create migration summary table
        table = Table(title="Migration Summary", show_header=True, header_style="bold magenta")
        table.add_column("Old Filename", style="cyan")
        table.add_column("New Filename", style="green") 
        table.add_column("Tags", style="yellow")
        table.add_column("Status", justify="center")
        
        for migration in self.migration_log:
            status = "🔄 DRY RUN" if self.dry_run else "✅ MIGRATED"
            tags_str = ', '.join(migration['tags'])
            table.add_row(
                migration['old_name'],
                migration['new_name'],
                tags_str,
                status
            )
        
        console.print(table)
        
        # Error summary
        if self.errors:
            console.print("\n❌ Errors encountered:")
            for error in self.errors:
                console.print(f"  • {error}")
        
        # Write detailed log file
        log_filename = f"migration_log_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
        log_path = Path("scripts") / log_filename
        
        import json
        try:
            with open(log_path, 'w') as f:
                json.dump({
                    'timestamp': datetime.now().isoformat(),
                    'dry_run': self.dry_run,
                    'migrations': self.migration_log,
                    'errors': self.errors
                }, f, indent=2)
            console.print(f"\n📝 Detailed log saved to: {log_path}")
        except Exception as e:
            logger.error(f"Failed to save migration log: {e}")

@click.command()
@click.argument('directory', type=click.Path(exists=True, path_type=Path))
@click.option('--date', '-d', 
              help='Completion date (YYYY-MM-DD). Defaults to today.',
              type=click.DateTime(formats=['%Y-%m-%d']))
@click.option('--dry-run', is_flag=True, 
              help='Preview migrations without making changes')
@click.option('--verbose', '-v', is_flag=True, 
              help='Enable verbose logging')
def main(directory: Path, date: Optional[datetime], dry_run: bool, verbose: bool):
    """
    Migrate archive files from old naming convention to new standardized format.
    
    DIRECTORY: Path to the archive directory containing files to migrate
    
    Examples:
        # Preview migration for today's date
        python scripts/migrate_archive_naming.py --dry-run /path/to/archive
        
        # Migrate with specific completion date  
        python scripts/migrate_archive_naming.py /path/to/archive --date 2025-01-15
        
        # Verbose mode for debugging
        python scripts/migrate_archive_naming.py /path/to/archive --verbose --dry-run
    """
    
    # Setup logging level
    if verbose:
        logging.getLogger().setLevel(logging.DEBUG)
    
    # Use today if no date specified
    completion_date = date.date() if date else datetime.now().date()
    
    # Create migrator
    migrator = ArchiveMigrator(dry_run=dry_run)
    
    try:
        # Display header
        mode = "DRY RUN" if dry_run else "LIVE MIGRATION"
        console.print(Panel.fit(
            f"🔧 Archive Naming Convention Migration - {mode}\n"
            f"📁 Directory: {directory}\n"
            f"📅 Completion Date: {completion_date}\n"
            f"🏷️  Format: YYYY_MM_DD-[ID]-description-[tags].md",
            title="Migration Tool",
            style="bold blue"
        ))
        
        # Perform migration
        results = migrator.migrate_directory(directory, completion_date)
        
        # Display results summary
        console.print("\n📊 Migration Results:")
        console.print(f"  📁 Total files: {results['total_files']}")
        console.print(f"  ✅ Migrated: {results['migrated']}")  
        console.print(f"  ⏭️  Skipped: {results['skipped']}")
        console.print(f"  ❌ Errors: {results['errors']}")
        
        # Generate detailed report
        migrator.generate_migration_report()
        
        if dry_run and results['migrated'] > 0:
            console.print("\n💡 Run without --dry-run to perform actual migration")
        
        return 0 if results['errors'] == 0 else 1
        
    except Exception as e:
        console.print(f"[red]❌ Migration failed: {e}[/red]")
        logger.exception("Migration failed")
        return 1

if __name__ == "__main__":
    sys.exit(main())