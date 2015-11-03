using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using HellBrick.TestBrowser.Common;
using HellBrick.TestBrowser.Core;
using Humanizer;
using Microsoft.VisualStudio.TestWindow.Controller;
using Microsoft.VisualStudio.TestWindow.Data;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.TestWindow.Model;

namespace HellBrick.TestBrowser.Models
{
	public class TestModel : RunnableNode, IHumanizable
	{
		private readonly TestServiceContext _serviceContext;
		private readonly TestData _test;
		private readonly string _humanizedMethodName;

		public TestModel( SolutionTestBrowserModel testBrowser, TestData test, TestServiceContext serviceContext )
			: base( testBrowser, serviceContext.Dispatcher )
		{
			_serviceContext = serviceContext;
			_test = test;

			Location = _test.Namespace + "." + _test.ClassName;
			ParseMethodNameAndTestCase();
			_humanizedMethodName = MethodName.Humanize();
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

					//	It's possible for the case name to be empty if someone used parameterless [TestCase] attribute for whatever reason.
					//	If this is the case, we prefer to treat this as a simple test method without any cases, so the case name is set to null.
					TestCaseName = testName.Substring( openBracketIndex + 1, closeBracketIndex - openBracketIndex - 1 ).NullIfEmpty();
					return true;
				}
			}

			return false;
		}

		public string MethodName { get; private set; }
		public string TestCaseName { get; private set; }
		public string Location { get; private set; }

		public TestState State => _test.State;
		public Guid ID => _test.Id;
		public bool IsStale => _test.Stale;
		public bool IsCurrentlyRunning => _test.IsCurrentlyRunning;

		public Result[] Results
		{
			get { return _test.Results.Select( r => _serviceContext.TestObjectFactory.CreateResult( r ) ).ToArray(); }
		}

		public bool HasResults => _test.Results.Count > 0;

		public event EventHandler<TestModel, EventArgs> SelectionChanged;
		private void RaiseSelectionChanged()
		{
			var handler = SelectionChanged;
			if ( handler != null )
				handler( this, EventArgs.Empty );
		}

		public void RaiseStateChanged()
		{
			base.NotifyOfPropertyChange( nameof( State ) );
			base.NotifyOfPropertyChange( nameof( IsStale ) );
			base.NotifyOfPropertyChange( nameof( IsCurrentlyRunning ) );
			base.NotifyOfPropertyChange( nameof( Results ) );
			base.NotifyOfPropertyChange( nameof( HasResults ) );
		}

		public override string ToString() => $"[{State}] {Location}/{Name}";

		#region RunnableNode members

		public override NodeType Type => NodeType.Test;
		public override string Name => TestCaseName ?? ( HumanizeName ? _humanizedMethodName : MethodName );
		public override string Key => TestCaseName ?? MethodName;

		public override INode Parent { get; set; }

		public override ICollection<INode> Children { get; } = new List<INode>();
		public override INode Presenter => this;

		private bool _isSelected;
		public override bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; NotifyOfPropertyChange( nameof( IsSelected ) ); RaiseSelectionChanged(); }
		}

		public override bool IsExpanded
		{
			get { return true; }
			set { }
		}

		protected override IEnumerable<IGestureCommand> EnumerateCommands()
		{
			foreach ( var baseCommand in base.EnumerateCommands() )
				yield return baseCommand;

			yield return new SafeGestureCommand(
				_serviceContext.Dispatcher, () => GoToTest(), "Go to test",
				new KeyGesture( System.Windows.Input.Key.F12 ),
				new MouseGesture( MouseAction.LeftDoubleClick ) );
		}

		private void GoToTest()
		{
			_serviceContext.Host.Open( new TestOpenTarget( _test ) );
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

		private struct TestOpenTarget : IOpenTarget, IEquatable<TestOpenTarget>
		{
			private readonly TestData _testData;

			public TestOpenTarget( TestData testData )
			{
				_testData = testData;
			}

			#region IOpenTarget Members

			public bool Enabled => !String.IsNullOrEmpty( FilePath );
			public string FileName => Path.GetFileName( FilePath );
			public string FilePath => _testData.FilePath;
			public int LineNumber => _testData.LineNumber;
			public string Name => _testData.FullyQualifiedName;

			#endregion

			#region IEquatable<TestOpenTarget>

			public override int GetHashCode() => EqualityComparer<TestData>.Default.GetHashCode( _testData );
			public bool Equals( TestOpenTarget other ) => EqualityComparer<TestData>.Default.Equals( _testData, other._testData );
			public override bool Equals( object obj ) => obj is TestOpenTarget && Equals( (TestOpenTarget) obj );

			public static bool operator ==( TestOpenTarget x, TestOpenTarget y ) => x.Equals( y );
			public static bool operator !=( TestOpenTarget x, TestOpenTarget y ) => !x.Equals( y );

			#endregion
		}
	}
}
