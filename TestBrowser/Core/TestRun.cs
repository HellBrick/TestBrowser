using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Controller;
using Microsoft.VisualStudio.TestWindow.Data;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace HellBrick.TestBrowser.Core
{
	public class TestRun: IDisposable
	{
		private TestRunRequest _runRequest;
		private TestServiceContext _serviceContext;
		private Dictionary<int, TestData> _testsToMonitor;

		#region Construction

		public TestRun( TestRunRequest runRequest, TestServiceContext serviceContext )
		{
			_runRequest = runRequest;
			_serviceContext = serviceContext;

			IEnumerable<TestData> testsToRun = EnumerateTestsTorun( runRequest ).Cast<TestData>();
			_testsToMonitor = testsToRun.ToDictionary( t => t.FastId );
			_runRequest.Internals().TestRunStatsChanged += OnTestRunStatsChanged;
		}

		private IEnumerable<ITest> EnumerateTestsTorun( TestRunRequest runRequest )
		{
			var reader = _serviceContext.Storage.ActiveUnitTestReader;

			if ( runRequest.TotalTestCount == reader.TotalTestCount )
				return reader.GetAllTests();
			else
			{
				var testCases = runRequest.Internals().TestRunConfig.Tests;
				return reader.GetAllTests( testCases.Select( tc => tc.Id ) );
			}
		}

		private void OnTestRunStatsChanged( object sender, TestRunRequestStats e )
		{
			List<TestData> finishedTests = FindNewFinishedTests();
			StopMonitoring( finishedTests );
			RaiseTestRunUpdated( finishedTests );
		}

		private List<TestData> FindNewFinishedTests()
		{
			List<TestData> finishedTests = new List<TestData>();

			foreach ( int fastID in _testsToMonitor.Keys )
			{
				var results = _serviceContext.Storage.ActiveUnitTestReader.GetResults( fastID );
				var finishedTestsForCurrentFastID = results
					.Select( r => r.TestData )
					.Where( t => !t.Stale && !t.IsCurrentlyRunning );

				finishedTests.AddRange( finishedTestsForCurrentFastID );
			}

			return finishedTests;
		}

		private void StopMonitoring<T>( T finishedTests ) where T: IEnumerable<TestData>
		{
			foreach ( var test in finishedTests )
				_testsToMonitor.Remove( test.FastId );
		}

		private void RaiseTestRunUpdated( List<TestData> finishedTests )
		{
			var handler = TestRunUpdated;
			if ( handler != null )
			{
				var finishedTestIDs = finishedTests.Select( t => t.Id ).ToList();
				var currentlyRunningTestIDs = _testsToMonitor.Values.Where( t => t.IsCurrentlyRunning ).Select( t => t.Id ).ToList();
				TestsRunUpdatedEventArgs updateEventArgs = new TestsRunUpdatedEventArgs( finishedTestIDs, currentlyRunningTestIDs );
				handler( this, updateEventArgs );
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			_runRequest.Internals().TestRunStatsChanged -= OnTestRunStatsChanged;
		}

		#endregion

		public event EventHandler<TestsRunUpdatedEventArgs> TestRunUpdated;
	}
}
