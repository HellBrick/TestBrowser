using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Common
{
	public class SafeObservableDictionary<TKey, TValue>: SafeObservableCollection<TValue>
	{
		private readonly Func<TValue, TKey> _keyExtractor;
		private readonly Dictionary<TKey, TValue> _lookup = new Dictionary<TKey, TValue>();

		public SafeObservableDictionary( SafeDispatcher dispatcher, Func<TValue, TKey> keyExtractor )
			: base( dispatcher )
		{
			_keyExtractor = keyExtractor;
		}

		#region SafeObservableCollection<TValue> members

		protected override void ClearItems()
		{
			_lookup.Clear();
			base.ClearItems();
		}

		protected override void InsertItem( int index, TValue item )
		{
			_lookup.Add( _keyExtractor( item ), item );
			base.InsertItem( index, item );
		}

		protected override void RemoveItem( int index )
		{
			var item = base[ index ];
			_lookup.Remove( _keyExtractor( item ) );
			base.RemoveItem( index );
		}

		protected override void SetItem( int index, TValue item )
		{
			var oldItem = base[ index ];
			_lookup.Remove( _keyExtractor( oldItem ) );
			_lookup.Add( _keyExtractor( item ), item );
			base.SetItem( index, item );
		}

		#endregion

		public TValue this[ TKey key ] => _lookup[ key ];
		public bool ContainsKey( TKey key ) => _lookup.ContainsKey( key );
	}
}
