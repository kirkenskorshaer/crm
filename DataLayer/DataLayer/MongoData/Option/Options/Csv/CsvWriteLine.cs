using MongoDB.Bson;
using System.Collections.Generic;

namespace DataLayer.MongoData.Option.Options.Csv
{
	public class CsvWriteLine : CsvBase
	{
		public List<CsvElement> CsvElements { get; set; }

		public static CsvWriteLine Create(MongoConnection connection, string name, Schedule schedule, string fileName, string fileNameTmp, char delimeter, List<CsvElement> csvElements)
		{
			CsvWriteLine csvWriteLine = new CsvWriteLine
			{
				FileName = fileName,
				FileNameTmp = fileNameTmp,
				Delimeter = delimeter,
				CsvElements = csvElements,
			};

			Create(connection, csvWriteLine, name, schedule);

			return csvWriteLine;
		}

		public static List<CsvWriteLine> Read(MongoConnection connection, string id)
		{
			ObjectId objectId = new ObjectId(id);
			return ReadById<CsvWriteLine>(connection, objectId);
		}

		public void Update(MongoConnection connection)
		{
			Update<CsvWriteLine>(connection);
		}

		public void Delete(MongoConnection connection)
		{
			Delete<CsvWriteLine>(connection);
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
