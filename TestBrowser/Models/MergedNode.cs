using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace HellBrick.TestBrowser.Models
{
	public class MergedNode: PropertyChangedBase, INode
	{
		public MergedNode( LocationNode[] mergedNodes )
		{
			Nodes = mergedNodes.ToList();
			Name = String.Join( ".", mergedNodes.Select( n => n.Name ) );
		}

		public IReadOnlyList<LocationNode> Nodes { get; private set; }

		#region INode Members

		public NodeType Type
		{
			get { return NodeType.Location; }
		}

		public string Name { get; private set; }

		public INode Parent
		{
			get { return Nodes[ 0 ].Parent; }
			set {}
		}

		public ICollection<INode> Children
		{
			get { return Nodes[ Nodes.Count - 1 ].Children; }
		}

		public bool IsVisible
		{
			get { return true; }
		}

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; NotifyOfPropertyChange( () => IsSelected ); }
		}

		#endregion
	}
}
