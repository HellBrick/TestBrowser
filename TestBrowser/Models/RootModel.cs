using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using EnvDTE;
using HellBrick.TestBrowser.Core;
using HellBrick.TestBrowser.Options;

namespace HellBrick.TestBrowser.Models
{
	public class RootModel : PropertyChangedBase, IDisposable
	{
		private readonly TestServiceContext _testServiceContext;
		private readonly IOptionsService _optionsService;
		private readonly DTE _dte;

		public RootModel( TestServiceContext testServiceContext, IOptionsService optionsService, DTE dte )
		{
			_testServiceContext = testServiceContext;
			_optionsService = optionsService;
			_dte = dte;

			_dte.Events.SolutionEvents.Opened += CreateTestBrowser;
			_dte.Events.SolutionEvents.BeforeClosing += SaveOptionsAndDisposeTestBrowser;
			_dte.Events.DTEEvents.OnBeginShutdown += SaveOptionsAndDisposeTestBrowser;

			if ( !String.IsNullOrEmpty( _dte.Solution.FullName ) )
				CreateTestBrowser();
		}

		private SolutionTestBrowserModel _solutionBrowser;
		public SolutionTestBrowserModel SolutionBrowser
		{
			get { return _solutionBrowser; }
			set { _solutionBrowser = value; NotifyOfPropertyChange( nameof( SolutionBrowser ) ); }
		}

		public void Dispose()
		{
			_dte.Events.SolutionEvents.BeforeClosing -= SaveOptionsAndDisposeTestBrowser;
			_dte.Events.DTEEvents.OnBeginShutdown -= SaveOptionsAndDisposeTestBrowser;
			_dte.Events.SolutionEvents.Opened -= CreateTestBrowser;
		}

		private void CreateTestBrowser()
		{
			SolutionBrowser = new SolutionTestBrowserModel( _testServiceContext, _optionsService.LoadOptions() );
		}

		private void SaveOptionsAndDisposeTestBrowser()
		{
			TestBrowserOptions options = new TestBrowserOptions()
			{
				HumanizeTestNames = SolutionBrowser.Settings.HumanizeTestNames,
				CollapsedNodes = SolutionBrowser.TestTree
					.EnumerateDescendantsAndSelf()
					.Where( n => n.IsVisible && !n.IsExpanded )
					.Select( n => new NodeKey( n.Type, n.Key ) )
					.ToList()
			};

			_optionsService.SaveOptions( options );
			SolutionBrowser?.Dispose();
			SolutionBrowser = null;
		}
	}
}
