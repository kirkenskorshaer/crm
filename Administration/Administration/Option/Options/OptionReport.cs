namespace Administration.Option.Options
{
	public class OptionReport
	{
		public string Name;
		public int Workload;
		public int SubWorkload;
		public bool Success = false;

		public OptionReport(string name)
		{
			Name = name;
		}

		public override string ToString()
		{
			return $"{Name}=S:{Success} W:{Workload} SW:{SubWorkload}";
		}
	}
}
