# API Documentation

This document provides detailed API documentation for the COM Port Logger application.

## Table of Contents

- [Core Classes](#core-classes)
- [Logging System](#logging-system)
- [Error Handling](#error-handling)
- [Configuration](#configuration)
- [Services](#services)

## Core Classes

### PortLog Class

The main application class that handles serial port communication and logging.

#### Methods

##### `Start(string consoleName)`
Starts the COM port logger using a configuration file.

**Parameters:**
- `consoleName` (string): Name of the configuration to load from the `configs/` directory

**Throws:**
- `ConfigurationException`: When configuration cannot be loaded
- `SerialPortException`: When serial port cannot be opened
- `FileOperationException`: When log file cannot be created

**Example:**
```csharp
PortLog.Start("MyLogger");
```

##### `Start(string baseDirectory, string logFileName, string comPort, int baudRate, string colorSchemeName, string consoleTitle)`
Starts the COM port logger with direct configuration parameters.

**Parameters:**
- `baseDirectory` (string): Directory where log files will be stored
- `logFileName` (string): Name of the log file
- `comPort` (string): Serial port name (e.g., COM1, COM3)
- `baudRate` (int): Baud rate (110-256000)
- `colorSchemeName` (string): Color scheme name
- `consoleTitle` (string): Console window title

**Throws:**
- `ValidationException`: When parameters are invalid
- `SerialPortException`: When serial port cannot be opened
- `FileOperationException`: When log file cannot be created

**Example:**
```csharp
PortLog.Start("C:\\logs", "my_log.txt", "COM3", 9600, "DarkMode", "My Logger");
```

## Logging System

### Logger Class

Core logging functionality with background processing and multiple output formats.

#### Properties

##### `LogLevel`
Enumeration of available log levels:
- `Trace` (0): Most verbose logging
- `Debug` (1): Debug information
- `Info` (2): General information
- `Warning` (3): Warning messages
- `Error` (4): Error messages
- `Critical` (5): Critical errors

#### Methods

##### `Log(LogLevel level, string message, string context = null, string source = null, Exception exception = null, Dictionary<string, object> properties = null)`
Logs a message with the specified level and context.

**Parameters:**
- `level` (LogLevel): Log level
- `message` (string): Log message
- `context` (string, optional): Additional context
- `source` (string, optional): Source of the log message
- `exception` (Exception, optional): Associated exception
- `properties` (Dictionary<string, object>, optional): Additional properties

**Example:**
```csharp
var properties = new Dictionary<string, object>
{
    ["PortName"] = "COM1",
    ["BaudRate"] = 9600
};
logger.Log(LogLevel.Info, "Serial port opened", "SerialPort", "PortLog", null, properties);
```

##### `Trace(string message, string context = null, string source = null, Dictionary<string, object> properties = null)`
Logs a trace message.

##### `Debug(string message, string context = null, string source = null, Dictionary<string, object> properties = null)`
Logs a debug message.

##### `Info(string message, string context = null, string source = null, Dictionary<string, object> properties = null)`
Logs an info message.

##### `Warning(string message, string context = null, string source = null, Exception exception = null, Dictionary<string, object> properties = null)`
Logs a warning message.

##### `Error(string message, string context = null, string source = null, Exception exception = null, Dictionary<string, object> properties = null)`
Logs an error message.

##### `Critical(string message, string context = null, string source = null, Exception exception = null, Dictionary<string, object> properties = null)`
Logs a critical message.

### Log Class (Static)

Static logger instance for easy access throughout the application.

#### Methods

##### `Initialize(LoggingConfiguration config = null)`
Initializes the logging system with the specified configuration.

**Parameters:**
- `config` (LoggingConfiguration, optional): Logging configuration. If null, uses default configuration.

**Example:**
```csharp
var config = LoggingConfigurationHelper.CreateDefaultConfiguration();
Log.Initialize(config);
```

##### `Dispose()`
Disposes the logging system and flushes all pending logs.

### LoggingConfiguration Class

Configuration class for the logging system.

#### Properties

- `LogDirectory` (string): Directory where log files are stored
- `MinimumLevel` (LogLevel): Minimum log level to process
- `EnableConsoleOutput` (bool): Whether to enable console output
- `EnableFileOutput` (bool): Whether to enable file output
- `MaxLogFileSizeMB` (int): Maximum log file size in MB
- `MaxLogFiles` (int): Maximum number of log files to keep
- `LogFileNameFormat` (string): Format string for log file names
- `IncludeStackTrace` (bool): Whether to include stack traces
- `IncludeProperties` (bool): Whether to include properties in logs

### LoggingConfigurationHelper Class

Helper class for creating common logging configurations.

#### Methods

##### `CreateDefaultConfiguration()`
Creates a default logging configuration suitable for general use.

**Returns:** `LoggingConfiguration`

##### `CreateDevelopmentConfiguration()`
Creates a development logging configuration with verbose output.

**Returns:** `LoggingConfiguration`

##### `CreateProductionConfiguration()`
Creates a production logging configuration with minimal console output.

**Returns:** `LoggingConfiguration`

### StructuredLogging Class

Helper class for structured logging of common operations.

#### Methods

##### `LogSerialPortOperation(string operation, string portName, int baudRate, bool success, string message = null, Exception exception = null)`
Logs serial port operations with structured data.

**Parameters:**
- `operation` (string): Operation name (e.g., "Open", "Close", "Read", "Write")
- `portName` (string): Serial port name
- `baudRate` (int): Baud rate
- `success` (bool): Whether the operation succeeded
- `message` (string, optional): Additional message
- `exception` (Exception, optional): Associated exception

##### `LogFileOperation(string operation, string filePath, bool success, long? fileSize = null, string message = null, Exception exception = null)`
Logs file operations with structured data.

##### `LogConfigurationLoad(string configFile, bool success, string message = null, Exception exception = null)`
Logs configuration loading operations.

##### `LogConnectionEvent(string eventType, string connectionType, int retryCount = 0, string message = null, Exception exception = null)`
Logs connection events with structured data.

## Error Handling

### COMPortLoggerException Class

Base exception class for all COM Port Logger specific exceptions.

#### Properties

- `ErrorCode` (string): Unique error code
- `Timestamp` (DateTime): When the exception occurred

### SerialPortException Class

Exception thrown when serial port operations fail.

#### Properties

- `PortName` (string): Serial port name
- `BaudRate` (int): Baud rate
- Inherits from `COMPortLoggerException`

### ConfigurationException Class

Exception thrown when configuration operations fail.

#### Properties

- `ConfigFile` (string): Configuration file path
- `ConfigSection` (string): Configuration section
- Inherits from `COMPortLoggerException`

### FileOperationException Class

Exception thrown when file operations fail.

#### Properties

- `FilePath` (string): File path
- `Operation` (string): Operation name
- Inherits from `COMPortLoggerException`

### ValidationException Class

Exception thrown when validation operations fail.

#### Properties

- `FieldName` (string): Field name that failed validation
- `InvalidValue` (object): Invalid value
- Inherits from `COMPortLoggerException`

### ConnectionException Class

Exception thrown when connection operations fail.

#### Properties

- `ConnectionType` (string): Type of connection
- `RetryCount` (int): Number of retry attempts
- Inherits from `COMPortLoggerException`

## Configuration

### ConfigSettings Class

Main configuration settings class.

#### Properties

- `SerialPort` (SerialPortConfig): Serial port configuration
- `LogFile` (LogFileSettings): Log file configuration
- `Display` (DisplaySettings): Display configuration

### SerialPortConfig Class

Serial port configuration settings.

#### Properties

- `PortName` (string): Serial port name
- `BaudRate` (int): Baud rate
- `Parity` (string): Parity setting
- `DataBits` (int): Number of data bits
- `StopBits` (string): Stop bits setting
- `Handshake` (string): Handshake protocol

### LogFileSettings Class

Log file configuration settings.

#### Properties

- `BaseDirectory` (string): Base directory for log files
- `FileName` (string): Log file name

### DisplaySettings Class

Display configuration settings.

#### Properties

- `ConsoleName` (string): Console window title
- `ColorScheme` (string): Color scheme name
- `FontSize` (ushort): Font size

### ColorScheme Class

Color scheme management for console output.

#### Static Properties

- `Default`: Default color scheme
- `DarkMode`: Dark mode color scheme
- `LightMode`: Light mode color scheme
- `SolarizedDark`: Solarized dark theme
- `SolarizedLight`: Solarized light theme
- `Monokai`: Monokai color scheme
- `GruvboxDark`: Gruvbox dark theme
- `GruvboxLight`: Gruvbox light theme
- `Nord`: Nord color scheme
- `Ocean`: Ocean blue theme
- `Desert`: Desert theme
- `Retro`: Retro green terminal theme
- `Cyberpunk`: Cyberpunk magenta theme
- `Twilight`: Twilight purple theme
- `Forest`: Forest green theme
- `Sunset`: Sunset orange theme

#### Methods

##### `GetColorScheme(string schemeName)`
Gets a color scheme by name.

**Parameters:**
- `schemeName` (string): Name of the color scheme

**Returns:** `ColorScheme`

## Services

### ErrorHandler Class

Centralized error handling service.

#### Methods

##### `Initialize()`
Initializes the error handler.

##### `HandleException(Exception ex, string context = "", bool showToUser = true)`
Handles generic exceptions.

##### `HandleCOMPortLoggerException(COMPortLoggerException ex, string context = "", bool showToUser = true)`
Handles COM Port Logger specific exceptions.

##### `HandleSerialPortException(SerialPortException ex, string context = "", bool showToUser = true)`
Handles serial port exceptions.

##### `HandleConfigurationException(ConfigurationException ex, string context = "", bool showToUser = true)`
Handles configuration exceptions.

##### `HandleFileOperationException(FileOperationException ex, string context = "", bool showToUser = true)`
Handles file operation exceptions.

##### `HandleConnectionException(ConnectionException ex, string context = "", bool showToUser = true)`
Handles connection exceptions.

##### `LogWarning(string message, string context = "", bool showToUser = true)`
Logs a warning message.

##### `LogInfo(string message, string context = "", bool showToUser = false)`
Logs an info message.

### FileHandler Class

File operations management service.

#### Methods

##### `CreateLogFile(string baseDirectory, string filename)`
Creates a log file with automatic directory structure.

**Parameters:**
- `baseDirectory` (string): Base directory for the log file
- `filename` (string): Log file name

**Returns:** `LogFileResult`

**Throws:**
- `ValidationException`: When parameters are invalid
- `FileOperationException`: When file creation fails

##### `SearchConfigFiles(string directoryPath)`
Searches for configuration files in the specified directory.

**Parameters:**
- `directoryPath` (string): Directory to search

**Returns:** `List<string>` of configuration file paths

**Throws:**
- `ValidationException`: When directory path is invalid
- `FileOperationException`: When directory doesn't exist

### InputValidator Class

Input validation and sanitization service.

#### Methods

##### `ValidatePortName(string portName)`
Validates serial port name.

**Parameters:**
- `portName` (string): Port name to validate

**Returns:** `string` (validated port name)

**Throws:**
- `ValidationException`: When port name is invalid

##### `ValidateBaudRate(int baudRate)`
Validates baud rate.

**Parameters:**
- `baudRate` (int): Baud rate to validate

**Returns:** `int` (validated baud rate)

**Throws:**
- `ValidationException`: When baud rate is invalid

##### `ValidateParity(string parity)`
Validates parity setting.

**Parameters:**
- `parity` (string): Parity setting to validate

**Returns:** `Parity` (validated parity)

**Throws:**
- `ValidationException`: When parity is invalid

##### `ValidateDataBits(int dataBits)`
Validates data bits setting.

**Parameters:**
- `dataBits` (int): Data bits to validate

**Returns:** `int` (validated data bits)

**Throws:**
- `ValidationException`: When data bits are invalid

##### `ValidateStopBits(string stopBits)`
Validates stop bits setting.

**Parameters:**
- `stopBits` (string): Stop bits to validate

**Returns:** `StopBits` (validated stop bits)

**Throws:**
- `ValidationException`: When stop bits are invalid

##### `ValidateHandshake(string handshake)`
Validates handshake setting.

**Parameters:**
- `handshake` (string): Handshake to validate

**Returns:** `Handshake` (validated handshake)

**Throws:**
- `ValidationException`: When handshake is invalid

##### `ValidateLogDirectory(string directory)`
Validates log directory path.

**Parameters:**
- `directory` (string): Directory path to validate

**Returns:** `string` (validated directory path)

**Throws:**
- `ValidationException`: When directory path is invalid

##### `ValidateLogFileName(string fileName)`
Validates log file name.

**Parameters:**
- `fileName` (string): File name to validate

**Returns:** `string` (validated file name)

**Throws:**
- `ValidationException`: When file name is invalid

### ConsoleHandler Class

Console manipulation utilities.

#### Methods

##### `SetConsoleFontSize(ushort fontSizeY)`
Sets the console font size.

**Parameters:**
- `fontSizeY` (ushort): Font size in pixels

**Note:** This method uses Windows API calls and may not work on all systems.

## Usage Examples

### Basic Logging
```csharp
// Initialize logging
Log.Initialize(LoggingConfigurationHelper.CreateDefaultConfiguration());

// Log different levels
Log.Info("Application started", "Main");
Log.Warning("Configuration file not found", "Config");
Log.Error("Failed to open serial port", "SerialPort", exception);
```

### Structured Logging
```csharp
// Log serial port operation
StructuredLogging.LogSerialPortOperation("Open", "COM1", 9600, true, "Port opened successfully");

// Log file operation
StructuredLogging.LogFileOperation("Create", "C:\\logs\\app.log", true, 1024, "Log file created");

// Log connection event
StructuredLogging.LogConnectionEvent("connected", "SerialPort", 0, "Connection established");
```

### Error Handling
```csharp
try
{
    // Some operation that might fail
    serialPort.Open();
}
catch (Exception ex)
{
    ErrorHandler.HandleSerialPortException(
        new SerialPortException("COM1", 9600, "Failed to open port", ex),
        "Port initialization"
    );
}
```

### Configuration
```csharp
// Load configuration
var config = LoadConfig("MyLogger");

// Use configuration
var portName = config.SerialPort.PortName;
var baudRate = config.SerialPort.BaudRate;
var colorScheme = ColorScheme.GetColorScheme(config.Display.ColorScheme);
```

---

This API documentation provides comprehensive information about all public classes, methods, and properties in the COM Port Logger application. For more detailed examples and usage patterns, refer to the main README.md file.
