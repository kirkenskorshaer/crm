namespace DataLayer.MongoData.Option.Status
{
	public class OptionStatusLine
	{
		public int FailTotal { get; set; }
		public int Fail24Hour { get; set; }
		public int Fail1Hour { get; set; }
		public int Fail10Minute { get; set; }

		public int SuccessTotal { get; set; }
		public int Success24Hour { get; set; }
		public int Success1Hour { get; set; }
		public int Success10Minute { get; set; }

		public int ExecutionTotal { get; set; }
	}
}
