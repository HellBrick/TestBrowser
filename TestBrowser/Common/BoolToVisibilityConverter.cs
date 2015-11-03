using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace HellBrick.TestBrowser.Common
{
	public class BoolToVisibilityConverter : IValueConverter
	{
		public BoolToVisibilityConverter()
		{
			IfTrue = Visibility.Visible;
			IfFalse = Visibility.Collapsed;
		}

		public Visibility IfTrue { get; set; }
		public Visibility IfFalse { get; set; }

		#region IValueConverter Members

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			return ( (bool) value ) ? IfTrue : IfFalse;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
