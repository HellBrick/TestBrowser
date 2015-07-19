using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using HellBrick.TestBrowser.Common;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Models
{
	public abstract class RunnableNode : PropertyChangedBase, INode
	{
		private readonly SolutionTestBrowserModel _testBrowser;
		private readonly SafeDispatcher _dispatcher;

		protected RunnableNode( SolutionTestBrowserModel testBrowser, SafeDispatcher dispatcher )
		{
			_testBrowser = testBrowser;
			_dispatcher = dispatcher;
		}

		public abstract ICollection<INode> Children { get; }
		public abstract bool IsExpanded { get; set; }
		public abstract bool IsSelected { get; set; }
		public abstract bool IsVisible { get; }
		public abstract string Key { get; }
		public abstract string Name { get; }
		public abstract INode Parent { get; set; }
		public abstract INode Presenter { get; }
		public abstract NodeType Type { get; }

		public ICollection<IGestureCommand> Commands => EnumerateCommands().ToList();

		protected virtual IEnumerable<IGestureCommand> EnumerateCommands()
		{
			yield return new TestMethodCommand( _testBrowser, () => _testBrowser.RunNode( this ), "Run", new KeyGesture( System.Windows.Input.Key.Enter, ModifierKeys.Control ) );
			yield return new TestMethodCommand( _testBrowser, () => _testBrowser.DebugNode( this ), "Debug", new KeyGesture( System.Windows.Input.Key.Enter, ModifierKeys.Control | ModifierKeys.Shift ) );
		}

		private class TestMethodCommand : IGestureCommand
		{
			private readonly SolutionTestBrowserModel _testBrowser;
			private readonly System.Action _action;

			public TestMethodCommand( SolutionTestBrowserModel testBrowser, System.Action action, string text, params InputGesture[] gestures )
			{
				_testBrowser = testBrowser;
				_action = action;
				Text = text;
				Gestures = gestures;
				GestureText = Gestures.GetGestureText();
			}

			public string Text { get; }
			public InputGesture[] Gestures { get; }
			public string GestureText { get; }

			public event EventHandler CanExecuteChanged
			{
				add { _testBrowser.CanRunTestsChanged += value; }
				remove { _testBrowser.CanRunTestsChanged += value; }
			}

			public bool CanExecute( object parameter ) => _testBrowser.CanRunTests();
			public void Execute( object parameter ) => _action();
		}
	}
}
