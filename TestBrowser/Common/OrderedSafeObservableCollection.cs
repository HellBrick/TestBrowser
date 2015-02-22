using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Common
{
	public class OrderedSafeObservableCollection<T>: SafeObservableCollection<T>
	{
		private IComparer<T> _comparer;

		public OrderedSafeObservableCollection( SafeDispatcher dispatcher, IComparer<T> comparer )
			: base( dispatcher )
		{
			_comparer = comparer;
		}

		protected override void InsertItem( int index, T item )
		{
			int insertIndex = FindInsertIndex( item );
			base.InsertItem( insertIndex, item );
		}

		private int FindInsertIndex( T item )
		{
			if ( Count == 0 )
				return 0;

			List<T> baseList = base.Items as List<T>;
			if ( baseList == null )
				throw new NotSupportedException();

			return ~baseList.BinarySearch( item, _comparer );
		}
	}
}
