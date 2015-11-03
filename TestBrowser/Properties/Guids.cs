// Guids.cs
// MUST match guids.h
using System;

namespace HellBrick.TestBrowser
{
	internal static class GuidList
	{
		public const string guidTestBrowserPkgString = "70cb3239-bde6-4a23-9c90-8d355765613b";
		public const string guidTestBrowserCmdSetString = "b937e73a-73e4-4c3f-914e-8743d9a7fd5e";
		public const string guidToolWindowPersistanceString = "2cf8fca7-b42d-4640-828c-c13246d59e9c";

		public static readonly Guid guidTestBrowserCmdSet = new Guid( guidTestBrowserCmdSetString );
	};
}