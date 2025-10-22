using System;
using System.Collections.Generic;
using System.IO;
using COM_Port_Logger.Exceptions;

namespace COM_Port_Logger.Services
{
	public class LogFileResult
	{
		public StreamWriter StreamWriter { get; set; }
		public string FilePath { get; set; }
	}

	public static class FileHandler
	{
		public static LogFileResult CreateLogFile(string baseDirectory, string filename)
		{
			try
			{
				// Validate inputs
				if (string.IsNullOrWhiteSpace(baseDirectory))
				{
					throw new ValidationException("BaseDirectory", baseDirectory, "Base directory cannot be null or empty");
				}

				if (string.IsNullOrWhiteSpace(filename))
				{
					throw new ValidationException("FileName", filename, "Filename cannot be null or empty");
				}

				// Get the current date and time
				DateTime now = DateTime.Now;

				// Create the directory path based on the current date and time
				string directoryPath = Path.Combine(baseDirectory,
					now.ToString("yyyy"),
					now.ToString("MM_MMM"),
					now.ToString("MM_dd"));

				// Ensure the directory exists
				try
				{
					Directory.CreateDirectory(directoryPath);
				}
				catch (Exception ex)
				{
					throw new FileOperationException(directoryPath, "CreateDirectory", 
						$"Failed to create directory: {ex.Message}", ex);
				}

				// Remove the .txt extension from the filename
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

				// Create the log file path
				string filePath = Path.Combine(directoryPath, $"{now:HH_mm_ss}_{fileNameWithoutExtension}.txt");

				// Create or open the log file with shared read access
				FileStream fileStream;
				try
				{
					fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
				}
				catch (Exception ex)
				{
					throw new FileOperationException(filePath, "CreateFileStream", 
						$"Failed to create file stream: {ex.Message}", ex);
				}

				StreamWriter streamWriter;
				try
				{
					streamWriter = new StreamWriter(fileStream);
				}
				catch (Exception ex)
				{
					fileStream?.Dispose();
					throw new FileOperationException(filePath, "CreateStreamWriter", 
						$"Failed to create stream writer: {ex.Message}", ex);
				}

				// Return both StreamWriter and the file path in a custom class
				return new LogFileResult { StreamWriter = streamWriter, FilePath = filePath };
			}
			catch (ValidationException)
			{
				throw; // Re-throw validation exceptions
			}
			catch (FileOperationException)
			{
				throw; // Re-throw file operation exceptions
			}
			catch (Exception ex)
			{
				throw new FileOperationException(baseDirectory, "CreateLogFile", 
					$"Unexpected error creating log file: {ex.Message}", ex);
			}
		} // End of CreateLogFile()

		public static List<string> SearchConfigFiles(string directoryPath)
		{
			var iniFiles = new List<string>();

			try
			{
				// Validate input
				if (string.IsNullOrWhiteSpace(directoryPath))
				{
					throw new ValidationException("DirectoryPath", directoryPath, "Directory path cannot be null or empty");
				}

				if (!Directory.Exists(directoryPath))
				{
					throw new FileOperationException(directoryPath, "SearchConfigFiles", 
						$"Directory does not exist: {directoryPath}");
				}

				// Search for all .ini files in the specified directory and its subdirectories
				foreach (var file in Directory.EnumerateFiles(directoryPath, "*.ini", SearchOption.AllDirectories))
				{
					iniFiles.Add(file);
				}
			}
			catch (ValidationException)
			{
				throw; // Re-throw validation exceptions
			}
			catch (FileOperationException)
			{
				throw; // Re-throw file operation exceptions
			}
			catch (Exception ex)
			{
				throw new FileOperationException(directoryPath, "SearchConfigFiles", 
					$"Unexpected error searching for config files: {ex.Message}", ex);
			}

			return iniFiles;
		}
	} // End of FileHandler class
} // End of COM_Port_Logger namespace
