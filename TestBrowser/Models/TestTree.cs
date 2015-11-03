using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using HellBrick.TestBrowser.Common;
using HellBrick.TestBrowser.Options;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Models
{
	public class TestTree : PropertyChangedBase
	{
		private readonly Dictionary<string, LocationNode> _locationLookup = new Dictionary<string, LocationNode>();
		private readonly Dictionary<string, TestMethodNode> _methodLookup = new Dictionary<string, TestMethodNode>();
		private readonly SolutionTestBrowserModel _testBrowser;
		private readonly AllTestsNode _rootNode;
		private readonly HashSet<NodeKey> _autoCollapsedNodes;
		private readonly SafeDispatcher _dispatcher;

		public TestTree( SolutionTestBrowserModel testBrowser, SafeDispatcher dispatcher, IEnumerable<NodeKey> autoCollapsedNodes )
		{
			_testBrowser = testBrowser;
			_dispatcher = dispatcher;
			_autoCollapsedNodes = new HashSet<NodeKey>( autoCollapsedNodes );
			_rootNode = new AllTestsNode( testBrowser, dispatcher );
			_rootCollectionWrapper = new NodeCollection( _dispatcher );
			_rootCollectionWrapper.Add( _rootNode );
		}

		private readonly Dictionary<Guid, TestModel> _testLookup = new Dictionary<Guid, TestModel>();
		public IReadOnlyDictionary<Guid, TestModel> Tests => _testLookup;

		private readonly NodeCollection _rootCollectionWrapper;
		public ICollection<INode> VisualChildren => _rootNode.Children.Count < 2 ? _rootNode.Children : _rootCollectionWrapper;
		public AllTestsNode RootNode => _rootNode;

		public void InsertTest( TestModel newTest )
		{
			_testLookup.Add( newTest.ID, newTest );

			LocationNode locationNode;
			if ( !_locationLookup.TryGetValue( newTest.Location, out locationNode ) )
				locationNode = CreateLocationNode( newTest.Location );

			INode insertionNode = locationNode;
			if ( newTest.TestCaseName != null )
			{
				string key = MethodNodeKey( locationNode, newTest.MethodName );
				TestMethodNode methodNode;
				if ( !_methodLookup.TryGetValue( key, out methodNode ) )
					methodNode = CreateMethodNode( locationNode, key, newTest.MethodName, newTest.HumanizeName );

				insertionNode = methodNode;
			}

			insertionNode.InsertChild( newTest );
			OptimizeTreeAfterChangeAt( locationNode );
		}

		public void RemoveTest( TestModel test )
		{
			INode parent = test.Parent;
			RemoveChild( parent, test );
			_testLookup.Remove( test.ID );
		}

		private const char _separator = '.';

		private void InsertChildAndTryAutoExpand( INode parent, INode child )
		{
			parent.InsertChild( child );
			if ( _autoCollapsedNodes.Remove( new NodeKey( child.Type, child.Key ) ) )
				child.IsExpanded = false;
		}

		private LocationNode CreateLocationNode( string location )
		{
			string[] locationFragments = location.Split( _separator );
			StringBuilder currentLocationBuilder = new StringBuilder( location.Length );
			INode currentParent = _rootNode;

			for ( int i = 0; i < locationFragments.Length; i++ )
			{
				if ( currentLocationBuilder.Length > 0 )
					currentLocationBuilder.Append( _separator );

				currentLocationBuilder.Append( locationFragments[ i ] );
				string currentLocation = currentLocationBuilder.ToString();

				LocationNode currentLocationNode;
				if ( !_locationLookup.TryGetValue( currentLocation, out currentLocationNode ) )
				{
					currentLocationNode = new LocationNode( _testBrowser, _dispatcher, currentLocation, locationFragments[ i ] );
					_locationLookup[ currentLocation ] = currentLocationNode;
					InsertChildAndTryAutoExpand( currentParent, currentLocationNode );
					UpdateRootVisibilityAfterChangeAt( currentParent );
				}

				currentParent = currentLocationNode;
			}

			return currentParent as LocationNode;
		}

		private string MethodNodeKey( LocationNode location, string methodName ) => location.Location + methodName;

		private TestMethodNode CreateMethodNode( LocationNode locationNode, string key, string methodName, bool humanizeTestName )
		{
			TestMethodNode methodNode = new TestMethodNode( _testBrowser, _dispatcher, methodName ) { HumanizeName = humanizeTestName };
			InsertChildAndTryAutoExpand( locationNode, methodNode );
			_methodLookup.Add( key, methodNode );
			return methodNode;
		}

		private void RemoveChild( INode parent, INode child )
		{
			while ( parent != null )
			{
				//	Dictionaries must be cleaned up BEFORE severing connection between the parent and the child
				LocationNode locationNode = child as LocationNode;
				if ( locationNode != null )
					_locationLookup.Remove( locationNode.Location );

				TestMethodNode methodNode = child as TestMethodNode;
				if ( methodNode != null )
					_methodLookup.Remove( MethodNodeKey( ( methodNode.Parent as LocationNode ), methodNode.Name ) );

				child.Parent = null;
				parent.Children.Remove( child );

				//	If the parent doesn't have any more children, we should remove it as well.
				if ( parent.Children.Count == 0 )
				{
					child = parent;
					parent = parent.Parent;
				}
				else
					break;
			}

			var survivingLocation = parent as LocationNode;
			if ( survivingLocation != null )
				OptimizeTreeAfterChangeAt( survivingLocation );

			UpdateRootVisibilityAfterChangeAt( parent );
		}

		/// <param name="locationNode">The parent of the node that has just been inserted into or removed from the tree.</param>
		private void OptimizeTreeAfterChangeAt( LocationNode locationNode )
		{
			foreach ( var node in locationNode.EnumerateAncestorsAndSelf().OfType<LocationNode>() )
				node.TryRecalculatePresenter();
		}

		private void UpdateRootVisibilityAfterChangeAt( INode parent )
		{
			if ( parent == _rootNode )
				NotifyOfPropertyChange( nameof( VisualChildren ) );
		}
	}
}
