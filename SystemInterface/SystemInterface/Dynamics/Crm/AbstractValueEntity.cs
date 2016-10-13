using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utilities;

namespace SystemInterface.Dynamics.Crm
{
	public abstract class AbstractValueEntity : AbstractCrm
	{
		private string _entityName;
		private string _idName;

		public AbstractValueEntity(IDynamicsCrmConnection connection, Entity crmEntity, string entityName, string idName) : base(connection)
		{
			_idName = idName;
			_entityName = entityName;

			InitializeFromEntity(crmEntity);
		}

		public AbstractValueEntity(IDynamicsCrmConnection connection, string entityName, string idName) : base(connection)
		{
			_idName = idName;
			_entityName = entityName;
		}

		public AbstractValueEntity(IDynamicsCrmConnection connection) : base(connection)
		{
			throw new NotImplementedException();
		}

		public AbstractValueEntity(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity)
		{
			throw new NotImplementedException();
		}

		protected override ColumnSet ColumnSetCrmGenerated { get { throw new NotImplementedException(); } }

		protected override string entityName { get { return _entityName; } }

		protected override string idName { get { return _idName; } }

		protected override CrmEntity GetAsEntity(bool includeId)
		{
			return GetAsEntity(includeId, false);
		}

		protected CrmEntity GetAsEntity(bool includeId, bool includeNulls)
		{
			CrmEntity crmEntity = new CrmEntity(_entityName);

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

			Dictionary<string, object> values = GetEntityValues();

			foreach (KeyValuePair<string, object> keyValue in values)
			{
				if (includeNulls == false && ReflectionHelper.IsNullOrWhiteSpace(keyValue.Value))
				{
					continue;
				}

				crmEntity.Attributes.Add(new KeyValuePair<string, object>(keyValue.Key, keyValue.Value));
			}

			return crmEntity;
		}

		private Dictionary<string, object> GetEntityValues()
		{
			Dictionary<string, object> values = new Dictionary<string, object>();

			List<Type> allowedTypes = new List<Type>()
			{
				typeof(EntityReference),
				typeof(OptionSetValue),
				typeof(Money),
				typeof(int?),
				typeof(string),
				typeof(decimal?),
				typeof(bool?),
				typeof(DateTime?),
			};

			MemberInfo[] allMembers = GetType().GetMembers();
			List<MemberInfo> members = allMembers.Where
			(member =>
				(
					member.MemberType == MemberTypes.Field ||
					member.MemberType == MemberTypes.Property
				) &&
				allowedTypes.Any(allowedType => allowedType == ReflectionHelper.GetType(member))
			).ToList();

			foreach (MemberInfo member in members)
			{
				string name = member.Name;

				if (name == "ownerid" || name == "owningbusinessunit")
				{
					continue;
				}

				object value = ReflectionHelper.GetValue(this, member);

				values.Add(name, value);
			}

			return values;
		}
	}
}
