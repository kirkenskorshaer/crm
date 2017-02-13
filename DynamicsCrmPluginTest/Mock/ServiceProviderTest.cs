using Microsoft.Xrm.Sdk;
using System;

namespace DynamicsCrmPluginTest.Mock
{
	public class ServiceProviderTest : IServiceProvider
	{
		private TracingServiceTest _tracingService = new TracingServiceTest();
		private IPluginExecutionContext _pluginExecutionContext = new PluginExecutionContextTest();

		public object GetService(Type serviceType)
		{
			if (serviceType == typeof(ITracingService))
			{
				return _tracingService;
			}

			if (serviceType == typeof(IPluginExecutionContext))
			{
				return _pluginExecutionContext;
			}

			return null;
		}
	}
}
