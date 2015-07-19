using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Models
{
	public class AllTestsNode : RunnableNode
	{
		private readonly NodeCollection _children;

		public AllTestsNode( SolutionTestBrowserModel testBrowser, SafeDispatcher dispatcher )
			: base( testBrowser, dispatcher )
		{
			_children = new NodeCollection( dispatcher );
		}

		#region RunnableNode members

		public override string Key => Name;
		public override string Name => "All tests";
		public override NodeType Type => NodeType.Location;
		public override ICollection<INode> Children => _children;
		public override INode Presenter => this;

		public override bool IsSelected { get; set; }

		public override INode Parent
		{
			get { return null; }
			set { }
		}

		public override bool IsExpanded
		{
			get { return true; }
			set { }
		}

		#endregion
	}
}
