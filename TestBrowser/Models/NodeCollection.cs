using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HellBrick.TestBrowser.Common;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Models
{
	public class NodeCollection: OrderedSafeObservableCollection<INode>
	{
		private static readonly IComparer<INode> _comparer = new NodeComparer();

		public NodeCollection( SafeDispatcher dispatcher )
			: base( dispatcher, _comparer )
		{
		}

		private class NodeComparer: IComparer<INode>
		{
			#region IComparer<INode> Members

			public int Compare( INode x, INode y )
			{
				int result = 0;

				result = ( (int) x.Type ).CompareTo( (int) y.Type );
				if ( result != 0 )
					return result;

				return x.Name.CompareTo( y.Name );
			}

			#endregion
		}
	}
}
