﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using HellBrick.TestBrowser.Common;
using HellBrick.TestBrowser.Core;
using HellBrick.TestBrowser.Options;
using Microsoft.VisualStudio.TestWindow.Controller;
using Microsoft.VisualStudio.TestWindow.Data;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace HellBrick.TestBrowser.Models
{
	public sealed class SolutionTestBrowserModel : PropertyChangedBase, IDisposable
	{
		private readonly TestServiceContext _serviceContext;
		private TestBrowserOptions _options;
		private TestRun _currentTestRun;

		public SolutionTestBrowserModel( TestServiceContext serviceContext, TestBrowserOptions options )
		{
			_serviceContext = serviceContext;
			InitializeSettings( options );
			_serviceContext.RequestFactory.StateChanged += OnStateChanged;

			TestTree = new Models.TestTree( this, _serviceContext.Dispatcher, _options.CollapsedNodes );
			InitializeCommands();
			var discoverTask = DiscoverTestsAsync();    //	 no need to await it
		}

		private void InitializeSettings( TestBrowserOptions options )
		{
			_options = options ?? new TestBrowserOptions();
			Settings = new SettingsModel( _options );
			Settings.PropertyChanged += OnSettingsChanged;
		}

		private async Task DiscoverTestsAsync()
		{
			await _serviceContext.WaitForBuildAsync().ConfigureAwait( false );
			DiscoverAllOrRunOnInitializeOperation discoverOperation = new DiscoverAllOrRunOnInitializeOperation( _serviceContext.OperationData, false );
			await _serviceContext.ExecuteOperationAsync( discoverOperation ).ConfigureAwait( false );
		}

		#region Global event handlers

		private void OnSettingsChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
		{
			if ( e.PropertyName == nameof( SettingsModel.HumanizeTestNames ) )
			{
				foreach ( var humanizableNode in TestTree.RootNode.EnumerateDescendants().OfType<IHumanizable>() )
				{
					humanizableNode.HumanizeName = Settings.HumanizeTestNames;
				}
			}
		}

		private void OnStateChanged( object sender, OperationStateChangedEventArgs e )
		{
			try
			{
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
				_serviceContext.Logger.Log( MessageLevel.Error, ex.ToString() );
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

				//	When the debugging session is terminated (Shift + F5),
				//	the currently running tests are stuck in the 'currently running' state.
				var testsStuckInRunningState = _currentTestRun.CurentlyRunningTestIDs.Select( id => TestTree.Tests[ id ] );
				foreach ( var test in testsStuckInRunningState )
					test.RaiseStateChanged();

				_currentTestRun.Dispose();
			}
		}

		private void OnExecutionStarted( OperationStateChangedEventArgs e )
		{
			TestRunRequest runRequest = e.Operation as TestRunRequest;
			if ( runRequest == null )
			{
				_serviceContext.Logger.Log(
					MessageLevel.Error,
					$"ExecutionStarted event with {e.Operation.GetType().Name} operation ({typeof( TestRunRequest ).Name} expected)" );

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
			{
				TestModel test = testRecord.Value;
				test.SelectionChanged -= OnTestSelectionChanged;
				TestTree.RemoveTest( test );
			}

			var newTests = newTestLookup.Where( kvp => !TestTree.Tests.ContainsKey( kvp.Key ) );
			foreach ( var kvp in newTests )
			{
				TestModel test = new TestModel( this, kvp.Value as TestData, _serviceContext ) { HumanizeName = Settings.HumanizeTestNames };
				test.SelectionChanged += OnTestSelectionChanged;
				TestTree.InsertTest( test );
			}
		}

		private void OnTestSelectionChanged( TestModel sender, EventArgs e )
		{
			if ( !sender.IsSelected && SelectedTest == sender )
				SelectedTest = null;

			if ( sender.IsSelected && SelectedTest != sender )
				SelectedTest = sender;
		}

		#endregion

		#region Properties

		public TestTree TestTree { get; private set; }

		private RunSummary _lastRunSummary;
		public RunSummary LastRunSummary
		{
			get { return _lastRunSummary; }
			private set { _lastRunSummary = value; NotifyOfPropertyChange( nameof( LastRunSummary ) ); }
		}

		private TestOperationStates _state = TestOperationStates.None;
		public TestOperationStates State
		{
			get { return _state; }
			set
			{
				_state = value;
				base.NotifyOfPropertyChange( nameof( State ) );
				RefreshCommands();
				NotifyOfPropertyChange( nameof( IsDoingSomething ) );
				NotifyOfPropertyChange( nameof( IsDoingIndefiniteOperation ) );
			}
		}

		public bool IsDoingSomething => !_serviceContext.RequestFactory.OperationSetFinished;

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
			set { _maxProgress = value; base.NotifyOfPropertyChange( nameof( MaxProgress ) ); }
		}

		private int _currentProgress;
		public int CurrentProgress
		{
			get { return _currentProgress; }
			set { _currentProgress = value; base.NotifyOfPropertyChange( nameof( CurrentProgress ) ); }
		}

		private TestModel _selectedTest;
		public TestModel SelectedTest
		{
			get { return _selectedTest; }
			set { _selectedTest = value; base.NotifyOfPropertyChange( nameof( SelectedTest ) ); }
		}

		public List<SafeCommand> Commands { get; private set; }
		public SettingsModel Settings { get; private set; }

		#endregion

		#region Commands

		public event EventHandler CanRunTestsChanged;

		private void InitializeCommands()
		{
			Commands = new List<SafeCommand>()
			{
				new SafeCommand( _serviceContext.Dispatcher, () => RunSelected(), () => CanRunTests(), "Run" ),
				new SafeCommand( _serviceContext.Dispatcher, () => DebugSelected(), () => CanRunTests(), "Debug" ),
				new SafeCommand( _serviceContext.Dispatcher, () => Cancel(), () => CanCancel(), "Cancel" )
			};
		}

		private void RefreshCommands()
		{
			_serviceContext.Dispatcher.Invoke( () => CanRunTestsChanged?.Invoke( this, EventArgs.Empty ) );
			foreach ( var command in Commands )
				command.RaiseCanExecuteChanged();
		}

		public bool CanRunTests() => _serviceContext.RequestFactory.OperationSetFinished && TestTree.VisualChildren.Count > 0;

		private void RunSelected()
		{
			INode selectedNode = FindSelectedNode();
			RunNode( selectedNode );
		}

		public void RunNode( INode selectedNode )
		{
			if ( ShouldInvokeFullRun( selectedNode ) )
				_serviceContext.ExecuteOperationAsync( new RunAllOperation( _serviceContext.OperationData ) { ShowTestWindowAfterRun = false } );
			else
				_serviceContext.RequestFactory.ExecuteTestsAsync( EnumerateSelectedTestIDs( selectedNode ), configProvider => { } );
		}

		private void DebugSelected()
		{
			INode selectedNode = FindSelectedNode();
			DebugNode( selectedNode );
		}

		public void DebugNode( INode selectedNode )
		{
			if ( ShouldInvokeFullRun( selectedNode ) )
				_serviceContext.RequestFactory.DebugTestsAsync();
			else
				_serviceContext.RequestFactory.DebugTestsAsync( EnumerateSelectedTestIDs( selectedNode ) );
		}

		private INode FindSelectedNode() => TestTree.RootNode.EnumerateDescendantsAndSelf().FirstOrDefault( n => n.IsSelected );

		private bool ShouldInvokeFullRun( INode selectedNode ) => selectedNode == null || selectedNode == TestTree;

		private IEnumerable<Guid> EnumerateSelectedTestIDs( INode selectedNode )
		{
			//	If the node is selected, all its descendants are considered selected automatically.
			return selectedNode.EnumerateDescendantsAndSelf()
				.OfType<TestModel>()
				.Select( t => t.ID );
		}

		private void Cancel() => _serviceContext.RequestFactory.Cancel();
		private bool CanCancel() => _serviceContext.RequestFactory.CanCancel;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			_serviceContext.RequestFactory.StateChanged -= OnStateChanged;
			Settings.PropertyChanged -= OnSettingsChanged;
			_currentTestRun?.Dispose();
		}

		#endregion
	}
}
