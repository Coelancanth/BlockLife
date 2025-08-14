@echo off
REM Quick test watcher for BlockLife development
REM Auto-stops after 30 minutes of no file changes

echo ========================================
echo BlockLife Test Watcher
echo ========================================
echo.
echo Monitoring for file changes...
echo Tests run every 10 seconds
echo Auto-stops after 30 minutes of inactivity
echo Press Ctrl+C to stop manually
echo.

python scripts\test_monitor.py --continuous --interval 10 --timeout 30

echo.
echo Test watcher stopped.
pause