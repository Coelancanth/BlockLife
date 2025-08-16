#!/usr/bin/env python3
"""
Test script for the Backlog Maintainer Verification System
Verifies that TD_021 fixes are working correctly.

This script tests scenarios from BF_003, BF_005, BF_006 to ensure
the verification system prevents these issues.
"""

import os
import sys
import time
import shutil
from pathlib import Path
from datetime import datetime

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent))

from backlog_maintainer_verification import FileOperationVerifier, OperationType


def print_section(title):
    """Print a formatted section header."""
    print("\n" + "=" * 70)
    print(f"🔬 {title}")
    print("=" * 70)


def test_bf_005_scenario():
    """
    Test BF_005: Archive operation false success reports.
    
    The bug: Agent reported files as archived but they weren't actually moved.
    """
    print_section("Testing BF_005 Scenario: False Archive Success Reports")
    
    # Setup test environment
    test_base = Path("test_bf_005")
    test_base.mkdir(exist_ok=True)
    
    items_dir = test_base / "items"
    items_dir.mkdir(exist_ok=True)
    
    archive_dir = test_base / "archive" / "2025-Q3"
    archive_dir.mkdir(parents=True, exist_ok=True)
    
    # Create test files
    test_items = ["TD_014_Test_Item.md", "TD_015_Another_Item.md", "TD_020_Third_Item.md"]
    for item_name in test_items:
        item_path = items_dir / item_name
        item_path.write_text(f"# {item_name}\n\nTest content for {item_name}")
        print(f"✅ Created test file: {item_name}")
    
    # Initialize verifier
    verifier = FileOperationVerifier(base_path=test_base)
    
    # Test archiving with verification
    print("\n📦 Attempting to archive items WITH verification...")
    
    results = []
    for item_name in test_items:
        source = items_dir / item_name
        destination = archive_dir / f"2025_08_16-{item_name}"
        
        print(f"\n  Moving {item_name}...")
        success = verifier.move_with_verification(source, destination)
        results.append((item_name, success))
        
        # Verify the actual state
        source_exists = source.exists()
        dest_exists = destination.exists()
        
        print(f"    Verification result: {'✅ PASSED' if success else '❌ FAILED'}")
        print(f"    Source still exists: {source_exists}")
        print(f"    Destination exists: {dest_exists}")
        
        if success and not source_exists and dest_exists:
            print(f"    ✅ {item_name} correctly archived")
        else:
            print(f"    ❌ {item_name} archive verification failed!")
    
    # Summary
    successful = sum(1 for _, success in results if success)
    print(f"\n📊 Results: {successful}/{len(results)} items successfully archived")
    
    # Cleanup
    try:
        shutil.rmtree(test_base)
        print("🧹 Test environment cleaned up")
    except Exception as e:
        print(f"⚠️  Cleanup failed: {e}")
    
    return successful == len(results)


