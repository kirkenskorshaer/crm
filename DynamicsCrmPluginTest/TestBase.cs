using DynamicsCrmPluginTest.Mock;
using NUnit.Framework;
using System;

namespace DynamicsCrmPluginTest
{
	[TestFixture]
	public abstract class TestBase
	{
		protected IServiceProvider _serviceProvider;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			_serviceProvider = new ServiceProviderTest();
		}
	}
}
