using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using COM_Port_Logger.Services;
using COM_Port_Logger.ConfigurationSettings;
using COM_Port_Logger.Exceptions;

namespace COM_Port_Logger
{
	public class PortLog
	{
		static bool _continue; // Flag to control the continuation of threads
		static SerialPort _serialPort; // SerialPort object
		static LogFileResult _logFileResult; // StreamWriter for log file
		static string _logMessage; // Message to be logged
		static readonly object _lock = new object(); // Lock object for synchronization
		static bool _reconnecting; // Flag to prevent multiple reconnection attempts simultaneously
		static ConfigSettings _config; // Configuration settings
		static ColorScheme _colorScheme; // Color scheme for console


		public static void Start(string consoleName)
		{
			try
			{
				// Initialize error handler
				ErrorHandler.Initialize("error_logs");

				// Load configuration settings for the specified console name
				_config = LoadConfig(consoleName);

				if (_config == null)
				{
					throw new ConfigurationException("", "", $"No configuration found for console name: {consoleName}");
				}

				_colorScheme = ColorScheme.GetColorScheme(_config.Display.ColorScheme);

				// Apply display settings
				ApplyDisplaySettings();

				// Initialize and configure the SerialPort
				_serialPort = new SerialPort();
				try
				{
					_serialPort.PortName = InputValidator.ValidatePortName(_config.SerialPort.PortName);
					_serialPort.BaudRate = InputValidator.ValidateBaudRate(_config.SerialPort.BaudRate);
					_serialPort.Parity = InputValidator.ValidateParity(_config.SerialPort.Parity);
					_serialPort.DataBits = InputValidator.ValidateDataBits(_config.SerialPort.DataBits);
					_serialPort.StopBits = InputValidator.ValidateStopBits(_config.SerialPort.StopBits);
					_serialPort.Handshake = InputValidator.ValidateHandshake(_config.SerialPort.Handshake);
					_serialPort.ReadTimeout = 500;
					_serialPort.WriteTimeout = 500;
				}
				catch (ValidationException ex)
				{
					throw new SerialPortException(_config.SerialPort.PortName, _config.SerialPort.BaudRate, 
						$"Serial port configuration validation failed: {ex.Message}", ex);
				}

				// Open the serial port
				try
				{
					_serialPort.Open();
					ErrorHandler.LogInfo($"Serial port {_serialPort.PortName} opened successfully", "Start");
				}
				catch (Exception ex)
				{
					throw new SerialPortException(_serialPort.PortName, _serialPort.BaudRate, 
						$"Failed to open serial port: {ex.Message}", ex);
				}

				// Open the log file
				try
				{
					string logDirectory = InputValidator.ValidateLogDirectory(_config.LogFile.BaseDirectory);
					string logFileName = InputValidator.ValidateLogFileName(_config.LogFile.FileName);
					_logFileResult = FileHandler.CreateLogFile(logDirectory, logFileName);
					string logFilePath = _logFileResult.FilePath;
					ErrorHandler.LogInfo($"Log file created: {logFilePath}", "Start");
				}
				catch (ValidationException ex)
				{
					throw new ConfigurationException("", "", $"Log file configuration validation failed: {ex.Message}", ex);
				}
				catch (FileOperationException ex)
				{
					throw new ConfigurationException("", "", $"Log file creation failed: {ex.Message}", ex);
				}

				_continue = true; // Set continuation flag to true

				// Start background threads for reading from serial port and writing to log file
				ThreadPool.QueueUserWorkItem(ReadSerialPort);
				ThreadPool.QueueUserWorkItem(WriteToLog);
				ThreadPool.QueueUserWorkItem(CheckConnection);
				
				Console.WriteLine("Type QUIT to exit");

				// Main loop to read user input and send to the serial port
				while (_continue)
				{
					try
					{
						string message = Console.ReadLine();
						if (string.IsNullOrEmpty(message))
						{
							// Handle empty message
							continue;
						}
						else if (string.Equals("QUIT", message, StringComparison.OrdinalIgnoreCase))
						{
							_continue = false; // Exit the loop if "QUIT" is entered
						}
						else
						{
							_serialPort.WriteLine(message); // Write the message to the serial port
						}
					}
					catch (Exception ex)
					{
						ErrorHandler.HandleSerialPortException(
							new SerialPortException(_serialPort.PortName, _serialPort.BaudRate, 
								$"Error sending message to serial port: {ex.Message}", ex), 
							"User input processing");
					}
				}

				// Cleanup
				CleanupResources();
			}
			catch (COMPortLoggerException ex)
			{
				ErrorHandler.HandleCOMPortLoggerException(ex, "Start method");
				throw; // Re-throw to allow caller to handle
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleException(ex, "Start method");
				throw; // Re-throw to allow caller to handle
			}
		} // End of Start()

