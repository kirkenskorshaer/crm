using Administration.Option.Options.Logic.SumIndbetalingData;
using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using DatabaseSumIndbetaling = DataLayer.MongoData.Option.Options.Logic.SumIndbetaling;

namespace Administration.Option.Options.Logic
{
	public class SumIndbetaling : OptionBase
	{
		private DatabaseSumIndbetaling _databaseSumIndbetaling;

		public SumIndbetaling(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSumIndbetaling = (DatabaseSumIndbetaling)databaseOption;
		}

		protected override void ExecuteOption(OptionReport report)
		{
			string urlLoginName = _databaseSumIndbetaling.urlLoginName;

			SetDynamicsCrmConnectionIfEmpty(urlLoginName);

			IEnumerable<Indbetaling> indbetalings = Indbetaling.GetIndbetalingValue(_dynamicsCrmConnection);

			IndbetalingSumCollection byarbejdeCollection = new IndbetalingSumCollection(IndbetalingSumCollection.CollectionTypeEnum.byarbejde);
			IndbetalingSumCollection campaignCollection = new IndbetalingSumCollection(IndbetalingSumCollection.CollectionTypeEnum.campaign);
			IndbetalingSumCollection campaignCollectionBy = new IndbetalingSumCollection(IndbetalingSumCollection.CollectionTypeEnum.campaign);
			IndbetalingSumCollection campaignCollectionKreds = new IndbetalingSumCollection(IndbetalingSumCollection.CollectionTypeEnum.campaign);
			IndbetalingSumCollection indsamlingsstedCollection = new IndbetalingSumCollection(IndbetalingSumCollection.CollectionTypeEnum.indsamlingssted);
			IndbetalingSumCollection kontoCollection = new IndbetalingSumCollection(IndbetalingSumCollection.CollectionTypeEnum.konto);

			Dictionary<Guid, IndbetalingSumCollection> byarbejdesumByCampaign = new Dictionary<Guid, IndbetalingSumCollection>();
			Dictionary<Guid, IndbetalingSumCollection> indsamlingsstedByCampaign = new Dictionary<Guid, IndbetalingSumCollection>();

			int indbetalingCount = 0;
			foreach (Indbetaling indbetaling in indbetalings)
			{
				indbetalingCount++;

				if (indbetaling.kilde == null)
				{
					report.TextBuilder.AppendLine($"indbetaling {indbetaling.Id} has no kilde");
					continue;
				}

				AddIndbetalingToSums(indbetaling, byarbejdeCollection, campaignCollection, campaignCollectionBy, campaignCollectionKreds, indsamlingsstedCollection, kontoCollection, byarbejdesumByCampaign, indsamlingsstedByCampaign);
			}

			report.Workload = indbetalingCount;

			WriteByarbejdeAmount(byarbejdeCollection, byarbejdesumByCampaign);
			WriteCampaignAmount(campaignCollection, campaignCollectionBy, campaignCollectionKreds);
			WriteIndsamlingsstedAmount(indsamlingsstedCollection, indsamlingsstedByCampaign);
			WriteKontoAmount(kontoCollection);

			report.Success = true;
		}

		private void WriteByarbejdeAmount(IndbetalingSumCollection byarbejdeCollection, Dictionary<Guid, IndbetalingSumCollection> byarbejdesumByCampaign)
		{
			IEnumerable<IGrouping<Guid, IndbetalingSumPart>> byarbejdeById = byarbejdeCollection.indbetalingParts.Where(part => part.Kilde == null).GroupBy(part => part.Id);

			foreach (IGrouping<Guid, IndbetalingSumPart> byarbejdeGroup in byarbejdeById)
			{
				Byarbejde.WriteIndbetalingsum(_dynamicsCrmConnection, byarbejdeGroup.Key, byarbejdeGroup.Single().Amount);
			}
		}

