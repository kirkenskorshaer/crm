using DataLayer;
using System.Collections.Generic;
using System.Linq;
using DatabaseImportDanskeBank = DataLayer.MongoData.Option.Options.Logic.ImportDanskeBank;
using System;
using System.IO;
using System.Xml.Linq;
using Administration.Option.Options.Logic.ImportDanskeBankData;
using SystemInterface.Dynamics.Crm;

namespace Administration.Option.Options.Logic
{
	public class ImportDanskeBank : OptionBase
	{
		private DatabaseImportDanskeBank _databaseImportDanskeBank;

		public ImportDanskeBank(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseImportDanskeBank = (DatabaseImportDanskeBank)databaseOption;
		}

		public override void ExecuteOption(OptionReport report)
		{
			string urlLoginName = _databaseImportDanskeBank.urlLoginName;
			string importFolder = _databaseImportDanskeBank.importFolder;

			SetDynamicsCrmConnectionIfEmpty(urlLoginName);

			string folder = Config.GetOrCreateResourcePath(importFolder);

			string[] files = Directory.GetFiles(folder, "*.xml");

			List<XDocument> bankXmlFiles = GetBankXml(files);

			bankXmlFiles.ForEach(ImportXml);

			files.ToList().ForEach(file => MoveFileToImported(file, folder));

			DataLayer.MongoData.Option.Schedule recalculateSchedule = new DataLayer.MongoData.Option.Schedule()
			{
				NextAllowedExecution = DateTime.Now,
				Recurring = false,
				TimeBetweenAllowedExecutions = TimeSpan.FromSeconds(60),
				Enabled = true,
				ActionOnFail = DataLayer.MongoData.Option.Schedule.ActionOnFailEnum.TryAgain,
			};
			DataLayer.MongoData.Option.Options.Logic.SumIndbetaling.Create(Connection, urlLoginName, "Auto Generated indbetaling calculation", recalculateSchedule);

			report.Success = true;
		}

		private void MoveFileToImported(string file, string folder)
		{
			string filename = file.Split('/', '\\').Last();
			string targetFolder = folder + "/imported/";

			if (Directory.Exists(targetFolder) == false)
			{
				Directory.CreateDirectory(targetFolder);
			}

			File.Move(file, targetFolder + filename);
		}

		private void ImportXml(XDocument bankXml)
		{
			Iso20022Document iso20022Document = new Iso20022Document(bankXml);

			Console.Out.WriteLine($"{iso20022Document.IBAN} : {iso20022Document.TtlNetNtryAmt}");

			List<Indbetaling> existingIndbetalings = Indbetaling.GetIndbetalingOnIban(_dynamicsCrmConnection, iso20022Document.IBAN).ToList();

			Konto konto = ReadKonto(iso20022Document.IBAN);
			Guid? owner = konto.owner;
			Guid? campaignId = konto.campaignid;
			if (campaignId == null)
			{
				throw new Exception($"could not find campaign on konto {konto.Id} with iban {konto.iban}");
			}

			foreach (Iso20022Ntry ntry in iso20022Document.Ntries)
			{
				//Console.Out.WriteLine($"{ntry.BankId}: {ntry.ValDt} : {ntry.Amt} {ntry.Ccy} - {ntry.Prtry} - {GetKilde(ntry, 0)} - ({ntry.BkTxCdDomnCd}_{ntry.BkTxCdDomnFmlyCd}_{ntry.BkTxCdDomnFmlySubFmlyCd})");

				Dictionary<string, int> crmIndbetalingCountByBankId = CreateIndbetalingCountByBankId(existingIndbetalings);
				Dictionary<string, int> fileIndbetalingCountByBankId = new Dictionary<string, int>();

				bool createNewIndbetaling = CalculateCreateNewIndsamling(crmIndbetalingCountByBankId, fileIndbetalingCountByBankId, ntry.BankId);

				if (createNewIndbetaling == false)
				{
					continue;
				}

				long? new_kkadminmedlemsnr = GetKkadminNr(ntry.BkTxCdPrtryCd);

				Account indsamlingssted = GetIndsamlingssted(new_kkadminmedlemsnr);

				Indbetaling.kildeEnum kilde = GetKilde(ntry, new_kkadminmedlemsnr);
				Guid? byarbejdeid = indsamlingssted?.byarbejdeid;
				Guid? indsamlingsstedid = indsamlingssted?.Id;
				Guid? indsamlingskoordinatorid = indsamlingssted?.indsamlingskoordinatorid;

				CreateIndbetaling(iso20022Document, ntry, konto.Id, campaignId.Value, kilde, byarbejdeid, indsamlingsstedid, indsamlingskoordinatorid, owner.Value);
			}
		}

		private bool CalculateCreateNewIndsamling(Dictionary<string, int> crmIndbetalingCountByBankId, Dictionary<string, int> fileIndbetalingCountByBankId, string bankId)
		{
			if (fileIndbetalingCountByBankId.ContainsKey(bankId))
			{
				fileIndbetalingCountByBankId[bankId]++;
			}
			else
			{
				fileIndbetalingCountByBankId.Add(bankId, 1);
			}

			int crmCount = 0;

			if (crmIndbetalingCountByBankId.ContainsKey(bankId))
			{
				crmCount = crmIndbetalingCountByBankId[bankId];
			}

			int fileCount = fileIndbetalingCountByBankId[bankId];

			if (crmCount < fileCount)
			{
				return true;
			}

			return false;
		}

