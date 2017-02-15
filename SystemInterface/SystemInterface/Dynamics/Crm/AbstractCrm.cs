using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Utilities;

namespace SystemInterface.Dynamics.Crm
{
	public abstract class AbstractCrm
	{
		protected abstract string entityName { get; }
		protected abstract string idName { get; }
		protected abstract ColumnSet ColumnSetCrmGenerated { get; }

		protected IDynamicsCrmConnection Connection { get; private set; }

		protected virtual void AfterInsert(Entity generatedEntity) { }
		protected virtual void AfterUpdate(Entity generatedEntity) { }

		public Guid Id { get; protected set; }

		public EntityReference ownerid;
		public Guid? owner { get { return GetEntityReferenceId(ownerid); } set { ownerid = SetEntityReferenceId(value, ""); } }

		public EntityReference owningbusinessunit;
		public Guid? owningbusinessunitGuid { get { return GetEntityReferenceId(owningbusinessunit); } set { owningbusinessunit = SetEntityReferenceId(value, "businessunit"); } }

		public AbstractCrm(IDynamicsCrmConnection connection)
		{
			Connection = connection;
		}

		public AbstractCrm(IDynamicsCrmConnection connection, Entity crmEntity)
		{
			Connection = connection;

			InitializeFromEntity(crmEntity);
		}

		public void InitializeFromEntity(Entity crmEntity)
		{
			if (crmEntity.Attributes.ContainsKey(idName))
			{
				Id = (Guid)crmEntity.Attributes[idName];
			}

			IEnumerable<string> keys = crmEntity.Attributes.Keys.Where(key => key != idName);

			foreach (string key in keys)
			{
				object value = crmEntity.Attributes[key];

				if (value is AliasedValue)
				{
					value = ((AliasedValue)value).Value;
				}

				Utilities.ReflectionHelper.SetValue(this, key, value);
			}
		}

		protected abstract Entity GetAsEntity(bool includeId);

		public List<RelatedEntityType> ReadNNRelationship<RelatedEntityType>(string relationshipName, Entity currentEntity, Func<Entity, RelatedEntityType> crmEntityCreater, params string[] relatedColumns)
		{
			List<RelatedEntityType> relatedObjects = new List<RelatedEntityType>();

			ColumnSet columns = new ColumnSet(relatedColumns);

			RetrieveRelationshipRequest request = new RetrieveRelationshipRequest()
			{
				Name = relationshipName,
			};

			RetrieveRelationshipResponse response = (RetrieveRelationshipResponse)Connection.Service.Execute(request);

			ManyToManyRelationshipMetadata metaData = (ManyToManyRelationshipMetadata)response.Results["RelationshipMetadata"];

			KeyValuePair<EntityRelationshipInfo, EntityRelationshipInfo> relationshipInfo = EntityRelationshipInfo.GetFromMetaData(currentEntity.LogicalName, metaData);

			List<Guid> relatedIds = GetRelatedIds(currentEntity.Id, metaData.IntersectEntityName, relationshipInfo.Key.IntersectAttribute, relationshipInfo.Value.IntersectAttribute);

			foreach (Guid entityId in relatedIds)
			{
				Entity relatedEntity = Connection.Service.Retrieve(relationshipInfo.Value.LogicalName, entityId, columns);
				RelatedEntityType relatedObject = crmEntityCreater(relatedEntity);
				relatedObjects.Add(relatedObject);
			}

			return relatedObjects;
		}

		private List<Guid> GetRelatedIds(Guid originId, string intersectEntityName, string originIntersectAttribute, string relatedIntersectAttribute)
		{
			ConditionExpression equalsOriginIdExpression = new ConditionExpression
			{
				AttributeName = originIntersectAttribute,
				Operator = ConditionOperator.Equal,
			};
			equalsOriginIdExpression.Values.Add(originId);

			FilterExpression filterExpression = new FilterExpression();
			filterExpression.Conditions.Add(equalsOriginIdExpression);

			QueryExpression query = new QueryExpression(intersectEntityName)
			{
				ColumnSet = new ColumnSet(relatedIntersectAttribute),
			};
			query.Criteria.AddFilter(filterExpression);

			EntityCollection relatedEntities = Connection.Service.RetrieveMultiple(query);

			return relatedEntities.Entities.Select(entity => (Guid)entity[relatedIntersectAttribute]).ToList();
		}

