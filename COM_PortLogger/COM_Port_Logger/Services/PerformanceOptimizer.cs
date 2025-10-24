using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using COM_Port_Logger.Logging;

namespace COM_Port_Logger.Services
{
    /// <summary>
    /// Performance monitoring and optimization service for COM Port Logger.
    /// Provides performance counters, memory monitoring, and optimization recommendations.
    /// </summary>
    public static class PerformanceOptimizer
    {
        private static readonly object _lock = new object();
        private static bool _isInitialized = false;
        private static readonly ConcurrentDictionary<string, PerformanceCounter> _counters = new ConcurrentDictionary<string, PerformanceCounter>();
        private static readonly ConcurrentQueue<MemoryUsage> _memoryHistory = new ConcurrentQueue<MemoryUsage>();
        private static Timer _monitoringTimer;
        private static readonly int _maxMemoryHistorySize = 100;

        /// <summary>
        /// Performance counter for tracking operation metrics.
        /// Records timing, frequency, and performance statistics for various operations.
        /// </summary>
        public class PerformanceCounter
        {
            /// <summary>
            /// Gets or sets the name of the performance counter.
            /// </summary>
            public string Name { get; set; }
            
            /// <summary>
            /// Gets or sets the total number of operations recorded.
            /// </summary>
            public long TotalOperations { get; set; }
            
            /// <summary>
            /// Gets or sets the total duration of all operations in milliseconds.
            /// </summary>
            public long TotalDurationMs { get; set; }
            
            /// <summary>
            /// Gets or sets the minimum operation duration in milliseconds.
            /// </summary>
            public long MinDurationMs { get; set; } = long.MaxValue;
            
            /// <summary>
            /// Gets or sets the maximum operation duration in milliseconds.
            /// </summary>
            public long MaxDurationMs { get; set; }
            
            /// <summary>
            /// Gets or sets the timestamp of the last operation.
            /// </summary>
            public DateTime LastOperation { get; set; }
            
            /// <summary>
            /// Gets the average duration of operations in milliseconds.
            /// </summary>
            public double AverageDurationMs => TotalOperations > 0 ? (double)TotalDurationMs / TotalOperations : 0;

