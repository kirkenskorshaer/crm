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

		public static StressTestCrm Create(MongoConnection connection, string name, Schedule schedule, string urlLoginName, int contactsToCreate)
		{
			StressTestCrm stressTestCrm = new StressTestCrm
			{
				urlLoginName = urlLoginName,
				contactsToCreate = contactsToCreate
			};

			Create(connection, stressTestCrm, name, schedule);

			return stressTestCrm;
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
