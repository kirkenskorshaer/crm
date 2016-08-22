using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using SystemInterface.Dynamics.Crm;

namespace TestUtilities
{
	public class DynamicsCrmConnectionTester : IDynamicsCrmConnection
	{
		public DynamicsCrmConnectionTester()
		{
			Service = new OrganizationServiceTester();
		}

		public OrganizationServiceContext Context { get; }
		public IOrganizationService Service { get; }

		public void EnqueueRetrieveMultiple(string entityName, List<Dictionary<string, object>> entityDictionaryList)
		{
			List<Entity> entities = new List<Entity>();

			foreach (Dictionary<string, object> entityDictionary in entityDictionaryList)
			{
				Entity entity = new Entity(entityName);
				foreach (KeyValuePair<string, object> entityAttribute in entityDictionary)
				{
					entity.Attributes.Add(entityAttribute);
				}
				entities.Add(entity);
			}
			EntityCollection collection = new EntityCollection(entities);

			((OrganizationServiceTester)Service).RetrieveMultipleQueue.Enqueue(collection);
		}
	}
}
