using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COM_Port_Logger.ConfigurationSettings
{
	/// <summary>
	/// Represents the complete configuration settings for the COM Port Logger application.
	/// Contains all necessary settings for serial port communication, logging, and display.
	/// </summary>
	public class ConfigSettings
	{
		/// <summary>
		/// Gets or sets the serial port configuration settings.
		/// </summary>
		public SerialPortConfig SerialPort { get; set; } = new SerialPortConfig();
		
		/// <summary>
		/// Gets or sets the log file configuration settings.
		/// </summary>
		public LogFileSettings LogFile { get; set; } = new LogFileSettings();
		
		/// <summary>
		/// Gets or sets the display configuration settings including color scheme and console appearance.
		/// </summary>
		public DisplaySettings Display { get; set; } = new DisplaySettings();
	}

	/// <summary>
	/// Represents configuration settings for serial port communication.
	/// Contains all parameters needed to establish a serial port connection.
	/// </summary>
	public class SerialPortConfig
	{
		/// <summary>
		/// Gets or sets the name of the COM port (e.g., "COM1", "COM3").
		/// </summary>
		public string PortName { get; set; } = "COM1";
		
		/// <summary>
		/// Gets or sets the baud rate for serial communication.
		/// Common values include 9600, 19200, 38400, 57600, 115200.
		/// </summary>
		public int BaudRate { get; set; } = 115200;
		
		/// <summary>
		/// Gets or sets the parity setting for error detection.
		/// Valid values: "None", "Odd", "Even", "Mark", "Space".
		/// </summary>
		public string Parity { get; set; } = "None";
		
		/// <summary>
		/// Gets or sets the number of data bits per character.
		/// Common values are 7 or 8.
		/// </summary>
		public int DataBits { get; set; } = 8;
		
		/// <summary>
		/// Gets or sets the stop bits configuration.
		/// Valid values: "None", "One", "OnePointFive", "Two".
		/// </summary>
		public string StopBits { get; set; } = "One";
		
		/// <summary>
		/// Gets or sets the handshake protocol for flow control.
		/// Valid values: "None", "XOnXOff", "RequestToSend", "RequestToSendXOnXOff".
		/// </summary>
		public string Handshake { get; set; } = "None";
	}

	/// <summary>
	/// Represents configuration settings for log file management.
	/// Defines where and how log files are created and stored.
	/// </summary>
	public class LogFileSettings
	{
		/// <summary>
		/// Gets or sets the base directory where log files will be stored.
		/// </summary>
		public string BaseDirectory { get; set; } = "logs";
		
		/// <summary>
		/// Gets or sets the name of the log file.
		/// </summary>
		public string FileName { get; set; } = "log.txt";
	}

	/// <summary>
	/// Represents configuration settings for console display and appearance.
	/// Controls the visual aspects of the application interface.
	/// </summary>
	public class DisplaySettings
	{
		/// <summary>
		/// Gets or sets the name/title of the console window.
		/// </summary>
		public string ConsoleName { get; set; }
		
		/// <summary>
		/// Gets or sets the color scheme name to be used for the console display.
		/// Must match one of the predefined color scheme names.
		/// </summary>
		public string ColorScheme { get; set; } = "DarkMode";
		
		/// <summary>
		/// Gets or sets the font size for the console display.
		/// </summary>
		public ushort FontSize { get; set; } = 24;
	}
}
