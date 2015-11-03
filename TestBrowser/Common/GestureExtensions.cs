using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HellBrick.TestBrowser.Common
{
	public static class GestureExtensions
	{
		public static string GetGestureText<T>( this T gestureCollection ) where T : IEnumerable<InputGesture>
		{
			return gestureCollection.OfType<KeyGesture>().FirstOrDefault()?.GetDisplayStringForCulture( CultureInfo.InvariantCulture );
		}
	}
}
