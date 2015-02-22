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

		private LocationNode CreateLocationNode( string location )
		{
			LocationNode locationNode = new LocationNode( _dispatcher, location, location );
			this.InsertChild( locationNode );
			_locationLookup[ location ] = locationNode;

			return locationNode;
		}

		public void RemoveTest( TestModel test )
		{
			LocationNode locationNode = _locationLookup[ test.Location ];
			RemoveChildRecursive( locationNode, test );
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
