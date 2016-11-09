using DataLayer;
using System;
using System.IO;
using SystemInterface.Dynamics.Crm;
using SystemInterface.Mailrelay;
using SystemInterface.Twilio;
using DatabaseOptionType = DataLayer.MongoData.Option.OptionBase;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;

namespace Administration.Option
{
	public abstract class OptionBase
	{
		protected readonly MongoConnection Connection;
		protected readonly DataLayer.MongoData.Config Config;
		internal readonly DatabaseOptionType DatabaseOption;
		protected IMailrelayConnection _mailrelayConnection;
		protected IDynamicsCrmConnection _dynamicsCrmConnection;
		protected ITwilioConnection _twilioConnection;

		protected OptionBase(MongoConnection connection, DatabaseOptionType databaseOption)
		{
			Connection = connection;
			Config = DataLayer.MongoData.Config.GetConfig(Connection);
			Log.LogLevel = Config.LogLevel;
			DatabaseOption = databaseOption;

			string mailrelayUrl = Config.MailrelayUrl;
			string apiKey = Config.MailrelayApiKey;

			_mailrelayConnection = new MailrelayConnection(mailrelayUrl, apiKey)
			{
				sendInterval = TimeSpan.FromMilliseconds(Config.MailrelaySendIntervalMilliseconds),
			};

			if (Config.EnableTest == true)
			{
				string path = Config.GetResourcePath("emailtest");
				if (Directory.Exists(path) == false)
				{
					Directory.CreateDirectory(path);
				}

				SystemInterface.Email.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory;
				SystemInterface.Email.PickupDirectoryLocation = path;

				SystemInterface.DanskeBank.DanskeBankHandler.Environment = SystemInterface.DanskeBank.ApplicationRequest.EnvironmentEnum.TEST;
			}
			else
			{
				SystemInterface.Email.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
				SystemInterface.DanskeBank.DanskeBankHandler.Environment = SystemInterface.DanskeBank.ApplicationRequest.EnvironmentEnum.PRODUCTION;
			}
		}

		protected abstract bool ExecuteOption();

		protected void SetDynamicsCrmConnectionIfEmpty(string urlLoginName)
		{
			if (_dynamicsCrmConnection == null)
			{
				DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
				_dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);
			}
		}

		public void SetDynamicsCrmConnectionIfEmpty(IDynamicsCrmConnection dynamicsCrmConnection)
		{
			if (_dynamicsCrmConnection == null)
			{
				_dynamicsCrmConnection = dynamicsCrmConnection;
			}
		}

		protected void SetTwilioConnectionIfEmpty(string fromNumber, string accountSid, string authToken, string statusCallback)
		{
			if (_twilioConnection == null)
			{
				_twilioConnection = new TwilioConnection(fromNumber, accountSid, authToken, statusCallback);
			}
		}

		public void SetTwilioConnectionIfEmpty(ITwilioConnection newConnection)
		{
			if (_twilioConnection == null)
			{
				_twilioConnection = newConnection;
			}
		}

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
