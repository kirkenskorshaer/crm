using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities;

namespace DataLayer.MongoData.Option.Status
{
	public class OptionResult : AbstractMongoData
	{
		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime BeginTime;
		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime EndTime;
		public string Name;
		public bool Success;
		public long Memory;

		public static OptionResult Create(MongoConnection mongoConnection, DateTime beginTime, DateTime endTime, string name, bool success, long Memory)
		{
			OptionResult optionResult = new OptionResult()
			{
				BeginTime = beginTime,
				EndTime = endTime,
				Name = name,
				Success = success,
				Memory = Memory,
			};

			Create(mongoConnection, optionResult);

			return optionResult;
		}

		public static long ClearOldResults(MongoConnection mongoConnection, DateTime maxAllowedEndDate)
		{
			IMongoCollection<OptionResult> datas = mongoConnection.Database.GetCollection<OptionResult>(typeof(OptionResult).Name);

			FilterDefinition<OptionResult> deleteFilter = Builders<OptionResult>.Filter.Lt(optionResult => optionResult.EndTime, maxAllowedEndDate);

			Task<DeleteResult> deleteTask = datas.DeleteManyAsync(deleteFilter);

			DeleteResult deleteResult = MongoDataHelper.GetValueOrThrowTimeout(deleteTask);

			if (deleteResult.IsAcknowledged == false)
			{
				return 0;
			}

			return deleteResult.DeletedCount;
		}

		public static Dictionary<string, OptionStatusLine> GetOptionStatus(MongoConnection mongoConnection)
		{
			IMongoCollection<OptionResult> datas = mongoConnection.Database.GetCollection<OptionResult>(typeof(OptionResult).Name);

			BsonDocument projectionDocument = GetOptionResultProjectionBsonDocument();
			BsonDocument groupingDocument = GetOptionResultGroupingBsonDocument();

			IAggregateFluent<BsonDocument> aggregateFluent = datas.Aggregate().Project(projectionDocument).Group(groupingDocument);

			Task<List<BsonDocument>> aggregateTask = aggregateFluent.ToListAsync();

			List<BsonDocument> aggregatedDocuments = aggregateTask.Result;

			Dictionary<string, OptionStatusLine> resultDictionary = new Dictionary<string, OptionStatusLine>();

			foreach (BsonDocument optionResultGroup in aggregatedDocuments)
			{
				string name = optionResultGroup["_id"]["name"].AsString;
				bool success = optionResultGroup["_id"]["success"].AsBoolean;
				bool within10Minutes = optionResultGroup["_id"]["within10Minutes"].AsBoolean;
				bool within1Hour = optionResultGroup["_id"]["within1Hour"].AsBoolean;
				bool within24Hours = optionResultGroup["_id"]["within24Hours"].AsBoolean;
				int count = optionResultGroup["count"].AsInt32;

				OptionStatusLine line;
				if (resultDictionary.ContainsKey(name))
				{
					line = resultDictionary[name];
				}
				else
				{
					line = new OptionStatusLine();
					resultDictionary.Add(name, line);
				}

				UpdateLine(success, within10Minutes, within1Hour, within24Hours, count, line);
			}

			return resultDictionary;
		}

		public static Dictionary<string, long> GetMemoryStatistics(MongoConnection _mongoConnection)
		{
			IMongoCollection<OptionResult> datas = _mongoConnection.Database.GetCollection<OptionResult>(typeof(OptionResult).Name);

			FilterDefinition<OptionResult> blankFilter = Builders<OptionResult>.Filter.Where(_ => true);

			Task<IAsyncCursor<string>> distinctCursorTask = datas.DistinctAsync<string>("Name", blankFilter);

			IAsyncCursor<string> cursor = distinctCursorTask.Result;

			Task<List<string>> distinctTask = cursor.ToListAsync();

			List<string> names = distinctTask.Result;

			Dictionary<string, long> memoryStatistics = new Dictionary<string, long>();

			foreach (string name in names)
			{
				long latestMemory = GetLatestMemory(_mongoConnection, name);

				memoryStatistics.Add(name, latestMemory);
			}

			return memoryStatistics;
		}

		private static long GetLatestMemory(MongoConnection _mongoConnection, string name)
		{
			IMongoCollection<OptionResult> datas = _mongoConnection.Database.GetCollection<OptionResult>(typeof(OptionResult).Name);

			FilterDefinition<OptionResult> filter = Builders<OptionResult>.Filter.Eq(result => result.Name, name);

			SortDefinition<OptionResult> sort = Builders<OptionResult>.Sort.Descending(result => result.EndTime);

			IFindFluent<OptionResult, OptionResult> memoryFind = datas.Find(filter).Sort(sort).Limit(1);

			Task<OptionResult> memoryTask = memoryFind.SingleOrDefaultAsync();

			long memory = memoryTask.Result.Memory;

			return memory;
		}

		private static void UpdateLine(bool success, bool within10Minutes, bool within1Hour, bool within24Hours, int count, OptionStatusLine line)
		{
			line.ExecutionTotal += count;
			if (success)
			{
				line.SuccessTotal += count;

				if (within10Minutes)
				{
					line.Success10Minute += count;
				}
				if (within1Hour)
				{
					line.Success1Hour += count;
				}
				if (within24Hours)
				{
					line.Success24Hour += count;
				}
			}
			else
			{
				line.FailTotal += count;

				if (within10Minutes)
				{
					line.Fail10Minute += count;
				}
				if (within1Hour)
				{
					line.Fail1Hour += count;
				}
				if (within24Hours)
				{
					line.Fail24Hour += count;
				}
			}
		}

		private static BsonDocument GetOptionResultProjectionBsonDocument()
		{
			DateTime currentTime = Clock.Now;
			DateTime time10MinutesAgo = currentTime - TimeSpan.FromMinutes(10);
			DateTime time1HourAgo = currentTime - TimeSpan.FromHours(1);
			DateTime time24HoursAgo = currentTime - TimeSpan.FromHours(24);

			return new BsonDocument
			{
				{
					"within10Minutes", new BsonDocument
					{
						{
							"$gt", new BsonArray
							{
								"$EndTime",
								time10MinutesAgo
							}
						}
					}
				},
				{
					"within1Hour", new BsonDocument
					{
						{
							"$gt", new BsonArray
							{
								"$EndTime",
								time1HourAgo
							}
						}
					}
				},
				{
					"within24Hours", new BsonDocument
					{
						{
							"$gt", new BsonArray
							{
								"$EndTime",
								time24HoursAgo
							}
						}
					}
				},
				{ "Success", 1 },
				{ "Name", 1 },
			};
		}

		private static BsonDocument GetOptionResultGroupingBsonDocument()
		{
			return new BsonDocument
			{
				{
					"_id", new BsonDocument
					{
						{ "name", "$Name" },
						{ "success", "$Success" },
						{ "within10Minutes", "$within10Minutes" },
						{ "within1Hour", "$within1Hour" },
						{ "within24Hours", "$within24Hours" },
					}
				},
				{
					"count", new BsonDocument
					{
						{ "$sum", 1 }
					}
				}
			};
		}
	}
}
