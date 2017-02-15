using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;

namespace SystemInterface.Dynamics.Crm
{
	internal class EntityRelationshipInfo
	{
		internal static KeyValuePair<EntityRelationshipInfo, EntityRelationshipInfo> GetFromMetaData(string originLogicalName, ManyToManyRelationshipMetadata metaData)
		{
			EntityRelationshipInfo entityRelationship1 = new EntityRelationshipInfo
			{
				IntersectAttribute = metaData.Entity1IntersectAttribute,
				LogicalName = metaData.Entity1LogicalName,
				NavigationPropertyName = metaData.Entity1NavigationPropertyName,
			};

			EntityRelationshipInfo entityRelationship2 = new EntityRelationshipInfo
			{
				IntersectAttribute = metaData.Entity2IntersectAttribute,
				LogicalName = metaData.Entity2LogicalName,
				NavigationPropertyName = metaData.Entity2NavigationPropertyName,
			};

			if (metaData.Entity1LogicalName == originLogicalName)
			{
				return new KeyValuePair<EntityRelationshipInfo, EntityRelationshipInfo>(entityRelationship1, entityRelationship2);
			}
			else
			{
				return new KeyValuePair<EntityRelationshipInfo, EntityRelationshipInfo>(entityRelationship2, entityRelationship1);
			}
		}

		internal string IntersectAttribute;
		internal string LogicalName;
		internal string NavigationPropertyName;
	}
}