def test_bf_006_scenario():
    """
    Test BF_006: Duplicate files in both items and archive.
    
    The bug: Files were copied instead of moved, creating duplicates.
    """
    print_section("Testing BF_006 Scenario: Duplicate File Prevention")
    
    # Setup test environment
    test_base = Path("test_bf_006")
    test_base.mkdir(exist_ok=True)
    
    items_dir = test_base / "items"
    items_dir.mkdir(exist_ok=True)
    
    archive_dir = test_base / "archive" / "2025-Q3"
    archive_dir.mkdir(parents=True, exist_ok=True)
    
    # Create test file
    test_file = items_dir / "TD_019_Test_Duplication.md"
    test_file.write_text("# TD_019\n\nThis file should not be duplicated")
    print(f"✅ Created test file: {test_file.name}")
    
    # Initialize verifier
    verifier = FileOperationVerifier(base_path=test_base)
    
    # Simulate the duplication bug
    print("\n🔍 Testing duplicate prevention...")
    
    destination = archive_dir / f"2025_08_16-{test_file.name}"
    
    # First, do a proper move with verification
    print("\n1️⃣  First move (should succeed):")
    success1 = verifier.move_with_verification(test_file, destination)
    print(f"   Result: {'✅ SUCCESS' if success1 else '❌ FAILED'}")
    
    # Check state
    source_exists = test_file.exists()
    dest_exists = destination.exists()
    print(f"   Source exists: {source_exists}")
    print(f"   Destination exists: {dest_exists}")
    
    if not source_exists and dest_exists:
        print("   ✅ File correctly moved (not duplicated)")
    else:
        print("   ❌ Unexpected state!")
    
    # Now try to create a duplicate (simulate the bug)
    print("\n2️⃣  Attempting to create duplicate:")
    
    # Recreate source file (simulating incomplete move)
    test_file.write_text("# TD_019\n\nDuplicate attempt")
    print("   Recreated source file")
    
    # Try to move again (should fail because destination exists)
    success2 = verifier.move_with_verification(test_file, destination)
    print(f"   Result: {'❌ FAILED (as expected)' if not success2 else '✅ SUCCESS (unexpected!)'}")
    
    # Final state check
    final_source_exists = test_file.exists()
    final_dest_exists = destination.exists()
    
    print(f"\n📊 Final state:")
    print(f"   Source exists: {final_source_exists}")
    print(f"   Destination exists: {final_dest_exists}")
    
    if final_dest_exists and final_source_exists:
        # Check if they're different
        dest_content = destination.read_text()
        source_content = test_file.read_text()
        if dest_content != source_content:
            print("   ✅ Duplicate prevention working (files have different content)")
            test_passed = True
        else:
            print("   ⚠️  Both files exist with same content")
            test_passed = False
    elif final_dest_exists and not final_source_exists:
        print("   ✅ Only destination exists (correct)")
        test_passed = True
    else:
        print("   ❌ Unexpected final state")
        test_passed = False
    
    # Cleanup
    try:
        shutil.rmtree(test_base)
        print("\n🧹 Test environment cleaned up")
    except Exception as e:
        print(f"⚠️  Cleanup failed: {e}")
    
    return test_passed


def test_rollback_capability():
    """
    Test rollback capability when verification fails.
    """
    print_section("Testing Rollback Capability")
    
    # Setup test environment
    test_base = Path("test_rollback")
    test_base.mkdir(exist_ok=True)
    
    items_dir = test_base / "items"
    items_dir.mkdir(exist_ok=True)
    
    # Create test file
    test_file = items_dir / "BF_003_Test_Rollback.md"
    original_content = "# BF_003\n\nOriginal content that should be preserved"
    test_file.write_text(original_content)
    print(f"✅ Created test file: {test_file.name}")
    
    # Initialize verifier with rollback enabled
    verifier = FileOperationVerifier(base_path=test_base, enable_rollback=True)
    
    # Test 1: Try to move to a destination that already exists (should fail and rollback)
    archive_dir = test_base / "archive"
    archive_dir.mkdir(exist_ok=True)
    
    destination = archive_dir / "existing_file.md"
    destination.write_text("This file already exists")
    print("✅ Created existing destination file to trigger failure")
    
    print("\n🔄 Testing rollback on failed move (destination exists)...")
    
    # Store original state
    original_exists = test_file.exists()
    original_size = test_file.stat().st_size if original_exists else 0
    
    # Attempt move to existing destination
    # This should fail and trigger rollback
    success = verifier.move_with_verification(test_file, destination)
    
    print(f"\n📊 Results:")
    print(f"   Move operation: {'❌ FAILED (expected)' if not success else '✅ SUCCESS (unexpected)'}")
    
    # Check if file was restored
    restored_exists = test_file.exists()
    if restored_exists:
        restored_content = test_file.read_text()
        content_matches = restored_content == original_content
        print(f"   Source file exists: ✅")
        print(f"   Content preserved: {'✅' if content_matches else '❌'}")
        test_passed = not success and content_matches  # Should fail but preserve file
    else:
        print(f"   Source file exists: ❌")
        test_passed = False
    
    # Cleanup
    try:
        shutil.rmtree(test_base)
        print("\n🧹 Test environment cleaned up")
    except Exception as e:
        print(f"⚠️  Cleanup failed: {e}")
    
    return test_passed


