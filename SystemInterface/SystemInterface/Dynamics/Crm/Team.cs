using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;
using System;

namespace SystemInterface.Dynamics.Crm
{
	public class Team : AbstractCrm
	{
		private static readonly ColumnSet ColumnSetTeam = new ColumnSet(
			"teamid");

		private static readonly ColumnSet ColumnSetTeamCrmGenerated = new ColumnSet("createdon", "modifiedon");

		public Team(IDynamicsCrmConnection connection) : base(connection)
		{
		}

		public Team(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity)
		{
		}

		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetTeamCrmGenerated; } }

		protected override string entityName { get { return "team"; } }
		protected override string idName { get { return "teamid"; } }

		protected override Entity GetAsEntity(bool includeContactId)
		{
			Entity crmEntity = new Entity("team");

			if (includeContactId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

			return crmEntity;
		}

		public static List<Team> ReadFromFetchXml(IDynamicsCrmConnection dynamicsCrmConnection, List<string> fields, Dictionary<string, string> keyContent)
		{
			return StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, fields, keyContent, null, (connection, contactEntity) => new Team(connection, contactEntity), new PagingInformation());
		}

		public static Guid? GetIdByNamed(IDynamicsCrmConnection dynamicsCrmConnection, string name)
		{
			Dictionary<string, string> teamSearch = new Dictionary<string, string>()
			{
				{ "name", name }
			};

			List<Team> teams = ReadFromFetchXml(dynamicsCrmConnection, new List<string>() { "teamid" }, teamSearch);

			Team team = teams.SingleOrDefault();

			return team?.Id;
		}
	}
}
