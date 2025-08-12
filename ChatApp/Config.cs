using System;
using System.Configuration;

namespace ChatApp
{
    /// <summary>
    /// Configuration helper class to manage application settings
    /// Provides centralized access to configuration values with defaults
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Server IP address to connect to
        /// Default: 127.0.0.1 (localhost)
        /// </summary>
        public static string ServerIP => GetSetting("ServerIP", "127.0.0.1");

        /// <summary>
        /// Server port number for TCP connection
        /// Default: 8000
        /// </summary>
        public static int ServerPort => GetIntSetting("ServerPort", 8000);

        /// <summary>
        /// Connection timeout in milliseconds
        /// Default: 5000ms (5 seconds)
        /// </summary>
        public static int ConnectionTimeoutMs => GetIntSetting("ConnectionTimeoutMs", 5000);

        /// <summary>
        /// Maximum allowed message length in characters
        /// Default: 1000 characters
        /// </summary>
        public static int MaxMessageLength => GetIntSetting("MaxMessageLength", 1000);

        /// <summary>
        /// Gets a string setting from app.config with fallback default
        /// </summary>
        private static string GetSetting(string key, string defaultValue)
        {
            try
            {
                string value = ConfigurationManager.AppSettings[key];
                return string.IsNullOrEmpty(value) ? defaultValue : value;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets an integer setting from app.config with fallback default
        /// </summary>
        private static int GetIntSetting(string key, int defaultValue)
        {
            try
            {
                string value = ConfigurationManager.AppSettings[key];
                return int.TryParse(value, out int result) ? result : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
