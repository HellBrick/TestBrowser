﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace HellBrick.TestBrowser.Common
{
	public class NullToVisibilityConverter: IValueConverter
	{
		public NullToVisibilityConverter()
		{
			IfNull = Visibility.Collapsed;
			IfNotNull = Visibility.Visible;
		}

		public Visibility IfNull { get; set; }
		public Visibility IfNotNull { get; set; }

		#region IValueConverter Members

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			return value == null ? IfNull : IfNotNull;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
