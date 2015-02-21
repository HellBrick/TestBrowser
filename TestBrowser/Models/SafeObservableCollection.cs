using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Models
{
	public class SafeObservableCollection<T>: ObservableCollection<T>
	{
		private readonly SafeDispatcher _dispatcher;

		public SafeObservableCollection( SafeDispatcher dispatcher )
		{
			_dispatcher = dispatcher;
		}

		#region ObservableCollection<T> members

		protected override void ClearItems()
		{
			_dispatcher.Invoke( () => base.ClearItems() );
		}

		protected override void InsertItem( int index, T item )
		{
			_dispatcher.Invoke( () => base.InsertItem( index, item ) );
		}

		protected override void MoveItem( int oldIndex, int newIndex )
		{
			_dispatcher.Invoke( () => base.MoveItem( oldIndex, newIndex ) );
		}

		protected override void RemoveItem( int index )
		{
			_dispatcher.Invoke( () => base.RemoveItem( index ) );
		}

		protected override void SetItem( int index, T item )
		{
			_dispatcher.Invoke( () => base.SetItem( index, item ) );
		}

		#endregion
	}
}
