using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COM_Port_Logger
{
	public class ConfigSettings
	{
		public SerialPortConfig SerialPort { get; set; } = new SerialPortConfig();
		public LogFileSettings LogFile { get; set; } = new LogFileSettings();
	}

	public class SerialPortConfig
	{
		public string PortName { get; set; } = "COM1";
		public int BaudRate { get; set; } = 9600;
		public string Parity { get; set; } = "None";
		public int DataBits { get; set; } = 8;
		public string StopBits { get; set; } = "One";
		public string Handshake { get; set; } = "None";
	}

	public class LogFileSettings
	{
		public string Directory { get; set; } = "logs";
		public string FileName { get; set; } = "log.txt";
	}
}
