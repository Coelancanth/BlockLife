@echo off
echo ========================================
echo Running BlockLife Integration Tests
echo ========================================
echo.

set GODOT_BIN=C:\Users\Coel\Downloads\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64_console.exe

echo Building solution first...
dotnet build BlockLife.sln
if %ERRORLEVEL% NEQ 0 (
    echo Build failed! Please fix compilation errors first.
    exit /b %ERRORLEVEL%
)

echo.
echo Running integration tests in headless mode...
echo.

REM Run the tests with GdUnit4
call addons\gdUnit4\runtest.cmd --godot_bin "%GODOT_BIN%" -a test/integration --headless -c -rd reports

echo.
echo ========================================
echo Test execution completed!
echo Check reports folder for detailed results
echo ========================================