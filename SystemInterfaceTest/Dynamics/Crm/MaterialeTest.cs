using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class MaterialeTest : TestBase
	{
		private Contact _contact;
		private Account _account;
		private Materiale _materiale;
		private MaterialePakke _pakke;
		private MaterialeBehov _behov;
		private MaterialeBehovDefinition _definition;

		[SetUp]
		public void SetUp()
		{
			_contact = CreateTestContact();
			_contact.Insert();

			_account = new Account(_dynamicsCrmConnection);
			_account.name = $"test {Guid.NewGuid()}";
			_account.erindsamlingssted = Account.erindsamlingsstedEnum.Ja;
			_account.leveringkontaktid = _contact.Id;
			_account.Insert();

			_materiale = new Materiale(_dynamicsCrmConnection);
			_materiale.new_name = $"materiale {Guid.NewGuid()}";
			_materiale.behovsberegning = Materiale.behovsberegningEnum.Start;
			_materiale.Insert();

			_pakke = new MaterialePakke(_dynamicsCrmConnection);
			_pakke.new_name = $"test pakke 7 {Guid.NewGuid()}";
			_pakke.materialeid = _materiale.Id;
			_pakke.new_stoerrelse = 4;
			_pakke.Insert();

			_definition = new MaterialeBehovDefinition(_dynamicsCrmConnection);
			_definition.behovtype = MaterialeBehovDefinition.behovtypeEnum.Indsamlingssted;
			_definition.materialeid = _materiale.Id;
			_definition.new_antal = 10;
			_definition.new_name = $"test definition {Guid.NewGuid()}";
			_definition.Insert();

			_behov = new MaterialeBehov(_dynamicsCrmConnection);
			_behov.materialepakkeid = _pakke.Id;
			_behov.modtagerid = _account.Id;
			_behov.new_antal = 4;
			_behov.new_name = $"test behov {Guid.NewGuid()}";
			_behov.Insert();
		}

		[TearDown]
		public void TearDown()
		{
			_materiale.Delete();
			_pakke.Delete();
			_behov.Delete();
			_account.Delete();
			_contact.Delete();
		}

		[Test]
		public void MaterialeBehovDefinitionCanBeRecovered()
		{
			MaterialeBehovDefinition definitionCreated = new MaterialeBehovDefinition(_dynamicsCrmConnection);
			definitionCreated.behovtype = MaterialeBehovDefinition.behovtypeEnum.Indsamlingssted;
			definitionCreated.materialeid = _materiale.Id;
			definitionCreated.new_antal = 10;
			definitionCreated.Insert();

			MaterialeBehovDefinition definitionRead = MaterialeBehovDefinition.FindMaterialeBehovDefinitionPerMateriale(_dynamicsCrmConnection, _materiale.Id, _config.GetResourcePath).Single();

			Assert.AreEqual(definitionCreated.Id, definitionRead.Id);
		}

		[Test]
		public void MaterialePakkeCanBeRead()
		{
			List<MaterialePakke> materialePakker = _materiale.GetMaterialePakker();

			MaterialePakke pakkeRead = materialePakker.Single();

			Assert.AreEqual(_pakke.Id, pakkeRead.Id);
		}
	}
}
