using System.Collections.Generic;
using System.Linq;
using DataLayer;
using DatabaseEmail = DataLayer.MongoData.Option.Options.Email;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options
{
	public class Email : OptionBase
	{
		public Email(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		private DatabaseEmail _databaseEmail;

		public static List<Email> Find(MongoConnection connection)
		{
			List<DatabaseEmail> databaseEmails = DatabaseOptionBase.ReadAllowed<DatabaseEmail>(connection);

			return databaseEmails.Select(databaseEmail => new Email(connection, databaseEmail)
			{
				_databaseEmail = databaseEmail,
			}).ToList();
		}

		protected override void ExecuteOption(OptionReport report)
		{
			SystemInterface.Email emailSender = new SystemInterface.Email();
			emailSender.Send(_databaseEmail.MessageBody, false, _databaseEmail.Subject, Config.Email, _databaseEmail.To, Config.EmailSmtpHost, Config.EmailSmtpPort, Config.Email, Config.EmailPassword);
			Log.Write(Connection, $"Email sendt to {_databaseEmail.To}", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);

			report.Success = true;
		}
	}
}
