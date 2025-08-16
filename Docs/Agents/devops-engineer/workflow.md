# DevOps Engineer Workflow

## Purpose
Define procedures for the DevOps Engineer to create CI/CD pipelines, Python automation scripts, manage deployments, and eliminate manual work through automation.

## 📚 Your Documentation References
**ALWAYS READ FIRST**: [Automation_Scripts_Guide.md](../../Workflows/Development/Automation_Scripts_Guide.md)

**Existing Automation Infrastructure**: 4,850+ lines of production Python automation
- **Backlog automation**: `scripts/auto_archive_completed.py` - Saves 5-10 min per item
- **Git workflow protection**: `scripts/enforce_git_workflow.py` - Prevents main branch commits
- **Documentation sync**: `scripts/sync_documentation_status.py` - Saves 30-60 min monthly
- **Test automation**: `scripts/collect_test_metrics.py` - Auto-updates test statistics

---

## Core Workflow Actions

### 1. Create Python Automation Script

**Trigger**: "Automate this task" or "Create script for..."

**Input Required**:
- Task to automate
- Frequency of task
- Current manual steps
- Desired outcome

**Steps**:

1. **Analyze Automation Opportunity**
   ```
   Questions to assess:
   - How often is this done?
   - How long does it take manually?
   - What are the error-prone steps?
   - What's the ROI of automation?
   ```

2. **Design Script Architecture**
   ```python
   # Script structure
   scripts/
   ├── task_automator.py      # Main script
   ├── config.yaml            # Configuration
   ├── requirements.txt       # Dependencies
   └── README.md             # Documentation
   ```

3. **Follow Verification-First Pattern** (Learned from TD_021)
   ```python
   #!/usr/bin/env python3
   """
   Automates: [task description]
   Saves: ~[X] minutes per run
   Usage: python scripts/task_automator.py [options]
   
   CRITICAL: Follow verification-first pattern for all file operations
   """
   
   import sys
   import logging
   from pathlib import Path
   import subprocess
   import click
   from rich.console import Console
   from rich.progress import track
   
   console = Console()
   logger = logging.getLogger(__name__)
   
   @click.command()
   @click.option('--dry-run', is_flag=True, help='Preview without executing')
   @click.option('--verbose', is_flag=True, help='Detailed output')
   @click.option('--config', default='config.yaml', help='Config file path')
   def main(dry_run, verbose, config):
       """Automates [task description]."""
       
       # Setup UTF-8 logging (Windows compatibility)
       if sys.platform == 'win32':
           import codecs
           sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')
           sys.stderr = codecs.getwriter('utf-8')(sys.stderr.buffer, 'strict')
       
       level = logging.DEBUG if verbose else logging.INFO
       logging.basicConfig(
           level=level, 
           format='%(asctime)s - %(levelname)s - %(message)s',
           handlers=[
               logging.StreamHandler(sys.stdout),
               logging.FileHandler('scripts/automation.log', mode='a', encoding='utf-8')
           ]
       )
       
       try:
           console.print("[bold green]Starting automation...[/]")
           
           # MANDATORY: Verification for all file operations
           from scripts.backlog_maintainer_verification import FileOperationVerifier
           verifier = FileOperationVerifier()
           
           # Example verified file operation:
           # success = verifier.move_with_verification(source, destination)
           # if not success:
           #     console.print(f"[red]Operation failed: {verifier.get_error_report()}[/]")
           #     sys.exit(1)
           
           # Load configuration
           config_data = load_config(config)
           
           # Execute automation steps
           for step in track(automation_steps, description="Processing..."):
               if not dry_run:
                   execute_step(step)
               else:
                   console.print(f"[yellow]Would execute: {step}[/]")
           
           console.print("[bold green]✅ Automation complete![/]")
           return 0
           
       except Exception as e:
           console.print(f"[bold red]❌ Error: {e}[/]")
           logger.exception("Automation failed")
           return 1
   
   def load_config(path):
       """Load configuration from YAML."""
       import yaml
       with open(path) as f:
           return yaml.safe_load(f)
   
   def execute_step(step):
       """Execute a single automation step."""
       # Implementation specific to task
       pass
   
   if __name__ == "__main__":
       sys.exit(main())
   ```

