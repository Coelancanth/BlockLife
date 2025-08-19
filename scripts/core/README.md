# Core Build Scripts

Essential build, clean, and run operations for BlockLife.

## Available Scripts

### Windows (PowerShell)
```powershell
# Core build operations
.\core\build.ps1 build      # Build the solution  
.\core\build.ps1 test       # Build + run tests (safe default)
.\core\build.ps1 test-only  # Run tests only (dev iteration)
.\core\build.ps1 clean      # Clean build artifacts
.\core\build.ps1 run        # Launch the game (requires Godot)
.\core\build.ps1 all        # Clean, build, and test
```

### Linux/Mac (Bash)
```bash
# Core build operations  
./core/build.sh build       # Build the solution
./core/build.sh test        # Build + run tests (safe default)
./core/build.sh test-only   # Run tests only (dev iteration)
./core/build.sh clean       # Clean build artifacts
./core/build.sh run         # Launch the game (requires Godot)
./core/build.sh all         # Clean, build, and test
```

## Usage Guidelines

### For Development
- Use `test-only` for rapid iteration (tests only)
- Use `test` before committing (builds + tests)

### For CI/CD
- GitHub Actions uses these scripts for consistent builds
- Pre-commit hooks use `test` to validate changes

## Design Principles

1. **Simple and Fast** - Direct dotnet commands, minimal overhead
2. **Safe Defaults** - `test` command builds first to catch Godot issues
3. **Cross-Platform** - Identical functionality on Windows/Linux/Mac
4. **CI Integration** - Used by both local development and GitHub Actions