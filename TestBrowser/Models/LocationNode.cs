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
		private INode _currentPresenter;

		public LocationNode( SolutionTestBrowserModel testBrowser, SafeDispatcher dispatcher, string location, string name )
			: base( testBrowser, dispatcher )
		{
			_testBrowser = testBrowser;
			_dispatcher = dispatcher;
			Location = location;
			Name = name;
			_children = new NodeCollection( _dispatcher );
			_currentPresenter = this;
		}

		public string Location { get; set; }

		public bool TryRecalculatePresenter()
		{
			var nodesToMerge = this.EnumerateNodesToMerge().ToArray();
			bool shouldBeMerged = nodesToMerge.Length > 1;
			var mergedNode = _currentPresenter as MergedNode;
			var isMerged = mergedNode != null;

			bool isPresenterUpdated =
				isMerged && nodesToMerge.LastOrDefault() != mergedNode.Nodes.LastOrDefault() ||
				!isMerged && shouldBeMerged;

			if ( isPresenterUpdated )
			{
				_currentPresenter = shouldBeMerged ? CreateMergedPresenter( nodesToMerge ) : this;
				NotifyOfPropertyChange( nameof( Presenter ) );
			}

			return isPresenterUpdated;
		}

		private IEnumerable<LocationNode> EnumerateNodesToMerge()
		{
			foreach ( var node in this.EnumerateDescendantsAndSelf().OfType<LocationNode>() )
			{
				yield return node;

				if ( node.Children.Count > 1 )
					break;
			}
		}

		private INode CreateMergedPresenter( LocationNode[] nodesToMerge )
		{
			if ( nodesToMerge.Length < 2 )
				throw new InvalidOperationException( $"{nameof( CreateMergedPresenter )}() is not supposed to be called when there's nothing to merge." );

			return new MergedNode( _testBrowser, _dispatcher, nodesToMerge );
		}

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

		public override string ToString() => Location;

		#region RunnableNode members

		public override NodeType Type => NodeType.Location;
		public override string Name { get; }
		public override string Key => Name;
		public override INode Parent { get; set; }
		public override ICollection<INode> Children => _children;
		public override INode Presenter => _currentPresenter;
		public override bool IsVisible => true;

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

		private struct MergeableInterval
		{
			public MergeableInterval( LocationNode firstNode, LocationNode lastNode, int intervalLength )
			{
				FirstNode = firstNode;
				LastNode = lastNode;
				Length = intervalLength;
			}

			public readonly LocationNode FirstNode;
			public readonly LocationNode LastNode;
			public readonly int Length;
		}
	}
}
