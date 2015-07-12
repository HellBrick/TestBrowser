using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using HellBrick.TestBrowser.Core;
using System.ComponentModel.Composition.Hosting;
using HellBrick.TestBrowser.Models;
using System.IO;
using HellBrick.TestBrowser.Options;
using EnvDTE80;
using EnvDTE;

namespace HellBrick.TestBrowser
{
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
	// a package.
	[PackageRegistration( UseManagedResourcesOnly = true )]
	// This attribute is used to register the information needed to show this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration( "#110", "#112", "1.0", IconResourceID = 400 )]
	// This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideMenuResource( "Menus.ctmenu", 1 )]
	// This attribute registers a tool window exposed by this package.
	[ProvideToolWindow( typeof( TestBrowserToolWindow ), Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Right, Window = ToolWindowGuids.SolutionExplorer )]
	[Guid( GuidList.guidTestBrowserPkgString )]
	public sealed class TestBrowserPackage : Package, IOptionsService
	{
		private TestServiceContext _serviceContext;
		private TestBrowserOptions _options;

		public TestBrowserPackage()
		{
		}

		public static RootModel RootModel { get; private set; }

		#region IOptionsService

		TestBrowserOptions IOptionsService.LoadOptions()
		{
			var service = this.GetService<IVsSolutionPersistence>();
			service.LoadPackageUserOpts( this, TestBrowserOptions.OptionStreamKey );
			return _options ?? new TestBrowserOptions();
		}

		void IOptionsService.SaveOptions( TestBrowserOptions options )
		{
			_options = options;
			var service = this.GetService<IVsSolutionPersistence>();
			service.SavePackageUserOpts( this, TestBrowserOptions.OptionStreamKey );
		}

		protected override void OnLoadOptions( string key, Stream stream )
		{
			base.OnLoadOptions( key, stream );

			if ( key != TestBrowserOptions.OptionStreamKey )
				return;

			_options = TestBrowserOptions.FromStream( stream );
		}

		protected override void OnSaveOptions( string key, Stream stream )
		{
			base.OnSaveOptions( key, stream );

			if ( key != TestBrowserOptions.OptionStreamKey )
				return;

			_options.WriteToStream( stream );
		}

		#endregion

		#region Package Members

		protected override void Initialize()
		{
			base.Initialize();
			InitializeServiceContext();
			try
			{
				InitializeViewModels();
				InitializeCommands();
			}
			catch ( Exception ex )
			{
				_serviceContext.Logger.Log( Microsoft.VisualStudio.TestWindow.Extensibility.MessageLevel.Error, ex.ToString() );
			}
		}

		private void InitializeServiceContext()
		{
			IComponentModel service = this.GetService<IComponentModel>( typeof( SComponentModel ) );
			if ( service == null )
				throw new InvalidOperationException( $"Can't get {nameof( SComponentModel )} service" );

			var typeCatalog = new TypeCatalog( typeof( TestServiceContext ) );
			var definition = typeCatalog.FirstOrDefault();
			var part = definition.CreatePart();
			service.DefaultCompositionService.SatisfyImportsOnce( part );
			_serviceContext = part.GetExportedValue( part.ExportDefinitions.FirstOrDefault() ) as TestServiceContext;
		}

		private void InitializeViewModels()
		{
			RootModel = new RootModel( _serviceContext, this, this.GetService<DTE>() );
		}

		private void InitializeCommands()
		{
			// Add our command handlers for menu (commands must exist in the .vsct file)
			OleMenuCommandService mcs = GetService( typeof( IMenuCommandService ) ) as OleMenuCommandService;
			if ( null != mcs )
			{
				// Create the command for the tool window
				CommandID toolwndCommandID = new CommandID( GuidList.guidTestBrowserCmdSet, (int) PkgCmdIDList.TestBrowserOpenCommand );
				MenuCommand menuToolWin = new MenuCommand( ShowToolWindow, toolwndCommandID );
				mcs.AddCommand( menuToolWin );
			}
		}

		private void ShowToolWindow( object sender, EventArgs e )
		{
			// Get the instance number 0 of this tool window. This window is single instance so this instance
			// is actually the only one.
			// The last flag is set to true so that if the tool window does not exists it will be created.
			ToolWindowPane window = this.FindToolWindow( typeof( TestBrowserToolWindow ), 0, true );
			if ( ( null == window ) || ( null == window.Frame ) )
			{
				throw new NotSupportedException( Resources.CanNotCreateWindow );
			}
			IVsWindowFrame windowFrame = (IVsWindowFrame) window.Frame;
			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure( windowFrame.Show() );
		}

		#endregion

		protected override int QueryClose( out bool canClose )
		{
			RootModel.Dispose();
			return base.QueryClose( out canClose );
		}
	}
}