		public int CountNNRelationship(string relationshipName, string countIdName)
		{
			string numberAlias = "numberofentities";

			XDocument xDocument = new XDocument(
			new XElement("fetch", new XAttribute("aggregate", true),
				new XElement("entity", new XAttribute("name", entityName),
					new XElement("link-entity", new XAttribute("from", idName), new XAttribute("name", relationshipName), new XAttribute("to", idName),
						new XElement("attribute", new XAttribute("name", countIdName), new XAttribute("aggregate", "count"), new XAttribute("alias", numberAlias))),
					new XElement("filter",
						new XElement("condition", new XAttribute("attribute", idName), new XAttribute("operator", "eq"), new XAttribute("value", Id))))));

			FetchExpression fetchExpression = new FetchExpression(xDocument.ToString());

			EntityCollection entityCollection = Connection.Service.RetrieveMultiple(fetchExpression);

			int collectionCount = entityCollection.Entities.Count;

			if (collectionCount == 0)
			{
				return 0;
			}

			if (collectionCount != 1)
			{
				throw new Exception("wrong collection count");
			}

			Entity entity = entityCollection.Entities[0];

			AliasedValue aliasedValue = (AliasedValue)entity.Attributes[numberAlias];

			return (int)aliasedValue.Value;
		}

		public void Delete()
		{
			Connection.Service.Delete(entityName, Id);
		}

		public void Insert()
		{
			Entity crmEntity = GetAsEntity(false);

			Id = Connection.Service.Create(crmEntity);

			Entity generatedEntity = Connection.Service.Retrieve(entityName, Id, ColumnSetCrmGenerated);
			ReadCrmGeneratedFields(generatedEntity);

			AfterInsert(generatedEntity);
		}

		public void InsertWithoutRead()
		{
			Entity crmEntity = GetAsEntity(false);

			Id = Connection.Service.Create(crmEntity);
		}

		protected Entity GetAsIdEntity()
		{
			Entity crmEntity = new Entity(entityName);

			crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
			crmEntity.Id = Id;

			return crmEntity;
		}

		protected EntityReference GetAsEntityReference()
		{
			EntityReference entityReference = new EntityReference(entityName, Id);

			return entityReference;
		}

		public bool Assign()
		{
			if (Id == Guid.Empty)
			{
				throw new ArgumentException($"Assign can only be called on entities with id, the entity is {entityName}");
			}

			string ownerEntityName = GetOwnerEntityName();
			ownerid.LogicalName = ownerEntityName;

			AssignRequest assignRequest = new AssignRequest()
			{
				Assignee = ownerid,
				Target = GetAsEntityReference(),
			};

			try
			{
				Connection.Service.Execute(assignRequest);
			}

			catch (System.ServiceModel.FaultException<OrganizationServiceFault>)
			{
				return false;
			}

			return true;
		}

		private string GetOwnerEntityName()
		{
			SystemUser user = SystemUser.ReadFromFetchXml(Connection, new List<string>() { "systemuserid" }, new Dictionary<string, string>() { { "systemuserid", owner.ToString() } }).SingleOrDefault();
			if (user != null)
			{
				return "systemuser";
			}

			Team team = Team.ReadFromFetchXml(Connection, new List<string>() { "teamid" }, new Dictionary<string, string>() { { "teamid", owner.ToString() } }).SingleOrDefault();
			if (user == null)
			{
				return "team";
			}

			throw new ArgumentException($"no user or team found on id {owner}");
		}

		public void Update()
		{
			Entity crmEntity = GetAsEntity(true);

			Connection.Service.Update(crmEntity);

			Entity generatedEntity = Connection.Service.Retrieve(entityName, Id, ColumnSetCrmGenerated);
			ReadCrmGeneratedFields(generatedEntity);

			AfterUpdate(generatedEntity);
		}

		public static void Update(IDynamicsCrmConnection dynamicsCrmConnection, string entityName, string entityIdName, Guid entityId, Dictionary<string, object> values)
		{
			Entity crmEntity = new Entity(entityName);

			crmEntity.Attributes.Add(new KeyValuePair<string, object>(entityIdName, entityId));
			crmEntity.Attributes.AddRange(values);

			crmEntity.Id = entityId;

			dynamicsCrmConnection.Service.Update(crmEntity);
		}

		private void ReadCrmGeneratedFields(Entity entity)
		{
			foreach (string key in ColumnSetCrmGenerated.Columns)
			{
				Utilities.ReflectionHelper.SetValue(this, key, entity.Attributes[key]);
			}
		}

		protected void AddRelated(string relationshipName, string relatedEntityName, List<Guid> idsToAdd)
		{
			if (idsToAdd.Any() == false)
			{
				return;
			}

			Relationship relationShip = new Relationship(relationshipName);
			EntityReferenceCollection entityReferenceCollection = CreateReferenceCollection(relationshipName, relatedEntityName, idsToAdd);

			Connection.Service.Associate(entityName, Id, relationShip, entityReferenceCollection);
		}

