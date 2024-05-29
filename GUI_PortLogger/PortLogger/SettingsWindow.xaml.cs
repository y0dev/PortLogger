using System.ComponentModel;
using System.Windows;

namespace YourNamespace
{
	public partial class SettingsWindow : Window
	{
		// Bindable properties for IP address and port
		public string IpAddress { get; set; }
		public int Port { get; set; }

		public SettingsWindow(string ipAddress, int port)
		{
			InitializeComponent();
			DataContext = this; // Set the data context to enable binding

			// Initialize the IP address and port from parameters
			IpAddress = ipAddress;
			Port = port;
		}

		// Event handler for the Save button click
		private void Save_Click(object sender, RoutedEventArgs e)
		{
			// Save settings to your application (e.g., update configuration file)
			// Close the settings window
			Close();
		}

		// Event handler for the Cancel button click
		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			// Close the settings window without saving changes
			Close();
		}
	}
}