		private void WriteCampaignAmount(IndbetalingSumCollection campaignCollection, IndbetalingSumCollection campaignCollectionBy, IndbetalingSumCollection campaignCollectionKreds)
		{
			IEnumerable<Guid> campaignIdTotal = campaignCollection.indbetalingParts.Select(part => part.Id);
			IEnumerable<Guid> campaignIdBy = campaignCollectionBy.indbetalingParts.Select(part => part.Id);
			IEnumerable<Guid> campaignIdKreds = campaignCollectionKreds.indbetalingParts.Select(part => part.Id);

			List<Guid> campaignids = campaignIdTotal.Union(campaignIdBy).Union(campaignIdKreds).ToList();

			foreach (Guid campaignid in campaignids)
			{
				decimal indbetalingsum = campaignCollection.indbetalingParts.Where(part => part.Id == campaignid && part.Kilde == null).Single().Amount;
				decimal indbetalingsumBy = campaignCollectionBy.indbetalingParts.Where(part => part.Id == campaignid && part.Kilde == null).Single().Amount;
				decimal indbetalingsumKreds = campaignCollectionKreds.indbetalingParts.Where(part => part.Id == campaignid && part.Kilde == null).Single().Amount;

				decimal? indbetalingsumBankoverfoersel = campaignCollection.indbetalingParts.Where(part => part.Id == campaignid && part.Kilde == Indbetaling.kildeEnum.Bankoverfoersel).SingleOrDefault()?.Amount;
				decimal? indbetalingsumGiro = campaignCollection.indbetalingParts.Where(part => part.Id == campaignid && part.Kilde == Indbetaling.kildeEnum.Giro).SingleOrDefault()?.Amount;
				decimal? indbetalingsumKontant = campaignCollection.indbetalingParts.Where(part => part.Id == campaignid && part.Kilde == Indbetaling.kildeEnum.Kontant).SingleOrDefault()?.Amount;
				decimal? indbetalingsumMobilePay = campaignCollection.indbetalingParts.Where(part => part.Id == campaignid && part.Kilde == Indbetaling.kildeEnum.MobilePay).SingleOrDefault()?.Amount;
				decimal? indbetalingsumSms = campaignCollection.indbetalingParts.Where(part => part.Id == campaignid && part.Kilde == Indbetaling.kildeEnum.Sms).SingleOrDefault()?.Amount;
				decimal? indbetalingsumSwipp = campaignCollection.indbetalingParts.Where(part => part.Id == campaignid && part.Kilde == Indbetaling.kildeEnum.Swipp).SingleOrDefault()?.Amount;
				decimal? indbetalingsumUkendt = campaignCollection.indbetalingParts.Where(part => part.Id == campaignid && part.Kilde == Indbetaling.kildeEnum.Ukendt).SingleOrDefault()?.Amount;

				Campaign.WriteIndbetalingSums
				(
					_dynamicsCrmConnection, campaignid,
					indbetalingsum, indbetalingsumBy, indbetalingsumKreds,
					indbetalingsumBankoverfoersel, indbetalingsumGiro, indbetalingsumKontant, indbetalingsumMobilePay, indbetalingsumSms, indbetalingsumSwipp, indbetalingsumUkendt
				);
			}
		}

		private void WriteCampaignAmount(IndbetalingSumCollection campaignCollection, Action<DynamicsCrmConnection, Guid, decimal> writeMethod)
		{

		}

		private void WriteIndsamlingsstedAmount(IndbetalingSumCollection indsamlingsstedCollection, Dictionary<Guid, IndbetalingSumCollection> indsamlingsstedByCampaign)
		{
			IEnumerable<IGrouping<Guid, IndbetalingSumPart>> indsamlingsstedById = indsamlingsstedCollection.indbetalingParts.Where(part => part.Kilde == null).GroupBy(part => part.Id);

			foreach (IGrouping<Guid, IndbetalingSumPart> indsamlingsstedGroup in indsamlingsstedById)
			{
				Account.WriteIndbetalingsum(_dynamicsCrmConnection, indsamlingsstedGroup.Key, indsamlingsstedGroup.Single().Amount);
			}
		}

		private void WriteKontoAmount(IndbetalingSumCollection kontoCollection)
		{
			IEnumerable<IGrouping<Guid, IndbetalingSumPart>> kontoById = kontoCollection.indbetalingParts.Where(part => part.Kilde == null).GroupBy(part => part.Id);

			foreach (IGrouping<Guid, IndbetalingSumPart> kontoGroup in kontoById)
			{
				Konto.WriteIndbetalingsum(_dynamicsCrmConnection, kontoGroup.Key, kontoGroup.Single().Amount);
			}
		}

		private void AddIndbetalingToSums
		(
			Indbetaling indbetaling, IndbetalingSumCollection byarbejdeCollection,
			IndbetalingSumCollection campaignCollection,
			IndbetalingSumCollection campaignCollectionBy,
			IndbetalingSumCollection campaignCollectionKreds,
			IndbetalingSumCollection indsamlingsstedCollection,
			IndbetalingSumCollection kontoCollection,
			Dictionary<Guid, IndbetalingSumCollection> byarbejdesumByCampaign,
			Dictionary<Guid, IndbetalingSumCollection> indsamlingsstedByCampaign
		)
		{
			kontoCollection.Add(indbetaling);
			byarbejdeCollection.Add(indbetaling);
			indsamlingsstedCollection.Add(indbetaling);

			Guid? campaignid = indbetaling.campaignid;
			if (campaignid.HasValue == false)
			{
				return;
			}

			campaignCollection.Add(indbetaling);

			if (indbetaling.bykoordinatorid.HasValue)
			{
				campaignCollectionBy.Add(indbetaling);
			}

			if (indbetaling.omraadekoordinatorid.HasValue)
			{
				campaignCollectionKreds.Add(indbetaling);
			}

			if (byarbejdesumByCampaign.ContainsKey(campaignid.Value) == false)
			{
				byarbejdesumByCampaign.Add(campaignid.Value, new IndbetalingSumCollection(IndbetalingSumCollection.CollectionTypeEnum.byarbejde));
			}
			byarbejdesumByCampaign[campaignid.Value].Add(indbetaling);

			if (indsamlingsstedByCampaign.ContainsKey(campaignid.Value) == false)
			{
				indsamlingsstedByCampaign.Add(campaignid.Value, new IndbetalingSumCollection(IndbetalingSumCollection.CollectionTypeEnum.indsamlingssted));
			}
			indsamlingsstedByCampaign[campaignid.Value].Add(indbetaling);
		}

		public static List<SumIndbetaling> Find(MongoConnection connection)
		{
			List<DatabaseSumIndbetaling> options = DatabaseSumIndbetaling.ReadAllowed<DatabaseSumIndbetaling>(connection);

			return options.Select(option => new SumIndbetaling(connection, option)).ToList();
		}
	}
}