4. **Add Error Handling & Recovery**
   ```python
   def safe_execute(func):
       """Decorator for safe execution with retry."""
       def wrapper(*args, **kwargs):
           max_retries = 3
           for attempt in range(max_retries):
               try:
                   return func(*args, **kwargs)
               except Exception as e:
                   if attempt < max_retries - 1:
                       logger.warning(f"Attempt {attempt + 1} failed: {e}")
                       time.sleep(2 ** attempt)  # Exponential backoff
                   else:
                       raise
       return wrapper
   ```

5. **Create Documentation**
   ```markdown
   # Task Automator Script
   
   ## Purpose
   Automates [detailed description]
   
   ## Installation
   ```bash
   pip install -r scripts/requirements.txt
   ```
   
   ## Usage
   ```bash
   # Basic usage
   python scripts/task_automator.py
   
   # Dry run to preview
   python scripts/task_automator.py --dry-run
   
   # Verbose output
   python scripts/task_automator.py --verbose
   ```
   
   ## Time Savings
   - Manual process: ~15 minutes
   - Automated: ~1 minute
   - Savings per run: 14 minutes
   - Monthly savings: ~7 hours
   ```

**Output**: Python script with documentation and configuration

---

### 2. Setup CI/CD Pipeline

**Trigger**: "Setup CI/CD" or "Automate builds"

**Input Required**:
- Repository structure
- Build requirements
- Test suites
- Deployment targets

**Steps**:

1. **Create GitHub Actions Workflow**
   ```yaml
   # .github/workflows/ci-cd.yml
   name: CI/CD Pipeline
   
   on:
     push:
       branches: [main, develop]
     pull_request:
       branches: [main]
     workflow_dispatch:  # Manual trigger
   
   env:
     DOTNET_VERSION: '8.0.x'
     GODOT_VERSION: '4.4'
   
   jobs:
     build-and-test:
       runs-on: windows-latest
       
       steps:
       - name: Checkout code
         uses: actions/checkout@v4
         
       - name: Setup .NET
         uses: actions/setup-dotnet@v4
         with:
           dotnet-version: ${{ env.DOTNET_VERSION }}
       
       - name: Setup Python
         uses: actions/setup-python@v4
         with:
           python-version: '3.11'
           
       - name: Install Python dependencies
         run: |
           pip install -r scripts/requirements.txt
       
       - name: Run pre-build checks
         run: python scripts/pre_build_checks.py
       
       - name: Build solution
         run: dotnet build BlockLife.sln --configuration Release
       
       - name: Run architecture tests
         run: dotnet test --filter "FullyQualifiedName~Architecture"
       
       - name: Run unit tests
         run: dotnet test tests/BlockLife.Core.Tests.csproj
       
       - name: Generate test report
         run: python scripts/test_reporter.py
       
       - name: Upload test results
         uses: actions/upload-artifact@v3
         with:
           name: test-results
           path: test-results/
   
     quality-gates:
       needs: build-and-test
       runs-on: windows-latest
       
       steps:
       - name: Check code coverage
         run: python scripts/coverage_checker.py --min-coverage 80
       
       - name: Check architecture compliance
         run: python scripts/architecture_validator.py
       
       - name: Security scan
         run: python scripts/security_scanner.py
   ```

2. **Create Build Validation Script**
   ```python
   # scripts/pre_build_checks.py
   #!/usr/bin/env python3
   """Pre-build validation checks."""
   
   import sys
   import subprocess
   from pathlib import Path
   
   def check_dependencies():
       """Verify all dependencies are available."""
       checks = [
           ("dotnet", ["--version"]),
           ("git", ["--version"]),
       ]
       
       for cmd, args in checks:
           try:
               subprocess.run([cmd] + args, check=True, capture_output=True)
               print(f"✅ {cmd} found")
           except:
               print(f"❌ {cmd} not found")
               return False
       return True
   
   def check_file_structure():
       """Verify expected files exist."""
       required_files = [
           "BlockLife.sln",
           "project.godot",
           "tests/BlockLife.Core.Tests.csproj",
       ]
       
       for file in required_files:
           if not Path(file).exists():
               print(f"❌ Missing: {file}")
               return False
           print(f"✅ Found: {file}")
       return True
   
   if __name__ == "__main__":
       if not (check_dependencies() and check_file_structure()):
           sys.exit(1)
       print("✅ All pre-build checks passed")
   ```

**Output**: Complete CI/CD pipeline configuration

---

### 3. Create Test Automation Suite

