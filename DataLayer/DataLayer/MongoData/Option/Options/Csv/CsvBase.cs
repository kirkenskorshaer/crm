namespace DataLayer.MongoData.Option.Options.Csv
{
	public abstract class CsvBase : OptionBase
	{
		public string FileName { get; set; }
		public string FileNameTmp { get; set; }
		public char Delimeter { get; set; }
	}
}
