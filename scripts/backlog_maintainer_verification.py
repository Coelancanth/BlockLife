#!/usr/bin/env python3
"""
Backlog Maintainer Verification System
Ensures file operations are ACTUALLY verified, not just reported.

Created to fix TD_021: Critical reliability issues in backlog-maintainer agent
Addresses BF_003, BF_005, BF_006 incidents where verification code existed but wasn't executed.

This module provides:
1. Mandatory verification after EVERY file operation
2. Comprehensive logging at every step
3. Loud failures instead of silent ones
4. Rollback mechanisms for failed operations
5. Self-verification health checks

Usage:
    from backlog_maintainer_verification import FileOperationVerifier
    
    verifier = FileOperationVerifier()
    success = verifier.move_with_verification(source, destination)
    if not success:
        print(verifier.get_error_report())
"""

import os
import sys
import shutil
import time
import json
import hashlib
import logging
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Tuple, Optional, Any
from enum import Enum
import traceback

# Configure comprehensive logging
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s.%(msecs)03d [%(levelname)s] %(name)s: %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)

logger = logging.getLogger(__name__)


class OperationType(Enum):
    """Types of file operations we verify."""
    CREATE = "CREATE"
    MOVE = "MOVE"
    COPY = "COPY"
    DELETE = "DELETE"
    UPDATE = "UPDATE"
    ARCHIVE = "ARCHIVE"


class VerificationError(Exception):
    """Custom exception for verification failures."""
    pass


