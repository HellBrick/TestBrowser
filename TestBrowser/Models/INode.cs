using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HellBrick.TestBrowser.Common;

namespace HellBrick.TestBrowser.Models
{
	public interface INode
	{
		NodeType Type { get; }
		string Name { get; }
		string Key { get; }
		INode Parent { get; set; }
		ICollection<INode> Children { get; }
		ICollection<SafeGestureCommand> Commands { get; }
		bool IsVisible { get; }
		bool IsSelected { get; }
		bool IsExpanded { get; set; }
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

		public static IEnumerable<INode> EnumerateDescendants( this INode node )
		{
			foreach ( var child in node.Children )
			{
				yield return child;
				foreach ( var childDescendant in child.EnumerateDescendants() )
					yield return childDescendant;
			}
		}

		public static IEnumerable<INode> EnumerateDescendantsAndSelf( this INode node )
		{
			yield return node;
			foreach ( var ancestor in node.EnumerateDescendants() )
				yield return ancestor;
		}
	}
}
