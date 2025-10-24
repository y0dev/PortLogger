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
    /// Memory-efficient buffer manager for serial port operations.
    /// Provides object pooling for StringBuilder and byte array buffers to reduce garbage collection pressure.
    /// </summary>
    public class BufferManager : IDisposable
    {
        private readonly ConcurrentQueue<StringBuilder> _stringBuilderPool;
        private readonly ConcurrentQueue<byte[]> _byteBufferPool;
        private readonly int _maxPoolSize;
        private readonly int _bufferSize;
        private readonly object _lock = new object();
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the BufferManager class with specified pool and buffer sizes.
        /// </summary>
        /// <param name="maxPoolSize">Maximum number of buffers to keep in the pool (default: 50).</param>
        /// <param name="bufferSize">Size of each byte buffer in bytes (default: 4096).</param>
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
        /// Gets a StringBuilder from the pool for efficient string operations.
        /// If no StringBuilder is available in the pool, creates a new one.
        /// </summary>
        /// <returns>A cleared StringBuilder ready for use.</returns>
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
        /// Returns a StringBuilder to the pool for reuse.
        /// The StringBuilder is cleared before being returned to the pool.
        /// </summary>
        /// <param name="sb">The StringBuilder to return to the pool.</param>
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
        /// Gets a byte buffer from the pool for efficient byte operations.
        /// If no buffer is available in the pool, creates a new one.
        /// </summary>
        /// <returns>A cleared byte array ready for use.</returns>
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
        /// Returns a byte buffer to the pool for reuse.
        /// The buffer is cleared before being returned to the pool.
        /// </summary>
        /// <param name="buffer">The byte buffer to return to the pool.</param>
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
        /// Gets current statistics about the buffer pools.
        /// </summary>
        /// <returns>A tuple containing the count of available StringBuilder and byte buffer objects.</returns>
        public (int StringBuilderCount, int ByteBufferCount) GetPoolStats()
        {
            return (_stringBuilderPool.Count, _byteBufferPool.Count);
        }

        /// <summary>
        /// Disposes of the BufferManager and clears all pooled buffers.
        /// </summary>
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
    /// High-performance serial port reader with optimized memory usage.
    /// Uses pooled buffers and efficient data handling for serial port communication.
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

        /// <summary>
        /// Initializes a new instance of the OptimizedSerialPortReader class.
        /// </summary>
        /// <param name="serialPort">The serial port to read from.</param>
        /// <param name="bufferManager">The buffer manager for efficient memory usage.</param>
        /// <param name="dataReceivedCallback">Callback function to handle received data.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public OptimizedSerialPortReader(SerialPort serialPort, BufferManager bufferManager, Action<string> dataReceivedCallback)
        {
            _serialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
            _bufferManager = bufferManager ?? throw new ArgumentNullException(nameof(bufferManager));
            _dataReceivedCallback = dataReceivedCallback ?? throw new ArgumentNullException(nameof(dataReceivedCallback));
            
            _readBuffer = _bufferManager.GetByteBuffer();
            _stringBuilder = _bufferManager.GetStringBuilder();
        }

        /// <summary>
        /// Starts reading from the serial port using event-driven data reception.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the reader has been disposed.</exception>
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
        /// Stops reading from the serial port and unsubscribes from data events.
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
        /// Handles the data received event from the serial port.
        /// Processes incoming data efficiently using pooled buffers.
        /// </summary>
        /// <param name="sender">The serial port that raised the event.</param>
        /// <param name="e">Event arguments containing data information.</param>
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

        /// <summary>
        /// Disposes of the OptimizedSerialPortReader and returns buffers to the pool.
        /// </summary>
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
    /// High-performance file writer with buffering and async operations.
    /// Uses queued writing and periodic flushing for optimal I/O performance.
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

        /// <summary>
        /// Initializes a new instance of the OptimizedFileWriter class.
        /// </summary>
        /// <param name="filePath">The path to the file to write to.</param>
        /// <param name="bufferManager">The buffer manager for efficient memory usage.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
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
        /// Writes a line of data to the file using buffered queuing for optimal performance.
        /// </summary>
        /// <param name="data">The data line to write to the file.</param>
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
        /// Processes the write queue by batching data and writing to the file stream.
        /// Uses pooled StringBuilder for efficient string operations.
        /// </summary>
        /// <param name="state">Thread pool state parameter (unused).</param>
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
        /// Flushes buffered data to the file stream.
        /// Called periodically by the flush timer.
        /// </summary>
        /// <param name="state">Timer state parameter (unused).</param>
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
        /// Forces an immediate flush of all buffered data to the file.
        /// Processes any remaining queued items and flushes the stream.
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

        /// <summary>
        /// Disposes of the OptimizedFileWriter and ensures all data is written to disk.
        /// </summary>
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
