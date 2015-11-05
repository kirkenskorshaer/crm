using System;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class SynchronizeFromCsv : OptionBase
	{
		public string fileName { get; set; }
		public string fileNameTmp { get; set; }
		public char delimeter { get; set; }
		public string keyName { get; set; }
		public string dateName { get; set; }
		public string mappingField { get; set; }
		public string[] fields { get; set; }

		public Guid changeProviderId { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<SynchronizeFromCsv>(connection);
			}
			else
			{
				Delete<SynchronizeFromCsv>(connection);
			}
		}
	}
}
