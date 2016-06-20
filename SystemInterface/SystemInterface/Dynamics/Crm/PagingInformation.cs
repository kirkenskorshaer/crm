namespace SystemInterface.Dynamics.Crm
{
	public class PagingInformation
	{
		public bool MoreRecords;
		public string PagingCookie;
		public int Page = 1;
		public bool FirstRun = true;

		public void Reset()
		{
			Page = 1;
			PagingCookie = string.Empty;
			FirstRun = true;
		}
	}
}
