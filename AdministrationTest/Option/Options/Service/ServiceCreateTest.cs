using Administration.Option.Options.Service;
using DataLayer.MongoData;
using NUnit.Framework;

namespace AdministrationTest.Option.Options.Service
{
	[TestFixture]
	public class ServiceCreateTest : TestBase
	{
		[Test]
		[Ignore]
		public void ExecuteOptionTest()
		{
			DataLayer.MongoData.Option.Options.Service.ServiceCreate databaseServiceCreate = new DataLayer.MongoData.Option.Options.Service.ServiceCreate()
			{
				Ip = "127.0.0.1",
				Name = "localHost",
				Path = "c:/test.test",
				Schedule = CreateSchedule(),
				ServiceName = "testService",
			};

			Server server = new Server()
			{
				Ip = "127.0.0.1",
				Password = "test",
				Username = "test",
			};

			server.Insert(Connection);

			ServiceCreate serviceCreate = new ServiceCreate(Connection, databaseServiceCreate);

			serviceCreate.Execute();
		}
	}
}