		private Dictionary<string, int> CreateIndbetalingCountByBankId(List<Indbetaling> indbetalings)
		{
			Dictionary<string, int> countByBankId = new Dictionary<string, int>();

			foreach (Indbetaling indbetaling in indbetalings)
			{
				if (countByBankId.ContainsKey(indbetaling.new_bankid))
				{
					countByBankId[indbetaling.new_bankid]++;
				}
				else
				{
					countByBankId.Add(indbetaling.new_bankid, 1);
				}
			}

			return countByBankId;
		}

		private Account GetIndsamlingssted(long? new_kkadminmedlemsnr)
		{
			if (new_kkadminmedlemsnr.HasValue == false)
			{
				return null;
			}

			Account indsamlingssted = Account.ReadFromFetchXml
			(
				_dynamicsCrmConnection,
				new List<string>()
				{
					"accountid",
					"new_byarbejdeid",
					"new_indsamlingskoordinatorid"
				},
				new Dictionary<string, string>()
				{
					{ "new_kkadminmedlemsnr" , new_kkadminmedlemsnr.Value.ToString()}
				}
			).SingleOrDefault();

			return indsamlingssted;
		}

		private long? GetKkadminNr(string comment)
		{
			if (string.IsNullOrWhiteSpace(comment))
			{
				return null;
			}

			long parsedNumber;
			bool isParsed = long.TryParse(comment, out parsedNumber);

			if (isParsed)
			{
				return parsedNumber;
			}

			return null;
		}

		private Indbetaling.kildeEnum GetKilde(Iso20022Ntry ntry, long? new_kkadminmedlemsnr)
		{
			if (ntry.BkTxCdDomnCd == "PMNT" && ntry.BkTxCdDomnFmlyCd == "MCRD" && ntry.BkTxCdDomnFmlySubFmlyCd == "POSP")
			{
				return Indbetaling.kildeEnum.MobilePay;
			}

			if (ntry.BkTxCdDomnCd == "PMNT" && ntry.BkTxCdDomnFmlyCd == "RCDT" && ntry.BkTxCdDomnFmlySubFmlyCd == "VCOM")
			{
				return Indbetaling.kildeEnum.Giro;
			}

			if (ntry.BkTxCdDomnCd == "PMNT" && ntry.BkTxCdDomnFmlyCd == "RCDT" && ntry.BkTxCdDomnFmlySubFmlyCd == "DMCT")
			{
				return Indbetaling.kildeEnum.Bankoverfoersel;
			}

			if (ntry.BkTxCdDomnCd == "PMNT" && ntry.BkTxCdDomnFmlyCd == "CNTR" && ntry.BkTxCdDomnFmlySubFmlyCd == "CDPT")
			{
				return Indbetaling.kildeEnum.Kontant;
			}

			if (new_kkadminmedlemsnr.HasValue)
			{
				return Indbetaling.kildeEnum.Kontant;
			}

			return Indbetaling.kildeEnum.Ukendt;
		}

		private Konto ReadKonto(string iban)
		{
			Konto konto = Konto.ReadFromIban(_dynamicsCrmConnection, iban);

			if (konto == null)
			{
				throw new Exception($"could not read konto with iban {iban}");
			}

			return konto;
		}

		private void CreateIndbetaling(Iso20022Document iso20022Document, Iso20022Ntry ntry, Guid kontoId, Guid campaignId, Indbetaling.kildeEnum kilde, Guid? byarbejdeid, Guid? indsamlingsstedid, Guid? indsamlingskoordinatorid, Guid owner)
		{
			string bankkildekode = $"{ntry.BkTxCdDomnCd} / {ntry.BkTxCdDomnFmlyCd} / {ntry.BkTxCdDomnFmlySubFmlyCd}";

			Indbetaling indbetaling = Indbetaling.CreateAndInsert(_dynamicsCrmConnection, iso20022Document.IBAN, ntry.Amt, ntry.BankId, ntry.Prtry, ntry.ValDt, kontoId, campaignId, kilde, byarbejdeid, indsamlingsstedid, indsamlingskoordinatorid, ntry.BkTxCdPrtryCd, bankkildekode, owner);
		}

		private List<XDocument> GetBankXml(string[] files)
		{
			List<XDocument> bankXmlFiles = new List<XDocument>();

			foreach (string file in files)
			{
				try
				{
					XDocument bankFile = XDocument.Load(file);
					bankXmlFiles.Add(bankFile);
				}
				catch (Exception exception)
				{
					Log.WriteLocation(Connection, $"Could not import bankFile {file}", "ImportDanskeBank", exception.StackTrace, DataLayer.MongoData.Config.LogLevelEnum.OptionError);
				}
			}

			return bankXmlFiles;
		}
	}
}
