using System;
using System.Collections.Generic;
using COM_Port_Logger.ConfigurationSettings;

namespace COM_Port_Logger.Logging
{
    /// <summary>
    /// Logging configuration settings
    /// </summary>
    public class LoggingSettings
    {
        public string LogDirectory { get; set; } = "logs";
        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;
        public bool EnableConsoleOutput { get; set; } = true;
        public bool EnableFileOutput { get; set; } = true;
        public bool EnableStructuredLogging { get; set; } = true;
        public int MaxLogFileSizeMB { get; set; } = 10;
        public int MaxLogFiles { get; set; } = 10;
        public string LogFileNameFormat { get; set; } = "com-port-logger-{0:yyyy-MM-dd}.log";
        public bool IncludeStackTrace { get; set; } = true;
        public bool IncludeProperties { get; set; } = true;
        public bool EnablePerformanceLogging { get; set; } = false;
        public bool EnableSerialPortLogging { get; set; } = true;
        public bool EnableConfigurationLogging { get; set; } = true;
        public bool EnableFileOperationLogging { get; set; } = true;
    }

    /// <summary>
    /// Extended configuration settings that includes logging
    /// </summary>
    public class ExtendedConfigSettings : ConfigSettings
    {
        public LoggingSettings Logging { get; set; } = new LoggingSettings();
    }

    /// <summary>
    /// Logging configuration helper
    /// </summary>
    public static class LoggingConfigurationHelper
    {
        /// <summary>
        /// Convert LoggingSettings to LoggingConfiguration
        /// </summary>
        public static LoggingConfiguration ToLoggingConfiguration(LoggingSettings settings)
        {
            return new LoggingConfiguration
            {
                LogDirectory = settings.LogDirectory,
                MinimumLevel = settings.MinimumLevel,
                EnableConsoleOutput = settings.EnableConsoleOutput,
                EnableFileOutput = settings.EnableFileOutput,
                EnableStructuredLogging = settings.EnableStructuredLogging,
                MaxLogFileSizeMB = settings.MaxLogFileSizeMB,
                MaxLogFiles = settings.MaxLogFiles,
                LogFileNameFormat = settings.LogFileNameFormat,
                IncludeStackTrace = settings.IncludeStackTrace,
                IncludeProperties = settings.IncludeProperties
            };
        }

        /// <summary>
        /// Create default logging configuration
        /// </summary>
        public static LoggingConfiguration CreateDefaultConfiguration()
        {
            return new LoggingConfiguration
            {
                LogDirectory = "logs",
                MinimumLevel = LogLevel.Info,
                EnableConsoleOutput = true,
                EnableFileOutput = true,
                EnableStructuredLogging = true,
                MaxLogFileSizeMB = 10,
                MaxLogFiles = 10,
                LogFileNameFormat = "com-port-logger-{0:yyyy-MM-dd}.log",
                IncludeStackTrace = true,
                IncludeProperties = true
            };
        }

        /// <summary>
        /// Create development logging configuration
        /// </summary>
        public static LoggingConfiguration CreateDevelopmentConfiguration()
        {
            return new LoggingConfiguration
            {
                LogDirectory = "logs",
                MinimumLevel = LogLevel.Debug,
                EnableConsoleOutput = true,
                EnableFileOutput = true,
                EnableStructuredLogging = true,
                MaxLogFileSizeMB = 5,
                MaxLogFiles = 5,
                LogFileNameFormat = "com-port-logger-dev-{0:yyyy-MM-dd}.log",
                IncludeStackTrace = true,
                IncludeProperties = true
            };
        }

        /// <summary>
        /// Create production logging configuration
        /// </summary>
        public static LoggingConfiguration CreateProductionConfiguration()
        {
            return new LoggingConfiguration
            {
                LogDirectory = "logs",
                MinimumLevel = LogLevel.Warning,
                EnableConsoleOutput = false,
                EnableFileOutput = true,
                EnableStructuredLogging = true,
                MaxLogFileSizeMB = 50,
                MaxLogFiles = 30,
                LogFileNameFormat = "com-port-logger-{0:yyyy-MM-dd}.log",
                IncludeStackTrace = true,
                IncludeProperties = false
            };
        }
    }