            /// <summary>
            /// Records a new operation with its duration.
            /// Updates all performance metrics including min, max, and average durations.
            /// </summary>
            /// <param name="durationMs">The duration of the operation in milliseconds.</param>
            public void RecordOperation(long durationMs)
            {
                TotalOperations++;
                TotalDurationMs += durationMs;
                MinDurationMs = Math.Min(MinDurationMs, durationMs);
                MaxDurationMs = Math.Max(MaxDurationMs, durationMs);
                LastOperation = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Memory usage tracking
        /// </summary>
        public class MemoryUsage
        {
            public DateTime Timestamp { get; set; }
            public long WorkingSetBytes { get; set; }
            public long PrivateMemoryBytes { get; set; }
            public long Gen0Collections { get; set; }
            public long Gen1Collections { get; set; }
            public long Gen2Collections { get; set; }
        }

        /// <summary>
        /// Initialize performance monitoring
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            lock (_lock)
            {
                if (_isInitialized) return;

                try
                {
                    // Start memory monitoring timer (every 30 seconds)
                    _monitoringTimer = new Timer(MonitorMemoryUsage, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
                    
                    // Force garbage collection to establish baseline
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    Log.Info("Performance monitoring initialized", "PerformanceOptimizer");
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to initialize performance monitoring: {ex.Message}", "PerformanceOptimizer", "PerformanceOptimizer", ex);
                }
            }
        }

        /// <summary>
        /// Record operation performance
        /// </summary>
        public static void RecordOperation(string operationName, long durationMs)
        {
            var counter = _counters.GetOrAdd(operationName, name => new PerformanceCounter { Name = name });
            counter.RecordOperation(durationMs);
        }

        /// <summary>
        /// Record operation performance with automatic timing
        /// </summary>
        public static T RecordOperation<T>(string operationName, Func<T> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                return operation();
            }
            finally
            {
                stopwatch.Stop();
                RecordOperation(operationName, stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Record operation performance with automatic timing (void)
        /// </summary>
        public static void RecordOperation(string operationName, Action operation)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                operation();
            }
            finally
            {
                stopwatch.Stop();
                RecordOperation(operationName, stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Get performance statistics
        /// </summary>
        public static Dictionary<string, PerformanceCounter> GetPerformanceStats()
        {
            return new Dictionary<string, PerformanceCounter>(_counters);
        }

        /// <summary>
        /// Get memory usage statistics
        /// </summary>
        public static MemoryUsage GetCurrentMemoryUsage()
        {
            var process = Process.GetCurrentProcess();
            return new MemoryUsage
            {
                Timestamp = DateTime.UtcNow,
                WorkingSetBytes = process.WorkingSet64,
                PrivateMemoryBytes = process.PrivateMemorySize64,
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2)
            };
        }

        /// <summary>
        /// Optimize memory usage
        /// </summary>
        public static void OptimizeMemory()
        {
            try
            {
                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Compact large object heap if available
                if (GCSettings.IsServerGC)
                {
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect();
                }

                Log.Info("Memory optimization completed", "PerformanceOptimizer");
            }
            catch (Exception ex)
            {
                Log.Error($"Memory optimization failed: {ex.Message}", "PerformanceOptimizer", "PerformanceOptimizer", ex);
            }
        }

        /// <summary>
        /// Monitor memory usage periodically
        /// </summary>
        private static void MonitorMemoryUsage(object state)
        {
            try
            {
                var memoryUsage = GetCurrentMemoryUsage();
                
                // Add to history
                _memoryHistory.Enqueue(memoryUsage);
                
                // Maintain history size
                while (_memoryHistory.Count > _maxMemoryHistorySize)
                {
                    _memoryHistory.TryDequeue(out _);
                }

                // Log memory usage if it's high
                var workingSetMB = memoryUsage.WorkingSetBytes / (1024 * 1024);
                if (workingSetMB > 100) // Log if using more than 100MB
                {
                    Log.Warning($"High memory usage detected: {workingSetMB}MB", "PerformanceOptimizer");
                }

                // Trigger optimization if memory usage is very high
                if (workingSetMB > 200) // Optimize if using more than 200MB
                {
                    Log.Warning("Triggering memory optimization due to high usage", "PerformanceOptimizer");
                    OptimizeMemory();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Memory monitoring failed: {ex.Message}", "PerformanceOptimizer", "PerformanceOptimizer", ex);
            }
        }

        /// <summary>
        /// Get memory history
        /// </summary>
        public static List<MemoryUsage> GetMemoryHistory()
        {
            return new List<MemoryUsage>(_memoryHistory);
        }

        /// <summary>
        /// Dispose performance monitoring
        /// </summary>
        public static void Dispose()
        {
            _monitoringTimer?.Dispose();
            _counters.Clear();
            
            while (_memoryHistory.TryDequeue(out _)) { }
            
            _isInitialized = false;
        }
    }

    /// <summary>
    /// Security service for input sanitization and validation
    /// </summary>
    public static class SecurityService
    {
        private static readonly HashSet<string> _allowedFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".log", ".csv", ".json", ".xml"
        };

        private static readonly HashSet<string> _allowedPortNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new object();
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize security service
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            lock (_lock)
            {
                if (_isInitialized) return;

                try
                {
                    // Initialize allowed port names
                    RefreshAllowedPorts();
                    
                    Log.Info("Security service initialized", "SecurityService");
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to initialize security service: {ex.Message}", "SecurityService", "SecurityService", ex);
                }
            }
        }

        /// <summary>
        /// Sanitize file path to prevent directory traversal attacks
        /// </summary>
        public static string SanitizeFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            // Remove any directory traversal attempts
            var sanitized = filePath.Replace("..", "").Replace("~", "");
            
            // Ensure path is within allowed directory
            var fullPath = Path.GetFullPath(sanitized);
            var currentDir = Path.GetFullPath(Directory.GetCurrentDirectory());
            
            if (!fullPath.StartsWith(currentDir, StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityException($"File path '{filePath}' is outside allowed directory");
            }

            return fullPath;
        }

        /// <summary>
        /// Sanitize file name to prevent injection attacks
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            }

            // Remove invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = fileName;
            
            foreach (var invalidChar in invalidChars)
            {
                sanitized = sanitized.Replace(invalidChar.ToString(), "");
            }

            // Ensure file has allowed extension
            var extension = Path.GetExtension(sanitized);
            if (!string.IsNullOrEmpty(extension) && !_allowedFileExtensions.Contains(extension))
            {
                throw new SecurityException($"File extension '{extension}' is not allowed");
            }

            // Limit file name length
            if (sanitized.Length > 255)
            {
                sanitized = sanitized.Substring(0, 255);
            }

            return sanitized;
        }

