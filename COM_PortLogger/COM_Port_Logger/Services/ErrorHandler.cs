using System;
using System.IO;
using System.Text;
using COM_Port_Logger.Exceptions;

namespace COM_Port_Logger.Services
{
    /// <summary>
    /// Service responsible for handling errors and logging them appropriately
    /// </summary>
    public static class ErrorHandler
    {
        private static readonly object _lock = new object();
        private static string _errorLogPath;
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize the error handler with a log file path
        /// </summary>
        /// <param name="logDirectory">Directory where error logs will be stored</param>
        public static void Initialize(string logDirectory)
        {
            lock (_lock)
            {
                if (_isInitialized) return;

                try
                {
                    // Create error log directory if it doesn't exist
                    if (!Directory.Exists(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }

                    // Create error log file with timestamp
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    _errorLogPath = Path.Combine(logDirectory, $"error_log_{timestamp}.txt");

                    // Write initialization message
                    WriteToErrorLog($"Error Handler initialized at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    // Fallback to console if file logging fails
                    Console.WriteLine($"Failed to initialize error handler: {ex.Message}");
                    _isInitialized = true; // Still mark as initialized to prevent infinite loops
                }
            }
        }

        /// <summary>
        /// Handle a generic exception
        /// </summary>
        /// <param name="ex">The exception to handle</param>
        /// <param name="context">Additional context about where the error occurred</param>
        /// <param name="showToUser">Whether to display the error to the user</param>
        public static void HandleException(Exception ex, string context = "", bool showToUser = true)
        {
            try
            {
                var errorMessage = BuildErrorMessage(ex, context);
                
                // Log to error file
                WriteToErrorLog(errorMessage);

                // Show to user if requested
                if (showToUser)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: {ex.Message}");
                    if (!string.IsNullOrEmpty(context))
                    {
                        Console.WriteLine($"Context: {context}");
                    }
                    Console.ResetColor();
                }
            }
            catch (Exception handlerEx)
            {
                // Fallback error handling
                Console.WriteLine($"Critical error in error handler: {handlerEx.Message}");
                Console.WriteLine($"Original error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle a COM Port Logger specific exception
        /// </summary>
        /// <param name="ex">The COM Port Logger exception to handle</param>
        /// <param name="context">Additional context about where the error occurred</param>
        /// <param name="showToUser">Whether to display the error to the user</param>
        public static void HandleCOMPortLoggerException(COMPortLoggerException ex, string context = "", bool showToUser = true)
        {
            try
            {
                var errorMessage = BuildCOMPortLoggerErrorMessage(ex, context);
                
                // Log to error file
                WriteToErrorLog(errorMessage);

                // Show to user if requested
                if (showToUser)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR [{ex.ErrorCode}]: {ex.Message}");
                    if (!string.IsNullOrEmpty(context))
                    {
                        Console.WriteLine($"Context: {context}");
                    }
                    Console.WriteLine($"Timestamp: {ex.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
                    Console.ResetColor();
                }
            }
            catch (Exception handlerEx)
            {
                // Fallback error handling
                Console.WriteLine($"Critical error in error handler: {handlerEx.Message}");
                Console.WriteLine($"Original error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle a serial port specific exception
        /// </summary>
        /// <param name="ex">The serial port exception to handle</param>
        /// <param name="context">Additional context about where the error occurred</param>
        /// <param name="showToUser">Whether to display the error to the user</param>
        public static void HandleSerialPortException(SerialPortException ex, string context = "", bool showToUser = true)
        {
            try
            {
                var errorMessage = BuildSerialPortErrorMessage(ex, context);
                
                // Log to error file
                WriteToErrorLog(errorMessage);

                // Show to user if requested
                if (showToUser)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"SERIAL PORT ERROR [{ex.ErrorCode}]: {ex.Message}");
                    Console.WriteLine($"Port: {ex.PortName}, Baud Rate: {ex.BaudRate}");
                    if (!string.IsNullOrEmpty(context))
                    {
                        Console.WriteLine($"Context: {context}");
                    }
                    Console.WriteLine($"Timestamp: {ex.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
                    Console.ResetColor();
                }
            }
            catch (Exception handlerEx)
            {
                // Fallback error handling
                Console.WriteLine($"Critical error in error handler: {handlerEx.Message}");
                Console.WriteLine($"Original error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle a configuration specific exception
        /// </summary>
        /// <param name="ex">The configuration exception to handle</param>
        /// <param name="context">Additional context about where the error occurred</param>
        /// <param name="showToUser">Whether to display the error to the user</param>
        public static void HandleConfigurationException(ConfigurationException ex, string context = "", bool showToUser = true)
        {
            try
            {
                var errorMessage = BuildConfigurationErrorMessage(ex, context);
                
                // Log to error file
                WriteToErrorLog(errorMessage);

                // Show to user if requested
                if (showToUser)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"CONFIGURATION ERROR [{ex.ErrorCode}]: {ex.Message}");
                    Console.WriteLine($"Config File: {ex.ConfigFile}, Section: {ex.ConfigSection}");
                    if (!string.IsNullOrEmpty(context))
                    {
                        Console.WriteLine($"Context: {context}");
                    }
                    Console.WriteLine($"Timestamp: {ex.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
                    Console.ResetColor();
                }
            }
            catch (Exception handlerEx)
            {
                // Fallback error handling
                Console.WriteLine($"Critical error in error handler: {handlerEx.Message}");
                Console.WriteLine($"Original error: {ex.Message}");
            }
        }

		/// <summary>
		/// Handle a file operation specific exception
		/// </summary>
		/// <param name="ex">The file operation exception to handle</param>
		/// <param name="context">Additional context about where the error occurred</param>
		/// <param name="showToUser">Whether to display the error to the user</param>
		public static void HandleFileOperationException(FileOperationException ex, string context = "", bool showToUser = true)
		{
			try
			{
				var errorMessage = BuildFileOperationErrorMessage(ex, context);
				
				// Log to error file
				WriteToErrorLog(errorMessage);

				// Show to user if requested
				if (showToUser)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"FILE OPERATION ERROR [{ex.ErrorCode}]: {ex.Message}");
					Console.WriteLine($"File: {ex.FilePath}, Operation: {ex.Operation}");
					if (!string.IsNullOrEmpty(context))
					{
						Console.WriteLine($"Context: {context}");
					}
					Console.WriteLine($"Timestamp: {ex.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
					Console.ResetColor();
				}
			}
			catch (Exception handlerEx)
			{
				// Fallback error handling
				Console.WriteLine($"Critical error in error handler: {handlerEx.Message}");
				Console.WriteLine($"Original error: {ex.Message}");
			}
		}

		/// <summary>
		/// Handle a connection specific exception
		/// </summary>
		/// <param name="ex">The connection exception to handle</param>
		/// <param name="context">Additional context about where the error occurred</param>
		/// <param name="showToUser">Whether to display the error to the user</param>
		public static void HandleConnectionException(ConnectionException ex, string context = "", bool showToUser = true)
		{
			try
			{
				var errorMessage = BuildConnectionErrorMessage(ex, context);
				
				// Log to error file
				WriteToErrorLog(errorMessage);

				// Show to user if requested
				if (showToUser)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"CONNECTION ERROR [{ex.ErrorCode}]: {ex.Message}");
					Console.WriteLine($"Connection Type: {ex.ConnectionType}, Retry Count: {ex.RetryCount}");
					if (!string.IsNullOrEmpty(context))
					{
						Console.WriteLine($"Context: {context}");
					}
					Console.WriteLine($"Timestamp: {ex.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
					Console.ResetColor();
				}
			}
			catch (Exception handlerEx)
			{
				// Fallback error handling
				Console.WriteLine($"Critical error in error handler: {handlerEx.Message}");
				Console.WriteLine($"Original error: {ex.Message}");
			}
		}

        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="context">Additional context</param>
        /// <param name="showToUser">Whether to display the warning to the user</param>
        public static void LogWarning(string message, string context = "", bool showToUser = true)
        {
            try
            {
                var warningMessage = $"[WARNING] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}";
                if (!string.IsNullOrEmpty(context))
                {
                    warningMessage += $" | Context: {context}";
                }

                WriteToErrorLog(warningMessage);

                if (showToUser)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"WARNING: {message}");
                    if (!string.IsNullOrEmpty(context))
                    {
                        Console.WriteLine($"Context: {context}");
                    }
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Log an informational message
        /// </summary>
        /// <param name="message">The info message</param>
        /// <param name="context">Additional context</param>
        /// <param name="showToUser">Whether to display the info to the user</param>
        public static void LogInfo(string message, string context = "", bool showToUser = false)
        {
            try
            {
                var infoMessage = $"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}";
                if (!string.IsNullOrEmpty(context))
                {
                    infoMessage += $" | Context: {context}";
                }

                WriteToErrorLog(infoMessage);

                if (showToUser)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"INFO: {message}");
                    if (!string.IsNullOrEmpty(context))
                    {
                        Console.WriteLine($"Context: {context}");
                    }
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log info: {ex.Message}");
            }
        }

        private static void WriteToErrorLog(string message)
        {
            if (!_isInitialized || string.IsNullOrEmpty(_errorLogPath))
            {
                Console.WriteLine($"Error log not initialized: {message}");
                return;
            }

            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_errorLogPath, message + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to error log: {ex.Message}");
                Console.WriteLine($"Original message: {message}");
            }
        }

        private static string BuildErrorMessage(Exception ex, string context)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Type: {ex.GetType().Name}");
            sb.AppendLine($"Message: {ex.Message}");
            if (!string.IsNullOrEmpty(context))
            {
                sb.AppendLine($"Context: {context}");
            }
            sb.AppendLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
            }
            sb.AppendLine(new string('-', 80));
            return sb.ToString();
        }

        private static string BuildCOMPortLoggerErrorMessage(COMPortLoggerException ex, string context)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[COMPORT_LOGGER_ERROR] {ex.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Error Code: {ex.ErrorCode}");
            sb.AppendLine($"Type: {ex.GetType().Name}");
            sb.AppendLine($"Message: {ex.Message}");
            if (!string.IsNullOrEmpty(context))
            {
                sb.AppendLine($"Context: {context}");
            }
            sb.AppendLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
            }
            sb.AppendLine(new string('-', 80));
            return sb.ToString();
        }

        private static string BuildSerialPortErrorMessage(SerialPortException ex, string context)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[SERIAL_PORT_ERROR] {ex.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Error Code: {ex.ErrorCode}");
            sb.AppendLine($"Port Name: {ex.PortName}");
            sb.AppendLine($"Baud Rate: {ex.BaudRate}");
            sb.AppendLine($"Message: {ex.Message}");
            if (!string.IsNullOrEmpty(context))
            {
                sb.AppendLine($"Context: {context}");
            }
            sb.AppendLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
            }
            sb.AppendLine(new string('-', 80));
            return sb.ToString();
        }

        private static string BuildConfigurationErrorMessage(ConfigurationException ex, string context)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[CONFIGURATION_ERROR] {ex.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Error Code: {ex.ErrorCode}");
            sb.AppendLine($"Config File: {ex.ConfigFile}");
            sb.AppendLine($"Config Section: {ex.ConfigSection}");
            sb.AppendLine($"Message: {ex.Message}");
            if (!string.IsNullOrEmpty(context))
            {
                sb.AppendLine($"Context: {context}");
            }
            sb.AppendLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
            }
            sb.AppendLine(new string('-', 80));
            return sb.ToString();
        }

