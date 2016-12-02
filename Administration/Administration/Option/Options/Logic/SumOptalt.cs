using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using DatabaseSumOptalt = DataLayer.MongoData.Option.Options.Logic.SumOptalt;

namespace Administration.Option.Options.Logic
{
	public class SumOptalt : OptionBase
	{
		private DatabaseSumOptalt _databaseSumOptalt;

		public SumOptalt(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSumOptalt = (DatabaseSumOptalt)databaseOption;

			string urlLoginName = _databaseSumOptalt.urlLoginName;

			SetDynamicsCrmConnectionIfEmpty(urlLoginName);
		}

		protected override void ExecuteOption(OptionReport report)
		{
			decimal kreds = Account.GetOptaltbeloebSumKreds(_dynamicsCrmConnection);
			decimal by = Account.GetOptaltbeloebSumBy(_dynamicsCrmConnection);
			decimal total = Account.GetOptaltbeloebSum(_dynamicsCrmConnection);
			Guid campaignid = _databaseSumOptalt.campaignid;

			if (_databaseSumOptalt.kreds != kreds)
			{
				_databaseSumOptalt.kreds = kreds;
				Campaign.WriteOptaltsumKreds(_dynamicsCrmConnection, campaignid, kreds);
				report.Workload++;
			}

			if (_databaseSumOptalt.by != by)
			{
				_databaseSumOptalt.by = by;
				Campaign.WriteOptaltsumBy(_dynamicsCrmConnection, campaignid, by);
				report.Workload++;
			}

			if (_databaseSumOptalt.total != total)
			{
				_databaseSumOptalt.total = total;
				Campaign.WriteOptaltsum(_dynamicsCrmConnection, campaignid, total);
				report.Workload++;
			}

			report.Success = true;
		}
	}
}
