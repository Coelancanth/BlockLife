# BlockLife Build Scripts

Simple, no-complexity build automation for the BlockLife project.

## Quick Start

### Windows
```powershell
./scripts/build.ps1 test    # Run all tests
./scripts/build.ps1 build   # Build the solution
./scripts/build.ps1 all     # Clean, build, and test
```

### Linux/Mac
```bash
./scripts/build.sh test     # Run all tests
./scripts/build.sh build    # Build the solution
./scripts/build.sh all      # Clean, build, and test
```

## Available Commands

- `build` - Build the solution in Debug configuration
- `test` - Run all unit tests
- `clean` - Clean all build artifacts
- `run` - Launch the game (requires Godot)
- `all` - Clean, build, and test in sequence

## CI/CD Integration

These scripts are used by:
- GitHub Actions CI pipeline (`.github/workflows/ci.yml`)
- Local development workflow
- Pre-commit testing

## Design Principles

1. **Simple** - No complex dependency management
2. **Fast** - Direct dotnet commands, no overhead
3. **Portable** - Works on Windows, Linux, and Mac
4. **Focused** - Does one thing well

## Adding New Scripts

When adding automation:
1. Keep it simple - avoid over-engineering
2. Document usage clearly
3. Support both Windows and Linux when possible
4. Test locally before committing