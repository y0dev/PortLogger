using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PortLogger.Resources
{
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
		public void AddTab(string header, string content)
		{
			var newTab = new TabItemViewModel { Header = header, Content = content };
			Tabs.Add(newTab);
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
	}

    /// <summary>
    /// ViewModel for a tab item.
    /// </summary>
	public class TabItemViewModel
	{
		public string Header { get; set; }
		public string Content { get; set; }
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
