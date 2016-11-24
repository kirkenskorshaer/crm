using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemInterface.Dynamics.Crm
{
	public class SavedQuery : AbstractValueEntity
	{
		public string name;
		public string returnedtypecode;
		public string fetchxml;
		public string layoutxml;
		public int? querytype = 0;

		public SavedQuery(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity, "savedquery", "savedqueryid")
		{
		}

		private SavedQuery(IDynamicsCrmConnection connection) : base(connection, "savedquery", "savedqueryid")
		{
		}

		public static SavedQuery CreateAndInsert(IDynamicsCrmConnection dynamicsCrmConnection, string name, string returnedtypecode, string fetchxml, string layoutxml, Guid owner)
		{
			SavedQuery savedQuery = new SavedQuery(dynamicsCrmConnection)
			{
				name = name,
				returnedtypecode = returnedtypecode,
				fetchxml = fetchxml,
				layoutxml = layoutxml,
			};

			savedQuery.InsertWithoutRead();

			//savedQuery.owner = owner;

			//savedQuery.Assign();

			return savedQuery;
		}
	}
}
