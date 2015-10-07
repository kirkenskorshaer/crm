using MongoDB.Bson;
using System.Collections.Generic;

namespace DataLayer.MongoData.Option.Options.Csv
{
	public class CsvDelete : CsvBase
	{
		public string KeyName { get; set; }
		public string KeyValue { get; set; }

		public static CsvDelete Create(MongoConnection connection, string name, Schedule schedule, string fileName, string fileNameTmp, char delimeter, string keyName, string keyValue)
		{
			CsvDelete csvDelete = new CsvDelete
			{
				FileName = fileName,
				FileNameTmp = fileNameTmp,
				Delimeter = delimeter,

				KeyName = keyName,
				KeyValue = keyValue,
			};

			Create(connection, csvDelete, name, schedule);

			return csvDelete;
		}

		public static List<CsvDelete> Read(MongoConnection connection, string id)
		{
			ObjectId objectId = new ObjectId(id);
			return ReadById<CsvDelete>(connection, objectId);
		}

		public void Update(MongoConnection connection)
		{
			Update<CsvDelete>(connection);
		}

		public void Delete(MongoConnection connection)
		{
			Delete<CsvDelete>(connection);
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
