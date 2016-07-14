using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemInterface.Dynamics.Crm.Custom
{
	public class Indsamlingssted
	{
		public Guid accountid { get; set; }
		public string name { get; set; }
		public string address1_line1 { get; set; }
		public string address1_line2 { get; set; }
		public string address1_postalcode { get; set; }
		public string address1_city { get; set; }
	}
}
