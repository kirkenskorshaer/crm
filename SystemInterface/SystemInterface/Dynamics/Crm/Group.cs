using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class Group : IComparable
	{
		public Guid GroupId { get; private set; }
		public DateTime CreatedOn { get; private set; }
		public DateTime ModifiedOn { get; private set; }
		public string Name;

		//private static readonly ColumnSet ColumnSetGroup = new ColumnSet("createdby", "createdon", "modifiedby", "modifiedon", "new_groupid", "new_name", "ownerid", "owningbusinessunit", "owninguser", "statecode", "statuscode");
		private static readonly ColumnSet ColumnSetGroup = new ColumnSet("new_groupid", "new_name", "createdon", "modifiedon");

		public Group()
		{
		}

		public Group(Entity groupEntity)
		{
			SetValuesFromEntity(groupEntity);
		}

		private static Group EntityToGroup(Entity entity)
		{
			Group group = new Group();

			group.SetValuesFromEntity(entity);

			return group;
		}

		private void SetValuesFromEntity(Entity entity)
		{
			GroupId = (Guid)entity.Attributes["new_groupid"];
			CreatedOn = (DateTime)entity.Attributes["createdon"];
			ModifiedOn = (DateTime)entity.Attributes["modifiedon"];
			Name = entity.Attributes["new_name"].ToString();
		}

		private Entity GetGroupAsEntity(bool includeId)
		{
			Entity crmEntity = new Entity("new_group");
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_name", Name));

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_groupid", GroupId));
			}

			return crmEntity;
		}

		public static Group Read(IDynamicsCrmConnection connection, Guid groupid)
		{
			Entity contactEntity = connection.Service.Retrieve("new_group", groupid, ColumnSetGroup);

			Group group = EntityToGroup(contactEntity);

			return group;
		}

		public static List<Group> Read(IDynamicsCrmConnection connection, string name)
		{
			ConditionExpression equalsNameExpression = new ConditionExpression
			{
				AttributeName = "new_name",
				Operator = ConditionOperator.Equal,
			};
			equalsNameExpression.Values.Add(name);

			FilterExpression filterExpression = new FilterExpression();
			filterExpression.Conditions.Add(equalsNameExpression);

			QueryExpression query = new QueryExpression("new_group")
			{
				ColumnSet = ColumnSetGroup,
			};
			query.Criteria.AddFilter(filterExpression);

			EntityCollection GroupEntities = connection.Service.RetrieveMultiple(query);

			List<Group> groups = GroupEntities.Entities.Select(EntityToGroup).ToList();

			return groups;
		}

		public static Group ReadOrCreate(IDynamicsCrmConnection connection, string name)
		{
			List<Group> groups = Read(connection, name);
			Group group = groups.FirstOrDefault(lGroup => lGroup.Name == name);

			if (group != null)
			{
				return group;
			}

			group = new Group()
			{
				Name = name,
			};

			group.Insert(connection);

			return group;
		}

		public void Delete(IDynamicsCrmConnection connection)
		{
			connection.Service.Delete("new_group", GroupId);
		}

		public void Insert(IDynamicsCrmConnection connection)
		{
			Entity crmEntity = GetGroupAsEntity(false);

			GroupId = connection.Service.Create(crmEntity);
		}

		public void Update(IDynamicsCrmConnection connection)
		{
			Entity crmEntity = GetGroupAsEntity(true);

			connection.Service.Update(crmEntity);
		}

		public static List<string> GetAllAttributeNames(IDynamicsCrmConnection connection, Guid groupId)
		{
			List<string> attributeNames = new List<string>();

			ColumnSet columnsAll = new ColumnSet(true);

			Entity entity = connection.Service.Retrieve("new_group", groupId, columnsAll);

			attributeNames = entity.Attributes.Select(attribute => attribute.Key).ToList();

			return attributeNames;
		}

		public int CompareTo(object obj)
		{
			Group objAsGroup = obj as Group;

			return Name.CompareTo(objAsGroup.Name);
		}
	}
}