        /// <summary>
        /// Sanitize serial port name
        /// </summary>
        public static string SanitizePortName(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
            {
                throw new ArgumentException("Port name cannot be null or empty", nameof(portName));
            }

            // Remove any non-alphanumeric characters except COM prefix
            var sanitized = System.Text.RegularExpressions.Regex.Replace(portName, @"[^A-Za-z0-9]", "");
            
            // Ensure it starts with COM
            if (!sanitized.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityException($"Invalid port name format: {portName}");
            }

            // Check if port is in allowed list
            if (!_allowedPortNames.Contains(sanitized))
            {
                throw new SecurityException($"Port '{sanitized}' is not available or not allowed");
            }

            return sanitized;
        }

        /// <summary>
        /// Sanitize configuration value
        /// </summary>
        public static string SanitizeConfigValue(string value, int maxLength = 1000)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            // Remove potential injection characters
            var sanitized = value.Replace("\0", "").Replace("\r", "").Replace("\n", "");
            
            // Limit length
            if (sanitized.Length > maxLength)
            {
                sanitized = sanitized.Substring(0, maxLength);
            }

            return sanitized;
        }

        /// <summary>
        /// Validate and sanitize baud rate
        /// </summary>
        public static int SanitizeBaudRate(int baudRate)
        {
            // Common valid baud rates
            var validBaudRates = new HashSet<int>
            {
                110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200, 230400, 460800, 921600
            };

            if (!validBaudRates.Contains(baudRate))
            {
                throw new SecurityException($"Baud rate {baudRate} is not in the list of allowed values");
            }

            return baudRate;
        }

        /// <summary>
        /// Refresh list of allowed serial ports
        /// </summary>
        public static void RefreshAllowedPorts()
        {
            try
            {
                _allowedPortNames.Clear();
                var availablePorts = SerialPort.GetPortNames();
                
                foreach (var port in availablePorts)
                {
                    _allowedPortNames.Add(port);
                }

                Log.Debug($"Refreshed allowed ports: {string.Join(", ", _allowedPortNames)}", "SecurityService");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to refresh allowed ports: {ex.Message}", "SecurityService", "SecurityService", ex);
            }
        }

        /// <summary>
        /// Validate input against SQL injection patterns
        /// </summary>
        public static bool IsValidInput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return true;
            }

            // Check for common injection patterns
            var suspiciousPatterns = new[]
            {
                "';", "\";", "--", "/*", "*/", "xp_", "sp_", "exec", "execute", "union", "select", "insert", "update", "delete", "drop", "create", "alter"
            };

            var lowerInput = input.ToLowerInvariant();
            foreach (var pattern in suspiciousPatterns)
            {
                if (lowerInput.Contains(pattern))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get security audit information
        /// </summary>
        public static Dictionary<string, object> GetSecurityAudit()
        {
            return new Dictionary<string, object>
            {
                ["AllowedPorts"] = _allowedPortNames.Count,
                ["AllowedFileExtensions"] = _allowedFileExtensions.Count,
                ["ServiceInitialized"] = _isInitialized,
                ["LastPortRefresh"] = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Security exception for security-related errors
    /// </summary>
    public class SecurityException : Exception
    {
        public string SecurityContext { get; }
        public string ThreatType { get; }

        public SecurityException(string message, string securityContext = "", string threatType = "General") 
            : base(message)
        {
            SecurityContext = securityContext;
            ThreatType = threatType;
        }

        public SecurityException(string message, Exception innerException, string securityContext = "", string threatType = "General") 
            : base(message, innerException)
        {
            SecurityContext = securityContext;
            ThreatType = threatType;
        }
    }
}
