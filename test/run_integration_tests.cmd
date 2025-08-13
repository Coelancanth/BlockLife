@echo off
REM Integration Test Runner for BlockLife
REM This script runs GdUnit4 integration tests for the Godot project

echo ========================================
echo BlockLife Integration Test Runner
echo ========================================
echo.

REM Load local settings if they exist
if exist "%~dp0local_settings.cmd" (
    call "%~dp0local_settings.cmd"
) else (
    echo WARNING: local_settings.cmd not found
    echo Create test\local_settings.cmd with your GODOT_BIN path
)

REM Check if GODOT_BIN is set
if "%GODOT_BIN%"=="" (
    echo ERROR: GODOT_BIN environment variable is not set!
    echo Please create test\local_settings.cmd with:
    echo   set GODOT_BIN=C:\Path\To\Godot.exe
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