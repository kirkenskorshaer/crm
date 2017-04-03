using Administration.Option.Options.Logic.ImportFromKKAdminToNewModelData;
using CsvHelper;
using DataLayer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SystemInterface.Dynamics.Crm;
using DatabaseImportFromKKAdminToNewModel = DataLayer.MongoData.Option.Options.Logic.ImportFromKKAdminToNewModel;
using System;

namespace Administration.Option.Options.Logic
{
	public class ImportFromKKAdminToNewModel : OptionBase
	{
		private DatabaseImportFromKKAdminToNewModel _databaseImportFromKKAdminToNewModel;

		public ImportFromKKAdminToNewModel(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseImportFromKKAdminToNewModel = (DatabaseImportFromKKAdminToNewModel)databaseOption;
		}

		public override void ExecuteOption(OptionReport report)
		{
			SetDynamicsCrmConnectionIfEmpty();

			string reportFileName = _databaseImportFromKKAdminToNewModel.ReportFileName;
			string stamDataFileName = _databaseImportFromKKAdminToNewModel.StamDataFileName;
			string tilknytningerFileName = _databaseImportFromKKAdminToNewModel.TilknytningerFileName;
			string indbetalingerFileName = _databaseImportFromKKAdminToNewModel.IndbetalingerFileName;
			int? maxNumberOfImports = _databaseImportFromKKAdminToNewModel.MaxNumberOfImports;

			using (FileStream fileStreamWrite = File.Open(reportFileName, FileMode.Create, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStreamWrite, Encoding.UTF8))
				{
					using (CsvWriter reportFile = new CsvWriter(streamWriter))
					{
						reportFile.Configuration.Delimiter = ";";
						reportFile.WriteHeader<ImportReport>();

						IEnumerable<KKAdminStamData> stamDatas = GetData<KKAdminStamData>(stamDataFileName);
						List<KKAdminTilknytning> tilknytninger = GetData<KKAdminTilknytning>(tilknytningerFileName).ToList();
						List<KKAdminIndbetaling> indbetalinger = GetData<KKAdminIndbetaling>(indbetalingerFileName).ToList();

						int readCount = 0;
						foreach (KKAdminStamData stamData in stamDatas)
						{
							stamData.SetRelations(tilknytninger, indbetalinger);

							ImportReport.ImportEnum importReport = ImportReport.ImportEnum.ContactIgnored;

							if (stamData.DataType == KKAdminStamData.DataTypeEnum.Contact)
							{
								importReport = ImportContact(stamData);
							}
							else if (stamData.DataType == KKAdminStamData.DataTypeEnum.Account)
							{
								importReport = ImportAccount(stamData);
							}

							reportFile.WriteRecord(new ImportReport() { MedlemsNr = stamData.MedlemsNr, Import = importReport });

							readCount++;
							if (maxNumberOfImports.HasValue && readCount >= maxNumberOfImports)
							{
								return;
							}
						}
					}
				}
			}
		}

		private ImportReport.ImportEnum ImportContact(KKAdminStamData stamData)
		{
			DateTime firstValidIndbetaling = _databaseImportFromKKAdminToNewModel.FirstValidIndbetaling;

			if (stamData.IsValidIndbetaling(firstValidIndbetaling) == false)
			{
				return ImportReport.ImportEnum.Obsolete;
			}

			bool import = _databaseImportFromKKAdminToNewModel.Import;

			Dictionary<string, string> crmContact = stamData.ToCrmContact();

			Contact contact = Contact.Create(_dynamicsCrmConnection, crmContact);
			Contact existingContact = Contact.ReadFromFetchXml(_dynamicsCrmConnection, new List<string>() { "contactid" }, new Dictionary<string, string>() { { "new_kkadminmedlemsnr", contact.new_kkadminmedlemsnr.ToString() } }).FirstOrDefault();

			if (existingContact == null)
			{
				if (import)
				{
					contact.InsertWithoutRead();

					MaybeAddAnnotation(contact, stamData);

					contact.SynchronizeGroups(stamData.Tilknytninger.Select(tilknytning => tilknytning.TilknytningsNavn).ToList());

					AddIndbetalings(stamData, contact);
				}
				return ImportReport.ImportEnum.ContactImported;
			}
			else
			{
				return ImportReport.ImportEnum.ContactIgnored;
			}
		}

