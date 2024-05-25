using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Text.Json;

namespace COM_Port_Logger
{
	public class PortLog
	{
		static bool _continue;
		static SerialPort _serialPort;
		static StreamWriter _logFile;
		static Thread readThread;
		static Thread logThread;
		static string _logMessage;
		static readonly object _lockObject = new object();

		public static void Start()
		{
			try
			{
				// Initialize and configure the SerialPort
				_serialPort = new SerialPort();
				_serialPort.PortName = SerialPortSettings.SetPortName(_serialPort.PortName);
				_serialPort.BaudRate = SerialPortSettings.SetPortBaudRate(_serialPort.BaudRate);
				_serialPort.Parity = SerialPortSettings.SetPortParity(_serialPort.Parity);
				_serialPort.DataBits = SerialPortSettings.SetPortDataBits(_serialPort.DataBits);
				_serialPort.StopBits = SerialPortSettings.SetPortStopBits(_serialPort.StopBits);
				_serialPort.Handshake = SerialPortSettings.SetPortHandshake(_serialPort.Handshake);
				_serialPort.ReadTimeout = 500;
				_serialPort.WriteTimeout = 500;

				// Open the serial port and log file with shared read access
				_serialPort.Open();
				_logFile = FileHandler.CreateLogFile("log");

				_continue = true; // Set continuation flag to true

				// Start the read and log threads
				readThread = new Thread(ReadSerialPort);
				logThread = new Thread(WriteToLog);
				readThread.Start();
				logThread.Start();

				Console.WriteLine("Type QUIT to exit");

				// Main loop to read user input and send to the serial port
				while (_continue)
				{
					string message = Console.ReadLine();
					if (string.Equals("QUIT", message, StringComparison.OrdinalIgnoreCase))
					{
						_continue = false; // Exit the loop if "QUIT" is entered
					}
					else
					{
						_serialPort.WriteLine(message); // Write the message to the serial port
					}
				}

				// Wait for threads to complete and close resources
				readThread.Join();
				logThread.Join();
				_serialPort.Close();
				_logFile.Close();
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
				return new ConfigSettings();  // JsonSerializer.Deserialize<ConfigSettings>(configFile);
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

		private static void ReadSerialPort()
		{
			// Continuously read from the serial port
			while (_continue)
			{
				try
				{
					string message = _serialPort.ReadLine(); // Read a line from the serial port
					Console.WriteLine(message); // Display the message
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
	} // End of PortLog class
} // End of COM_Port_Logger namespace