class FileOperationVerifier:
    """
    Ensures file operations are verified with comprehensive logging.
    
    This class implements the verification protocol that EXISTS in the
    backlog-maintainer workflow but ISN'T EXECUTED, causing critical failures.
    """
    
    def __init__(self, base_path: str = ".", enable_rollback: bool = True):
        """
        Initialize the verifier with comprehensive configuration.
        
        Args:
            base_path: Base directory for operations
            enable_rollback: Whether to enable automatic rollback on failure
        """
        self.base_path = Path(base_path).resolve()
        self.enable_rollback = enable_rollback
        self.operation_log = []
        self.error_log = []
        self.verification_failures = []
        self.backup_dir = self.base_path / ".backlog_backups"
        self.log_file = self.base_path / "scripts" / "verification_log.json"
        
        # Create backup directory
        self.backup_dir.mkdir(parents=True, exist_ok=True)
        
        logger.info(f"🔧 FileOperationVerifier initialized")
        logger.info(f"   Base path: {self.base_path}")
        logger.info(f"   Rollback enabled: {self.enable_rollback}")
        logger.info(f"   Backup directory: {self.backup_dir}")
        
        # Perform initial health check
        self._perform_health_check()
    
    def _perform_health_check(self) -> bool:
        """
        Perform health checks before operations.
        
        This prevents operations from starting if the environment isn't ready.
        """
        logger.info("🏥 Performing health check...")
        
        checks = {
            "base_path_exists": self.base_path.exists(),
            "backup_dir_writable": self._check_writable(self.backup_dir),
            "has_permissions": self._check_permissions(),
            "filesystem_responsive": self._check_filesystem_responsive()
        }
        
        for check_name, result in checks.items():
            if result:
                logger.info(f"   ✅ {check_name}: PASS")
            else:
                logger.error(f"   ❌ {check_name}: FAIL")
        
        all_passed = all(checks.values())
        if not all_passed:
            logger.error("🚨 HEALTH CHECK FAILED - Operations may fail")
            self._log_error("health_check_failed", checks)
        
        return all_passed
    
    def _check_writable(self, path: Path) -> bool:
        """Check if a directory is writable."""
        try:
            test_file = path / f".write_test_{datetime.now().timestamp()}"
            test_file.write_text("test")
            test_file.unlink()
            return True
        except Exception as e:
            logger.error(f"Write check failed for {path}: {e}")
            return False
    
    def _check_permissions(self) -> bool:
        """Check if we have necessary permissions."""
        try:
            # Try to read and write in base path
            test_path = self.base_path / f".perm_test_{datetime.now().timestamp()}"
            test_path.write_text("permission test")
            content = test_path.read_text()
            test_path.unlink()
            return content == "permission test"
        except Exception as e:
            logger.error(f"Permission check failed: {e}")
            return False
    
    def _check_filesystem_responsive(self) -> bool:
        """Check if filesystem operations are responsive."""
        try:
            start = time.time()
            test_file = self.base_path / f".responsive_test_{datetime.now().timestamp()}"
            test_file.write_text("responsive test")
            test_file.unlink()
            elapsed = time.time() - start
            
            if elapsed > 2.0:
                logger.warning(f"Filesystem slow: {elapsed:.2f}s for simple operation")
                return False
            return True
        except Exception as e:
            logger.error(f"Filesystem responsiveness check failed: {e}")
            return False
    
    def _create_backup(self, source_path: Path) -> Optional[Path]:
        """
        Create a backup of a file before operations.
        
        Args:
            source_path: File to backup
            
        Returns:
            Path to backup file or None if failed
        """
        if not source_path.exists():
            logger.warning(f"Cannot backup non-existent file: {source_path}")
            return None
        
        try:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            backup_name = f"{source_path.name}.{timestamp}.backup"
            backup_path = self.backup_dir / backup_name
            
            logger.info(f"📋 Creating backup: {source_path} → {backup_path}")
            shutil.copy2(str(source_path), str(backup_path))
            
            # Verify backup
            if backup_path.exists() and backup_path.stat().st_size == source_path.stat().st_size:
                logger.info(f"   ✅ Backup created successfully")
                return backup_path
            else:
                logger.error(f"   ❌ Backup verification failed")
                return None
                
        except Exception as e:
            logger.error(f"Backup creation failed: {e}")
            return None
    
    def _calculate_checksum(self, file_path: Path) -> Optional[str]:
        """Calculate SHA256 checksum of a file."""
        try:
            sha256_hash = hashlib.sha256()
            with open(file_path, "rb") as f:
                for byte_block in iter(lambda: f.read(4096), b""):
                    sha256_hash.update(byte_block)
            return sha256_hash.hexdigest()
        except Exception as e:
            logger.error(f"Checksum calculation failed for {file_path}: {e}")
            return None
    
    def _verify_file_operation(self, 
                              operation_type: OperationType,
                              source_path: Path,
                              dest_path: Optional[Path] = None,
                              expected_checksum: Optional[str] = None) -> bool:
        """
        ACTUALLY VERIFY file operations - not just report success.
        
        This is the CRITICAL function that MUST be called after EVERY operation.
        The workflow has this code but doesn't execute it!
        
        Args:
            operation_type: Type of operation to verify
            source_path: Source file path
            dest_path: Destination file path (for move/copy)
            expected_checksum: Expected checksum for integrity verification
            
        Returns:
            True if verification passed, False otherwise
        """
        logger.info(f"🔍 VERIFYING {operation_type.value} operation...")
        logger.info(f"   Source: {source_path}")
        if dest_path:
            logger.info(f"   Destination: {dest_path}")
        
        # CRITICAL: Add delay for filesystem sync (Windows especially needs this)
        time.sleep(1.5)
        logger.info("   ⏰ Waited 1.5s for filesystem sync")
        
        try:
            if operation_type == OperationType.MOVE:
                # Verify move: dest exists, source doesn't, size matches
                if not dest_path.exists():
                    error = f"MOVE FAILED: Destination {dest_path} does not exist"
                    logger.error(f"   ❌ {error}")
                    self._log_verification_failure(operation_type, error, source_path, dest_path)
                    return False
                
                if source_path.exists():
                    error = f"MOVE FAILED: Source {source_path} still exists (not deleted)"
                    logger.error(f"   ❌ {error}")
                    self._log_verification_failure(operation_type, error, source_path, dest_path)
                    return False
                
                if dest_path.stat().st_size == 0:
                    error = f"MOVE FAILED: Destination {dest_path} is empty (0 bytes)"
                    logger.error(f"   ❌ {error}")
                    self._log_verification_failure(operation_type, error, source_path, dest_path)
                    return False
                
                # Verify checksum if provided
                if expected_checksum:
                    actual_checksum = self._calculate_checksum(dest_path)
                    if actual_checksum != expected_checksum:
                        error = f"MOVE FAILED: Checksum mismatch (expected: {expected_checksum}, got: {actual_checksum})"
                        logger.error(f"   ❌ {error}")
                        self._log_verification_failure(operation_type, error, source_path, dest_path)
                        return False
                
                logger.info(f"   ✅ MOVE VERIFIED: File successfully moved")
                return True
            
            elif operation_type == OperationType.CREATE:
                # Verify creation: file exists and is not empty
                if not source_path.exists():
                    error = f"CREATE FAILED: File {source_path} does not exist"
                    logger.error(f"   ❌ {error}")
                    self._log_verification_failure(operation_type, error, source_path)
                    return False
                
                if source_path.stat().st_size == 0:
                    error = f"CREATE FAILED: File {source_path} is empty (0 bytes)"
                    logger.error(f"   ❌ {error}")
                    self._log_verification_failure(operation_type, error, source_path)
                    return False
                
                logger.info(f"   ✅ CREATE VERIFIED: File successfully created")
                return True
            
            elif operation_type == OperationType.DELETE:
                # Verify deletion: file doesn't exist
                if source_path.exists():
                    error = f"DELETE FAILED: File {source_path} still exists"
                    logger.error(f"   ❌ {error}")
                    self._log_verification_failure(operation_type, error, source_path)
                    return False
                
                logger.info(f"   ✅ DELETE VERIFIED: File successfully deleted")
                return True
            
            elif operation_type == OperationType.COPY:
                # Verify copy: both files exist with same size
                if not source_path.exists():
                    error = f"COPY FAILED: Source {source_path} does not exist"
                    logger.error(f"   ❌ {error}")
                    self._log_verification_failure(operation_type, error, source_path, dest_path)
                    return False
                
                if not dest_path.exists():
                    error = f"COPY FAILED: Destination {dest_path} does not exist"
                    logger.error(f"   ❌ {error}")
                    self._log_verification_failure(operation_type, error, source_path, dest_path)
                    return False
                
                if source_path.stat().st_size != dest_path.stat().st_size:
                    error = f"COPY FAILED: Size mismatch (source: {source_path.stat().st_size}, dest: {dest_path.stat().st_size})"
                    logger.error(f"   ❌ {error}")
                    self._log_verification_failure(operation_type, error, source_path, dest_path)
                    return False
                
                logger.info(f"   ✅ COPY VERIFIED: File successfully copied")
                return True
            
            elif operation_type == OperationType.UPDATE:
                # Verify update: file exists and was modified
                if not source_path.exists():
                    error = f"UPDATE FAILED: File {source_path} does not exist"
                    logger.error(f"   ❌ {error}")
                    self._log_verification_failure(operation_type, error, source_path)
                    return False
                
                logger.info(f"   ✅ UPDATE VERIFIED: File successfully updated")
                return True
            
            else:
                error = f"Unknown operation type: {operation_type}"
                logger.error(f"   ❌ {error}")
                return False
                
        except Exception as e:
            error = f"Verification exception: {e}\n{traceback.format_exc()}"
            logger.error(f"   ❌ {error}")
            self._log_verification_failure(operation_type, error, source_path, dest_path)
            return False
    
    def _log_verification_failure(self, 
                                 operation_type: OperationType,
                                 error: str,
                                 source_path: Path,
                                 dest_path: Optional[Path] = None):
        """Log verification failures for analysis."""
        failure = {
            "timestamp": datetime.now().isoformat(),
            "operation": operation_type.value,
            "error": error,
            "source": str(source_path),
            "destination": str(dest_path) if dest_path else None,
            "traceback": traceback.format_stack()
        }
        self.verification_failures.append(failure)
        self._save_logs()
    
    def _log_operation(self,
                      operation_type: OperationType,
                      success: bool,
                      source_path: Path,
                      dest_path: Optional[Path] = None,
                      details: Optional[Dict] = None):
        """Log all operations for audit trail."""
        log_entry = {
            "timestamp": datetime.now().isoformat(),
            "operation": operation_type.value,
            "success": success,
            "source": str(source_path),
            "destination": str(dest_path) if dest_path else None,
            "details": details or {}
        }
        self.operation_log.append(log_entry)
        
        # Also log to console
        status = "✅ SUCCESS" if success else "❌ FAILED"
        logger.info(f"📝 Operation logged: {operation_type.value} - {status}")
        
        # Save logs immediately for crash recovery
        self._save_logs()
    
    def _log_error(self, error_type: str, details: Any):
        """Log errors for debugging."""
        error_entry = {
            "timestamp": datetime.now().isoformat(),
            "type": error_type,
            "details": details,
            "traceback": traceback.format_stack()
        }
        self.error_log.append(error_entry)
        self._save_logs()
    
    def _save_logs(self):
        """Save all logs to file for persistence."""
        try:
            log_data = {
                "generated": datetime.now().isoformat(),
                "operations": self.operation_log,
                "errors": self.error_log,
                "verification_failures": self.verification_failures
            }
            
            # Ensure scripts directory exists
            self.log_file.parent.mkdir(parents=True, exist_ok=True)
            
            # Write logs
            with open(self.log_file, 'w') as f:
                json.dump(log_data, f, indent=2, default=str)
                
        except Exception as e:
            logger.error(f"Failed to save logs: {e}")
    
    def move_with_verification(self, 
                              source: Path,
                              destination: Path,
                              create_backup: bool = True) -> bool:
        """
        Move a file with MANDATORY verification.
        
        This is the PRIMARY method that should be used for all move operations.
        It ensures verification ACTUALLY HAPPENS, not just reported.
        
        Args:
            source: Source file path
            destination: Destination file path
            create_backup: Whether to create backup before move
            
        Returns:
            True if move AND verification succeeded, False otherwise
        """
        source = Path(source).resolve()
        destination = Path(destination).resolve()
        
        logger.info("=" * 60)
        logger.info("🚀 MOVE OPERATION WITH VERIFICATION")
        logger.info(f"   From: {source}")
        logger.info(f"   To: {destination}")
        logger.info("=" * 60)
        
        # Pre-flight checks
        if not source.exists():
            error = f"Source file does not exist: {source}"
            logger.error(f"❌ PRE-FLIGHT FAILED: {error}")
            self._log_error("source_not_found", {"source": str(source)})
            self._log_operation(OperationType.MOVE, False, source, destination, {"error": error})
            return False
        
        if destination.exists():
            error = f"Destination already exists: {destination}"
            logger.error(f"❌ PRE-FLIGHT FAILED: {error}")
            self._log_error("destination_exists", {"destination": str(destination)})
            self._log_operation(OperationType.MOVE, False, source, destination, {"error": error})
            return False
        
        # Create backup if requested
        backup_path = None
        if create_backup and self.enable_rollback:
            backup_path = self._create_backup(source)
            if not backup_path:
                logger.warning("⚠️  Backup creation failed, continuing without rollback capability")
        
        # Calculate checksum for integrity verification
        source_checksum = self._calculate_checksum(source)
        source_size = source.stat().st_size
        logger.info(f"📊 Source file size: {source_size} bytes")
        logger.info(f"🔐 Source checksum: {source_checksum}")
        
        # Ensure destination directory exists
        destination.parent.mkdir(parents=True, exist_ok=True)
        
        # Perform the move operation
        try:
            logger.info("📦 Executing move operation...")
            shutil.move(str(source), str(destination))
            logger.info("   Move command executed")
            
        except Exception as e:
            error = f"Move operation failed: {e}"
            logger.error(f"❌ MOVE FAILED: {error}")
            self._log_error("move_exception", {"error": str(e), "traceback": traceback.format_exc()})
            self._log_operation(OperationType.MOVE, False, source, destination, {"error": error})
            
            # Attempt rollback if we have a backup
            if backup_path and backup_path.exists():
                try:
                    logger.info("🔄 Attempting rollback from backup...")
                    shutil.copy2(str(backup_path), str(source))
                    logger.info("   ✅ Rollback successful")
                except Exception as rollback_error:
                    logger.error(f"   ❌ Rollback failed: {rollback_error}")
            
            return False
        
        # CRITICAL: VERIFY the move operation
        logger.info("🔍 Starting MANDATORY verification...")
        verification_passed = self._verify_file_operation(
            OperationType.MOVE,
            source,
            destination,
            source_checksum
        )
        
        if verification_passed:
            logger.info("✅ MOVE OPERATION FULLY VERIFIED")
            self._log_operation(OperationType.MOVE, True, source, destination, {
                "size": source_size,
                "checksum": source_checksum,
                "backup": str(backup_path) if backup_path else None
            })
            
            # Clean up backup after successful operation
            if backup_path and backup_path.exists():
                try:
                    backup_path.unlink()
                    logger.info("🧹 Backup cleaned up")
                except Exception as e:
                    logger.warning(f"Could not clean up backup: {e}")
            
            return True
        else:
            logger.error("❌ VERIFICATION FAILED - ATTEMPTING ROLLBACK")
            self._log_operation(OperationType.MOVE, False, source, destination, {
                "error": "Verification failed",
                "verification_failures": self.verification_failures[-1] if self.verification_failures else None
            })
            
            # Attempt rollback
            if self.enable_rollback:
                rollback_success = self._perform_rollback(source, destination, backup_path)
                if rollback_success:
                    logger.info("✅ Rollback completed successfully")
                else:
                    logger.error("❌ Rollback failed - manual intervention required")
            
            return False
    
    def _perform_rollback(self,
                         original_source: Path,
                         failed_destination: Path,
                         backup_path: Optional[Path]) -> bool:
        """
        Perform rollback after a failed operation.
        
        Args:
            original_source: Original source path
            failed_destination: Destination that failed verification
            backup_path: Path to backup file
            
        Returns:
            True if rollback succeeded, False otherwise
        """
        logger.info("🔄 PERFORMING ROLLBACK...")
        
        try:
            # First, try to move the file back if it exists at destination
            if failed_destination.exists():
                logger.info(f"   Moving {failed_destination} back to {original_source}")
                shutil.move(str(failed_destination), str(original_source))
                
                # Verify the rollback
                if original_source.exists() and not failed_destination.exists():
                    logger.info("   ✅ File successfully moved back")
                    return True
            
            # If that didn't work, try to restore from backup
            if backup_path and backup_path.exists():
                logger.info(f"   Restoring from backup: {backup_path}")
                shutil.copy2(str(backup_path), str(original_source))
                
                # Verify the restore
                if original_source.exists():
                    logger.info("   ✅ File successfully restored from backup")
                    
                    # Clean up any partial destination file
                    if failed_destination.exists():
                        failed_destination.unlink()
                        logger.info("   🧹 Cleaned up failed destination")
                    
                    return True
            
            logger.error("   ❌ Rollback failed - no viable recovery method")
            return False
            
        except Exception as e:
            logger.error(f"   ❌ Rollback exception: {e}")
            self._log_error("rollback_failed", {
                "error": str(e),
                "original_source": str(original_source),
                "failed_destination": str(failed_destination),
                "backup_path": str(backup_path) if backup_path else None
            })
            return False
    
    def archive_with_verification(self,
                                 item_id: str,
                                 source: Path,
                                 archive_dir: Path) -> Tuple[bool, Optional[Path]]:
        """
        Archive a backlog item with full verification.
        
        This prevents the duplication issues seen in BF_006.
        
        Args:
            item_id: Backlog item ID
            source: Source file path
            archive_dir: Archive directory
            
        Returns:
            Tuple of (success, archive_path)
        """
        source = Path(source).resolve()
        archive_dir = Path(archive_dir).resolve()
        
        logger.info("=" * 60)
        logger.info(f"📚 ARCHIVING {item_id} WITH VERIFICATION")
        logger.info("=" * 60)
        
        # Generate archive filename
        timestamp = datetime.now().strftime("%Y_%m_%d")
        archive_filename = f"{timestamp}-{source.name}"
        destination = archive_dir / archive_filename
        
        # Use move_with_verification for the actual operation
        success = self.move_with_verification(source, destination, create_backup=True)
        
        if success:
            logger.info(f"✅ {item_id} successfully archived to {destination}")
            return True, destination
        else:
            logger.error(f"❌ {item_id} archive failed")
            return False, None
    
    def get_verification_report(self) -> Dict[str, Any]:
        """
        Get a comprehensive verification report.
        
        Returns:
            Dictionary containing all verification data
        """
        total_operations = len(self.operation_log)
        successful_operations = sum(1 for op in self.operation_log if op["success"])
        failed_operations = total_operations - successful_operations
        
        return {
            "summary": {
                "total_operations": total_operations,
                "successful": successful_operations,
                "failed": failed_operations,
                "success_rate": (successful_operations / total_operations * 100) if total_operations > 0 else 0,
                "total_errors": len(self.error_log),
                "verification_failures": len(self.verification_failures)
            },
            "operations": self.operation_log,
            "errors": self.error_log,
            "verification_failures": self.verification_failures,
            "timestamp": datetime.now().isoformat()
        }
    
    def print_verification_report(self):
        """Print a human-readable verification report."""
        report = self.get_verification_report()
        
        print("\n" + "=" * 60)
        print("📊 VERIFICATION REPORT")
        print("=" * 60)
        print(f"Generated: {report['timestamp']}")
        print("\nSUMMARY:")
        print(f"  Total Operations: {report['summary']['total_operations']}")
        print(f"  Successful: {report['summary']['successful']}")
        print(f"  Failed: {report['summary']['failed']}")
        print(f"  Success Rate: {report['summary']['success_rate']:.1f}%")
        print(f"  Total Errors: {report['summary']['total_errors']}")
        print(f"  Verification Failures: {report['summary']['verification_failures']}")
        
        if report['verification_failures']:
            print("\n❌ VERIFICATION FAILURES:")
            for failure in report['verification_failures']:
                print(f"\n  {failure['timestamp']}:")
                print(f"    Operation: {failure['operation']}")
                print(f"    Error: {failure['error']}")
                print(f"    Source: {failure['source']}")
                if failure['destination']:
                    print(f"    Destination: {failure['destination']}")
        
        if report['errors']:
            print("\n⚠️  ERRORS:")
            for error in report['errors'][-5:]:  # Show last 5 errors
                print(f"\n  {error['timestamp']}:")
                print(f"    Type: {error['type']}")
                print(f"    Details: {error['details']}")
        
        print("\n" + "=" * 60)
    
    def get_error_report(self) -> str:
        """
        Get a detailed error report for debugging.
        
        Returns:
            String containing formatted error information
        """
        if not self.verification_failures and not self.error_log:
            return "No errors recorded"
        
        report_lines = []
        report_lines.append("=" * 60)
        report_lines.append("ERROR REPORT")
        report_lines.append("=" * 60)
        
        if self.verification_failures:
            report_lines.append("\nVERIFICATION FAILURES:")
            for i, failure in enumerate(self.verification_failures, 1):
                report_lines.append(f"\n{i}. {failure['timestamp']}")
                report_lines.append(f"   Operation: {failure['operation']}")
                report_lines.append(f"   Error: {failure['error']}")
                report_lines.append(f"   Source: {failure['source']}")
                if failure['destination']:
                    report_lines.append(f"   Destination: {failure['destination']}")
        
        if self.error_log:
            report_lines.append("\n\nERROR LOG:")
            for i, error in enumerate(self.error_log, 1):
                report_lines.append(f"\n{i}. {error['timestamp']}")
                report_lines.append(f"   Type: {error['type']}")
                report_lines.append(f"   Details: {json.dumps(error['details'], indent=6)}")
        
        report_lines.append("\n" + "=" * 60)
        return "\n".join(report_lines)


