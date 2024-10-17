using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COM_Port_Logger.ConfigurationSettings
{
	public class ColorScheme
	{
		public ConsoleColor BackgroundColor { get; }
		public ConsoleColor TextColor { get; }
		public ConsoleColor NumberColor { get; }

		public ColorScheme(ConsoleColor backgroundColor, ConsoleColor textColor, ConsoleColor numberColor)
		{
			BackgroundColor = backgroundColor;
			TextColor = textColor;
			NumberColor = numberColor;
		}

		public static ColorScheme Default => new ColorScheme(ConsoleColor.Black, ConsoleColor.White, ConsoleColor.Yellow);
		public static ColorScheme DarkMode => new ColorScheme(ConsoleColor.Black, ConsoleColor.Gray, ConsoleColor.Cyan);
		public static ColorScheme LightMode => new ColorScheme(ConsoleColor.White, ConsoleColor.Black, ConsoleColor.Blue);
		public static ColorScheme SolarizedDark => new ColorScheme(ConsoleColor.DarkBlue, ConsoleColor.Gray, ConsoleColor.Yellow);
		public static ColorScheme SolarizedLight => new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue, ConsoleColor.DarkGreen);
		public static ColorScheme Monokai => new ColorScheme(ConsoleColor.Black, ConsoleColor.Gray, ConsoleColor.Magenta);
		public static ColorScheme GruvboxDark => new ColorScheme(ConsoleColor.DarkGray, ConsoleColor.White, ConsoleColor.Yellow);
		public static ColorScheme GruvboxLight => new ColorScheme(ConsoleColor.White, ConsoleColor.DarkGray, ConsoleColor.Yellow);
		public static ColorScheme Nord => new ColorScheme(ConsoleColor.DarkBlue, ConsoleColor.Gray, ConsoleColor.Cyan);
		public static ColorScheme Ocean => new ColorScheme(ConsoleColor.DarkCyan, ConsoleColor.White, ConsoleColor.Cyan);
		public static ColorScheme Desert => new ColorScheme(ConsoleColor.DarkYellow, ConsoleColor.Black, ConsoleColor.DarkRed);
		public static ColorScheme Retro => new ColorScheme(ConsoleColor.Black, ConsoleColor.Green, ConsoleColor.Yellow);
		public static ColorScheme Cyberpunk => new ColorScheme(ConsoleColor.Black, ConsoleColor.Magenta, ConsoleColor.Cyan);
		public static ColorScheme Twilight => new ColorScheme(ConsoleColor.DarkMagenta, ConsoleColor.White, ConsoleColor.DarkYellow);
		public static ColorScheme Forest => new ColorScheme(ConsoleColor.DarkGreen, ConsoleColor.White, ConsoleColor.Yellow);
		public static ColorScheme Sunset => new ColorScheme(ConsoleColor.DarkYellow, ConsoleColor.DarkRed, ConsoleColor.Yellow);


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
