using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COM_Port_Logger.ConfigurationSettings
{
	public class ConfigSettings
	{
		public SerialPortConfig SerialPort { get; set; } = new SerialPortConfig();
		public LogFileSettings LogFile { get; set; } = new LogFileSettings();
		public DisplaySettings Display { get; set; } = new DisplaySettings(); // Added display settings
	}

	public class SerialPortConfig
	{
		public string PortName { get; set; } = "COM1";
		public int BaudRate { get; set; } = 115200;
		public string Parity { get; set; } = "None";
		public int DataBits { get; set; } = 8;
		public string StopBits { get; set; } = "One";
		public string Handshake { get; set; } = "None";
	}

	public class LogFileSettings
	{
		public string BaseDirectory { get; set; } = "logs";
		public string FileName { get; set; } = "log.txt";
	}

	public class DisplaySettings
	{
		public string ColorScheme { get; set; } = "DarkMode";
		public string ConsoleName { get; set; }
	}
}
