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
			TestList = new SafeObservableDictionary<Guid, TestModel>( serviceContext.Dispatcher, test => test.ID );

			_serviceContext = serviceContext;
			_serviceContext.RequestFactory.StateChanged += OnStateChanged;

			State = TestOperationStates.None;
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
						TestList[ test.Id ].RaiseStateChanged();
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

			//	The tests are stale now, it's a good reason to notify the UI about it.
			foreach ( var test in TestList )
				test.RaiseStateChanged();
		}

		private void OnTestsFinished( object sender, TestsRunUpdatedEventArgs e )
		{
			foreach ( var testID in Enumerable.Concat( e.FinishedTests, e.CurrentlyRunningTests ) )
				TestList[ testID ].RaiseStateChanged();
		}

		private void UpdateTestList( IEnumerable<ITest> tests )
		{
			var newTestLookup = tests.ToDictionary( t => t.Id );

			var removedTests = TestList.Where( t => !newTestLookup.ContainsKey( t.ID ) ).ToArray();
			foreach ( var test in removedTests )
				TestList.Remove( test );

			var newTests = newTestLookup.Where( kvp => !TestList.ContainsKey( kvp.Key ) );
			foreach ( var kvp in newTests )
				TestList.Add( new TestModel( kvp.Value ) );
		}

		#endregion

		#region Properties

		public SafeObservableDictionary<Guid, TestModel> TestList { get; private set; }

		private TestOperationStates _state;
		public TestOperationStates State
		{
			get { return _state; }
			set { _state = value; base.NotifyOfPropertyChange( () => State ); }
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
