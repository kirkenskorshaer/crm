using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class StressTestCrm : OptionBase
	{
		public string urlLoginName { get; set; }
		public int contactsToCreate { get; set; }
		public int accountsToCreate { get; set; }

		public static StressTestCrm Create(MongoConnection connection, string name, Schedule schedule, string urlLoginName, int contactsToCreate, int accountsToCreate)
		{
			StressTestCrm stressTestCrm = new StressTestCrm
			{
				urlLoginName = urlLoginName,
				contactsToCreate = contactsToCreate,
				accountsToCreate = accountsToCreate,
			};

			Create(connection, stressTestCrm, name, schedule);

			return stressTestCrm;
		}

		public void Update(MongoConnection connection)
		{
			Update<Email>(connection);
		}

		public static List<StressTestCrm> Read(MongoConnection connection, string id)
		{
			ObjectId objectId = new ObjectId(id);
			return ReadById<StressTestCrm>(connection, objectId);
		}

		public void Delete(MongoConnection connection)
		{
			Delete<StressTestCrm>(connection);
		}

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<StressTestCrm>(connection);
			}
			else
			{
				Delete<StressTestCrm>(connection);
			}
		}
	}
}
