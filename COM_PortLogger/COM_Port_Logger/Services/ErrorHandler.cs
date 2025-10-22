using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using COM_Port_Logger.Exceptions;
using COM_Port_Logger.Logging;

namespace COM_Port_Logger.Services
{
    /// <summary>
    /// Service responsible for handling errors and logging them appropriately
    /// </summary>
    public static class ErrorHandler
    {
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize the error handler
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                // Initialize the logging system if not already initialized
                if (Log._logger == null)
                {
                    Log.Initialize(LoggingConfigurationHelper.CreateDefaultConfiguration());
                }

                Log.Info("Error Handler initialized", "ErrorHandler");
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                // Fallback to console if logging initialization fails
                Console.WriteLine($"Failed to initialize error handler: {ex.Message}");
                _isInitialized = true; // Still mark as initialized to prevent infinite loops
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
                Initialize();
                
                var properties = new Dictionary<string, object>
                {
                    ["ExceptionType"] = ex.GetType().Name,
                    ["Context"] = context
                };

                if (ex.InnerException != null)
                {
                    properties["InnerException"] = ex.InnerException.Message;
                }

                Log.Error($"Exception occurred: {ex.Message}", context, "ErrorHandler", ex, properties);

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
				Initialize();
				
				var properties = new Dictionary<string, object>
				{
					["ErrorCode"] = ex.ErrorCode,
					["ExceptionType"] = ex.GetType().Name,
					["Context"] = context
				};

				if (ex.InnerException != null)
				{
					properties["InnerException"] = ex.InnerException.Message;
				}

				Log.Error($"COM Port Logger Exception [{ex.ErrorCode}]: {ex.Message}", context, "ErrorHandler", ex, properties);

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
				Initialize();
				
				var properties = new Dictionary<string, object>
				{
					["ErrorCode"] = ex.ErrorCode,
					["PortName"] = ex.PortName,
					["BaudRate"] = ex.BaudRate,
					["Context"] = context
				};

				if (ex.InnerException != null)
				{
					properties["InnerException"] = ex.InnerException.Message;
				}

				Log.Error($"Serial Port Exception [{ex.ErrorCode}]: {ex.Message}", context, "ErrorHandler", ex, properties);

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
				Initialize();
				
				var properties = new Dictionary<string, object>
				{
					["ErrorCode"] = ex.ErrorCode,
					["ConfigFile"] = ex.ConfigFile,
					["ConfigSection"] = ex.ConfigSection,
					["Context"] = context
				};

				if (ex.InnerException != null)
				{
					properties["InnerException"] = ex.InnerException.Message;
				}

				Log.Error($"Configuration Exception [{ex.ErrorCode}]: {ex.Message}", context, "ErrorHandler", ex, properties);

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
				Initialize();
				
				var properties = new Dictionary<string, object>
				{
					["ErrorCode"] = ex.ErrorCode,
					["FilePath"] = ex.FilePath,
					["Operation"] = ex.Operation,
					["Context"] = context
				};

				if (ex.InnerException != null)
				{
					properties["InnerException"] = ex.InnerException.Message;
				}

				Log.Error($"File Operation Exception [{ex.ErrorCode}]: {ex.Message}", context, "ErrorHandler", ex, properties);

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
				Initialize();
				
				var properties = new Dictionary<string, object>
				{
					["ErrorCode"] = ex.ErrorCode,
					["ConnectionType"] = ex.ConnectionType,
					["RetryCount"] = ex.RetryCount,
					["Context"] = context
				};

				if (ex.InnerException != null)
				{
					properties["InnerException"] = ex.InnerException.Message;
				}

				Log.Error($"Connection Exception [{ex.ErrorCode}]: {ex.Message}", context, "ErrorHandler", ex, properties);

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
				Initialize();
				Log.Warning(message, context, "ErrorHandler");

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
				Initialize();
				Log.Info(message, context, "ErrorHandler");

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
    }
}
