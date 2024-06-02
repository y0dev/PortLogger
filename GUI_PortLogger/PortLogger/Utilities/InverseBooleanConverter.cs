﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PortLogger.Utilities
{
	public class InverseBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolean)
			{
				return !boolean;
			}
			return DependencyProperty.UnsetValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolean)
			{
				return !boolean;
			}
			return DependencyProperty.UnsetValue;
		}
	}
}
