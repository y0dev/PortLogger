using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace COM_Port_Logger.Logging
{
    /// <summary>
    /// Defines the available log levels for the logging system.
    /// Levels are ordered from most verbose (Trace) to least verbose (Critical).
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Most verbose level - traces execution flow.
        /// </summary>
        Trace = 0,
        
        /// <summary>
        /// Debug level - detailed information for debugging.
        /// </summary>
        Debug = 1,
        
        /// <summary>
        /// Information level - general application flow.
        /// </summary>
        Info = 2,
        
        /// <summary>
        /// Warning level - potentially harmful situations.
        /// </summary>
        Warning = 3,
        
        /// <summary>
        /// Error level - error events that might still allow the application to continue.
        /// </summary>
        Error = 4,
        
        /// <summary>
        /// Critical level - very severe errors that might cause the application to terminate.
        /// </summary>
        Critical = 5
    }

    /// <summary>
    /// Represents a single log entry with all associated metadata.
    /// Contains timestamp, level, message, context, and additional properties.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Gets or sets the timestamp when the log entry was created.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets the log level of this entry.
        /// </summary>
        public LogLevel Level { get; set; }
        
        /// <summary>
        /// Gets or sets the main log message.
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the context where the log entry was created.
        /// </summary>
        public string Context { get; set; }
        
        /// <summary>
        /// Gets or sets the source component that created the log entry.
        /// </summary>
        public string Source { get; set; }
        
        /// <summary>
        /// Gets or sets the exception associated with this log entry.
        /// </summary>
        public Exception Exception { get; set; }
        
        /// <summary>
        /// Gets or sets additional properties for structured logging.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the LogEntry class.
        /// </summary>
        public LogEntry()
        {
            Properties = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Configuration settings for the logging system.
    /// Controls logging behavior, output destinations, and file management.
    /// </summary>
    public class LoggingConfiguration
    {
        public string LogDirectory { get; set; } = "logs";
        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;
        public bool EnableConsoleOutput { get; set; } = true;
        public bool EnableFileOutput { get; set; } = true;
        public bool EnableStructuredLogging { get; set; } = true;
        public int MaxLogFileSizeMB { get; set; } = 10;
        public int MaxLogFiles { get; set; } = 10;
        public string LogFileNameFormat { get; set; } = "app-{0:yyyy-MM-dd}.log";
        public bool IncludeStackTrace { get; set; } = true;
        public bool IncludeProperties { get; set; } = true;
    }

    /// <summary>
    /// Interface for log writers
    /// </summary>
    public interface ILogWriter
    {
        void Write(LogEntry entry);
        void Flush();
        void Dispose();
    }

    /// <summary>
    /// Console log writer
    /// </summary>
    public class ConsoleLogWriter : ILogWriter
    {
        private readonly LoggingConfiguration _config;
        private readonly object _lock = new object();

        public ConsoleLogWriter(LoggingConfiguration config)
        {
            _config = config;
        }

        public void Write(LogEntry entry)
        {
            lock (_lock)
            {
                var originalColor = Console.ForegroundColor;
                var color = GetLogLevelColor(entry.Level);
                
                Console.ForegroundColor = color;
                Console.Write($"[{entry.Timestamp:HH:mm:ss.fff}] ");
                Console.Write($"[{entry.Level.ToString().ToUpper()}] ");
                Console.Write($"[{entry.Source}] ");
                
                if (!string.IsNullOrEmpty(entry.Context))
                {
                    Console.Write($"[{entry.Context}] ");
                }
                
                Console.ForegroundColor = originalColor;
                Console.WriteLine(entry.Message);

                if (entry.Exception != null && _config.IncludeStackTrace)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Exception: {entry.Exception.Message}");
                    Console.WriteLine($"Stack Trace: {entry.Exception.StackTrace}");
                    Console.ForegroundColor = originalColor;
                }

                if (entry.Properties?.Count > 0 && _config.IncludeProperties)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    foreach (var prop in entry.Properties)
                    {
                        Console.WriteLine($"  {prop.Key}: {prop.Value}");
                    }
                    Console.ForegroundColor = originalColor;
                }
            }
        }

        public void Flush()
        {
            // Console doesn't need flushing
        }

        public void Dispose()
        {
            // Nothing to dispose for console
        }

        private ConsoleColor GetLogLevelColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    return ConsoleColor.Gray;
                case LogLevel.Debug:
                    return ConsoleColor.DarkGray;
                case LogLevel.Info:
                    return ConsoleColor.Green;
                case LogLevel.Warning:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                case LogLevel.Critical:
                    return ConsoleColor.Magenta;
                default:
                    return ConsoleColor.White;
            }
        }
    }

    /// <summary>
    /// File log writer with rotation
    /// </summary>
    public class FileLogWriter : ILogWriter
    {
        private readonly LoggingConfiguration _config;
        private readonly object _lock = new object();
        private StreamWriter _writer;
        private string _currentLogFile;
        private long _currentFileSize;

        public FileLogWriter(LoggingConfiguration config)
        {
            _config = config;
            InitializeLogFile();
        }

        public void Write(LogEntry entry)
        {
            lock (_lock)
            {
                try
                {
                    if (_writer == null)
                    {
                        InitializeLogFile();
                    }

                    var logLine = FormatLogEntry(entry);
                    _writer.WriteLine(logLine);
                    _writer.Flush();

                    _currentFileSize += Encoding.UTF8.GetByteCount(logLine + Environment.NewLine);

                    // Check if we need to rotate the log file
                    if (_currentFileSize > _config.MaxLogFileSizeMB * 1024 * 1024)
                    {
                        RotateLogFile();
                    }
                }
                catch (Exception ex)
                {
                    // Fallback to console if file writing fails
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }
        }

        public void Flush()
        {
            lock (_lock)
            {
                _writer?.Flush();
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _writer?.Dispose();
            }
        }

        private void InitializeLogFile()
        {
            try
            {
                if (!Directory.Exists(_config.LogDirectory))
                {
                    Directory.CreateDirectory(_config.LogDirectory);
                }

                _currentLogFile = Path.Combine(_config.LogDirectory, 
                    string.Format(_config.LogFileNameFormat, DateTime.Now));

                _writer = new StreamWriter(_currentLogFile, true, Encoding.UTF8);
                _currentFileSize = new FileInfo(_currentLogFile).Length;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize log file: {ex.Message}");
            }
        }

        private void RotateLogFile()
        {
            try
            {
                _writer?.Dispose();

                // Clean up old log files
                CleanupOldLogFiles();

                // Initialize new log file
                InitializeLogFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to rotate log file: {ex.Message}");
            }
        }

        private void CleanupOldLogFiles()
        {
            try
            {
                var logFiles = Directory.GetFiles(_config.LogDirectory, "*.log")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                if (logFiles.Count > _config.MaxLogFiles)
                {
                    var filesToDelete = logFiles.Skip(_config.MaxLogFiles);
                    foreach (var file in filesToDelete)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to delete old log file {file.Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to cleanup old log files: {ex.Message}");
            }
        }

        private string FormatLogEntry(LogEntry entry)
        {
            var sb = new StringBuilder();
            sb.Append($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} ");
            sb.Append($"[{entry.Level.ToString().ToUpper()}] ");
            sb.Append($"[{entry.Source}] ");

            if (!string.IsNullOrEmpty(entry.Context))
            {
                sb.Append($"[{entry.Context}] ");
            }

            sb.Append(entry.Message);

            if (entry.Exception != null)
            {
                sb.AppendLine();
                sb.AppendLine($"Exception: {entry.Exception.Message}");
                if (_config.IncludeStackTrace)
                {
                    sb.AppendLine($"Stack Trace: {entry.Exception.StackTrace}");
                }
            }

            if (entry.Properties?.Count > 0 && _config.IncludeProperties)
            {
                sb.AppendLine();
                foreach (var prop in entry.Properties)
                {
                    sb.AppendLine($"  {prop.Key}: {prop.Value}");
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Main logger class
    /// </summary>
    public class Logger : IDisposable
    {
        private readonly LoggingConfiguration _config;
        private readonly List<ILogWriter> _writers;
        private readonly ConcurrentQueue<LogEntry> _logQueue;
        private readonly Thread _logThread;
        private volatile bool _disposed;
        private volatile bool _stopLogging;

        public Logger(LoggingConfiguration config = null)
        {
            _config = config ?? new LoggingConfiguration();
            _writers = new List<ILogWriter>();
            _logQueue = new ConcurrentQueue<LogEntry>();

            // Initialize writers
            if (_config.EnableConsoleOutput)
            {
                _writers.Add(new ConsoleLogWriter(_config));
            }

            if (_config.EnableFileOutput)
            {
                _writers.Add(new FileLogWriter(_config));
            }

            // Start background logging thread
            _logThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "LoggerThread"
            };
            _logThread.Start();
        }

        public void Log(LogLevel level, string message, string context = null, string source = null, 
            Exception exception = null, Dictionary<string, object> properties = null)
        {
            if (_disposed || level < _config.MinimumLevel)
                return;

            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Context = context,
                Source = source ?? GetCallingSource(),
                Exception = exception,
                Properties = properties
            };

            _logQueue.Enqueue(entry);
        }

        public void Trace(string message, string context = null, string source = null, 
            Dictionary<string, object> properties = null)
        {
            Log(LogLevel.Trace, message, context, source, null, properties);
        }

        public void Debug(string message, string context = null, string source = null, 
            Dictionary<string, object> properties = null)
        {
            Log(LogLevel.Debug, message, context, source, null, properties);
        }

        public void Info(string message, string context = null, string source = null, 
            Dictionary<string, object> properties = null)
        {
            Log(LogLevel.Info, message, context, source, null, properties);
        }

        public void Warning(string message, string context = null, string source = null, 
            Exception exception = null, Dictionary<string, object> properties = null)
        {
            Log(LogLevel.Warning, message, context, source, exception, properties);
        }

        public void Error(string message, string context = null, string source = null, 
            Exception exception = null, Dictionary<string, object> properties = null)
        {
            Log(LogLevel.Error, message, context, source, exception, properties);
        }

        public void Critical(string message, string context = null, string source = null, 
            Exception exception = null, Dictionary<string, object> properties = null)
        {
            Log(LogLevel.Critical, message, context, source, exception, properties);
        }

        private void ProcessLogQueue()
        {
            while (!_stopLogging)
            {
                try
                {
                    if (_logQueue.TryDequeue(out LogEntry entry))
                    {
                        foreach (var writer in _writers)
                        {
                            try
                            {
                                writer.Write(entry);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error writing log entry: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(10); // Small delay when queue is empty
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in log processing thread: {ex.Message}");
                    Thread.Sleep(100);
                }
            }

            // Process remaining entries
            while (_logQueue.TryDequeue(out LogEntry entry))
            {
                foreach (var writer in _writers)
                {
                    try
                    {
                        writer.Write(entry);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error writing final log entry: {ex.Message}");
                    }
                }
            }
        }

        private string GetCallingSource()
        {
            try
            {
                var stackTrace = new System.Diagnostics.StackTrace(true);
                var frame = stackTrace.GetFrame(3); // Skip Logger methods
                return frame?.GetMethod()?.DeclaringType?.Name ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _stopLogging = true;

            // Wait for logging thread to finish
            _logThread?.Join(5000);

            // Dispose all writers
            foreach (var writer in _writers)
            {
                try
                {
                    writer.Flush();
                    writer.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing log writer: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Static logger instance for easy access
    /// </summary>
    public static class Log
    {
        internal static Logger _logger;
        private static readonly object _lock = new object();

        public static void Initialize(LoggingConfiguration config = null)
        {
            lock (_lock)
            {
                _logger?.Dispose();
                _logger = new Logger(config);
            }
        }

        public static void Trace(string message, string context = null, string source = null, 
            Dictionary<string, object> properties = null)
        {
            _logger?.Trace(message, context, source, properties);
        }

        public static void Debug(string message, string context = null, string source = null, 
            Dictionary<string, object> properties = null)
        {
            _logger?.Debug(message, context, source, properties);
        }

        public static void Info(string message, string context = null, string source = null, 
            Dictionary<string, object> properties = null)
        {
            _logger?.Info(message, context, source, properties);
        }

        public static void Warning(string message, string context = null, string source = null, 
            Exception exception = null, Dictionary<string, object> properties = null)
        {
            _logger?.Warning(message, context, source, exception, properties);
        }

        public static void Error(string message, string context = null, string source = null, 
            Exception exception = null, Dictionary<string, object> properties = null)
        {
            _logger?.Error(message, context, source, exception, properties);
        }

        public static void Critical(string message, string context = null, string source = null, 
            Exception exception = null, Dictionary<string, object> properties = null)
        {
            _logger?.Critical(message, context, source, exception, properties);
        }

        public static void Dispose()
        {
            lock (_lock)
            {
                _logger?.Dispose();
                _logger = null;
            }
        }
    }
}