    /// <summary>
    /// Performance logging helper
    /// </summary>
    public class PerformanceLogger : IDisposable
    {
        private readonly string _operation;
        private readonly DateTime _startTime;
        private readonly Dictionary<string, object> _properties;

        public PerformanceLogger(string operation, Dictionary<string, object> properties = null)
        {
            _operation = operation;
            _startTime = DateTime.Now;
            _properties = properties ?? new Dictionary<string, object>();
            
            Log.Debug($"Starting operation: {_operation}", "Performance", null, _properties);
        }

        public void Dispose()
        {
            var duration = DateTime.Now - _startTime;
            var properties = new Dictionary<string, object>(_properties)
            {
                ["Duration"] = duration.TotalMilliseconds,
                ["DurationFormatted"] = duration.ToString(@"hh\:mm\:ss\.fff")
            };
            
            Log.Debug($"Completed operation: {_operation}", "Performance", null, properties);
        }
    }

    /// <summary>
    /// Structured logging helpers
    /// </summary>
    public static class StructuredLogging
    {
        public static void LogSerialPortOperation(string operation, string portName, int baudRate, 
            bool success, string message = null, Exception exception = null)
        {
            var properties = new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["PortName"] = portName,
                ["BaudRate"] = baudRate,
                ["Success"] = success
            };

            if (success)
            {
                Log.Info($"Serial port {operation}: {message ?? "Success"}", "SerialPort", "StructuredLogging", properties);
            }
            else
            {
                Log.Error($"Serial port {operation} failed: {message ?? "Unknown error"}", "SerialPort", "StructuredLogging", exception, properties);
            }
        }

        public static void LogFileOperation(string operation, string filePath, bool success, 
            long? fileSize = null, string message = null, Exception exception = null)
        {
            var properties = new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["FilePath"] = filePath,
                ["Success"] = success
            };

            if (fileSize.HasValue)
            {
                properties["FileSize"] = fileSize.Value;
            }

		if (success)
		{
			Log.Info($"File {operation}: {message ?? "Success"}", "FileOperation", "StructuredLogging", properties);
		}
		else
		{
			Log.Error($"File {operation} failed: {message ?? "Unknown error"}", "FileOperation", "StructuredLogging", exception, properties);
		}
        }

        public static void LogConfigurationLoad(string configFile, bool success, string message = null, Exception exception = null)
        {
            var properties = new Dictionary<string, object>
            {
                ["ConfigFile"] = configFile,
                ["Success"] = success
            };

		if (success)
		{
			Log.Info($"Configuration loaded: {message ?? "Success"}", "Configuration", "StructuredLogging", properties);
		}
		else
		{
			Log.Error($"Configuration load failed: {message ?? "Unknown error"}", "Configuration", "StructuredLogging", exception, properties);
		}
        }

        public static void LogConnectionEvent(string eventType, string connectionType, int retryCount = 0, 
            string message = null, Exception exception = null)
        {
            var properties = new Dictionary<string, object>
            {
                ["EventType"] = eventType,
                ["ConnectionType"] = connectionType,
                ["RetryCount"] = retryCount
            };

		switch (eventType.ToLower())
		{
			case "connected":
				Log.Info($"Connection established: {message ?? "Success"}", "Connection", "StructuredLogging", properties);
				break;
			case "disconnected":
				Log.Warning($"Connection lost: {message ?? "Connection lost"}", "Connection", "StructuredLogging", exception, properties);
				break;
			case "reconnecting":
				Log.Info($"Reconnecting: {message ?? "Attempting reconnection"}", "Connection", "StructuredLogging", properties);
				break;
			case "reconnect_failed":
				Log.Error($"Reconnection failed: {message ?? "Failed to reconnect"}", "Connection", "StructuredLogging", exception, properties);
				break;
			default:
				Log.Info($"Connection event: {message ?? eventType}", "Connection", "StructuredLogging", properties);
				break;
		}
        }
    }
}
