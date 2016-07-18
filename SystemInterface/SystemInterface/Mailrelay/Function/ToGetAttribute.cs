using System;

namespace SystemInterface.Mailrelay.Function
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
	public class ToGetAttribute : Attribute
	{
		public string name { get; set; }
		public typeEnum type { get; set; }

		public enum typeEnum
		{
			defaultType = 1,
			intEnum = 2,
			ignore = 3,
			ShortDate = 4,
		}
	}
}
