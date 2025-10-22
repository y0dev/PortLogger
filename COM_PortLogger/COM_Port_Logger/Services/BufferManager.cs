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
    /// Memory-efficient buffer manager for serial port operations
    /// </summary>
    public class BufferManager : IDisposable
    {
        private readonly ConcurrentQueue<StringBuilder> _stringBuilderPool;
        private readonly ConcurrentQueue<byte[]> _byteBufferPool;
        private readonly int _maxPoolSize;
        private readonly int _bufferSize;
        private readonly object _lock = new object();
        private bool _disposed = false;

        public BufferManager(int maxPoolSize = 50, int bufferSize = 4096)
        {
            _maxPoolSize = maxPoolSize;
            _bufferSize = bufferSize;
            _stringBuilderPool = new ConcurrentQueue<StringBuilder>();
            _byteBufferPool = new ConcurrentQueue<byte[]>();

            // Pre-allocate some buffers
            for (int i = 0; i < Math.Min(10, maxPoolSize); i++)
            {
                _stringBuilderPool.Enqueue(new StringBuilder(1024));
                _byteBufferPool.Enqueue(new byte[bufferSize]);
            }
        }

        /// <summary>
        /// Get a StringBuilder from the pool
        /// </summary>
        public StringBuilder GetStringBuilder()
        {
            if (_stringBuilderPool.TryDequeue(out var sb))
            {
                sb.Clear();
                return sb;
            }

            // Create new one if pool is empty
            return new StringBuilder(1024);
        }

        /// <summary>
        /// Return a StringBuilder to the pool
        /// </summary>
        public void ReturnStringBuilder(StringBuilder sb)
        {
            if (sb == null || _disposed) return;

            if (_stringBuilderPool.Count < _maxPoolSize)
            {
                sb.Clear();
                _stringBuilderPool.Enqueue(sb);
            }
        }

        /// <summary>
        /// Get a byte buffer from the pool
        /// </summary>
        public byte[] GetByteBuffer()
        {
            if (_byteBufferPool.TryDequeue(out var buffer))
            {
                Array.Clear(buffer, 0, buffer.Length);
                return buffer;
            }

            // Create new one if pool is empty
            return new byte[_bufferSize];
        }

        /// <summary>
        /// Return a byte buffer to the pool
        /// </summary>
        public void ReturnByteBuffer(byte[] buffer)
        {
            if (buffer == null || _disposed || buffer.Length != _bufferSize) return;

            if (_byteBufferPool.Count < _maxPoolSize)
            {
                Array.Clear(buffer, 0, buffer.Length);
                _byteBufferPool.Enqueue(buffer);
            }
        }

        /// <summary>
        /// Get buffer pool statistics
        /// </summary>
        public (int StringBuilderCount, int ByteBufferCount) GetPoolStats()
        {
            return (_stringBuilderPool.Count, _byteBufferPool.Count);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            // Clear pools
            while (_stringBuilderPool.TryDequeue(out _)) { }
            while (_byteBufferPool.TryDequeue(out _)) { }
        }
    }

    /// <summary>
    /// High-performance serial port reader with optimized memory usage
    /// </summary>
    public class OptimizedSerialPortReader : IDisposable
    {
        private readonly SerialPort _serialPort;
        private readonly BufferManager _bufferManager;
        private readonly Action<string> _dataReceivedCallback;
        private readonly byte[] _readBuffer;
        private readonly StringBuilder _stringBuilder;
        private readonly object _lock = new object();
        private bool _disposed = false;
        private bool _isReading = false;

        public OptimizedSerialPortReader(SerialPort serialPort, BufferManager bufferManager, Action<string> dataReceivedCallback)
        {
            _serialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
            _bufferManager = bufferManager ?? throw new ArgumentNullException(nameof(bufferManager));
            _dataReceivedCallback = dataReceivedCallback ?? throw new ArgumentNullException(nameof(dataReceivedCallback));
            
            _readBuffer = _bufferManager.GetByteBuffer();
            _stringBuilder = _bufferManager.GetStringBuilder();
        }

        /// <summary>
        /// Start reading from serial port
        /// </summary>
        public void StartReading()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(OptimizedSerialPortReader));

            lock (_lock)
            {
                if (_isReading) return;

                _serialPort.DataReceived += OnDataReceived;
                _isReading = true;
            }
        }

        /// <summary>
        /// Stop reading from serial port
        /// </summary>
        public void StopReading()
        {
            lock (_lock)
            {
                if (!_isReading) return;

                _serialPort.DataReceived -= OnDataReceived;
                _isReading = false;
            }
        }

        /// <summary>
        /// Handle data received event
        /// </summary>
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_disposed || !_isReading) return;

                var bytesToRead = _serialPort.BytesToRead;
                if (bytesToRead <= 0) return;

                // Ensure buffer is large enough
                if (bytesToRead > _readBuffer.Length)
                {
                    Log.Warning($"Data size ({bytesToRead}) exceeds buffer size ({_readBuffer.Length})", "OptimizedSerialPortReader");
                    return;
                }

                // Read data
                var bytesRead = _serialPort.Read(_readBuffer, 0, bytesToRead);
                
                if (bytesRead > 0)
                {
                    // Convert to string efficiently
                    var data = Encoding.UTF8.GetString(_readBuffer, 0, bytesRead);
                    
                    // Call callback
                    _dataReceivedCallback?.Invoke(data);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error reading serial port data: {ex.Message}", "OptimizedSerialPortReader", "OptimizedSerialPortReader", ex);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            StopReading();

            // Return buffers to pool
            _bufferManager.ReturnByteBuffer(_readBuffer);
            _bufferManager.ReturnStringBuilder(_stringBuilder);
        }
    }

    /// <summary>
    /// High-performance file writer with buffering and async operations
    /// </summary>
    public class OptimizedFileWriter : IDisposable
    {
        private readonly string _filePath;
        private readonly BufferManager _bufferManager;
        private readonly ConcurrentQueue<string> _writeQueue;
        private readonly Timer _flushTimer;
        private readonly object _lock = new object();
        private FileStream _fileStream;
        private StreamWriter _streamWriter;
        private bool _disposed = false;
        private bool _isWriting = false;

        public OptimizedFileWriter(string filePath, BufferManager bufferManager)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _bufferManager = bufferManager ?? throw new ArgumentNullException(nameof(bufferManager));
            _writeQueue = new ConcurrentQueue<string>();

            // Initialize file stream
            _fileStream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, true);
            _streamWriter = new StreamWriter(_fileStream, Encoding.UTF8, 4096) { AutoFlush = false };

            // Start flush timer (every 5 seconds)
            _flushTimer = new Timer(FlushToFile, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Write data to file (buffered)
        /// </summary>
        public void WriteLine(string data)
        {
            if (_disposed) return;

            _writeQueue.Enqueue(data);
            
            // Start writing if not already writing
            if (!_isWriting)
            {
                ThreadPool.QueueUserWorkItem(ProcessWriteQueue);
            }
        }

        /// <summary>
        /// Process write queue
        /// </summary>
        private void ProcessWriteQueue(object state)
        {
            lock (_lock)
            {
                if (_isWriting || _disposed) return;
                _isWriting = true;
            }

            try
            {
                var stringBuilder = _bufferManager.GetStringBuilder();
                
                while (_writeQueue.TryDequeue(out var data))
                {
                    if (_disposed) break;
                    
                    stringBuilder.AppendLine(data);
                    
                    // Write in batches to reduce I/O operations
                    if (stringBuilder.Length > 8192)
                    {
                        _streamWriter.Write(stringBuilder.ToString());
                        stringBuilder.Clear();
                    }
                }

                // Write remaining data
                if (stringBuilder.Length > 0)
                {
                    _streamWriter.Write(stringBuilder.ToString());
                }

                _bufferManager.ReturnStringBuilder(stringBuilder);
            }
            catch (Exception ex)
            {
                Log.Error($"Error writing to file: {ex.Message}", "OptimizedFileWriter", "OptimizedFileWriter", ex);
            }
            finally
            {
                lock (_lock)
                {
                    _isWriting = false;
                }
            }
        }

        /// <summary>
        /// Flush data to file
        /// </summary>
        private void FlushToFile(object state)
        {
            try
            {
                _streamWriter?.Flush();
            }
            catch (Exception ex)
            {
                Log.Error($"Error flushing file: {ex.Message}", "OptimizedFileWriter", "OptimizedFileWriter", ex);
            }
        }

        /// <summary>
        /// Force flush all data
        /// </summary>
        public void Flush()
        {
            try
            {
                // Process any remaining queue items
                ProcessWriteQueue(null);
                
                // Flush to file
                _streamWriter?.Flush();
            }
            catch (Exception ex)
            {
                Log.Error($"Error during flush: {ex.Message}", "OptimizedFileWriter", "OptimizedFileWriter", ex);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                // Stop timer
                _flushTimer?.Dispose();

                // Process any remaining queue items
                ProcessWriteQueue(null);

                // Flush and close
                _streamWriter?.Flush();
                _streamWriter?.Dispose();
                _fileStream?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error($"Error disposing file writer: {ex.Message}", "OptimizedFileWriter", "OptimizedFileWriter", ex);
            }
        }
    }
}