		public static void Start(string baseDirectory, string logFileName, string comPort, int baudRate, string colorSchemeName, string consoleTitle)
		{
			try
			{
				// Initialize custom configuration settings based on the provided parameters
				_config = new ConfigSettings
				{
					LogFile = new LogFileSettings
					{
						BaseDirectory = InputValidator.ValidateLogDirectory(baseDirectory),
						FileName = logFileName // You can add an additional parameter for the log file name if needed
					},
					SerialPort = new SerialPortConfig
					{
						PortName = InputValidator.ValidatePortName(comPort),
						BaudRate = baudRate, // Default baud rate, you can add an extra parameter for this if needed
						Parity = "None",  // Default parity
						DataBits = 8,     // Default data bits
						StopBits = "One", // Default stop bits
						Handshake = "None"// Default handshake
					},
					Display = new DisplaySettings
					{
						ColorScheme = colorSchemeName,
						ConsoleName = consoleTitle
					}
				};

				// Set the color scheme based on the passed color scheme name
				_colorScheme = ColorScheme.GetColorScheme(colorSchemeName);



				// Apply display settings for console background, foreground color, and title
				ApplyDisplaySettings();

				// Initialize and configure the SerialPort
				_serialPort = new SerialPort();
				_serialPort.PortName = InputValidator.ValidatePortName(comPort);
				_serialPort.BaudRate = _config.SerialPort.BaudRate;
				_serialPort.Parity = InputValidator.ValidateParity(_config.SerialPort.Parity);
				_serialPort.DataBits = _config.SerialPort.DataBits;
				_serialPort.StopBits = InputValidator.ValidateStopBits(_config.SerialPort.StopBits);
				_serialPort.Handshake = InputValidator.ValidateHandshake(_config.SerialPort.Handshake);
				_serialPort.ReadTimeout = 500;
				_serialPort.WriteTimeout = 500;

				// Open the serial port
				_serialPort.Open();

				// Combine base directory and log file name to form the full log file path
				_logFileResult = FileHandler.CreateLogFile(_config.LogFile.BaseDirectory, _config.LogFile.FileName);
				string logFilePath = _logFileResult.FilePath;

				_continue = true; // Set continuation flag to true

				// Start background threads for reading from the serial port and writing to the log file
				ThreadPool.QueueUserWorkItem(ReadSerialPort);
				ThreadPool.QueueUserWorkItem(WriteToLog);
				ThreadPool.QueueUserWorkItem(CheckConnection);


				// Console output of important configuration settings
				Console.WriteLine("----- Configuration Information -----");
				Console.WriteLine($"Log File Path: {logFilePath}");
				Console.WriteLine($"COM Port: {_config.SerialPort.PortName}");
				Console.WriteLine($"Baud Rate: {_config.SerialPort.BaudRate}");
				Console.WriteLine($"Color Scheme: {_config.Display.ColorScheme}");
				Console.WriteLine("--------------------------------------\n");

				Console.WriteLine("Type QUIT to exit");

				// Main loop to read user input and send to the serial port
				while (_continue)
				{
					string message = Console.ReadLine();
					if (string.IsNullOrEmpty(message))
					{
						continue;
					}
					else if (string.Equals("QUIT", message, StringComparison.OrdinalIgnoreCase))
					{
						_continue = false; // Exit the loop if "QUIT" is entered
					}
					else
					{
						_serialPort.WriteLine(message); // Write the message to the serial port
					}
				}

				_serialPort.Close();
				_logFileResult.StreamWriter.Close();
				Console.ResetColor();

				// Set log file as read-only
				File.SetAttributes(logFilePath, File.GetAttributes(logFilePath) | FileAttributes.ReadOnly);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				Console.ResetColor();
			}
		} // End of Start()

