using PortLogger.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PortLogger.Resources
{
	/// <summary>
	/// ViewModel for a tab item.
	/// </summary>
	public class TabItemViewModel
	{
		private bool _isRunning;

		public string Header { get; set; }
		public TextBox LogTextBox { get; set; }
		public SerialPortReader SerialPortReader { get; set; }
		public LogFile SerialLogFile { get; set; }

		public bool IsRunning
		{
			get => _isRunning;
			set
			{
				if (_isRunning != value)
				{
					_isRunning = value;
					OnPropertyChanged(nameof(IsRunning));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	/// <summary>
	/// A UserControl that represents a dynamic TabControl with the ability to close tabs.
	/// </summary>
	public partial class DynamicTabControl : UserControl
	{
        // Collection of tab items
		public ObservableCollection<TabItemViewModel> Tabs { get; private set; }
        // The currently selected tab
		public TabItemViewModel SelectedTab { get; set; }

        // Command to close a tab
		public ICommand CloseTabCommand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTabControl"/> class.
        /// </summary>
		public DynamicTabControl()
		{
			InitializeComponent();
			Tabs = new ObservableCollection<TabItemViewModel>();
			CloseTabCommand = new RelayCommand<TabItemViewModel>(CloseTab);
			DataContext = this;
		}

        /// <summary>
        /// Adds a new tab with the specified header and content.
        /// </summary>
        /// <param name="header">The header of the new tab.</param>
        /// <param name="content">The content of the new tab.</param>
		public void AddTab(string header, string portName, string baudRateString)
		{
			int baudRate;
			if (!int.TryParse(baudRateString, out baudRate))
			{
				baudRate = 115200; // Default value or handle error
			}

			var logTextBox = new TextBox
			{
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
				IsReadOnly = true
			};

			// Open the log file
			LogFile logFile = FileHandler.CreateLogFile("logs", "log.txt");

			var serialPortReader = new SerialPortReader(portName, baudRate);
			serialPortReader.DataReceived += (s, e) =>
			{
				Dispatcher.Invoke((Action)(() =>
				{
					logTextBox.AppendText($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - INFO - {e.Data}{Environment.NewLine}");
					LogMessage($"{e.Data}{Environment.NewLine}", LogLevel.INFO);
				}));
			};

			TabItemViewModel newTab = new TabItemViewModel
			{
				Header = header,
				LogTextBox = logTextBox,
				SerialPortReader = serialPortReader,
				SerialLogFile = logFile
			};
			bool tabExists = false;
			foreach(TabItemViewModel tab in Tabs)
			{
				if(header == tab.Header)
				{
					tabExists = true;
					newTab = tab;
					break;
				}
			}

			if(!tabExists)
			{
				Tabs.Add(newTab);
			}
			SelectedTab = newTab;
		}

        /// <summary>
        /// Closes the specified tab.
        /// </summary>
        /// <param name="tab">The tab to be closed.</param>
		private void CloseTab(TabItemViewModel tab)
		{
			if (tab != null)
			{
				Tabs.Remove(tab);
			}
		}

		/// <summary>
		/// Start serial port on the specified tab.
		/// </summary>
		/// <param name="sender">Sender of the action</param>
		public void StartButton_Click(object sender, RoutedEventArgs e)
		{
			SelectedTab?.SerialPortReader.StartReading();
		}

		/// <summary>
		/// Stop serial port on the specified tab.
		/// </summary>
		/// <param name="sender">Sender of the action</param>
		public void StopButton_Click(object sender, RoutedEventArgs e)
		{
			SelectedTab?.SerialPortReader.StopReading();
		}

		public void StartSerialPort(TabItemViewModel tab)
		{
			if (!tab.IsRunning)
			{
				tab.SerialPortReader.StartReading();
				tab.IsRunning = true;
			}
		}

		public void StopSerialPort(TabItemViewModel tab)
		{
			if (tab.IsRunning)
			{
				tab.SerialPortReader.StopReading();
				tab.IsRunning = false;
			}
		}

		/// <summary>
		/// Stop serial port on the specified tab.
		/// </summary>
		/// <param name="sender">Sender of the action</param>
		public void LogMessage(string message, LogLevel logLevel)
		{
			// Determine if was written by main window or COM port
			if(!ContainsLogLevel(message))
			{
				string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {logLevel.ToString()} - {message}";
				SelectedTab?.SerialLogFile.WriteLine(logEntry);
			}
			else
			{
				SelectedTab?.SerialLogFile.WriteLine(message);
			}
		}

		private bool ContainsLogLevel(string message)
		{
			string pattern = @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3} - (\w+) -";
			var match = Regex.Match(message, pattern);

			if (match.Success)
			{
				// Extract the log level from the message
				string logLevelStr = match.Groups[1].Value;

				// Parse the log level from the extracted string
				return Enum.TryParse(logLevelStr, out LogLevel logLevel);
			}

			// If the message doesn't match the pattern or the log level couldn't be parsed, return false
			return false;
		} // End of ContainsLogLevel()
	}


    /// <summary>
    /// A generic command that delegates execution to specified actions.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
	public class RelayCommand<T> : ICommand
	{
		private readonly Action<T> _execute;
		private readonly Func<T, bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">The function that determines whether the action can be executed.</param>
		public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

        /// <summary>
        /// Determines whether the command can execute.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>true if the command can execute; otherwise, false.</returns>
		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute((T)parameter);
		}

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
		public void Execute(object parameter)
		{
			_execute((T)parameter);
		}

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}
	}
}
