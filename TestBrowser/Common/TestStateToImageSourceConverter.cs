using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace HellBrick.TestBrowser.Common
{
	public class TestStateToImageSourceConverter : IValueConverter
	{
		public ImageSource IfFailed { get; set; }
		public ImageSource IfNotFound { get; set; }
		public ImageSource IfNotRun { get; set; }
		public ImageSource IfPassed { get; set; }
		public ImageSource IfSkipped { get; set; }

		#region IValueConverter Members

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			switch ( (TestState) value )
			{
				case TestState.Failed:
					return IfFailed;

				case TestState.NotFound:
					return IfNotFound;

				case TestState.NotRun:
					return IfNotRun;

				case TestState.Passed:
					return IfPassed;

				case TestState.Skipped:
					return IfSkipped;

				case TestState.None:
				default:
					return null;
			}
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
