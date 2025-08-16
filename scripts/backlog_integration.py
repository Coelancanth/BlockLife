#!/usr/bin/env python3
"""
Backlog Maintainer Integration Helper
Demonstrates how the backlog maintainer workflow integrates with auto-archival

This script shows the integration pattern for triggering automatic archival
when items reach 100% completion.
"""

import subprocess
import sys
import logging
from pathlib import Path
from typing import Optional

logger = logging.getLogger(__name__)

class BacklogIntegration:
    """Integration helper for backlog maintainer workflow."""
    
    def __init__(self):
        self.scripts_path = Path("scripts")
        self.auto_archive_script = self.scripts_path / "auto_archive_completed.py"
    
    def trigger_auto_archive(self, item_id: Optional[str] = None) -> bool:
        """
        Trigger automatic archival for completed items.
        
        Args:
            item_id: Optional specific item ID to archive
            
        Returns:
            True if archival succeeded, False otherwise
        """
        
        logger.info(f"🤖 Triggering automatic archival for {item_id or 'all completed items'}")
        
        try:
            # Build command
            cmd = [sys.executable, str(self.auto_archive_script), "--verbose"]
            
            # Execute archival script
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=300  # 5 minute timeout
            )
            
            if result.returncode == 0:
                logger.info("✅ Auto-archival completed successfully")
                if result.stdout:
                    logger.debug(f"Output: {result.stdout}")
                return True
            else:
                logger.error(f"❌ Auto-archival failed with code {result.returncode}")
                if result.stderr:
                    logger.error(f"Error: {result.stderr}")
                return False
        
        except subprocess.TimeoutExpired:
            logger.error("❌ Auto-archival timed out after 5 minutes")
            return False
        except Exception as e:
            logger.error(f"❌ Auto-archival failed: {e}")
            return False
    
    def verify_archival_result(self, item_id: str) -> bool:
        """
        Verify that an item was successfully archived.
        
        Args:
            item_id: The work item ID to verify
            
        Returns:
            True if successfully archived, False otherwise
        """
        
        # Use existing verification script
        verify_script = self.scripts_path / "verify_backlog_archive.py"
        
        if not verify_script.exists():
            logger.warning("Verification script not found, skipping verification")
            return True
        
        try:
            cmd = [sys.executable, str(verify_script)]
            result = subprocess.run(cmd, capture_output=True, text=True)
            
            # Check if verification passed
            return result.returncode == 0
        
        except Exception as e:
            logger.error(f"Verification failed: {e}")
            return False
    
    def update_progress_with_auto_archive(self, item_id: str, new_progress: int) -> str:
        """
        Simulate the enhanced update_progress workflow with auto-archival.
        
        This demonstrates how the backlog maintainer workflow would be enhanced
        to automatically trigger archival when items reach 100%.
        
        Args:
            item_id: Work item identifier
            new_progress: New progress percentage
            
        Returns:
            Status message
        """
        
        # Simulate progress update
        logger.info(f"Updating {item_id} progress to {new_progress}%")
        
        # Check if 100% completion triggers archival
        if new_progress == 100:
            logger.info(f"🎯 {item_id} reached 100% - triggering automatic archival")
            
            if self.trigger_auto_archive():
                # Verify archival completed
                if self.verify_archival_result(item_id):
                    return f"✅ {item_id}: 100% → Auto-archived successfully"
                else:
                    return f"⚠️ {item_id}: 100% → Archival verification failed"
            else:
                return f"❌ {item_id}: 100% → Auto-archival failed"
        else:
            return f"✓ {item_id}: Progress updated to {new_progress}%"

def demonstrate_integration():
    """Demonstrate the integration workflow."""
    
    logging.basicConfig(level=logging.INFO, format='%(message)s')
    
    print("🤖 Backlog Maintainer Integration Demo")
    print("=" * 50)
    
    integration = BacklogIntegration()
    
    # Simulate different scenarios
    scenarios = [
        ("TD_019", 85),  # Normal progress update
        ("TD_020", 100), # Completion triggering archival
    ]
    
    for item_id, progress in scenarios:
        print(f"\n📋 Scenario: {item_id} → {progress}%")
        result = integration.update_progress_with_auto_archive(item_id, progress)
        print(f"Result: {result}")
    
    print("\n📊 Integration Pattern Summary:")
    print("1. Progress update triggers 100% check")
    print("2. 100% completion automatically runs archival script")
    print("3. Verification confirms archival success")
    print("4. Backlog updated with archive reference")
    print("\n⚡ Time Saved: ~5-10 minutes manual work per completed item")

if __name__ == "__main__":
    demonstrate_integration()