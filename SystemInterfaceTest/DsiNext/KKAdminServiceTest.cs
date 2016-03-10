using DataLayer.MongoData;
using NUnit.Framework;
using SystemInterface;

namespace SystemInterfaceTest.DsiNext
{
	[TestFixture]
	public class KKAdminServiceTest : TestBase
	{
		[Test]
		public void Test()
		{
			UrlLogin urlLogin = UrlLogin.GetUrlLogin(_mongoConnection, "KKAdmin");
			InterfaceHelper initializer = InterfaceHelper.GetInstance();
			initializer.AllowThumbprint(urlLogin.CertificateThumbprint);

			SystemInterface.KKAdminService.KKAdminServiceClient proxy = new SystemInterface.KKAdminService.KKAdminServiceClient();

			string version = proxy.GetVersion();
			Assert.AreEqual("1.00", version);
		}
	}
}
