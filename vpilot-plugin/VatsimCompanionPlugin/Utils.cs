using System;
using System.IO;
using System.Text;

namespace VatsimCompanionPlugin.Utils
{
    /// <summary>
    /// Simple file-based logger for plugin debugging
    /// </summary>
    public class Logger
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();
        private readonly bool _enabled;

        public Logger(bool enabled = true)
        {
            _enabled = enabled;

            if (_enabled)
            {
                string logDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "VatsimCompanion",
                    "Logs"
                );

                Directory.CreateDirectory(logDirectory);

                string logFileName = $"plugin_{DateTime.Now:yyyyMMdd}.log";
                _logFilePath = Path.Combine(logDirectory, logFileName);
            }
        }

        public void Info(string message)
        {
            Log("INFO", message);
        }

        public void Warning(string message)
        {
            Log("WARN", message);
        }

        public void Error(string message, Exception ex = null)
        {
            if (ex != null)
            {
                Log("ERROR", $"{message}: {ex.Message}\n{ex.StackTrace}");
            }
            else
            {
                Log("ERROR", message);
            }
        }

        public void Debug(string message)
        {
            Log("DEBUG", message);
        }

        private void Log(string level, string message)
        {
            if (!_enabled) return;

            try
            {
                lock (_lockObject)
                {
                    string logEntry = $"[{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}] [{level}] {message}";

                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);

                    // Also write to debug output
                    System.Diagnostics.Debug.WriteLine(logEntry);
                }
            }
            catch
            {
                // Fail silently to avoid breaking plugin
            }
        }

        public void ClearOldLogs(int daysToKeep = 7)
        {
            try
            {
                string logDirectory = Path.GetDirectoryName(_logFilePath);
                if (Directory.Exists(logDirectory))
                {
                    var files = Directory.GetFiles(logDirectory, "plugin_*.log");
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.CreationTime < DateTime.Now.AddDays(-daysToKeep))
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
            catch
            {
                // Fail silently
            }
        }
    }

    /// <summary>
    /// Retry helper for network operations
    /// </summary>
    public class RetryHelper
    {
        public static T Execute<T>(Func<T> operation, int maxRetries = 3, int delayMs = 1000)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    return operation();
                }
                catch (Exception ex)
                {
                    attempts++;
                    if (attempts >= maxRetries)
                    {
                        throw;
                    }
                    System.Threading.Thread.Sleep(delayMs);
                }
            }
        }

        public static void Execute(Action operation, int maxRetries = 3, int delayMs = 1000)
        {
            Execute<object>(() =>
            {
                operation();
                return null;
            }, maxRetries, delayMs);
        }
    }
}
