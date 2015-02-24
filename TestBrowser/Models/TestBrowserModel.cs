using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using HellBrick.TestBrowser.Common;
using HellBrick.TestBrowser.Core;
using Microsoft.VisualStudio.TestWindow.Controller;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace HellBrick.TestBrowser.Models
{
	public class TestBrowserModel: PropertyChangedBase, IDisposable
	{
		private TestServiceContext _serviceContext;
		private TestRun _currentTestRun;

		public TestBrowserModel( TestServiceContext serviceContext )
		{
			_serviceContext = serviceContext;
			_serviceContext.RequestFactory.StateChanged += OnStateChanged;

			TestTree = new Models.TestTree( _serviceContext.Dispatcher );
			InitializeCommands();
		}

		#region Global event handlers

		private void OnStateChanged( object sender, OperationStateChangedEventArgs e )
		{
			try
			{
				_serviceContext.Logger.Log( MessageLevel.Informational, e.ToString() );
				State = e.State;

				switch ( e.State )
				{
					case TestOperationStates.DiscoveryFinished:
						OnDiscoveryFinished( e );
						break;

					case TestOperationStates.TestExecutionFinished:
						OnRunFinished( e );
						break;

					case TestOperationStates.TestExecutionStarted:
						OnExecutionStarted( e );
						break;
				}
			}
			catch ( Exception ex )
			{
				_serviceContext.Logger.LogException( ex );
			}
		}

		private void OnDiscoveryFinished( OperationStateChangedEventArgs e )
		{
			using ( var reader = _serviceContext.Storage.ActiveUnitTestReader )
			{
				using ( var query = reader.GetAllTests() )
				{
					UpdateTestList( query );
				}
			}
		}

		private void OnRunFinished( OperationStateChangedEventArgs e )
		{
			if ( _currentTestRun != null )
			{
				_currentTestRun.TestRunUpdated -= OnTestsFinished;
				_currentTestRun.Dispose();
			}

			using ( var reader = _serviceContext.Storage.ActiveUnitTestReader )
			{
				using ( var query = reader.GetAllTests() )
				{
					var testsInLastRun = reader.GetTestsInLastRun( query );
					foreach ( var test in testsInLastRun )
						TestTree.Tests[ test.Id ].RaiseStateChanged();
				}
			}
		}

		private void OnExecutionStarted( OperationStateChangedEventArgs e )
		{
			TestRunRequest runRequest = e.Operation as TestRunRequest;
			if ( runRequest == null )
			{
				_serviceContext.Logger.Log(
					MessageLevel.Error,
					String.Format( "ExecutionStarted event with {0} operation ({1} expected)", e.Operation.GetType().Name, typeof( TestRunRequest ).Name ) );

				return;
			}
			
			_currentTestRun = new TestRun( runRequest, _serviceContext );
			_currentTestRun.TestRunUpdated += OnTestsFinished;
			MaxProgress = _currentTestRun.TestCount;
			CurrentProgress = 0;
			LastRunSummary = new RunSummary( _currentTestRun );

			//	The tests are stale now, it's a good reason to notify the UI about it.
			foreach ( var test in TestTree.Tests.Values )
				test.RaiseStateChanged();
		}

		private void OnTestsFinished( object sender, TestsRunUpdatedEventArgs e )
		{
			foreach ( var testID in Enumerable.Concat( e.FinishedTests, e.CurrentlyRunningTests ) )
				TestTree.Tests[ testID ].RaiseStateChanged();

			CurrentProgress += e.FinishedTests.Count;
			LastRunSummary.RaisePropertiesChanged();
		}

		private void UpdateTestList( IEnumerable<ITest> tests )
		{
			var newTestLookup = tests.ToDictionary( t => t.Id );

			var removedTests = TestTree.Tests.Where( t => !newTestLookup.ContainsKey( t.Key ) ).ToArray();
			foreach ( var testRecord in removedTests )
				TestTree.RemoveTest( testRecord.Value );

			var newTests = newTestLookup.Where( kvp => !TestTree.Tests.ContainsKey( kvp.Key ) );
			foreach ( var kvp in newTests )
				TestTree.InsertTest( new TestModel( kvp.Value ) );
		}

		#endregion

		#region Properties

		public TestTree TestTree { get; private set; }

		private RunSummary _lastRunSummary;
		public RunSummary LastRunSummary
		{
			get { return _lastRunSummary; }
			private set { _lastRunSummary = value; NotifyOfPropertyChange( () => LastRunSummary ); }
		}

		private TestOperationStates _state = TestOperationStates.None;
		public TestOperationStates State
		{
			get { return _state; }
			set
			{
				_state = value;
				base.NotifyOfPropertyChange( () => State );
				RefreshCommands();
				NotifyOfPropertyChange( () => IsDoingSomething );
				NotifyOfPropertyChange( () => IsDoingIndefiniteOperation );
			}
		}

		public bool IsDoingSomething
		{
			get { return !_serviceContext.RequestFactory.OperationSetFinished; }
		}

		public bool IsDoingIndefiniteOperation
		{
			get
			{
				switch ( State )
				{
					case TestOperationStates.TestExecutionStarted:
						return false;

					default:
						return true;
				}
			}
		}

		private int _maxProgress;
		public int MaxProgress
		{
			get { return _maxProgress; }
			set { _maxProgress = value; base.NotifyOfPropertyChange( () => MaxProgress ); }
		}

		private int _currentProgress;
		public int CurrentProgress
		{
			get { return _currentProgress; }
			set { _currentProgress = value; base.NotifyOfPropertyChange( () => CurrentProgress ); }
		}

		public List<SafeCommand> Commands { get; private set; }

		#endregion

		#region Commands

		private void InitializeCommands()
		{
			Commands = new List<SafeCommand>()
			{
				new SafeCommand( _serviceContext.Dispatcher, () => RunAll(), () => CanRunTests(), "Run all" ),
				new SafeCommand( _serviceContext.Dispatcher, () => DebugAll(), () => CanRunTests(), "Debug all" ),
				new SafeCommand( _serviceContext.Dispatcher, () => RunSelected(), () => CanRunTests(), "Run selected" ),
				new SafeCommand( _serviceContext.Dispatcher, () => DebugSelected(), () => CanRunTests(), "Debug selected" )
			};
		}

		private void RefreshCommands()
		{
			foreach ( var command in Commands )
				command.RaiseCanExecuteChanged();
		}

		private bool CanRunTests()
		{
			return _serviceContext.RequestFactory.OperationSetFinished;
		}

		private void RunAll()
		{
			_serviceContext.RequestFactory.ExecuteTestsAsync();
		}

		private void DebugAll()
		{
			_serviceContext.RequestFactory.DebugTestsAsync();
		}

		private void RunSelected()
		{
			_serviceContext.RequestFactory.ExecuteTestsAsync( EnumerateSelectedTestIDs(), configProvider => { } );
		}

		private void DebugSelected()
		{
			_serviceContext.RequestFactory.DebugTestsAsync( EnumerateSelectedTestIDs() );
		}

		private IEnumerable<Guid> EnumerateSelectedTestIDs()
		{
			return EnumerateSelectedTestIDs( TestTree );
		}

		private IEnumerable<Guid> EnumerateSelectedTestIDs( INode node )
		{
			if ( node.IsSelected )
			{
				//	If the node is selected, all its descendants are considered selected automatically.
				return node.EnumerateDescendantsAndSelf()
					.OfType<TestModel>()
					.Select( t => t.ID );
			}
			else
			{
				//	Otherwise, we have to recursively examine all its descendants personally.
				return node.Children.SelectMany( c => EnumerateSelectedTestIDs( c ) );
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			_serviceContext.RequestFactory.StateChanged -= OnStateChanged;
		}

		#endregion
	}
}
