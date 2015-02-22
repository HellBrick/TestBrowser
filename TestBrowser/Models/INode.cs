using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HellBrick.TestBrowser.Models
{
	public interface INode
	{
		INode Parent { get; set; }
		ICollection<INode> Children { get; }
	}

	public static class NodeExtensions
	{
		public static void InsertChild( this INode parent, INode child )
		{
			parent.Children.Add( child );
			child.Parent = parent;
		}
	}
}
