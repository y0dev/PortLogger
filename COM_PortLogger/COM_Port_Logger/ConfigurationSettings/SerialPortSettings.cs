using System;
using System.IO.Ports;

namespace COM_Port_Logger.ConfigurationSettings
{
	/// <summary>
	/// Provides time constants for various time intervals in milliseconds.
	/// Used for timing operations throughout the application.
	/// </summary>
	public static class TimeConstants
	{
		/// <summary>
		/// One second in milliseconds (1000ms).
		/// </summary>
		public const int Seconds = 1000;
		
		/// <summary>
		/// One minute in milliseconds (60000ms).
		/// </summary>
		public const int Minutes = 60000;
		
		/// <summary>
		/// One hour in milliseconds (3600000ms).
		/// </summary>
		public const int Hours = 3600000;
	}

	/// <summary>
	/// Provides static methods for interactive configuration of serial port settings.
	/// These methods display available options and allow user input for configuration.
	/// </summary>
	public static class SerialPortSettings
	{
		/// <summary>
		/// Interactively sets the COM port name by displaying available ports and allowing user selection.
		/// </summary>
		/// <param name="defaultPortName">The default port name to use if no input is provided.</param>
		/// <returns>The selected port name, or the default if no input is provided.</returns>
		public static string SetPortName(string defaultPortName)
		{
			// Display available ports and allow user to select one
			Console.WriteLine("Available Ports:");
			foreach (string s in SerialPort.GetPortNames())
			{
				Console.WriteLine(" {0}", s);
			}
			Console.Write("COM port({0}): ", defaultPortName);
			string portName = Console.ReadLine();
			if (string.IsNullOrEmpty(portName))
			{
				portName = defaultPortName; // Use default if no input
			}
			return portName;
		} // End of SetPortName()

		/// <summary>
		/// Interactively sets the baud rate for serial communication.
		/// </summary>
		/// <param name="defaultPortBaudRate">The default baud rate to use if no input is provided.</param>
		/// <returns>The selected baud rate, or the default if no input is provided.</returns>
		public static int SetPortBaudRate(int defaultPortBaudRate)
		{
			// Allow user to set the baud rate
			Console.Write("Baud Rate({0}): ", defaultPortBaudRate);
			string baudRate = Console.ReadLine();
			if (string.IsNullOrEmpty(baudRate))
			{
				return defaultPortBaudRate; // Use default if no input
			}
			return int.Parse(baudRate);
		} // End of SetPortBaudRate()

		/// <summary>
		/// Interactively sets the parity setting for error detection in serial communication.
		/// </summary>
		/// <param name="defaultPortParity">The default parity setting to use if no input is provided.</param>
		/// <returns>The selected parity setting, or the default if no input is provided.</returns>
		public static Parity SetPortParity(Parity defaultPortParity)
		{
			// Allow user to set the parity
			Console.WriteLine("Available Parity options: none, odd, even");
			Console.Write("Parity({0}): ", defaultPortParity.ToString());
			string parity = Console.ReadLine();
			if (string.IsNullOrEmpty(parity))
			{
				return defaultPortParity; // Use default if no input
			}
			return (Parity)Enum.Parse(typeof(Parity), parity, true);
		} // End of SetPortParity()

		/// <summary>
		/// Interactively sets the number of data bits per character in serial communication.
		/// </summary>
		/// <param name="defaultPortDataBits">The default number of data bits to use if no input is provided.</param>
		/// <returns>The selected number of data bits, or the default if no input is provided.</returns>
		public static int SetPortDataBits(int defaultPortDataBits)
		{
			// Allow user to set the data bits
			Console.Write("Data Bits({0}): ", defaultPortDataBits);
			string dataBits = Console.ReadLine();
			if (string.IsNullOrEmpty(dataBits))
			{
				return defaultPortDataBits; // Use default if no input
			}
			return int.Parse(dataBits);
		} // End of SetPortDataBits()

		/// <summary>
		/// Interactively sets the stop bits configuration for serial communication.
		/// </summary>
		/// <param name="defaultPortStopBits">The default stop bits setting to use if no input is provided.</param>
		/// <returns>The selected stop bits setting, or the default if no input is provided.</returns>
		public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
		{
			// Allow user to set the stop bits
			Console.WriteLine("Available Stop Bits options: None, One, OnePointFive, Two");
			Console.Write("Stop Bits({0}): ", defaultPortStopBits.ToString());
			string stopBits = Console.ReadLine();
			if (string.IsNullOrEmpty(stopBits))
			{
				return defaultPortStopBits; // Use default if no input
			}
			return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
		} // End of SetPortStopBits()

		/// <summary>
		/// Interactively sets the handshake protocol for flow control in serial communication.
		/// </summary>
		/// <param name="defaultPortHandshake">The default handshake setting to use if no input is provided.</param>
		/// <returns>The selected handshake setting, or the default if no input is provided.</returns>
		public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
		{
			// Allow user to set the handshake
			Console.WriteLine("Available Handshake options: None, XOnXOff, RequestToSend, RequestToSendXOnXOff");
			Console.Write("Handshake({0}): ", defaultPortHandshake.ToString());
			string handshake = Console.ReadLine();
			if (string.IsNullOrEmpty(handshake))
			{
				return defaultPortHandshake; // Use default if no input
			}
			return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
		} // End of SetPortHandshake()
	} // End of SerialPortSettings class
} // End of COM_Port_Logger namespace
