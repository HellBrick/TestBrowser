using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Controller;
using Microsoft.VisualStudio.TestWindow.Data;

namespace HellBrick.TestBrowser.Core
{
	internal static class TestRunRequestExtensions
	{
		public static TestRunRequestInternals Internals( this TestRunRequest request ) => new TestRunRequestInternals( request );

		private static class CodeGen
		{
			public static Func<TestRunRequest, TestRunConfiguration> TestRunConfigAccessor;
			public static Func<EventHandler<TestRunRequestStats>, Delegate> InternalHandlerFactory;
			public static Dictionary<EventHandler<TestRunRequestStats>, Delegate> DelegateMap;
			public static EventInfo Event;

			private static readonly Type _eventArgsType;
			private static readonly Type _eventHandlerType;

			static CodeGen()
			{
				TestRunConfigAccessor = BuildTestRunConfigAccessor();

				DelegateMap = new Dictionary<EventHandler<TestRunRequestStats>, Delegate>();
				Event = typeof( TestRunRequest ).GetEvent( "TestRunStatsChanged", BindingFlags.NonPublic | BindingFlags.Instance );
				_eventHandlerType = Event.EventHandlerType;
				_eventArgsType = _eventHandlerType.GetGenericArguments()[ 0 ];
				InternalHandlerFactory = BuildInternalHandlerFactory();
			}

			/// <summary>
			/// Builds the following delegate:
			/// <code>( TestRunRequest request ) => request.TestRunConfig</code>
			/// </summary>
			/// <returns></returns>
			private static Func<TestRunRequest, TestRunConfiguration> BuildTestRunConfigAccessor()
			{
				ParameterExpression request = Expression.Parameter( typeof( TestRunRequest ), "request" );
				PropertyInfo property = typeof( TestRunRequest ).GetProperty( "TestRunConfig", BindingFlags.Instance | BindingFlags.NonPublic );

				Expression<Func<TestRunRequest, TestRunConfiguration>> lambda = Expression.Lambda<Func<TestRunRequest, TestRunConfiguration>>(
					Expression.Property( request, property ),
					request );

				return lambda.Compile();
			}

			/// <summary>
			/// Builds the following delegate:
			/// <code>
			/// <para>( EventHandler&lt;TestRunRequestStats&gt; handler ) =></para>
			/// <para>{</para>
			/// <para>return ( object sender, TestRunStatsChangedEventArgs args ) => handler( sender, args.TestRunStatistics );</para>
			/// <para>}</para>
			/// </code>
			/// </summary>
			/// <returns></returns>
			private static Func<EventHandler<TestRunRequestStats>, Delegate> BuildInternalHandlerFactory()
			{
				ParameterExpression handler = Expression.Parameter( typeof( EventHandler<TestRunRequestStats> ), "handler" );
				ParameterExpression sender = Expression.Parameter( typeof( object ), "sender" );
				ParameterExpression args = Expression.Parameter( _eventArgsType, "args" );

				ParameterExpression result = Expression.Parameter( _eventHandlerType, "result" );

				Expression body = Expression.Block(
					_eventHandlerType,
					new ParameterExpression[] { result },
					Expression.Assign(
						result,
						Expression.Lambda(
							_eventHandlerType,
							Expression.Invoke( handler, sender, Expression.Property( args, "TestRunStatistics" ) ),
							sender, args ) ),
					result );

				Type factoryMethodType = typeof( Func<,> ).MakeGenericType( typeof( EventHandler<TestRunRequestStats> ), _eventHandlerType );
				LambdaExpression factoryMethodLambda = Expression.Lambda( factoryMethodType, body, handler );
				return (Func<EventHandler<TestRunRequestStats>, Delegate>) factoryMethodLambda.Compile();
			}
		}

		internal struct TestRunRequestInternals
		{
			private readonly TestRunRequest _instance;

			public TestRunRequestInternals( TestRunRequest instance )
			{
				_instance = instance;
			}

			public TestRunConfiguration TestRunConfig => CodeGen.TestRunConfigAccessor( _instance );

			public event EventHandler<TestRunRequestStats> TestRunStatsChanged
			{
				add
				{
					Delegate internalHandler = CodeGen.InternalHandlerFactory( value );
					CodeGen.DelegateMap[ value ] = internalHandler;
					CodeGen.Event.GetAddMethod( nonPublic: true ).Invoke( _instance, new object[] { internalHandler } );
				}
				remove
				{
					Delegate internalHandler = CodeGen.DelegateMap[ value ];
					CodeGen.Event.GetRemoveMethod( nonPublic: true ).Invoke( _instance, new object[] { internalHandler } );
				}
			}
		}
	}
}
