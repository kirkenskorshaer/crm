using Microsoft.Xrm.Sdk;
using System;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;

namespace TestUtilities
{
	public class OrganizationServiceTester : IOrganizationService
	{
		public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
		{
			throw new NotImplementedException();
		}

		public Guid Create(Entity entity)
		{
			throw new NotImplementedException();
		}

		public void Delete(string entityName, Guid id)
		{
			throw new NotImplementedException();
		}

		public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
		{
			throw new NotImplementedException();
		}

		public OrganizationResponse Execute(OrganizationRequest request)
		{
			throw new NotImplementedException();
		}

		public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
		{
			throw new NotImplementedException();
		}

		public Queue<EntityCollection> RetrieveMultipleQueue = new Queue<EntityCollection>();
		public List<QueryBase> RetrieveMultipleQueries = new List<QueryBase>();
		public EntityCollection RetrieveMultiple(QueryBase query)
		{
			if (RetrieveMultipleQueue.Count == 0)
			{
				string errorString = "";

				if (query is FetchExpression)
				{
					errorString = ((FetchExpression)query).Query;
				}

				throw new Exception($"RetrieveMultipleQueue queue was empty calling: {errorString}");
			}

			RetrieveMultipleQueries.Add(query);

			EntityCollection reply = RetrieveMultipleQueue.Dequeue();

			return reply;
		}

		public void Update(Entity entity)
		{
			throw new NotImplementedException();
		}
	}
}
