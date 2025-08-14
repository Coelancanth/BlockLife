# BlockLife Integration Tests

This folder contains GdUnit4 integration tests for the BlockLife project.

## Quick Start

### 1. Configure Godot Path
Create `local_settings.cmd` in this directory with your Godot path:
```cmd
set GODOT_BIN=C:\Path\To\Godot_v4.4.1-stable_mono_win64_console.exe
```

### 2. Run Tests
```cmd
run_integration_tests.cmd
```

## Test Structure

```
test/
├── integration/          # Integration test suites
│   └── features/        # Feature-specific tests
│       └── block_placement/
├── run_integration_tests.cmd  # Test runner script
└── local_settings.cmd   # Your local Godot path (git ignored)
```

## Writing New Tests

1. Create a new test file in appropriate folder
2. Use `[TestSuite]` attribute for test classes
3. Use `[TestCase]` attribute for test methods
4. Follow patterns in existing tests

## Integration Test Categories

- **Feature Tests**: End-to-end user flows
- **Presenter Tests**: MVP communication validation
- **Performance Tests**: Load and stress testing
- **Regression Tests**: Bug prevention tests

## Running in Godot Editor

1. Open project in Godot
2. Go to Project → Tools → GdUnit4
3. Navigate to test folder
4. Run tests directly from editor

## CI/CD Integration

Integration tests are configured in `.github/workflows/ci.yml` but disabled by default.
To enable in CI, uncomment the `integration-tests` job.

## Troubleshooting

- **Godot not found**: Update path in `local_settings.cmd`
- **Tests not discovered**: Ensure test files have `[TestSuite]` attribute
- **Async issues**: Use `await _sceneTree.ProcessFrame()` and `Task.Delay()`

For more details, see [Integration Testing Guide](../Docs/1_Architecture/Integration_Testing_Guide.md)