		private void MaybeAddAnnotation(Contact contact, KKAdminStamData stamData)
		{
			if (string.IsNullOrWhiteSpace(stamData.Notat))
			{
				return;
			}

			Annotation annotation = new Annotation(_dynamicsCrmConnection);
			annotation.notetext = stamData.Notat;
			annotation.ObjectidSet(contact.Id, "contact");

			annotation.Insert();
		}

		private ImportReport.ImportEnum ImportAccount(KKAdminStamData stamData)
		{
			bool import = _databaseImportFromKKAdminToNewModel.Import;

			Dictionary<string, string> crmAccount = stamData.ToCrmAccount();

			Account account = Account.Create(_dynamicsCrmConnection, crmAccount);
			Account existingAccount = Account.ReadFromFetchXml(_dynamicsCrmConnection, new List<string>() { "accountid" }, new Dictionary<string, string>() { { "new_kkadminmedlemsnr", account.new_kkadminmedlemsnr.ToString() } }).FirstOrDefault();

			if (existingAccount == null)
			{
				if (import)
				{
					account.InsertWithoutRead();

					account.SynchronizeGroups(stamData.Tilknytninger.Select(tilknytning => tilknytning.TilknytningsNavn).ToList());

					AddIndbetalings(stamData, account);
				}
				return ImportReport.ImportEnum.AccountImported;
			}
			else
			{
				return ImportReport.ImportEnum.AccountIgnored;
			}
		}

		private void AddIndbetalings(KKAdminStamData stamData, Contact contact)
		{
			foreach (KKAdminIndbetaling indbetaling in stamData.Indbetalinger)
			{
				Indbetaling.CreateAndInsert(_dynamicsCrmConnection, "auto imported 2017", indbetaling.BeløbDecimal, indbetaling.BankId, indbetaling.BogføringsDatoDateTime, indbetaling.FinansKonto, indbetaling.IndbetalingDatoDateTime, indbetaling.IndbetalingStatus, indbetaling.IndbetalingsTypeNavn, indbetaling.IndbetalingTekst, indbetaling.KontoType, Indbetaling.kildeEnum.Ukendt, contact.Id);
			}
		}

		private void AddIndbetalings(KKAdminStamData stamData, Account account)
		{
			foreach (KKAdminIndbetaling indbetaling in stamData.Indbetalinger)
			{
				Indbetaling.CreateAndInsert(_dynamicsCrmConnection, "auto imported 2017", indbetaling.BeløbDecimal, indbetaling.BankId, indbetaling.BogføringsDatoDateTime, indbetaling.FinansKonto, indbetaling.IndbetalingDatoDateTime, indbetaling.IndbetalingStatus, indbetaling.IndbetalingsTypeNavn, indbetaling.IndbetalingTekst, indbetaling.KontoType, Indbetaling.kildeEnum.Ukendt, account.Id);
			}
		}

		private IEnumerable<DataType> GetData<DataType>(string fileName)
		{
			TextReader reader = GetReader(fileName);
			CsvReader dataCsv = new CsvReader(reader);
			dataCsv.Configuration.Delimiter = ";";
			IEnumerable<DataType> datas = dataCsv.GetRecords<DataType>();
			return datas;
		}

		private StreamReader GetReader(string fileName)
		{
			FileStream fileStreamRead = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
			StreamReader streamReader = new StreamReader(fileStreamRead, Encoding.UTF8);
			return streamReader;
		}

		private StreamWriter GetWriter(string fileName)
		{
			FileStream fileStreamWrite = File.Open(fileName, FileMode.Create, FileAccess.Write);
			StreamWriter streamWriter = new StreamWriter(fileStreamWrite, Encoding.UTF8);
			return streamWriter;
		}
	}
}