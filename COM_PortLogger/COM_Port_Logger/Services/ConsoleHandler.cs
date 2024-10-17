using System;
using System.Runtime.InteropServices;

namespace COM_Port_Logger.Services
{
	public class ConsoleHandler
	{
		// P/Invoke to set console font size
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

		[StructLayout(LayoutKind.Sequential)]
		public struct CONSOLE_FONT_INFO_EX
		{
			public uint cbSize;
			public uint FontFamily;
			public uint FontStyle;
			public ushort FontSizeX;
			public ushort FontSizeY;
			public uint FontWeight;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string FaceName;
			public uint UnicodeFont;

			public void Init()
			{
				cbSize = (uint)Marshal.SizeOf(typeof(CONSOLE_FONT_INFO_EX));
				FaceName = "Consolas"; // or any other font
				FontWeight = 400; // Normal weight
			}
		}

		public static void SetConsoleFontSize(ushort fontSizeY)
		{
			IntPtr handle = GetStdHandle(-11); // -11 = STD_OUTPUT_HANDLE
			CONSOLE_FONT_INFO_EX fontInfo = new CONSOLE_FONT_INFO_EX();
			fontInfo.Init();

			fontInfo.FontSizeY = fontSizeY; // Set desired font size

			SetCurrentConsoleFontEx(handle, false, ref fontInfo);
		}
	}
}
