#!/usr/bin/env python3
"""
Document Reference Updater Script
=================================

This script updates all cross-references in markdown files to match the new naming conventions.
It systematically finds old file name patterns and replaces them with the standardized names.

Usage:
    python scripts/update_doc_references.py [--dry-run] [--verbose]

Options:
    --dry-run    Show what would be changed without making changes
    --verbose    Show detailed output of all operations
"""

import os
import re
import argparse
from pathlib import Path
from typing import Dict, List, Tuple
import sys

class DocumentReferenceUpdater:
    def __init__(self, project_root: Path, dry_run: bool = False, verbose: bool = False):
        self.project_root = project_root
        self.dry_run = dry_run
        self.verbose = verbose
        self.changes_made = []
        
        # Define the mapping of old file names to new file names
        self.file_mappings = {
            # Implementation Plans - add zero padding
            "0_Vertical_Slice_Architecture_Plan.md": "00_Vertical_Slice_Architecture_Plan.md",
            "1_F1_Block_Placement_Implementation_Plan.md": "01_F1_Block_Placement_Implementation_Plan.md",
            "2_Move_Block_Feature_Implementation_Plan.md": "02_Move_Block_Feature_Implementation_Plan.md",
            "3_F3_Basic_Rule_Engine_Implementation_Plan.md": "03_F3_Basic_Rule_Engine_Implementation_Plan.md",
            "4_Animation_System_Implementation_Plan.md": "04_Animation_System_Implementation_Plan.md",
            "5_Dotnet_New_Templates_Implementation_Plan.md": "05_Dotnet_New_Templates_Implementation_Plan.md",
            "6_Advanced_Logger_And_GameStrapper_Implementation_Plan.md": "06_Advanced_Logger_And_GameStrapper_Implementation_Plan.md",
            "7_Dynamic_Logging_UI_Implementation_Plan.md": "07_Dynamic_Logging_UI_Implementation_Plan.md",
            "8_Automated_Debug_Console_Implementation_Plan.md": "08_Automated_Debug_Console_Implementation_Plan.md",
            
            # ADR naming standardization
            "ADR-006_Fin_Task_Consistency.md": "ADR_006_Fin_Task_Consistency.md",
            "ADR-007_Enhanced_Functional_Validation_Pattern.md": "ADR_007_Enhanced_Functional_Validation_Pattern.md",
            "ADR-008_Anchor_Based_Rule_Engine_Architecture.md": "ADR_008_Anchor_Based_Rule_Engine_Architecture.md",
            "Rule_Engine_001_Grid_Scanning_Approach_SUPERSEDED.md": "ADR_001_Grid_Scanning_Approach_SUPERSEDED.md",
            "Rule_Engine_002_Anchor_Based_Implementation_Guide.md": "ADR_002_Anchor_Based_Implementation_Guide.md",
            
            # Design documents
            "01_Life_Stages.md": "Life_Stages_Design.md",
            "02_Personality_System.md": "Personality_System_Design.md",
            "03_Luck_System.md": "Luck_System_Design.md",
            "04_Block_Narratives.md": "Block_Narratives_Design.md",
            
            # Post Mortems (bug reports moved and renamed)
            "000_Bug_Post_Mortem_Template.md": "TEMPLATE_Bug_Post_Mortem.md",
            "000_Bug_Post_Mortem_Examples.md": "EXAMPLE_Bug_Post_Mortem.md",
            "003_DI_Container_Presenter_Registration.md": "DI_Container_Presenter_Registration_Bug_Report.md",
            "004_SceneRoot_Autoload_Configuration.md": "SceneRoot_Autoload_Configuration_Bug_Report.md",
            "005_Block_Placement_Display_Bug.md": "Block_Placement_Display_Bug_Report.md",
            "006_F1_Block_Placement_Implementation_Issues.md": "F1_Block_Placement_Implementation_Issues_Report.md",
            
            # Moved guide
            "Developer_Tooling_Guide.md": "Developer_Tooling_Guide.md"  # Just moved, name unchanged
        }
        
        # Path mappings for files that moved between folders
        self.path_mappings = {
            # Folder consolidation
            "4_Bug_PostMortems/": "4_Post_Mortems/",
            "3_Implementation_Plan/": "3_Implementation_Plans/",
            
            # Files moved between categories
            "5_Architecture_Decision_Records/Developer_Tooling_Guide.md": "6_Guides/Developer_Tooling_Guide.md"
        }

    def find_markdown_files(self) -> List[Path]:
        """Find all markdown files in the project."""
        md_files = []
        for root, dirs, files in os.walk(self.project_root):
            # Skip certain directories
            if any(skip in root for skip in ['.git', 'node_modules', 'obj', 'bin']):
                continue
                
            for file in files:
                if file.endswith('.md'):
                    md_files.append(Path(root) / file)
        
        return md_files

    def update_file_references(self, file_path: Path) -> int:
        """Update references in a single file. Returns number of changes made."""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            original_content = content
            changes_in_file = 0
            
            # Update file name references
            for old_name, new_name in self.file_mappings.items():
                # Pattern to match markdown links and paths
                patterns = [
                    rf'\[([^\]]*)\]\(([^)]*){re.escape(old_name)}\)',  # [text](path/old_name)
                    rf'{re.escape(old_name)}',  # Direct file name references
                ]
                
                for pattern in patterns:
                    if pattern.startswith(r'\['):  # Markdown link pattern
                        matches = list(re.finditer(pattern, content))
                        if matches:
                            content = re.sub(pattern, rf'[\1](\2{new_name})', content)
                            changes_in_file += len(matches)
                            if self.verbose:
                                print(f"  {old_name} -> {new_name} ({len(matches)} occurrences)")
                    else:  # Direct reference pattern - use simple string replacement
                        if old_name in content:
                            count = content.count(old_name)
                            content = content.replace(old_name, new_name)
                            changes_in_file += count
                            if self.verbose:
                                print(f"  {old_name} -> {new_name} ({count} occurrences)")
            
            # Update path references
            for old_path, new_path in self.path_mappings.items():
                if old_path in content:
                    content = content.replace(old_path, new_path)
                    changes_in_file += content.count(new_path) - original_content.count(new_path)
                    
                    if self.verbose:
                        print(f"  Path: {old_path} -> {new_path}")
            
            # Write back if changes were made and not in dry-run mode
            if content != original_content:
                if not self.dry_run:
                    with open(file_path, 'w', encoding='utf-8') as f:
                        f.write(content)
                
                self.changes_made.append({
                    'file': str(file_path.relative_to(self.project_root)),
                    'changes': changes_in_file
                })
                
                return changes_in_file
            
            return 0
            
        except Exception as e:
            print(f"Error processing {file_path}: {e}")
            return 0

    def run(self) -> None:
        """Run the reference update process."""
        print("Document Reference Updater")
        print("=" * 40)
        
        if self.dry_run:
            print("DRY RUN MODE - No files will be modified")
        
        print(f"Scanning project: {self.project_root}")
        
        # Find all markdown files
        md_files = self.find_markdown_files()
        print(f"Found {len(md_files)} markdown files")
        
        # Process each file
        total_changes = 0
        files_modified = 0
        
        for file_path in md_files:
            if self.verbose:
                print(f"\nProcessing: {file_path.relative_to(self.project_root)}")
            
            changes = self.update_file_references(file_path)
            if changes > 0:
                files_modified += 1
                total_changes += changes
                
                if not self.verbose:
                    print(f"Modified: {file_path.relative_to(self.project_root)} ({changes} changes)")
        
        # Print summary
        print("\nSummary")
        print("-" * 20)
        print(f"Files processed: {len(md_files)}")
        print(f"Files modified: {files_modified}")
        print(f"Total changes: {total_changes}")
        
        if self.dry_run and total_changes > 0:
            print("\nRun without --dry-run to apply these changes")
        elif total_changes > 0:
            print("\nAll references updated successfully!")
        else:
            print("\nAll references are already up to date!")
        
        # Detailed change report
        if self.changes_made and self.verbose:
            print("\nDetailed Change Report")
            print("-" * 30)
            for change in self.changes_made:
                print(f"  {change['file']}: {change['changes']} changes")

