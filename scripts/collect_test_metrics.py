#!/usr/bin/env python3
"""
Test Metrics Collector Script for BlockLife

This script collects test metrics from dotnet test runs and automatically updates
documentation files with current statistics to reduce cognitive load and manual maintenance.

Features:
- Parses dotnet test output for test counts and results
- Updates Documentation Catalogue with current statistics
- Updates Implementation Status Tracker with test coverage metrics
- Provides structured logging for integration with CI/CD
- Supports dry-run mode for validation

Usage:
    python scripts/collect_test_metrics.py [--update-docs] [--dry-run] [--verbose]
"""

import re
import subprocess
import json
import sys
import argparse
from pathlib import Path
from typing import Dict, List, Optional, Tuple
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
            logging.FileHandler('scripts/test_metrics.log', mode='a', encoding='utf-8')
        ]
    )

setup_logging()
logger = logging.getLogger(__name__)

@dataclass
class TestMetrics:
    """Test metrics data structure following functional programming principles"""
    total_tests: int
    passed_tests: int
    failed_tests: int
    skipped_tests: int
    architecture_tests: int
    unit_tests: int
    property_tests: int
    coverage_percentage: float
    execution_time: str
    timestamp: str

class TestMetricsCollector:
    """
    Collects test metrics using functional programming patterns
    Follows Clean Architecture principles with clear separation of concerns
    """
    
    def __init__(self, project_root: Path):
        self.project_root = project_root
        self.docs_path = project_root / "Docs"
        self.test_project_path = project_root / "tests" / "BlockLife.Core.Tests.csproj"
        
    def run_tests_and_collect_metrics(self, dry_run: bool = False) -> Optional[TestMetrics]:
        """
        Execute test suites and collect comprehensive metrics
        Returns None if tests fail or cannot be executed
        """
        logger.info("🧪 Starting test execution and metrics collection...")
        
        if dry_run:
            logger.info("🔄 DRY RUN MODE: Simulating test execution")
            return self._create_mock_metrics()
        
        try:
            # Run comprehensive test suite as defined in CLAUDE.md workflow
            metrics = self._execute_test_commands()
            logger.info(f"✅ Test metrics collected successfully: {metrics.total_tests} total tests")
            return metrics
            
        except subprocess.CalledProcessError as e:
            logger.error(f"❌ Test execution failed: {e}")
            return None
        except Exception as e:
            logger.error(f"❌ Unexpected error during test collection: {e}")
            return None

    def _execute_test_commands(self) -> TestMetrics:
        """Execute the exact test workflow from CLAUDE.md"""
        
        # WORKFLOW STEP 1: Architecture fitness tests (run FIRST)
        logger.info("📐 Running architecture fitness tests...")
        arch_result = self._run_command([
            "dotnet", "test", 
            "--filter", "FullyQualifiedName~Architecture",
            "--logger", "trx",
            "--verbosity", "quiet"
        ])
        
        # WORKFLOW STEP 2: Unit tests
        logger.info("🔬 Running unit tests...")
        unit_result = self._run_command([
            "dotnet", "test", str(self.test_project_path),
            "--filter", "Category=Unit",
            "--logger", "trx",
            "--verbosity", "quiet"
        ])
        
        # WORKFLOW STEP 3: Property-based tests
        logger.info("🎲 Running property-based tests...")
        property_result = self._run_command([
            "dotnet", "test",
            "--filter", "FullyQualifiedName~PropertyTests",
            "--logger", "trx", 
            "--verbosity", "quiet"
        ])
        
        # WORKFLOW STEP 4: All core tests together
        logger.info("🎯 Running complete test suite...")
        all_tests_result = self._run_command([
            "dotnet", "test", str(self.test_project_path),
            "--logger", "trx",
            "--verbosity", "normal"
        ])
        
        return self._parse_test_results(arch_result, unit_result, property_result, all_tests_result)

    def _run_command(self, cmd: List[str]) -> str:
        """Execute command and return output, following error handling patterns"""
        try:
            result = subprocess.run(
                cmd,
                cwd=self.project_root,
                capture_output=True,
                text=True,
                check=True,
                timeout=300  # 5 minute timeout
            )
            return result.stdout + result.stderr
        except subprocess.TimeoutExpired:
            logger.error(f"⏰ Command timed out: {' '.join(cmd)}")
            raise
        except subprocess.CalledProcessError as e:
            logger.error(f"❌ Command failed: {' '.join(cmd)}")
            logger.error(f"Return code: {e.returncode}")
            logger.error(f"Output: {e.stdout}")
            logger.error(f"Error: {e.stderr}")
            raise

    def _parse_test_results(self, arch_output: str, unit_output: str, 
                          property_output: str, all_output: str) -> TestMetrics:
        """Parse dotnet test output following functional patterns"""
        
        # Parse overall test counts from comprehensive run
        total_match = re.search(r'Total tests: (\d+)', all_output)
        passed_match = re.search(r'Passed: (\d+)', all_output)
        failed_match = re.search(r'Failed: (\d+)', all_output)
        skipped_match = re.search(r'Skipped: (\d+)', all_output)
        
        # Parse execution time
        time_match = re.search(r'Test Run Successful\.\s*Total tests: \d+\s*Passed: \d+[^)]*\)\s*\[([^\]]+)\]', all_output)
        
        # Count specific test categories
        arch_tests = self._count_tests_in_output(arch_output, "Architecture")
        unit_tests = self._count_tests_in_output(unit_output, "Unit")
        property_tests = self._count_tests_in_output(property_output, "Property")
        
        return TestMetrics(
            total_tests=int(total_match.group(1)) if total_match else 0,
            passed_tests=int(passed_match.group(1)) if passed_match else 0,
            failed_tests=int(failed_match.group(1)) if failed_match else 0,
            skipped_tests=int(skipped_match.group(1)) if skipped_match else 0,
            architecture_tests=arch_tests,
            unit_tests=unit_tests,
            property_tests=property_tests,
            coverage_percentage=0.0,  # TODO: Integrate with coverage tool
            execution_time=time_match.group(1) if time_match else "Unknown",
            timestamp=datetime.now().isoformat()
        )

    def _count_tests_in_output(self, output: str, category: str) -> int:
        """Count tests for specific category"""
        total_match = re.search(r'Total tests: (\d+)', output)
        return int(total_match.group(1)) if total_match else 0

    def _create_mock_metrics(self) -> TestMetrics:
        """Create mock metrics for dry run mode"""
        return TestMetrics(
            total_tests=75,
            passed_tests=75,
            failed_tests=0,
            skipped_tests=0,
            architecture_tests=16,
            unit_tests=50,
            property_tests=9,
            coverage_percentage=92.5,
            execution_time="2.34 sec",
            timestamp=datetime.now().isoformat()
        )

    def update_documentation(self, metrics: TestMetrics, dry_run: bool = False) -> bool:
        """
        Update documentation files with current metrics
        Returns True if successful, follows functional error handling
        """
        logger.info("📝 Updating documentation with latest metrics...")
        
        try:
            # Update Documentation Catalogue
            self._update_documentation_catalogue(metrics, dry_run)
            
            # Update Implementation Status Tracker  
            self._update_implementation_status_tracker(metrics, dry_run)
            
            logger.info("✅ Documentation updated successfully")
            return True
            
        except Exception as e:
            logger.error(f"❌ Failed to update documentation: {e}")
            return False

    def _update_documentation_catalogue(self, metrics: TestMetrics, dry_run: bool):
        """Update the documentation catalogue with current test statistics"""
        catalogue_path = self.docs_path / "DOCUMENTATION_CATALOGUE.md"
        
        if not catalogue_path.exists():
            raise FileNotFoundError(f"Documentation catalogue not found: {catalogue_path}")
        
        content = catalogue_path.read_text(encoding='utf-8')
        
        # Update current statistics section (lines around 156-160)
        stats_pattern = r'## 📊 Current Statistics\n- \*\*Total Tests\*\*: \d+ \(as of [^)]+\)\n- \*\*Architecture Tests\*\*: \d+\n- \*\*Unit Tests\*\*: \d+\n- \*\*Property Tests\*\*: \d+ \(\d+ validations\)'
        
        new_stats = f"""## 📊 Current Statistics
- **Total Tests**: {metrics.total_tests} (as of {datetime.now().strftime('%Y-%m-%d')})
- **Architecture Tests**: {metrics.architecture_tests}
- **Unit Tests**: {metrics.unit_tests}
- **Property Tests**: {metrics.property_tests} ({metrics.property_tests * 100} validations)"""
        
        updated_content = re.sub(stats_pattern, new_stats, content, flags=re.MULTILINE)
        
        if dry_run:
            logger.info(f"🔄 DRY RUN: Would update {catalogue_path}")
            logger.info(f"New stats section:\n{new_stats}")
        else:
            catalogue_path.write_text(updated_content, encoding='utf-8')
            logger.info(f"📝 Updated {catalogue_path}")

    def _update_implementation_status_tracker(self, metrics: TestMetrics, dry_run: bool):
        """Update implementation status tracker with test metrics"""
        tracker_path = self.docs_path / "0_Global_Tracker" / "Implementation_Status_Tracker.md"
        
        if not tracker_path.exists():
            raise FileNotFoundError(f"Implementation status tracker not found: {tracker_path}")
        
        content = tracker_path.read_text(encoding='utf-8')
        
        # Update test coverage metrics section
        metrics_pattern = r'### Test Coverage Metrics\n- \*\*Total Tests\*\*: \d+ \([^)]+\)\n- \*\*Architecture Tests\*\*: \d+ \([^)]+\)\n- \*\*Unit Tests\*\*: \d+ \([^)]+\)\n- \*\*Property Tests\*\*: \d+ \([^)]+\)'
        
        new_metrics = f"""### Test Coverage Metrics
- **Total Tests**: {metrics.total_tests} (updated after latest feature development)
- **Architecture Tests**: {metrics.architecture_tests} (comprehensive constraint enforcement)
- **Unit Tests**: {metrics.unit_tests} (including latest feature tests)
- **Property Tests**: {metrics.property_tests} ({metrics.property_tests * 100} mathematical validations)"""
        
        updated_content = re.sub(metrics_pattern, new_metrics, content, flags=re.MULTILINE)
        
        if dry_run:
            logger.info(f"🔄 DRY RUN: Would update {tracker_path}")
            logger.info(f"New metrics section:\n{new_metrics}")
        else:
            tracker_path.write_text(updated_content, encoding='utf-8')
            logger.info(f"📝 Updated {tracker_path}")

