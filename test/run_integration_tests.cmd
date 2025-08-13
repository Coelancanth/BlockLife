@echo off
REM Integration Test Runner for BlockLife
REM This script runs GdUnit4 integration tests for the Godot project

echo ========================================
echo BlockLife Integration Test Runner
echo ========================================
echo.

REM Check if GODOT_BIN is set
if "%GODOT_BIN%"=="" (
    echo ERROR: GODOT_BIN environment variable is not set!
    echo Please set it to your Godot executable path.
    echo Example: set GODOT_BIN=C:\Program Files\Godot\Godot.exe
    exit /b 1
)

echo Using Godot: %GODOT_BIN%
echo.

REM Run integration tests
echo Running integration tests...
echo ----------------------------------------

REM Run all tests in the test/integration folder
call addons\gdUnit4\runtest.cmd --godot_bin "%GODOT_BIN%" --path test/integration

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ========================================
    echo TESTS FAILED!
    echo ========================================
    exit /b %ERRORLEVEL%
) else (
    echo.
    echo ========================================
    echo ALL TESTS PASSED!
    echo ========================================
)

echo.
echo Test run complete.