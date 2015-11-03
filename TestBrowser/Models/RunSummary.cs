using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using HellBrick.TestBrowser.Core;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace HellBrick.TestBrowser.Models
{
	public class RunSummary : PropertyChangedBase
	{
		private readonly TestRun _testRun;

		public RunSummary( TestRun testRun )
		{
			_testRun = testRun;
		}

		public void RaisePropertiesChanged()
		{
			NotifyOfPropertyChange( nameof( IsRunning ) );
			NotifyOfPropertyChange( nameof( TestsPassed ) );
			NotifyOfPropertyChange( nameof( TestsFailed ) );
		}

		#region Properties

		public bool IsRunning => _testRun.Stats.ResultCount < _testRun.TestCount;
		public int TestsPassed => _testRun.Stats.Stats[ TestState.Passed ];
		public int TestsFailed => _testRun.Stats.Stats[ TestState.Failed ];
		public int TestCount => _testRun.TestCount;

		#endregion
	}
}
