using System;
using System.Collections.Generic;
using System.IO;

namespace PortLogger.Utilities
{
	public class IniFile
	{
		private readonly Dictionary<string, Dictionary<string, string>> _sections;

		public IniFile()
		{
			_sections = new Dictionary<string, Dictionary<string, string>>();
		}

		public void AddSection(string section)
		{
			if (!_sections.ContainsKey(section))
			{
				_sections.Add(section, new Dictionary<string, string>());
			}
		}

		public void AddKey(string section, string key, string value)
		{
			if (_sections.ContainsKey(section))
			{
				_sections[section][key] = value;
			}
		}

		public string GetValue(string section, string key)
		{
			if (_sections.ContainsKey(section) && _sections[section].ContainsKey(key))
			{
				return _sections[section][key];
			}
			return null;
		}

		public void Save(string filePath)
		{
			using (StreamWriter sw = new StreamWriter(filePath))
			{
				foreach (var section in _sections)
				{
					sw.WriteLine($"[{section.Key}]");
					foreach (var keyValuePair in section.Value)
					{
						sw.WriteLine($"{keyValuePair.Key}={keyValuePair.Value}");
					}
				}
			}
		}

		public bool Load(string filePath)
		{
			if (File.Exists(filePath))
			{
				// Clear existing sections
				_sections.Clear();
				string currentSection = null;
				foreach (string line in File.ReadLines(filePath))
				{
					if (line.StartsWith("[") && line.EndsWith("]"))
					{
						currentSection = line.Substring(1, line.Length - 2);
						AddSection(currentSection);
					}
					else if (!string.IsNullOrWhiteSpace(line) && currentSection != null)
					{
						string[] parts = line.Split(new char[] { '=' }, 2);
						if (parts.Length == 2)
						{
							AddKey(currentSection, parts[0].Trim(), parts[1].Trim());
						}
					}
				}
				return true;
			}
			else
			{
				Console.WriteLine("The configuration file does not exist.");
				return false;
			}
		}
	}
}
