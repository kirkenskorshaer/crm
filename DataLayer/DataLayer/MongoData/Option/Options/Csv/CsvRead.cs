using MongoDB.Bson;
using System.Collections.Generic;

namespace DataLayer.MongoData.Option.Options.Csv
{
	public class CsvRead : CsvBase
	{
		public string KeyName { get; set; }
		public string KeyValue { get; set; }

		public static CsvRead Create(MongoConnection connection, string name, Schedule schedule, string fileName, string fileNameTmp, char delimeter, string keyName, string keyValue)
		{
			CsvRead csvRead = new CsvRead
			{
				FileName = fileName,
				FileNameTmp = fileNameTmp,
				Delimeter = delimeter,

				KeyName = keyName,
				KeyValue = keyValue,
			};

			Create(connection, csvRead, name, schedule);

			return csvRead;
		}

		public static List<CsvRead> Read(MongoConnection connection, string id)
		{
			ObjectId objectId = new ObjectId(id);
			return ReadById<CsvRead>(connection, objectId);
		}

		public void Update(MongoConnection connection)
		{
			Update<CsvRead>(connection);
		}

		public void Delete(MongoConnection connection)
		{
			Delete<CsvRead>(connection);
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
