using System;
using System.IO;

namespace BlockLife.Core.Infrastructure.Configuration;

public static class EnvironmentLoader
{
    public static void LoadDotEnv()
    {
        var envFile = ".env";
        if (!File.Exists(envFile))
        {
            // Try parent directory (in case we're running from bin folder)
            envFile = Path.Combine("..", ".env");
            if (!File.Exists(envFile))
            {
                // Try from project root
                envFile = Path.Combine("..", "..", "..", ".env");
                if (!File.Exists(envFile))
                {
                    return; // No .env file found, that's okay
                }
            }
        }

        foreach (var line in File.ReadAllLines(envFile))
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            var separatorIndex = trimmedLine.IndexOf('=');
            if (separatorIndex > 0)
            {
                var key = trimmedLine.Substring(0, separatorIndex).Trim();
                var value = trimmedLine.Substring(separatorIndex + 1).Trim();
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