		private static ConfigSettings LoadConfig(string consoleName)
		{
			var configDirectory = "configs"; // Directory containing the configuration files
			
			try
			{
				var configFiles = FileHandler.SearchConfigFiles(configDirectory);

				foreach (var configFilePath in configFiles)
				{
					var config = new ConfigSettings();
					ErrorHandler.LogInfo($"Loading configuration from: {configFilePath}", "LoadConfig");
					
					try
					{
						var lines = File.ReadAllLines(configFilePath);
						string currentSection = string.Empty;

						foreach (var line in lines)
						{
							var trimmedLine = line.Trim();
							if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";"))
								continue;

							if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
							{
								currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
							}
							else
							{
								var keyValue = trimmedLine.Split('=');
								if (keyValue.Length == 2)
								{
									var key = keyValue[0].Trim();
									var value = keyValue[1].Trim();

									try
									{
										switch (currentSection)
										{
											case "SerialPort":
												SetSerialPortSetting(config.SerialPort, key, value);
												break;
											case "LogFile":
												SetLogFileSetting(config.LogFile, key, value);
												break;
											case "Display":
												SetDisplaySetting(config.Display, key, value);
												break;
										}
									}
									catch (Exception ex)
									{
										throw new ConfigurationException(configFilePath, currentSection, 
											$"Error setting {key}={value}: {ex.Message}", ex);
									}
								}
							}
						}
						
						if (config.Display.ConsoleName.Equals(consoleName, StringComparison.OrdinalIgnoreCase))
						{
							if (ValidateCOMPort(config.SerialPort.PortName))
							{
								ErrorHandler.LogInfo($"Configuration loaded successfully for console: {consoleName}", "LoadConfig");
								return config;
							}
							else
							{
								throw new ConfigurationException(configFilePath, "SerialPort", 
									$"COM port {config.SerialPort.PortName} is not available");
							}
						}
					}
					catch (ConfigurationException)
					{
						throw; // Re-throw configuration exceptions
					}
					catch (Exception ex)
					{
						throw new ConfigurationException(configFilePath, "", 
							$"Error loading config file: {ex.Message}", ex);
					}
				}

