using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.MongoData.Input;
using SystemInterface.Dynamics.Crm;

namespace Administration.Conversion
{
	public static class Campaign
	{
		internal static WebCampaign.CollectTypeEnum ToDatabaseEnum(SystemInterface.Dynamics.Crm.Campaign.collecttypeEnum? collecttype)
		{
			switch (collecttype)
			{
				case SystemInterface.Dynamics.Crm.Campaign.collecttypeEnum.Lead:
					return WebCampaign.CollectTypeEnum.Lead;
				case SystemInterface.Dynamics.Crm.Campaign.collecttypeEnum.ContactOgLeadVedEksisterendeContact:
					return WebCampaign.CollectTypeEnum.LeadOgContactHvisContactIkkeFindes;
				default:
					break;
			}

			throw new ArgumentException($"unknown type {collecttype}");
		}
	}
}
