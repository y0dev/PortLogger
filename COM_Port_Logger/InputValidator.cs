using System;
using System.IO.Ports;
using System.Linq;

namespace COM_Port_Logger
{
	public static class InputValidator
	{
		public static string ValidatePortName(string portName)
		{
			// Validate port name input
			if (!SerialPort.GetPortNames().Contains(portName))
			{
				Console.WriteLine($"Invalid port name '{portName}'. Using default port COM1.");
				return "COM1"; // Use default port if input is invalid
			}
			return portName;
		}

		public static int ValidateBaudRate(int baudRate)
		{
			// Validate baud rate input
			if (baudRate < 110 || baudRate > 256000)
			{
				Console.WriteLine($"Invalid baud rate '{baudRate}'. Using default rate 9600.");
				return 9600; // Use default baud rate if input is out of range
			}
			return baudRate;
		}

		public static Parity ValidateParity(string parity)
		{
			// Validate parity input
			if (!Enum.TryParse(parity, out Parity parsedParity))
			{
				Console.WriteLine($"Invalid parity '{parity}'. Using default parity None.");
				return Parity.None; // Use default parity if input is invalid
			}
			return (Parity)parsedParity;
		}

		public static int ValidateDataBits(int dataBits)
		{
			// Validate data bits input
			if (dataBits < 5 || dataBits > 8)
			{
				Console.WriteLine($"Invalid data bits '{dataBits}'. Using default value 8.");
				return 8; // Use default data bits if input is out of range
			}
			return dataBits;
		}

		public static StopBits ValidateStopBits(string stopBits)
		{
			// Validate stop bits input
			if (!Enum.TryParse(stopBits, out StopBits parsedStopBits))
			{
				Console.WriteLine($"Invalid stop bits '{stopBits}'. Using default value One.");
				return StopBits.One; // Use default stop bits if input is invalid
			}
			return parsedStopBits;
		}

		public static Handshake ValidateHandshake(string handshake)
		{
			// Validate handshake input
			if (!Enum.TryParse(handshake, out Handshake parsedHandshake))
			{
				Console.WriteLine($"Invalid handshake '{handshake}'. Using default value None.");
				return Handshake.None; // Use default handshake if input is invalid
			}
			return parsedHandshake;
		}

		public static string ValidateLogDirectory(string directory)
		{
			// Validate log directory input
			if (string.IsNullOrWhiteSpace(directory))
			{
				Console.WriteLine("Invalid log directory. Using default directory 'logs'.");
				return "logs"; // Use default directory if input is empty or null
			}
			return directory;
		}

		public static string ValidateLogFileName(string fileName)
		{
			// Validate log file name input
			if (string.IsNullOrWhiteSpace(fileName))
			{
				Console.WriteLine("Invalid log file name. Using default name 'log.txt'.");
				return "log.txt"; // Use default file name if input is empty or null
			}
			return fileName;
		}
	}
}
