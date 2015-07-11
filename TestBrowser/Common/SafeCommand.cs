using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Common
{
	public class SafeCommand: Caliburn.Micro.PropertyChangedBase, ICommand
	{
		private readonly SafeDispatcher _dispatcher;
		private readonly Action _execute;
		private readonly Func<bool> _canExecute;

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
		public bool IsEnabled => CanExecute( null );

		#region ICommand Members

		public bool CanExecute( object parameter ) => _canExecute();
		public void Execute( object parameter ) => _execute();

		public event EventHandler CanExecuteChanged;

		#endregion

		public void RaiseCanExecuteChanged()
		{
			base.NotifyOfPropertyChange( nameof( IsEnabled ) );

			var handler = CanExecuteChanged;
			if ( handler != null )
				_dispatcher.Invoke( () => handler( this, EventArgs.Empty ) );
		}
	}
}
