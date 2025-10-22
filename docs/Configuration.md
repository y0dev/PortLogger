# Configuration Guide

This guide provides detailed information about configuring the COM Port Logger application.

## Table of Contents

- [Configuration Files](#configuration-files)
- [Serial Port Settings](#serial-port-settings)
- [Log File Settings](#log-file-settings)
- [Display Settings](#display-settings)
- [Logging Configuration](#logging-configuration)
- [Advanced Configuration](#advanced-configuration)

## Configuration Files

### File Location
Configuration files are stored in the `configs/` directory relative to the application executable.

### File Format
Configuration files use the INI format with sections and key-value pairs:

```ini
[SectionName]
Key=Value
; This is a comment
```

### Sample Configuration File
```ini
[SerialPort]
PortName=COM1
BaudRate=9600
Parity=None
DataBits=8
StopBits=One
Handshake=None

[LogFile]
BaseDirectory=/log/

[Display]
ColorScheme=DarkMode
ConsoleName=COM Port Logger
```

## Serial Port Settings

### PortName
Specifies the serial port to use.

**Valid Values:**
- COM1, COM2, COM3, etc.
- Must be an available port on the system

**Example:**
```ini
PortName=COM3
```

### BaudRate
Specifies the communication speed in bits per second.

**Valid Range:** 110 - 256000

**Common Values:**
- 9600 (default)
- 19200
- 38400
- 57600
- 115200
- 230400
- 460800
- 921600

**Example:**
```ini
BaudRate=115200
```

### Parity
Specifies the parity checking method.

**Valid Values:**
- `None` (default)
- `Odd`
- `Even`
- `Mark`
- `Space`

**Example:**
```ini
Parity=None
```

### DataBits
Specifies the number of data bits per byte.

**Valid Range:** 5 - 8

**Common Values:**
- 7 (for ASCII)
- 8 (default, for binary data)

**Example:**
```ini
DataBits=8
```

### StopBits
Specifies the number of stop bits.

**Valid Values:**
- `One` (default)
- `OnePointFive`
- `Two`

**Example:**
```ini
StopBits=One
```

### Handshake
Specifies the handshake protocol.

**Valid Values:**
- `None` (default)
- `XOnXOff`
- `RequestToSend`
- `RequestToSendXOnXOff`

**Example:**
```ini
Handshake=None
```

## Log File Settings

### BaseDirectory
Specifies the base directory where log files will be stored.

**Notes:**
- Can be absolute or relative path
- Directory will be created if it doesn't exist
- Supports environment variables (e.g., `%TEMP%`)

**Examples:**
```ini
BaseDirectory=C:\Logs
BaseDirectory=./logs
BaseDirectory=%TEMP%\COM_Logger
```

### FileName
Specifies the base name for log files.

**Notes:**
- Extension will be automatically added
- Timestamp will be prepended to filename
- Directory structure will be created based on date

**Example:**
```ini
FileName=serial_communication
```

**Result:** `2024-10-21_14-30-25_serial_communication.txt`

## Display Settings

### ColorScheme
Specifies the color scheme for console output.

**Available Schemes:**
- `Default` - Standard black background with white text
- `DarkMode` - Dark theme with gray text and cyan numbers
- `LightMode` - Light theme with white background
- `SolarizedDark` - Solarized dark theme
- `SolarizedLight` - Solarized light theme
- `Monokai` - Monokai color scheme
- `GruvboxDark` - Gruvbox dark theme
- `GruvboxLight` - Gruvbox light theme
- `Nord` - Nord color scheme
- `Ocean` - Ocean blue theme
- `Desert` - Desert theme
- `Retro` - Retro green terminal theme
- `Cyberpunk` - Cyberpunk magenta theme
- `Twilight` - Twilight purple theme
- `Forest` - Forest green theme
- `Sunset` - Sunset orange theme

**Example:**
```ini
ColorScheme=DarkMode
```

### ConsoleName
Specifies the title for the console window.

**Example:**
```ini
ConsoleName=My Serial Logger
```

## Logging Configuration

### Programmatic Configuration
The logging system can be configured programmatically using the `LoggingConfiguration` class:

```csharp
var config = new LoggingConfiguration
{
    LogDirectory = "logs",
    MinimumLevel = LogLevel.Info,
    EnableConsoleOutput = true,
    EnableFileOutput = true,
    EnableStructuredLogging = true,
    MaxLogFileSizeMB = 10,
    MaxLogFiles = 10,
    LogFileNameFormat = "com-port-logger-{0:yyyy-MM-dd}.log",
    IncludeStackTrace = true,
    IncludeProperties = true
};

Log.Initialize(config);
```

### Configuration Properties

#### LogDirectory
Directory where log files will be stored.

**Default:** `"logs"`

#### MinimumLevel
Minimum log level to process.

**Valid Values:**
- `Trace` (0) - Most verbose
- `Debug` (1)
- `Info` (2) - Default
- `Warning` (3)
- `Error` (4)
- `Critical` (5) - Least verbose

#### EnableConsoleOutput
Whether to enable console output.

**Default:** `true`

#### EnableFileOutput
Whether to enable file output.

**Default:** `true`

#### MaxLogFileSizeMB
Maximum log file size in megabytes before rotation.

**Default:** `10`

#### MaxLogFiles
Maximum number of log files to keep.

**Default:** `10`

#### LogFileNameFormat
Format string for log file names. Uses DateTime formatting.

**Default:** `"com-port-logger-{0:yyyy-MM-dd}.log"`

**Examples:**
- `"app-{0:yyyy-MM-dd}.log"` → `app-2024-10-21.log`
- `"logger-{0:yyyy-MM-dd_HH-mm}.log"` → `logger-2024-10-21_14-30.log`

#### IncludeStackTrace
Whether to include stack traces in log entries.

**Default:** `true`

#### IncludeProperties
Whether to include additional properties in log entries.

**Default:** `true`

## Advanced Configuration

### Multiple Configuration Files
You can create multiple configuration files for different scenarios:

```
configs/
├── development.ini
├── production.ini
├── testing.ini
└── default.ini
```

### Environment-Specific Configuration
Use different configurations for different environments:

```bash
# Development
COM_Port_Logger.exe "development"

# Production
COM_Port_Logger.exe "production"

# Testing
COM_Port_Logger.exe "testing"
```

### Configuration Validation
The application validates all configuration values:

- **Port Names:** Must be available on the system
- **Baud Rates:** Must be within valid range (110-256000)
- **Parity:** Must be valid enum value
- **Data Bits:** Must be between 5-8
- **Stop Bits:** Must be valid enum value
- **Handshake:** Must be valid enum value
- **Directories:** Must be valid paths
- **File Names:** Must not contain invalid characters

### Error Handling
If configuration validation fails, the application will:

1. Log the validation error
2. Use default values where possible
3. Throw a `ConfigurationException` for critical errors
4. Display helpful error messages to the user

### Configuration Examples

#### High-Speed Communication
```ini
[SerialPort]
PortName=COM3
BaudRate=115200
Parity=None
DataBits=8
StopBits=One
Handshake=None

[LogFile]
BaseDirectory=C:\HighSpeedLogs

[Display]
ColorScheme=Cyberpunk
ConsoleName=High Speed Logger
```

#### Reliable Communication
```ini
[SerialPort]
PortName=COM1
BaudRate=9600
Parity=Even
DataBits=7
StopBits=Two
Handshake=RequestToSend

[LogFile]
BaseDirectory=C:\ReliableLogs

[Display]
ColorScheme=Default
ConsoleName=Reliable Logger
```

#### Development Configuration
```ini
[SerialPort]
PortName=COM2
BaudRate=38400
Parity=None
DataBits=8
StopBits=One
Handshake=None

[LogFile]
BaseDirectory=./dev_logs

[Display]
ColorScheme=DarkMode
ConsoleName=Development Logger
```

#### Production Configuration
```ini
[SerialPort]
PortName=COM4
BaudRate=57600
Parity=None
DataBits=8
StopBits=One
Handshake=XOnXOff

[LogFile]
BaseDirectory=/var/log/com_logger

[Display]
ColorScheme=Default
ConsoleName=Production Logger
```

## Troubleshooting Configuration

### Common Issues

#### Port Not Available
```
Error: Port COM1 is not available
```
**Solution:**
- Check Device Manager for available ports
- Ensure no other application is using the port
- Try a different port number

#### Invalid Baud Rate
```
Error: Invalid baud rate 50000
```
**Solution:**
- Use a standard baud rate (9600, 19200, 38400, 57600, 115200)
- Ensure the value is within the valid range (110-256000)

#### Directory Access Denied
```
Error: Cannot create directory C:\Logs
```
**Solution:**
- Run as administrator
- Choose a different directory
- Check directory permissions

#### Configuration File Not Found
```
Error: No configuration found for console name: MyConfig
```
**Solution:**
- Verify the configuration file exists in `configs/` directory
- Check the console name spelling
- Ensure the file has proper INI format

### Debug Configuration
Enable debug logging to troubleshoot configuration issues:

```csharp
Log.Initialize(LoggingConfigurationHelper.CreateDevelopmentConfiguration());
```

This will provide detailed information about:
- Configuration file loading
- Parameter validation
- Default value usage
- Error conditions

### Configuration Testing
Test your configuration before using it in production:

1. Create a test configuration file
2. Use a test serial port or simulator
3. Enable debug logging
4. Verify all settings work as expected
5. Check log file creation and rotation

---

This configuration guide provides comprehensive information about all configuration options available in the COM Port Logger application. For more information about specific features, refer to the API documentation or main README file.
