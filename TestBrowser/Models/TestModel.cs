using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using HellBrick.TestBrowser.Common;
using HellBrick.TestBrowser.Core;
using Microsoft.VisualStudio.TestWindow.Controller;
using Microsoft.VisualStudio.TestWindow.Data;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.TestWindow.Model;

namespace HellBrick.TestBrowser.Models
{
	public class TestModel: PropertyChangedBase, INode
	{
		private TestServiceContext _serviceContext;
		private TestData _test;

		public TestModel( TestData test, TestServiceContext serviceContext )
		{
			_serviceContext = serviceContext;
			_test = test;

			Location = _test.FullyQualifiedName.Substring( 0, _test.FullyQualifiedName.Length - _test.DisplayName.Length - 1 );
			InitializeCommands();
		}

		public string Location { get; private set; }
		public TestState State { get { return _test.State; } }
		public Guid ID { get { return _test.Id; } }
		public bool IsStale { get { return _test.Stale; } }
		public bool IsCurrentlyRunning { get { return _test.IsCurrentlyRunning; } }
		public Result Result
		{
			get
			{
				var testResultData = _test.Results.FirstOrDefault() as TestResultData;
				if (testResultData == null)
					return null;

				return _serviceContext.TestObjectFactory.CreateResult( testResultData );
			}
		}

		public event EventHandler<TestModel, EventArgs> SelectionChanged;
		private void RaiseSelectionChanged()
		{
			var handler = SelectionChanged;
			if ( handler != null )
				handler( this, EventArgs.Empty );
		}

		public void RaiseStateChanged()
		{
			base.NotifyOfPropertyChange( () => State );
			base.NotifyOfPropertyChange( () => IsStale );
			base.NotifyOfPropertyChange( () => IsCurrentlyRunning );
			base.NotifyOfPropertyChange( () => Result );
		}

		public override string ToString()
		{
			return String.Format( "[{0}] {1}/{2}", State, Location, Name );
		}

		#region INode Members

		public NodeType Type
		{
			get { return NodeType.Test; }
		}

		public string Name { get { return _test.DisplayName; } }
		public INode Parent { get; set; }

		private List<INode> _emptyList = new List<INode>();
		public ICollection<INode> Children
		{
			get { return _emptyList; }
		}

		public bool IsVisible
		{
			get { return true; }
		}

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; NotifyOfPropertyChange( () => IsSelected ); RaiseSelectionChanged(); }
		}

		#endregion

		#region Commands

		private void InitializeCommands()
		{
			GoToTestCommand = new SafeCommand( _serviceContext.Dispatcher, () => GoToTest(), "Go to test" );
		}

		public SafeCommand GoToTestCommand { get; private set; }

		private void GoToTest()
		{
			_serviceContext.Host.Open( new TestOpenTarget( _test as TestData ) );
		}

		#endregion

		private struct TestOpenTarget: IOpenTarget
		{
			private TestData _testData;

			public TestOpenTarget( TestData testData )
			{
				_testData = testData;
			}

			#region IOpenTarget Members

			public bool Enabled
			{
				get { return !String.IsNullOrEmpty( FilePath ); }
			}

			public string FileName
			{
				get { return Path.GetFileName( FilePath ); }
			}

			public string FilePath
			{
				get { return _testData.FilePath; }
			}

			public int LineNumber
			{
				get { return _testData.LineNumber; }
			}

			public string Name
			{
				get { return _testData.FullyQualifiedName; }
			}

			#endregion
		}

	}
}
