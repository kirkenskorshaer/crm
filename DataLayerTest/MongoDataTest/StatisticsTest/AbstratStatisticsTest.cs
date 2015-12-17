using DataLayer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayerTest.MongoDataTest.StatisticsTest
{
	[TestFixture]
	public abstract class AbstratStatisticsTest
	{
		protected MongoConnection Connection;

		[SetUp]
		public void SetUp()
		{
			Connection = MongoConnection.GetConnection("test");
			Connection.CleanDatabase();
		}
	}
}
