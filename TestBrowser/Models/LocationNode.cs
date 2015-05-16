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
			set { _mergedNode = value; NotifyOfPropertyChange( () => IsVisible ); }
		}

		public bool IsMerged
		{
			get { return _mergedNode != null; }
		}

		public bool ShouldBeMerged
		{
			get { return Children.Count == 1 && Children[ 0 ].Type == NodeType.Location; }
		}

		public bool RequiresMerge
		{
			get { return !IsMerged && ShouldBeMerged; }
		}

		public bool IsLastMergedNode
		{
			get { return IsMerged && this == MergedNode.Nodes.Last(); }
		}

		public bool RequiresBreakUp
		{
			get { return IsMerged && !ShouldBeMerged && !IsLastMergedNode; }
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

		public string Key
		{
			get { return Name; }
		}

		public INode Parent { get; set; }
		public NodeCollection Children { get; private set; }

		ICollection<INode> INode.Children
		{
			get { return this.Children; }
		}

		public bool IsVisible
		{
			get { return !IsMerged || IsLastMergedNode; }
		}

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; NotifyOfPropertyChange( () => IsSelected ); }
		}

		private bool _isExpanded = true;
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set { _isExpanded = value; NotifyOfPropertyChange( () => IsExpanded ); }
		}

		#endregion
	}
}