		protected void RemoveRelated(string relationshipName, string relatedEntityName, List<Guid> idsToRemove)
		{
			if (idsToRemove.Any() == false)
			{
				return;
			}

			Relationship relationShip = new Relationship(relationshipName);
			EntityReferenceCollection entityReferenceCollection = CreateReferenceCollection(relationshipName, relatedEntityName, idsToRemove);

			Connection.Service.Disassociate(entityName, Id, relationShip, entityReferenceCollection);
		}

		protected void DeleteRelated(string relatedEntityName, List<Guid> idsToDelete)
		{
			if (idsToDelete.Any() == false)
			{
				return;
			}

			foreach (Guid id in idsToDelete)
			{
				Connection.Service.Delete(relatedEntityName, id);
			}
		}

		private EntityReferenceCollection CreateReferenceCollection(string relationshipName, string entityName, List<Guid> idsToCollect)
		{
			EntityReferenceCollection entityReferenceCollection = new EntityReferenceCollection();

			foreach (Guid id in idsToCollect)
			{
				EntityReference entityReference = new EntityReference(entityName, id);
				entityReferenceCollection.Add(entityReference);
			}

			return entityReferenceCollection;
		}

		protected enum SynchronizeActionEnum
		{
			Disassociate = 1,
			Delete = 2,
		}

		protected void SynchronizeNNRelationship(Entity currentEntity, string relationshipName, string relatedEntityName, string relatedKeyName, List<Guid> localIds, SynchronizeActionEnum synchronizeAction)
		{
			EntityCollection relatedEntities = GetRelatedEntities(currentEntity, relationshipName);

			Dictionary<Guid, Entity> entitiesByKey = relatedEntities.Entities.ToDictionary(entity => entity.GetAttributeValue<Guid>(relatedKeyName));
			List<Guid> remoteIds = relatedEntities.Entities.Select(entity => entity.GetAttributeValue<Guid>(relatedKeyName)).ToList();

			List<Guid> localButNotInCrm = localIds.Where(id => entitiesByKey.ContainsKey(id) == false).ToList();

			List<Guid> remoteButNotLocal = remoteIds.Where(remoteId => localIds.Any(localId => localId == remoteId) == false).ToList();

			AddRelated(relationshipName, relatedEntityName, localButNotInCrm);

			if (synchronizeAction.HasFlag(SynchronizeActionEnum.Disassociate))
			{
				RemoveRelated(relationshipName, relatedEntityName, remoteButNotLocal);
			}
			if (synchronizeAction.HasFlag(SynchronizeActionEnum.Delete))
			{
				DeleteRelated(relatedEntityName, remoteButNotLocal);
			}
		}

		protected EntityCollection GetRelatedEntities(Entity currentEntity, string relationshipName)
		{
			Relationship relationShip = new Relationship(relationshipName);
			EntityCollection relatedEntities = currentEntity.RelatedEntities[relationShip];

			return relatedEntities;
		}

		protected Guid? GetEntityReferenceId(EntityReference entityReference)
		{
			if (entityReference == null)
			{
				return null;
			}

			return entityReference.Id;
		}

		protected EntityReference SetEntityReferenceId(Guid? id, string foreignEntityName)
		{
			if (id == null)
			{
				return null;
			}

			EntityReference entityReference = new EntityReference(foreignEntityName, id.Value);

			return entityReference;
		}

		protected enumType? GetOptionSet<enumType>(OptionSetValue enumValue)
		where enumType : struct, IComparable, IConvertible, IFormattable
		{
			if (enumValue == null)
			{
				return null;
			}

			return (enumType)Enum.Parse(typeof(enumType), enumValue.Value.ToString());
		}

		protected OptionSetValue SetOptionSet(int? value)
		{
			if (value == null)
			{
				return null;
			}

			return new OptionSetValue((int)value.Value);
		}

		protected static void CreateFromContent(IDynamicsCrmConnection dynamicsCrmConnection, AbstractCrm crmObject, Dictionary<string, string> allContent)
		{
			foreach (KeyValuePair<string, string> keyValue in allContent)
			{
				string key = keyValue.Key;
				string valueString = keyValue.Value;

				Type targetType = Utilities.ReflectionHelper.GetType(crmObject, key);
				if (targetType == null)
				{
					continue;
				}

				object value = Utilities.ReflectionHelper.StringToObject(valueString, targetType);

				Utilities.ReflectionHelper.SetValue(crmObject, key, value);
			}
		}
	}
}