**Trigger**: "Automate testing" or "Create test runner"

**Input Required**:
- Test frameworks used
- Test categories
- Reporting requirements

**Steps**:

1. **Enhanced Test Monitor Script**
   ```python
   # scripts/advanced_test_monitor.py
   import asyncio
   import json
   from datetime import datetime
   from pathlib import Path
   import subprocess
   from watchdog.observers import Observer
   from watchdog.events import FileSystemEventHandler
   
   class TestRunner:
       def __init__(self, config_path="test_config.json"):
           self.config = self.load_config(config_path)
           self.test_history = []
           
       def load_config(self, path):
           with open(path) as f:
               return json.load(f)
       
       async def run_test_suite(self, category=None):
           """Run test suite with optional filtering."""
           
           commands = [
               ("Architecture", 'dotnet test --filter "FullyQualifiedName~Architecture"'),
               ("Unit", 'dotnet test --filter "Category=Unit"'),
               ("Integration", 'dotnet test --filter "Category=Integration"'),
           ]
           
           results = {}
           for name, cmd in commands:
               if category and category != name:
                   continue
                   
               print(f"🧪 Running {name} tests...")
               start = datetime.now()
               
               result = subprocess.run(
                   cmd, shell=True, capture_output=True, text=True
               )
               
               duration = (datetime.now() - start).total_seconds()
               results[name] = {
                   "passed": "Passed!" in result.stdout,
                   "duration": duration,
                   "output": result.stdout
               }
               
           self.generate_report(results)
           return results
       
       def generate_report(self, results):
           """Generate test report in multiple formats."""
           
           # Markdown report
           with open("test-summary.md", "w") as f:
               f.write("# Test Results\n\n")
               f.write(f"Generated: {datetime.now()}\n\n")
               
               for category, result in results.items():
                   status = "✅" if result["passed"] else "❌"
                   f.write(f"## {status} {category} Tests\n")
                   f.write(f"- Duration: {result['duration']:.2f}s\n")
                   f.write(f"- Status: {'PASSED' if result['passed'] else 'FAILED'}\n\n")
           
           # JSON report for CI/CD
           with open("test-results.json", "w") as f:
               json.dump(results, f, indent=2)
   
   class TestWatcher(FileSystemEventHandler):
       """Watch for file changes and trigger tests."""
       
       def __init__(self, runner):
           self.runner = runner
           self.debounce_timer = None
           
       def on_modified(self, event):
           if event.src_path.endswith(('.cs', '.csproj')):
               self.trigger_tests()
       
       def trigger_tests(self):
           """Debounced test trigger."""
           if self.debounce_timer:
               self.debounce_timer.cancel()
           
           self.debounce_timer = asyncio.create_task(
               self.run_after_delay()
           )
       
       async def run_after_delay(self):
           await asyncio.sleep(2)  # Wait for file saves to complete
           await self.runner.run_test_suite()
   ```

**Output**: Comprehensive test automation system

---

### 4. Setup Deployment Pipeline

**Trigger**: "Setup deployment" or "Automate release"

**Input Required**:
- Deployment targets
- Environment configurations
- Release process

**Steps**:

