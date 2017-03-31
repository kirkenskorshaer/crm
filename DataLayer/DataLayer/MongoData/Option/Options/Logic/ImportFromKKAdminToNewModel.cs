using System;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class ImportFromKKAdminToNewModel : OptionBase
	{
		public string ReportFileName { get; set; }
		public string StamDataFileName { get; set; }
		public string TilknytningerFileName { get; set; }
		public string IndbetalingerFileName { get; set; }
		public bool Import { get; set; }
		public DateTime FirstValidIndbetaling { get; set; }
		public int? MaxNumberOfImports { get; set; }
	}
}
