using COM_Port_Logger.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using YourNamespace;

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
		private TcpListener _listener;
		private Thread _listenerThread;
		private bool _isConnected;
		private List<string> logMessages = new List<string>();
		private List<string> _connectedClients = new List<string>();
		private LogLevel _selectedLogLevel;
		static StreamWriter _logFile; // StreamWriter for log file
		private string logFilePath = "log.txt"; // Path to the log file

		public MainWindow()
		{
			InitializeComponent();
			UpdateStatusIndicator(false);
			logLevelComboBox.SelectionChanged += LogLevelComboBox_SelectionChanged;
			_selectedLogLevel = LogLevel.INFO; // Default log level

			// Set the default log level in the ComboBox
			logLevelComboBox.SelectedIndex = (int)_selectedLogLevel;
		}

		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			StartServer();
			btnStart.IsEnabled = false;
			btnStop.IsEnabled = true;

			// Open the log file
			// string logDirectory = InputValidator.ValidateLogDirectory(_config.LogFile.BaseDirectory);
			// string logFileName = InputValidator.ValidateLogFileName(_config.LogFile.FileName);
			// logFilePath = System.IO.Path.Combine("logs", "log.txt");
			_logFile = FileHandler.CreateLogFile("logs", "log.txt");

			// Log that the server has started
			LogMessage($"Server started successfully.", LogLevel.INFO);
		}

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			StopServer();
			btnStart.IsEnabled = true;
			btnStop.IsEnabled = false;

			// Log that the server has stopped
			LogMessage($"Server stopped.", LogLevel.INFO);

			btnSaveLog.IsEnabled = logMessages.Count > 0;
		}

		private void btnSaveLog_Click(object sender, RoutedEventArgs e)
		{
			// Check if the log file exists and there are log messages
			if (logMessages.Count > 0)
			{
				// Close Original File
				_logFile.Close();
				
				// Open a new file
				_logFile = FileHandler.CreateLogFile("logs", "log.txt");

				logMessages = new List<string>();

				btnSaveLog.IsEnabled = logMessages.Count > 0;
			}
		}

		private void btnExit_Click(object sender, RoutedEventArgs e)
		{

		} // End of btnExit_Click()

		private void StartServer()
		{
			_listener = new TcpListener(IPAddress.Any, 5000);
			_listener.Start();
			_listenerThread = new Thread(ListenForClients);
			_listenerThread.IsBackground = true;
			_listenerThread.Start();

			_isConnected = true;
			UpdateStatusIndicator(_isConnected);
		}

		private void StopServer()
		{
			_isConnected = false;
			UpdateStatusIndicator(_isConnected);

			_listener.Stop();
			_listenerThread.Join();
		}

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
		}

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
		}

		private void LogMessage(string message, LogLevel logLevel)
		{
			// Create the log message with the specified format
			string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {logLevel.ToString()} - {message}";

			// Append the message to the logTextBox with a newline
			logTextBox.AppendText(logEntry + Environment.NewLine);

			// Write the log message to the log file
			_logFile.WriteLine(logEntry);

			// Add the log message to the logMessages list
			logMessages.Add(logEntry);
		} // End of LogMessage()

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
			//connectedClientsTextBox.Text = string.Join(Environment.NewLine, _connectedClients);
		} // End of SetStatus()

		// Event handler for the settings button click
		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			// Open the settings window
			SettingsWindow settingsWindow = new SettingsWindow("192.168.1.45",2500);
			settingsWindow.ShowDialog();
		} // End of Settings_Click()

		private void LogLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Clear the logTextBox
			logTextBox.Clear();

			// Filter log messages based on the selected log level
			FilterLogMessages();
		} // End of LogLevelComboBox_SelectionChanged()

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
	}
}
