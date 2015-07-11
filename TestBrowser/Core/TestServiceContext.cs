using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Controller;
using Microsoft.VisualStudio.TestWindow.Data;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.TestWindow.Host;
using Microsoft.VisualStudio.TestWindow.Model;

namespace HellBrick.TestBrowser.Core
{
	[Export]
	public class TestServiceContext
	{
		private static readonly Task<bool> _completedTask = Task.FromResult( true );

		[ImportingConstructor]
		public TestServiceContext(
			IRequestFactory requestFactory, IUnitTestStorage storage, SafeDispatcher safeDispatcher, ILogger logger, OperationData operationData, IHost host,
			ITestObjectFactory testObjectFactory )
		{
			RequestFactory = requestFactory;
			Storage = storage;
			Dispatcher = safeDispatcher;
			Logger = logger;
			OperationData = operationData;
			Host = host;
			TestObjectFactory = testObjectFactory;
		}

		public IRequestFactory RequestFactory { get; private set; }
		public IUnitTestStorage Storage { get; private set; }
		public SafeDispatcher Dispatcher { get; set; }
		public ILogger Logger { get; private set; }
		public OperationData OperationData { get; private set; }
		public IHost Host { get; set; }
		public ITestObjectFactory TestObjectFactory { get; set; }

		/// <summary>
		/// This is the way OperationBroker.EnqueueOperation implemented.
		/// </summary>
		public Task<bool> ExecuteOperationAsync( Operation operation )
		{
			RequestFactory.Initialize();
			return OperationData.EnqueueOperation( operation );
		}

		public Task<bool> WaitForBuildAsync()
		{
			if ( !Host.IsBuildInProgress )
				return _completedTask;

			WaitForBuildOperation operation = WaitForBuildOperationFactory.Create( OperationData, Host );
			return ExecuteOperationAsync( operation );
		}

		private static class WaitForBuildOperationFactory
		{
			#region CodeGen

			private static readonly Func<IOperationData, IHost, WaitForBuildOperation> _factoryMethod;

			static WaitForBuildOperationFactory()
			{
				_factoryMethod = BuildFactoryMethod();
			}

			private static Func<IOperationData, IHost, WaitForBuildOperation> BuildFactoryMethod()
			{
				ParameterExpression operationData = Expression.Parameter( typeof( IOperationData ), "operationData" );
				ParameterExpression host = Expression.Parameter( typeof( IHost ), "host" );
				ConstructorInfo constructor = typeof( WaitForBuildOperation ).GetConstructor(
					BindingFlags.Instance | BindingFlags.NonPublic,
					null,
					new Type[] { typeof( IOperationData ), typeof( IHost ) },
					null );

				var constructorCall = Expression.New( constructor, operationData, host );
				var lambda = Expression.Lambda<Func<IOperationData, IHost, WaitForBuildOperation>>( constructorCall, operationData, host );
				return lambda.Compile();
			}

			#endregion

			public static WaitForBuildOperation Create( IOperationData operationData, IHost host )
			{
				return _factoryMethod( operationData, host );
			}
		}
	}
}