		private static string BuildFileOperationErrorMessage(FileOperationException ex, string context)
		{
			var sb = new StringBuilder();
			sb.AppendLine($"[FILE_OPERATION_ERROR] {ex.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
			sb.AppendLine($"Error Code: {ex.ErrorCode}");
			sb.AppendLine($"File Path: {ex.FilePath}");
			sb.AppendLine($"Operation: {ex.Operation}");
			sb.AppendLine($"Message: {ex.Message}");
			if (!string.IsNullOrEmpty(context))
			{
				sb.AppendLine($"Context: {context}");
			}
			sb.AppendLine($"Stack Trace: {ex.StackTrace}");
			if (ex.InnerException != null)
			{
				sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
			}
			sb.AppendLine(new string('-', 80));
			return sb.ToString();
		}

		private static string BuildConnectionErrorMessage(ConnectionException ex, string context)
		{
			var sb = new StringBuilder();
			sb.AppendLine($"[CONNECTION_ERROR] {ex.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
			sb.AppendLine($"Error Code: {ex.ErrorCode}");
			sb.AppendLine($"Connection Type: {ex.ConnectionType}");
			sb.AppendLine($"Retry Count: {ex.RetryCount}");
			sb.AppendLine($"Message: {ex.Message}");
			if (!string.IsNullOrEmpty(context))
			{
				sb.AppendLine($"Context: {context}");
			}
			sb.AppendLine($"Stack Trace: {ex.StackTrace}");
			if (ex.InnerException != null)
			{
				sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
			}
			sb.AppendLine(new string('-', 80));
			return sb.ToString();
		}
    }
}
