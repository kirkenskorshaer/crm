using System;
using DataLayer;
using DatabaseCopyNN = DataLayer.MongoData.Option.Options.Logic.CopyNN;
using SystemInterface.Dynamics.Crm;
using System.Collections.Generic;

namespace Administration.Option.Options.Logic
{
	public class CopyNN : OptionBase
	{
		private DatabaseCopyNN _databaseCopyNN;

		public CopyNN(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseCopyNN = (DatabaseCopyNN)databaseOption;
		}

		public override void ExecuteOption(OptionReport report)
		{
			string insertedEntityName = _databaseCopyNN.insertedEntityName;
			string insertedEntityIdName = _databaseCopyNN.insertedEntityIdName;
			string insertedEntityOriginIdName = _databaseCopyNN.insertedEntityOriginIdName;

			SetDynamicsCrmConnectionIfEmpty();

			List<dynamic> insertedEntities = GetInsertedEntities(insertedEntityName, insertedEntityIdName, insertedEntityOriginIdName);

			foreach (dynamic insertedEntity in insertedEntities)
			{
				CreateInsertedIntermediateEntity(insertedEntityName, insertedEntityIdName, insertedEntityOriginIdName, insertedEntity);
			}
		}

		private void CreateInsertedIntermediateEntity(string insertedEntityName, string insertedEntityIdName, string insertedEntityOriginIdName, dynamic insertedEntity)
		{
			string originEntityIdName = _databaseCopyNN.originEntityIdName;
			string targetName = _databaseCopyNN.targetName;
			string targetIdName = _databaseCopyNN.targetIdName;
			string intermediateEntityRelationshipName = _databaseCopyNN.intermediateEntityRelationshipName;

			IDictionary<string, object> insertedEntityDictionary = (IDictionary<string, object>)insertedEntity;

			Guid insertedEntityId = (Guid)insertedEntityDictionary[insertedEntityIdName];
			Guid insertedEntityOriginId = StaticCrm.GetGuidFromReference(insertedEntityDictionary[insertedEntityOriginIdName]);

			List<dynamic> originIntermediateEntities = GetOriginIntermediateEntities(originEntityIdName, targetIdName, insertedEntityOriginId);

			foreach (dynamic originIntermediateEntity in originIntermediateEntities)
			{
				IDictionary<string, object> originIntermediateEntityDictionary = (IDictionary<string, object>)originIntermediateEntity;

				Guid targetId = (Guid)originIntermediateEntityDictionary[targetIdName];

				StaticCrm.AssociateNN(_dynamicsCrmConnection, intermediateEntityRelationshipName, insertedEntityName, insertedEntityId, targetName, targetId);
			}
		}

		private List<dynamic> GetOriginIntermediateEntities(string originEntityIdName, string targetIdName, Guid insertedEntityOriginId)
		{
			string originIntermediateEntityName = _databaseCopyNN.originIntermediateEntityName;

			List<string> originIntermediateEntityFields = new List<string>()
			{
				targetIdName,
			};

			Dictionary<string, string> originIntermediateKeyFields = new Dictionary<string, string>()
			{
				{ originEntityIdName, insertedEntityOriginId.ToString() }
			};

			List<dynamic> originIntermediateEntities = DynamicFetch.ReadFromFetchXml(_dynamicsCrmConnection, originIntermediateEntityName, originIntermediateEntityFields, originIntermediateKeyFields, null, new PagingInformation());
			return originIntermediateEntities;
		}

		private List<dynamic> GetInsertedEntities(string insertedEntityName, string insertedEntityIdName, string insertedEntityOriginIdName)
		{
			Guid? insertedEntityId = _databaseCopyNN.insertedEntityId;

			List<string> insertedFields = new List<string>()
			{
				insertedEntityIdName,
				insertedEntityOriginIdName,
			};

			Dictionary<string, string> insertedKeyFields = new Dictionary<string, string>();

			if (insertedEntityId.HasValue)
			{
				insertedKeyFields.Add(insertedEntityIdName, insertedEntityId.Value.ToString());
			}

			List<dynamic> insertedEntities = DynamicFetch.ReadFromFetchXml(_dynamicsCrmConnection, insertedEntityName, insertedFields, insertedKeyFields, null, new PagingInformation());

			return insertedEntities;
		}
	}
}
