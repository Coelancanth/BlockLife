using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace BlockLife.Tests.Integration
{
    public static class TestLogger
    {
        private static readonly string LogPath = Path.Combine(@"C:\Users\Coel\Documents\Godot\blocklife", "test_debug.log");
        private static readonly object _lock = new object();
        
        static TestLogger()
        {
            // Clear the log file at the start
            try
            {
                File.WriteAllText(LogPath, $"=== BlockLife Integration Test Log Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
            }
            catch { }
        }
        
        public static void Log(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
        {
            lock (_lock)
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    File.AppendAllText(LogPath, $"[{timestamp}] [{fileName}.{caller}] {message}\n");
                }
                catch { }
            }
        }
        
        public static string GetLogPath() => LogPath;
    }
}