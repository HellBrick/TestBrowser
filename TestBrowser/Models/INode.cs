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
		NodeType Type { get; }
		string Name { get; }
		INode Parent { get; set; }
		ICollection<INode> Children { get; }
		bool IsVisible { get; }
	}

	public static class NodeExtensions
	{
		public static void InsertChild( this INode parent, INode child )
		{
			parent.Children.Add( child );
			child.Parent = parent;
		}

		public static IEnumerable<INode> EnumerateAncestors( this INode node )
		{
			while ( node.Parent != null )
			{
				yield return node.Parent;
				node = node.Parent;
			}
		}

		public static IEnumerable<INode> EnumerateAncestorsAndSelf( this INode node )
		{
			yield return node;
			foreach ( var ancestor in node.EnumerateAncestors() )
				yield return ancestor;
		}
	}
}