				throw new ConfigurationException("", "", $"No configuration found for console name: {consoleName}");
			}
			catch (ConfigurationException)
			{
				throw; // Re-throw configuration exceptions
			}
			catch (Exception ex)
			{
				throw new ConfigurationException(configDirectory, "", 
					$"Error loading configuration: {ex.Message}", ex);
			}
		} // End of LoadConfig()

		private static bool ValidateCOMPort(string portName)
		{
			try
			{
				ErrorHandler.LogInfo($"Checking if we can connect to port {portName}", "ValidateCOMPort");
				using (var tempSerialPort = new SerialPort(portName))
				{
					tempSerialPort.Open();
					ErrorHandler.LogInfo($"Port {portName} is available", "ValidateCOMPort");
					return true; // COM port can be opened
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.LogWarning($"Port {portName} is not available: {ex.Message}", "ValidateCOMPort");
				return false; // COM port cannot be opened
			}
		} // End of ValidateCOMPort()

		private static void SetSerialPortSetting(SerialPortConfig settings, string key, string value)
		{
			switch (key)
			{
				case "PortName":
					settings.PortName = value;
					break;
				case "BaudRate":
					settings.BaudRate = int.Parse(value);
					break;
				case "Parity":
					settings.Parity = value;
					break;
				case "DataBits":
					settings.DataBits = int.Parse(value);
					break;
				case "StopBits":
					settings.StopBits = value;
					break;
				case "Handshake":
					settings.Handshake = value;
					break;
			}
		} // End of SetSerialPortSetting()

		private static void SetLogFileSetting(LogFileSettings settings, string key, string value)
		{
			if (key == "BaseDirectory")
			{
				settings.BaseDirectory = value;
			}
		} // End of SetLogFileSetting()

		private static void SetDisplaySetting(DisplaySettings settings, string key, string value)
		{
			switch (key)
			{
				case "ColorScheme":
					settings.ColorScheme = value;
					break;
				case "ConsoleName":
					settings.ConsoleName = value;
					break;
			}
		} // End of SetDisplaySetting()

		private static void ApplyDisplaySettings()
		{
			try
			{
				
				Console.BackgroundColor = _colorScheme.BackgroundColor;
				Console.ForegroundColor = _colorScheme.TextColor;
				Console.Title = _config.Display.ConsoleName;
				Console.Clear();
			}
			catch (ArgumentException ex)
			{
				Console.WriteLine($"Error applying display settings: {ex.Message}");
				Console.ResetColor();
			}
		} // End of ApplyDisplaySettings()

		private static void DisplayMessage(string message)
		{
			var numberRegex = new Regex(@"\d+");
			var parts = numberRegex.Split(message);
			var matches = numberRegex.Matches(message);

			Console.ForegroundColor = _colorScheme.TextColor;
			for (int i = 0; i < parts.Length; i++)
			{
				Console.Write(parts[i]);
				if (i < matches.Count)
				{
					Console.ForegroundColor = _colorScheme.NumberColor;
					Console.Write(matches[i].Value);
					Console.ForegroundColor = _colorScheme.TextColor;
				}
			}
			Console.WriteLine();
		}

		private static void ReadSerialPort(object state)
		{
			byte[] buffer = new byte[1024];
			while (_continue)
			{
				try
				{
					int bytesRead = _serialPort.Read(buffer, 0, buffer.Length);
					if (bytesRead > 0)
					{
						string message = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
						Console.WriteLine(message);

						lock (_lock)
						{
							_logMessage = message;
						}
					}
				}
				catch (TimeoutException)
				{
					// Handle timeout exception - this is normal behavior
					continue;
				}
				catch (IOException ex)
				{
					ErrorHandler.HandleSerialPortException(
						new SerialPortException(_serialPort.PortName, _serialPort.BaudRate, 
							$"IO error reading from serial port: {ex.Message}", ex), 
						"ReadSerialPort");
					_continue = false;
				}
				catch (InvalidOperationException ex)
				{
					ErrorHandler.HandleSerialPortException(
						new SerialPortException(_serialPort.PortName, _serialPort.BaudRate, 
							$"Serial port is not open: {ex.Message}", ex), 
						"ReadSerialPort");
					_continue = false;
				}
				catch (Exception ex)
				{
					ErrorHandler.HandleSerialPortException(
						new SerialPortException(_serialPort.PortName, _serialPort.BaudRate, 
							$"Unexpected error reading from serial port: {ex.Message}", ex), 
						"ReadSerialPort");
					_continue = false;
				}
			}
		} // End of ReadSerialPort()


		private static void WriteToLog(object state)
		{
			// Continuously write to the log file while the _continue flag is true
			while (_continue)
			{
				lock (_lock)
				{
					// If there is a log message to write
					if (_logMessage != null)
					{
						try
						{
							// Get current timestamp
							string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

							// Write the log message with timestamp to the file
							_logFileResult.StreamWriter.WriteLine($"{timestamp}: {_logMessage}");

							// Flush the stream to ensure the message is written
							_logFileResult.StreamWriter.Flush();

							// Clear the log message
							_logMessage = null;
						}
						catch (IOException ex)
						{
							ErrorHandler.HandleFileOperationException(
								new FileOperationException(_logFileResult.FilePath, "WriteToLog", 
									$"IO error writing to log file: {ex.Message}", ex), 
								"WriteToLog");
							_continue = false;
						}
						catch (ObjectDisposedException ex)
						{
							ErrorHandler.HandleFileOperationException(
								new FileOperationException(_logFileResult.FilePath, "WriteToLog", 
									$"Log file stream has been disposed: {ex.Message}", ex), 
								"WriteToLog");
							_continue = false;
						}
						catch (Exception ex)
						{
							ErrorHandler.HandleFileOperationException(
								new FileOperationException(_logFileResult.FilePath, "WriteToLog", 
									$"Unexpected error writing to log file: {ex.Message}", ex), 
								"WriteToLog");
							_continue = false;
						}
					}
				}

				Thread.Sleep(100); // Add a small delay to prevent high CPU usage
			}
		} // End of WriteToLog()

		private static void TryReconnect()
		{
			if (!_reconnecting)
			{
				_reconnecting = true;
				ErrorHandler.LogWarning("Attempting to reconnect to serial port", "TryReconnect");

				int retryCount = 0;
				const int maxRetries = 5;
				const int retryDelay = 5000; // 5 seconds

				while (!_serialPort.IsOpen && retryCount < maxRetries)
				{
					try
					{
						retryCount++;
						ErrorHandler.LogInfo($"Reconnection attempt {retryCount}/{maxRetries}", "TryReconnect");
						
						_serialPort.Open();
						ErrorHandler.LogInfo("Reconnection successful", "TryReconnect");
						break;
					}
					catch (Exception ex)
					{
						ErrorHandler.HandleConnectionException(
							new ConnectionException("SerialPort", retryCount, 
								$"Reconnection attempt {retryCount} failed: {ex.Message}", ex), 
							"TryReconnect");
						
						if (retryCount < maxRetries)
						{
							ErrorHandler.LogInfo($"Waiting {retryDelay/1000} seconds before next retry", "TryReconnect");
							Thread.Sleep(retryDelay);
						}
					}
				}

				if (retryCount >= maxRetries)
				{
					ErrorHandler.LogWarning($"Failed to reconnect after {maxRetries} attempts", "TryReconnect");
				}

				_reconnecting = false;
			}
		} // End of TryReconnect()

		private static void CheckConnection(object state)
		{
			while (_continue)
			{
				try
				{
					if (!_serialPort.IsOpen)
					{
						ErrorHandler.LogWarning("Serial port connection lost", "CheckConnection");
						TryReconnect();
					}
				}
				catch (Exception ex)
				{
					ErrorHandler.HandleConnectionException(
						new ConnectionException("SerialPort", 0, 
							$"Error checking connection: {ex.Message}", ex), 
						"CheckConnection");
				}

				Thread.Sleep(1000); // Check the connection status every 1 second
			}
		} // End of CheckConnection()

		private static void CleanupResources()
		{
			try
			{
				// Close serial port
				if (_serialPort != null && _serialPort.IsOpen)
				{
					_serialPort.Close();
					ErrorHandler.LogInfo("Serial port closed successfully", "CleanupResources");
				}

				// Close log file
				if (_logFileResult?.StreamWriter != null)
				{
					_logFileResult.StreamWriter.Close();
					ErrorHandler.LogInfo("Log file closed successfully", "CleanupResources");

					// Set log file as read-only
					try
					{
						if (!string.IsNullOrEmpty(_logFileResult.FilePath) && File.Exists(_logFileResult.FilePath))
						{
							File.SetAttributes(_logFileResult.FilePath, File.GetAttributes(_logFileResult.FilePath) | FileAttributes.ReadOnly);
							ErrorHandler.LogInfo($"Log file set as read-only: {_logFileResult.FilePath}", "CleanupResources");
						}
					}
					catch (Exception ex)
					{
						ErrorHandler.LogWarning($"Failed to set log file as read-only: {ex.Message}", "CleanupResources");
					}
				}

				// Reset console colors
				Console.ResetColor();
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleException(ex, "CleanupResources", false);
			}
		}

	} // End of PortLog class
} // End of COM_Port_Logger namespace
