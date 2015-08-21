using SystemInterface;
using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;

namespace SystemInterfaceTest
{
	[TestFixture]
	public class EmailTest
	{
		[Test]
		[Ignore]
		public void SendTest()
		{
			Email email = new Email();

			MongoConnection connection = MongoConnection.GetConnection("test");

			Config config = Config.GetConfig(connection);

			email.Send("test", "test", config.Email, "svend.l@kirkenskorshaer.dk", config.EmailSmtpHost, config.EmailSmtpPort, config.Email, config.EmailPassword);
		}
	}
}
