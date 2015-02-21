// Guids.cs
// MUST match guids.h
using System;

namespace HellBrick.TestBrowser
{
	static class GuidList
	{
		public const string guidTestBrowserPkgString = "95628654-1ff6-4434-bb0e-d90a65c763f0";
		public const string guidTestBrowserCmdSetString = "bc1b1326-1b3d-49fa-a255-b22ba976873f";
		public const string guidToolWindowPersistanceString = "a771a04c-f8c6-4f54-8bf1-2e64e7cdd770";

		public static readonly Guid guidTestBrowserCmdSet = new Guid( guidTestBrowserCmdSetString );
	};
}