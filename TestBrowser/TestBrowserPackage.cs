using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

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
	[ProvideToolWindow( typeof( MyToolWindow ) )]
	[Guid( GuidList.guidTestBrowserPkgString )]
	public sealed class TestBrowserPackage: Package
	{
		public TestBrowserPackage()
		{
		}

		#region Package Members

		protected override void Initialize()
		{
			Debug.WriteLine( string.Format( CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString() ) );
			base.Initialize();

			// Add our command handlers for menu (commands must exist in the .vsct file)
			OleMenuCommandService mcs = GetService( typeof( IMenuCommandService ) ) as OleMenuCommandService;
			if ( null != mcs )
			{
				// Create the command for the tool window
				CommandID toolwndCommandID = new CommandID( GuidList.guidTestBrowserCmdSet, (int) PkgCmdIDList.cmdidTestBrowserTool );
				MenuCommand menuToolWin = new MenuCommand( ShowToolWindow, toolwndCommandID );
				mcs.AddCommand( menuToolWin );
			}
		}

		private void ShowToolWindow( object sender, EventArgs e )
		{
			// Get the instance number 0 of this tool window. This window is single instance so this instance
			// is actually the only one.
			// The last flag is set to true so that if the tool window does not exists it will be created.
			ToolWindowPane window = this.FindToolWindow( typeof( MyToolWindow ), 0, true );
			if ( ( null == window ) || ( null == window.Frame ) )
			{
				throw new NotSupportedException( Resources.CanNotCreateWindow );
			}
			IVsWindowFrame windowFrame = (IVsWindowFrame) window.Frame;
			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure( windowFrame.Show() );
		}

		#endregion
	}
}