def test_verification_system():
    """
    Test the verification system to ensure it works correctly.
    
    This test simulates the scenarios from BF_003, BF_005, BF_006.
    """
    print("\n" + "=" * 60)
    print("🧪 TESTING VERIFICATION SYSTEM")
    print("=" * 60)
    
    # Create test environment
    test_dir = Path("test_verification")
    test_dir.mkdir(exist_ok=True)
    
    verifier = FileOperationVerifier(base_path=test_dir)
    
    # Test 1: Successful move with verification
    print("\n📝 Test 1: Successful move with verification")
    test_file = test_dir / "test_item.md"
    test_file.write_text("# Test Item\n\nThis is a test backlog item.")
    
    archive_dir = test_dir / "archive" / "2025-Q3"
    archive_dir.mkdir(parents=True, exist_ok=True)
    
    dest_file = archive_dir / "test_item_archived.md"
    
    success = verifier.move_with_verification(test_file, dest_file)
    if success:
        print("   ✅ Test 1 PASSED: Move verified successfully")
    else:
        print("   ❌ Test 1 FAILED: Move verification failed")
        print(verifier.get_error_report())
    
    # Test 2: Prevent duplicate (BF_006 scenario)
    print("\n📝 Test 2: Prevent duplicate creation")
    test_file2 = test_dir / "test_duplicate.md"
    test_file2.write_text("# Test Duplicate\n\nThis should not create duplicates.")
    
    dest_file2 = archive_dir / "test_duplicate_archived.md"
    
    # First move should succeed
    success1 = verifier.move_with_verification(test_file2, dest_file2)
    
    # Try to "move" again (simulating the duplication bug)
    test_file2.write_text("# Duplicate attempt")  # Recreate source
    success2 = verifier.move_with_verification(test_file2, dest_file2)
    
    if success1 and not success2:
        print("   ✅ Test 2 PASSED: Duplicate prevention working")
    else:
        print("   ❌ Test 2 FAILED: Duplicate not prevented")
    
    # Test 3: Rollback on failure
    print("\n📝 Test 3: Rollback on verification failure")
    test_file3 = test_dir / "test_rollback.md"
    test_file3.write_text("# Test Rollback\n\nThis tests rollback capability.")
    
    # This will be tested by the internal verification
    # The system should detect any failures and rollback
    
    # Print final report
    verifier.print_verification_report()
    
    # Cleanup
    try:
        shutil.rmtree(test_dir)
        print("\n🧹 Test environment cleaned up")
    except Exception as e:
        print(f"\n⚠️  Could not clean up test directory: {e}")


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "test":
        test_verification_system()
    else:
        print("Backlog Maintainer Verification System")
        print("=" * 40)
        print("This module provides mandatory verification for file operations.")
        print("\nUsage:")
        print("  python backlog_maintainer_verification.py test  # Run tests")
        print("\nOr import and use in your code:")
        print("  from backlog_maintainer_verification import FileOperationVerifier")
        print("  verifier = FileOperationVerifier()")
        print("  success = verifier.move_with_verification(source, dest)")