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
					Console.WriteLine("Please provide the console name as a command-line argument.");
					return;
				}

				string consoleName = args[0];
				PortLog.Start(consoleName); // Start the PortChat application
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Unexpected error: {ex.Message}");
			}
		}
	}
}
