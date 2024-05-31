using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace PortLogger.Utilities
{
	public class DataReceivedEventArgs : EventArgs
	{
		public string Data { get; }

		public DataReceivedEventArgs(string data)
		{
			Data = data;
		}
	}


	public class SerialPortReader
	{
		private SerialPort _serialPort;
		private Thread _readingThread;
		private bool _isReading;

		public event EventHandler<DataReceivedEventArgs> DataReceived;

		public SerialPortReader(string portName, int baudRate)
		{
			_serialPort = new SerialPort(portName, baudRate);
			_serialPort.DataReceived += SerialPort_DataReceived;
			_isReading = false;
		}

		public void StartReading()
		{
			if (!_isReading)
			{
				_isReading = true;
				_serialPort.Open();
				_readingThread = new Thread(ReadingThread);
				_readingThread.IsBackground = true;
				_readingThread.Start();
			}
		}

		public void StopReading()
		{
			if (_isReading)
			{
				_isReading = false;
				_serialPort.Close();

				if (_readingThread != null && _readingThread.IsAlive)
				{
					_readingThread.Join();
				}
			}

		}

		private void ReadingThread()
		{
			while (_isReading)
			{
				try
				{
					string data = _serialPort.ReadLine();
					OnDataReceived(data);
				}
				catch (TimeoutException) { }
				catch (InvalidOperationException) { }
				catch (IOException) { }
			}
		}

		private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				string data = _serialPort.ReadLine();
				OnDataReceived(data);
			}
			catch (TimeoutException) { }
			catch (InvalidOperationException) { }
			catch (IOException) { }
		}

		protected virtual void OnDataReceived(string data)
		{
			DataReceived?.Invoke(this, new DataReceivedEventArgs(data));
		}
	}
}
