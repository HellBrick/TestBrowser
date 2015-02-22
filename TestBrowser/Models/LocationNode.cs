using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HellBrick.TestBrowser.Common;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Models
{
	public class LocationNode: INode
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
