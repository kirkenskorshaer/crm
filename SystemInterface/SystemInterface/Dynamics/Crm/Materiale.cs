using System;
using System.Collections.Generic;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Linq;
using System.Xml.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class Materiale : AbstractCrm
	{
		public string new_name;
		public decimal? new_antalfaktor;

		public OptionSetValue new_behovsberegning;
		public behovsberegningEnum? behovsberegning { get { return GetOptionSet<behovsberegningEnum>(new_behovsberegning); } set { new_behovsberegning = SetOptionSet((int?)value); } }

		public int? new_beregningsstatus;

		public EntityReference new_leverandoerid;
		public Guid? leverandoerid { get { return GetEntityReferenceId(new_leverandoerid); } set { new_leverandoerid = SetEntityReferenceId(value, "contact"); } }

		public OptionSetValue new_materialetype;

		private int _numberOfAccountsPerMaterialeAssign = 15;

		private static readonly ColumnSet ColumnSetMaterialer = new ColumnSet(
			"new_name",
			"new_materialeid",
			"new_antalfaktor",
			"new_behovsberegning",
			"new_beregningsstatus",
			"new_leverandoerid",
			"new_materialetype"
		);

		private static readonly ColumnSet ColumnSetMaterialeCrmGenerated = new ColumnSet("createdon");
		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetMaterialeCrmGenerated; } }

		protected override string entityName { get { return "new_materiale"; } }
		protected override string idName { get { return "new_materialeid"; } }

		private string _materialePakkeRelationshipName = "new_materiale_materialepakke_Materiale";

		public Materiale(DynamicsCrmConnection connection) : base(connection)
		{
		}

		public Materiale(DynamicsCrmConnection connection, Entity entity) : base(connection, entity)
		{
		}

		public enum behovsberegningEnum
		{
			Start = 100000000,
			Afsluttet = 100000001,
		}

		protected override CrmEntity GetAsEntity(bool includeId)
		{
			CrmEntity crmEntity = new CrmEntity(entityName);

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_name", new_name));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_antalfaktor", new_antalfaktor));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_behovsberegning", new_behovsberegning));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_beregningsstatus", new_beregningsstatus));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_leverandoerid", new_leverandoerid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_materialetype", new_materialetype));

			return crmEntity;
		}

		private CrmEntity GetAsUpdateEntity()
		{
			CrmEntity crmEntity = new CrmEntity(entityName);

			crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
			crmEntity.Id = Id;

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_behovsberegning", new_behovsberegning));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_beregningsstatus", new_beregningsstatus));

			return crmEntity;
		}

		public static Materiale ReadCalculationNeed(DynamicsCrmConnection dynamicsCrmConnection, Func<string, string> getResourcePath)
		{
			string path = getResourcePath("Dynamics/Crm/FetchXml/Materiale/MaterialeNeedingUpdate.xml");

			Materiale materiale = StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, path, (connection, entity) => new Materiale(connection, entity), new PagingInformation()).FirstOrDefault();

			return materiale;
		}

		public void RemoveStaleMaterialeBehov(Func<string, string> getResourcePath)
		{
			string path = getResourcePath("Dynamics/Crm/FetchXml/Materiale/FindStaleMaterialeBehov.xml");
			XDocument xDocument = XDocument.Load(path);
			xDocument.Element("fetch").Element("entity").Element("link-entity").Element("filter").Element("condition").Attribute("value").Value = Id.ToString();

			List<MaterialeBehov> materialeBehovList = StaticCrm.ReadFromFetchXml(Connection, xDocument, (connection, entity) => new MaterialeBehov(connection, entity), new PagingInformation());

			foreach (MaterialeBehov materialeBehov in materialeBehovList)
			{
				materialeBehov.Delete();
			}
		}

		public List<MaterialeBehovDefinition> BehovDefinitioner { get; private set; }
		public void FindBehovDefinitioner(Func<string, string> getResourcePath)
		{
			BehovDefinitioner = MaterialeBehovDefinition.FindMaterialeBehovDefinitionPerMateriale(Connection, Id, getResourcePath, new PagingInformation());
		}

		public void AddMissingMateriale(MaterialeProcessState state, Func<string, string> getResourcePath)
		{
			List<MaterialePakke> pakker = GetMaterialePakker();
			pakker = pakker.OrderBy(pakke => pakke.new_stoerrelse).ToList();

			List<Account> accounts = GetAccountsToAddMaterialeBehov(BehovDefinitioner, state, getResourcePath);

			foreach (Account account in accounts)
			{
				Dictionary<MaterialePakke, int> materialePakkerOnAccount = new Dictionary<MaterialePakke, int>();

				decimal materialeNeededCount = GetMaterialeNeededCount(BehovDefinitioner, account) * new_antalfaktor.GetValueOrDefault(1);
				int currentSize = 0;

				currentSize = account.CountMaterialeBehov(Id, getResourcePath);

				FindMaterialeNeed(pakker, materialePakkerOnAccount, materialeNeededCount, currentSize);

				CreateMaterialeBehov(account, materialePakkerOnAccount);
			}

			state.AccountsProcessed = accounts.Count;
		}

		private static void FindMaterialeNeed(List<MaterialePakke> pakker, Dictionary<MaterialePakke, int> materialePakkerOnAccount, decimal materialeNeededCount, int currentSize)
		{
			while (currentSize < materialeNeededCount)
			{
				for (int pakkeIndex = 0; pakkeIndex < pakker.Count; pakkeIndex++)
				{
					if ((pakker[pakkeIndex].new_stoerrelse + currentSize) > materialeNeededCount || pakkeIndex == pakker.Count - 1)
					{
						MaterialePakke pakke = pakker[pakkeIndex];
						currentSize += pakke.new_stoerrelse.GetValueOrDefault(1);

						if (materialePakkerOnAccount.ContainsKey(pakke))
						{
							materialePakkerOnAccount[pakke]++;
						}
						else
						{
							materialePakkerOnAccount.Add(pakke, 1);
						}

						break;
					}
				}
			}
		}

		private void CreateMaterialeBehov(Account account, Dictionary<MaterialePakke, int> materialePakkerOnAccount)
		{
			foreach (KeyValuePair<MaterialePakke, int> materialePakkeAndCount in materialePakkerOnAccount)
			{
				MaterialeBehov behov = new MaterialeBehov(Connection)
				{
					new_name = $"auto defineret {DateTime.Now.ToString("yyyyMMdd")}",
					new_antal = materialePakkeAndCount.Value,
					ownerid = account.ownerid,
					forsendelsestatus = MaterialeBehov.forsendelsestatusEnum.Oprettet,
					materialepakkeid = materialePakkeAndCount.Key.Id,
					leveringsstedid = GetLeveringsSted(account),
					modtagerid = account.Id,
					kontaktpersonid = GetKontaktPerson(account)
				};

				behov.Insert();
			}
		}

		private Guid? GetLeveringsSted(Account account)
		{
			if (account.leveringstedid.HasValue)
			{
				return account.leveringstedid;
			}

			return account.Id;
		}

		private Guid? GetKontaktPerson(Account account)
		{
			if (account.leveringkontaktid.HasValue)
			{
				return account.leveringkontaktid;
			}

			return null;
		}

		private List<Account> GetAccountsToAddMaterialeBehov(List<MaterialeBehovDefinition> behovDefinitioner, MaterialeProcessState state, Func<string, string> getResourcePath)
		{
			List<Account> accounts = new List<Account>();

			switch (state.BehovType)
			{
				case MaterialeBehovDefinition.behovtypeEnum.Indsamlingssted:
					GetAccountsIndsamlingssted(behovDefinitioner, accounts, state.owningbusinessunit, getResourcePath, state.pagingInformation);
					if (accounts.Count == 0)
					{
						state.BehovType = MaterialeBehovDefinition.behovtypeEnum.ForventetAntalIndsamlere2016;
						return GetAccountsToAddMaterialeBehov(behovDefinitioner, state, getResourcePath);
					}
					break;
				case MaterialeBehovDefinition.behovtypeEnum.ForventetAntalIndsamlere2016:
					GetAccountsForventetAntalIndsamlere2016(behovDefinitioner, accounts, state.owningbusinessunit, getResourcePath, state.pagingInformation);
					if (accounts.Count == 0)
					{
						state.BehovType = MaterialeBehovDefinition.behovtypeEnum.Indsamlingshjaelper;
						return GetAccountsToAddMaterialeBehov(behovDefinitioner, state, getResourcePath);
					}
					break;
				case MaterialeBehovDefinition.behovtypeEnum.Indsamlingshjaelper:
					GetAccountsIndsamlingshjaelper(behovDefinitioner, accounts, state.owningbusinessunit, getResourcePath, state.pagingInformation);
					if (accounts.Count == 0)
					{
						state.pagingInformation.Reset();
					}
					break;
				default:
					break;
			}

			return accounts;
		}

		private void GetAccountsIndsamlingssted(List<MaterialeBehovDefinition> behovDefinitioner, List<Account> accounts, Guid owningbusinessunit, Func<string, string> getResourcePath, PagingInformation pagingInformation)
		{
			if (behovDefinitioner.Any(definition => definition.behovtype == MaterialeBehovDefinition.behovtypeEnum.Indsamlingssted))
			{
				accounts.AddRange(Account.GetIndsamlingsSted(Connection, _numberOfAccountsPerMaterialeAssign, owningbusinessunit, getResourcePath, pagingInformation));
			}
		}

		private void GetAccountsForventetAntalIndsamlere2016(List<MaterialeBehovDefinition> behovDefinitioner, List<Account> accounts, Guid owningbusinessunit, Func<string, string> getResourcePath, PagingInformation pagingInformation)
		{
			if (behovDefinitioner.Any(definition => definition.behovtype == MaterialeBehovDefinition.behovtypeEnum.ForventetAntalIndsamlere2016))
			{
				accounts.AddRange(Account.GetForventetAntalIndsamlere2016(Connection, _numberOfAccountsPerMaterialeAssign, owningbusinessunit, getResourcePath, pagingInformation));
			}
		}

		private void GetAccountsIndsamlingshjaelper(List<MaterialeBehovDefinition> behovDefinitioner, List<Account> accounts, Guid owningbusinessunit, Func<string, string> getResourcePath, PagingInformation pagingInformation)
		{
			if (behovDefinitioner.Any(definition => definition.behovtype == MaterialeBehovDefinition.behovtypeEnum.Indsamlingshjaelper))
			{
				accounts.AddRange(Account.GetIndsamlingshjaelper(Connection, _numberOfAccountsPerMaterialeAssign, owningbusinessunit, getResourcePath, pagingInformation));
			}
		}

		private int GetMaterialeNeededCount(List<MaterialeBehovDefinition> behovDefinitioner, Account account)
		{
			int needCount = 0;
			foreach (MaterialeBehovDefinition definition in behovDefinitioner)
			{
				switch (definition.behovtype.Value)
				{
					case MaterialeBehovDefinition.behovtypeEnum.Indsamlingssted:
						if (account.erindsamlingssted == Account.erindsamlingsstedEnum.Ja)
						{
							needCount += definition.new_antal.GetValueOrDefault(0);
						}
						break;
					case MaterialeBehovDefinition.behovtypeEnum.ForventetAntalIndsamlere2016:
						needCount += (account.new_forventetantalindsamlere2016.GetValueOrDefault(0) * definition.new_antal.GetValueOrDefault(0));
						break;
					case MaterialeBehovDefinition.behovtypeEnum.Indsamlingshjaelper:
						int indsamlingshjaelperCount = account.CountIndsamlingsHjaelper();
						needCount += (indsamlingshjaelperCount * definition.new_antal.GetValueOrDefault(0));
						break;
					default:
						break;
				}
			}

			return needCount;
		}

		public List<MaterialePakke> GetMaterialePakker()
		{
			Entity currentEntity = GetAsEntity(true);

			IEnumerable<Entity> pakkeEntities = GetRelatedEntities(currentEntity, _materialePakkeRelationshipName);

			return pakkeEntities.Select(pakkeEntity => new MaterialePakke(Connection, pakkeEntity)).ToList();
		}
	}
}
