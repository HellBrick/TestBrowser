using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Common
{
	public class SafeGestureCommand : SafeCommand
	{
		public SafeGestureCommand( SafeDispatcher dispatcher, Action execute, string text, KeyGesture gesture )
			: base( dispatcher, execute, text )
		{
			Gesture = gesture;
		}

		public KeyGesture Gesture { get; }
		public string GestureText => Gesture.GetDisplayStringForCulture( CultureInfo.InvariantCulture );
	}
}
