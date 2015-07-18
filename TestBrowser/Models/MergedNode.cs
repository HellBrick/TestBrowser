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
	public class MergedNode : RunnableNode
	{
		public MergedNode( SolutionTestBrowserModel testBrowser, SafeDispatcher dispatcher, LocationNode[] mergedNodes )
			: base( testBrowser, dispatcher )
		{
			Nodes = mergedNodes.ToList();
			Name = String.Join( ".", mergedNodes.Select( n => n.Name ) );
		}

		public IReadOnlyList<LocationNode> Nodes { get; }

		#region RunnableNode members

		public override NodeType Type => NodeType.Location;
		public override string Name { get; }
		public override string Key => Name;

		public override INode Parent
		{
			get { return Nodes[ 0 ].Parent; }
			set {}
		}

		public override ICollection<INode> Children => Nodes[ Nodes.Count - 1 ].Children;
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
	}
}
