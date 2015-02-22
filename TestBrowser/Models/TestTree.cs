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
			Children = new SafeObservableCollection<INode>( _dispatcher );
		}

		#region INode Members

		public INode Parent
		{
			get { return null; }
			set { }
		}

		ICollection<INode> INode.Children
		{
			get { return this.Children; }
		}

		public SafeObservableCollection<INode> Children { get; private set; }

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

		public void RemoveTest( TestModel test )
		{
			LocationNode locationNode = _locationLookup[ test.Location ];
			RemoveChildRecursive( locationNode, test );
			_testLookup.Remove( test.ID );
		}

		private void RemoveChildRecursive( INode parent, INode child )
		{
			child.Parent = null;
			parent.Children.Remove( child );

			LocationNode locationNode = child as LocationNode;
			if ( locationNode != null )
				_locationLookup.Remove( locationNode.Location );

			if ( parent.Children.Count == 0 && parent.Parent != null )
				RemoveChildRecursive( parent.Parent, parent );
		}
	}
}
