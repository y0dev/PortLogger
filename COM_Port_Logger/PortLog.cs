using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using COM_Port_Logger.Services;
using COM_Port_Logger.ConfigurationSettings;

namespace COM_Port_Logger
{
	public class PortLog
	{
		static bool _continue; // Flag to control the continuation of threads
		static SerialPort _serialPort; // SerialPort object
		static StreamWriter _logFile; // StreamWriter for log file
		static string _logMessage; // Message to be logged
		static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // Semaphore for async synchronization
		static bool _reconnecting; // Flag to prevent multiple reconnection attempts simultaneously
		static ConfigSettings _config; // Configuration settings

		public static void Start(string consoleName)
		{
			try
			{
				// Load configuration settings for the specified console name
				_config = LoadConfig(consoleName);

				if (_config == null)
				{
					Console.WriteLine($"No configuration found for console name: {consoleName}");
					return;
				}

				ConsoleColor bgColor = Console.BackgroundColor;
				ConsoleColor fgColor = Console.ForegroundColor;
				string title = Console.Title;

				// Apply display settings
				ApplyDisplaySettings();

				// Initialize and configure the SerialPort
				_serialPort = new SerialPort();
				_serialPort.PortName = InputValidator.ValidatePortName(_config.SerialPort.PortName);
				_serialPort.BaudRate = InputValidator.ValidateBaudRate(_config.SerialPort.BaudRate);
				_serialPort.Parity = InputValidator.ValidateParity(_config.SerialPort.Parity);
				_serialPort.DataBits = InputValidator.ValidateDataBits(_config.SerialPort.DataBits);
				_serialPort.StopBits = InputValidator.ValidateStopBits(_config.SerialPort.StopBits);
				_serialPort.Handshake = InputValidator.ValidateHandshake(_config.SerialPort.Handshake);
				_serialPort.ReadTimeout = 500;
				_serialPort.WriteTimeout = 500;

				// Open the serial port and log file with shared read access
				_serialPort.Open();
				// Open the log file
				string logDirectory = InputValidator.ValidateLogDirectory(_config.LogFile.BaseDirectory);
				string logFileName = InputValidator.ValidateLogFileName(_config.LogFile.FileName);
				string logFilePath = Path.Combine(logDirectory, logFileName);
				_logFile = FileHandler.CreateLogFile(logFilePath);

				_continue = true; // Set continuation flag to true

				// Start background tasks for reading from serial port and writing to log file
				_ = Task.Run(ReadSerialPortAsync);
				_ = Task.Run(WriteToLogAsync);
				_ = Task.Run(CheckConnectionAsync);

				// Define timeout duration (in milliseconds)
				int timeoutMilliseconds = 5 * TimeConstants.Minutes; // 5 minutes

				Console.WriteLine("Type QUIT to exit");

				// Main loop to read user input and send to the serial port
				while (_continue)
				{
					string message = Console.ReadLine();
					if (string.IsNullOrEmpty(message))
					{
						// Start timeout countdown if there is no user input
						DateTime startTime = DateTime.Now;
						while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMilliseconds)
						{
							if (!string.IsNullOrEmpty(Console.ReadLine()))
							{
								// Reset timeout countdown if user input is detected
								startTime = DateTime.Now;
							}
							else
							{
								// Exit the program if timeout expires
								_continue = false;
								break;
							}
						}
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
				_logFile.Close();
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
			var configFiles = FileHandler.SearchConfigFiles(configDirectory);

			foreach (var configFilePath in configFiles)
			{
				var config = new ConfigSettings();
				Console.WriteLine($"Configuration path: {configFilePath}");
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
						}
					}
					
					if (config.Display.ConsoleName.Equals(consoleName, StringComparison.OrdinalIgnoreCase))
					{
						if (ValidateCOMPort(config.SerialPort.PortName))
						{
							return config;
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error loading config file: {ex.Message}");
				}
			}

			return null;
			
		} // End of LoadConfig()

		private static bool ValidateCOMPort(string portName)
		{
			try
			{

				Console.WriteLine($"Checking if we can connect to port {portName}");
				using (var tempSerialPort = new SerialPort(portName))
				{
					tempSerialPort.Open();
					return true; // COM port can be opened
				}
			}
			catch
			{
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
				case "BackgroundColor":
					settings.BackgroundColor = value;
					break;
				case "TextColor":
					settings.TextColor = value;
					break;
				case "NumberColor":
					settings.NumberColor = value;
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
				Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), _config.Display.BackgroundColor, true);
				Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), _config.Display.TextColor, true);
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

			Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), _config.Display.TextColor, true);
			for (int i = 0; i < parts.Length; i++)
			{
				Console.Write(parts[i]);
				if (i < matches.Count)
				{
					Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), _config.Display.NumberColor, true);
					Console.Write(matches[i].Value);
					Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), _config.Display.TextColor, true);
				}
			}
			Console.WriteLine();
		}

		private static async Task ReadSerialPortAsync()
        {
            byte[] buffer = new byte[1024];
            while (_continue)
            {
                try
                {
                    int bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine(message);
                        await _semaphore.WaitAsync();
                        try
                        {
                            _logMessage = message;
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }
                }
                catch (TimeoutException)
                {
                    // Handle timeout exception (if needed)
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error reading from serial port: {ex.Message}");
                    _continue = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while reading from serial port: {ex.Message}");
                }
            }
        } // End of ReadSerialPortAsync()


		private static async Task WriteToLogAsync()
		{
			// Continuously write to the log file while the _continue flag is true
			while (_continue)
			{
				await _semaphore.WaitAsync();
				try
				{
					// If there is a log message to write
					if (_logMessage != null)
					{
						try
						{
							// Get current timestamp
							string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

							// Write the log message with timestamp to the file
							await _logFile.WriteLineAsync($"{timestamp}: {_logMessage}");

							// Flush the stream to ensure the message is written
							await _logFile.FlushAsync();

							// Clear the log message
							_logMessage = null;
						}
						catch (IOException ex)
						{
							// Handle IO exceptions that may occur while writing to the log file
							Console.WriteLine($"Error writing to log file: {ex.Message}");

							// Stop the writing loop by setting the _continue flag to false
							_continue = false;
						}
						catch (Exception ex)
						{
							// Handle any other exceptions that may occur
							Console.WriteLine($"An error occurred while writing to log file: {ex.Message}");
						}
					}
				}
				finally
				{
					_semaphore.Release();
				}
			}
		} // End of WriteToLogAsync()

		private static async Task OpenSerialPortAsync()
		{
			try
			{
				// Check if the serial port is not already open
				if (!_serialPort.IsOpen)
				{
					// Attempt to open the serial port
					_serialPort.Open();
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				// Handle access denied errors (e.g., when the port is in use by another application)
				Console.WriteLine($"Access denied to the serial port: {ex.Message}");
				await Task.Delay(5000); // Wait for 5 seconds before attempting to reconnect
			}
			catch (IOException ex)
			{
				// Handle IO exceptions that may occur while opening the serial port
				Console.WriteLine($"IO error opening serial port: {ex.Message}");
				await Task.Delay(5000); // Wait for 5 seconds before attempting to reconnect
			}
			catch (Exception ex)
			{
				// Handle any other exceptions that may occur
				Console.WriteLine($"An error occurred while opening the serial port: {ex.Message}");
				await Task.Delay(5000); // Wait for 5 seconds before attempting to reconnect
			}
		} // End of OpenSerialPortAsync()

		private static async Task TryReconnectAsync()
		{
			// Ensure only one reconnection attempt occurs at a time
			if (!_reconnecting)
			{
				_reconnecting = true;
				Console.WriteLine("Attempting to reconnect...");

				// Continuously attempt to reconnect until successful
				while (!_serialPort.IsOpen)
				{
					// Attempt to open the serial port asynchronously
					await OpenSerialPortAsync();
				}

				Console.WriteLine("Reconnection successful.");
				_reconnecting = false;
			}
		} // End of TryReconnectAsync()

		private static async Task CheckConnectionAsync()
		{
			// Continuously check the connection status while the _continue flag is true
			while (_continue)
			{
				// If the serial port is not open
				if (!_serialPort.IsOpen)
				{
					// Attempt to reconnect
					await TryReconnectAsync();
				}

				// Wait for 1 second before checking the connection status again
				await Task.Delay(1000);
			}
		} // End of CheckConnectionAsync()

	} // End of PortLog class
} // End of COM_Port_Logger namespace