def main():
    """Main entry point following functional programming principles"""
    parser = argparse.ArgumentParser(
        description="Collect test metrics and update BlockLife documentation"
    )
    parser.add_argument(
        "--update-docs", 
        action="store_true",
        help="Update documentation files with collected metrics"
    )
    parser.add_argument(
        "--dry-run",
        action="store_true", 
        help="Run in dry-run mode (no actual changes)"
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
    
    logger.info("🚀 BlockLife Test Metrics Collector Starting...")
    logger.info(f"📂 Project root: {project_root}")
    
    if args.dry_run:
        logger.info("🔄 Running in DRY RUN mode - no changes will be made")
    
    # Initialize collector
    collector = TestMetricsCollector(project_root)
    
    # Collect metrics
    metrics = collector.run_tests_and_collect_metrics(dry_run=args.dry_run)
    
    if metrics is None:
        logger.error("❌ Failed to collect test metrics")
        sys.exit(1)
    
    # Display metrics
    logger.info("📊 Test Metrics Summary:")
    logger.info(f"   Total Tests: {metrics.total_tests}")
    logger.info(f"   Passed: {metrics.passed_tests}")
    logger.info(f"   Failed: {metrics.failed_tests}")
    logger.info(f"   Architecture Tests: {metrics.architecture_tests}")
    logger.info(f"   Unit Tests: {metrics.unit_tests}")
    logger.info(f"   Property Tests: {metrics.property_tests}")
    logger.info(f"   Execution Time: {metrics.execution_time}")
    
    # Update documentation if requested
    if args.update_docs:
        success = collector.update_documentation(metrics, dry_run=args.dry_run)
        if not success:
            logger.error("❌ Failed to update documentation")
            sys.exit(1)
    else:
        logger.info("📝 Use --update-docs to update documentation files")
    
    logger.info("✅ Test metrics collection completed successfully")

if __name__ == "__main__":
    main()