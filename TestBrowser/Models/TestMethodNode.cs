using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Models
{
	public class TestMethodNode: PropertyChangedBase, INode
	{
		public TestMethodNode( SafeDispatcher dispatcher, string methodName )
		{
			_children = new NodeCollection( dispatcher );
			Name = methodName;
		}

		public override string ToString()
		{
			return Name;
		}

		#region INode Members

		public NodeType Type
		{
			get { return NodeType.Method; }
		}

		public string Name { get; private set; }

		public INode Parent { get; set; }

		private NodeCollection _children;
		public ICollection<INode> Children { get { return _children; } }

		public bool IsVisible { get { return true; } }

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; NotifyOfPropertyChange( () => IsSelected ); }
		}

		private bool _isExpanded;
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set { _isExpanded = value; NotifyOfPropertyChange( () => IsExpanded ); }
		}

		#endregion
	}
}
