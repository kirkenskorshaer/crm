using System;
using System.Collections.Generic;

namespace SystemInterface.Dynamics.Crm.Custom
{
	public class CampaignIndsamlingssted
	{
		public Guid campaignid { get; set; }
		public List<Indsamlingssted> indsamlingssteder { get; set; }
	}
}
