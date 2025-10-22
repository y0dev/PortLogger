# Troubleshooting Guide

This guide helps you diagnose and resolve common issues with the COM Port Logger application.

## Table of Contents

- [Common Issues](#common-issues)
- [Serial Port Problems](#serial-port-problems)
- [Configuration Issues](#configuration-issues)
- [Logging Problems](#logging-problems)
- [Performance Issues](#performance-issues)
- [Debug Mode](#debug-mode)
- [Error Codes](#error-codes)

## Common Issues

### Application Won't Start

#### Issue: Missing .NET Framework
**Symptoms:**
- Application fails to start
- Error message about missing framework

**Solution:**
1. Install .NET Framework 4.6.1 or later
2. Download from Microsoft's website
3. Restart the application

#### Issue: Permission Denied
**Symptoms:**
- "Access denied" errors
- Cannot create log files

**Solution:**
1. Run as administrator
2. Choose a different log directory
3. Check folder permissions

### Command Line Arguments

#### Issue: Invalid Arguments
**Symptoms:**
- "Invalid number of arguments" error
- Application exits immediately

**Solution:**
- Use exactly 1 argument (configuration name) OR
- Use exactly 6 arguments (direct configuration)
- Check argument order and format

**Correct Usage:**
```bash
# Configuration mode
COM_Port_Logger.exe "MyConfig"

# Direct mode
COM_Port_Logger.exe "C:\logs" "my_log.txt" "COM3" 9600 "DarkMode" "My Logger"
```

## Serial Port Problems

### Port Access Denied

#### Issue: Port Already in Use
**Symptoms:**
```
Error: Access to the port 'COM1' is denied.
```

**Diagnosis:**
1. Check Device Manager for port status
2. Look for other applications using the port
3. Verify port is not locked by another process

**Solutions:**
1. **Close other applications** using the port
2. **Restart the port** in Device Manager:
   - Right-click on the port
   - Select "Disable device"
   - Wait 5 seconds
   - Right-click and select "Enable device"
3. **Use a different port** if available
4. **Run as administrator**

#### Issue: Port Not Found
**Symptoms:**
```
Error: Port COM5 is not available
```

**Diagnosis:**
1. Check Device Manager for available ports
2. Verify USB-to-Serial adapter is connected
3. Check device drivers

**Solutions:**
1. **Check Device Manager:**
   - Open Device Manager
   - Look under "Ports (COM & LPT)"
   - Note available port numbers
2. **Update drivers** for USB-to-Serial adapter
3. **Try different USB port**
4. **Use correct port number** in configuration

### Communication Issues

#### Issue: No Data Received
**Symptoms:**
- Application starts but no data appears
- Log files are empty

**Diagnosis:**
1. Check serial port settings
2. Verify device is sending data
3. Test with known working device

**Solutions:**
1. **Verify settings match device:**
   - Baud rate
   - Data bits
   - Stop bits
   - Parity
   - Handshake
2. **Test with terminal program** (PuTTY, Tera Term)
3. **Check device configuration**
4. **Try different baud rates**

#### Issue: Garbled Data
**Symptoms:**
- Data appears but is unreadable
- Random characters in log

**Diagnosis:**
1. Baud rate mismatch
2. Wrong data format
3. Electrical interference

**Solutions:**
1. **Check baud rate** - must match device exactly
2. **Verify data format:**
   - ASCII vs Binary
   - Line endings (CR, LF, CRLF)
3. **Check cable quality**
4. **Try different settings**

### Connection Drops

#### Issue: Intermittent Connection
**Symptoms:**
- Connection works initially then drops
- "Connection lost" messages

**Diagnosis:**
1. Power management issues
2. USB power saving
3. Driver problems

**Solutions:**
1. **Disable USB power saving:**
   - Device Manager → USB Controllers
   - Right-click USB Root Hub
   - Properties → Power Management
   - Uncheck "Allow computer to turn off this device"
2. **Update drivers**
3. **Use powered USB hub**
4. **Check cable connections**

## Configuration Issues

### Configuration File Problems

#### Issue: Configuration Not Found
**Symptoms:**
```
Error: No configuration found for console name: MyConfig
```

**Diagnosis:**
1. Check file exists in `configs/` directory
2. Verify file name and spelling
3. Check file format

**Solutions:**
1. **Verify file location:**
   ```
   configs/
   └── MyConfig.ini
   ```
2. **Check file name spelling**
3. **Verify INI format:**
   ```ini
   [SerialPort]
   PortName=COM1
   BaudRate=9600
   
   [Display]
   ColorScheme=DarkMode
   ConsoleName=MyConfig
   ```

#### Issue: Invalid Configuration Values
**Symptoms:**
```
Error: Invalid baud rate 50000
Error: Invalid port name COM99
```

**Diagnosis:**
1. Check parameter ranges
2. Verify enum values
3. Check syntax

**Solutions:**
1. **Use valid baud rates:** 110-256000
2. **Use available ports:** Check Device Manager
3. **Use correct enum values:**
   - Parity: None, Odd, Even, Mark, Space
   - StopBits: One, OnePointFive, Two
   - Handshake: None, XOnXOff, RequestToSend, RequestToSendXOnXOff

### Color Scheme Issues

#### Issue: Colors Not Applied
**Symptoms:**
- Console appears in default colors
- Color scheme not working

**Solutions:**
1. **Check color scheme name spelling**
2. **Use valid color scheme names**
3. **Restart application** after configuration change

## Logging Problems

### Log File Issues

#### Issue: Cannot Create Log File
**Symptoms:**
```
Error: Failed to create log file
```

**Diagnosis:**
1. Check directory permissions
2. Verify disk space
3. Check path validity

**Solutions:**
1. **Check directory permissions:**
   - Ensure write access to log directory
   - Run as administrator if needed
2. **Verify disk space:**
   - Ensure sufficient free space
   - Check disk for errors
3. **Use valid path:**
   - Avoid invalid characters
   - Use absolute paths if relative paths fail

#### Issue: Log Files Not Rotating
**Symptoms:**
- Log files grow very large
- Old files not deleted

**Diagnosis:**
1. Check rotation settings
2. Verify file permissions
3. Check disk space

**Solutions:**
1. **Configure rotation settings:**
   ```csharp
   var config = new LoggingConfiguration
   {
       MaxLogFileSizeMB = 10,
       MaxLogFiles = 5
   };
   ```
2. **Check file permissions**
3. **Ensure sufficient disk space**

### Log Level Issues

#### Issue: Too Much/Little Logging
**Symptoms:**
- Console flooded with messages
- Missing important information

**Solutions:**
1. **Adjust log level:**
   ```csharp
   // For production (less verbose)
   var config = LoggingConfigurationHelper.CreateProductionConfiguration();
   
   // For development (more verbose)
   var config = LoggingConfigurationHelper.CreateDevelopmentConfiguration();
   ```
2. **Configure specific levels:**
   ```csharp
   config.MinimumLevel = LogLevel.Warning; // Only warnings and errors
   ```

## Performance Issues

### High CPU Usage

#### Issue: Application Uses Too Much CPU
**Symptoms:**
- High CPU usage
- System becomes slow

**Diagnosis:**
1. Check logging frequency
2. Verify thread usage
3. Check for infinite loops

**Solutions:**
1. **Reduce logging frequency:**
   - Increase log level threshold
   - Disable console output in production
2. **Check thread configuration**
3. **Monitor resource usage**

### Memory Issues

#### Issue: High Memory Usage
**Symptoms:**
- Memory usage grows over time
- Application becomes slow

**Diagnosis:**
1. Check for memory leaks
2. Verify log file rotation
3. Check buffer sizes

**Solutions:**
1. **Enable log rotation**
2. **Monitor memory usage**
3. **Restart application periodically**

## Debug Mode

### Enabling Debug Mode

Enable debug logging for detailed troubleshooting:

```csharp
Log.Initialize(LoggingConfigurationHelper.CreateDevelopmentConfiguration());
```

### Debug Information

Debug mode provides:
- Detailed configuration loading
- Parameter validation results
- Thread activity
- Resource usage
- Error context

### Debug Output Example

```
[DEBUG] [PortLog] Starting COM Port Logger with console name: MyConfig
[DEBUG] [LoadConfig] Loading configuration from: configs\MyConfig.ini
[DEBUG] [ValidateCOMPort] Checking if we can connect to port COM1
[DEBUG] [ValidateCOMPort] Port COM1 is available
[INFO] [SerialPort] Serial port Open: Port opened successfully
[INFO] [FileOperation] File Create: Log file created successfully
[INFO] [PortLog] COM Port Logger started successfully
```

## Error Codes

### Serial Port Errors

| Error Code | Description | Solution |
|------------|-------------|----------|
| `SERIAL_PORT_ERROR` | General serial port error | Check port settings and availability |
| `CONNECTION_ERROR` | Connection lost or failed | Check cable and device connection |

### Configuration Errors

| Error Code | Description | Solution |
|------------|-------------|----------|
| `CONFIG_ERROR` | Configuration file error | Check file format and location |
| `VALIDATION_ERROR` | Parameter validation failed | Check parameter values and ranges |

### File Operation Errors

| Error Code | Description | Solution |
|------------|-------------|----------|
| `FILE_OPERATION_ERROR` | File I/O error | Check permissions and disk space |

## Getting Help

### Before Asking for Help

1. **Enable debug mode** and collect logs
2. **Check this troubleshooting guide**
3. **Verify system requirements**
4. **Test with minimal configuration**

### Information to Provide

When reporting issues, include:
1. **Error messages** (exact text)
2. **Configuration file** (if using file-based config)
3. **Command line** (if using direct config)
4. **System information:**
   - Windows version
   - .NET Framework version
   - Available COM ports
5. **Debug logs** (if debug mode enabled)

### Contact Information

- **GitHub Issues:** [Create an issue](../../issues)
- **Documentation:** Check this guide and API documentation
- **Examples:** See sample configurations in the repository

---

This troubleshooting guide covers the most common issues encountered with the COM Port Logger application. For additional help, refer to the API documentation or create an issue on GitHub with detailed information about your problem.
