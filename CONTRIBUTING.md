# Contributing Guide

Thank you for your interest in contributing to the COM Port Logger project! This guide will help you get started with contributing to the project.

## Table of Contents

- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Code Style Guidelines](#code-style-guidelines)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Issue Reporting](#issue-reporting)

## Getting Started

### Prerequisites

- **Visual Studio 2017** or later (or Visual Studio Code)
- **.NET Framework 4.6.1** or later
- **Git** for version control
- **Windows Operating System** (for serial port development)

### Fork and Clone

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/yourusername/COM_Port_Logger.git
   cd COM_Port_Logger
   ```
3. **Add upstream remote**:
   ```bash
   git remote add upstream https://github.com/originalowner/COM_Port_Logger.git
   ```

## Development Setup

### Project Structure

```
COM_Port_Logger/
├── COM_PortLogger/                 # Console application
│   └── COM_Port_Logger/
│       ├── ConfigurationSettings/   # Configuration classes
│       ├── Exceptions/             # Custom exception types
│       ├── Logging/                # Logging system
│       ├── Services/               # Service classes
│       ├── PortLog.cs              # Main application logic
│       └── Program.cs              # Entry point
├── GUI_PortLogger/                 # WPF GUI application
│   └── PortLogger/
│       ├── Resources/              # GUI resources
│       ├── Utilities/              # Utility classes
│       └── MainWindow.xaml         # Main GUI window
├── docs/                           # Documentation
├── README.md                       # Main documentation
└── CHANGELOG.md                    # Version history
```

### Building the Project

1. **Open the solution** in Visual Studio:
   ```bash
   # Console version
   start COM_PortLogger/COM_Port_Logger.sln
   
   # GUI version
   start GUI_PortLogger/PortLogger.sln
   ```

2. **Build the solution**:
   - Press `Ctrl+Shift+B` in Visual Studio
   - Or use command line: `dotnet build`

3. **Run tests** (when available):
   - Press `Ctrl+R, A` in Visual Studio
   - Or use command line: `dotnet test`

## Code Style Guidelines

### General Guidelines

- **Follow C# naming conventions**:
  - PascalCase for public members
  - camelCase for private members
  - UPPER_CASE for constants
- **Use meaningful names** for variables, methods, and classes
- **Add XML documentation** for all public APIs
- **Keep methods focused** and single-purpose
- **Use appropriate access modifiers** (private, internal, public)

### Code Formatting

- **Use 4 spaces** for indentation (not tabs)
- **Use braces** for all control structures
- **Place opening braces** on the same line
- **Add blank lines** between logical sections
- **Limit line length** to 120 characters

### Example Code Style

```csharp
/// <summary>
/// Validates serial port name and returns the validated name.
/// </summary>
/// <param name="portName">The port name to validate</param>
/// <returns>Validated port name</returns>
/// <exception cref="ValidationException">Thrown when port name is invalid</exception>
public static string ValidatePortName(string portName)
{
    if (string.IsNullOrWhiteSpace(portName))
    {
        throw new ValidationException("PortName", portName, "Port name cannot be null or empty");
    }

    if (!SerialPort.GetPortNames().Contains(portName))
    {
        var availablePorts = string.Join(", ", SerialPort.GetPortNames());
        throw new ValidationException("PortName", portName, 
            $"Invalid port name '{portName}'. Available ports: {availablePorts}");
    }
    
    return portName;
}
```

### Error Handling

- **Use specific exception types** from the `Exceptions` namespace
- **Include context information** in error messages
- **Log errors appropriately** using the logging system
- **Handle exceptions gracefully** with proper cleanup

### Logging Guidelines

- **Use appropriate log levels**:
  - `Trace`: Very detailed information
  - `Debug`: Debug information
  - `Info`: General information
  - `Warning`: Warning messages
  - `Error`: Error conditions
  - `Critical`: Critical errors
- **Include context information** in log messages
- **Use structured logging** for complex data
- **Avoid logging sensitive information**

## Testing Guidelines

### Unit Testing

- **Test all public methods** and properties
- **Test error conditions** and edge cases
- **Test configuration validation**
- **Test logging functionality**
- **Use descriptive test names**

### Integration Testing

- **Test serial port communication** (with test device)
- **Test file operations** (create, write, read)
- **Test configuration loading**
- **Test error handling scenarios**

### Test Structure

```csharp
[TestClass]
public class InputValidatorTests
{
    [TestMethod]
    public void ValidatePortName_ValidPort_ReturnsPortName()
    {
        // Arrange
        string validPort = "COM1";
        
        // Act
        string result = InputValidator.ValidatePortName(validPort);
        
        // Assert
        Assert.AreEqual(validPort, result);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public void ValidatePortName_InvalidPort_ThrowsValidationException()
    {
        // Arrange
        string invalidPort = "COM99";
        
        // Act & Assert
        InputValidator.ValidatePortName(invalidPort);
    }
}
```

## Pull Request Process

### Before Submitting

1. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following the code style guidelines

3. **Add tests** for new functionality

4. **Update documentation** if needed

5. **Test your changes** thoroughly

6. **Commit your changes**:
   ```bash
   git add .
   git commit -m "feat: Add new feature description"
   ```

### Commit Message Format

Use conventional commit format:

- `feat:` New features
- `fix:` Bug fixes
- `docs:` Documentation changes
- `style:` Code style changes
- `refactor:` Code refactoring
- `test:` Test additions or changes
- `chore:` Maintenance tasks

### Pull Request Guidelines

1. **Create a pull request** from your feature branch
2. **Provide a clear description** of your changes
3. **Reference any related issues**
4. **Include screenshots** for UI changes
5. **Ensure all tests pass**
6. **Request review** from maintainers

### Pull Request Template

```markdown
## Description
Brief description of the changes made.

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] Tests added/updated
```

## Issue Reporting

### Before Creating an Issue

1. **Search existing issues** to avoid duplicates
2. **Check the troubleshooting guide** in the documentation
3. **Try the latest version** of the application
4. **Enable debug mode** to gather more information

### Issue Template

```markdown
## Bug Report

### Description
Clear description of the bug.

### Steps to Reproduce
1. Step one
2. Step two
3. Step three

### Expected Behavior
What should happen.

### Actual Behavior
What actually happens.

### Environment
- Windows Version:
- .NET Framework Version:
- Application Version:
- Hardware:

### Additional Information
- Error messages:
- Log files:
- Configuration files:
- Screenshots:
```

### Feature Request Template

```markdown
## Feature Request

### Description
Clear description of the requested feature.

### Use Case
Why would this feature be useful?

### Proposed Solution
How should this feature work?

### Alternatives
Alternative solutions considered.

### Additional Context
Any other relevant information.
```

## Development Workflow

### Daily Workflow

1. **Sync with upstream**:
   ```bash
   git fetch upstream
   git checkout main
   git merge upstream/main
   ```

2. **Create feature branch**:
   ```bash
   git checkout -b feature/your-feature
   ```

3. **Make changes** and commit frequently

4. **Push to your fork**:
   ```bash
   git push origin feature/your-feature
   ```

5. **Create pull request** when ready

### Code Review Process

1. **Automated checks** must pass
2. **Code review** by maintainers
3. **Address feedback** and make changes
4. **Merge** when approved

## Community Guidelines

### Code of Conduct

- **Be respectful** and inclusive
- **Be constructive** in feedback
- **Be patient** with newcomers
- **Be collaborative** in discussions

### Getting Help

- **Check documentation** first
- **Search existing issues**
- **Ask questions** in discussions
- **Be specific** about problems

## Recognition

Contributors will be recognized in:
- **README.md** contributors section
- **CHANGELOG.md** for significant contributions
- **GitHub contributors** page

---

Thank you for contributing to the COM Port Logger project! Your contributions help make this tool better for everyone.
