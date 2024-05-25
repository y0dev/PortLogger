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
				PortLog.Start(); // Start the PortChat application
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Unexpected error: {ex.Message}");
			}
		}
	}
}
