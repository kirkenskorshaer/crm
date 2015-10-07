using MongoDB.Bson;
using System.Collections.Generic;

namespace DataLayer.MongoData.Option.Options.Csv
{
	public class CsvUpdate : CsvBase
	{
		public List<CsvElement> CsvElements { get; set; }

		public string KeyName { get; set; }
		public string KeyValue { get; set; }

		public static CsvUpdate Create(MongoConnection connection, string name, Schedule schedule, string fileName, string fileNameTmp, char delimeter, string keyName, string keyValue, List<CsvElement> csvElements)
		{
			CsvUpdate csvUpdate = new CsvUpdate
			{
				FileName = fileName,
				FileNameTmp = fileNameTmp,
				Delimeter = delimeter,

				KeyName = keyName,
				KeyValue = keyValue,

				CsvElements = csvElements,
			};

			Create(connection, csvUpdate, name, schedule);

			return csvUpdate;
		}

		public static List<CsvUpdate> Read(MongoConnection connection, string id)
		{
			ObjectId objectId = new ObjectId(id);
			return ReadById<CsvUpdate>(connection, objectId);
		}

		public void Update(MongoConnection connection)
		{
			Update<CsvUpdate>(connection);
		}

		public void Delete(MongoConnection connection)
		{
			Delete<CsvUpdate>(connection);
		}

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update(connection);
			}
			else
			{
				Delete(connection);
			}
		}
	}
}
