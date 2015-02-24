using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Common
{
	public class SafeCommand: ICommand
	{
		private SafeDispatcher _dispatcher;
		private Action _execute;
		private Func<bool> _canExecute;

		public SafeCommand( SafeDispatcher dispatcher, Action execute, string text )
			: this( dispatcher, execute, () => true, text )
		{
		}

		public SafeCommand( SafeDispatcher dispatcher, Action execute, Func<bool> canExecute, string text )
		{
			_dispatcher = dispatcher;
			_execute = execute;
			_canExecute = canExecute;
			Text = text;
		}

		public string Text { get; private set; }

		#region ICommand Members

		public bool CanExecute( object parameter )
		{
			return _canExecute();
		}

		public void Execute( object parameter )
		{
			_execute();
		}

		public event EventHandler CanExecuteChanged;

		#endregion

		public void RaiseCanExecuteChanged()
		{
			var handler = CanExecuteChanged;
			if ( handler != null )
				_dispatcher.Invoke( () => handler( this, EventArgs.Empty ) );
		}
	}
}
