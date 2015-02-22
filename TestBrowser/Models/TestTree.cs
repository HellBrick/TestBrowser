using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HellBrick.TestBrowser.Common;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Models
{
	public class TestTree: INode
	{
		private Dictionary<string, LocationNode> _locationLookup = new Dictionary<string, LocationNode>();
		private SafeDispatcher _dispatcher;

		public TestTree( SafeDispatcher dispatcher )
		{
			_dispatcher = dispatcher;
			Children = new NodeCollection( _dispatcher );
		}

		#region INode Members

		public NodeType Type
		{
			get { return NodeType.Location; }
		}

		public string Name
		{
			get { return "[Root]"; }
		}

		public INode Parent
		{
			get { return null; }
			set { }
		}

		ICollection<INode> INode.Children
		{
			get { return this.Children; }
		}

		public NodeCollection Children { get; private set; }

		public bool IsVisible
		{
			get { return true; }
		}

		#endregion

		private Dictionary<Guid, TestModel> _testLookup = new Dictionary<Guid, TestModel>();
		public IReadOnlyDictionary<Guid, TestModel> Tests
		{
			get { return _testLookup; }
		}

		public void InsertTest( TestModel newTest )
		{
			_testLookup.Add( newTest.ID, newTest );

			LocationNode locationNode;
			if ( !_locationLookup.TryGetValue( newTest.Location, out locationNode ) )
				locationNode = CreateLocationNode( newTest.Location );

			locationNode.InsertChild( newTest );
			OptimizeTreeBranchAfterInsert( locationNode );
		}

		public void RemoveTest( TestModel test )
		{
			LocationNode locationNode = _locationLookup[ test.Location ];
			RemoveChild( locationNode, test );
			_testLookup.Remove( test.ID );
		}

		private const char _separator = '.';

		private LocationNode CreateLocationNode( string location )
		{
			string[] locationFragments = location.Split( _separator );
			StringBuilder currentLocationBuilder = new StringBuilder( location.Length );
			INode currentParent = this;

			for ( int i = 0; i < locationFragments.Length; i++ )
			{
				if ( currentLocationBuilder.Length > 0 )
					currentLocationBuilder.Append( _separator );

				currentLocationBuilder.Append( locationFragments[ i ] );
				string currentLocation = currentLocationBuilder.ToString();

				LocationNode currentLocationNode;
				if ( !_locationLookup.TryGetValue( currentLocation, out currentLocationNode ) )
				{
					currentLocationNode = new LocationNode( _dispatcher, currentLocation, locationFragments[ i ] );
					_locationLookup[ currentLocation ] = currentLocationNode;
					currentParent.InsertChild( currentLocationNode );
				}

				currentParent = currentLocationNode;
			}

			return currentParent as LocationNode;
		}

		private void RemoveChild( INode parent, INode child )
		{
			while ( parent != null )
			{
				child.Parent = null;
				parent.Children.Remove( child );

				LocationNode locationNode = child as LocationNode;
				if ( locationNode != null )
					_locationLookup.Remove( locationNode.Location );

				//	If the parent doesn't have any more children, we should remove it as well.
				if ( parent.Children.Count == 0 )
				{
					child = parent;
					parent = parent.Parent;
				}
				else
					break;
			}
		}

		/// <param name="locationNode">The parent of the node that has just been inserted into the tree.</param>
		private void OptimizeTreeBranchAfterInsert( LocationNode locationNode )
		{
			//	1. The insertion might have occurred in the middle of a merged node.
			//	If this is the case, it has to be broken.
			BreakMergeIfNeeded( locationNode );

			//	2. New testless nodes might have been added (or appear after the break-up).
			//	If this is the case, they have to be merged.
			MergeAncestorsIfNeeded( locationNode );
		}

		private void BreakMergeIfNeeded( LocationNode locationNode )
		{
			//	Break-up may be needed anywhere in the ancestor branch.
			//	e.g. inserting into HellBrick.TestBrowser.Whatever breaks HellBrick.TestBrowser.Models.
			foreach ( var node in locationNode.EnumerateAncestorsAndSelf().OfType<LocationNode>() )
			{
				if ( node.RequiresBreakUp )
					BreakMerge( node.MergedNode );
			}
		}

		private void BreakMerge( MergedNode mergedNode )
		{
			RemoveChild( mergedNode.Parent, mergedNode );
			foreach ( var node in mergedNode.Nodes )
				node.MergedNode = null;
		}

		private void MergeAncestorsIfNeeded( LocationNode locationNode )
		{
			MergeableInterval? intervalToMerge = FindIntervalToMergeUpFrom( locationNode );
			if ( intervalToMerge.HasValue )
				MergeNodes( intervalToMerge.Value );
		}

		private MergeableInterval? FindIntervalToMergeUpFrom( LocationNode startingNode )
		{
			LocationNode firstNodeToMerge = null;
			LocationNode lastNodeToMerge = null;
			int intervalLength = 0;

			foreach ( var node in startingNode.EnumerateAncestorsAndSelf().OfType<LocationNode>() )
			{
				if ( firstNodeToMerge == null )
				{
					// We're searching for the first mergeable node.
					// 1. If the current node *is not* mergeable, than we need to keep searching.
					//    This also means that the current node may be the last one with any direct test children.
					//	2. If the current node *is* mergeable, we've found the last two nodes of the mergeable block.
					//    The previous node is the last node to merge and we keep bubbling up to find which node is really the first to merge.
					if ( !node.RequiresMerge )
						lastNodeToMerge = node;
					else
					{
						firstNodeToMerge = node;
						intervalLength = 2;
					}
				}
				else
				{
					//	We're searching for the first node that no longer requires merge.
					//	When we find it, the previous node is the first one to merge, so we break.
					//	Otherwise, keep bubbling up.
					if ( !node.RequiresMerge )
						break;
					else
					{
						firstNodeToMerge = node;
						intervalLength++;
					}
				}
			}

			if ( firstNodeToMerge == null )
				return null;

			return new MergeableInterval( firstNodeToMerge, lastNodeToMerge, intervalLength );
		}

		private void MergeNodes( MergeableInterval intervalToMerge )
		{
			LocationNode[] nodes = new LocationNode[ intervalToMerge.Length ];
			int i = intervalToMerge.Length - 1;
			foreach ( var node in intervalToMerge.LastNode.EnumerateAncestorsAndSelf().Cast<LocationNode>().Take( intervalToMerge.Length ) )
				nodes[ i-- ] = node;

			MergedNode mergedNode = new MergedNode( nodes );
			foreach ( var node in nodes )
				node.MergedNode = mergedNode;

			nodes[ 0 ].Parent.InsertChild( mergedNode );
		}

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
