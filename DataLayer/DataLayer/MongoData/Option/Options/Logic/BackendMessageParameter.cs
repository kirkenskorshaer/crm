namespace DataLayer.MongoData.Option.Options.Logic
{
	public class BackendMessageParameter
	{
		public string Key;
		public object Value;

		public BackendMessageParameter(string key, object value)
		{
			Key = key;
			Value = value;
		}
	}
}
