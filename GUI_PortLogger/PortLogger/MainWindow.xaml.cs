using PortLogger.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ConnectionIndicatorApp
{

	public partial class MainWindow : Window
	{
		private bool _isInitialized;
		private TcpListener _listener;
		private Thread _listenerThread;
		private bool _isConnected;
		private List<string> logMessages = new List<string>();
		private List<string> _connectedClients = new List<string>();
		private LogLevel _selectedLogLevel;
		static LogFile _logFile; 
		private string _selectedLoggingMode = "Ethernet";
		private IniFile _iniFile = new IniFile();

		public MainWindow()
		{
			InitializeComponent();
			_isInitialized = true;

			UpdateStatusIndicator(false);
			LoadAvailableSerialPorts();

			InitializeApplication();
			
			// Ensure the ComboBox selection logic runs after initialization
			LogLevelComboBox_SelectionChanged(logLevelComboBox, null);
			LoggingModeComboBox_SelectionChanged(loggingModeComboBox, null);

		} // End of MainWindow()


		/*
		 * Action Functions
		 */

		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			// Open the log file
			_logFile = FileHandler.CreateLogFile("logs", "log.txt");

			if (_selectedLoggingMode == "Ethernet")
			{
				StartEthernetLogging();
			}
			else if (_selectedLoggingMode == "Serial")
			{
				StartSerialLogging(sender, e);
			}

			btnStart.IsEnabled = false;
			btnStop.IsEnabled = true;

			_isConnected = true;
			UpdateStatusIndicator(_isConnected);

		} // End of btnStart_Click()

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			if (_selectedLoggingMode == "Ethernet")
			{
				StopEthernetLogging();
			}
			else if (_selectedLoggingMode == "Serial")
			{
				StopSerialLogging(sender, e);
			}

			btnStart.IsEnabled = true;
			btnStop.IsEnabled = false;

			_isConnected = false;
			UpdateStatusIndicator(_isConnected);

			// Log that the server has stopped
			LogMessage($"Server stopped.", LogLevel.INFO);

			btnSaveLog.IsEnabled = logMessages.Count > 0;
		} // End of btnStop_Click()

		private void btnSaveLog_Click(object sender, RoutedEventArgs e)
		{
			// Check if the log file exists and there are log messages
			if (logMessages.Count > 0)
			{
				// Close Original File
				_logFile.Close();
				_logFile.SetAsReadOnly();


				// Open a new file
				_logFile = FileHandler.CreateLogFile("logs", "log.txt");

				logMessages = new List<string>();

				btnSaveLog.IsEnabled = logMessages.Count > 0;
			}
		} // End of btnSaveLog_Click()

		private void btnAddSerialPort_Click(object sender, RoutedEventArgs e)
		{
			string portName = serialPortsComboBox.SelectedItem.ToString();
			string baudRate = baudRateComboBox.SelectedItem.ToString();

			dynamicTabControl.AddTab(portName, portName, baudRate);
		} // End of btnAddSerialPort_Click()

		private void btnExit_Click(object sender, RoutedEventArgs e)
		{

			Close();
		} // End of btnExit_Click()

		private void LogLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_isInitialized) return;

			// Get the selected log level from the ComboBox
			string selectedLogLevel = (logLevelComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

			// Parse the selected log level string to the LogLevel enum
			if (Enum.TryParse(selectedLogLevel, out LogLevel newLogLevel))
			{
				// Update the _selectedLogLevel
				_selectedLogLevel = newLogLevel;

				// Clear the logTextBox
				logTextBox.Clear();

				// Filter log messages based on the selected log level
				FilterLogMessages();

				_iniFile.AddKey("Debug", "LogLevel", selectedLogLevel);

				// Save the INI file
				_iniFile.Save("config.ini");
			}
		} // End of LogLevelComboBox_SelectionChanged()

		private void LoggingModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_isInitialized) return;

			_selectedLoggingMode = ((ComboBoxItem)loggingModeComboBox.SelectedItem).Content.ToString();

			if (_selectedLoggingMode == "Ethernet")
			{
				connLabel.Content = "Connected Clients";

				ipAddrLabel.Visibility = Visibility.Visible;
				ipAddrValueLabel.Visibility = Visibility.Visible;
				serialPortLabel.Visibility = Visibility.Collapsed;
				serialPortValueLabel.Visibility = Visibility.Collapsed;

				connectedClientsTextBox.Visibility = Visibility.Visible;
				ethernetSettingsPanel.Visibility = Visibility.Visible;
				serialSettingsPanel.Visibility = Visibility.Collapsed;
				serialPortsComboBox.Visibility = Visibility.Collapsed;
				connectionGroupBox.Visibility = Visibility.Visible;

				dynamicTabControl.Visibility = Visibility.Collapsed;

				Grid.SetColumn(loggerGroupBox, 2);
				Grid.SetColumnSpan(loggerGroupBox, 2);
			}
			else if (_selectedLoggingMode == "Serial")
			{
				connLabel.Content = "Available Serial Ports";
				ipAddrLabel.Visibility = Visibility.Collapsed;
				ipAddrValueLabel.Visibility = Visibility.Collapsed;
				serialPortLabel.Visibility = Visibility.Visible;
				serialPortValueLabel.Visibility = Visibility.Visible;

				connectedClientsTextBox.Visibility = Visibility.Collapsed;
				ethernetSettingsPanel.Visibility = Visibility.Collapsed;
				serialSettingsPanel.Visibility = Visibility.Visible;
				serialPortsComboBox.Visibility = Visibility.Visible;
				connectionGroupBox.Visibility = Visibility.Collapsed;

				dynamicTabControl.Visibility = Visibility.Visible;

				LoadAvailableSerialPorts();

				Grid.SetColumn(loggerGroupBox, 1);
				Grid.SetColumnSpan(loggerGroupBox, 3);
			}

			_iniFile.AddKey("Connection", "Mode", _selectedLoggingMode);

			// Save the INI file
			_iniFile.Save("config.ini");

		} // End of LoggingModeComboBox_SelectionChanged()


		private void SerialPortsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Get the selected item from the SerialPortComboBox
			string selectedSerialPort = (string)serialPortsComboBox.SelectedItem;
			ComboBoxItem selectedBaudRate = (ComboBoxItem)baudRateComboBox.SelectedItem;

			serialPortValueLabel.Content = selectedSerialPort;

			// Get the content of the selected item (which should be the baud rate as a string)
			string baudRateString = selectedBaudRate.Content.ToString();

			// Parse the baud rate string to an integer
			int baudRate = int.Parse(baudRateString);

			_iniFile.AddKey("Connection", "BaudRate", baudRateString);

			// Save the INI file
			_iniFile.Save("config.ini");
		} // End of SerialPortsComboBox_SelectionChanged()

		private void SerialPort_DataReceived(object sender, DataReceivedEventArgs e)
		{
			// Cast the sender object back to SerialPort
			SerialPort serialPort = (SerialPort)sender;

			// Read the data from the serial port
			string data = serialPort.ReadExisting();

			Dispatcher.Invoke((Action)(() =>
			{
				logTextBox.AppendText($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - INFO - {e.Data}{Environment.NewLine}");
			}));

			// Log the received data
			LogMessage(data, LogLevel.INFO);
		} // End of SerialPort_DataReceived()

		/*
		 * Ethernet Helper Functions
		 */

		private void ListenForClients()
		{
			try
			{
				while (true)
				{
					var client = _listener.AcceptTcpClient();

					var clientAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
					_connectedClients.Add(clientAddress);

					var clientThread = new Thread(() => HandleClient(client));
					clientThread.IsBackground = true;
					clientThread.Start();
				}
			}
			catch (SocketException)
			{
				// Handle server stop scenario
			}
		} // End of ListenForClients()

		private void HandleClient(TcpClient client)
		{
			using (client)
			{
				var buffer = new byte[1024];
				var stream = client.GetStream();

				var remoteEndpoint = client.Client.RemoteEndPoint.ToString(); // Get the IP address of the remote endpoint

				try
				{
					// Log the connection
					LogMessage($"{remoteEndpoint} is now connected", LogLevel.INFO);

					while (_isConnected && client.Connected)
					{
						var bytesRead = stream.Read(buffer, 0, buffer.Length);
						if (bytesRead > 0)
						{
							// Handle received data here
						}
						else
						{
							_isConnected = false;
							UpdateStatusIndicator(_isConnected);
							break;
						}
					}
				}
				catch
				{
					_isConnected = false;
					UpdateStatusIndicator(_isConnected);
				}
			}
		} // End of HandleClient()	

		/*
		 * Serial Helper Functions
		 */

		private void StartSerialLogging(object sender, RoutedEventArgs e)
		{
			// Add your code to start serial logging
			var selectedPort = serialPortsComboBox.SelectedItem as string;

			if (!string.IsNullOrEmpty(selectedPort))
			{
				// Initialize and start serial port logging using selectedPort
				LogMessage($"Serial logging started on {selectedPort}", LogLevel.INFO);
			}

			dynamicTabControl.StartButton_Click(sender, e);

		} // End of StartSerialLogging()

		private void StartEthernetLogging()
		{
			_listener = new TcpListener(IPAddress.Any, 5000);
			_listener.Start();
			_listenerThread = new Thread(ListenForClients);
			_listenerThread.IsBackground = true;
			_listenerThread.Start();


			// Log that the server has started
			LogMessage($"Server started successfully.", LogLevel.INFO);
		} // End of StartEthernetLogging()


		private void StopSerialLogging(object sender, RoutedEventArgs e)
		{
			// Add your code to start serial logging
			var selectedPort = serialPortsComboBox.SelectedItem as string;
			if (!string.IsNullOrEmpty(selectedPort))
			{
				// Initialize and start serial port logging using selectedPort
				LogMessage($"Serial logging stopped on {selectedPort}", LogLevel.INFO);
			}

			dynamicTabControl.StopButton_Click(sender, e);

		} // End of StopSerialLogging()

		private void StopEthernetLogging()
		{
			_isConnected = false;
			UpdateStatusIndicator(_isConnected);

			_listener.Stop();
			_listenerThread.Join();
		} // End of StopEthernetLogging()

		/*
		 * Helper Functions
		 */

		private void InitializeApplication()
		{
			bool status = true;
			status = _iniFile.Load("config.ini");
			if(status)
			{
				// Get a value from the INI file
				string logLevel = _iniFile.GetValue("Debug", "LogLevel");
				string loggingMode = _iniFile.GetValue("Connection", "Mode");
				string baudrate = _iniFile.GetValue("Connection", "BaudRate");
				string ipAddress = _iniFile.GetValue("Ethernet", "IPAddress");
				string ipAddressPrt = _iniFile.GetValue("Ethernet", "Port");

				// Parse the selected log level string to the LogLevel enum
				if (Enum.TryParse(logLevel, out LogLevel newLogLevel))
				{
					_selectedLogLevel = newLogLevel; // Log level from Config file
				}

				// Set the default log level in the ComboBox
				logLevelComboBox.SelectedIndex = (int)_selectedLogLevel;

				// Set the ComboBox selection based on the logging mode
				if (loggingMode != null)
				{
					foreach (ComboBoxItem item in loggingModeComboBox.Items)
					{
						if (item.Content.ToString().Equals(loggingMode, StringComparison.OrdinalIgnoreCase))
						{
							loggingModeComboBox.SelectedItem = item;
							break;
						}
					}

					_selectedLoggingMode = (loggingModeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
				}

				// Set the ComboBox selection based on the baud rate
				if (baudrate != null)
				{
					foreach (ComboBoxItem item in baudRateComboBox.Items)
					{
						if (item.Content.ToString().Equals(baudrate, StringComparison.OrdinalIgnoreCase))
						{
							baudRateComboBox.SelectedItem = item;
							break;
						}
					}
				}

				// Set the ComboBox selection based on the baud rate
				if (ipAddress != null)
				{
					ipAddrValueLabel.Content = ipAddress;
					hostTextBox.Text = ipAddress;
				}

				if(ipAddressPrt != null)
				{
					portTextBox.Text = ipAddressPrt;
				}

			}
			else
			{

				// Get the selected log level from the ComboBox
				string selectedLogLevel = (logLevelComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
				string selectedLoggingMode = (loggingModeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
				string selectedBaudRate = (baudRateComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
				string selectedIPAddress = ipAddrValueLabel.Content.ToString();
				string selectedIPAddressPort = "8080";

				portTextBox.Text = "8080";

				// Save Debug configuration
				_iniFile.AddSection("Debug");
				_iniFile.AddKey("Debug", "LogLevel", selectedLogLevel); // Example: "DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL"

				// Add sections and keys
				_iniFile.AddSection("Connection");
				_iniFile.AddKey("Connection", "Mode", _selectedLoggingMode); // Example: "Ethernet" or "Serial"
				_iniFile.AddKey("Connection", "BaudRate", selectedBaudRate); // Example: "9600" for baud rate


				// Save Ethernet configuration
				_iniFile.AddSection("Ethernet");
				_iniFile.AddKey("Ethernet", "IPAddress", selectedIPAddress); // Example: IP address
				_iniFile.AddKey("Ethernet", "Port", selectedIPAddressPort); // Example: Port number

				// Initalizing Fields
				portTextBox.Text = selectedIPAddressPort;
				hostTextBox.Text = selectedIPAddress;

				// Save the INI file
				_iniFile.Save("config.ini");
			}
		} // End of InitializeApplication()

		private void FilterLogMessages()
		{
			// Get the selected log level from the ComboBox
			string selectedLogLevel = (logLevelComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

			// If no log level is selected, show all messages
			if (string.IsNullOrEmpty(selectedLogLevel))
			{
				foreach (string message in logMessages)
				{
					logTextBox.AppendText(message + Environment.NewLine);
				}
				return;
			}

			// Filter messages based on the selected log level
			foreach (string message in logMessages)
			{
				if (IsMessageLogLevelHigher(message, selectedLogLevel))
				{
					logTextBox.AppendText(message + Environment.NewLine);
				}
			}
		} // End of FilterLogMessages()

		private bool IsMessageLogLevelHigher(string message, string selectedLogLevel)
		{
			// Implement logic to determine if the log level of the message is equal to or higher than the selected log level
			// For example, you can compare the log levels using a predefined hierarchy (e.g., INFO < DEBUG < WARNING < ERROR)
			// You may need to parse the message to extract its log level
			// This method should return true if the log level of the message is equal to or higher than the selected log level, and false otherwise
			// Replace this logic with your actual implementation
			return true;
		} // End of IsMessageLogLevelHigher()

		private void LoadAvailableSerialPorts()
		{
			serialPortsComboBox.Items.Clear();
			foreach (var port in SerialPort.GetPortNames())
			{
				serialPortsComboBox.Items.Add(port);
			}
		} // End of LoadAvailableSerialPorts()

		private void UpdateStatusIndicator(bool isConnected)
		{
			ThreadStart start = delegate
			{
				Dispatcher.Invoke(DispatcherPriority.Normal, method: new Action<bool>(SetStatus), arg: isConnected);
			};
			new Thread(start).Start();
		} // End of UpdateStatusIndicator()

		private void SetStatus(bool isConnected)
		{
			statusCircle.Fill = isConnected ? Brushes.Green : Brushes.Red;
			statusTextBlock.Text = isConnected ? "Connected" : "Not Connected";

			// Update connected clients list
			connectedClientsTextBox.Text = string.Join(Environment.NewLine, _connectedClients);
		} // End of SetStatus()

		private void LogMessage(string message, LogLevel logLevel)
		{
			// Check if the specified log level is at or above the threshold level
			if (logLevel >= _selectedLogLevel)
			{
				// Create the log message with the specified format
				string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {logLevel.ToString()} - {message}";

				if (_selectedLoggingMode == "Ethernet")
				{
					// Append the message to the logTextBox with a newline
					logTextBox.AppendText(logEntry + Environment.NewLine);

					// Write the log message to the log file
					_logFile.WriteLine(logEntry);

					// Add the log message to the logMessages list
					logMessages.Add(logEntry);
				}
				else if (_selectedLoggingMode == "Serial")
				{
					dynamicTabControl.LogMessage(logEntry);
				}
			}
		} // End of LogMessage()
	}
}
