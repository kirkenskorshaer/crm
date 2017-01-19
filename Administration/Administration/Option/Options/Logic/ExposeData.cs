using DataLayer;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SystemInterface.Dynamics.Crm;
using DatabaseExposeData = DataLayer.MongoData.Option.Options.Logic.ExposeData;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;

namespace Administration.Option.Options.Logic
{
	public class ExposeData : OptionBase
	{
		private DatabaseExposeData _databaseExposeData;

		public ExposeData(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseExposeData = (DatabaseExposeData)databaseOption;
		}

		public override void ExecuteOption(OptionReport report)
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
				StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
			};

			string json = JsonConvert.SerializeObject(dataList, Formatting.None, settings);
			json = json.Replace("\\r\\n", "\\n");

			if (Directory.Exists(fullExposePath) == false)
			{
				Directory.CreateDirectory(fullExposePath);
			}

			File.WriteAllText(fullExposePath + "/" + exposeName, json, Encoding.UTF8);

			report.Success = true;

			return;
		}
	}
}