1. **Create Release Builder Script**
   ```python
   # scripts/release_builder.py
   #!/usr/bin/env python3
   """Automated release builder for BlockLife."""
   
   import os
   import shutil
   import subprocess
   from pathlib import Path
   import zipfile
   from datetime import datetime
   import click
   
   @click.command()
   @click.option('--version', required=True, help='Version number (e.g., 1.2.0)')
   @click.option('--platform', default='windows', help='Target platform')
   @click.option('--config', default='Release', help='Build configuration')
   def build_release(version, platform, config):
       """Build and package a release."""
       
       print(f"🚀 Building BlockLife v{version} for {platform}")
       
       # Clean previous builds
       build_dir = Path("build")
       if build_dir.exists():
           shutil.rmtree(build_dir)
       build_dir.mkdir()
       
       # Build .NET solution
       print("📦 Building .NET solution...")
       subprocess.run([
           "dotnet", "build", "BlockLife.sln",
           "--configuration", config,
           "--output", str(build_dir / "dotnet")
       ], check=True)
       
       # Export Godot project
       print("🎮 Exporting Godot project...")
       godot_export_preset = {
           "windows": "Windows Desktop",
           "linux": "Linux/X11",
           "mac": "macOS"
       }[platform]
       
       subprocess.run([
           "godot",
           "--headless",
           "--export-release",
           godot_export_preset,
           str(build_dir / f"BlockLife.{platform}.exe")
       ], check=True)
       
       # Create release package
       print("📁 Creating release package...")
       release_name = f"BlockLife-v{version}-{platform}"
       release_path = build_dir / release_name
       release_path.mkdir()
       
       # Copy files
       shutil.copytree(build_dir / "dotnet", release_path / "bin")
       shutil.copy(build_dir / f"BlockLife.{platform}.exe", release_path)
       shutil.copy("README.md", release_path)
       shutil.copy("LICENSE", release_path)
       
       # Create zip
       zip_path = build_dir / f"{release_name}.zip"
       with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zf:
           for file in release_path.rglob('*'):
               zf.write(file, file.relative_to(release_path))
       
       print(f"✅ Release built: {zip_path}")
       print(f"📊 Size: {zip_path.stat().st_size / 1024 / 1024:.2f} MB")
       
       # Generate release notes
       generate_release_notes(version)
       
   def generate_release_notes(version):
       """Generate release notes from git history."""
       
       # Get commits since last tag
       result = subprocess.run([
           "git", "log", "--oneline", "--no-merges",
           f"v{version}^..HEAD"
       ], capture_output=True, text=True)
       
       with open(f"release-notes-v{version}.md", "w") as f:
           f.write(f"# Release Notes - v{version}\n\n")
           f.write(f"Release Date: {datetime.now().strftime('%Y-%m-%d')}\n\n")
           f.write("## Changes\n\n")
           
           for line in result.stdout.splitlines():
               commit_hash, message = line.split(' ', 1)
               f.write(f"- {message} ({commit_hash})\n")
   
   if __name__ == "__main__":
       build_release()
   ```

**Output**: Automated release pipeline

---

### 5. Create Development Environment Setup

**Trigger**: "Setup dev environment" or "Onboarding script"

**Input Required**:
- Required tools
- Dependencies
- Configuration needs

**Steps**:

1. **Create Environment Setup Script**
   ```python
   # scripts/setup_dev_env.py
   #!/usr/bin/env python3
   """One-command development environment setup."""
   
   import os
   import sys
   import subprocess
   import platform
   from pathlib import Path
   import urllib.request
   
   class DevEnvironmentSetup:
       def __init__(self):
           self.os_type = platform.system()
           self.errors = []
           
       def run(self):
           """Run complete setup process."""
           
           steps = [
               ("Checking prerequisites", self.check_prerequisites),
               ("Installing .NET SDK", self.install_dotnet),
               ("Setting up Godot", self.setup_godot),
               ("Installing Python packages", self.install_python_packages),
               ("Configuring Git hooks", self.setup_git_hooks),
               ("Running initial build", self.initial_build),
               ("Running tests", self.run_tests),
           ]
           
           for description, func in steps:
               print(f"\n📋 {description}...")
               if not func():
                   print(f"❌ Failed: {description}")
                   self.print_errors()
                   return False
               print(f"✅ Completed: {description}")
           
           print("\n🎉 Development environment ready!")
           self.print_next_steps()
           return True
       
       def check_prerequisites(self):
           """Check for required tools."""
           
           required = {
               "git": "Git version control",
               "python": "Python 3.8+",
           }
           
           for cmd, description in required.items():
               try:
                   subprocess.run([cmd, "--version"], 
                                check=True, capture_output=True)
                   print(f"  ✓ {description} found")
               except:
                   self.errors.append(f"Missing: {description}")
                   return False
           return True
       
       def install_dotnet(self):
           """Install or verify .NET SDK."""
           
           try:
               result = subprocess.run(["dotnet", "--version"], 
                                     capture_output=True, text=True)
               print(f"  ✓ .NET SDK found: {result.stdout.strip()}")
               return True
           except:
               print("  → Installing .NET SDK...")
               if self.os_type == "Windows":
                   # Download and run installer
                   installer_url = "https://dot.net/v1/dotnet-install.ps1"
                   subprocess.run([
                       "powershell", "-Command",
                       f"Invoke-WebRequest -Uri {installer_url} -OutFile dotnet-install.ps1; "
                       f"./dotnet-install.ps1 -Channel 8.0"
                   ])
               return True
       
       def setup_godot(self):
           """Setup Godot with .NET support."""
           
           godot_path = Path("godot/godot.exe")
           if not godot_path.exists():
               print("  → Downloading Godot 4.4...")
               # Download Godot
               # Setup in tools directory
           
           # Set environment variable
           os.environ["GODOT_BIN"] = str(godot_path.absolute())
           return True
       
       def install_python_packages(self):
           """Install required Python packages."""
           
           requirements = Path("scripts/requirements.txt")
           if requirements.exists():
               subprocess.run([
                   sys.executable, "-m", "pip", "install",
                   "-r", str(requirements)
               ], check=True)
           return True
       
       def setup_git_hooks(self):
           """Configure Git hooks."""
           
           hook_script = Path("scripts/setup_git_hooks.py")
           if hook_script.exists():
               subprocess.run([sys.executable, str(hook_script)], check=True)
           return True
       
       def initial_build(self):
           """Run initial build to verify setup."""
           
           return subprocess.run([
               "dotnet", "build", "BlockLife.sln"
           ]).returncode == 0
       
       def run_tests(self):
           """Run tests to verify everything works."""
           
           return subprocess.run([
               "dotnet", "test", "--filter", "Category=Unit"
           ]).returncode == 0
       
       def print_next_steps(self):
           """Print next steps for developer."""
           
           print("\n📚 Next Steps:")
           print("1. Run 'python scripts/test_monitor.py' for continuous testing")
           print("2. Check out the documentation in Docs/")
           print("3. Review CLAUDE.md for AI-assisted development")
           print("4. Create a feature branch to start developing")
   
   if __name__ == "__main__":
       setup = DevEnvironmentSetup()
       sys.exit(0 if setup.run() else 1)
   ```

