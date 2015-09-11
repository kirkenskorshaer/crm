using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Administration.Option.Options.Service;
using DataLayer.MongoData;
using NUnit.Framework;

namespace AdministrationTest.Option.Options.Service
{
	[TestFixture]
	public class ServiceDeleteTest : TestBase
	{
		[Test]
		[Ignore]
		public void ExecuteOptionTest()
		{
			DataLayer.MongoData.Option.Options.Service.ServiceDelete databaseServiceStart = new DataLayer.MongoData.Option.Options.Service.ServiceDelete()
			{
				Ip = "127.0.0.1",
				Name = "localHost",
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

			ServiceDelete serviceDelete = new ServiceDelete(Connection, databaseServiceStart);

			serviceDelete.Execute();
		}
	}
}
