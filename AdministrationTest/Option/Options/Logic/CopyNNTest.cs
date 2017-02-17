using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using DatabaseCopyNN = DataLayer.MongoData.Option.Options.Logic.CopyNN;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class CopyNNTest : TestBase
	{
		[Test]
		[Ignore("")]
		public void copyNN()
		{
			DatabaseCopyNN databaseCopyNN = new DatabaseCopyNN()
			{
				originIntermediateEntityName = "new_account_contact_indsamlere",
				intermediateEntityRelationshipName = "new_campaignaccount_contact_indsamlingshlp",

				insertedEntityName = "new_campaignaccount",
				insertedEntityIdName = "new_campaignaccountid",
				insertedEntityOriginIdName = "new_accountid",

				originEntityIdName = "accountid",
				targetName = "contact",
				targetIdName = "contactid",

				insertedEntityId = Guid.Parse("182C965B-01F5-E611-811F-001C4215C4A0"),
			};

			CopyNN copyNN = new CopyNN(Connection, databaseCopyNN);

			copyNN.ExecuteOption(new Administration.Option.Options.OptionReport(GetType()));
		}
	}
}
