namespace DataLayer.SqlData
{
	internal class SqlCondition
	{
		internal string searchName;
		internal string operatorString;
		internal object searchValue;

		public SqlCondition(string searchName, string operatorString, object searchValue)
		{
			this.searchName = searchName;
			this.operatorString = operatorString;
			this.searchValue = searchValue;
		}
	}
}
