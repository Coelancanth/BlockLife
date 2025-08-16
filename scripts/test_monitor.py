#!/usr/bin/env python3
"""
Test Monitor - Automated test runner with structured output for Claude Code
Provides file-based communication for test results without manual copy-pasting
"""

import json
import subprocess
import time
import os
import sys
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Optional
import argparse

class TestMonitor:
    def __init__(self, project_root: Path):
        self.project_root = project_root
        self.results_file = project_root / "test-results.json"
        self.log_file = project_root / "test-output.log"
        self.summary_file = project_root / "test-summary.md"
        
    def run_unit_tests(self) -> Dict:
        """Run unit tests and capture results"""
        print("Running unit tests...")
        result = subprocess.run(
            ["dotnet", "test", "tests/BlockLife.Core.Tests.csproj"],
            capture_output=True,
            text=True,
            cwd=self.project_root
        )
        
        return {
            "type": "unit",
            "success": result.returncode == 0,
            "output": result.stdout,
            "errors": result.stderr
        }
    
    def run_integration_tests(self, headless: bool = False) -> Dict:
        """Run integration tests using GdUnit4"""
        print("Running integration tests...")
        
        godot_bin = r"C:\Users\Coel\Downloads\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64.exe"
        if headless:
            godot_bin = godot_bin.replace(".exe", "_console.exe")
        
        cmd = [
            "powershell", "-Command",
            f"cd '{self.project_root}'; .\\addons\\gdUnit4\\runtest.cmd --godot_bin '{godot_bin}' -a test/integration"
        ]
        
        if headless:
            cmd[-1] += " --headless --ignoreHeadlessMode"
        
        result = subprocess.run(cmd, capture_output=True, text=True)
        
        # Parse output for test results
        output_lines = result.stdout.split('\n')
        test_summary = self._parse_test_output(output_lines)
        
        return {
            "type": "integration",
            "success": result.returncode == 0,
            "summary": test_summary,
            "output": result.stdout,
            "errors": result.stderr
        }
    
    def _parse_test_output(self, lines: List[str]) -> Dict:
        """Parse GdUnit4 output for test metrics"""
        import re
        
        summary = {
            "total": 0,
            "passed": 0,
            "failed": 0,
            "errors": 0,
            "skipped": 0,
            "duration": 0,
            "suites": []
        }
        
        # Remove ANSI color codes
        ansi_escape = re.compile(r'\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])')
        
        for line in lines:
            # Remove ANSI codes from line
            clean_line = ansi_escape.sub('', line)
            
            # Parse overall statistics
            if "tests cases" in clean_line and "|" in clean_line:
                parts = clean_line.split("|")
                for part in parts:
                    # Extract numbers from each part
                    numbers = re.findall(r'\d+', part)
                    if "tests cases" in part and numbers:
                        summary["total"] = int(numbers[0])
                    elif "errors" in part and numbers:
                        summary["errors"] = int(numbers[0])
                    elif "failures" in part and numbers:
                        summary["failed"] = int(numbers[0])
                    elif "skipped" in part and numbers:
                        summary["skipped"] = int(numbers[0])
            
            # Parse execution time
            if "Total execution time:" in clean_line:
                numbers = re.findall(r'\d+', clean_line)
                if numbers:
                    summary["duration"] = int(numbers[-1])
            
            # Track test suites
            if "Run Test Suite:" in clean_line:
                suite_name = clean_line.split("Run Test Suite:")[-1].strip()
                summary["suites"].append(suite_name)
        
        summary["passed"] = summary["total"] - summary["failed"] - summary["errors"] - summary["skipped"]
        return summary
    
    def write_summary(self, unit_results: Dict, integration_results: Dict):
        """Write a markdown summary file for easy reading"""
        with open(self.summary_file, 'w') as f:
            f.write("# Test Results Summary\n\n")
            f.write(f"**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n\n")
            
            # Unit Tests
            f.write("## Unit Tests\n")
            if unit_results["success"]:
                f.write("**PASSED**\n\n")
            else:
                f.write("**FAILED**\n\n")
                if unit_results.get("errors"):
                    f.write("```\n")
                    f.write(unit_results["errors"][:1000])  # First 1000 chars of errors
                    f.write("\n```\n\n")
            
            # Integration Tests
            f.write("## Integration Tests\n")
            if integration_results.get("summary"):
                s = integration_results["summary"]
                f.write(f"- **Total Tests:** {s['total']}\n")
                f.write(f"- **Passed:** {s['passed']}\n")
                f.write(f"- **Failed:** {s['failed']}\n")
                f.write(f"- **Errors:** {s['errors']}\n")
                f.write(f"- **Skipped:** {s['skipped']}\n")
                f.write(f"- **Duration:** {s['duration']}ms\n\n")
                
                if s['suites']:
                    f.write("### Test Suites Run:\n")
                    for suite in s['suites']:
                        f.write(f"- {suite}\n")
            
            # Overall Status
            f.write("\n## Overall Status\n")
            if unit_results["success"] and integration_results["success"]:
                f.write("**All tests passed!**\n")
            else:
                f.write("**Some tests failed - review output above**\n")
    
    def continuous_mode(self, interval: int = 5, timeout_minutes: int = 30):
        """Run tests continuously and update results files
        
        Args:
            interval: Seconds between test runs
            timeout_minutes: Auto-stop after this many minutes of no file changes
        """
        print(f"Starting continuous test monitoring (interval: {interval}s)")
        print(f"Auto-timeout after {timeout_minutes} minutes of inactivity")
        print("Results will be written to:")
        print(f"  - {self.summary_file}")
        print(f"  - {self.results_file}")
        print("\nPress Ctrl+C to stop\n")
        
        # Track last modification times of source files
        last_file_change = datetime.now()
        watched_dirs = [
            self.project_root / "src",
            self.project_root / "tests",
            self.project_root / "godot_project"
        ]
        
        def get_latest_modification():
            """Get the most recent modification time from watched directories"""
            latest = datetime.fromtimestamp(0)
            for dir_path in watched_dirs:
                if not dir_path.exists():
                    continue
                for file_path in dir_path.rglob("*.cs"):
                    try:
                        mtime = datetime.fromtimestamp(file_path.stat().st_mtime)
                        if mtime > latest:
                            latest = mtime
                    except:
                        pass
            return latest
        
        while True:
            try:
                # Check for file changes
                current_latest = get_latest_modification()
                if current_latest > last_file_change:
                    last_file_change = current_latest
                    print(f"Detected file changes at {current_latest.strftime('%H:%M:%S')}")
                
                # Check for timeout
                time_since_change = (datetime.now() - last_file_change).total_seconds() / 60
                if time_since_change > timeout_minutes:
                    print(f"\nAuto-stopping: No file changes for {timeout_minutes} minutes")
                    print(f"Last change was at {last_file_change.strftime('%H:%M:%S')}")
                    break
                
                # Run tests
                unit_results = self.run_unit_tests()
                integration_results = self.run_integration_tests(headless=True)
                
                # Write results
                all_results = {
                    "timestamp": datetime.now().isoformat(),
                    "unit": unit_results,
                    "integration": integration_results
                }
                
                with open(self.results_file, 'w') as f:
                    json.dump(all_results, f, indent=2)
                
                self.write_summary(unit_results, integration_results)
                
                # Show status with time remaining
                remaining = timeout_minutes - time_since_change
                print(f"Results updated at {datetime.now().strftime('%H:%M:%S')} (timeout in {remaining:.1f} min)")
                
                # Wait before next run
                time.sleep(interval)
                
            except KeyboardInterrupt:
                print("\nStopping test monitor")
                break
            except Exception as e:
                print(f"Error: {e}")
                time.sleep(interval)

