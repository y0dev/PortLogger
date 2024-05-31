using System;
using System.IO.Ports;

namespace COM_Port_Logger.ConfigurationSettings
{
	public static class TimeConstants
	{
		public const int Seconds = 1000;     // 1 second in milliseconds
		public const int Minutes = 60000;    // 1 minute in milliseconds
		public const int Hours = 3600000;    // 1 hour in milliseconds
	}

	public static class SerialPortSettings
	{
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
