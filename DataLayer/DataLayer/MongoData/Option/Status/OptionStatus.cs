using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLayer.MongoData.Option.Status
{
	public class OptionStatus : AbstractMongoData
	{
		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime LastUpdateTime { get; set; }

		public Dictionary<string, OptionStatusLine> options = new Dictionary<string, OptionStatusLine>();
		public Dictionary<string, long> optionLastMemory = new Dictionary<string, long>();

		public Dictionary<string, ActiveOption> ActiveOptions = new Dictionary<string, ActiveOption>();

		public static OptionStatus ReadOrCreate(MongoConnection mongoConnection)
		{
			OptionStatus optionStatus = First<OptionStatus>(mongoConnection);

			if (optionStatus == null)
			{
				optionStatus = new OptionStatus();

				Create(mongoConnection, optionStatus);
			}

			return optionStatus;
		}

		public void UpdateStatus(MongoConnection mongoConnection)
		{
			LastUpdateTime = DateTime.Now;
			Update<OptionStatus>(mongoConnection);
		}

		public void UpdateStatisticsFromResults(MongoConnection _mongoConnection)
		{
			optionLastMemory = OptionResult.GetMemoryStatistics(_mongoConnection);

			options = OptionResult.GetOptionStatus(_mongoConnection);
		}

		public void ActiveOptionsAdd(OptionBase option, Worker worker)
		{
			if (option == null)
			{
				return;
			}

			if (ActiveOptions.ContainsKey(option.Id))
			{
				return;
			}

			ActiveOption activeOption = new ActiveOption()
			{
				Begin = DateTime.Now,
				Name = option.Name,
				TypeName = option.GetType().Name,
				WorkerId = worker._id,
			};

			ActiveOptions.Add(option.Id, activeOption);
		}

		public void ActiveOptionsRemove(OptionBase option)
		{
			if (option == null)
			{
				return;
			}

			if (ActiveOptions.ContainsKey(option.Id) == false)
			{
				return;
			}

			ActiveOptions.Remove(option.Id);
		}

		public void ActiveOptionsRemoveByWorkerList(List<Worker> deadWorkers)
		{
			List<string> optionIdsToDelete = ActiveOptions.
				Where(optionKeyValue =>
					deadWorkers.Any(worker => optionKeyValue.Value.WorkerId == worker._id)).
				Select(optionKeyValue => optionKeyValue.Key).ToList();

			foreach (string id in optionIdsToDelete)
			{
				ActiveOptions.Remove(id);
			}
		}
	}
}
