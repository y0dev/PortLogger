﻿using System;
using System.Collections.Generic;
using System.IO;

namespace COM_Port_Logger.Services
{
	public static class FileHandler
	{
		public static StreamWriter CreateLogFile(string baseDirectory)
		{
			// Get the current date and time
			DateTime now = DateTime.Now;

			// Create the directory path based on the current date and time
			string directoryPath = Path.Combine(baseDirectory,
				now.ToString("yyyy"),
				now.ToString("MM_MMM"),
				now.ToString("MM_DD"),
				now.ToString("HH_mm_ss"));

			// Ensure the directory exists
			Directory.CreateDirectory(directoryPath);

			// Create the log file path
			string filePath = Path.Combine(directoryPath, "log.txt");

			// Create or open the log file with shared read access
			FileStream fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
			StreamWriter streamWriter = new StreamWriter(fileStream);

			return streamWriter;
		} // End of CreateLogFile()

		public static List<string> SearchConfigFiles(string directoryPath)
		{
			var iniFiles = new List<string>();

			try
			{
				// Search for all .ini files in the specified directory and its subdirectories
				foreach (var file in Directory.EnumerateFiles(directoryPath, "*.ini", SearchOption.AllDirectories))
				{
					iniFiles.Add(file);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error searching for .ini files: {ex.Message}");
			}

			return iniFiles;
		}
	} // End of FileHandler class
} // End of COM_Port_Logger namespace
