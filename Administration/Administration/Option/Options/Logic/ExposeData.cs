using Administration.Option.Options.Data;
using DataLayer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using DatabaseExposeData = DataLayer.MongoData.Option.Options.Logic.ExposeData;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;

namespace Administration.Option.Options.Logic
{
	public class ExposeData : AbstractDataOptionBase
	{
		private DatabaseExposeData _databaseExposeData;

		public ExposeData(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseExposeData = (DatabaseExposeData)databaseOption;
		}

		protected override bool ExecuteOption()
		{
			string urlLoginName = _databaseExposeData.urlLoginName;
			string fetchXmlPath = _databaseExposeData.fetchXmlPath;
			string exposePath = _databaseExposeData.exposePath;
			string exposeName = _databaseExposeData.exposeName;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			DynamicsCrmConnection dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);

			string fullFetchXmlPath = Config.GetResourcePath(fetchXmlPath);
			string fullExposePath = Config.GetResourcePath(exposePath);

			List<dynamic> dataList = DynamicFetch.ReadFromFetchXml(dynamicsCrmConnection, fullFetchXmlPath, new PagingInformation());

			JsonSerializerSettings settings = new JsonSerializerSettings()
			{
				NullValueHandling = NullValueHandling.Ignore,
			};

			string json = JsonConvert.SerializeObject(dataList, Formatting.Indented, settings);
			json = json.Replace("\\r\\n", Environment.NewLine);

			if (Directory.Exists(fullExposePath) == false)
			{
				Directory.CreateDirectory(fullExposePath);
			}

			File.WriteAllText(fullExposePath + "/" + exposeName, json, System.Text.Encoding.UTF8);

			return true;
		}

		public static List<ExposeData> Find(MongoConnection connection)
		{
			List<DatabaseExposeData> options = DatabaseExposeData.ReadAllowed<DatabaseExposeData>(connection);

			return options.Select(option => new ExposeData(connection, option)).ToList();
		}
	}
}
