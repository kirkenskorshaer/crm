using Administration.Option.Options.Logic;
using NUnit.Framework;
using DatabaseAggregateSubTable = DataLayer.MongoData.Option.Options.Logic.AggregateSubTable;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class AggregateSubTableTest : TestBase
	{
		[Test]
		public void AggregateCount()
		{
			DatabaseAggregateSubTable databaseAggregateSubTable = CreateDatabaseAggregateSubTableCount();

			AggregateSubTable aggregateSubTable = new AggregateSubTable(Connection, databaseAggregateSubTable);

			aggregateSubTable.Execute();
		}

		[Test]
		public void AggregateSum()
		{
			DatabaseAggregateSubTable databaseAggregateSubTable = CreateDatabaseAggregateSubTableSum();

			AggregateSubTable aggregateSubTable = new AggregateSubTable(Connection, databaseAggregateSubTable);

			aggregateSubTable.Execute();
		}

		private DatabaseAggregateSubTable CreateDatabaseAggregateSubTableCount()
		{
			DatabaseAggregateSubTable databaseAggregateSubTable = new DatabaseAggregateSubTable()
			{
				aggregate = DatabaseAggregateSubTable.AggregateEnum.count,
				topEntityName = "account",
				topEntityIdName = "accountid",
				topEntityReferenceIdName = "accountid",
				topEntityResultFieldName = "new_antalindsamlere",
				aggregateReferenceIdName = "new_indsamler2016",
				Name = "test",
				aggregateEntityName = "contact",
				aggregateFieldName = "contactid",
				aggregateEntityIdName = "contactid",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
			};

			return databaseAggregateSubTable;
		}

		private DatabaseAggregateSubTable CreateDatabaseAggregateSubTableSum()
		{
			DatabaseAggregateSubTable databaseAggregateSubTable = new DatabaseAggregateSubTable()
			{
				aggregate = DatabaseAggregateSubTable.AggregateEnum.sum,
				topEntityName = "contact",
				topEntityIdName = "contactid",
				topEntityReferenceIdName = "contactid",
				topEntityResultFieldName = "new_antalaccounts",
				aggregateReferenceIdName = "new_indsamlingskoordinatorid",
				Name = "test",
				aggregateEntityName = "account",
				aggregateFieldName = "new_antalindsamlere",
				aggregateEntityIdName = "accountid",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
			};

			return databaseAggregateSubTable;
		}
	}
}
