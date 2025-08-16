#!/usr/bin/env python3
"""
Agent Output Verification Script
Verifies that agent-reported actions actually completed successfully.
Created after BF_003 incident to catch false success reports.
"""

import os
import sys
import json
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Tuple, Optional

class AgentVerifier:
    """Verifies agent operations completed as reported"""
    
    def __init__(self):
        self.project_root = Path("C:/Users/Coel/Documents/Godot/blocklife")
        self.backlog_path = self.project_root / "Docs/Backlog"
        self.verification_log = []
        
    def verify_archive(self, item_ids: List[str]) -> Dict[str, Dict]:
        """
        Verify items were properly archived
        
        Args:
            item_ids: List of item IDs to verify (e.g., ["TD_012", "TD_013"])
            
        Returns:
            Dict with verification results for each item
        """
        results = {}
        current_quarter = self._get_current_quarter()
        
        for item_id in item_ids:
            # Check if file exists in items folder
            in_items = list(self.backlog_path.glob(f"items/{item_id}_*.md"))
            
            # Check if file exists in archive
            in_archive = list(self.backlog_path.glob(f"archive/completed/*/{item_id}_*.md"))
            
            # Properly archived = in archive AND not in items
            properly_archived = len(in_archive) > 0 and len(in_items) == 0
            
            results[item_id] = {
                "archived": properly_archived,
                "in_items": len(in_items),
                "in_archive": len(in_archive),
                "archive_path": str(in_archive[0]) if in_archive else None,
                "items_path": str(in_items[0]) if in_items else None
            }
            
        return results
    
    def verify_status_update(self, item_id: str, expected_status: str, expected_progress: Optional[int] = None) -> bool:
        """
        Verify item status and progress in Backlog.md
        
        Args:
            item_id: Item ID to check
            expected_status: Expected status string
            expected_progress: Expected progress percentage (optional)
            
        Returns:
            True if status matches expected
        """
        backlog_file = self.backlog_path / "Backlog.md"
        
        if not backlog_file.exists():
            self._log(f"ERROR: Backlog.md not found at {backlog_file}")
            return False
            
        with open(backlog_file, 'r', encoding='utf-8') as f:
            content = f.read()
            
        # Look for the item line in the tracker table
        import re
        pattern = rf"\| .* \| .*{item_id}.* \|.*\|.*{expected_status}.*\|"
        
        if expected_progress is not None:
            pattern = rf"\| .* \| .*{item_id}.* \|.*\|.*{expected_status}.*\|.*{expected_progress}%.*\|"
            
        found = bool(re.search(pattern, content))
        
        self._log(f"Status verification for {item_id}: {'✅' if found else '❌'}")
        return found
    
    def verify_file_creation(self, file_path: str) -> bool:
        """
        Verify a file was created
        
        Args:
            file_path: Path to file that should exist
            
        Returns:
            True if file exists
        """
        path = Path(file_path)
        exists = path.exists()
        
        self._log(f"File creation verification for {path.name}: {'✅' if exists else '❌'}")
        return exists
    
    def verify_file_move(self, source_path: str, dest_path: str) -> bool:
        """
        Verify a file was moved (not at source, exists at destination)
        
        Args:
            source_path: Original file location
            dest_path: New file location
            
        Returns:
            True if file was properly moved
        """
        source = Path(source_path)
        dest = Path(dest_path)
        
        properly_moved = not source.exists() and dest.exists()
        
        self._log(f"File move verification: {'✅' if properly_moved else '❌'}")
        if not properly_moved:
            self._log(f"  Source exists: {source.exists()}, Dest exists: {dest.exists()}")
            
        return properly_moved
    
    def verify_backlog_link(self, item_id: str, expected_link_pattern: str) -> bool:
        """
        Verify Backlog.md contains correct link for item
        
        Args:
            item_id: Item ID to check
            expected_link_pattern: Pattern to match in link (e.g., "archive/completed")
            
        Returns:
            True if link matches pattern
        """
        backlog_file = self.backlog_path / "Backlog.md"
        
        with open(backlog_file, 'r', encoding='utf-8') as f:
            content = f.read()
            
        import re
        pattern = rf"\[{item_id}\]\(.*{expected_link_pattern}.*\)"
        found = bool(re.search(pattern, content))
        
        self._log(f"Link verification for {item_id}: {'✅' if found else '❌'}")
        return found
    
    def batch_verify_archive(self, operation_report: str) -> Tuple[bool, Dict]:
        """
        Verify an archive operation based on agent's report
        
        Args:
            operation_report: Text report from agent about what was archived
            
        Returns:
            Tuple of (all_successful, detailed_results)
        """
        # Extract item IDs from report
        import re
        item_ids = re.findall(r'(TD_\d+|VS_\d+|BF_\d+|HF_\d+)', operation_report)
        
        if not item_ids:
            self._log("WARNING: No item IDs found in operation report")
            return False, {}
            
        results = self.verify_archive(item_ids)
        all_successful = all(r["archived"] for r in results.values())
        
        return all_successful, results
    
    def generate_report(self, verification_type: str, results: Dict) -> bool:
        """
        Generate and print verification report
        
        Args:
            verification_type: Type of verification performed
            results: Verification results dictionary
            
        Returns:
            True if all verifications passed
        """
        print(f"\n{'='*50}")
        print(f"📊 VERIFICATION REPORT: {verification_type}")
        print(f"Time: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        print(f"{'='*50}\n")
        
        all_passed = True
        
        for key, value in results.items():
            if isinstance(value, dict):
                status = "✅ PASS" if value.get("archived", False) else "❌ FAIL"
                all_passed = all_passed and value.get("archived", False)
                
                print(f"{status} | {key}")
                if not value.get("archived", False):
                    print(f"  → In items: {value.get('in_items', 0)}")
                    print(f"  → In archive: {value.get('in_archive', 0)}")
                    if value.get('items_path'):
                        print(f"  → Still at: {value['items_path']}")
            else:
                status = "✅ PASS" if value else "❌ FAIL"
                all_passed = all_passed and value
                print(f"{status} | {key}: {value}")
        
        print(f"\n{'='*50}")
        print(f"Overall Result: {'✅ ALL PASSED' if all_passed else '❌ FAILURES DETECTED'}")
        print(f"{'='*50}\n")
        
        # Save log
        self._save_log()
        
        return all_passed
    
    def _get_current_quarter(self) -> str:
        """Get current quarter in YYYY-QN format"""
        now = datetime.now()
        quarter = (now.month - 1) // 3 + 1
        return f"{now.year}-Q{quarter}"
    
    def _log(self, message: str):
        """Add message to verification log"""
        timestamp = datetime.now().strftime('%H:%M:%S')
        self.verification_log.append(f"[{timestamp}] {message}")
    
    def _save_log(self):
        """Save verification log to file"""
        log_dir = self.project_root / "scripts" / "logs"
        log_dir.mkdir(exist_ok=True)
        
        log_file = log_dir / f"verification_{datetime.now().strftime('%Y%m%d_%H%M%S')}.log"
        with open(log_file, 'w', encoding='utf-8') as f:
            f.write('\n'.join(self.verification_log))
        
        print(f"Log saved to: {log_file}")


def main():
    """Main entry point for verification script"""
    verifier = AgentVerifier()
    
    # Check command line arguments
    if len(sys.argv) < 2:
        print("Usage: python verify_agent_output.py <verification_type> [args...]")
        print("\nAvailable verification types:")
        print("  archive TD_012 TD_013 ...  - Verify archive operation")
        print("  status TD_012 complete     - Verify status update")
        print("  file path/to/file.md       - Verify file creation")
        print("  recent-archive             - Verify most recent archive operation")
        return
    
    verification_type = sys.argv[1].lower()
    
    if verification_type == "archive":
        # Verify specific items were archived
        item_ids = sys.argv[2:]
        if not item_ids:
            print("ERROR: Please provide item IDs to verify")
            return
            
        results = verifier.verify_archive(item_ids)
        verifier.generate_report("Archive Operation", results)
        
    elif verification_type == "status":
        # Verify status update
        if len(sys.argv) < 4:
            print("ERROR: Usage: verify_agent_output.py status <item_id> <expected_status> [progress]")
            return
            
        item_id = sys.argv[2]
        expected_status = sys.argv[3]
        expected_progress = int(sys.argv[4]) if len(sys.argv) > 4 else None
        
        result = verifier.verify_status_update(item_id, expected_status, expected_progress)
        results = {f"{item_id} status={expected_status}": result}
        verifier.generate_report("Status Update", results)
        
    elif verification_type == "file":
        # Verify file creation
        if len(sys.argv) < 3:
            print("ERROR: Please provide file path to verify")
            return
            
        file_path = sys.argv[2]
        result = verifier.verify_file_creation(file_path)
        results = {f"File {Path(file_path).name}": result}
        verifier.generate_report("File Creation", results)
        
    elif verification_type == "recent-archive":
        # Verify the most recent archive operation (last 5 completed items)
        print("Checking most recently completed items...")
        recent_items = ["TD_012", "TD_013", "TD_016", "TD_017", "TD_018"]
        results = verifier.verify_archive(recent_items)
        verifier.generate_report("Recent Archive Operation", results)
        
    else:
        print(f"ERROR: Unknown verification type: {verification_type}")
        print("Use 'archive', 'status', 'file', or 'recent-archive'")


if __name__ == "__main__":
    main()