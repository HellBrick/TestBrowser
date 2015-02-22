﻿using System;
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
		public static TestRunRequestInternals Internals( this TestRunRequest request )
		{
			return new TestRunRequestInternals( request );
		}

		internal struct TestRunRequestInternals
		{
			#region Code generation

			private static Func<TestRunRequest, TestRunConfiguration> _testRunConfigAccessor;

			private static Func<EventHandler<TestRunRequestStats>, Delegate> _internalHandlerFactory;
			private static EventInfo _event;
			private static Type _eventArgsType;
			private static Type _eventHandlerType;
			private static Dictionary<EventHandler<TestRunRequestStats>, Delegate> _delegateMap;

			static TestRunRequestInternals()
			{
				_testRunConfigAccessor = BuildTestRunConfigAccessor();

				_delegateMap = new Dictionary<EventHandler<TestRunRequestStats>, Delegate>();
				_event = typeof( TestRunRequest ).GetEvent( "TestRunStatsChanged", BindingFlags.NonPublic | BindingFlags.Instance );
				_eventHandlerType = _event.EventHandlerType;
				_eventArgsType = _eventHandlerType.GetGenericArguments()[ 0 ];
				_internalHandlerFactory = BuildInternalHandlerFactory();
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

			#endregion

			private readonly TestRunRequest _instance;

			public TestRunRequestInternals( TestRunRequest instance )
			{
				_instance = instance;
			}

			public TestRunConfiguration TestRunConfig
			{
				get { return _testRunConfigAccessor( _instance ); }
			}

			public event EventHandler<TestRunRequestStats> TestRunStatsChanged
			{
				add
				{
					Delegate internalHandler = _internalHandlerFactory( value );
					_delegateMap[ value ] = internalHandler;
					_event.GetAddMethod( nonPublic : true ).Invoke( _instance, new object[] { internalHandler } );
				}
				remove
				{
					Delegate internalHandler = _delegateMap[ value ];
					_event.GetRemoveMethod( nonPublic : true ).Invoke( _instance, new object[] { internalHandler } );
				}
			}
		}
	}
}