**Output**: Complete environment setup automation

---

## Common Automation Patterns

### File Watching Pattern
```python
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler
import time

class AutoRunner(FileSystemEventHandler):
    def on_modified(self, event):
        if not event.is_directory:
            self.run_automation()
    
    def run_automation(self):
        # Your automation here
        pass

observer = Observer()
observer.schedule(AutoRunner(), path='src/', recursive=True)
observer.start()
```

### Parallel Task Execution
```python
import asyncio
from concurrent.futures import ThreadPoolExecutor

async def run_parallel_tasks():
    with ThreadPoolExecutor(max_workers=4) as executor:
        loop = asyncio.get_event_loop()
        tasks = [
            loop.run_in_executor(executor, task1),
            loop.run_in_executor(executor, task2),
            loop.run_in_executor(executor, task3),
        ]
        results = await asyncio.gather(*tasks)
    return results
```

### Configuration Management
```python
import yaml
from dataclasses import dataclass, field
from typing import List, Dict

@dataclass
class AutomationConfig:
    tasks: List[str] = field(default_factory=list)
    schedule: str = "*/10 * * * *"  # Cron format
    notifications: Dict[str, str] = field(default_factory=dict)
    
    @classmethod
    def from_yaml(cls, path):
        with open(path) as f:
            return cls(**yaml.safe_load(f))
```

---

## Quality Checklist

Before automation is complete:
- [ ] Script is idempotent (safe to run multiple times)?
- [ ] Error handling and recovery implemented?
- [ ] Logging and monitoring included?
- [ ] Documentation with examples provided?
- [ ] Time savings calculated and documented?
- [ ] Works on target platforms (Windows/Linux)?
- [ ] Configuration externalized?
- [ ] Tests for the automation itself?

---

## Response Templates

### When creating automation:
"🤖 Automation Script Created: [name].py

PURPOSE: Automates [task]
TIME SAVINGS: ~[X] minutes per run
LOCATION: scripts/[name].py

Features:
- [Feature 1]
- [Feature 2]

Usage:
```bash
python scripts/[name].py --help
```

ROI: Break-even after [N] uses"

### When setting up CI/CD:
"🚀 CI/CD Pipeline Configured

STAGES:
1. Build & Test (automated)
2. Quality Gates (enforced)
3. Deployment (ready)

Triggers:
- Push to main/develop
- Pull requests
- Manual dispatch

Dashboard: [GitHub Actions URL]"

### When optimizing process:
"⚡ Process Optimization Complete

BEFORE: [X] minutes manual work
AFTER: [Y] seconds automated
IMPROVEMENT: [Z]% reduction

Automation includes:
- [Step 1]
- [Step 2]

Monthly time saved: [hours]"