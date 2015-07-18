using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace HellBrick.TestBrowser.Common
{
	public class CommandsToBindingsConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			var commandCollection = value as ICollection<SafeGestureCommand>;
			if ( commandCollection == null )
				return null;

			var bindingList = commandCollection
				.Select( c => new InputBinding( c, c.Gesture ) )
				.ToList();

			return new InputBindingCollection( bindingList );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
	}
}
