using System;

namespace COM_Port_Logger.Exceptions
{
    /// <summary>
    /// Base exception class for all COM Port Logger specific exceptions
    /// </summary>
    public abstract class COMPortLoggerException : Exception
    {
        public string ErrorCode { get; }
        public DateTime Timestamp { get; }

        protected COMPortLoggerException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
            Timestamp = DateTime.UtcNow;
        }

        protected COMPortLoggerException(string errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Exception thrown when serial port operations fail
    /// </summary>
    public class SerialPortException : COMPortLoggerException
    {
        public string PortName { get; }
        public int BaudRate { get; }

        public SerialPortException(string portName, int baudRate, string message) 
            : base("SERIAL_PORT_ERROR", message)
        {
            PortName = portName;
            BaudRate = baudRate;
        }

        public SerialPortException(string portName, int baudRate, string message, Exception innerException) 
            : base("SERIAL_PORT_ERROR", message, innerException)
        {
            PortName = portName;
            BaudRate = baudRate;
        }
    }

    /// <summary>
    /// Exception thrown when configuration operations fail
    /// </summary>
    public class ConfigurationException : COMPortLoggerException
    {
        public string ConfigFile { get; }
        public string ConfigSection { get; }

        public ConfigurationException(string configFile, string configSection, string message) 
            : base("CONFIG_ERROR", message)
        {
            ConfigFile = configFile;
            ConfigSection = configSection;
        }

        public ConfigurationException(string configFile, string configSection, string message, Exception innerException) 
            : base("CONFIG_ERROR", message, innerException)
        {
            ConfigFile = configFile;
            ConfigSection = configSection;
        }
    }

    /// <summary>
    /// Exception thrown when file operations fail
    /// </summary>
    public class FileOperationException : COMPortLoggerException
    {
        public string FilePath { get; }
        public string Operation { get; }

        public FileOperationException(string filePath, string operation, string message) 
            : base("FILE_OPERATION_ERROR", message)
        {
            FilePath = filePath;
            Operation = operation;
        }

        public FileOperationException(string filePath, string operation, string message, Exception innerException) 
            : base("FILE_OPERATION_ERROR", message, innerException)
        {
            FilePath = filePath;
            Operation = operation;
        }
    }

    /// <summary>
    /// Exception thrown when validation operations fail
    /// </summary>
    public class ValidationException : COMPortLoggerException
    {
        public string FieldName { get; }
        public object InvalidValue { get; }

        public ValidationException(string fieldName, object invalidValue, string message) 
            : base("VALIDATION_ERROR", message)
        {
            FieldName = fieldName;
            InvalidValue = invalidValue;
        }

        public ValidationException(string fieldName, object invalidValue, string message, Exception innerException) 
            : base("VALIDATION_ERROR", message, innerException)
        {
            FieldName = fieldName;
            InvalidValue = invalidValue;
        }
    }

    /// <summary>
    /// Exception thrown when connection operations fail
    /// </summary>
    public class ConnectionException : COMPortLoggerException
    {
        public string ConnectionType { get; }
        public int RetryCount { get; }

        public ConnectionException(string connectionType, int retryCount, string message) 
            : base("CONNECTION_ERROR", message)
        {
            ConnectionType = connectionType;
            RetryCount = retryCount;
        }

        public ConnectionException(string connectionType, int retryCount, string message, Exception innerException) 
            : base("CONNECTION_ERROR", message, innerException)
        {
            ConnectionType = connectionType;
            RetryCount = retryCount;
        }
    }
}
