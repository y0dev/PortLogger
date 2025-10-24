using System;
using System.IO.Ports;
using System.Linq;
using COM_Port_Logger.Exceptions;

namespace COM_Port_Logger.Services
{
	/// <summary>
	/// Provides input validation and sanitization for serial port configuration parameters.
	/// Ensures all inputs are secure, valid, and within acceptable ranges.
	/// </summary>
	public static class InputValidator
	{
		/// <summary>
		/// Validates and sanitizes port name with security checks.
		/// Ensures the port name exists and is available on the system.
		/// </summary>
		/// <param name="portName">The port name to validate (e.g., "COM1", "COM3").</param>
		/// <returns>The sanitized and validated port name.</returns>
		/// <exception cref="ValidationException">Thrown when port name is invalid or not available.</exception>
		/// <exception cref="SecurityException">Thrown when security validation fails.</exception>
		public static string ValidatePortName(string portName)
		{
			try
			{
				// Initialize security service
				SecurityService.Initialize();

				// Validate port name input
				if (string.IsNullOrWhiteSpace(portName))
				{
					throw new ValidationException("PortName", portName, "Port name cannot be null or empty");
				}

				// Security sanitization
				var sanitizedPortName = SecurityService.SanitizePortName(portName);

				// Additional validation
				if (!SerialPort.GetPortNames().Contains(sanitizedPortName))
				{
					var availablePorts = string.Join(", ", SerialPort.GetPortNames());
					throw new ValidationException("PortName", sanitizedPortName, 
						$"Invalid port name '{sanitizedPortName}'. Available ports: {availablePorts}");
				}
				
				return sanitizedPortName;
			}
			catch (SecurityException ex)
			{
				throw new ValidationException("PortName", portName, 
					$"Security validation failed: {ex.Message}", ex);
			}
			catch (ValidationException)
			{
				throw; // Re-throw validation exceptions
			}
			catch (Exception ex)
			{
				throw new ValidationException("PortName", portName, 
					$"Error validating port name: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Validates and sanitizes baud rate with security checks.
		/// Ensures the baud rate is within acceptable range (110-256000).
		/// </summary>
		/// <param name="baudRate">The baud rate to validate.</param>
		/// <returns>The sanitized and validated baud rate.</returns>
		/// <exception cref="ValidationException">Thrown when baud rate is outside valid range.</exception>
		/// <exception cref="SecurityException">Thrown when security validation fails.</exception>
		public static int ValidateBaudRate(int baudRate)
		{
			try
			{
				// Initialize security service
				SecurityService.Initialize();

				// Security sanitization
				var sanitizedBaudRate = SecurityService.SanitizeBaudRate(baudRate);

				// Additional validation
				if (sanitizedBaudRate < 110 || sanitizedBaudRate > 256000)
				{
					throw new ValidationException("BaudRate", sanitizedBaudRate, 
						$"Invalid baud rate '{sanitizedBaudRate}'. Must be between 110 and 256000");
				}
				return sanitizedBaudRate;
			}
			catch (SecurityException ex)
			{
				throw new ValidationException("BaudRate", baudRate, 
					$"Security validation failed: {ex.Message}", ex);
			}
			catch (ValidationException)
			{
				throw; // Re-throw validation exceptions
			}
			catch (Exception ex)
			{
				throw new ValidationException("BaudRate", baudRate, 
					$"Error validating baud rate: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Validates parity setting for serial communication.
		/// Ensures the parity value is a valid enum value.
		/// </summary>
		/// <param name="parity">The parity setting to validate (e.g., "None", "Odd", "Even").</param>
		/// <returns>The validated Parity enum value.</returns>
		/// <exception cref="ValidationException">Thrown when parity value is invalid.</exception>
		public static Parity ValidateParity(string parity)
		{
			try
			{
				// Validate parity input
				if (string.IsNullOrWhiteSpace(parity))
				{
					throw new ValidationException("Parity", parity, "Parity cannot be null or empty");
				}

				if (!Enum.TryParse(parity, true, out Parity parsedParity))
				{
					var validValues = string.Join(", ", Enum.GetNames(typeof(Parity)));
					throw new ValidationException("Parity", parity, 
						$"Invalid parity '{parity}'. Valid values: {validValues}");
				}
				return parsedParity;
			}
			catch (ValidationException)
			{
				throw; // Re-throw validation exceptions
			}
			catch (Exception ex)
			{
				throw new ValidationException("Parity", parity, 
					$"Error validating parity: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Validates data bits setting for serial communication.
		/// Ensures the data bits value is within acceptable range (5-8).
		/// </summary>
		/// <param name="dataBits">The number of data bits to validate.</param>
		/// <returns>The validated data bits value.</returns>
		/// <exception cref="ValidationException">Thrown when data bits value is outside valid range.</exception>
		public static int ValidateDataBits(int dataBits)
		{
			try
			{
				// Validate data bits input
				if (dataBits < 5 || dataBits > 8)
				{
					throw new ValidationException("DataBits", dataBits, 
						$"Invalid data bits '{dataBits}'. Must be between 5 and 8");
				}
				return dataBits;
			}
			catch (ValidationException)
			{
				throw; // Re-throw validation exceptions
			}
			catch (Exception ex)
			{
				throw new ValidationException("DataBits", dataBits, 
					$"Error validating data bits: {ex.Message}", ex);
			}
		}

		public static StopBits ValidateStopBits(string stopBits)
		{
			try
			{
				// Validate stop bits input
				if (string.IsNullOrWhiteSpace(stopBits))
				{
					throw new ValidationException("StopBits", stopBits, "Stop bits cannot be null or empty");
				}

				if (!Enum.TryParse(stopBits, true, out StopBits parsedStopBits))
				{
					var validValues = string.Join(", ", Enum.GetNames(typeof(StopBits)));
					throw new ValidationException("StopBits", stopBits, 
						$"Invalid stop bits '{stopBits}'. Valid values: {validValues}");
				}
				return parsedStopBits;
			}
			catch (ValidationException)
			{
				throw; // Re-throw validation exceptions
			}
			catch (Exception ex)
			{
				throw new ValidationException("StopBits", stopBits, 
					$"Error validating stop bits: {ex.Message}", ex);
			}
		}

		public static Handshake ValidateHandshake(string handshake)
		{
			try
			{
				// Validate handshake input
				if (string.IsNullOrWhiteSpace(handshake))
				{
					throw new ValidationException("Handshake", handshake, "Handshake cannot be null or empty");
				}

				if (!Enum.TryParse(handshake, true, out Handshake parsedHandshake))
				{
					var validValues = string.Join(", ", Enum.GetNames(typeof(Handshake)));
					throw new ValidationException("Handshake", handshake, 
						$"Invalid handshake '{handshake}'. Valid values: {validValues}");
				}
				return parsedHandshake;
			}
			catch (ValidationException)
			{
				throw; // Re-throw validation exceptions
			}
			catch (Exception ex)
			{
				throw new ValidationException("Handshake", handshake, 
					$"Error validating handshake: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Validate and sanitize log directory with security checks
		/// </summary>
		public static string ValidateLogDirectory(string directory)
		{
			try
			{
				// Initialize security service
				SecurityService.Initialize();

				// Validate log directory input
				if (string.IsNullOrWhiteSpace(directory))
				{
					throw new ValidationException("LogDirectory", directory, "Log directory cannot be null or empty");
				}

				// Security sanitization
				var sanitizedDirectory = SecurityService.SanitizeFilePath(directory);

				// Additional validation
				if (sanitizedDirectory.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
				{
					throw new ValidationException("LogDirectory", sanitizedDirectory, 
						"Log directory contains invalid path characters");
				}

				return sanitizedDirectory;
			}
			catch (SecurityException ex)
			{
				throw new ValidationException("LogDirectory", directory, 
					$"Security validation failed: {ex.Message}", ex);
			}
			catch (ValidationException)
			{
				throw; // Re-throw validation exceptions
			}
			catch (Exception ex)
			{
				throw new ValidationException("LogDirectory", directory, 
					$"Error validating log directory: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Validate and sanitize log file name with security checks
		/// </summary>
		public static string ValidateLogFileName(string fileName)
		{
			try
			{
				// Initialize security service
				SecurityService.Initialize();

				// Validate log file name input
				if (string.IsNullOrWhiteSpace(fileName))
				{
					throw new ValidationException("LogFileName", fileName, "Log file name cannot be null or empty");
				}

				// Security sanitization
				var sanitizedFileName = SecurityService.SanitizeFileName(fileName);

				// Additional validation
				if (sanitizedFileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
				{
					throw new ValidationException("LogFileName", sanitizedFileName, 
						"Log file name contains invalid filename characters");
				}

				return sanitizedFileName;
			}
			catch (SecurityException ex)
			{
				throw new ValidationException("LogFileName", fileName, 
					$"Security validation failed: {ex.Message}", ex);
			}
			catch (ValidationException)
			{
				throw; // Re-throw validation exceptions
			}
			catch (Exception ex)
			{
				throw new ValidationException("LogFileName", fileName, 
					$"Error validating log file name: {ex.Message}", ex);
			}
		}
	}
}
