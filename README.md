# COM Port Logger

A robust, enterprise-grade COM port logging application built with C# .NET Framework 4.6.1. This application provides comprehensive serial port communication logging with advanced error handling, structured logging, and flexible configuration options.

## 🚀 Features

### Core Functionality
- **Serial Port Communication**: Full duplex communication with configurable serial port settings
- **Real-time Logging**: Live logging of all serial port data with timestamps
- **Dual Interface**: Both console application and WPF GUI versions available
- **Configuration Management**: Flexible INI-based configuration system
- **Multiple Color Schemes**: 15+ built-in color schemes for console customization

### Advanced Features
- **Enterprise Error Handling**: Comprehensive exception handling with custom exception types
- **Structured Logging**: Multi-level logging system (Trace, Debug, Info, Warning, Error, Critical)
- **Log Rotation**: Automatic log file rotation with size and count limits
- **Connection Monitoring**: Automatic reconnection with retry logic
- **Performance Monitoring**: Built-in performance logging capabilities
- **Resource Management**: Proper cleanup and disposal of resources

### Logging Capabilities
- **Console Output**: Color-coded console logging with configurable levels
- **File Logging**: Structured file logging with automatic rotation
- **Context-Aware**: Rich context information for better debugging
- **Exception Tracking**: Detailed exception logging with stack traces
- **Performance Metrics**: Operation timing and performance monitoring

## 📋 Requirements

- **.NET Framework 4.6.1** or later
- **Windows Operating System** (for serial port access)
- **Visual Studio 2017** or later (for development)

## 🛠️ Installation

### Pre-built Executable
1. Download the latest release from the [Releases](../../releases) page
2. Extract the files to your desired directory
3. Run `COM_Port_Logger.exe` from command line or `PortLogger.exe` for GUI version

### Building from Source
1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/COM_Port_Logger.git
   cd COM_Port_Logger
   ```

2. Open the solution in Visual Studio:
   ```bash
   # For console version
   start COM_PortLogger/COM_Port_Logger.sln
   
   # For GUI version
   start GUI_PortLogger/PortLogger.sln
   ```

3. Build the solution:
   - Press `Ctrl+Shift+B` in Visual Studio, or
   - Use command line: `dotnet build`

## 🎯 Usage

### Console Application

#### Basic Usage
```bash
# Using configuration file
COM_Port_Logger.exe "MyConsole"

# Using command line parameters
COM_Port_Logger.exe "C:\logs" "my_log.txt" "COM3" 9600 "DarkMode" "My Logger"
```

#### Command Line Parameters
1. **Configuration Mode** (1 parameter):
   - `consoleName`: Name of the configuration to load from `configs/` directory

2. **Direct Mode** (6 parameters):
   - `baseDirectory`: Directory where log files will be stored
   - `logFileName`: Name of the log file
   - `comPort`: Serial port name (e.g., COM1, COM3)
   - `baudRate`: Baud rate (110-256000)
   - `colorSchemeName`: Color scheme name
   - `consoleTitle`: Console window title

#### Interactive Commands
- Type any message and press Enter to send to the serial port
- Type `QUIT` and press Enter to exit the application

### GUI Application
1. Run `PortLogger.exe`
2. Configure serial port settings in the GUI
3. Click "Start Logging" to begin
4. Use the interface to send commands and monitor data

## ⚙️ Configuration

### Configuration Files
Configuration files are stored in the `configs/` directory and use INI format:

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

### Available Color Schemes
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

### Logging Configuration
The application supports extensive logging configuration:

```csharp
var config = new LoggingConfiguration
{
    LogDirectory = "logs",
    MinimumLevel = LogLevel.Info,
    EnableConsoleOutput = true,
    EnableFileOutput = true,
    MaxLogFileSizeMB = 10,
    MaxLogFiles = 10,
    IncludeStackTrace = true,
    IncludeProperties = true
};
```

## 📁 Project Structure

```
COM_Port_Logger/
├── COM_PortLogger/                 # Console application
│   └── COM_Port_Logger/
│       ├── ConfigurationSettings/   # Configuration classes
│       ├── Exceptions/             # Custom exception types
│       ├── Logging/                # Logging system
│       ├── Services/               # Service classes
│       ├── PortLog.cs              # Main application logic
│       ├── Program.cs              # Entry point
│       └── config.ini              # Sample configuration
├── GUI_PortLogger/                 # WPF GUI application
│   └── PortLogger/
│       ├── Resources/              # GUI resources
│       ├── Utilities/              # Utility classes
│       └── MainWindow.xaml         # Main GUI window
└── README.md                       # This file
```

## 🔧 Architecture

### Core Components

#### PortLog Class
- Main application logic
- Serial port management
- Thread coordination
- Resource cleanup

#### Logging System
- **Logger**: Core logging functionality
- **LogEntry**: Structured log entry representation
- **ILogWriter**: Extensible logging output interface
- **ConsoleLogWriter**: Color-coded console output
- **FileLogWriter**: File-based logging with rotation

#### Error Handling
- **COMPortLoggerException**: Base exception class
- **SerialPortException**: Serial port specific errors
- **ConfigurationException**: Configuration errors
- **FileOperationException**: File I/O errors
- **ValidationException**: Input validation errors
- **ConnectionException**: Connection related errors

#### Services
- **ErrorHandler**: Centralized error handling
- **FileHandler**: File operations management
- **InputValidator**: Input validation and sanitization
- **ConsoleHandler**: Console manipulation utilities

## 🐛 Troubleshooting

### Common Issues

#### Serial Port Access Denied
```
Error: Access to the port 'COM1' is denied.
```
**Solution**: 
- Ensure no other application is using the port
- Run as administrator if necessary
- Check device manager for port conflicts

#### Configuration Not Found
```
Error: No configuration found for console name: MyConsole
```
**Solution**:
- Verify configuration file exists in `configs/` directory
- Check console name spelling in configuration file
- Ensure configuration file has proper INI format

#### Log File Permission Issues
```
Error: Failed to create log file
```
**Solution**:
- Check write permissions for log directory
- Ensure sufficient disk space
- Verify directory path is valid

### Debug Mode
Enable debug logging for detailed troubleshooting:

```csharp
Log.Initialize(LoggingConfigurationHelper.CreateDevelopmentConfiguration());
```

This will:
- Enable Debug level logging
- Show more verbose console output
- Include additional context information

## 🤝 Contributing

We welcome contributions! Please follow these guidelines:

### Development Setup
1. Fork the repository
2. Create a feature branch: `git checkout -b feature-name`
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

### Code Style
- Follow C# naming conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Include error handling for all external operations

### Testing
- Add unit tests for new functionality
- Test error scenarios
- Verify logging output
- Test configuration changes

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with .NET Framework 4.6.1
- Uses System.IO.Ports for serial communication
- Inspired by the need for reliable COM port logging tools

## 📞 Support

For support, please:
1. Check the [Issues](../../issues) page for known problems
2. Create a new issue with detailed information
3. Include log files and configuration details
4. Describe steps to reproduce the problem

## 🔄 Changelog

### Version 2.0.0 (Current)
- ✅ Comprehensive error handling system
- ✅ Advanced logging with multiple levels
- ✅ Log rotation and management
- ✅ Structured logging capabilities
- ✅ Performance monitoring
- ✅ Enhanced configuration management

### Version 1.0.0
- ✅ Basic serial port communication
- ✅ Simple file logging
- ✅ Configuration file support
- ✅ Multiple color schemes
- ✅ Console and GUI versions

---

**Made with ❤️ for reliable serial port communication**
