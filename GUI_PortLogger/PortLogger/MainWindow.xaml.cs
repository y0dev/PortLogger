﻿using COM_Port_Logger.Services;
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

	enum LogLevel
	{
		DEBUG,
		INFO,
		WARNING,
		ERROR,
		CRITICAL
	}

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
		private SerialPortReader _serialPortReader;

		public MainWindow()
		{
			InitializeComponent();
			_isInitialized = true;

			UpdateStatusIndicator(false);
			LoadAvailableSerialPorts();

			_iniFile.Load("config.ini");

			_selectedLogLevel = LogLevel.INFO; // Default log level

			// Set the default log level in the ComboBox
			logLevelComboBox.SelectedIndex = (int)_selectedLogLevel;

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
				StartSerialLogging();
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
				StopSerialLogging();
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
			}
		} // End of LogLevelComboBox_SelectionChanged()

		private void LoggingModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_isInitialized) return;

			_selectedLoggingMode = ((ComboBoxItem)loggingModeComboBox.SelectedItem).Content.ToString();

			if (_selectedLoggingMode == "Ethernet")
			{
				connLabel.Content = "Connected Clients";
				ipAddrLabel.Content = "IP Address";
				ipAddrValueLabel.Content = "127.0.0.0";
				connectedClientsTextBox.Visibility = Visibility.Visible;
				ethernetSettingsPanel.Visibility = Visibility.Visible;
				serialSettingsPanel.Visibility = Visibility.Collapsed;
				serialPortsComboBox.Visibility = Visibility.Collapsed;
				connectionGroupBox.Visibility = Visibility.Visible;

				Grid.SetColumn(loggerGroupBox, 2);
				Grid.SetColumnSpan(loggerGroupBox, 2);
			}
			else if (_selectedLoggingMode == "Serial")
			{
				connLabel.Content = "Available Serial Ports";
				ipAddrLabel.Content = "Serial Port";
				ipAddrValueLabel.Content = "";
				connectedClientsTextBox.Visibility = Visibility.Collapsed;
				ethernetSettingsPanel.Visibility = Visibility.Collapsed;
				serialSettingsPanel.Visibility = Visibility.Visible;
				serialPortsComboBox.Visibility = Visibility.Visible;
				connectionGroupBox.Visibility = Visibility.Collapsed;
				LoadAvailableSerialPorts();

				Grid.SetColumn(loggerGroupBox, 1);
				Grid.SetColumnSpan(loggerGroupBox, 3);
			}
		} // End of LoggingModeComboBox_SelectionChanged()


		private void SerialPortsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Get the selected item from the SerialPortComboBox
			string selectedSerialPort = (string)serialPortsComboBox.SelectedItem;
			ComboBoxItem selectedBaudRate = (ComboBoxItem)baudRateComboBox.SelectedItem;

			ipAddrValueLabel.Content = selectedSerialPort;

			// Get the content of the selected item (which should be the baud rate as a string)
			string baudRateString = selectedBaudRate.Content.ToString();

			// Parse the baud rate string to an integer
			int baudRate = int.Parse(baudRateString);


			_serialPortReader = new SerialPortReader(selectedSerialPort, baudRate);
		} // End of SerialPortsComboBox_SelectionChanged()

		private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			// Cast the sender object back to SerialPort
			SerialPort serialPort = (SerialPort)sender;

			// Read the data from the serial port
			string data = serialPort.ReadExisting();

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

		private void StartSerialLogging()
		{
			// Add your code to start serial logging
			var selectedPort = serialPortsComboBox.SelectedItem as string;

			if (!string.IsNullOrEmpty(selectedPort))
			{
				// Initialize and start serial port logging using selectedPort
				LogMessage($"Serial logging started on {selectedPort}", LogLevel.INFO);
			}

			_serialPortReader.StartReading();

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


		private void StopSerialLogging()
		{
			// Add your code to start serial logging
			var selectedPort = serialPortsComboBox.SelectedItem as string;
			if (!string.IsNullOrEmpty(selectedPort))
			{
				// Initialize and start serial port logging using selectedPort
				LogMessage($"Serial logging stopped on {selectedPort}", LogLevel.INFO);
			}

			_serialPortReader.StopReading();

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

				// Append the message to the logTextBox with a newline
				logTextBox.AppendText(logEntry + Environment.NewLine);

				// Write the log message to the log file
				_logFile.WriteLine(logEntry);

				// Add the log message to the logMessages list
				logMessages.Add(logEntry);
			}
		} // End of LogMessage()
	}
}
