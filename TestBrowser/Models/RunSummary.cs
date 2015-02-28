﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using HellBrick.TestBrowser.Core;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace HellBrick.TestBrowser.Models
{
	public class RunSummary: PropertyChangedBase
	{
		private TestRun _testRun;

		public RunSummary( TestRun testRun )
		{
			_testRun = testRun;
		}

		public void RaisePropertiesChanged()
		{
			NotifyOfPropertyChange( () => IsRunning );
			NotifyOfPropertyChange( () => TestsPassed );
			NotifyOfPropertyChange( () => TestsFailed );
		}

		#region Properties

		public bool IsRunning
		{
			get { return _testRun.Stats.ResultCount < _testRun.TestCount; }
		}

		public int TestsPassed
		{
			get { return _testRun.Stats.Stats[ TestState.Passed ]; }
		}

		public int TestsFailed
		{
			get { return _testRun.Stats.Stats[ TestState.Failed ]; }
		}

		public int TestCount
		{
			get { return _testRun.TestCount; }
		}

		#endregion
	}
}