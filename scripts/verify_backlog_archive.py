#!/usr/bin/env python3
"""
Backlog Archive Verification Script
Prevents false positive bug reports by verifying archive operations

This script was created after BF_005 incorrectly reported that files weren't archived
when they actually were successfully moved.
"""

import os
import sys
import json
import re
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Tuple, Optional

class BacklogArchiveVerifier:
    """Verifies backlog archive operations to prevent false positive bug reports."""
    
    def __init__(self, base_path: str = "."):
        self.base_path = Path(base_path)
        self.backlog_path = self.base_path / "Docs" / "Backlog"
        self.items_path = self.backlog_path / "items"
        self.archive_path = self.backlog_path / "archive"
        self.backlog_file = self.backlog_path / "Backlog.md"
        self.verification_log = []
        self.errors = []
        self.warnings = []
        
    def verify_archive_operation(self, item_id: str) -> Dict[str, any]:
        """
        Verify a specific item's archive status.
        Returns detailed verification results.
        """
        result = {
            "item_id": item_id,
            "status": "unknown",
            "in_items_folder": False,
            "in_archive_folder": False,
            "archive_path": None,
            "backlog_status": None,
            "verification_passed": False,
            "errors": [],
            "warnings": []
        }
        
        # Check items folder
        items_files = list(self.items_path.glob(f"{item_id}_*.md"))
        if items_files:
            result["in_items_folder"] = True
            result["items_file"] = str(items_files[0])
        
        # Check archive folder (recursively)
        archive_files = list(self.archive_path.rglob(f"*{item_id}*.md"))
        if archive_files:
            result["in_archive_folder"] = True
            result["archive_path"] = str(archive_files[0])
        
        # Check Backlog.md status
        if self.backlog_file.exists():
            content = self.backlog_file.read_text()
            # Look for the item in backlog
            pattern = rf"\[{item_id}\]\(([^)]+)\)[^|]*\|[^|]*\|[^|]*\|[^|]*\|[^|]*([^|]+)"
            match = re.search(pattern, content)
            if match:
                linked_path = match.group(1)
                status_text = match.group(2)
                result["backlog_link"] = linked_path
                if "ARCHIVED" in status_text or "archive" in linked_path.lower():
                    result["backlog_status"] = "archived"
                else:
                    result["backlog_status"] = "active"
        
        # Determine verification status
        if result["backlog_status"] == "archived":
            # Should be in archive, not in items
            if result["in_archive_folder"] and not result["in_items_folder"]:
                result["status"] = "correctly_archived"
                result["verification_passed"] = True
            elif result["in_items_folder"] and not result["in_archive_folder"]:
                result["status"] = "false_archive_status"
                result["errors"].append(f"{item_id} marked as archived but still in items folder")
            elif result["in_items_folder"] and result["in_archive_folder"]:
                result["status"] = "duplicate_files"
                result["errors"].append(f"{item_id} exists in both items and archive folders")
            else:
                result["status"] = "missing_file"
                result["errors"].append(f"{item_id} marked as archived but file not found anywhere")
        else:
            # Should be in items, not in archive
            if result["in_items_folder"] and not result["in_archive_folder"]:
                result["status"] = "correctly_active"
                result["verification_passed"] = True
            elif result["in_archive_folder"] and not result["in_items_folder"]:
                result["status"] = "false_active_status"
                result["warnings"].append(f"{item_id} in archive but marked as active")
            elif result["in_items_folder"] and result["in_archive_folder"]:
                result["status"] = "duplicate_files"
                result["errors"].append(f"{item_id} exists in both items and archive folders")
        
        return result
    
    def verify_all_archives(self) -> Dict[str, any]:
        """Verify all archived items in the backlog."""
        results = {
            "timestamp": datetime.now().isoformat(),
            "total_items_checked": 0,
            "verification_passed": 0,
            "verification_failed": 0,
            "errors": [],
            "warnings": [],
            "items": {}
        }
        
        # Parse Backlog.md to find all items
        if not self.backlog_file.exists():
            results["errors"].append("Backlog.md not found")
            return results
        
        content = self.backlog_file.read_text()
        
        # Find all item references
        pattern = r"\[([A-Z]{2}_\d{3})\]"
        matches = re.findall(pattern, content)
        
        for item_id in set(matches):  # Use set to avoid duplicates
            verification = self.verify_archive_operation(item_id)
            results["items"][item_id] = verification
            results["total_items_checked"] += 1
            
            if verification["verification_passed"]:
                results["verification_passed"] += 1
            else:
                results["verification_failed"] += 1
            
            results["errors"].extend(verification["errors"])
            results["warnings"].extend(verification["warnings"])
        
        return results
    
    def check_false_positive_bf005(self) -> bool:
        """
        Specifically check if BF_005's claim about TD_014, TD_015, TD_020 is accurate.
        Returns True if BF_005 is a false positive.
        """
        print("\n=== Checking BF_005 Claim ===")
        items_to_check = ["TD_014", "TD_015", "TD_020", "TD_019"]
        false_positive = True
        
        for item_id in items_to_check:
            result = self.verify_archive_operation(item_id)
            print(f"\n{item_id}:")
            print(f"  In items folder: {result['in_items_folder']}")
            print(f"  In archive folder: {result['in_archive_folder']}")
            if result["archive_path"]:
                print(f"  Archive location: {result['archive_path']}")
            print(f"  Backlog status: {result['backlog_status']}")
            print(f"  Verification: {'PASSED' if result['verification_passed'] else 'FAILED'}")
            
            if result["in_items_folder"]:
                false_positive = False
                print(f"  ERROR: {item_id} still in items folder!")
        
        if false_positive:
            print("\n✓ BF_005 is a FALSE POSITIVE - All files were correctly archived!")
        else:
            print("\n✗ BF_005 is accurate - Some files were not archived properly")
        
        return false_positive
    
    def generate_report(self, results: Dict) -> str:
        """Generate a human-readable verification report."""
        report = []
        report.append("=" * 60)
        report.append("BACKLOG ARCHIVE VERIFICATION REPORT")
        report.append("=" * 60)
        report.append(f"Timestamp: {results['timestamp']}")
        report.append(f"Total Items Checked: {results['total_items_checked']}")
        report.append(f"Verification Passed: {results['verification_passed']}")
        report.append(f"Verification Failed: {results['verification_failed']}")
        
        if results['errors']:
            report.append("\nERRORS FOUND:")
            for error in results['errors']:
                report.append(f"  ✗ {error}")
        
        if results['warnings']:
            report.append("\nWARNINGS:")
            for warning in results['warnings']:
                report.append(f"  ⚠ {warning}")
        
        if results['verification_failed'] > 0:
            report.append("\nFAILED ITEMS DETAILS:")
            for item_id, details in results['items'].items():
                if not details['verification_passed']:
                    report.append(f"\n  {item_id}:")
                    report.append(f"    Status: {details['status']}")
                    report.append(f"    In items: {details['in_items_folder']}")
                    report.append(f"    In archive: {details['in_archive_folder']}")
                    if details['errors']:
                        report.append(f"    Errors: {', '.join(details['errors'])}")
        
        if results['verification_failed'] == 0:
            report.append("\n✓ ALL VERIFICATIONS PASSED!")
            report.append("No archive inconsistencies detected.")
        
        report.append("=" * 60)
        return "\n".join(report)

def main():
    """Main entry point for the verification script."""
    verifier = BacklogArchiveVerifier()
    
    # First, check the specific BF_005 claim
    is_false_positive = verifier.check_false_positive_bf005()
    
    print("\n" + "=" * 60)
    print("Running full archive verification...")
    print("=" * 60)
    
    # Run full verification
    results = verifier.verify_all_archives()
    
    # Generate and print report
    report = verifier.generate_report(results)
    print(report)
    
    # Save results to file
    results_file = Path("scripts") / "backlog_archive_verification.json"
    results_file.write_text(json.dumps(results, indent=2))
    print(f"\nDetailed results saved to: {results_file}")
    
    # Return exit code based on verification
    if results['verification_failed'] > 0:
        sys.exit(1)
    else:
        sys.exit(0)

if __name__ == "__main__":
    main()