def main():
    parser = argparse.ArgumentParser(description="Automated test monitor for BlockLife")
    parser.add_argument("--continuous", "-c", action="store_true", 
                       help="Run tests continuously")
    parser.add_argument("--interval", "-i", type=int, default=5,
                       help="Interval between test runs in continuous mode (seconds)")
    parser.add_argument("--timeout", "-t", type=int, default=30,
                       help="Auto-stop after this many minutes of no file changes (default: 30)")
    parser.add_argument("--headless", action="store_true",
                       help="Run integration tests in headless mode")
    
    args = parser.parse_args()
    
    project_root = Path(__file__).parent.parent
    monitor = TestMonitor(project_root)
    
    if args.continuous:
        monitor.continuous_mode(args.interval, args.timeout)
    else:
        # Run once
        print("Running tests once...")
        unit_results = monitor.run_unit_tests()
        integration_results = monitor.run_integration_tests(args.headless)
        
        # Write results
        all_results = {
            "timestamp": datetime.now().isoformat(),
            "unit": unit_results,
            "integration": integration_results
        }
        
        with open(monitor.results_file, 'w') as f:
            json.dump(all_results, f, indent=2)
        
        monitor.write_summary(unit_results, integration_results)
        
        print(f"\nResults written to:")
        print(f"  - {monitor.summary_file}")
        print(f"  - {monitor.results_file}")

if __name__ == "__main__":
    main()