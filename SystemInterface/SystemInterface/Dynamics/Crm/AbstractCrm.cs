using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public abstract class AbstractCrm
	{
		protected abstract string entityName { get; }
		protected abstract string idName { get; }
		protected abstract ColumnSet ColumnSetCrmGenerated { get; }

		protected DynamicsCrmConnection Connection { get; private set; }

		protected virtual void AfterInsert(Entity generatedEntity) { }
		protected virtual void AfterUpdate(Entity generatedEntity) { }

		public Guid Id { get; private set; }

		public AbstractCrm(DynamicsCrmConnection connection)
		{
			Connection = connection;
		}

		public AbstractCrm(DynamicsCrmConnection connection, Entity crmEntity)
		{
			Connection = connection;

			Id = (Guid)crmEntity.Attributes[idName];

			IEnumerable<string> keys = crmEntity.Attributes.Keys.Where(key => key != idName);

			foreach (string key in keys)
			{
				Utilities.ReflectionHelper.SetValue(this, key, crmEntity.Attributes[key]);
			}
		}

		protected abstract CrmEntity GetAsEntity(bool includeId);

		public List<RelatedEntityType> ReadNNRelationship<RelatedEntityType>(string relationshipName, Entity currentEntity, Func<Entity, RelatedEntityType> crmEntityCreater)
		{
			List<RelatedEntityType> relatedObjects = new List<RelatedEntityType>();

			Relationship relationShip = new Relationship(relationshipName);
			IEnumerable<Entity> relatedEntities = currentEntity.GetRelatedEntities(Connection.Context, relationShip);

			foreach (Entity relatedEntity in relatedEntities)
			{
				RelatedEntityType relatedObject = crmEntityCreater(relatedEntity);
				relatedObjects.Add(relatedObject);
			}

			return relatedObjects;
		}

		public void Delete()
		{
			Connection.Service.Delete(entityName, Id);
		}

		public void Insert()
		{
			CrmEntity crmEntity = GetAsEntity(false);

			Id = Connection.Service.Create(crmEntity);

			Entity generatedEntity = Connection.Service.Retrieve(entityName, Id, ColumnSetCrmGenerated);
			ReadCrmGeneratedFields(generatedEntity);

			AfterInsert(generatedEntity);
		}

		public void Update()
		{
			CrmEntity crmEntity = GetAsEntity(true);

			Connection.Service.Update(crmEntity);

			Entity generatedEntity = Connection.Service.Retrieve(entityName, Id, ColumnSetCrmGenerated);
			ReadCrmGeneratedFields(generatedEntity);

			AfterUpdate(generatedEntity);
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

		protected void SynchronizeNNRelationship(Entity currentEntity, string relationshipName, string relatedEntityName, string relatedKeyName, List<Guid> localIds)
		{
			IEnumerable<Entity> relatedEntities = GetRelatedEntities(currentEntity, relationshipName);

			Dictionary<Guid, Entity> entitiesByKey = relatedEntities.ToDictionary(entity => entity.GetAttributeValue<Guid>(relatedKeyName));
			List<Guid> remoteIds = relatedEntities.Select(entity => entity.GetAttributeValue<Guid>(relatedKeyName)).ToList();

			List<Guid> localButNotInCrm = localIds.Where(id => entitiesByKey.ContainsKey(id) == false).ToList();

			List<Guid> remoteButNotLocal = remoteIds.Where(remoteId => localIds.Any(localId => localId == remoteId) == false).ToList();

			AddRelated(relationshipName, relatedEntityName, localButNotInCrm);

			RemoveRelated(relationshipName, relatedEntityName, remoteButNotLocal);
		}

		protected IEnumerable<Entity> GetRelatedEntities(Entity currentEntity, string relationshipName)
		{
			Relationship relationShip = new Relationship(relationshipName);
			IEnumerable<Entity> relatedEntities = currentEntity.GetRelatedEntities(Connection.Context, relationShip);

			return relatedEntities;
		}
	}
}
