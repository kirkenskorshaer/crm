using DynamicsCrmPlugin;
using NUnit.Framework;

namespace DynamicsCrmPluginTest
{
	[TestFixture]
	public class BackendCallbackTest : TestBase
	{
		[Test]
		public void BackendCallbackCanSetOutputParameters()
		{
			BackendCallback backendCallback = new BackendCallback("http://localhost/read.txt");

			backendCallback.Execute(_serviceProvider);
        }
	}
}
