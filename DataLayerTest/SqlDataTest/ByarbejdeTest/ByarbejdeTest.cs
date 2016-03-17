using DataLayer.SqlData.Byarbejde;
using NUnit.Framework;
using System;

namespace DataLayerTest.SqlDataTest.ByarbejdeTest
{
	[TestFixture]
	public class ByarbejdeTest: TestBase
	{
		[Test]
		public void MaintainTable()
		{
			Byarbejde.MaintainTable(_sqlConnection);
		}

		internal Byarbejde ByarbejdeInsert()
		{
			Byarbejde createdByarbejde = new Byarbejde()
			{
				new_name = $"name_{Guid.NewGuid()}",
			};

			createdByarbejde.Insert(_sqlConnection);

			return createdByarbejde;
		}

		[Test]
		public void UpdateUpdatesByarbejde()
		{
			Byarbejde createdByarbejde = ByarbejdeInsert();
			createdByarbejde.new_name = $"name_{Guid.NewGuid()}";
			createdByarbejde.Update(_sqlConnection);

			Byarbejde byarbejdeRead = Byarbejde.ReadByName(_sqlConnection, createdByarbejde.new_name);

			createdByarbejde.Delete(_sqlConnection);
			Assert.AreEqual(createdByarbejde.new_name, byarbejdeRead.new_name);
		}

		[Test]
		public void ExistsByNameReturnsTrueForExistingByarbejde()
		{
			Byarbejde createdByarbejde = ByarbejdeInsert();

			bool byarbejdeExists = Byarbejde.ExistsByName(_sqlConnection, createdByarbejde.new_name);

			createdByarbejde.Delete(_sqlConnection);
			Assert.IsTrue(byarbejdeExists);
		}

		[Test]
		public void ExistsByNameReturnsFalseForDeletedByarbejde()
		{
			Byarbejde createdByarbejde = ByarbejdeInsert();
			createdByarbejde.Delete(_sqlConnection);

			bool byarbejdeExists = Byarbejde.ExistsByName(_sqlConnection, createdByarbejde.new_name);

			Assert.IsFalse(byarbejdeExists);
		}
	}
}
