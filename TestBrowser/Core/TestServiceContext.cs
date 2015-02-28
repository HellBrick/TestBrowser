using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Controller;
using Microsoft.VisualStudio.TestWindow.Data;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.TestWindow.Host;

namespace HellBrick.TestBrowser.Core
{
	[Export]
	public class TestServiceContext
	{
		[ImportingConstructor]
		public TestServiceContext( IRequestFactory requestFactory, IUnitTestStorage storage, SafeDispatcher safeDispatcher, ILogger logger, OperationData operationData, IHost host )
		{
			RequestFactory = requestFactory;
			Storage = storage;
			Dispatcher = safeDispatcher;
			Logger = logger;
			OperationData = operationData;
			Host = host;
		}

		public IRequestFactory RequestFactory { get; private set; }
		public IUnitTestStorage Storage { get; private set; }
		public SafeDispatcher Dispatcher { get; set; }
		public ILogger Logger { get; private set; }
		public OperationData OperationData { get; private set; }
		public IHost Host { get; set; }

		/// <summary>
		/// This is the way OperationBroker.EnqueueOperation implemented.
		/// </summary>
		public Task<bool> ExecuteOperationAsync( Operation operation )
		{
			RequestFactory.Initialize();
			return OperationData.EnqueueOperation( operation );
		}
	}
}
