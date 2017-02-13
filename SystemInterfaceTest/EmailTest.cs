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
		[Ignore("")]
		public void SendTest()
		{
			Email email = new Email();

			MongoConnection connection = MongoConnection.GetConnection("test");

			Config config = Config.GetConfig(connection);

			email.Send("test", false, "test", config.Email, "svend.l@kirkenskorshaer.dk", config.EmailSmtpHost, config.EmailSmtpPort, config.Email, config.EmailPassword);
		}

		[Test]
		public void SendingTestEmailsWillNotSendActualEmails()
		{
			Email.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory;
			Email.PickupDirectoryLocation = "c:\\test\\email";

			Email email = new Email();
			email.Send("test", false, "test", "svend.l@kirkenskorshaer.dk", "svend.l@kirkenskorshaer.dk", string.Empty, 0, string.Empty, string.Empty);
		}
	}
}
