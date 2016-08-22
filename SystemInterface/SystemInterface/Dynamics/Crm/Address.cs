using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class Address
	{
		public Guid AddressId { get; private set; }
		public string line1;

		//private static readonly ColumnSet ColumnSetAddress = new ColumnSet("createdby", "createdon", "modifiedby", "modifiedon", "customeraddressid", "ownerid", "owningbusinessunit", "owninguser", "statecode", "statuscode");
		private static readonly ColumnSet ColumnSetAddress = new ColumnSet("customeraddressid", "line1");

		private static Address EntityToAddress(Entity entity)
		{
			return new Address
			{
				AddressId = (Guid)entity.Attributes["customeraddressid"],
				line1 = entity.Attributes["line1"].ToString(),
			};
		}

		private CrmEntity GetAddressAsEntity(bool includeId)
		{
			CrmEntity crmEntity = new CrmEntity("customeraddress");
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("line1", line1));

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>("customeraddressid", AddressId));
			}

			return crmEntity;
		}

		public static Address Read(IDynamicsCrmConnection connection, Guid addressid)
		{
			Entity contactEntity = connection.Service.Retrieve("customeraddress", addressid, ColumnSetAddress);

			Address address = EntityToAddress(contactEntity);

			return address;
		}

		public void Update(IDynamicsCrmConnection connection)
		{
			CrmEntity crmEntity = GetAddressAsEntity(true);

			connection.Service.Update(crmEntity);
		}

		public static List<string> GetAllAttributeNames(IDynamicsCrmConnection connection, Guid addressId)
		{
			List<string> attributeNames = new List<string>();

			ColumnSet columnsAll = new ColumnSet(true);

			Entity entity = connection.Service.Retrieve("customeraddress", addressId, columnsAll);

			attributeNames = entity.Attributes.Select(attribute => $"{attribute.Value.GetType().Name} {attribute.Key}").ToList();

			return attributeNames;
		}

	}
}
