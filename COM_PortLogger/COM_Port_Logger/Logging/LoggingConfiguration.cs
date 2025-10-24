using System;
using System.Collections.Generic;
using COM_Port_Logger.ConfigurationSettings;

namespace COM_Port_Logger.Logging
{
    /// <summary>
    /// Configuration settings for the logging system.
    /// Defines all parameters that control logging behavior and output.
    /// </summary>
    public class LoggingSettings
    {
        /// <summary>
        /// Gets or sets the directory where log files will be stored.
        /// </summary>
        public string LogDirectory { get; set; } = "logs";
        
        /// <summary>
        /// Gets or sets the minimum log level to record.
        /// Messages below this level will be filtered out.
        /// </summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;
        
        /// <summary>
        /// Gets or sets whether to enable console output for log messages.
        /// </summary>
        public bool EnableConsoleOutput { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to enable file output for log messages.
        /// </summary>
        public bool EnableFileOutput { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to enable structured logging with additional metadata.
        /// </summary>
        public bool EnableStructuredLogging { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the maximum size of a single log file in megabytes.
        /// </summary>
        public int MaxLogFileSizeMB { get; set; } = 10;
        
        /// <summary>
        /// Gets or sets the maximum number of log files to keep.
        /// Older files will be deleted when this limit is exceeded.
        /// </summary>
        public int MaxLogFiles { get; set; } = 10;
        
        /// <summary>
        /// Gets or sets the format string for log file names.
        /// Uses DateTime formatting (e.g., "com-port-logger-{0:yyyy-MM-dd}.log").
        /// </summary>
        public string LogFileNameFormat { get; set; } = "com-port-logger-{0:yyyy-MM-dd}.log";
        
        /// <summary>
        /// Gets or sets whether to include stack traces in log entries.
        /// </summary>
        public bool IncludeStackTrace { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to include additional properties in log entries.
        /// </summary>
        public bool IncludeProperties { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to enable performance logging for monitoring.
        /// </summary>
        public bool EnablePerformanceLogging { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to enable serial port operation logging.
        /// </summary>
        public bool EnableSerialPortLogging { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to enable configuration loading logging.
        /// </summary>
        public bool EnableConfigurationLogging { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to enable file operation logging.
        /// </summary>
        public bool EnableFileOperationLogging { get; set; } = true;
    }

    /// <summary>
    /// Extended configuration settings that includes logging configuration.
    /// Extends the base ConfigSettings with logging-specific options.
    /// </summary>
    public class ExtendedConfigSettings : ConfigSettings
    {
        /// <summary>
        /// Gets or sets the logging configuration settings.
        /// </summary>
        public LoggingSettings Logging { get; set; } = new LoggingSettings();
    }

    /// <summary>
    /// Helper class for managing logging configuration.
    /// Provides utility methods for converting between different configuration formats.
    /// </summary>
    public static class LoggingConfigurationHelper
    {
        /// <summary>
        /// Converts LoggingSettings to LoggingConfiguration format.
        /// </summary>
        /// <param name="settings">The LoggingSettings to convert.</param>
        /// <returns>A LoggingConfiguration object with equivalent settings.</returns>
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
        /// Creates a default logging configuration with standard settings.
        /// Suitable for production use with Info level logging.
        /// </summary>
        /// <returns>A LoggingConfiguration with default settings.</returns>
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
        /// Creates a development logging configuration with verbose settings.
        /// Includes Debug level logging and smaller file sizes for development.
        /// </summary>
        /// <returns>A LoggingConfiguration optimized for development.</returns>
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
        /// Creates a production logging configuration with optimized settings.
        /// Uses Warning level logging and larger file sizes for production environments.
        /// </summary>
        /// <returns>A LoggingConfiguration optimized for production.</returns>
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
    /// Performance logging helper for measuring operation execution times.
    /// Automatically logs start and end times with duration calculation.
    /// </summary>
    public class PerformanceLogger : IDisposable
    {
        private readonly string _operation;
        private readonly DateTime _startTime;
        private readonly Dictionary<string, object> _properties;

        /// <summary>
        /// Initializes a new instance of the PerformanceLogger class.
        /// </summary>
        /// <param name="operation">The name of the operation being measured.</param>
        /// <param name="properties">Additional properties to include in the log entry.</param>
        public PerformanceLogger(string operation, Dictionary<string, object> properties = null)
        {
            _operation = operation;
            _startTime = DateTime.Now;
            _properties = properties ?? new Dictionary<string, object>();
            
            Log.Debug($"Starting operation: {_operation}", "Performance", null, _properties);
        }

        /// <summary>
        /// Disposes of the PerformanceLogger and logs the operation completion with duration.
        /// </summary>
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
