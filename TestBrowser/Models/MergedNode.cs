using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using HellBrick.TestBrowser.Common;

namespace HellBrick.TestBrowser.Models
{
	public class MergedNode: PropertyChangedBase, INode
	{
		public MergedNode( LocationNode[] mergedNodes )
		{
			Nodes = mergedNodes.ToList();
			Name = String.Join( ".", mergedNodes.Select( n => n.Name ) );
		}

		public IReadOnlyList<LocationNode> Nodes { get; }

		#region INode Members

		public NodeType Type => NodeType.Location;
		public string Name { get; }
		public string Key => Name;

		public INode Parent
		{
			get { return Nodes[ 0 ].Parent; }
			set {}
		}

		public ICollection<INode> Children => Nodes[ Nodes.Count - 1 ].Children;
		public ICollection<SafeGestureCommand> Commands { get; } = new List<SafeGestureCommand>();
		public bool IsVisible => true;

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; NotifyOfPropertyChange( nameof( IsSelected ) ); }
		}

		private bool _isExpanded = true;
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set { _isExpanded = value; NotifyOfPropertyChange( nameof( IsExpanded ) ); }
		}

		#endregion
	}
}
