using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class BackendMessage : OptionBase
	{
		public List<BackendMessageParameter> Parameters = new List<BackendMessageParameter>();

		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime ReceivedTime;

		public static BackendMessage Create(MongoConnection mongoConnection, string name, Dictionary<string, object> inputParameters)
		{
			BackendMessage message = new BackendMessage()
			{
				ReceivedTime = DateTime.Now,
				Parameters = inputParameters.Select(parameter => new BackendMessageParameter(parameter.Key, parameter.Value)).ToList(),
			};

			Schedule schedule = new Schedule()
			{
				ActionOnFail = Schedule.ActionOnFailEnum.TryAgain,
				Enabled = false,
				NextAllowedExecution = DateTime.Now,
				Recurring = false,
				TimeBetweenAllowedExecutions = TimeSpan.FromMinutes(10),
			};

			Create(mongoConnection, message, name, schedule);

			return message;
		}
	}
}
