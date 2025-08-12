using System;
using System.IO;

namespace TCPSev
{
    /// <summary>
    /// Simple structured logging utility for the TCP server
    /// Provides timestamped log entries with different severity levels
    /// In production, consider using established frameworks like Serilog or NLog
    /// </summary>
    public static class Logger
    {
        private static readonly object lockObj = new object();
        private static readonly string logFilePath = "server.log";

        /// <summary>
        /// Log levels for categorizing messages
        /// </summary>
        public enum LogLevel
        {
            INFO,
            WARNING,
            ERROR,
            DEBUG
        }

        /// <summary>
        /// Logs a message with specified level to both console and file
        /// Thread-safe implementation using lock for concurrent access
        /// </summary>
        public static void Log(LogLevel level, string message)
        {
            lock (lockObj)
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"[{timestamp}] [{level}] {message}";
                
                // Output to console for immediate feedback
                Console.WriteLine(logEntry);
                
                // Also write to file for persistence (optional)
                try
                {
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{timestamp}] [ERROR] Failed to write to log file: {ex.Message}");
                }
            }
        }

        public static void Info(string message) => Log(LogLevel.INFO, message);
        public static void Warning(string message) => Log(LogLevel.WARNING, message);
        public static void Error(string message) => Log(LogLevel.ERROR, message);
        public static void Debug(string message) => Log(LogLevel.DEBUG, message);
    }
}