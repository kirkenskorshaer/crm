using Administration.Option.Options.Data;
using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Administration.Option.Options.Logic
{
	public class ExposeIndsamlingssteder : AbstractDataOptionBase
	{
		private DatabaseExposeIndsamlingssteder _databaseExposeIndsamlingssteder;

		public ExposeIndsamlingssteder(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseExposeIndsamlingssteder = (DatabaseExposeIndsamlingssteder)databaseOption;
		}

		protected override bool ExecuteOption()
		{
			throw new NotImplementedException();
		}
	}
}
