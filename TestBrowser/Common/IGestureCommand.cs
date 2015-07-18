using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HellBrick.TestBrowser.Common
{
	public interface IGestureCommand : ICommand
	{
		string Text { get; }
		InputGesture[] Gestures { get; }
		string GestureText { get; }
	}
}
