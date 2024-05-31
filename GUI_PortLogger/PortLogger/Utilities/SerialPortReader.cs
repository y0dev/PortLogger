using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace PortLogger.Utilities
{
	public class SerialPortReader
	{
		private SerialPort _serialPort;
		private Thread _readingThread;
		private bool _isReading;

		// public event EventHandler<string> DataReceived;

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
				_readingThread.Join();
				_serialPort.Close();
			}
		}

		private void ReadingThread()
		{
			while (_isReading)
			{
				try
				{
					string data = _serialPort.ReadLine();
					// DataReceived?.Invoke(this, data);
				}
				catch (TimeoutException) { }
			}
		}

		private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			string data = _serialPort.ReadLine();
			// DataReceived?.Invoke(this, data);
		}
	}
}
