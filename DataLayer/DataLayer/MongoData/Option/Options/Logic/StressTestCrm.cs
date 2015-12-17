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
