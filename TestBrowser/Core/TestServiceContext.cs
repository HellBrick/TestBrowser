using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Controller;
using Microsoft.VisualStudio.TestWindow.Data;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace HellBrick.TestBrowser.Core
{
	[Export]
	public class TestServiceContext
	{
		[ImportingConstructor]
		public TestServiceContext( IRequestFactory requestFactory, IUnitTestStorage storage, ILogger logger )
		{
			RequestFactory = requestFactory;
			Storage = storage;
			Logger = logger;
		}

		public IRequestFactory RequestFactory { get; private set; }
		public IUnitTestStorage Storage { get; private set; }
		public ILogger Logger { get; private set; }
	}
}
