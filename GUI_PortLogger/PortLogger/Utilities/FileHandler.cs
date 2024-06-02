using System;
using System.Collections.Generic;
using System.IO;

namespace PortLogger.Utilities
{

	public enum LogLevel
	{
		DEBUG,
		INFO,
		WARNING,
		ERROR,
		CRITICAL
	}

	public class LogFile
	{
		public string FileName { get; private set; }
		private StreamWriter _streamWriter;

		public LogFile(string fileName)
		{
			FileName = fileName;

			// Create or open the log file with shared read access
			FileStream fileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
			_streamWriter = new StreamWriter(fileStream); // Open file in append mode
		}

		public void WriteLine(string message)
		{
			try
			{
				_streamWriter.WriteLine(message);
				_streamWriter.Flush();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error writing to log file: {ex.Message}");
			}
		}

		public void Close()
		{
			_streamWriter?.Close();
		}

		public void SetAsReadOnly()
		{
			try
			{
				File.SetAttributes(FileName, File.GetAttributes(FileName) | FileAttributes.ReadOnly);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error setting log file as read-only: {ex.Message}");
			}
		}
	}

	public static class FileHandler
	{
		public static LogFile CreateLogFile(string baseDirectory, string fileName)
		{
			// Get the current date and time
			DateTime now = DateTime.Now;

			// Create the directory path based on the current date and time
			string directoryPath = Path.Combine(baseDirectory,
				now.ToString("yyyy"),
				now.ToString("MM_MMM"),
				now.ToString("MM_dd"));

			// Ensure the directory exists
			Directory.CreateDirectory(directoryPath);

			// Create the log file path
			string filePath = Path.Combine(directoryPath, String.Format("{0}_{1}", now.ToString("HH_mm_ss"), fileName));

			
			LogFile logFile = new LogFile(filePath);

			return logFile;
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
