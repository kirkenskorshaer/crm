using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using NUnit.Framework;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class CreateActivity : TestBase
	{
		[Test]
		[Ignore]
		public void CreateSmsActivity()
		{
			string prefix = "new_";

			string customEntityName = prefix + "sms";

			CreateEntityRequest request = new CreateEntityRequest
			{
				HasNotes = true,
				HasActivities = false,
				PrimaryAttribute = new StringAttributeMetadata
				{
					SchemaName = "Subject",
					RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
					MaxLength = 100,
					DisplayName = new Label("Subject", 1033)
				},
				Entity = new EntityMetadata
				{
					IsActivity = true,
					SchemaName = customEntityName,
					DisplayName = new Label("SMS", 1033),
					DisplayCollectionName = new Label("SMS", 1033),
					OwnershipType = OwnershipTypes.UserOwned,
					IsAvailableOffline = true,
					IsMailMergeEnabled = new BooleanManagedProperty(false),
					ActivityTypeMask = 1,
				}
			};

			_dynamicsCrmConnection.Service.Execute(request);
		}
	}
}
