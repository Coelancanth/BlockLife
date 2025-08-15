# Build and Development Commands

*This content was delegated from CLAUDE.md to the DevOps Engineer agent.*

## Building the Project
```bash
dotnet build                    # Build the entire solution
dotnet build --configuration Release  # Release build
```

## Running Tests (TDD Workflow)

### 🚀 **Automated Test Monitoring (Recommended)**
```bash
# BEST PRACTICE: Use automated test watcher during development
.\test-watch.bat                    # Runs every 10s, auto-stops after 30min inactivity

# Or with custom settings:
python scripts/test_monitor.py --continuous --interval 5 --timeout 60

# Single test run with file output (for Claude Code collaboration)
python scripts/test_monitor.py      # Creates test-summary.md and test-results.json
```

**Benefits of Automated Monitoring:**
- ✅ No manual copy-pasting of test results for Claude Code
- ✅ Auto-stops after inactivity (prevents zombie processes)
- ✅ Structured output in both markdown and JSON formats
- ✅ Tracks file changes and shows time until auto-stop
- ✅ Perfect for TDD workflow with continuous feedback

### Manual Test Commands
```bash
# WORKFLOW STEP 1: Architecture fitness tests (run FIRST)
dotnet test --filter "FullyQualifiedName~Architecture"

# WORKFLOW STEP 2: TDD Red-Green-Refactor cycle
dotnet test tests/BlockLife.Core.Tests.csproj --filter "Category=Unit"

# WORKFLOW STEP 3: Property-based mathematical proofs  
dotnet test --filter "FullyQualifiedName~PropertyTests"

# WORKFLOW STEP 4: All core tests together
dotnet test tests/BlockLife.Core.Tests.csproj

# WORKFLOW STEP 5: Integration tests (GdUnit4 - requires Godot)
addons/gdUnit4/runtest.cmd --godot_bin "path/to/godot.exe"

# Or set environment variable and run
set GODOT_BIN=C:\path\to\godot.exe
addons\gdUnit4\runtest.cmd

# QUALITY GATE: Full validation pipeline
dotnet build && dotnet test tests/BlockLife.Core.Tests.csproj && echo "✅ Ready for commit"

# AUTOMATION: Enhanced quality pipeline with cognitive load reduction
dotnet build && dotnet test tests/BlockLife.Core.Tests.csproj && python scripts/collect_test_metrics.py --update-docs && echo "✅ Ready for commit"
```

### Godot Project Commands
- Main project file: `project.godot`
- Main scene: Located in `godot_project/scenes/Main/main.tscn`
- The game uses Godot 4.4 with C# support enabled

### Godot Export Commands
```bash
# Export for Windows
godot --headless --export-release "Windows Desktop" build/BlockLife.exe

# Export with custom preset
godot --headless --export-release [preset_name] [output_path]
```

## Environment Setup

### Required Tools
- .NET 8.0 SDK
- Godot 4.4 with C# support
- Python 3.8+ (for automation scripts)
- Git

### Environment Variables
```bash
# For GdUnit4 integration tests
set GODOT_BIN=C:\path\to\godot.exe

# For development
set DOTNET_CLI_TELEMETRY_OPTOUT=1
```

## Automation Integration

### Test Monitor
```bash
# Single run - generates test-summary.md and test-results.json
python scripts/test_monitor.py

# Continuous monitoring with auto-timeout
python scripts/test_monitor.py --continuous --interval 10 --timeout 30
```

### Git Workflow Automation
```bash
# Setup automatic Git workflow enforcement
python scripts/setup_git_hooks.py
```

### Documentation Automation
```bash
# Keep all documentation tracking files synchronized
python scripts/sync_documentation_status.py

# Automatically update documentation with test statistics
python scripts/collect_test_metrics.py --update-docs
```

## Performance and Optimization

### Build Optimization
- Use Release configuration for performance testing
- Enable parallel builds: `dotnet build -m`
- Use incremental builds during development

### Test Execution Optimization
- Run architecture tests first (fastest feedback)
- Use filtered test runs for specific categories
- Leverage parallel test execution where safe