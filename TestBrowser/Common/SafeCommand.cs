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

		public SafeCommand( SafeDispatcher dispatcher, Action execute )
			: this( dispatcher, execute, () => true )
		{
		}

		public SafeCommand( SafeDispatcher dispatcher, Action execute, Func<bool> canExecute )
		{
			_dispatcher = dispatcher;
			_execute = execute;
			_canExecute = canExecute;
		}

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
