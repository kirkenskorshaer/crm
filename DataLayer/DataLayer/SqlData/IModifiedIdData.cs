using System;

namespace DataLayer.SqlData
{
	public interface IModifiedIdData
	{
		Guid Id { get; }
		DateTime ModifiedOn { get; }
	}
}
