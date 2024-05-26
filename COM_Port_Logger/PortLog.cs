using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Text.Json;
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
		static readonly object _lockObject = new object(); // Object for thread synchronization
		static bool _reconnecting; // Flag to prevent multiple reconnection attempts simultaneously
		static ConfigSettings _config; // Configuration settings

		public static void Start()
		{
			try
			{
				// Load configuration settings
				_config = LoadConfig();

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

				// Start background threads for reading from serial port and writing to log file
				Task.Run(() => ReadSerialPort());
				Task.Run(() => WriteToLog());
				Task.Run(() => CheckConnection()); // Start background task to check connection

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

				// Set log file as read-only
				File.SetAttributes(logFilePath, File.GetAttributes(logFilePath) | FileAttributes.ReadOnly);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
			}
		} // End of Start()

		private static ConfigSettings LoadConfig()
		{
			try
			{
				// Read configuration from config.json file
				string configFile = File.ReadAllText("config.json");
				// Deserialize JSON into ConfigSettings object
				return JsonSerializer.Deserialize<ConfigSettings>(configFile);
			}
			catch (FileNotFoundException)
			{
				Console.WriteLine("Config file not found. Using default settings.");
				// If config file is not found, return default settings
				return new ConfigSettings();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading config file: {ex.Message}");
				// If any other error occurs, return default settings
				return new ConfigSettings();
			}
		} // End of LoadConfig()

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

		private static void ReadSerialPort()
		{
			// Continuously read from the serial port
			while (_continue)
			{
				try
				{
					string message = _serialPort.ReadLine(); // Read a line from the serial port
					DisplayMessage(message); // Display the message with color handling
					lock (_lockObject)
					{
						_logMessage = message; // Set the log message
						Monitor.Pulse(_lockObject); // Signal the log thread
					}
				}
				catch (TimeoutException)
				{
					// Handle timeout exception (if needed)
				}
				catch (IOException ex)
				{
					Console.WriteLine($"Error reading from serial port: {ex.Message}");
					_continue = false; // Terminate the loop on IOException
				}
				catch (Exception ex)
				{
					Console.WriteLine($"An error occurred while reading from serial port: {ex.Message}");
				}
			}
		} // End of ReadSerialPort()

		private static void WriteToLog()
		{
			// Continuously write to the log file
			while (_continue)
			{
				lock (_lockObject)
				{
					Monitor.Wait(_lockObject); // Wait for a signal from the read thread
					if (_logMessage != null)
					{
						try
						{
							string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); // Get current timestamp
							_logFile.WriteLine($"{timestamp}: {_logMessage}"); // Write the log message with timestamp to the file
							_logFile.Flush(); // Flush the stream to ensure the message is written
							_logMessage = null; // Clear the log message
						}
						catch (IOException ex)
						{
							Console.WriteLine($"Error writing to log file: {ex.Message}");
							_continue = false; // Terminate the loop on IOException
						}
						catch (Exception ex)
						{
							Console.WriteLine($"An error occurred while writing to log file: {ex.Message}");
						}
					}
				}
			}
		} // End of WriteToLog()

		private static void OpenSerialPort()
		{
			try
			{
				if (!_serialPort.IsOpen)
				{
					_serialPort.Open();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error opening serial port: {ex.Message}");
				TryReconnect();
			}
		} // End of OpenSerialPort()

		private static void TryReconnect()
		{
			if (!_reconnecting)
			{
				_reconnecting = true;
				Console.WriteLine("Attempting to reconnect...");
				Task.Run(async () =>
				{
					while (!_serialPort.IsOpen)
					{
						OpenSerialPort();
						await Task.Delay(5000); // Wait for 5 seconds before attempting to reconnect again
					}
					Console.WriteLine("Reconnection successful.");
					_reconnecting = false;
				});
			}
		} // End of TryReconnect()

		private static void CheckConnection()
		{
			while (_continue)
			{
				if (!_serialPort.IsOpen)
				{
					TryReconnect();
				}
				Thread.Sleep(1000); // Check connection status every 1 second
			}
		} // End of CheckConnection()
	} // End of PortLog class
} // End of COM_Port_Logger namespace