def test_comprehensive_logging():
    """
    Test that comprehensive logging is working.
    """
    print_section("Testing Comprehensive Logging")
    
    # Setup test environment
    test_base = Path("test_logging")
    test_base.mkdir(exist_ok=True)
    
    # Initialize verifier
    verifier = FileOperationVerifier(base_path=test_base)
    
    # Create test file
    test_file = test_base / "test_log.md"
    test_file.write_text("# Test Logging\n\nContent for logging test")
    
    dest_file = test_base / "archived_log.md"
    
    print("\n📝 Performing operation with logging...")
    success = verifier.move_with_verification(test_file, dest_file)
    
    # Check if logs were created
    log_file = test_base / "scripts" / "verification_log.json"
    log_exists = log_file.exists()
    
    print(f"\n📊 Logging results:")
    print(f"   Operation success: {'✅' if success else '❌'}")
    print(f"   Log file created: {'✅' if log_exists else '❌'}")
    
    if log_exists:
        import json
        log_data = json.loads(log_file.read_text())
        print(f"   Operations logged: {len(log_data.get('operations', []))}")
        print(f"   Errors logged: {len(log_data.get('errors', []))}")
        print(f"   Verification failures: {len(log_data.get('verification_failures', []))}")
        
        # Show sample log entry
        if log_data.get('operations'):
            op = log_data['operations'][0]
            print(f"\n   Sample log entry:")
            print(f"     Timestamp: {op.get('timestamp')}")
            print(f"     Operation: {op.get('operation')}")
            print(f"     Success: {op.get('success')}")
    
    # Get and display verification report
    print("\n📈 Verification Report:")
    report = verifier.get_verification_report()
    print(f"   Total operations: {report['summary']['total_operations']}")
    print(f"   Success rate: {report['summary']['success_rate']:.1f}%")
    
    # Cleanup
    try:
        shutil.rmtree(test_base)
        print("\n🧹 Test environment cleaned up")
    except Exception as e:
        print(f"⚠️  Cleanup failed: {e}")
    
    return log_exists and success


def run_all_tests():
    """
    Run all test scenarios.
    """
    print("\n" + "█" * 70)
    print("🚀 BACKLOG MAINTAINER VERIFICATION SYSTEM - COMPREHENSIVE TEST SUITE")
    print("█" * 70)
    print(f"\nTimestamp: {datetime.now().isoformat()}")
    print("Testing TD_021 fixes for BF_003, BF_005, BF_006 incidents")
    
    tests = [
        ("BF_005: False Archive Success", test_bf_005_scenario),
        ("BF_006: Duplicate File Prevention", test_bf_006_scenario),
        ("Rollback Capability", test_rollback_capability),
        ("Comprehensive Logging", test_comprehensive_logging)
    ]
    
    results = []
    for test_name, test_func in tests:
        try:
            passed = test_func()
            results.append((test_name, passed))
        except Exception as e:
            print(f"\n❌ Test '{test_name}' crashed: {e}")
            results.append((test_name, False))
    
    # Final summary
    print("\n" + "█" * 70)
    print("📊 TEST SUITE SUMMARY")
    print("█" * 70)
    
    for test_name, passed in results:
        status = "✅ PASSED" if passed else "❌ FAILED"
        print(f"  {test_name}: {status}")
    
    total_passed = sum(1 for _, passed in results if passed)
    total_tests = len(results)
    
    print(f"\n🎯 Overall: {total_passed}/{total_tests} tests passed")
    
    if total_passed == total_tests:
        print("\n🎉 ALL TESTS PASSED! The verification system is working correctly.")
        print("✅ TD_021 fixes have been successfully implemented.")
        return 0
    else:
        print("\n⚠️  Some tests failed. Please review the output above.")
        return 1


if __name__ == "__main__":
    sys.exit(run_all_tests())