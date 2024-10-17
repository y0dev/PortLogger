using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM_Port_Logger
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length == 0)
				{
					Console.WriteLine("Please provide the necessary arguments:");
					Console.WriteLine("Usage: <consoleName> OR <baseDirectory> <logFileName> <comPort> <baudRate> <colorSchemeName> <consoleTitle>");
					return;
				}

				// If only one argument is provided (console name)
				if (args.Length == 1)
				{
					string consoleName = args[0];
					Console.WriteLine($"Starting with console name: {consoleName}");

					// Call the original Start method with the console name
					PortLog.Start(consoleName);
				}
				// If six arguments are provided (full configuration)
				else if (args.Length == 6)
				{
					// Assign arguments to respective variables
					string baseDirectory = args[0];
					string logFileName = args[1];
					string comPort = args[2];
					int baudRate;

					// Parse baudRate argument to an integer
					if (!int.TryParse(args[3], out baudRate))
					{
						Console.WriteLine("Invalid baud rate. Please provide a valid integer value.");
						return;
					}

					string colorSchemeName = args[4];
					string consoleTitle = args[5];

					// Call the overloaded Start method with the provided arguments
					Console.WriteLine("Starting with full configuration...");
					PortLog.Start(baseDirectory, logFileName, comPort, baudRate, colorSchemeName, consoleTitle);
				}
				else
				{
					// Invalid number of arguments provided
					Console.WriteLine("Invalid number of arguments. Please provide:");
					Console.WriteLine("Usage: <consoleName> OR <baseDirectory> <logFileName> <comPort> <baudRate> <colorSchemeName> <consoleTitle>");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Unexpected error: {ex.Message}");
			}
		}
	}
}
