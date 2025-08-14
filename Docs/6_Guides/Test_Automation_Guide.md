# Test Automation Guide for BlockLife

## Overview

This guide documents the automated test monitoring system that streamlines the TDD workflow and enables efficient collaboration with Claude Code without manual copy-pasting of test results.

## Quick Start

### Recommended Workflow

```bash
# Start test watcher for development session
.\test-watch.bat

# This automatically:
# - Runs all tests every 10 seconds
# - Monitors file changes in src/, tests/, and godot_project/
# - Auto-stops after 30 minutes of inactivity
# - Generates test-summary.md and test-results.json
```

### For Claude Code Collaboration

When working with Claude Code, simply run:
```bash
python scripts/test_monitor.py
```

Then Claude Code can read the results directly from:
- `test-summary.md` - Human-readable test results
- `test-results.json` - Structured data for parsing

No more copy-pasting test output!

## Test Monitor Features

### Core Capabilities

1. **Automated Test Execution**
   - Runs both unit tests and integration tests
   - Configurable interval between test runs
   - Supports headless mode for CI/CD pipelines

2. **Smart File Monitoring**
   - Watches `src/`, `tests/`, and `godot_project/` directories
   - Detects changes to `.cs` files
   - Shows when files were last modified

3. **Auto-Timeout Protection**
   - Prevents zombie processes
   - Default: 30 minutes of inactivity
   - Shows countdown timer
   - Configurable timeout duration

4. **Structured Output**
   - Markdown summary for human reading
   - JSON format for tool parsing
   - Separate logs for debugging

### Command Line Options

```bash
# Basic usage
python scripts/test_monitor.py [options]

Options:
  --continuous, -c     Run tests continuously
  --interval N, -i N   Seconds between test runs (default: 5)
  --timeout M, -t M    Minutes until auto-stop (default: 30)
  --headless          Run integration tests in headless mode
```

### Usage Examples

```bash
# Single test run
python scripts/test_monitor.py

# Continuous with 5-second interval
python scripts/test_monitor.py -c --interval 5

# Long session with 60-minute timeout
python scripts/test_monitor.py -c --timeout 60

# Headless mode for CI
python scripts/test_monitor.py --headless

# Quick development cycle (3s interval, 15min timeout)
python scripts/test_monitor.py -c -i 3 -t 15
```

## Output Files

### test-summary.md

Human-readable markdown format:
```markdown
# Test Results Summary

**Generated:** 2025-08-14 15:45:22

## Unit Tests
**PASSED**

## Integration Tests
- **Total Tests:** 22
- **Passed:** 22
- **Failed:** 0
- **Errors:** 0
- **Skipped:** 0
- **Duration:** 151ms

### Test Suites Run:
- res://test/integration/features/block_placement/BlockPlacementIntegrationTest.cs
- [additional suites...]

## Overall Status
**All tests passed!**
```

### test-results.json

Structured JSON for programmatic access:
```json
{
  "timestamp": "2025-08-14T15:45:22",
  "unit": {
    "type": "unit",
    "success": true,
    "output": "...",
    "errors": ""
  },
  "integration": {
    "type": "integration",
    "success": true,
    "summary": {
      "total": 22,
      "passed": 22,
      "failed": 0,
      "errors": 0,
      "skipped": 0,
      "duration": 151,
      "suites": [...]
    }
  }
}
```

### test-output.log

Full test output with ANSI color codes for debugging.

## Integration with TDD Workflow

### Red-Green-Refactor Cycle

1. **Start Test Monitor**
   ```bash
   .\test-watch.bat
   ```

2. **Write Failing Test (RED)**
   - Create your test file
   - Monitor shows test failure in `test-summary.md`
   - Time remaining shows: `(timeout in 29.5 min)`

3. **Implement Code (GREEN)**
   - Write minimal code to pass
   - Monitor automatically detects changes
   - Shows: `Detected file changes at 15:30:45`
   - Test results update to show passing

4. **Refactor (REFACTOR)**
   - Improve code while keeping tests green
   - Monitor ensures tests stay passing
   - Auto-stops after 30 minutes if you take a break

### Benefits for TDD

- **Instant Feedback**: See test results within seconds of saving
- **No Context Switching**: No need to manually run test commands
- **Activity Tracking**: Know exactly when files changed
- **Auto-Cleanup**: Process stops automatically when done
- **Claude Code Integration**: AI assistant can read results directly

## Troubleshooting

### Common Issues

**Issue: "Could not find assembly" errors**
- Solution: Ensure `dotnet build` completes successfully first
- The test monitor runs build automatically

**Issue: Integration tests show "Skipping test - not in proper Godot context"**
- Solution: Run tests in editor mode (not headless) for full UI testing
- Use `--headless` flag only for CI/CD pipelines

**Issue: Auto-timeout too aggressive**
- Solution: Increase timeout with `--timeout 60` for longer sessions
- Or touch a file periodically to reset the timer

**Issue: Tests not detecting changes**
- Solution: Ensure you're modifying files in watched directories
- Check that file extensions are `.cs`

### Best Practices

1. **Start monitor at beginning of session**
   ```bash
   .\test-watch.bat  # First thing when you start coding
   ```

2. **Use appropriate intervals**
   - Development: 5-10 seconds
   - CI/CD: Single run (no `--continuous`)
   - Debugging: 3 seconds for rapid feedback

3. **Configure timeout for your workflow**
   - Short tasks: 15 minutes
   - Feature development: 30-45 minutes
   - Full day work: 60+ minutes

4. **Check output files for Claude Code**
   - Always generate before asking for help
   - Reference specific test failures from summary

## Advanced Configuration

### Custom File Watchers

Edit `test_monitor.py` to add more watched directories:
```python
watched_dirs = [
    self.project_root / "src",
    self.project_root / "tests",
    self.project_root / "godot_project",
    self.project_root / "your_custom_dir"  # Add custom paths
]
```

### Integration with CI/CD

```yaml
# GitHub Actions example
- name: Run Tests with Monitor
  run: |
    python scripts/test_monitor.py --headless
    cat test-summary.md  # Display results
```

### Git Hooks Integration

Add to `.git/hooks/pre-commit`:
```bash
#!/bin/bash
# Run test monitor before commit
python scripts/test_monitor.py
if [ $? -ne 0 ]; then
    echo "Tests failed! Check test-summary.md"
    exit 1
fi
```

## Summary

The test automation system significantly improves the development workflow by:

1. **Eliminating manual test execution** during TDD cycles
2. **Providing structured output** for both humans and tools
3. **Preventing resource waste** with smart auto-timeout
4. **Enabling efficient AI collaboration** without copy-paste
5. **Maintaining continuous quality feedback** throughout development

Use `.\test-watch.bat` as your default development companion!