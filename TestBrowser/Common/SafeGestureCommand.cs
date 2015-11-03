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
	public class SafeGestureCommand : SafeCommand, IGestureCommand
	{
		public SafeGestureCommand( SafeDispatcher dispatcher, Action execute, string text, params InputGesture[] gestures )
			: base( dispatcher, execute, text )
		{
			Gestures = gestures;
			GestureText = Gestures.GetGestureText();
		}

		public InputGesture[] Gestures { get; }
		public KeyGesture KeyGesture { get; }

		public string GestureText { get; }
	}
}
