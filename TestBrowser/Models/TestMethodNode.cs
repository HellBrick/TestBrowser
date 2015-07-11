using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Models
{
	public class TestMethodNode: PropertyChangedBase, INode, IHumanizable
	{
		public TestMethodNode( SafeDispatcher dispatcher, string methodName )
		{
			_children = new NodeCollection( dispatcher );
			_originalMethodName = methodName;
			_humanizedMethodName = Humanizer.StringHumanizeExtensions.Humanize( _originalMethodName );
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

		private string _originalMethodName;
		private string _humanizedMethodName;
		public string Name
		{
			get { return HumanizeName ? _humanizedMethodName : _originalMethodName; }
		}

		public string Key
		{
			get { return _originalMethodName; }
		}

		public INode Parent { get; set; }

		private NodeCollection _children;
		public ICollection<INode> Children { get { return _children; } }

		public bool IsVisible { get { return true; } }

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; NotifyOfPropertyChange( nameof( IsSelected ) ); }
		}

		private bool _isExpanded = true;
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set { _isExpanded = value; NotifyOfPropertyChange( nameof( IsExpanded ) ); }
		}

		#endregion

		#region IHumanizable Members

		private bool _humanizeName = true;
		public bool HumanizeName
		{
			get { return _humanizeName; }
			set
			{
				_humanizeName = value;
				NotifyOfPropertyChange( nameof( Name ) );
			}
		}

		#endregion
	}
}
