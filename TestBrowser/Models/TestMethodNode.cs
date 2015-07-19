using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using HellBrick.TestBrowser.Common;
using Humanizer;
using Microsoft.VisualStudio.TestWindow.Controller;

namespace HellBrick.TestBrowser.Models
{
	public class TestMethodNode : RunnableNode, IHumanizable
	{
		public TestMethodNode( SolutionTestBrowserModel testBrowser, SafeDispatcher dispatcher, string methodName )
			: base( testBrowser, dispatcher )
		{
			_children = new NodeCollection( dispatcher );
			_originalMethodName = methodName;
			_humanizedMethodName = _originalMethodName.Humanize();
		}

		public override string ToString() => Name;

		#region RunnableNode members

		public override NodeType Type => NodeType.Method;

		private readonly string _originalMethodName;
		private readonly string _humanizedMethodName;
		public override string Name => HumanizeName ? _humanizedMethodName : _originalMethodName;

		public override string Key => _originalMethodName;

		public override INode Parent { get; set; }

		private readonly NodeCollection _children;
		public override ICollection<INode> Children => _children;

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
