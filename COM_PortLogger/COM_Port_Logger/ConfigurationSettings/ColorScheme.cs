using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COM_Port_Logger.ConfigurationSettings
{
	/// <summary>
	/// Represents a color scheme configuration for the console display.
	/// Defines the background, text, and number colors used throughout the application.
	/// </summary>
	public class ColorScheme
	{
		/// <summary>
		/// Gets the background color for the console display.
		/// </summary>
		public ConsoleColor BackgroundColor { get; }
		
		/// <summary>
		/// Gets the primary text color for the console display.
		/// </summary>
		public ConsoleColor TextColor { get; }
		
		/// <summary>
		/// Gets the color used for highlighting numbers in the console display.
		/// </summary>
		public ConsoleColor NumberColor { get; }

		/// <summary>
		/// Initializes a new instance of the ColorScheme class with specified colors.
		/// </summary>
		/// <param name="backgroundColor">The background color for the console.</param>
		/// <param name="textColor">The primary text color for the console.</param>
		/// <param name="numberColor">The color used for highlighting numbers.</param>
		public ColorScheme(ConsoleColor backgroundColor, ConsoleColor textColor, ConsoleColor numberColor)
		{
			BackgroundColor = backgroundColor;
			TextColor = textColor;
			NumberColor = numberColor;
		}

		/// <summary>
		/// Gets the default color scheme with black background, white text, and yellow numbers.
		/// </summary>
		public static ColorScheme Default => new ColorScheme(ConsoleColor.Black, ConsoleColor.White, ConsoleColor.Yellow);
		
		/// <summary>
		/// Gets the dark mode color scheme with black background, gray text, and cyan numbers.
		/// </summary>
		public static ColorScheme DarkMode => new ColorScheme(ConsoleColor.Black, ConsoleColor.Gray, ConsoleColor.Cyan);
		
		/// <summary>
		/// Gets the light mode color scheme with white background, black text, and blue numbers.
		/// </summary>
		public static ColorScheme LightMode => new ColorScheme(ConsoleColor.White, ConsoleColor.Black, ConsoleColor.Blue);
		
		/// <summary>
		/// Gets the Solarized Dark color scheme with dark blue background, gray text, and yellow numbers.
		/// </summary>
		public static ColorScheme SolarizedDark => new ColorScheme(ConsoleColor.DarkBlue, ConsoleColor.Gray, ConsoleColor.Yellow);
		
		/// <summary>
		/// Gets the Solarized Light color scheme with white background, dark blue text, and dark green numbers.
		/// </summary>
		public static ColorScheme SolarizedLight => new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue, ConsoleColor.DarkGreen);
		
		/// <summary>
		/// Gets the Monokai color scheme with black background, gray text, and magenta numbers.
		/// </summary>
		public static ColorScheme Monokai => new ColorScheme(ConsoleColor.Black, ConsoleColor.Gray, ConsoleColor.Magenta);
		
		/// <summary>
		/// Gets the Gruvbox Dark color scheme with dark gray background, white text, and yellow numbers.
		/// </summary>
		public static ColorScheme GruvboxDark => new ColorScheme(ConsoleColor.DarkGray, ConsoleColor.White, ConsoleColor.Yellow);
		
		/// <summary>
		/// Gets the Gruvbox Light color scheme with white background, dark gray text, and yellow numbers.
		/// </summary>
		public static ColorScheme GruvboxLight => new ColorScheme(ConsoleColor.White, ConsoleColor.DarkGray, ConsoleColor.Yellow);
		
		/// <summary>
		/// Gets the Nord color scheme with dark blue background, gray text, and cyan numbers.
		/// </summary>
		public static ColorScheme Nord => new ColorScheme(ConsoleColor.DarkBlue, ConsoleColor.Gray, ConsoleColor.Cyan);
		
		/// <summary>
		/// Gets the Ocean color scheme with dark cyan background, white text, and cyan numbers.
		/// </summary>
		public static ColorScheme Ocean => new ColorScheme(ConsoleColor.DarkCyan, ConsoleColor.White, ConsoleColor.Cyan);
		
		/// <summary>
		/// Gets the Desert color scheme with dark yellow background, black text, and dark red numbers.
		/// </summary>
		public static ColorScheme Desert => new ColorScheme(ConsoleColor.DarkYellow, ConsoleColor.Black, ConsoleColor.DarkRed);
		
		/// <summary>
		/// Gets the Retro color scheme with black background, green text, and yellow numbers.
		/// </summary>
		public static ColorScheme Retro => new ColorScheme(ConsoleColor.Black, ConsoleColor.Green, ConsoleColor.Yellow);
		
		/// <summary>
		/// Gets the Cyberpunk color scheme with black background, magenta text, and cyan numbers.
		/// </summary>
		public static ColorScheme Cyberpunk => new ColorScheme(ConsoleColor.Black, ConsoleColor.Magenta, ConsoleColor.Cyan);
		
		/// <summary>
		/// Gets the Twilight color scheme with dark magenta background, white text, and dark yellow numbers.
		/// </summary>
		public static ColorScheme Twilight => new ColorScheme(ConsoleColor.DarkMagenta, ConsoleColor.White, ConsoleColor.DarkYellow);
		
		/// <summary>
		/// Gets the Forest color scheme with dark green background, white text, and yellow numbers.
		/// </summary>
		public static ColorScheme Forest => new ColorScheme(ConsoleColor.DarkGreen, ConsoleColor.White, ConsoleColor.Yellow);
		
		/// <summary>
		/// Gets the Sunset color scheme with dark yellow background, dark red text, and yellow numbers.
		/// </summary>
		public static ColorScheme Sunset => new ColorScheme(ConsoleColor.DarkYellow, ConsoleColor.DarkRed, ConsoleColor.Yellow);

		/// <summary>
		/// Retrieves a color scheme by its name. The comparison is case-insensitive.
		/// If the specified scheme name is not found, returns the default color scheme.
		/// </summary>
		/// <param name="schemeName">The name of the color scheme to retrieve.</param>
		/// <returns>The requested ColorScheme, or Default if the scheme name is not recognized.</returns>
		public static ColorScheme GetColorScheme(string schemeName)
		{
			switch(schemeName.ToLower())
			{
				case "default":
					return Default;
				case "darkmode":
					return DarkMode;
				case "lightmode":
					return LightMode;
				case "solarizeddark":
					return SolarizedDark;
				case "solarizedlight":
					return SolarizedLight;
				case "monokai":
					return Monokai;
				case "gruvboxdark":
					return GruvboxDark;
				case "gruvboxlight":
					return GruvboxLight;
				case "nord":
					return Nord;
				case "ocean":
					return Ocean;
				case "desert":
					return Desert;
				case "retro":
					return Retro;
				case "cyberpunk":
					return Cyberpunk;
				case "twilight":
					return Twilight;
				case "forest":
					return Forest;
				case "sunset":
					return Sunset;
				default:
					return Default;
			}
		}

	}
}
