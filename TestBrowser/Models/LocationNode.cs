using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using HellBrick.TestBrowser.Common;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Models
{
	public class LocationNode: PropertyChangedBase, INode
	{
		private SafeDispatcher _dispatcher;

		public LocationNode( SafeDispatcher dispatcher, string location, string name )
		{
			_dispatcher = dispatcher;
			Location = location;
			Name = name;
			Children = new NodeCollection( _dispatcher );
		}

		public string Location { get; set; }

		private MergedNode _mergedNode;
		public MergedNode MergedNode
		{
			get { return _mergedNode; }
			set { _mergedNode = value; NotifyOfPropertyChange( () => IsMerged ); }
		}

		public bool IsMerged
		{
			get { return _mergedNode != null; }
		}

		public bool ShouldBeMerged
		{
			get { return Children.Count == 1 && Children[ 0 ].Type != NodeType.Test; }
		}

		public bool RequiresMerge
		{
			get { return !IsMerged && ShouldBeMerged; }
		}

		public bool RequiresBreakUp
		{
			get { return IsMerged && !ShouldBeMerged && this != MergedNode.Nodes.Last(); }
		}

		public override string ToString()
		{
			return Location;
		}

		#region INode Members

		public NodeType Type
		{
			get { return NodeType.Location; }
		}

		public string Name { get; private set; }
		public INode Parent { get; set; }
		public NodeCollection Children { get; private set; }

		ICollection<INode> INode.Children
		{
			get { return this.Children; }
		}

		#endregion
	}
}
