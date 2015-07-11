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
			set { _mergedNode = value; NotifyOfPropertyChange( nameof( IsVisible ) ); }
		}

		public bool IsMerged => _mergedNode != null;

		public bool ShouldBeMerged => Children.Count == 1 && Children[ 0 ].Type == NodeType.Location;

		public bool RequiresMerge => !IsMerged && ShouldBeMerged;

		public bool IsLastMergedNode => IsMerged && this == MergedNode.Nodes.Last();

		public bool RequiresBreakUp => IsMerged && !ShouldBeMerged && !IsLastMergedNode;

		public override string ToString() => Location;

		#region INode Members

		public NodeType Type => NodeType.Location;

		public string Name { get; }

		public string Key => Name;

		public INode Parent { get; set; }
		public NodeCollection Children { get; private set; }

		ICollection<INode> INode.Children => this.Children;

		public bool IsVisible => !IsMerged || IsLastMergedNode;

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
