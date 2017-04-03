namespace Administration.Option.Options.Logic.ImportFromKKAdminToNewModelData
{
	public class ImportReport
	{
		public string MedlemsNr { get; set; }
		public ImportEnum Import { get; set; }

		public enum ImportEnum
		{
			ContactIgnored = 1,
			ContactImported = 2,
			Obsolete = 3,
			AccountIgnored = 4,
			AccountImported = 5,
		}
	}
}
