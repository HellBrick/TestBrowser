using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HellBrick.TestBrowser.Core
{
	public class TestsRunUpdatedEventArgs: EventArgs
	{
		public TestsRunUpdatedEventArgs( IReadOnlyCollection<Guid> finishedTests, IReadOnlyCollection<Guid> currentlyRunningTests )
		{
			FinishedTests = finishedTests;
			CurrentlyRunningTests = currentlyRunningTests;
		}

		public IReadOnlyCollection<Guid> FinishedTests { get; }
		public IReadOnlyCollection<Guid> CurrentlyRunningTests { get; }
	}
}
