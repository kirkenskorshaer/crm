using DataLayer;
using SystemInterface.Mailrelay;
using DatabaseOptionType = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option
{
	public abstract class OptionBase
	{
		protected readonly MongoConnection Connection;
		protected readonly DataLayer.MongoData.Config Config;
		internal readonly DatabaseOptionType DatabaseOption;
		protected IMailrelayConnection _mailrelayConnection;

		protected OptionBase(MongoConnection connection, DatabaseOptionType databaseOption)
		{
			Connection = connection;
			Config = DataLayer.MongoData.Config.GetConfig(Connection);
			Log.LogLevel = Config.LogLevel;
			DatabaseOption = databaseOption;

			string mailrelayUrl = Config.MailrelayUrl;
			string apiKey = Config.MailrelayApiKey;

			_mailrelayConnection = new MailrelayConnection(mailrelayUrl, apiKey);
		}

		protected abstract bool ExecuteOption();

		public void ChangeMailrelayConnection(IMailrelayConnection newConnection)
		{
			_mailrelayConnection = newConnection;
		}

		public bool Execute()
		{
			bool isSuccess = ExecuteOption();

			DatabaseOption?.Execute(Connection);

			return isSuccess;
		}
	}
}
