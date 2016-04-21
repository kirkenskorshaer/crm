namespace DataLayer.SqlData
{
	public interface IDeletableModifiedIdData : IModifiedIdData
	{
		bool isdeleted { get; }
	}
}