def main():
    parser = argparse.ArgumentParser(
        description="Update document cross-references to match new naming conventions",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python scripts/update_doc_references.py --dry-run     # Preview changes
  python scripts/update_doc_references.py --verbose    # Apply with detailed output
  python scripts/update_doc_references.py              # Apply changes quietly
        """
    )
    
    parser.add_argument(
        '--dry-run',
        action='store_true',
        help='Show what would be changed without making changes'
    )
    
    parser.add_argument(
        '--verbose', '-v',
        action='store_true',
        help='Show detailed output of all operations'
    )
    
    args = parser.parse_args()
    
    # Find project root (directory containing this script's parent)
    script_dir = Path(__file__).parent
    project_root = script_dir.parent
    
    # Verify we're in the right place
    if not (project_root / "CLAUDE.md").exists():
        print("Error: Could not find project root (no CLAUDE.md found)")
        print(f"   Looking in: {project_root}")
        sys.exit(1)
    
    # Run the updater
    updater = DocumentReferenceUpdater(
        project_root=project_root,
        dry_run=args.dry_run,
        verbose=args.verbose
    )
    
    try:
        updater.run()
    except KeyboardInterrupt:
        print("\n\nOperation cancelled by user")
        sys.exit(1)
    except Exception as e:
        print(f"\nError: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()