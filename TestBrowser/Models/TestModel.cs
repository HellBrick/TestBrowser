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
	public class TestModel: PropertyChangedBase, INode, IHumanizable
	{
		private TestServiceContext _serviceContext;
		private TestData _test;
		private string _humanizedMethodName;

		public TestModel( TestData test, TestServiceContext serviceContext )
		{
			_serviceContext = serviceContext;
			_test = test;

			Location = _test.Namespace + "." + _test.ClassName;
			ParseMethodNameAndTestCase();
			_humanizedMethodName = Humanizer.StringHumanizeExtensions.Humanize( MethodName );
			InitializeCommands();
		}

		private void ParseMethodNameAndTestCase()
		{
			//	xUnit test cases
			if ( _test.ExecutorUri.Contains( "xunit" ) && TryParseMethodNameAndTestCase( _test.DisplayName ) )
				return;

			//	nUnit test cases
			if ( _test.ExecutorUri.Contains( "nunit" ) && TryParseMethodNameAndTestCase( _test.FullyQualifiedName ) )
				return;

			//	Default behaviour (nUnit/MSTest w/o a test case)
			TestCaseName = null;
			MethodName = _test.DisplayName;

			//	xUnit uses fully qualified name here => the location has to be trimmed
			int dotIndex = MethodName.LastIndexOf( '.' );
			if ( dotIndex > 0 )
				MethodName = MethodName.Substring( dotIndex + 1 );
		}

		private bool TryParseMethodNameAndTestCase( string testName )
		{
			int openBracketIndex = testName.IndexOf( '(' );
			if ( openBracketIndex > 0 )
			{
				int closeBracketIndex = testName.LastIndexOf( ')' );
				if ( closeBracketIndex > 0 )
				{
					//	Fully qualified name contains Location + extra '.' in the beginning, which has to be skipped
					int charsToSkip = Location.Length + 1;

					MethodName = testName.Substring( charsToSkip, openBracketIndex - charsToSkip );
					TestCaseName = testName.Substring( openBracketIndex + 1, closeBracketIndex - openBracketIndex - 1 );
					return true;
				}
			}

			return false;
		}

		public string MethodName { get; private set; }
		public string TestCaseName { get; private set; }
		public string Location { get; private set; }
		public TestState State { get { return _test.State; } }
		public Guid ID { get { return _test.Id; } }
		public bool IsStale { get { return _test.Stale; } }
		public bool IsCurrentlyRunning { get { return _test.IsCurrentlyRunning; } }

		public Result[] Results
		{
			get { return _test.Results.Select( r => _serviceContext.TestObjectFactory.CreateResult( r ) ).ToArray(); }
		}

		public bool HasResults
		{
			get { return _test.Results.Count > 0; }
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
			base.NotifyOfPropertyChange( () => Results );
			base.NotifyOfPropertyChange( () => HasResults );
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

		public string Name { get { return TestCaseName ?? ( HumanizeName ? _humanizedMethodName : MethodName ); } }

		public string Key
		{
			get { return TestCaseName ?? MethodName; }
		}

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
			_serviceContext.Host.Open( new TestOpenTarget( _test ) );
		}

		public bool IsExpanded
		{
			get { return true; }
			set { }
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
				NotifyOfPropertyChange( () => Name );
			}
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
