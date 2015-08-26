using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Bson;

namespace DataLayer.MongoData.Option.Options
{
	public class Email : OptionBase
	{
		public string To { get; set; }
		public string MessageBody { get; set; }

		public static Email Create(MongoConnection connection, string name, Schedule schedule, string to, string messageBody)
		{
			Email email = new Email
			{
				To = to,
				MessageBody = messageBody
			};

			Create(connection, email, name, schedule);

			return email;
		}

		public static List<Email> Read(MongoConnection connection, string id)
		{
			ObjectId mongoId = new ObjectId(id);
			return Read<Email>(connection, email => email._id == mongoId);
		}

		public void Update(MongoConnection connection)
		{
			Update<Email>(connection);
		}

		public void Delete(MongoConnection connection)
		{
			Delete<Email>(connection);
		}
    }
}
