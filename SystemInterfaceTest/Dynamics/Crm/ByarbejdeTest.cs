using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class ByarbejdeTest : TestBase
	{
		[Test]
		public void InsertCreatesNewByarbejde()
		{
			DateTime testDate = DateTime.Now;
			Byarbejde byarbejdeInserted = CreateTestByarbejde(testDate);

			byarbejdeInserted.Insert();
			Byarbejde byarbejdeRead = Byarbejde.Read(_dynamicsCrmConnection, byarbejdeInserted.Id);
			byarbejdeInserted.Delete();

			Assert.AreEqual(byarbejdeInserted.new_name, byarbejdeRead.new_name);
		}

		[Test]
		public void ReadByNameReadsInsertedByarbejde()
		{
			DateTime testDate = DateTime.Now;
			Byarbejde byarbejdeInserted = CreateTestByarbejde(testDate);

			byarbejdeInserted.Insert();
			List<Byarbejde> byarbejdeRead = Byarbejde.Read(_dynamicsCrmConnection, byarbejdeInserted.new_name);
			byarbejdeInserted.Delete();

			Assert.AreEqual(byarbejdeInserted.Id, byarbejdeRead.Single().Id);
		}

		private Byarbejde CreateTestByarbejde(DateTime testDate)
		{
			string dateString = testDate.ToString("yyyy_MM_dd_HH_mm_ss");
			Byarbejde groupCreated = new Byarbejde(_dynamicsCrmConnection)
			{
				new_name = $"name_{dateString}",
			};
			return groupCreated;
		}


	}
}
