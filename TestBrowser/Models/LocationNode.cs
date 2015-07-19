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
	public class LocationNode : RunnableNode
	{
		private readonly SolutionTestBrowserModel _testBrowser;
		private readonly SafeDispatcher _dispatcher;
		private readonly NodeCollection _children;

		public LocationNode( SolutionTestBrowserModel testBrowser, SafeDispatcher dispatcher, string location, string name )
			: base( testBrowser, dispatcher )
		{
			_testBrowser = testBrowser;
			_dispatcher = dispatcher;
			Location = location;
			Name = name;
			_children = new NodeCollection( _dispatcher );
		}

		public string Location { get; set; }

		private MergedNode _mergedNode;
		public MergedNode MergedNode
		{
			get { return _mergedNode; }
			set { _mergedNode = value; NotifyOfPropertyChange( nameof( IsVisible ) ); }
		}

		public bool IsMerged => _mergedNode != null;

		/// <remarks>
		/// The node should be merged if it has only 1 child and this child is a <see cref="LocationNode"/>.
		/// However, if the node's visual child is a merged node, it will actually have *2* children in the tree: <see cref="Models.MergedNode"/> and a hidden <see cref="LocationNode"/>.
		/// </remarks>
		public bool ShouldBeMerged
		{
			get
			{
				bool singleNodeIsFound = false;

				foreach ( var node in _children )
				{
					if ( node is MergedNode )
						continue;

					if ( node is LocationNode )
					{
						if ( singleNodeIsFound )
							return false;

						singleNodeIsFound = true;
					}
					else
						return false;
				}

				return singleNodeIsFound;
			}
		}

		public bool RequiresMerge => !IsMerged && ShouldBeMerged;

		public bool IsLastMergedNode => IsMerged && this == MergedNode.Nodes.Last();

		public bool RequiresBreakUp => IsMerged && !ShouldBeMerged && !IsLastMergedNode;

		public override string ToString() => Location;

		#region RunnableNode members

		public override NodeType Type => NodeType.Location;
		public override string Name { get; }
		public override string Key => Name;
		public override INode Parent { get; set; }
		public override ICollection<INode> Children => _children;
		public override INode Presenter => this;
		public override bool IsVisible => !IsMerged || IsLastMergedNode;

		private bool _isSelected;
		public override bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; NotifyOfPropertyChange( nameof( IsSelected ) ); }
		}

		private bool _isExpanded = true;
		public override bool IsExpanded
		{
			get { return _isExpanded; }
			set { _isExpanded = value; NotifyOfPropertyChange( nameof( IsExpanded ) ); }
		}

		#endregion
	}
}
