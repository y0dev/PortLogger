using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace COM_Port_Logger.Services
{
    /// <summary>
    /// Tracks application operations including lifecycle events, COM port usage, and errors.
    /// Logs to a separate application log file distinct from serial port data logging.
    /// </summary>
    public static class ApplicationOperationLogger
    {
        private static string _applicationLogFilePath;
        private static StreamWriter _logWriter;
        private static readonly object _lock = new object();
        private static bool _isInitialized = false;
        
        // COM port usage tracking
        private static Dictionary<string, ComPortSession> _activeComPortSessions = new Dictionary<string, ComPortSession>();
        private static readonly object _sessionLock = new object();

        /// <summary>
        /// Represents a COM port usage session with start time and duration tracking.
        /// </summary>
        private class ComPortSession
        {
            /// <summary>
            /// Gets or sets the COM port name.
            /// </summary>
            public string PortName { get; set; }
            
            /// <summary>
            /// Gets or sets the baud rate.
            /// </summary>
            public int BaudRate { get; set; }
            
            /// <summary>
            /// Gets or sets the session start time.
            /// </summary>
            public DateTime StartTime { get; set; }
            
            /// <summary>
            /// Gets or sets the session end time (null if still active).
            /// </summary>
            public DateTime? EndTime { get; set; }
            
            /// <summary>
            /// Gets the duration of the session.
            /// </summary>
            public TimeSpan Duration => EndTime.HasValue 
                ? EndTime.Value - StartTime 
                : DateTime.Now - StartTime;
        }

        /// <summary>
        /// Initializes the application operation logger.
        /// </summary>
        /// <param name="logDirectory">The directory where application logs will be stored.</param>
        public static void Initialize(string logDirectory = "logs")
        {
            if (_isInitialized) return;

            try
            {
                lock (_lock)
                {
                    if (_isInitialized) return;

                    // Create application log file with date-based directory structure
                    DateTime now = DateTime.Now;
                    string directoryPath = Path.Combine(logDirectory, 
                        "application", 
                        now.ToString("yyyy"), 
                        now.ToString("MM_MMM"), 
                        now.ToString("MM_dd"));

                    // Ensure directory exists
                    Directory.CreateDirectory(directoryPath);

                    // Create log file name with timestamp
                    string fileName = $"app-operations-{now:yyyy-MM-dd}.log";
                    _applicationLogFilePath = Path.Combine(directoryPath, fileName);

                    // Create or append to log file
                    FileStream fileStream = new FileStream(_applicationLogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                    _logWriter = new StreamWriter(fileStream) { AutoFlush = true };

                    _isInitialized = true;

                    // Write initialization entry
                    WriteLogEntry("APPLICATION", "INITIALIZED", "Application operation logger initialized", null);
                }
            }
            catch (Exception ex)
            {
                // Fallback to console if file logging fails
                Console.WriteLine($"Failed to initialize application operation logger: {ex.Message}");
                _isInitialized = true; // Prevent infinite retry loops
            }
        }

        /// <summary>
        /// Logs application start event.
        /// </summary>
        /// <param name="consoleName">The console name or application identifier.</param>
        /// <param name="additionalInfo">Additional information about the startup.</param>
        public static void LogApplicationStart(string consoleName, string additionalInfo = null)
        {
            var properties = new Dictionary<string, object>
            {
                ["ConsoleName"] = consoleName,
                ["StartTime"] = DateTime.Now
            };

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                properties["AdditionalInfo"] = additionalInfo;
            }

            WriteLogEntry("APPLICATION", "STARTED", $"Application started - Console: {consoleName}", properties);
        }

        /// <summary>
        /// Logs application stop event.
        /// </summary>
        /// <param name="consoleName">The console name or application identifier.</param>
        /// <param name="reason">The reason for stopping (e.g., "QUIT", "Ctrl+C", "Error").</param>
        public static void LogApplicationStop(string consoleName, string reason = "Unknown")
        {
            var properties = new Dictionary<string, object>
            {
                ["ConsoleName"] = consoleName,
                ["StopTime"] = DateTime.Now,
                ["Reason"] = reason
            };

            WriteLogEntry("APPLICATION", "STOPPED", $"Application stopped - Console: {consoleName}, Reason: {reason}", properties);
        }

        /// <summary>
        /// Logs COM port open event and starts tracking usage duration.
        /// </summary>
        /// <param name="portName">The COM port name (e.g., "COM1").</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="parity">The parity setting.</param>
        /// <param name="dataBits">The number of data bits.</param>
        /// <param name="stopBits">The stop bits setting.</param>
        /// <param name="handshake">The handshake setting.</param>
        public static void LogComPortOpened(string portName, int baudRate, string parity, int dataBits, string stopBits, string handshake)
        {
            var session = new ComPortSession
            {
                PortName = portName,
                BaudRate = baudRate,
                StartTime = DateTime.Now
            };

            lock (_sessionLock)
            {
                _activeComPortSessions[portName] = session;
            }

            var properties = new Dictionary<string, object>
            {
                ["PortName"] = portName,
                ["BaudRate"] = baudRate,
                ["Parity"] = parity,
                ["DataBits"] = dataBits,
                ["StopBits"] = stopBits,
                ["Handshake"] = handshake,
                ["OpenTime"] = session.StartTime
            };

            WriteLogEntry("COM_PORT", "OPENED", $"COM port opened: {portName} @ {baudRate} baud", properties);
        }

        /// <summary>
        /// Logs COM port closed event and calculates usage duration.
        /// </summary>
        /// <param name="portName">The COM port name.</param>
        /// <param name="reason">The reason for closing (e.g., "Normal", "Error", "User Request").</param>
        public static void LogComPortClosed(string portName, string reason = "Normal")
        {
            ComPortSession session = null;
            TimeSpan? duration = null;

            lock (_sessionLock)
            {
                if (_activeComPortSessions.TryGetValue(portName, out session))
                {
                    session.EndTime = DateTime.Now;
                    duration = session.Duration;
                    _activeComPortSessions.Remove(portName);
                }
            }

            var properties = new Dictionary<string, object>
            {
                ["PortName"] = portName,
                ["CloseTime"] = DateTime.Now,
                ["Reason"] = reason
            };

            if (session != null)
            {
                properties["BaudRate"] = session.BaudRate;
                properties["Duration"] = duration.Value.TotalSeconds;
                properties["DurationFormatted"] = FormatDuration(duration.Value);
            }

            WriteLogEntry("COM_PORT", "CLOSED", 
                $"COM port closed: {portName}, Duration: {FormatDuration(duration ?? TimeSpan.Zero)}, Reason: {reason}", 
                properties);
        }

        /// <summary>
        /// Logs an error that occurred during application operation.
        /// </summary>
        /// <param name="errorType">The type of error (e.g., "SerialPort", "File", "Configuration").</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="exception">The exception object if available.</param>
        /// <param name="context">Additional context about where the error occurred.</param>
        public static void LogError(string errorType, string errorMessage, Exception exception = null, string context = null)
        {
            var properties = new Dictionary<string, object>
            {
                ["ErrorType"] = errorType,
                ["ErrorMessage"] = errorMessage,
                ["ErrorTime"] = DateTime.Now
            };

            if (!string.IsNullOrEmpty(context))
            {
                properties["Context"] = context;
            }

            if (exception != null)
            {
                properties["ExceptionType"] = exception.GetType().Name;
                properties["StackTrace"] = exception.StackTrace;
                if (exception.InnerException != null)
                {
                    properties["InnerException"] = exception.InnerException.Message;
                }
            }

            WriteLogEntry("ERROR", errorType.ToUpper(), $"Error occurred: {errorMessage}", properties);
        }

        /// <summary>
        /// Logs a general application operation event.
        /// </summary>
        /// <param name="operation">The operation name.</param>
        /// <param name="details">Details about the operation.</param>
        /// <param name="properties">Additional properties to include in the log.</param>
        public static void LogOperation(string operation, string details, Dictionary<string, object> properties = null)
        {
            var logProperties = properties ?? new Dictionary<string, object>();
            logProperties["OperationTime"] = DateTime.Now;

            WriteLogEntry("OPERATION", operation.ToUpper(), details, logProperties);
        }

        /// <summary>
        /// Writes a log entry to the application log file.
        /// </summary>
        /// <param name="category">The log category (e.g., "APPLICATION", "COM_PORT", "ERROR").</param>
        /// <param name="eventType">The event type (e.g., "STARTED", "OPENED", "CLOSED").</param>
        /// <param name="message">The log message.</param>
        /// <param name="properties">Additional properties to include.</param>
        private static void WriteLogEntry(string category, string eventType, string message, Dictionary<string, object> properties)
        {
            if (!_isInitialized || _logWriter == null) return;

            try
            {
                lock (_lock)
                {
                    if (!_isInitialized || _logWriter == null) return;

                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string logLine = $"[{timestamp}] [{category}] [{eventType}] {message}";

                    // Add properties if available
                    if (properties != null && properties.Count > 0)
                    {
                        var propStrings = new List<string>();
                        foreach (var prop in properties)
                        {
                            propStrings.Add($"{prop.Key}={prop.Value}");
                        }
                        logLine += $" | Properties: {string.Join(", ", propStrings)}";
                    }

                    _logWriter.WriteLine(logLine);
                }
            }
            catch (Exception ex)
            {
                // Fallback to console if file write fails
                Console.WriteLine($"Failed to write application log entry: {ex.Message}");
            }
        }

        /// <summary>
        /// Formats a TimeSpan duration into a human-readable string.
        /// </summary>
        /// <param name="duration">The duration to format.</param>
        /// <returns>A formatted duration string (e.g., "2h 30m 45s" or "1m 15s").</returns>
        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
            {
                return $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m {duration.Seconds}s";
            }
            else if (duration.TotalHours >= 1)
            {
                return $"{duration.Hours}h {duration.Minutes}m {duration.Seconds}s";
            }
            else if (duration.TotalMinutes >= 1)
            {
                return $"{duration.Minutes}m {duration.Seconds}s";
            }
            else
            {
                return $"{duration.TotalSeconds:F2}s";
            }
        }

        /// <summary>
        /// Gets all active COM port sessions with their current durations.
        /// </summary>
        /// <returns>A dictionary of active sessions with their durations.</returns>
        public static Dictionary<string, TimeSpan> GetActiveComPortSessions()
        {
            var result = new Dictionary<string, TimeSpan>();

            lock (_sessionLock)
            {
                foreach (var session in _activeComPortSessions.Values)
                {
                    result[session.PortName] = session.Duration;
                }
            }

            return result;
        }

        /// <summary>
        /// Shuts down the application operation logger and closes the log file.
        /// </summary>
        public static void Shutdown()
        {
            if (!_isInitialized) return;

            try
            {
                lock (_lock)
                {
                    if (!_isInitialized) return;

                    // Log shutdown
                    WriteLogEntry("APPLICATION", "SHUTDOWN", "Application operation logger shutting down", null);

                    // Close any remaining COM port sessions
                    lock (_sessionLock)
                    {
                        foreach (var session in _activeComPortSessions.Values)
                        {
                            LogComPortClosed(session.PortName, "Application Shutdown");
                        }
                        _activeComPortSessions.Clear();
                    }

                    // Close log writer
                    _logWriter?.Flush();
                    _logWriter?.Close();
                    _logWriter?.Dispose();
                    _logWriter = null;

                    _isInitialized = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error shutting down application operation logger: {ex.Message}");
            }
        }
    }
}

