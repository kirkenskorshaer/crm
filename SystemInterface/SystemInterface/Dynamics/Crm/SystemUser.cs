using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class SystemUser : AbstractCrm
	{
		public string domainname;

		public SystemUser(DynamicsCrmConnection connection) : base(connection)
		{
		}

		public SystemUser(DynamicsCrmConnection connection, Entity entity) : base(connection, entity)
		{
		}

		private static readonly ColumnSet ColumnSetUserCrmGenerated = new ColumnSet("systemuserid");
		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetUserCrmGenerated; } }
		private static readonly ColumnSet ColumnSetUser = new ColumnSet("domainname", "systemuserid");

		protected override string entityName { get { return "systemuser"; } }
		protected override string idName { get { return "systemuserid"; } }

		protected override CrmEntity GetAsEntity(bool includeId)
		{
			CrmEntity crmEntity = new CrmEntity("systemuser");

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>("systemuserid", Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("domainname", domainname));

			return crmEntity;
		}

		public static SystemUser Read(DynamicsCrmConnection connection, Guid userid)
		{
			Entity userEntity = connection.Service.Retrieve("systemuser", userid, ColumnSetUser);

			SystemUser contact = new SystemUser(connection, userEntity);

			return contact;
		}

		public static SystemUser ReadByDomainname(DynamicsCrmConnection connection, string domainname)
		{
			List<SystemUser> users = StaticCrm.ReadByAttribute(connection, "systemuser", ColumnSetUser, "domainname", domainname, (lConnection, entity) => new SystemUser(lConnection, entity));

			return users.Single();
		}
	}
}
