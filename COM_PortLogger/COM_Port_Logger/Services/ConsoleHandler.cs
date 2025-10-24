using System;
using System.Runtime.InteropServices;

namespace COM_Port_Logger.Services
{
	/// <summary>
	/// Handles console operations including font size configuration.
	/// Provides platform-specific console manipulation using P/Invoke calls.
	/// </summary>
	public class ConsoleHandler
	{
		/// <summary>
		/// P/Invoke declaration to get a standard handle from the console.
		/// </summary>
		/// <param name="nStdHandle">The standard handle identifier (-11 for STD_OUTPUT_HANDLE).</param>
		/// <returns>Handle to the standard output device.</returns>
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetStdHandle(int nStdHandle);

		/// <summary>
		/// P/Invoke declaration to set the current console font.
		/// </summary>
		/// <param name="hConsoleOutput">Handle to the console output device.</param>
		/// <param name="bMaximumWindow">Whether to set the font for the maximum window size.</param>
		/// <param name="lpConsoleCurrentFontEx">Font information structure.</param>
		/// <returns>True if successful, false otherwise.</returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

		/// <summary>
		/// Structure containing console font information for Windows API calls.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct CONSOLE_FONT_INFO_EX
		{
			/// <summary>
			/// Size of this structure in bytes.
			/// </summary>
			public uint cbSize;
			
			/// <summary>
			/// Font family identifier.
			/// </summary>
			public uint FontFamily;
			
			/// <summary>
			/// Font style flags (bold, italic, etc.).
			/// </summary>
			public uint FontStyle;
			
			/// <summary>
			/// Font width in logical units.
			/// </summary>
			public ushort FontSizeX;
			
			/// <summary>
			/// Font height in logical units.
			/// </summary>
			public ushort FontSizeY;
			
			/// <summary>
			/// Font weight (normal, bold, etc.).
			/// </summary>
			public uint FontWeight;
			
			/// <summary>
			/// Font face name (e.g., "Consolas", "Courier New").
			/// </summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string FaceName;
			
			/// <summary>
			/// Unicode font identifier.
			/// </summary>
			public uint UnicodeFont;

			/// <summary>
			/// Initializes the font structure with default values.
			/// Sets the font to Consolas with normal weight.
			/// </summary>
			public void Init()
			{
				cbSize = (uint)Marshal.SizeOf(typeof(CONSOLE_FONT_INFO_EX));
				FaceName = "Consolas"; // or any other font
				FontWeight = 400; // Normal weight
			}
		}

		/// <summary>
		/// Sets the console font size using Windows API calls.
		/// </summary>
		/// <param name="fontSizeY">The desired font height in logical units.</param>
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
